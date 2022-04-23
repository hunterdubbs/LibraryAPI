using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain.Requests
{
    public class CreateLibraryRequest
    {
        [Required]
        public string Name { get; set; }
    }
}
