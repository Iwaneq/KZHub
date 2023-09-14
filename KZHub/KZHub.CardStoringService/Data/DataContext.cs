using Microsoft.EntityFrameworkCore;

namespace KZHub.CardStoringService.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
            
        }
    }
}
