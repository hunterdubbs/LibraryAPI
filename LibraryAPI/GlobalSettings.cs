using System.Data.Common;

namespace LibraryAPI
{
    public static class GlobalSettings
    {
        public static string ConnectionString { get; set; }
        public static DbProviderFactory DbProviderFactory { get; set; }
        public const string BASE_URL = "https://localhost:44361";
        //public const string BASE_URL = "https://library.basedpenguin.com";
    }
}
