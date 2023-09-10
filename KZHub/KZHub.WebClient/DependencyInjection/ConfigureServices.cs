using KZHub.WebClient.AsyncDataServices;
using KZHub.WebClient.Services;

namespace KZHub.WebClient.DependencyInjection
{
    public static class ConfigureServices
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IMessageBusClient, MessageBusClient>();
            services.AddSingleton<CardState>();
        }
    }
}
