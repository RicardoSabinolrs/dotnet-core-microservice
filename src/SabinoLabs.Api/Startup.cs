using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SabinoLabs.Configuration;
using SabinoLabs.Configuration.Consul;
using SabinoLabs.Infrastructure.Configuration;
using SabinoLabs.Infrastructure.Data;

[assembly: ApiController]

namespace SabinoLabs
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        private IConfiguration Configuration { get; }

        public IHostEnvironment Environment { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddAppSettingsModule(Configuration);

            AddDatabase(services);

            services
                .AddSecurityModule()
                .AddProblemDetailsModule(Environment)
                .AddAutoMapperModule()
                .AddSwaggerModule()
                .AddWebModule()
                .AddConsul(Configuration)
                .AddRepositoryModule()
                .AddServiceModule();
        }

        public virtual void Configure(IApplicationBuilder app, IHostEnvironment env, IServiceProvider serviceProvider,
            ApplicationDatabaseContext context, IOptions<SecuritySettings> securitySettingsOptions)
        {
            SecuritySettings securitySettings = securitySettingsOptions.Value;
            app
                .UseApplicationSecurity(securitySettings)
                .UseApplicationProblemDetails()
                .UseApplicationSwagger()
                .UseApplicationWeb(env)
                .UseConsul(Configuration)
                .UseApplicationDatabase(serviceProvider, env);
        }

        protected virtual void AddDatabase(IServiceCollection services) => services.AddDatabaseModule(Configuration);
    }
}
