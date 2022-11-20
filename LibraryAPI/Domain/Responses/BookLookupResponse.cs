using System;
using System.Collections.Generic;

namespace LibraryAPI.Domain.Responses
{
    public class BookLookupResponse
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Published { get; set; }
        public List<Author> Authors { get; set; }
    }
}
