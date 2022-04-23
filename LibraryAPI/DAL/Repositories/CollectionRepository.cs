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
            DbCommand cmd = CreateCommand(@"SELECT * FROM tCollection");
            return ExtractData(cmd);
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
