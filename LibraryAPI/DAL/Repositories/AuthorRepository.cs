using LibraryAPI.Domain;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.DAL.Repositories
{
    public class AuthorRepository : RepositoryBase
    {
        public AuthorRepository() : base() { }

        public AuthorRepository(UnitOfWork unitOfWork) : base(unitOfWork) { }

        public void Add(Author author)
        {
            throw new NotImplementedException();
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
