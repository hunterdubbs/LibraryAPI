using LibraryAPI.DAL.Repositories;
using LibraryAPI.Domain;

namespace LibraryAPI.DAL
{
    public class LibraryDataContext : ILibraryDataContext
    {
        public BookRepository BookRepository { get; protected set; }
        public LibraryRepository LibraryRepository { get; protected set; }
        public PermissionRepository PermissionRepository { get; protected set; }
        public AuthorRepository AuthorRepository { get; protected set; }
        public CollectionRepository CollectionRepository { get; protected set; }
        public UserRepository UserRepository { get; protected set; }
        public InviteRepository InviteRepository { get; protected set; }
        public PasswordResetCodeRepository PasswordResetCodeRepository { get; protected set; }
        public EmailVerificationCodeRepository EmailVerificationCodeRepository { get; protected set; }

        public LibraryDataContext()
        {
            BookRepository = new BookRepository();
            LibraryRepository = new LibraryRepository();
            PermissionRepository = new PermissionRepository();
            AuthorRepository = new AuthorRepository();
            CollectionRepository = new CollectionRepository();
            UserRepository = new UserRepository();
            InviteRepository = new InviteRepository();
            PasswordResetCodeRepository = new PasswordResetCodeRepository();
            EmailVerificationCodeRepository = new EmailVerificationCodeRepository();
        }

        public LibraryDataContext(UnitOfWork unitOfWork)
        {
            BookRepository = new BookRepository(unitOfWork);
            LibraryRepository = new LibraryRepository(unitOfWork);
            PermissionRepository = new PermissionRepository(unitOfWork);
            AuthorRepository = new AuthorRepository(unitOfWork);
            CollectionRepository = new CollectionRepository(unitOfWork);
            UserRepository = new UserRepository(unitOfWork);
            InviteRepository = new InviteRepository(unitOfWork);
            PasswordResetCodeRepository = new PasswordResetCodeRepository(unitOfWork);
            EmailVerificationCodeRepository = new EmailVerificationCodeRepository(unitOfWork);
        }
    }
}
