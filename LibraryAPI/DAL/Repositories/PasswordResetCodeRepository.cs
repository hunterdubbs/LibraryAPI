using LibraryAPI.Domain;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace LibraryAPI.DAL.Repositories
{
    public class PasswordResetCodeRepository : RepositoryBase
    {
        public PasswordResetCodeRepository() : base() { }

        public PasswordResetCodeRepository(UnitOfWork unitOfWork) : base(unitOfWork) { }

        public void Add(PasswordResetCode code)
        {
            DbCommand cmd = CreateCommand(@"INSERT INTO tPasswordResetCodes(sUserID, sHash, sSalt, dtExpires) VALUES (@sUserID, @sHash, @sSalt, @dtExpires) RETURNING iID");
            cmd.Parameters.Add(CreateParameter("@sUserID", code.UserID));
            cmd.Parameters.Add(CreateParameter("@sHash", code.Hash));
            cmd.Parameters.Add(CreateParameter("@sSalt", code.Salt));
            cmd.Parameters.Add(CreateParameter("@dtExpires", code.Expires));
            code.ID = (int)cmd.ExecuteScalar();
        }

        public void Delete(int id)
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tPasswordResetCodes WHERE iID=@iID");
            cmd.Parameters.Add(CreateParameter("@iID", id));
            cmd.ExecuteNonQuery();
        }

        public void DeleteAllExpired()
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tPasswordResetCodes WHERE dtExpires<@dtExpires");
            cmd.Parameters.Add(CreateParameter("@dtExpires", DateTime.Now));
            cmd.ExecuteNonQuery();
        }

        public void DeleteByUserID(string userID)
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tPasswordResetCodes WHERE sUserID=@sUserID");
            cmd.Parameters.Add(CreateParameter("@sUserID", userID));
            cmd.ExecuteNonQuery();
        }

        public List<PasswordResetCode> GetAllByUserID(string userID)
        {
            DbCommand cmd = CreateCommand(@"SELECT * FROM tPasswordResetCodes WHERE sUserID=@sUserID AND dtExpires>@dtExpires");
            cmd.Parameters.Add(CreateParameter("@sUserID", userID));
            cmd.Parameters.Add(CreateParameter("@dtExpires", DateTime.Now));
            return ExtractData(cmd);
        }

        private List<PasswordResetCode> ExtractData(DbCommand cmd)
        {
            List<PasswordResetCode> results = new List<PasswordResetCode>();
            using(DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    PasswordResetCode result = new PasswordResetCode();
                    result.ID = ReadInt(reader, "iID");
                    result.UserID = ReadString(reader, "sUserID");
                    result.Hash = ReadString(reader, "sHash");
                    result.Salt = ReadString(reader, "sSalt");
                    result.Expires = ReadDateTime(reader, "dtExpires");
                    results.Add(result);
                }
            }
            return results;
        }
    }
}
