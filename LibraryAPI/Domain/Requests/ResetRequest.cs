using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Domain.Requests
{
    public class ResetRequest
    {
        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(40)]
        public string Password { get; set; }

        [Required]
        [MaxLength(10)]
        public string ResetCode { get; set; }
    }
}
