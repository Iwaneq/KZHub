namespace KZHub.WebClient.DTOs.Card
{
    public class CreateCardDTO
    {
        public string? Zastep { get; set; }
        public DateTime Date { get; set; }
        public string? Place { get; set; }
        public List<CreatePointDTO> Points { get; set; } = new List<CreatePointDTO>();
        public List<string> RequiredItems { get; set; } = new List<string>();
    }
}