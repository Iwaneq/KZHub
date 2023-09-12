using System.ComponentModel.DataAnnotations;

namespace KZHub.WebClient.DTOs.Card
{
    public class CreatePointDTO
    {
        [Required]
        public DateTime Time { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? ZastepMember { get; set; }
    }
}
