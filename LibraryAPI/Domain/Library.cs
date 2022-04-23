using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain
{
    public class Library
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
