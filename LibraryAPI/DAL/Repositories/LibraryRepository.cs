using LibraryAPI.Domain;
using LibraryAPI.Domain.Enum;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.DAL.Repositories
{
    public class LibraryRepository : RepositoryBase
    {
        public LibraryRepository() { }

        public LibraryRepository(UnitOfWork unitOfWork) : base(unitOfWork) { }

        public void Add(Library library)
        {
            DbCommand cmd = CreateCommand(@"INSERT INTO tLibrary(sName, sOwner, dtCreated) VALUES (@sName, @sOwner, @dtCreated)");
            cmd.Parameters.Add(CreateParameter("@sName", library.Name));
            cmd.Parameters.Add(CreateParameter("@sOwner", library.Owner));
            cmd.Parameters.Add(CreateParameter("@dtCreated", DateTime.Now));
            cmd.ExecuteNonQuery();
            library.ID = (int)((MySqlConnector.MySqlCommand)cmd).LastInsertedId;
        }

        public void Update(Library library)
        {
            DbCommand cmd = CreateCommand(@"UPDATE tLibrary SET sName=@sName WHERE iID=@iID");
            cmd.Parameters.Add(CreateParameter("@sName", library.Name));
            cmd.Parameters.Add(CreateParameter("@iID", library.ID));
            cmd.ExecuteNonQuery();
        }

        public void Delete(int libraryID)
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tLibrary WHERE iID=@iID");
            cmd.Parameters.Add(CreateParameter("@iID", libraryID));
            cmd.ExecuteNonQuery();
        }

        public List<Library> GetAllByUser(string userID)
        {
            DbCommand cmd = CreateCommand(@"SELECT l.*, p.iPermissionLevel FROM tLibrary l INNER JOIN tPermission p WHERE l.iID=p.iLibraryID AND p.sUserID=@sUserID AND p.iPermissionLevel > 0");
            cmd.Parameters.Add(CreateParameter("@sUserID", userID));
            return ExtractData(cmd);
        }

        public Library GetByID(int id)
        {
            DbCommand cmd = CreateCommand(@"SELECT l.*, p.iPermissionLevel FROM tLibrary l INNER JOIN tPermission p WHERE l.iID=p.iLibraryID AND l.iID=@iID AND p.iPermissionLevel > 0");
            cmd.Parameters.Add(CreateParameter("@iID", id));
            return ExtractData(cmd).FirstOrDefault();
        }

        private List<Library> ExtractData(DbCommand cmd)
        {
            List<Library> results = new List<Library>();
            using(DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Library result = new Library();
                    result.ID = ReadInt(reader, "iID");
                    result.Name = ReadString(reader, "sName");
                    result.Owner = ReadString(reader, "sOwner");
                    result.CreatedDate = ReadDateTime(reader, "dtCreated");
                    result.Permissions = (PermissionType)ReadInt(reader, "iPermissionLevel");
                    results.Add(result);
                }
            }

            return results;
        }
    }
}
