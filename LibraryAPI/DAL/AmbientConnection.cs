using System;

namespace LibraryAPI.DAL
{
    public class AmbientConnection
    {
        [ThreadStatic]
        private static DbConnectionUnit Connection;

        public static void Create(DbConnectionUnit connection)
        {
            Connection = connection;
        }

        public static DbConnectionUnit Get()
        {
            if (Connection == null || Connection.Connection == null) throw new InvalidOperationException("No connection availale");
            return Connection;
        }

        public static void Release()
        {
            Connection = null;
        }

        public static bool IsActive => Connection != null;
    }
}
