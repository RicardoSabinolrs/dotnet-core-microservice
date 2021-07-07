using System.Security.Claims;

namespace SabinoLabs.Test.Configuration
{
    public class MockClaimsPrincipalProvider
    {
        public MockClaimsPrincipalProvider(ClaimsPrincipal user) => User = user;

        public ClaimsPrincipal User { get; }
    }
}
