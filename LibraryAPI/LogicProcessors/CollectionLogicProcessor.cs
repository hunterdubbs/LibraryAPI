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

        public Result AddBookToCollection(int collectionID, int bookID)
        {
            Result result = new Result();



            return result;
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

            libraryDataContext.CollectionRepository.Delete(collectionID);
            return result;
        }
    }
}
