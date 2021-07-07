using Microsoft.Extensions.DependencyInjection;
using SabinoLabs.Domain.Services;
using SabinoLabs.Domain.Services.Interfaces;
using Scrutor;

namespace SabinoLabs.Configuration
{
    public static class ServiceStartup
    {
        public static IServiceCollection AddServiceModule(this IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(ServicesInterfacesAssemblyHelper), typeof(ServicesClassesAssemblyHelper))
                .AddClasses(classes => classes.InNamespaces(ServicesClassesAssemblyHelper.Namespace))
                .UsingRegistrationStrategy(RegistrationStrategy.Replace(ReplacementBehavior.ServiceType))
                .AsMatchingInterface()
                .WithScopedLifetime()

                .AddClasses(classes => classes.Where(type =>
                    type.Namespace.Equals(ServicesClassesAssemblyHelper.Namespace) &&
                    type.Name.EndsWith("ExtendedService")))
                .UsingRegistrationStrategy(RegistrationStrategy.Replace(ReplacementBehavior.ServiceType))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            );
            return services;
        }
    }
}
