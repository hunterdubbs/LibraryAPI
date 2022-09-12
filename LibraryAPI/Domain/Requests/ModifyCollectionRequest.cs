using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain.Requests
{
    public class ModifyCollectionRequest
    {
        [Required]
        public int CollectionID { get; set; }

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
