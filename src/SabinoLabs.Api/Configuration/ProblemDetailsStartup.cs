using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SabinoLabs.Crosscutting.Exceptions;
using SabinoLabs.Web.Rest.Problems;

namespace SabinoLabs.Configuration
{
    public static class ProblemDetailsStartup
    {
        public static IServiceCollection AddProblemDetailsModule(this IServiceCollection services,
            IHostEnvironment environment)
        {
            services.AddProblemDetails(setup =>
            {
                setup.IncludeExceptionDetails = (_context, _exception) => environment.IsDevelopment();

                setup.Map<BadRequestAlertException>(exception => new ProblemDetails
                {
                    Type = exception.Type,
                    Detail = exception.Detail,
                    Status = StatusCodes.Status400BadRequest,
                    Extensions = {["params"] = exception.EntityName, ["message"] = $"error.{exception.ErrorKey}"}
                });

                setup.Map<InternalServerErrorException>(exception => new ProblemDetails
                {
                    Type = exception.Type,
                    Detail = exception.Detail,
                    Status = StatusCodes.Status500InternalServerError
                });

                setup.Map<BaseException>(exception => new ProblemDetails
                {
                    Type = exception.Type, Detail = exception.Detail, Status = StatusCodes.Status400BadRequest
                });
            });

            services.ConfigureOptions<ProblemDetailsConfiguration>();

            return services;
        }

        public static IApplicationBuilder UseApplicationProblemDetails(this IApplicationBuilder app)
        {
            app.UseProblemDetails();
            return app;
        }
    }
}
