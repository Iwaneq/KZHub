using KZHub.WebClient.AsyncDataServices;

namespace KZHub.WebClient.DependencyInjection
{
    public static class ConfigureServices
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IMessageBusClient, MessageBusClient>();
        }
    }
}
