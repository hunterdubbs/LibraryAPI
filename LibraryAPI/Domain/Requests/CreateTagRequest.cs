using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Domain.Requests
{
    public class CreateTagRequest
    {
        [Required]
        public int LibraryID { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }
    }
}
