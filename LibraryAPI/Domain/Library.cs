using LibraryAPI.Domain.Enum;
using System;

namespace LibraryAPI.Domain
{
    public class Library
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public DateTime CreatedDate { get; set; }
        public PermissionType Permissions { get; set; }
        public int DefaultCollectionID { get; set; }
    }
}
