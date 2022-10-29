using LibraryAPI.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain.Requests
{
    public class CreateInviteRequest
    {
        [Required]
        public int LibraryID { get; set; }

        [Required]
        public string RecipientID { get; set; }

        [Required]
        public PermissionType PermissionType { get; set; }
    }
}
