namespace LibraryAPI.Domain.Responses
{
    public class LibraryResponse : Library
    {
        public Library Library { get; set; }
        public int BookCount { get; set; }
    }
}
