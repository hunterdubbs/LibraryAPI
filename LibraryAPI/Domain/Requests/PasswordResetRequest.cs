using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Domain.Requests
{
    public class PasswordResetRequest
    {
        [Required]
        [MaxLength(255)]
        public string Email { get; set; }
    }
}
