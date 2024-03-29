using LibraryAPI.Domain;
using LibraryAPI.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace LibraryAPI.DAL.Repositories
{
    public class LibraryRepository : RepositoryBase
    {
        public LibraryRepository() { }

        public LibraryRepository(UnitOfWork unitOfWork) : base(unitOfWork) { }

        public void Add(Library library)
        {
            DbCommand cmd = CreateCommand(@"INSERT INTO tLibrary(sName, sOwner, dtCreated, iDefaultCollectionID) VALUES (@sName, @sOwner, @dtCreated, @iDefaultCollectionID) RETURNING iID");
            cmd.Parameters.Add(CreateParameter("@sName", library.Name));
            cmd.Parameters.Add(CreateParameter("@sOwner", library.Owner));
            cmd.Parameters.Add(CreateParameter("@dtCreated", DateTime.Now));
            cmd.Parameters.Add(CreateParameter("@iDefaultCollectionID", library.DefaultCollectionID));
            library.ID = (int)cmd.ExecuteScalar();
        }

        public void Update(Library library)
        {
            DbCommand cmd = CreateCommand(@"UPDATE tLibrary SET sName=@sName, iDefaultCollectionID=@iDefaultCollectionID WHERE iID=@iID");
            cmd.Parameters.Add(CreateParameter("@sName", library.Name));
            cmd.Parameters.Add(CreateParameter("@iID", library.ID));
            cmd.Parameters.Add(CreateParameter("@iDefaultCollectionID", library.DefaultCollectionID));
            cmd.ExecuteNonQuery();
        }

        public void Delete(int libraryID)
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tLibrary WHERE iID=@iID");
            cmd.Parameters.Add(CreateParameter("@iID", libraryID));
            cmd.ExecuteNonQuery();
        }

        public void DeleteByUserID(string userID)
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tLibrary WHERE iID NOT IN (SELECT iLibraryID FROM tPermission)");
            cmd.Parameters.Add(CreateParameter("@sUserID", userID));
            cmd.ExecuteNonQuery();
        }

        public List<Library> GetAllByUser(string userID)
        {
            DbCommand cmd = CreateCommand(@"SELECT l.*, p.iPermissionLevel FROM tLibrary l INNER JOIN tPermission p ON l.iID=p.iLibraryID WHERE p.sUserID=@sUserID AND p.iPermissionLevel > 0");
            cmd.Parameters.Add(CreateParameter("@sUserID", userID));
            return ExtractData(cmd);
        }

        public Library GetByID(int id)
        {
            DbCommand cmd = CreateCommand(@"SELECT l.*, p.iPermissionLevel FROM tLibrary l INNER JOIN tPermission p ON l.iID=p.iLibraryID WHERE l.iID=@iID AND p.iPermissionLevel > 0");
            cmd.Parameters.Add(CreateParameter("@iID", id));
            return ExtractData(cmd).FirstOrDefault();
        }

        public Library GetByBookID(int bookID)
        {
            DbCommand cmd = CreateCommand(@"SELECT l.*, p.iPermissionLevel FROM tBook b INNER JOIN tLibrary l ON b.iLibraryID=l.iID INNER JOIN tPermission p ON l.iID=p.iLibraryID WHERE b.iID=@iID AND p.iPermissionLevel > 0");
            cmd.Parameters.Add(CreateParameter("@iID", bookID));
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
                    result.DefaultCollectionID = ReadInt(reader, "iDefaultCollectionID");
                    results.Add(result);
                }
            }

            return results;
        }
    }
}
