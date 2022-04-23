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
using System.Security.Claims;
using System.Threading.Tasks;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookController : ControllerBase
    {
        protected ILibraryDataContext libraryDataContext;
        protected BookLogicProcessor bookLogicProcessor;

        public BookController(ILibraryDataContext libraryDataContext, BookLogicProcessor bookLogicProcessor)
        {
            this.libraryDataContext = libraryDataContext;
            this.bookLogicProcessor = bookLogicProcessor;
        }


        [HttpGet]
        [Route("{bookID}")]
        public IActionResult GetBookByID([FromRoute][Required] int bookID)
        {
            Result<Book> result = null;
            bool permissionDenied = false;
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using(UnitOfWork uow = new UnitOfWork())
            {
                result = bookLogicProcessor.GetBookByID(bookID, userID, out permissionDenied);
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
