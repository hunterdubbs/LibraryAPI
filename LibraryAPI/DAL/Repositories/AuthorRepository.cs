using LibraryAPI.Domain;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace LibraryAPI.DAL.Repositories
{
    public class AuthorRepository : RepositoryBase
    {
        public AuthorRepository() : base() { }

        public AuthorRepository(UnitOfWork unitOfWork) : base(unitOfWork) { }

        public void Add(Author author)
        {
            DbCommand cmd = CreateCommand(@"INSERT INTO tAuthor(sFirstName, sLastName) VALUES (@sFirstName, @sLastName) RETURNING iID");
            cmd.Parameters.Add(CreateParameter("@sFirstName", author.FirstName));
            cmd.Parameters.Add(CreateParameter("@sLastName", author.LastName));
            author.ID = (int)cmd.ExecuteScalar();
        }

        public List<Author> GetByBookID(int bookID)
        {
            DbCommand cmd = CreateCommand(
@"SELECT a.*
FROM tAuthor a INNER JOIN
tBookAuthorXREF x ON a.iID=x.iAuthorID
WHERE x.iBookID=@iBookID
ORDER BY x.iListPosition");
            cmd.Parameters.Add(CreateParameter("@iBookID", bookID));
            return ExtractData(cmd);
        }

        public List<Author> GetByFullNamePart(string searchTerm)
        {
            DbCommand cmd = CreateCommand(
@"SELECT *
FROM tAuthor
WHERE CONCAT(sFirstName, ' ', sLastName) ILIKE @searchTerm");
            cmd.Parameters.Add(CreateParameter("@searchTerm", '%' + searchTerm + '%'));
            return ExtractData(cmd);
        }

        public Author GetByFirstLastName(string firstName, string lastName)
        {
            DbCommand cmd = CreateCommand(
@"SELECT *
FROM tAuthor
WHERE UPPER(sFirstName)=@sFirstName AND UPPER(sLastName)=@sLastName");
            cmd.Parameters.Add(CreateParameter("@sFirstName", firstName.ToUpper()));
            cmd.Parameters.Add(CreateParameter("@sLastName", lastName.ToUpper()));
            return ExtractData(cmd).FirstOrDefault();
        }

        private List<Author> ExtractData(DbCommand cmd)
        {
            List<Author> results = new List<Author>();
            using(DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Author result = new Author();
                    result.ID = ReadInt(reader, "iID");
                    result.FirstName = ReadString(reader, "sFirstName");
                    result.LastName = ReadString(reader, "sLastName");
                    results.Add(result);
                }
            }

            return results;
        }
    }
}
