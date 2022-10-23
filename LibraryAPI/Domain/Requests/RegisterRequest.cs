using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain.Requests
{
    public class RegisterRequest
    {
        [MaxLength(255)]
        [Required]
        public string Username { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        [Required]
        public string Email { get; set; }

        [MinLength(6)]
        [MaxLength(40)]
        public string Password { get; set; }
    }
}
