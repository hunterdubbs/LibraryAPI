using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain
{
    public class CollectionMembership : Collection
    {
        public bool IsMember { get; set; }
    }
}
