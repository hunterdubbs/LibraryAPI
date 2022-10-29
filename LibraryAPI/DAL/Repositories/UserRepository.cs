using LibraryAPI.Domain;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.DAL.Repositories
{
    public class UserRepository : RepositoryBase
    {
        public UserRepository() : base() { }

        public UserRepository(UnitOfWork unitOfWork) : base(unitOfWork) { }

        public List<InvitableUser> SearchUsersByUsername(string searchTerm)
        {
            DbCommand cmd = CreateCommand(@"SELECT Id, UserName FROM AspNetUsers WHERE UserName LIKE @searchTerm");
            cmd.Parameters.Add(CreateParameter("@searchTerm", '%' + searchTerm + '%'));

            List<InvitableUser> results = new List<InvitableUser>();
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    InvitableUser result = new InvitableUser();
                    result.UserId = ReadString(reader, "Id");
                    result.Username = ReadString(reader, "UserName");
                    results.Add(result);
                }
            }

            return results;
        }
    }
}
