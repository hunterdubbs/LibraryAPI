using LibraryAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.LogicProcessors
{
    public class LibraryLogicProcessor
    {
        protected ILibraryDataContext libraryDataContext;
        protected PermissionLogicProcessor permissionLogicProcessor;

        public LibraryLogicProcessor(ILibraryDataContext libraryDataContext, PermissionLogicProcessor permissionLogicProcessor)
        {
            this.libraryDataContext = libraryDataContext;
            this.permissionLogicProcessor = permissionLogicProcessor;
        }

        public Result CreateLibrary(Library library, string userID)
        {
            libraryDataContext.LibraryRepository.Add(library);
            libraryDataContext.PermissionRepository.Add(userID, library.ID, Domain.Enum.PermissionType.Owner);
            return new Result();
        }

        public Result UpdateLibrary(Library library, string userID, out bool permissionDenied)
        {
            Result result = new Result();
            permissionDenied = false;

            if(!permissionLogicProcessor.CheckPermissionOnLibrary(library, userID, Domain.Enum.PermissionType.Owner))
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to modify this library");
            }

            libraryDataContext.LibraryRepository.Update(library);
            return result;
        }

        public Result DeleteLibrary(int libraryID, string userID, out bool permissionDenied)
        {
            Result result = new Result();
            permissionDenied = false;

            if(!permissionLogicProcessor.CheckPermissionOnLibraryID(libraryID, userID, Domain.Enum.PermissionType.Owner))
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to delete this library");
            }

            libraryDataContext.PermissionRepository.Delete(userID, libraryID);
            libraryDataContext.CollectionRepository.DeleteByLibraryID(libraryID);
            libraryDataContext.LibraryRepository.Delete(libraryID);
            return result;
        }
    }
}
