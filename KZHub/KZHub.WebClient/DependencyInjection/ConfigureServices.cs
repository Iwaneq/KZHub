using Blazored.Toast;
using KZHub.WebClient.Areas.Identity;
using KZHub.WebClient.AsyncDataServices;
using KZHub.WebClient.Data;
using KZHub.WebClient.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KZHub.WebClient.DependencyInjection
{
    public static class ConfigureServices
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ICardGenerationServiceClient, CardGenerationServiceClient>();
            services.AddSingleton<ICardStorageServiceClient, CardStorageServiceClient>();
            services.AddSingleton<CardState>();
            services.AddBlazoredToast();

            var connectionString = configuration.GetConnectionString("DataContextConnection") ?? throw new InvalidOperationException("Connection string 'DataContextConnection' not found.");

            services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<DataContext>();
            services.AddScoped<TokenProvider>();
        }
    }
}
