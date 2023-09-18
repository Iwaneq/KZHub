using KZHub.CardStoringService.Models;
using Microsoft.EntityFrameworkCore;

namespace KZHub.CardStoringService.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
            
        }

        public DbSet<Card> Cards { get; set; }
    }
}
