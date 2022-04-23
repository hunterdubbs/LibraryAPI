using LibraryAPI.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain
{
    public interface ILibraryDataContext
    {
        BookRepository BookRepository { get; }
        LibraryRepository LibraryRepository { get; }
        PermissionRepository PermissionRepository { get; }
        AuthorRepository AuthorRepository { get; }
        CollectionRepository CollectionRepository { get; }
    }
}
