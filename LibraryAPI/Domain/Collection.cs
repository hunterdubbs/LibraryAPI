using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain
{
    public class Collection
    {
        public int ID { get; set; }
        public int LibraryID { get; set; }
        public int ParentCollectionID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Book> Books { get; set; }
        public bool IsUserModifiable { get; set; }
    }
}
