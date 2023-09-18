namespace KZHub.CardStoringService.Models
{
    public class Card
    {
        public string Zastep { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Place { get; set; } = string.Empty;
        public List<Point> Points { get; set; } = new List<Point>();
        public string? RequiredItems { get; set; }
    }
}
