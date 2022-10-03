﻿using LibraryAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.LogicProcessors
{
    public class BookLogicProcessor
    {
        protected ILibraryDataContext libraryDataContext;
        protected PermissionLogicProcessor permissionLogicProcessor;

        public BookLogicProcessor(ILibraryDataContext libraryDataContext, PermissionLogicProcessor permissionLogicProcessor)
        {
            this.libraryDataContext = libraryDataContext;
            this.permissionLogicProcessor = permissionLogicProcessor;
        }

        public Result<Book> GetBookByID(int bookID, string userID, out bool permissionDenied)
        {
            Result<Book> result = new Result<Book>();
            permissionDenied = false;

            if(!permissionLogicProcessor.CheckPermissionOnBookID(bookID, userID, Domain.Enum.PermissionType.Viewer))
            {
                permissionDenied = true;
                return result.Abort("you do not have permission to access this library");
            }

            Book book =  libraryDataContext.BookRepository.GetByID(bookID);
            if (book == null) return result;

            result.Value = book;
            return result;            
        }

        public Result<List<Book>> GetBooksByCollectionID(int collectionID, string userID, out bool permissionDenied)
        {
            Result<List<Book>> result = new Result<List<Book>>();
            permissionDenied = false;

            if(!permissionLogicProcessor.CheckPermissionOnCollectionID(collectionID, userID, Domain.Enum.PermissionType.Viewer))
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to access this library");
            }

            List<Book> books = libraryDataContext.BookRepository.GetByCollectionID(collectionID);
            if (books == null) return result;

            result.Value = books;
            return result;
        }

        public Result CreateBook(Book book, int? collectionID, string userID, out bool permissionDenied)
        {
            Result result = new Result();
            permissionDenied = false;

            if (!permissionLogicProcessor.CheckPermissionOnLibraryID(book.LibraryID, userID, Domain.Enum.PermissionType.Editor)
                || !(collectionID != null && permissionLogicProcessor.CheckPermissionOnCollectionID(collectionID.Value, userID, Domain.Enum.PermissionType.Editor))
                
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to add to this library");
            }

            libraryDataContext.BookRepository.Add(book);
            if (collectionID != null) libraryDataContext.CollectionRepository.AddBookToCollection(book.ID, collectionID.Value);

            return result;
        }

        public Result ModifyBook(Book book, string userID, out bool permissionDenied)
        {
            Result result = new Result();
            permissionDenied = false;

            Book oldBook = libraryDataContext.BookRepository.GetByID(book.ID);

            if(!permissionLogicProcessor.CheckPermissionOnBook(book, userID, Domain.Enum.PermissionType.Editor)
                || !permissionLogicProcessor.CheckPermissionOnBook(oldBook, userID, Domain.Enum.PermissionType.Editor))
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to modify this book");
            }

            libraryDataContext.BookRepository.Update(book);
            return result;
        }
    }
}
