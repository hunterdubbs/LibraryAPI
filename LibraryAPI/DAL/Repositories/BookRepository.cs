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
@"SELECT b.*, bax.iAuthorID, a.sFirstName, a.sLastName, t.iID AS 'iTagID', t.sName AS 'sTagName'
FROM tBook b LEFT OUTER JOIN
tBookAuthorXREF bax ON b.iID=bax.iBookID LEFT OUTER JOIN
tAuthor a ON bax.iAuthorID=a.iID LEFT OUTER JOIN
tBookTagXREF btx ON b.iID=btx.iBookID LEFT OUTER JOIN
tTag t ON btx.iTagID=t.iID
WHERE b.iID=@iID
ORDER BY b.iID, bax.iListPosition, t.iID");
            cmd.Parameters.Add(CreateParameter("@iID", id));
            return ExtractFullData(cmd).FirstOrDefault();
        }

        public List<Book> GetByCollectionID(int collectionID)
        {
            DbCommand cmd = CreateCommand(
@"SELECT b.*, bax.iAuthorID, a.sFirstName, a.sLastName, t.iID AS 'iTagID', t.sName AS 'sTagName' 
FROM tBook b INNER JOIN
tCollectionBookXREF cbx ON b.iID=cbx.iBookID LEFT OUTER JOIN
tBookAuthorXREF bax ON b.iID=bax.iBookID LEFT OUTER JOIN
tAuthor a ON bax.iAuthorID=a.iID LEFT OUTER JOIN
tBookTagXREF btx ON b.iID=btx.iBookID LEFT OUTER JOIN
tTag t ON btx.iTagID=t.iID
WHERE cbx.iCollectionID=@iCollectionID
ORDER BY b.iID, bax.iListPosition, t.iID");
            cmd.Parameters.Add(CreateParameter("@iCollectionID", collectionID));
            return ExtractFullData(cmd);
        }

        public List<Book> GetAll()
        {
            DbCommand cmd = CreateCommand(
@"SELECT b.*, bax.iAuthorID, a.sFirstName, a.sLastName, t.iID AS 'iTagID', t.sName AS 'sTagName'
FROM tBook b LEFT OUTER JOIN
tBookAuthorXREF bax ON b.iID=bax.iBookID LEFT OUTER JOIN
tAuthor a ON bax.iAuthorID=a.iID LEFT OUTER JOIN
tBookTagXREF btx ON b.iID=btx.iBookID LEFT OUTER JOIN
tTag t ON btx.iTagID=t.iID
ORDER BY b.iID, bax.iListPosition, t.iID");
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
    dtPublished,
    sSeries,
    sVolume
) VALUES (
    @iLibraryID,
    @sTitle,
    @sSynopsis,
    @dtAdded,
    @dtPublished,
    @sSeries,
    @sVolume
)");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", book.LibraryID));
            cmd.Parameters.Add(CreateParameter("@sTitle", book.Title));
            cmd.Parameters.Add(CreateParameter("@sSynopsis", book.Synopsis));
            cmd.Parameters.Add(CreateParameter("@dtAdded", book.DateAdded));
            cmd.Parameters.Add(CreateParameter("@dtPublished", book.DatePublished));
            cmd.Parameters.Add(CreateParameter("@sSeries", book.Series));
            cmd.Parameters.Add(CreateParameter("@sVolume", book.Volume));
            cmd.ExecuteNonQuery();
            book.ID = (int)((MySqlConnector.MySqlCommand)cmd).LastInsertedId;

            for(int i = 0; i < book.Authors.Count; i++)
            {
                DbCommand authorCmd = CreateCommand(@"INSERT INTO tBookAuthorXREF(iBookID, iAuthorID, iListPosition) VALUES (@iBookID, @iAuthorID, @iListPosition)");
                authorCmd.Parameters.Add(CreateParameter("@iBookID", book.ID));
                authorCmd.Parameters.Add(CreateParameter("@iAuthorID", book.Authors[i].ID));
                authorCmd.Parameters.Add(CreateParameter("@iListPosition", i));
                authorCmd.ExecuteNonQuery();
            }

            for(int i = 0; i < book.Tags.Count; i++)
            {
                DbCommand tagCmd = CreateCommand(@"INSERT INTO tBookTagXREF(iBookID, iTagID) VALUES (@iBookID, @iTagID)");
                tagCmd.Parameters.Add(CreateParameter("@iBookID", book.ID));
                tagCmd.Parameters.Add(CreateParameter("@iTagID", book.Tags[i].ID));
                tagCmd.ExecuteNonQuery();
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
    dtPublished = @dtPublished,
    sSeries = @sSeries,
    sVolume = @sVolume
WHERE iID = @iID");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", book.LibraryID));
            cmd.Parameters.Add(CreateParameter("@sTitle", book.Title));
            cmd.Parameters.Add(CreateParameter("@sSynopsis", book.Synopsis));
            cmd.Parameters.Add(CreateParameter("@dtAdded", book.DateAdded));
            cmd.Parameters.Add(CreateParameter("@dtPublished", book.DatePublished));
            cmd.Parameters.Add(CreateParameter("@iID", book.ID));
            cmd.Parameters.Add(CreateParameter("@sSeries", book.Series));
            cmd.Parameters.Add(CreateParameter("@sVolume", book.Volume));
            cmd.ExecuteNonQuery();

            DbCommand cleanAuthorsCmd = CreateCommand(@"DELETE FROM tBookAuthorXREF WHERE iBookID=@iBookID");
            cleanAuthorsCmd.Parameters.Add(CreateParameter("@iBookID", book.ID));
            cleanAuthorsCmd.ExecuteNonQuery();

            for (int i = 0; i < book.Authors.Count; i++)
            {
                DbCommand authorCmd = CreateCommand(@"INSERT INTO tBookAuthorXREF(iBookID, iAuthorID, iListPosition) VALUES (@iBookID, @iAuthorID, @iListPosition)");
                authorCmd.Parameters.Add(CreateParameter("@iBookID", book.ID));
                authorCmd.Parameters.Add(CreateParameter("@iAuthorID", book.Authors[i].ID));
                authorCmd.Parameters.Add(CreateParameter("@iListPosition", i));
                authorCmd.ExecuteNonQuery();
            }

            DbCommand cleanTagsCmd = CreateCommand(@"DELETE FROM tBookTagXREF WHERE iBookID=@iBookID");
            cleanTagsCmd.Parameters.Add(CreateParameter("@iBookID", book.ID));
            cleanTagsCmd.ExecuteNonQuery();

            for (int i = 0; i < book.Tags.Count; i++)
            {
                DbCommand tagCmd = CreateCommand(@"INSERT INTO tBookTagXREF(iBookID, iTagID) VALUES (@iBookID, @iTagID)");
                tagCmd.Parameters.Add(CreateParameter("@iBookID", book.ID));
                tagCmd.Parameters.Add(CreateParameter("@iTagID", book.Tags[i].ID));
                tagCmd.ExecuteNonQuery();
            }
        }

        public void Delete(int bookID)
        {
            DbCommand authorCmd = CreateCommand(@"DELETE FROM tBookAuthorXREF WHERE iBookID=@iBookID");
            authorCmd.Parameters.Add(CreateParameter("@iBookID", bookID));
            authorCmd.ExecuteNonQuery();

            DbCommand collectionMembershipCmd = CreateCommand(@"DELETE FROM tCollectionBookXREF WHERE iBookID=@iBookID");
            collectionMembershipCmd.Parameters.Add(CreateParameter("@iBookID", bookID));
            collectionMembershipCmd.ExecuteNonQuery();

            DbCommand bookTaagCmd = CreateCommand(@"DELETE FROM tBookTagXREF WHERE iBookID=@iBookID");
            bookTaagCmd.Parameters.Add(CreateParameter("@iBookID", bookID));
            bookTaagCmd.ExecuteNonQuery();

            DbCommand bookCmd = CreateCommand(@"DELETE FROM tBook WHERE iID=@iID");
            bookCmd.Parameters.Add(CreateParameter("@iID", bookID));
            bookCmd.ExecuteNonQuery();
        }

        public void DeleteByUserID(string userID)
        {
            DbCommand authorCmd = CreateCommand(@"DELETE FROM tBookAuthorXREF WHERE iBookID IN
(SELECT b.iID
FROM tBook b
INNER JOIN tLibrary l ON b.iLibraryID=l.iID
INNER JOIN tPermission p ON l.iID=p.iLibraryID
WHERE p.iPermissionLevel=3 AND p.sUserID=@sUserID)");
            authorCmd.Parameters.Add(CreateParameter("@sUserID", userID));
            authorCmd.ExecuteNonQuery();

            DbCommand collectionMembershipCmd = CreateCommand(@"DELETE FROM tCollectionBookXREF WHERE iBookID IN
(SELECT b.iID
FROM tBook b
INNER JOIN tLibrary l ON b.iLibraryID=l.iID
INNER JOIN tPermission p ON l.iID=p.iLibraryID
WHERE p.iPermissionLevel=3 AND p.sUserID=@sUserID)");
            collectionMembershipCmd.Parameters.Add(CreateParameter("@sUserID", userID));
            collectionMembershipCmd.ExecuteNonQuery();

            DbCommand bookTagsCmd = CreateCommand(@"DELETE FROM tBookTagXREF WHERE iBookID IN
(SELECT b.iID
FROM tBook b
INNER JOIN tLibrary l ON b.iLibraryID=l.iID
INNER JOIN tPermission p ON l.iID=p.iLibraryID
WHERE p.iPermissionLevel=3 AND p.sUserID=@sUserID)");
            bookTagsCmd.Parameters.Add(CreateParameter("@sUserID", userID));
            bookTagsCmd.ExecuteNonQuery();

            DbCommand bookCmd = CreateCommand(@"DELETE FROM tBook WHERE iLibraryID IN
(SELECT l.iID
FROM tLibrary l
INNER JOIN tPermission p ON l.iID=p.iLibraryID
WHERE p.iPermissionLevel=3 AND p.sUserID=@sUserID)");
            bookCmd.Parameters.Add(CreateParameter("@sUserID", userID));
            bookCmd.ExecuteNonQuery();
        }

        public Dictionary<int, int> GetBookCountByLibrary()
        {
            Dictionary<int, int> results = new Dictionary<int, int>();

            DbCommand cmd = CreateCommand(@"SELECT iLibraryID, COUNT(iID) AS 'Count' FROM tBook GROUP BY iLibraryID");
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int libraryID = ReadInt(reader, "iLibraryID");
                    int count = ReadInt(reader, "Count");
                    results.Add(libraryID, count);
                }
            }

            return results;
        }

        public Dictionary<int, int> GetBookCountByCollection(int libraryID)
        {
            Dictionary<int, int> results = new Dictionary<int, int>();

            DbCommand cmd = CreateCommand(@"SELECT iCollectionID, COUNT(iBookID) AS 'Count' FROM tCollectionBookXREF x INNER JOIN tCollection c ON (x.iCollectionID=c.iID) WHERE iLibraryID=@iLibraryID GROUP BY iCollectionID");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", libraryID));
            using(DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int collectionID = ReadInt(reader, "iCollectionID");
                    int count = ReadInt(reader, "Count");
                    results.Add(collectionID, count);
                }
            }

            return results;
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
                    result.Series = ReadString(reader, "sSeries");
                    result.Volume = ReadString(reader, "sVolume");
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
                int lastAuthorID = 0;
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
                        book.Tags = new List<Tag>();
                        book.Series = ReadString(reader, "sSeries");
                        book.Volume = ReadString(reader, "sVolume");
                        lastID = nextID;
                    
                    }
                    int nextAuthorID = ReadInt(reader, "iAuthorID");
                    if (nextAuthorID != 0 && lastAuthorID != nextAuthorID)
                    {
                        Author author = new Author();
                        author.ID = nextAuthorID;
                        lastAuthorID = nextAuthorID;
                        if (author.ID == 0) continue;
                        author.FirstName = ReadString(reader, "sFirstName");
                        author.LastName = ReadString(reader, "sLastName");
                        book.Authors.Add(author);
                    }

                    int nextTagID = ReadInt(reader, "iTagID");
                    if(nextTagID != 0 && !book.Tags.Any(t => t.ID == nextTagID))
                    {
                        Tag tag = new Tag();
                        tag.ID = nextTagID;
                        tag.LibraryID = book.LibraryID;
                        tag.Name = ReadString(reader, "sTagName");
                        book.Tags.Add(tag);
                    }
                }
                if (book != null) results.Add(book);
            }

            return results;
        }
    }
}
