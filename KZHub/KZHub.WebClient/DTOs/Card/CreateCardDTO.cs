using System.ComponentModel.DataAnnotations;

namespace KZHub.WebClient.DTOs.Card
{
    public class CreateCardDTO
    {
        [Required]
        public string Zastep { get; set; } = string.Empty;

        [Required]
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public string Place { get; set; } = string.Empty;
        public List<CreatePointDTO> Points { get; set; } = new List<CreatePointDTO>();
        public string? RequiredItems { get; set; }
    }
}