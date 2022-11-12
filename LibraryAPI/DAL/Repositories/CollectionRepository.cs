using LibraryAPI.Domain;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.DAL.Repositories
{
    public class CollectionRepository : RepositoryBase
    {
        public CollectionRepository() : base() { }

        public CollectionRepository(UnitOfWork unitOfWork) : base(unitOfWork) { }

        public Collection GetByID(int id)
        {
            DbCommand cmd = CreateCommand(@"SELECT * FROM tCollection WHERE iID=@iID");
            cmd.Parameters.Add(CreateParameter("@iID", id));
            return ExtractData(cmd).FirstOrDefault();
        }

        public List<Collection> GetAllByLibraryID(int libraryID)
        {
            DbCommand cmd = CreateCommand(@"SELECT * FROM tCollection WHERE iLibraryID=@iLibraryID");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", libraryID));
            return ExtractData(cmd);
        }

        public List<Collection> GetAllByBookID(int bookID)
        {
            DbCommand cmd = CreateCommand(
@"SELECT c.* 
FROM tCollection c INNER JOIN
tCollectionBookXREF x ON c.iID=x.iCollectionID
WHERE x.iBookID=@iBookID");
            cmd.Parameters.Add(CreateParameter("@iBookID", bookID));
            return ExtractData(cmd);
        }

        public void DeleteByLibraryID(int libraryID)
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tCollection WHERE iLibraryID=@iLibraryID");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", libraryID));
            cmd.ExecuteNonQuery();
        }

        public void Delete(int collectionID)
        {
            DbCommand xrefCmd = CreateCommand(@"DELETE FROM tCollectionBookXREF WHERE iCollectionID=@iCollectionID");
            xrefCmd.Parameters.Add(CreateParameter("@iCollectionID", collectionID));
            xrefCmd.ExecuteNonQuery();

            DbCommand cmd = CreateCommand(@"DELETE FROM tCollection WHERE iID=@iID");
            cmd.Parameters.Add(CreateParameter("@iID", collectionID));
            cmd.ExecuteNonQuery();
        }

        public void DeleteByUserID(string userID)
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tCollection WHERE iLibraryID IN
(SELECT l.iID
FROM tLibrary l
INNER JOIN tPermission p ON l.iID=p.iLibraryID
WHERE p.iPermissionLevel=3 AND p.sUserID=@sUserID)");
            cmd.Parameters.Add(CreateParameter("@sUserID", userID));
            cmd.ExecuteNonQuery();
        }

        public void Add(Collection collection)
        {
            DbCommand cmd = CreateCommand(@"INSERT INTO tCollection(iLibraryID, iParentCollectionID, sName, sDescription, bUserModifiable) VALUES (@iLibraryID, @iParentCollectionID, @sName, @sDescription, @bUserModifiable)");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", collection.LibraryID));
            cmd.Parameters.Add(CreateParameter("@iParentCollectionID", collection.ParentCollectionID));
            cmd.Parameters.Add(CreateParameter("@sName", collection.Name));
            cmd.Parameters.Add(CreateParameter("@sDescription", collection.Description));
            cmd.Parameters.Add(CreateParameter("@bUserModifiable", collection.IsUserModifiable));
            cmd.ExecuteNonQuery();
            collection.ID = (int)((MySqlConnector.MySqlCommand)cmd).LastInsertedId;
        }

        public void Update(Collection collection)
        {
            DbCommand cmd = CreateCommand(@"UPDATE tCollection SET sName=@sName, sDescription=@sDescription WHERE iID=@iID");
            cmd.Parameters.Add(CreateParameter("@sName", collection.Name));
            cmd.Parameters.Add(CreateParameter("@sDescription", collection.Description));
            cmd.Parameters.Add(CreateParameter("@iID", collection.ID));
            cmd.ExecuteNonQuery();
        }

        public void AddBookToCollection(int bookID, int collectionID)
        {
            DbCommand cmd = CreateCommand(@"INSERT INTO tCollectionBookXREF(iCollectionID, iBookID) VALUES (@iCollectionID, @iBookID)");
            cmd.Parameters.Add(CreateParameter("@iCollectionID", collectionID));
            cmd.Parameters.Add(CreateParameter("@iBookID", bookID));
            cmd.ExecuteNonQuery();
        }

        public void RemoveBookFromCollection(int bookID, int collectionID)
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tCollectionBookXREF WHERE iBookID=@iBookID AND iCollectionID=@iCollectionID");
            cmd.Parameters.Add(CreateParameter("@iCollectionID", collectionID));
            cmd.Parameters.Add(CreateParameter("@iBookID", bookID));
            cmd.ExecuteNonQuery();
        }

        public void RemoveBookFromAllCollections(int bookID)
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tCollectionBookXREF WHERE iBookID=@iBookID");
            cmd.Parameters.Add(CreateParameter("@iBookID", bookID));
            cmd.ExecuteNonQuery();
        }

        public List<CollectionMembership> GetAllWithMembershipStatusByBookID(int bookID, int libraryID)
        {
            DbCommand cmd = CreateCommand(
@"SELECT c.*, MAX(IF(x.iBookID IS NOT NULL AND x.iBookID=@iBookID, 1, 0)) AS 'bIsMember'
FROM tCollection c LEFT OUTER JOIN 
tCollectionBookXREF x ON c.iID=x.iCollectionID 
WHERE iLibraryID=@iLibraryID
GROUP BY c.iID, c.iLibraryID, c.iParentCollectionID, c.sName, c.sDescription;");
            cmd.Parameters.Add(CreateParameter("@iBookID", bookID));
            cmd.Parameters.Add(CreateParameter("@iLibraryID", libraryID));

            List<CollectionMembership> results = new List<CollectionMembership>();
            using(DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    CollectionMembership result = new CollectionMembership();
                    result.ID = ReadInt(reader, "iID");
                    result.LibraryID = ReadInt(reader, "iLibraryID");
                    result.ParentCollectionID = ReadInt(reader, "iParentCollectionID");
                    result.Name = ReadString(reader, "sName");
                    result.Description = ReadString(reader, "sDescription");
                    result.IsUserModifiable = ReadBool(reader, "bUserModifiable");
                    result.IsMember = ReadBool(reader, "bIsMember");
                    results.Add(result);
                }
            }

            return results;
        }

        private List<Collection> ExtractData(DbCommand cmd)
        {
            List<Collection> results = new List<Collection>();
            using(DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Collection result = new Collection();
                    result.ID = ReadInt(reader, "iID");
                    result.LibraryID = ReadInt(reader, "iLibraryID");
                    result.ParentCollectionID = ReadInt(reader, "iParentCollectionID");
                    result.Name = ReadString(reader, "sName");
                    result.Description = ReadString(reader, "sDescription");
                    result.IsUserModifiable = ReadBool(reader, "bUserModifiable");
                    results.Add(result);
                }
            }

            return results;
        }
    }
}
