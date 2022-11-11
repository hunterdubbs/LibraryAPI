using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Domain.Requests
{
    public class VerifyEmailRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
