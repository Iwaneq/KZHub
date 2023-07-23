namespace KZHub.WebClient.Data.Card
{
    public class Card
    {
        public string? Zastep { get; set; }
        public DateTime Date { get; set; }
        public string? Place { get; set; }
        public List<Point> Points { get; set; } = new List<Point>();
        public List<string> RequiredItems { get; set; } = new List<string>();
    }
}
