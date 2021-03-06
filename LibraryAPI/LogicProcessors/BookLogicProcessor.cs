using LibraryAPI.Domain;
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
    }
}
