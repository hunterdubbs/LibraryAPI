using LibraryAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.LogicProcessors
{
    public class CollectionLogicProcessor
    {
        protected ILibraryDataContext libraryDataContext;
        protected PermissionLogicProcessor permissionLogicProcessor;
        protected BookLogicProcessor bookLogicProcessor;

        public CollectionLogicProcessor(
            ILibraryDataContext libraryDataContext, 
            PermissionLogicProcessor permissionLogicProcessor, 
            BookLogicProcessor bookLogicProcessor)
        {
            this.libraryDataContext = libraryDataContext;
            this.permissionLogicProcessor = permissionLogicProcessor;
            this.bookLogicProcessor = bookLogicProcessor;
        }

        public Result<Collection> GetCollectionWithBooks(int collectionID, string userID, out bool permissionDenied)
        {
            Result<Collection> result = new Result<Collection>();
            permissionDenied = false;

            Collection collection = libraryDataContext.CollectionRepository.GetByID(collectionID);
            if (collection == null) return result.Abort("Collection not found");

            if(!permissionLogicProcessor.CheckPermissionOnLibraryID(collection.LibraryID, userID, Domain.Enum.PermissionType.Viewer))
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to access this collection");
            }

            collection.Books = libraryDataContext.BookRepository.GetByCollectionID(collection.ID);
            result.Value = collection;
            return result;
        }

        public Result<List<CollectionMembership>> GetCollectionMembershipsByBook(int bookID, string userID, out bool permissionDenied)
        {
            Result<List<CollectionMembership>> result = new Result<List<CollectionMembership>>();
            permissionDenied = false;

            if(!permissionLogicProcessor.CheckPermissionOnBookID(bookID, userID, Domain.Enum.PermissionType.Viewer))
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to view this book");
            }

            Book book = libraryDataContext.BookRepository.GetByID(bookID);
            if (book == null) return result.Abort("Book not found");

            result.Value = libraryDataContext.CollectionRepository.GetAllWithMembershipStatusByBookID(bookID, book.LibraryID);
            return result;
        }

        public Result UpdateBookCollectionMemberships(int bookID, IEnumerable<int> collections, string userID, out bool permissionDenied)
        {
            Result result = new Result();
            permissionDenied = false;

            if (!permissionLogicProcessor.CheckPermissionOnBookIDAndCollectionIDs(bookID, collections, userID, Domain.Enum.PermissionType.Editor))
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to modify this book");
            }

            Library library = libraryDataContext.LibraryRepository.GetByBookID(bookID);

            var currentCollections = libraryDataContext.CollectionRepository.GetAllByBookID(bookID).Select(c => c.ID);
            var collectionsToAdd = collections.Except(currentCollections);
            var collectionsToRemove = currentCollections.Except(collections);

            foreach(int collectionID in collectionsToAdd)
            {
                libraryDataContext.CollectionRepository.AddBookToCollection(bookID, collectionID);
            }

            foreach(int collectionID in collectionsToRemove)
            {
                if(collectionID != library.DefaultCollectionID) libraryDataContext.CollectionRepository.RemoveBookFromCollection(bookID, collectionID);
            }

            return result;
        }

        public Result<List<Collection>> GetCollections(int libraryID, string userID, out bool permissionDenied)
        {
            Result<List<Collection>> result = new Result<List<Collection>>();
            permissionDenied = false;

            if(!permissionLogicProcessor.CheckPermissionOnLibraryID(libraryID, userID, Domain.Enum.PermissionType.Viewer))
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to access this library");
            }

            result.Value = libraryDataContext.CollectionRepository.GetAllByLibraryID(libraryID);
            return result;
        }

        public Result CreateCollection(Collection collection, string userID, out bool permissionDenied)
        {
            Result result = new Result();
            permissionDenied = false;

            if(!permissionLogicProcessor.CheckPermissionOnLibraryID(collection.LibraryID, userID, Domain.Enum.PermissionType.Editor))
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to add to this library");
            }

            libraryDataContext.CollectionRepository.Add(collection);
            return result;
        }

        public Result ModifyCollection(Collection collection, string userID, out bool permissionDenied)
        {
            Result result = new Result();
            permissionDenied = false;

            if (!permissionLogicProcessor.CheckPermissionOnLibraryID(collection.LibraryID, userID, Domain.Enum.PermissionType.Editor))
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to add to this library");
            }

            libraryDataContext.CollectionRepository.Update(collection);
            return result;
        }

        public Result DeleteCollection(int collectionID, string userID, out bool permissionDenied)
        {
            Result result = new Result();
            permissionDenied = false;

            if (!permissionLogicProcessor.CheckPermissionOnCollectionID(collectionID, userID, Domain.Enum.PermissionType.Editor))
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to add to this library");
            }

            Collection collection = libraryDataContext.CollectionRepository.GetByID(collectionID);
            if (collection == null) return result.Abort("Collection not found");
            if (!collection.IsUserModifiable) return result.Abort("Cannot delete default collection");

            libraryDataContext.CollectionRepository.Delete(collectionID);
            return result;
        }
    }
}
