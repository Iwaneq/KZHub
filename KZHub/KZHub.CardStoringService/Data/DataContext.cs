using KZHub.CardStoringService.Models;
using Microsoft.EntityFrameworkCore;

namespace KZHub.CardStoringService.Data
{
    public class DataContext : DbContext
    {
        public DataContext() {}
        public DataContext(DbContextOptions<DataContext> options) : base(options) {}

        public DbSet<Card> Cards { get; set; }
    }
}
