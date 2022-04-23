using LibraryAPI.DAL.Repositories;
using LibraryAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.DAL
{
    public class LibraryDataContext : ILibraryDataContext
    {
        public BookRepository BookRepository { get; protected set; }
        public LibraryRepository LibraryRepository { get; protected set; }
        public PermissionRepository PermissionRepository { get; protected set; }
        public AuthorRepository AuthorRepository { get; protected set; }
        public CollectionRepository CollectionRepository { get; protected set; }

        public LibraryDataContext()
        {
            BookRepository = new BookRepository();
            LibraryRepository = new LibraryRepository();
            PermissionRepository = new PermissionRepository();
            AuthorRepository = new AuthorRepository();
            CollectionRepository = new CollectionRepository();
        }

        public LibraryDataContext(UnitOfWork unitOfWork)
        {
            BookRepository = new BookRepository(unitOfWork);
            LibraryRepository = new LibraryRepository(unitOfWork);
            PermissionRepository = new PermissionRepository(unitOfWork);
            AuthorRepository = new AuthorRepository(unitOfWork);
            CollectionRepository = new CollectionRepository(unitOfWork);
        }
    }
}
