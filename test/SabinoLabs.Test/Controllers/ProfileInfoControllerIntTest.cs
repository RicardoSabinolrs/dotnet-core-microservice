using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using SabinoLabs.Test.Setup;
using Xunit;

namespace SabinoLabs.Test.Controllers
{
    public class ProfileInfoControllerIntTest
    {
        private readonly AppWebApplicationFactory<TestStartup> _factory;

        public ProfileInfoControllerIntTest() => _factory = new AppWebApplicationFactory<TestStartup>();

        [Fact]
        public async Task TestGetProfileInfos()
        {
            HttpClient client = _factory.CreateClient();
            HttpResponseMessage response = await client.GetAsync("/management/info");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JToken json = JToken.Parse(await response.Content.ReadAsStringAsync());
            json.SelectToken("$.display-ribbon-on-profiles").Value<string>().Should().Be("dev");
            json.SelectToken("$.activeProfiles").ToObject<IEnumerable<string>>().Should()
                .Contain(new[] {"api-docs", "prod"});
        }
    }
}
