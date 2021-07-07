using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SabinoLabs.Infrastructure.Configuration;

namespace SabinoLabs.Configuration
{
    public static class AppSettingsConfiguration
    {
        public static IServiceCollection AddAppSettingsModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SecuritySettings>(options => configuration.GetSection("security").Bind(options));

            return services;
        }
    }
}
