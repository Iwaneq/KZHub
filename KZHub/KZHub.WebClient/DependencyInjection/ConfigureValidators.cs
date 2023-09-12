using KZHub.WebClient.Validation;

namespace KZHub.WebClient.DependencyInjection
{
    public static class ConfigureValidators
    {
        public static void AddValidators(this IServiceCollection services)
        {
            services.AddScoped<CreateCardDTOValidator>();
        }
    }
}
