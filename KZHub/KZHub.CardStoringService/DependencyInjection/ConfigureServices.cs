using KZHub.CardStoringService.AsyncDataServices;
using KZHub.CardStoringService.Data;
using KZHub.CardStoringService.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KZHub.CardStoringService.DependencyInjection
{
    public static class ConfigureServices
    {
        public static void AddServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            if (builder.Environment.IsDevelopment())
            {
                Console.WriteLine("--> Using InMemory Database");
                services.AddDbContext<DataContext>(options =>
                {
                    options.UseInMemoryDatabase("InMem");
                });
            }
            else
            {
                Console.WriteLine($"--> Using SQL Server Database: {builder.Configuration.GetConnectionString("CardsDB")}");
                services.AddDbContext<DataContext>(options =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("CardsDB"));
                });
            }

            services.AddScoped<ICardDataService, CardDataService>();
            services.AddScoped<ISaveCardProcessor, SaveCardProcessor>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddHostedService<MessageBusSubscriber>();
        }
    }
}
