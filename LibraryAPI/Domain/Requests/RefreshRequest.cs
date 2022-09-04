using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain.Requests
{
    public class RefreshRequest
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
