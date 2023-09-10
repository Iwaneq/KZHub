namespace KZHub.CardGenerationService.DTOs.Card
{
    public class CreateCardDTO
    {
        public string Zastep { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Place { get; set; } = string.Empty;
        public List<CreatePointDTO> Points { get; set; } = new List<CreatePointDTO>();
        public string? RequiredItems { get; set; }
    }
}