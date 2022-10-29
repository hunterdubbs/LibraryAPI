using LibraryAPI.Domain.Enum;
using System;

namespace LibraryAPI.Domain
{
    public class Invite
    {
        public int ID { get; set; }
        public int LibraryID { get; set; }
        public string LibraryName { get; set; }
        public string InviterID { get; set; }
        public string InviterUsername { get; set; }
        public string RecipientID { get; set; }
        public string RecipientUsername { get; set; }
        public PermissionType PermissionLevel { get; set; }
        public DateTime Sent { get; set; }
    }
}
