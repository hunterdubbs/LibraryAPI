﻿using LibraryAPI.DAL;
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
                DateAdded = DateTime.Now,
                DatePublished = request.DatePublished,
                LibraryID = request.LibraryID
            };

            using(UnitOfWork uow = new UnitOfWork())
            {
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
                Book book = libraryDataContext.BookRepository.GetByID(request.BookID);
                if (book == null) return BadRequest("Book not found");

                book.Title = request.Title;
                book.Synopsis = request.Synopsis;
                book.DatePublished = request.DatePublished;
                book.Authors = request.Authors;
                book.LibraryID = request.LibraryID;

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
    }
}
