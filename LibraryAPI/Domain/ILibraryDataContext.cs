using LibraryAPI.DAL.Repositories;

namespace LibraryAPI.Domain
{
    public interface ILibraryDataContext
    {
        BookRepository BookRepository { get; }
        LibraryRepository LibraryRepository { get; }
        PermissionRepository PermissionRepository { get; }
        AuthorRepository AuthorRepository { get; }
        CollectionRepository CollectionRepository { get; }
        UserRepository UserRepository { get; }
        InviteRepository InviteRepository { get; }
        PasswordResetCodeRepository PasswordResetCodeRepository { get; }
        EmailVerificationCodeRepository EmailVerificationCodeRepository { get; }
    }
}
