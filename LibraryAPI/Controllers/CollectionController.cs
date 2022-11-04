using LibraryAPI.DAL;
using LibraryAPI.Domain;
using LibraryAPI.Domain.Requests;
using LibraryAPI.LogicProcessors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CollectionController : ControllerBase
    {
        protected ILibraryDataContext libraryDataContext;
        protected CollectionLogicProcessor collectionLogicProcessor;

        public CollectionController(ILibraryDataContext libraryDataContext, CollectionLogicProcessor collectionLogicProcessor)
        {
            this.libraryDataContext = libraryDataContext;
            this.collectionLogicProcessor = collectionLogicProcessor;
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetCollectionByID([FromRoute][Required] int id)
        {
            Result<Collection> result;
            bool permissionDenied = false;
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                result = collectionLogicProcessor.GetCollectionWithBooks(id, userID, out permissionDenied);
            }

            if (!result.Succeeded)
            {
                if (permissionDenied) return Forbid();
                return StatusCode(500, result.Error);
            }

            return Ok(result.Value);
        }

        [HttpGet]
        [Route("list/{libraryID}")]
        public IActionResult GetCollections([FromRoute][Required] int libraryID)
        {
            Result<List<Collection>> result;
            bool permissionDenied = false;
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                result = collectionLogicProcessor.GetCollections(libraryID, userID, out permissionDenied);
            }

            if (!result.Succeeded)
            {
                if (permissionDenied) return Forbid();
                return StatusCode(500, result.Error);
            }

            return Ok(result.Value);
        }

        [HttpPost]
        [Route("create")]
        public IActionResult CreateCollection([FromBody] CreateCollectionRequest request)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                unitOfWork.Begin();

                Collection collection = new Collection()
                {
                    LibraryID = request.LibraryID,
                    ParentCollectionID = request.ParentCollectionID,
                    Name = request.Name,
                    Description = request.Description,
                    IsUserModifiable = true
                };

                Result result = collectionLogicProcessor.CreateCollection(collection, userID, out bool permissionDenied);

                if (result.Succeeded)
                {
                    unitOfWork.Commit();
                    return Ok();
                }
                else
                {
                    if (permissionDenied) return Forbid();
                    return StatusCode(500);
                }
            }
        }

        [HttpPost]
        [Route("modify")]
        public IActionResult ModifyCollection([FromBody] ModifyCollectionRequest request)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                unitOfWork.Begin();

                Collection collection = libraryDataContext.CollectionRepository.GetByID(request.CollectionID);
                if (collection == null) return BadRequest("Collection not found");

                if (!string.IsNullOrEmpty(request.Name)) collection.Name = request.Name;
                if (!string.IsNullOrEmpty(request.Description)) collection.Description = request.Description;

                Result result = collectionLogicProcessor.ModifyCollection(collection, userID, out bool permissionDenied);

                if (result.Succeeded)
                {
                    unitOfWork.Commit();
                    return Ok();
                }
                else
                {
                    if (permissionDenied) return Forbid();
                    return StatusCode(500);
                }
            }
        }

        [HttpPost]
        [Route("delete")]
        public IActionResult DeleteCollection([FromBody] DeleteCollectionRequest request)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                unitOfWork.Begin();

                Result result = collectionLogicProcessor.DeleteCollection(request.CollectionID, userID, out bool permissionDenied);

                if (result.Succeeded)
                {
                    unitOfWork.Commit();
                    return Ok();
                }
                else
                {
                    if (permissionDenied) return Forbid();
                    return BadRequest(result.Error);
                }
            }
        }

        [HttpGet]
        [Route("book/list/{bookID}")]
        public IActionResult GetCollectionsMembershipsByBook([FromRoute][Required] int bookID)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);
            bool permissionDenied = false;
            Result<List<CollectionMembership>> result;

            using(UnitOfWork unitOfWork = new UnitOfWork())
            {
                result = collectionLogicProcessor.GetCollectionMembershipsByBook(bookID, userID, out permissionDenied);
            }

            if (result.Succeeded)
            {
                return Ok(result.Value);
            }
            else
            {
                if (permissionDenied) return Forbid();
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("book")]
        public IActionResult UpdateBookCollections([FromBody] UpdateBookCollectionsRequest request)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using(UnitOfWork unitOfWork = new UnitOfWork())
            {
                unitOfWork.Begin();

                Result result = collectionLogicProcessor.UpdateBookCollectionMemberships(request.BookID, request.CollectionIDs, userID, out bool permissionDenied);

                if (result.Succeeded)
                {
                    unitOfWork.Commit();
                    return Ok();
                }
                else
                {
                    if (permissionDenied) return Forbid();
                    return StatusCode(500);
                }
            }
        }
    }
}
