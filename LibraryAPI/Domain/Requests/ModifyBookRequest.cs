using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain.Requests
{
    public class ModifyBookRequest
    {
        [Required]
        public int BookID { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength(1023)]
        public string Synopsis { get; set; } = "";

        [Required]
        public int LibraryID { get; set; }

        public DateTime DatePublished { get; set; } = DateTime.MinValue;

        [Required]
        public List<Author> Authors { get; set; } = new List<Author>();

        [Required]
        public List<Tag> Tags { get; set; } = new List<Tag>();

        [MaxLength(80)]
        public string Series { get; set; } = "";

        [MaxLength(3)]
        public string Volume { get; set; } = "";
    }
}
