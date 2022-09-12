using LibraryAPI.DAL;
using LibraryAPI.Domain;
using LibraryAPI.Domain.Requests;
using LibraryAPI.LogicProcessors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LibraryController : ControllerBase
    {
        protected ILibraryDataContext libraryDataContext;
        protected LibraryLogicProcessor libraryLogicProcessor;

        public LibraryController(ILibraryDataContext libraryDataContext, LibraryLogicProcessor libraryLogicProcessor)
        {
            this.libraryDataContext = libraryDataContext;
            this.libraryLogicProcessor = libraryLogicProcessor;
        }

        [HttpPost]
        [Route("create")]
        public IActionResult CreateLibrary([FromBody] CreateLibraryRequest libraryRequest)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                unitOfWork.Begin();

                Library library = new Library() {
                    Name = libraryRequest.Name,
                    Owner = ClaimsHelper.GetUserNameFromClaim(User)
                };
                Result result = libraryLogicProcessor.CreateLibrary(library, userID);

                if (result.Succeeded)
                {
                    unitOfWork.Commit();
                    return Ok();
                }
                else
                {
                    return StatusCode(500);
                }
            }
        }

        [HttpPost]
        [Route("delete")]
        public IActionResult DeleteLibrary([FromBody] DeleteLibraryRequest request)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using(UnitOfWork unitOfWork = new UnitOfWork())
            {
                unitOfWork.Begin();

                Result result = libraryLogicProcessor.DeleteLibrary(request.LibraryID, userID, out bool permissionDenied);

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
        public IActionResult ModifyLibrary([FromBody] ModifyLibraryRequest request)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using(UnitOfWork unitOfWork = new UnitOfWork())
            {
                unitOfWork.Begin();

                Library library = libraryDataContext.LibraryRepository.GetByID(request.LibraryID);
                if (library == null) return BadRequest("Library not found");

                library.Name = request.Name;
                Result result = libraryLogicProcessor.UpdateLibrary(library, userID, out bool permissionDenied);

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

        [HttpGet]
        [Route("all")]
        public IActionResult GetLibraries()
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);
            List<Library> results = null;

            using(UnitOfWork unitOfWork = new UnitOfWork())
            {
                results = libraryDataContext.LibraryRepository.GetAllByUser(userID);
            }

            return Ok(results);
        }
    }
}
