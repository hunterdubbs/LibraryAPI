using LibraryAPI.DAL;
using LibraryAPI.Domain;
using LibraryAPI.Domain.Requests;
using LibraryAPI.Domain.Responses;
using LibraryAPI.LogicProcessors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        protected PermissionLogicProcessor permissionLogicProcessor;
        protected IHttpClientFactory httpClientFactory;

        public BookController(ILibraryDataContext libraryDataContext, BookLogicProcessor bookLogicProcessor, PermissionLogicProcessor permissionLogicProcessor, IHttpClientFactory httpClientFactory)
        {
            this.libraryDataContext = libraryDataContext;
            this.bookLogicProcessor = bookLogicProcessor;
            this.permissionLogicProcessor = permissionLogicProcessor;
            this.httpClientFactory = httpClientFactory;
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

        [HttpGet]
        [Route("collection/{collectionID}")]
        public IActionResult GetBooksByCollectionID([FromRoute][Required] int collectionID)
        {
            Result<List<Book>> result = null;
            bool permissionDenied = false;
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using(UnitOfWork uow = new UnitOfWork())
            {
                result = bookLogicProcessor.GetBooksByCollectionID(collectionID, userID, out permissionDenied);
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
        public IActionResult CreateBook([FromBody] CreateBookRequest request)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            Book book = new Book()
            {
                Title = request.Title,
                Synopsis = request.Synopsis,
                Authors = request.Authors,
                Tags = request.Tags,
                DateAdded = DateTime.Now,
                DatePublished = request.DatePublished,
                Series = request.Series,
                Volume = request.Volume,
                LibraryID = request.LibraryID
            };

            using(UnitOfWork uow = new UnitOfWork())
            {
                uow.Begin();
                Result result = bookLogicProcessor.CreateBook(book, request.CollectionID, userID, out bool permissionDenied);

                if (result.Succeeded)
                {
                    uow.Commit();
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
        public IActionResult ModifyBook([FromBody] ModifyBookRequest request)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);
            using (UnitOfWork uow = new UnitOfWork())
            {
                uow.Begin();
                Book book = libraryDataContext.BookRepository.GetByID(request.BookID);
                if (book == null) return BadRequest("Book not found");

                book.Title = request.Title;
                book.Synopsis = request.Synopsis;
                book.DatePublished = request.DatePublished;
                book.Authors = request.Authors;
                book.Tags = request.Tags;
                book.LibraryID = request.LibraryID;
                book.Series = request.Series;
                book.Volume = request.Volume;

                Result result = bookLogicProcessor.ModifyBook(book, userID, out bool permissionDenied);

                if (result.Succeeded)
                {
                    uow.Commit();
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
        public IActionResult DeleteBook([FromBody] DeleteBookRequest request)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);

            using (UnitOfWork uow = new UnitOfWork())
            {
                uow.Begin();

                Result result = bookLogicProcessor.DeleteBook(request.BookID, userID, out bool permissionDenied);

                if (result.Succeeded)
                {
                    uow.Commit();
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
        [Route("tags/{libraryID}")]
        public IActionResult GetTags([FromRoute][Required] int libraryID)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);
            List<Tag> results;

            using (UnitOfWork uow = new UnitOfWork())
            {
                if (!permissionLogicProcessor.CheckPermissionOnLibraryID(libraryID, userID, Domain.Enum.PermissionType.Viewer)) return Forbid();
                results = libraryDataContext.TagRepository.GetByLibraryID(libraryID);
            }

            return Ok(results);
        }

        [HttpPost]
        [Route("tags/create")]
        public IActionResult CreateTag([FromBody][Required] CreateTagRequest request)
        {
            string userID = ClaimsHelper.GetUserIDFromClaim(User);
            Tag tag = new Tag()
            {
                LibraryID = request.LibraryID,
                Name = request.Name
            };

            using(UnitOfWork uow = new UnitOfWork())
            {
                uow.Begin();

                Result result = bookLogicProcessor.CreateTag(tag, userID, out bool permissionDenied);

                if (result.Succeeded)
                {
                    uow.Commit();
                    return Ok(tag);
                }
                else
                {
                    if (permissionDenied) return Forbid();
                    return StatusCode(500);
                }
            }
        }

        [HttpGet]
        [Route("lookup/isbn/{isbn}")]
        public async Task<IActionResult> LookupISBN([FromRoute] string isbn)
        {
            string normalizedISBN = isbn.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(normalizedISBN) || normalizedISBN.Length < 10 || normalizedISBN.Length > 13) return BadRequest("invalid isbn");
            string isbnPattern = @"^(?=[A-Z0-9]*$)(?:.{10}|.{13})$";
            bool patternMatch = false;
            try
            {
                patternMatch = Regex.IsMatch(normalizedISBN, isbnPattern, RegexOptions.None, new TimeSpan(0, 0, 1));
            }
            catch (Exception)
            {
                return BadRequest("invalid isbn");
            }
            if (!patternMatch) return BadRequest("invalid isbn");

            var lookupRequest = new HttpRequestMessage(HttpMethod.Get, string.Format("https://openlibrary.org/api/books?bibkeys=ISBN:{0}&jscmd=details&format=json", normalizedISBN));
            var client = httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.SendAsync(lookupRequest);
            if (!response.IsSuccessStatusCode) return BadRequest("isbn not found");

            BookLookupResponse result = new BookLookupResponse() { Authors = new List<Author>() };
            var responseContents = await response.Content.ReadAsStringAsync();

            try
            {
                using (JsonDocument json = JsonDocument.Parse(responseContents))
                {
                    string rootProperty = "ISBN:" + normalizedISBN;
                    var detailsNode = json.RootElement.GetProperty(rootProperty).GetProperty("details");

                    result.Title = detailsNode.TryGetProperty("title", out var titleNode) ? TrimTo(titleNode.GetString(), 255) : string.Empty;
                    result.Description = detailsNode.TryGetProperty("description", out var descriptionNode) ? TrimTo(descriptionNode.GetString(), 1023) : string.Empty;

                    result.Published = detailsNode.TryGetProperty("publish_date", out var publishDateNode) ? DateTime.TryParse(publishDateNode.GetString(), out DateTime publishedDate) ? publishedDate : DateTime.Now : DateTime.Now;

                    if (detailsNode.TryGetProperty("authors", out var authorsNode))
                    {
                        var authors = authorsNode.EnumerateArray();
                        using (UnitOfWork uow = new UnitOfWork())
                        {
                            uow.Begin();

                            foreach (var authorListing in authors)
                            {
                                var fullName = authorListing.GetProperty("name").GetString();
                                if (string.IsNullOrWhiteSpace(fullName)) continue;
                                //best guess here, take the last full word as the last name and combine remaining words for the first name
                                var nameSplit = fullName.Split(' ');
                                if (nameSplit.Length < 1) continue;
                                string lastName = nameSplit[nameSplit.Length - 1];
                                string firstName = string.Join(' ', nameSplit.ToList().Except(new List<string> { lastName }));


                                string normalizedLastName = TrimTo(lastName, 40);
                                string normalizedFirstName = TrimTo(firstName, 40);

                                //is it already in the db?
                                Author author = libraryDataContext.AuthorRepository.GetByFirstLastName(normalizedFirstName, normalizedLastName);
                                if (author == null)
                                {
                                    author = new Author()
                                    {
                                        FirstName = normalizedFirstName,
                                        LastName = normalizedLastName
                                    };
                                    libraryDataContext.AuthorRepository.Add(author);
                                }
                                result.Authors.Add(author);
                            }

                            uow.Commit();
                        }
                    }

                    return Ok(result);
                }
            }
            catch (Exception)
            {
                return BadRequest("isbn not found");
            }
        }

        private string TrimTo(string input, int maxLength)
        {
            return input.Length > maxLength ? input.Substring(0, maxLength) : input;
        }
    }
}
