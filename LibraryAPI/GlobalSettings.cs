using System.Data.Common;

namespace LibraryAPI
{
    public static class GlobalSettings
    {
        public static string ConnectionString { get; set; }
        public static DbProviderFactory DbProviderFactory { get; set; }
    }
}
