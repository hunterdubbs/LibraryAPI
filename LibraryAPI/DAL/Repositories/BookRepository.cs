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
tBookAuthorXREF bax ON b.iID=bax.iBookID LEFT OUTER JOIN
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
tBookAuthorXREF bax ON b.iID=bax.iBookID LEFT OUTER JOIN
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
tBookAuthorXREF bax ON b.iID=bax.iBookID LEFT OUTER JOIN
tAuthor a ON bax.iAuthorID=a.iID
ORDER BY b.iID, bax.iListPosition");
            return ExtractFullData(cmd);
        }

        public void Add(Book book)
        {
            DbCommand cmd = CreateCommand(
@"INSERT INTO tBook
(
    iLibraryID,
    sTitle,
    sSynopsis,
    dtAdded,
    dtPublished
) VALUES (
    @iLibraryID,
    @sTitle,
    @sSynopsis,
    @dtAdded,
    @dtPublished
)");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", book.LibraryID));
            cmd.Parameters.Add(CreateParameter("@sTitle", book.Title));
            cmd.Parameters.Add(CreateParameter("@sSynopsis", book.Synopsis));
            cmd.Parameters.Add(CreateParameter("@dtAdded", book.DateAdded));
            cmd.Parameters.Add(CreateParameter("@dtPublished", book.DatePublished));
            cmd.ExecuteNonQuery();
            book.ID = (int)((MySqlConnector.MySqlCommand)cmd).LastInsertedId;

            for(int i = 0; i < book.Authors.Count; i++)
            {
                DbCommand authorCmd = CreateCommand(@"INSERT INTO tBookAuthorXREF(iBookID, iAuthorID, iListPosition) VALUES (@iBookID, @iAuthorID, @iListPosition");
                authorCmd.Parameters.Add(CreateParameter("@iBookID", book.ID));
                authorCmd.Parameters.Add(CreateParameter("@iAuthorID", book.Authors[i].ID));
                authorCmd.Parameters.Add(CreateParameter("@iListPosition", i));
                authorCmd.ExecuteNonQuery();
            }
        }

        public void Update(Book book)
        {
            DbCommand cmd = CreateCommand(
@"UPDATE tBook SET
    iLibraryID = @ilibraryID,
    sTitle = @sTitle,
    sSynopsis = @sSynopsis,
    dtAdded = @dtAdded,
    dtPublished = @dtPublished
WHERE iID = @iID");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", book.LibraryID));
            cmd.Parameters.Add(CreateParameter("@sTitle", book.Title));
            cmd.Parameters.Add(CreateParameter("@sSynopsis", book.Synopsis));
            cmd.Parameters.Add(CreateParameter("@dtAdded", book.DateAdded));
            cmd.Parameters.Add(CreateParameter("@dtPublished", book.DatePublished));
            cmd.Parameters.Add(CreateParameter("@iID", book.ID));
            cmd.ExecuteNonQuery();

            DbCommand cleanAuthorsCmd = CreateCommand(@"DELETE FROM tBookAuthorXREF WHERE iBookID=@iBookID");
            cleanAuthorsCmd.Parameters.Add(CreateParameter("@iBookID", book.ID));
            cleanAuthorsCmd.ExecuteNonQuery();

            for (int i = 0; i < book.Authors.Count; i++)
            {
                DbCommand authorCmd = CreateCommand(@"INSERT INTO tBookAuthorXREF(iBookID, iAuthorID, iListPosition) VALUES (@iBookID, @iAuthorID, @iListPosition");
                authorCmd.Parameters.Add(CreateParameter("@iBookID", book.ID));
                authorCmd.Parameters.Add(CreateParameter("@iAuthorID", book.Authors[i].ID));
                authorCmd.Parameters.Add(CreateParameter("@iListPosition", i));
                authorCmd.ExecuteNonQuery();
            }
        }

        public void Delete(int bookID)
        {
            DbCommand authorCmd = CreateCommand(@"DELETE FROM tBookAuthorXREF WHERE iBookID=@iBookID");
            authorCmd.Parameters.Add(CreateParameter("@iBookID", bookID));
            authorCmd.ExecuteNonQuery();

            DbCommand bookCmd = CreateCommand(@"DELETE FROM tBook WHERE iID=@iID");
            bookCmd.Parameters.Add(CreateParameter("@iID", bookID));
            bookCmd.ExecuteNonQuery();
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
                    int nextID = ReadInt(reader, "iID");
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
                    if (author.ID == 0) continue;
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
