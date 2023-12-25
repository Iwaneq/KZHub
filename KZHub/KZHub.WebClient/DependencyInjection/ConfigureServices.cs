using Blazored.Toast;
using KZHub.WebClient.Areas.Identity;
using KZHub.WebClient.AsyncDataServices;
using KZHub.WebClient.Data;
using KZHub.WebClient.Services;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace KZHub.WebClient.DependencyInjection
{
    public static class ConfigureServices
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
        {
            services.AddSingleton<ICardGenerationServiceClient, CardGenerationServiceClient>();
            services.AddSingleton<ICardStorageServiceClient, CardStorageServiceClient>();
            services.AddSingleton<CardState>();
            services.AddBlazoredToast();

            if (isDevelopment)
            {
                services.AddDbContext<DataContext>(options => options.UseInMemoryDatabase("IdentityDB"));
            }
            else
            {
                var connectionString = configuration.GetConnectionString("DataContextConnection") ?? throw new InvalidOperationException("Connection string 'DataContextConnection' not found.");
                Console.WriteLine($"--> Configuring Identity Database Connection: {connectionString}");
                services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));
            }


            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<DataContext>();
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
            services.AddScoped<TokenProvider>();
        }
    }
}
