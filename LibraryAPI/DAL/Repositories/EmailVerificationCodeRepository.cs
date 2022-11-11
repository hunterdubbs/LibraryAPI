using LibraryAPI.Domain;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace LibraryAPI.DAL.Repositories
{
    public class EmailVerificationCodeRepository : RepositoryBase
    {
        public EmailVerificationCodeRepository() : base() { }

        public EmailVerificationCodeRepository(UnitOfWork unitOfWork) : base(unitOfWork) { }

        public void Add(EmailVerificationCode code)
        {
            DbCommand cmd = CreateCommand(@"INSERT INTO tEmailVerificationCodes(sUserID, sCode, dtSent, bVerified) VALUES (@sUserID, @sCode, @dtSent, @bVerified)");
            cmd.Parameters.Add(CreateParameter("@sUserID", code.UserID));
            cmd.Parameters.Add(CreateParameter("@sCode", code.Code));
            cmd.Parameters.Add(CreateParameter("@dtSent", code.Sent));
            cmd.Parameters.Add(CreateParameter("@bVerified", code.IsVerified));
            cmd.ExecuteNonQuery();
            code.ID = (int)((MySqlConnector.MySqlCommand)cmd).LastInsertedId;
        }

        public void Update(EmailVerificationCode code)
        {
            DbCommand cmd = CreateCommand(@"UPDATE tEmailVerificationCodes SET sUserID=@sUserID, sCode=@sCode, dtSent=@dtSent, bVerified=@bVerified WHERE iID=@iID");
            cmd.Parameters.Add(CreateParameter("@sUserID", code.UserID));
            cmd.Parameters.Add(CreateParameter("@sCode", code.Code));
            cmd.Parameters.Add(CreateParameter("@dtSent", code.Sent));
            cmd.Parameters.Add(CreateParameter("@bVerified", code.IsVerified));
            cmd.Parameters.Add(CreateParameter("@iID", code.ID));
            cmd.ExecuteNonQuery();
        }

        public void DeleteByUserID(string userID)
        {
            DbCommand cmd = CreateCommand(@"DELETE FROM tEmailVerificationCodes WHERE sUserID=@sUserID");
            cmd.Parameters.Add(CreateParameter("@sUserID", userID));
            cmd.ExecuteNonQuery();
        }

        public EmailVerificationCode GetByUserID(string userID)
        {
            DbCommand cmd = CreateCommand(@"SELECT * FROM tEmailVerificationCodes WHERE sUserID=@sUserID");
            cmd.Parameters.Add(CreateParameter("@sUserID", userID));
            return ExtractData(cmd).FirstOrDefault();
        }

        private List<EmailVerificationCode> ExtractData(DbCommand cmd)
        {
            List<EmailVerificationCode> results = new List<EmailVerificationCode>();
            using(DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    EmailVerificationCode result = new EmailVerificationCode();
                    result.ID = ReadInt(reader, "iID");
                    result.UserID = ReadString(reader, "sUserID");
                    result.Code = ReadString(reader, "sCode");
                    result.Sent = ReadDateTime(reader, "dtSent");
                    result.IsVerified = ReadBool(reader, "bVerified");
                    results.Add(result);
                }
            }
            return results;
        }
    }
}
