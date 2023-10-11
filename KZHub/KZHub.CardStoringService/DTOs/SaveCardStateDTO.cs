namespace KZHub.CardStoringService.DTOs
{
    public class SaveCardStateDTO
    {
        public int? CardId { get; set; }
        public bool IsSaved { get; set; }
        public string? Error { get; set; }
    }
}