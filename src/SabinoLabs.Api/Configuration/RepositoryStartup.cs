using Microsoft.Extensions.DependencyInjection;
using SabinoLabs.Domain.Repositories.Interfaces;
using SabinoLabs.Infrastructure.Data.Repositories;
using Scrutor;

namespace SabinoLabs.Configuration
{
    public static class RepositoryStartup
    {
        public static IServiceCollection AddRepositoryModule(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.Scan(scan => scan
                .FromAssembliesOf(typeof(IUnitOfWork), typeof(UnitOfWork))
                .AddClasses(classes => classes.InNamespaces("SabinoLabs.Api.Infrastructure.Data.Repositories"))
                .UsingRegistrationStrategy(RegistrationStrategy.Replace(ReplacementBehavior.ServiceType))
                .AsMatchingInterface()
                .WithScopedLifetime()
                .AddClasses(classes => classes.Where(type =>
                    type.Namespace.Equals("SabinoLabs.Api.Infrastructure.Data.Repositories") &&
                    type.Name.EndsWith("ExtendedRepository")))
                .UsingRegistrationStrategy(RegistrationStrategy.Replace(ReplacementBehavior.ServiceType))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            );

            return services;
        }
    }
}
