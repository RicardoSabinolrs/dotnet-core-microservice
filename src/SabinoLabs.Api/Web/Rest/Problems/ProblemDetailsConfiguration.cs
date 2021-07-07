#nullable enable
using System;
using System.Diagnostics;
using System.Security.Authentication;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace SabinoLabs.Web.Rest.Problems
{
    public class ProblemDetailsConfiguration : IConfigureOptions<ProblemDetailsOptions>
    {
        public ProblemDetailsConfiguration(IHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _HttpContextAccessor = httpContextAccessor;
        }

        private IHostEnvironment _environment { get; }
        private IHttpContextAccessor _HttpContextAccessor { get; }

        public void Configure(ProblemDetailsOptions options)
        {
            options.IncludeExceptionDetails = (ctx, details) => _environment.IsDevelopment();

            options.OnBeforeWriteDetails = (ctx, details) =>
            {
                string? traceId = Activity.Current?.Id ?? _HttpContextAccessor.HttpContext.TraceIdentifier;
                details.Extensions["traceId"] = traceId;
            };

            options.MapToStatusCode<AuthenticationException>(StatusCodes.Status401Unauthorized);
            options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);
            options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
        }
    }
}
