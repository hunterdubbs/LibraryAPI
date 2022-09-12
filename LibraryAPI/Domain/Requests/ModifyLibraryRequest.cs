using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain.Requests
{
    public class ModifyLibraryRequest
    {
        [Required]
        public int LibraryID { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
