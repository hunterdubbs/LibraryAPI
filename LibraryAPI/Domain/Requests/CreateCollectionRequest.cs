using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain.Requests
{
    public class CreateCollectionRequest
    {
        [Required]
        public int LibraryID { get; set; }

        [Required]
        public int ParentCollectionID { get; set; }

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
