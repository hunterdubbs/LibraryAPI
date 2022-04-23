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
    }
}
