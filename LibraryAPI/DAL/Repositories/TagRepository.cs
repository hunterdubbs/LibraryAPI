using LibraryAPI.Domain;
using System.Collections.Generic;
using System.Data.Common;

namespace LibraryAPI.DAL.Repositories
{
    public class TagRepository : RepositoryBase
    {
        public TagRepository() : base() { }

        public TagRepository(UnitOfWork unitOfWork) : base(unitOfWork) { }

        public void Add(Tag tag)
        {
            DbCommand cmd = CreateCommand(@"INSERT INTO tTag(iLibraryID, sName) VALUES (@iLibraryID, @sName) RETURNING iID");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", tag.LibraryID));
            cmd.Parameters.Add(CreateParameter("@sName", tag.Name));
            tag.ID = (int)cmd.ExecuteScalar();
        }

        public void DeleteAllByLibraryID(int libraryID)
        {
            DbCommand xrefCmd = CreateCommand(@"DELETE FROM tBookTagXREF WHERE iTagID IN (SELECT iID FROM tTag WHERE iLibraryID=@iLibraryID)");
            xrefCmd.Parameters.Add(CreateParameter("@iLibraryID", libraryID));
            xrefCmd.ExecuteNonQuery();

            DbCommand cmd = CreateCommand(@"DELETE FROM tTag WHERE iLibraryID=@iLibraryID");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", libraryID));
            cmd.ExecuteNonQuery();
        }

        public List<Tag> GetByLibraryID(int libraryID)
        {
            DbCommand cmd = CreateCommand(@"SELECT * FROM tTag WHERE iLibraryID=@iLibraryID");
            cmd.Parameters.Add(CreateParameter("@iLibraryID", libraryID));
            return ExtractData(cmd);
        }

        private List<Tag> ExtractData(DbCommand cmd)
        {
            List<Tag> results = new List<Tag>();

            using(DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Tag result = new Tag();
                    result.ID = ReadInt(reader, "iID");
                    result.LibraryID = ReadInt(reader, "iLibraryID");
                    result.Name = ReadString(reader, "sName");
                    results.Add(result);
                }
            }

            return results;
        }

    }
}
