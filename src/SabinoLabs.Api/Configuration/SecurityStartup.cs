using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SabinoLabs.Infrastructure.Configuration;
using SabinoLabs.Security.Jwt;

namespace SabinoLabs.Configuration
{
    public static class SecurityStartup
    {
        public const string UserNameClaimType = JwtRegisteredClaimNames.Sub;

        public static IServiceCollection AddSecurityModule(this IServiceCollection services)
        {
            IOptions<SecuritySettings> opt = services.BuildServiceProvider()
                .GetRequiredService<IOptions<SecuritySettings>>();
            SecuritySettings securitySettings = opt.Value;
            byte[] keyBytes;
            string secret = securitySettings.Authentication.Jwt.Secret;

            if (!string.IsNullOrWhiteSpace(secret))
            {
                keyBytes = Encoding.ASCII.GetBytes(secret);
            }
            else
            {
                keyBytes = Convert.FromBase64String(securitySettings.Authentication.Jwt.Base64Secret);
            }

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                        ClockSkew = TimeSpan.Zero,
                        NameClaimType = UserNameClaimType
                    };
                });

            services.AddScoped<IClaimsTransformation, RoleClaimsTransformation>();
            services.AddScoped<ITokenProvider, TokenProvider>();
            return services;
        }

        public static IApplicationBuilder UseApplicationSecurity(this IApplicationBuilder app,
            SecuritySettings securitySettings)
        {
            app.UseCors(CorsPolicyBuilder(securitySettings.Cors));
            app.UseAuthentication();
            if (securitySettings.EnforceHttps)
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            return app;
        }

        private static Action<CorsPolicyBuilder> CorsPolicyBuilder(Cors config) =>
            builder =>
            {
                if (!config.AllowedOrigins.Equals("*"))
                {
                    if (config.AllowCredentials)
                    {
                        builder.AllowCredentials();
                    }
                    else
                    {
                        builder.DisallowCredentials();
                    }
                }

                builder.WithOrigins(config.AllowedOrigins)
                    .WithMethods(config.AllowedMethods)
                    .WithHeaders(config.AllowedHeaders)
                    .WithExposedHeaders(config.ExposedHeaders)
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(config.MaxAge));
            };
    }
}
