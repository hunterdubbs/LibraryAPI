using LibraryAPI.Domain;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.DAL.Repositories
{
    public class BookRepository : RepositoryBase
    {
        public BookRepository() : base() { }

        public BookRepository(UnitOfWork unitOfWork) : base(unitOfWork) { }

        public Book GetByID(int id)
        {
            DbCommand cmd = CreateCommand(
@"SELECT * 
FROM tBook b LEFT OUTER JOIN
tBookAuthorXREF bax ON b.iID=bax.iBookID INNER JOIN
tAuthor a ON bax.iAuthorID=a.iID
WHERE b.iID=@iID
ORDER BY b.iID, bax.iListPosition");
            cmd.Parameters.Add(CreateParameter("@iID", id));
            return ExtractFullData(cmd).FirstOrDefault();
        }

        public List<Book> GetByCollectionID(int collectionID)
        {
            DbCommand cmd = CreateCommand(
@"SELECT * 
FROM tBook b INNER JOIN
tCollectionBookXREF cbx ON b.iID=cbx.iBookID LEFT OUTER JOIN
tBookAuthorXREF bax ON b.iID=bax.iBookID INNER JOIN
tAuthor a ON bax.iAuthorID=a.iID
WHERE cbx.iCollectionID=@iCollectionID
ORDER BY b.iID, bax.iListPosition");
            cmd.Parameters.Add(CreateParameter("@iCollectionID", collectionID));
            return ExtractFullData(cmd);
        }

        public List<Book> GetAll()
        {
            DbCommand cmd = CreateCommand(
@"SELECT * 
FROM tBook b LEFT OUTER JOIN
tBookAuthorXREF bax ON b.iID=bax.iBookID INNER JOIN
tAuthor a ON bax.iAuthorID=a.iID
ORDER BY b.iID, bax.iListPosition");
            return ExtractFullData(cmd);
        }

        public void Add(Book book)
        {
            throw new NotImplementedException();
        }

        private List<Book> ExtractData(DbCommand cmd)
        {
            List<Book> results = new List<Book>();
            using(DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Book result = new Book();
                    result.ID = ReadInt(reader, "iID");
                    result.LibraryID = ReadInt(reader, "iLibraryID");
                    result.Title = ReadString(reader, "sTitle");
                    result.Synopsis = ReadString(reader, "sSynopsis");
                    result.DateAdded = ReadDateTime(reader, "dtAdded");
                    result.DatePublished = ReadDateTime(reader, "dtPublished");
                    results.Add(result);
                }
            }

            return results;
        }

        private List<Book> ExtractFullData(DbCommand cmd)
        {
            List<Book> results = new List<Book>();
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                int lastID = 0;
                Book book = null;
                while (reader.Read())
                {
                    int nextID = ReadInt(reader, "iBookID");
                    if(lastID != nextID)
                    {
                        if (book != null) results.Add(book);
                        book = new Book();
                        book.ID = nextID;
                        book.LibraryID = ReadInt(reader, "iLibraryID");
                        book.Title = ReadString(reader, "sTitle");
                        book.Synopsis = ReadString(reader, "sSynopsis");
                        book.DateAdded = ReadDateTime(reader, "dtAdded");
                        book.DatePublished = ReadDateTime(reader, "dtPublished");
                        book.Authors = new List<Author>();
                        lastID = nextID;
                    }
                    Author author = new Author();
                    author.ID = ReadInt(reader, "iAuthorID");
                    author.FirstName = ReadString(reader, "sFirstName");
                    author.LastName = ReadString(reader, "sLastName");
                    book.Authors.Add(author);
                }
                if (book != null) results.Add(book);
            }

            return results;
        }
    }
}
