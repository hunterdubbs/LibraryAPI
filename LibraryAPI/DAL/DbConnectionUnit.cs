using System.Data.Common;

namespace LibraryAPI.DAL
{
    public class DbConnectionUnit
    {
        public DbConnection Connection { get; set; }
        public DbTransaction Transaction { get; set; }
    }
}
