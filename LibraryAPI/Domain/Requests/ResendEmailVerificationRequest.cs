using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Domain.Requests
{
    public class ResendEmailVerificationRequest
    {
        [Required]
        public string Email { get; set; }
    }
}
