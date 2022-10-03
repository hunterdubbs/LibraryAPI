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

        public void DeleteByLibraryID(int libraryID)
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tCollection WHERE iLibraryID=@iLibraryID");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", libraryID));
            cmd.ExecuteNonQuery();
        }

        public void Delete(int collectionID)
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tCollection WHERE iID=@iID");
            cmd.Parameters.Add(CreateParameter("@iID", collectionID));
            cmd.ExecuteNonQuery();
        }

        public void Add(Collection collection)
        {
            DbCommand cmd = CreateCommand(@"INSERT INTO tCollection(iLibraryID, iParentCollectionID, sName, sDescription) VALUES (@iLibraryID, @iParentCollectionID, @sName, @sDescription)");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", collection.LibraryID));
            cmd.Parameters.Add(CreateParameter("@iParentCollectionID", collection.ParentCollectionID));
            cmd.Parameters.Add(CreateParameter("@sName", collection.Name));
            cmd.Parameters.Add(CreateParameter("@sDescription", collection.Description));
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
            DbCommand cmd = CreateCommand("@INSERT INTO tCollectionBookXREF(iCollectionID, iBookID) VALUES (@iCollectionID, @iBookID)");
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
                    results.Add(result);
                }
            }

            return results;
        }
    }
}
