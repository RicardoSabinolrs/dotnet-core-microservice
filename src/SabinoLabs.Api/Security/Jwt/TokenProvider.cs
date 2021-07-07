using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SabinoLabs.Infrastructure.Configuration;

namespace SabinoLabs.Security.Jwt
{
    public interface ITokenProvider
    {
        string CreateToken(IPrincipal principal, bool rememberMe);
        ClaimsPrincipal TransformPrincipal(ClaimsPrincipal principal);
    }

    public class TokenProvider : ITokenProvider
    {
        private const string AuthoritiesKey = "auth";

        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

        private readonly ILogger<TokenProvider> _log;

        private readonly SecuritySettings _securitySettings;

        private SigningCredentials _key;

        private long _tokenValidityInSeconds;

        private long _tokenValidityInSecondsForRememberMe;


        public TokenProvider(ILogger<TokenProvider> log, IOptions<SecuritySettings> securitySettings)
        {
            _log = log;
            _securitySettings = securitySettings.Value;
            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            Init();
        }

        public string CreateToken(IPrincipal principal, bool rememberMe)
        {
            ClaimsIdentity subject = CreateSubject(principal);
            DateTime validity =
                DateTime.UtcNow.AddSeconds(rememberMe
                    ? _tokenValidityInSecondsForRememberMe
                    : _tokenValidityInSeconds);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject, Expires = validity, SigningCredentials = _key
            };

            SecurityToken token = _jwtSecurityTokenHandler.CreateToken(tokenDescriptor);
            return _jwtSecurityTokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal TransformPrincipal(ClaimsPrincipal principal)
        {
            ClaimsIdentity currentIdentity = (ClaimsIdentity)principal.Identity;
            List<Claim> roleClaims = principal
                .Claims
                .Filter(it => it.Type == AuthoritiesKey).First().Value
                .Split(",")
                .Map(role => new Claim(ClaimTypes.Role, role))
                .ToList();

            return new ClaimsPrincipal(
                new ClaimsIdentity(
                    principal.Claims.Union(roleClaims),
                    currentIdentity.AuthenticationType,
                    currentIdentity.NameClaimType,
                    currentIdentity.RoleClaimType
                )
            );
        }

        private void Init()
        {
            byte[] keyBytes;
            string secret = _securitySettings.Authentication.Jwt.Secret;

            if (!string.IsNullOrWhiteSpace(secret))
            {
                _log.LogWarning("Warning: the JWT key used is not Base64-encoded. " +
                                "We recommend using the `security.authentication.jwt.base64-secret` key for optimum security.");
                keyBytes = Encoding.ASCII.GetBytes(secret);
            }
            else
            {
                _log.LogDebug("Using a Base64-encoded JWT secret key");
                keyBytes = Convert.FromBase64String(_securitySettings.Authentication.Jwt.Base64Secret);
            }

            _key = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature);
            _tokenValidityInSeconds = _securitySettings.Authentication.Jwt.TokenValidityInSeconds;
            _tokenValidityInSecondsForRememberMe =
                _securitySettings.Authentication.Jwt.TokenValidityInSecondsForRememberMe;
        }

        private static ClaimsIdentity CreateSubject(IPrincipal principal)
        {
            string? username = principal.Identity.Name;
            IEnumerable<Claim> roles = GetRoles(principal);
            string authValue = string.Join(",", roles.Map(it => it.Value));
            return new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username), new Claim(AuthoritiesKey, authValue)
            });
        }

        private static IEnumerable<Claim> GetRoles(IPrincipal principal) =>
            principal is ClaimsPrincipal user
                ? user.FindAll(it => it.Type == ClaimTypes.Role)
                : Enumerable.Empty<Claim>();
    }
}
