using LibraryAPI.DAL;
using LibraryAPI.Domain;
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
        protected CollectionLogicProcessor collectionLogicProcessor;

        public CollectionController(CollectionLogicProcessor collectionLogicProcessor)
        {
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
                if (permissionDenied) return Unauthorized();
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

            using(UnitOfWork unitOfWork = new UnitOfWork())
            {
                result = collectionLogicProcessor.GetCollections(libraryID, userID, out permissionDenied);
            }

            if (!result.Succeeded)
            {
                if (permissionDenied) return Unauthorized();
                return StatusCode(500, result.Error);
            }

            return Ok(result.Value);
        }
    }
}
