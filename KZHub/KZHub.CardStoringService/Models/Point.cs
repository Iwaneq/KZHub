using System.ComponentModel.DataAnnotations;

namespace KZHub.CardStoringService.Models
{
    public class Point
    {
        [Key]
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string? Title { get; set; }
        public string? ZastepMember { get; set; }
    }
}
