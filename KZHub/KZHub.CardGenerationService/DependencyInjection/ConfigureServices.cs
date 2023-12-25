using KZHub.CardGenerationService.AsyncDataServices;
using KZHub.CardGenerationService.Services.CardProcessing;
using KZHub.CardGenerationService.Services.CardProcessing.Interfaces;

namespace KZHub.CardGenerationService.DependencyInjection
{
    public static class ConfigureServices
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<ICardGenerator, CardGenerator>();
            services.AddScoped<ICardGenerationProcessor, CardGenerationProcessor>();
            services.AddHostedService<MessageBusSubscriber>();
        }
    }
}
