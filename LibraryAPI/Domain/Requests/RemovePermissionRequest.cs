using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Domain.Requests
{
    public class RemovePermissionRequest
    {
        [Required]
        public int LibraryID { get; set; }

        [Required]
        public string UserID { get; set; }
    }
}
