using System;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SabinoLabs.Configuration.Consul
{
    public static class ConsulStartup
    {
        public static IServiceCollection AddConsul(this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            ConsulOptions consulConfigOptions = configuration.GetOptions<ConsulOptions>("Consul");

            serviceCollection.Configure<ConsulOptions>(configuration.GetSection("Consul"));

            return serviceCollection.AddSingleton<IConsulClient>(c => new ConsulClient(cfg =>
            {
                if (!string.IsNullOrEmpty(consulConfigOptions.Host))
                {
                    cfg.Address = new Uri(consulConfigOptions.Host);
                }
            }));
        }

        public static IApplicationBuilder UseConsul(this IApplicationBuilder app, IConfiguration configuration)
        {
            using (IServiceScope scope = app.ApplicationServices.CreateScope())
            {
                ConsulOptions config = configuration.GetOptions<ConsulOptions>("Consul");

                if (!config.Enabled)
                {
                    return app;
                }

                Guid serviceId = Guid.NewGuid();
                string consulServiceId = $"{config.Service}:{serviceId}";

                IConsulClient? client = scope.ServiceProvider.GetService<IConsulClient>();

                AgentServiceRegistration consulServiceRistration = new AgentServiceRegistration
                {
                    Name = config.Service, ID = consulServiceId, Address = config.Address, Port = config.Port
                };

                if (config.PingEnabled)
                {
                    HealthCheckService? healthService = scope.ServiceProvider.GetService<HealthCheckService>();

                    if (healthService != null)
                    {
                        string scheme = config.Address.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)
                            ? string.Empty
                            : "http://";
                        AgentServiceCheck check = new AgentServiceCheck
                        {
                            Interval = TimeSpan.FromSeconds(5),
                            DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(10),
                            HTTP =
                                $"{scheme}{config.Address}{(config.Port > 0 ? $":{config.Port}" : string.Empty)}/health"
                        };

                        consulServiceRistration.Checks = new[] {check};
                    }
                    else
                    {
                        throw new ConsulConfigurationException(
                            "Please ensure that Healthchecks has been added before adding checks to Consul.");
                    }
                }

                client.Agent.ServiceRegister(consulServiceRistration);

                return app;
            }
        }
    }
}
