using LibraryAPI.DAL;
using LibraryAPI.Domain;
using LibraryAPI.Domain.Requests;
using LibraryAPI.LogicProcessors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AuthorController : ControllerBase
    {
        protected ILibraryDataContext libraryDataContext;
        protected AuthorLogicProcessor authorLogicProcessor;

        public AuthorController(ILibraryDataContext libraryDataContext, AuthorLogicProcessor authorLogicProcessor)
        {
            this.libraryDataContext = libraryDataContext;
            this.authorLogicProcessor = authorLogicProcessor;
        }

        [HttpGet]
        [Route("search")]
        public IActionResult SearchAuthors([FromQuery] string searchTerm)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);
            Result<List<Author>> result;

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                result = authorLogicProcessor.Search(searchTerm);
            }

            return Ok(result.Value);
        }

        [HttpPost]
        [Route("create")]
        public IActionResult CreateAuthor([FromBody] CreateAuthorRequest request)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);
            Result<Author> result;

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                unitOfWork.Begin();

                Author author = new Author()
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName
                };
                result = authorLogicProcessor.CreateOrGetAuthor(author, userID);

                if (result.Succeeded)
                {
                    unitOfWork.Commit();
                    return Ok(result.Value);
                }
                else
                {
                    return StatusCode(500);
                }
            }
        }
    }
}
