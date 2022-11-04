using LibraryAPI.Domain;
using LibraryAPI.Domain.Enum;
using LibraryAPI.Domain.Responses;
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

        public bool CheckPermissionOnBookIDAndCollectionIDs(int bookID, IEnumerable<int> collectionIDs, string userID, PermissionType minPermissionLevel)
        {
            Book book = libraryDataContext.BookRepository.GetByID(bookID);
            if (book == null) return false;

            if (!CheckPermissionOnLibraryID(book.LibraryID, userID, minPermissionLevel)) return false;
            var collectionsInLibrary = libraryDataContext.CollectionRepository.GetAllByLibraryID(book.LibraryID);
            return !collectionIDs.Except(collectionsInLibrary.Select(c => c.ID)).Any();
        }

        public Result<LibraryPermissionResponse> GetLibraryPermissions(int libraryID, string userID, out bool permissionDenied)
        {
            Result<LibraryPermissionResponse> result = new Result<LibraryPermissionResponse>();
            permissionDenied = false;

            var permissions = libraryDataContext.PermissionRepository.GetAllByLibrary(libraryID);
            if (!permissions.Any(p => p.PermissionLevel == PermissionType.Owner && p.UserID == userID))
            {
                permissionDenied = true;
                return result.Abort("You do not have the required permission for this library");
            }
            var invites = libraryDataContext.InviteRepository.GetAllByLibrary(libraryID);

            result.Value = new LibraryPermissionResponse()
            {
                Permissions = permissions,
                Invites = invites
            };
            return result;
        }

        public Result RemoveLibraryPermission(int libraryID, string targetUserID, string userID, out bool permissionDenied)
        {
            Result result = new Result();
            permissionDenied = false;

            if(!CheckPermissionOnLibraryID(libraryID, userID, PermissionType.Owner))
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to remove users from this library");
            }

            libraryDataContext.PermissionRepository.Delete(targetUserID, libraryID);
            return result;
        }

        public Result CreateInvite(Invite invite, string userID, out bool permissionDenied)
        {
            Result result = new Result();
            permissionDenied = false;

            if(!CheckPermissionOnLibraryID(invite.LibraryID, userID, PermissionType.Owner))
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to invite others to this library");
            }

            var existingInvites = libraryDataContext.InviteRepository.GetAllByLibrary(invite.LibraryID);
            if(existingInvites.Any(i => i.RecipientID == invite.RecipientID))
            {
                return result.Abort("User already invited");
            }

            libraryDataContext.InviteRepository.Add(invite);
            return result;
        }

        public Result DeleteInvite(int inviteID, string userID, out bool permissionDenied)
        {
            Result result = new Result();
            permissionDenied = false;

            Invite invite = libraryDataContext.InviteRepository.GetByID(inviteID);
            if (invite == null) return result.Abort("Invite not found");

            if(!CheckPermissionOnLibraryID(invite.LibraryID, userID, PermissionType.Owner))
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to delete this invite");
            }

            libraryDataContext.InviteRepository.Delete(invite.ID);
            return result;
        }

        public Result RejectInvite(int inviteID, string userID, out bool permissionDenied)
        {
            Result result = new Result();
            permissionDenied = false;

            Invite invite = libraryDataContext.InviteRepository.GetByID(inviteID);
            if (invite == null) return result.Abort("Invite not found");

            if(invite.RecipientID != userID)
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to reject this invite");
            }

            libraryDataContext.InviteRepository.Delete(invite.ID);
            return result;
        }

        public Result AcceptInvite(int inviteID, string userID, out bool permissionDenied)
        {
            Result result = new Result();
            permissionDenied = false;

            Invite invite = libraryDataContext.InviteRepository.GetByID(inviteID);
            if (invite == null) return result.Abort("Invite not found");

            if (invite.RecipientID != userID)
            {
                permissionDenied = true;
                return result.Abort("You do not have permission to reject this invite");
            }

            libraryDataContext.PermissionRepository.Add(userID, invite.LibraryID, invite.PermissionLevel);
            libraryDataContext.InviteRepository.Delete(invite.ID);
            return result;
        }
    }
}
