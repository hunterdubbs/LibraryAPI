using LibraryAPI.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain
{
    public class LibraryPermission
    {
        public string UserID { get; set; }
        public string Username { get; set; }
        public PermissionType PermissionLevel { get; set; }
        public bool IsInvite { get; set; }
    }
}
