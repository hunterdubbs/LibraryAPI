using System;
using System.Data.Common;

namespace LibraryAPI.DAL
{
    public class RepositoryBase
    {
        private bool isAmbientConnection = false;
        protected UnitOfWork unitOfWork;

        public RepositoryBase()
        {
            isAmbientConnection = true;
        }

        public RepositoryBase(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        protected DbConnectionUnit Connection => isAmbientConnection ? AmbientConnection.Get() : unitOfWork.ConnectionUnit;

        protected DbCommand CreateCommand(string cmdText)
        {
            DbCommand cmd = GlobalSettings.DbProviderFactory.CreateCommand();
            cmd.CommandText = cmdText;
            cmd.Connection = Connection.Connection;
            cmd.Transaction = Connection.Transaction;
            return cmd;
        }

        protected DbParameter CreateParameter(string key, object value)
        {
            DbParameter param = GlobalSettings.DbProviderFactory.CreateParameter();
            param.ParameterName = key;
            param.Value = value;
            return param;
        }

        protected string ReadString(DbDataReader reader, string columnName, string defaultValue = "")
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? defaultValue : reader.GetString(ordinal);
        }

        protected int ReadInt(DbDataReader reader, string columnName, int defaultValue = 0)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? defaultValue : reader.GetInt32(ordinal);
        }

        protected DateTime ReadDateTime(DbDataReader reader, string columnName, DateTime defaultValue = default(DateTime))
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? defaultValue : reader.GetDateTime(ordinal);
        }

        protected bool ReadBool(DbDataReader reader, string columnName, bool defaultvalue = false)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? defaultvalue : reader.GetBoolean(ordinal);
        }
    }
}
