using LibraryAPI.Domain;
using LibraryAPI.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.LogicProcessors
{
    public class PermissionLogicProcessor
    {
        protected ILibraryDataContext libraryDataContext;

        public PermissionLogicProcessor(ILibraryDataContext libraryDataContext)
        {
            this.libraryDataContext = libraryDataContext;
        }

        public bool CheckPermissionOnBookID(int bookID, string userID, PermissionType minPermissionLevel)
        {
            Book book = libraryDataContext.BookRepository.GetByID(bookID);
            if (book == null) return false;

            return CheckPermissionOnBook(book, userID, minPermissionLevel);
        }

        public bool CheckPermissionOnBook(Book book, string userID, PermissionType minPermissionLevel)
        {
            return CheckPermissionOnLibraryID(book.LibraryID, userID, minPermissionLevel);
        }

        public bool CheckPermissionOnCollectionID(int collectionID, string userID, PermissionType minPermissionLevel)
        {
            Collection collection = libraryDataContext.CollectionRepository.GetByID(collectionID);
            if (collection == null) return false;

            return CheckPermissionOnCollection(collection, userID, minPermissionLevel);
        }

        public bool CheckPermissionOnCollection(Collection collection, string userID, PermissionType minPermissionLevel)
        {
            return CheckPermissionOnLibraryID(collection.LibraryID, userID, minPermissionLevel);
        }

        public bool CheckPermissionOnLibrary(Library library, string userID, PermissionType minPermissionLevel)
        {
            return CheckPermissionOnLibraryID(library.ID, userID, minPermissionLevel);
        }

        public bool CheckPermissionOnLibraryID(int libraryID, string userID, PermissionType minPermissionLevel)
        {
            PermissionType permissionLevel = libraryDataContext.PermissionRepository.GetByUserByLibrary(userID, libraryID);
            return (int)minPermissionLevel <= (int)permissionLevel;
        }
    }
}
