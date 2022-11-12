using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Domain.Requests
{
    public class DeleteAccountRequest
    {
        [Required]
        [MinLength(6)]
        [MaxLength(40)]
        public string Password { get; set; }
    }
}
