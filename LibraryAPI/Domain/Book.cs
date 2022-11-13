using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain
{
    public class Book
    {
        public int ID { get; set; }
        public int LibraryID { get; set; }
        public string Title { get; set; }
        public string Synopsis { get; set; }
        public List<Author> Authors { get; set; }
        public List<Tag> Tags { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DatePublished { get; set; }
    }
}
