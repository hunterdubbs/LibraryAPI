using System;
using System.Data.Common;

namespace LibraryAPI.DAL
{
    public class UnitOfWork : IDisposable
    {
        public DbConnectionUnit ConnectionUnit = new DbConnectionUnit();

        public DbConnection Connection => ConnectionUnit.Connection;
        public DbTransaction Transaction => ConnectionUnit.Transaction;

        public UnitOfWork()
        {
            DbConnection connection = CreateConnection();
            SetupAmbientConnection(connection);
        }

        private static DbConnection CreateConnection()
        {

            DbConnection connection = GlobalSettings.DbProviderFactory.CreateConnection();
            connection.ConnectionString = GlobalSettings.ConnectionString;
            return connection;
        }

        public void Begin()
        {
            if (ConnectionUnit.Transaction != null) throw new InvalidOperationException("Tranaction already open");
            ConnectionUnit.Transaction = ConnectionUnit.Connection.BeginTransaction();
        }

        public void Commit()
        {
            ConnectionUnit.Transaction.Commit();
            ConnectionUnit.Transaction = null;
        }

        public void Rollback()
        {
            ConnectionUnit.Transaction.Rollback();
            ConnectionUnit.Transaction = null;
        }

        public void Dispose()
        {
            if (ConnectionUnit.Transaction != null) Rollback();
            ConnectionUnit.Connection.Close();
            ConnectionUnit.Connection = null;
            AmbientConnection.Release();
        }

        private void SetupAmbientConnection(DbConnection connection)
        {
            if (AmbientConnection.IsActive) throw new InvalidOperationException("Ambient connection already open");

            connection.Open();
            ConnectionUnit.Connection = connection;
            AmbientConnection.Create(ConnectionUnit);
        }
    }
}
