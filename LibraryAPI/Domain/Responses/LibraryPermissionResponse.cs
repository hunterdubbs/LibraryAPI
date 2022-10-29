using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain.Responses
{
    public class LibraryPermissionResponse
    {
        public List<LibraryPermission> Permissions { get; set; } = new List<LibraryPermission>();
        public List<Invite> Invites { get; set; } = new List<Invite>();
    }
}
