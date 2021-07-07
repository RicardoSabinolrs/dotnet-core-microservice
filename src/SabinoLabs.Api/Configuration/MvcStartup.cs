using JHipsterNet.Web.Pagination.Binders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace SabinoLabs.Configuration
{
    public static class WebConfiguration
    {
        public static IServiceCollection AddWebModule(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddMvc();
            services.AddHealthChecks();
            services.AddControllers(
                    options => { options.ModelBinderProviders.Insert(0, new PageableBinderProvider()); })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.Formatting = Formatting.Indented;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                });

            return services;
        }

        public static IApplicationBuilder UseApplicationWeb(this IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseHealthChecks("/health");

            return app;
        }
    }
}
