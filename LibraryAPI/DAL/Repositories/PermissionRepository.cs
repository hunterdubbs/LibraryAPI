using LibraryAPI.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.DAL.Repositories
{
    public class PermissionRepository : RepositoryBase
    {
        public PermissionRepository() : base() { }

        public PermissionRepository(UnitOfWork unitOfWork) : base(unitOfWork) { }

        public PermissionType GetByUserByLibrary(string userID, int libraryID)
        {
            DbCommand cmd = CreateCommand(@"SELECT iPermissionLevel FROM tPermission WHERE iLibraryID=@iLibraryID AND sUserID=@sUserID");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", libraryID));
            cmd.Parameters.Add(CreateParameter("@sUserID", userID));
            return ExtractData(cmd);
        }

        public void Add(string userId, int libraryID, PermissionType permissionLevel)
        {
            DbCommand cmd = CreateCommand(@"INSERT INTO tPermission(sUserID, iLibraryID, iPermissionLevel) VALUES (@sUserID, @iLibraryID, @iPermissionlevel)");
            cmd.Parameters.Add(CreateParameter("@sUserID", userId));
            cmd.Parameters.Add(CreateParameter("@iLibraryID", libraryID));
            cmd.Parameters.Add(CreateParameter("@iPermissionLevel", (int)permissionLevel));
            cmd.ExecuteNonQuery();
        }

        public void Delete(string userId, int libraryId)
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tPermission WHERE iLibraryID=@iLibraryID AND sUserID=@sUserID");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", libraryId));
            cmd.Parameters.Add(CreateParameter("@sUserID", userId));
            cmd.ExecuteNonQuery();
        }

        private PermissionType ExtractData(DbCommand cmd)
        {
            using(DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    return (PermissionType)ReadInt(reader, "iPermissionLevel");
                }
            }
            return PermissionType.None;
        }
    }
}
