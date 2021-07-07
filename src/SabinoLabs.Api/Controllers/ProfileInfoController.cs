using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SabinoLabs.Domain.ProfileInfo;

namespace SabinoLabs.Controllers
{
    [Route("management")]
    [ApiController]
    public class ProfileInfoController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        private readonly ILogger<ProfileInfoController> _log;

        public ProfileInfoController(ILogger<ProfileInfoController> log, IHostEnvironment environment,
            IConfiguration configuration)
        {
            _log = log;
            _environment = environment;
            _configuration = configuration;
        }

        [HttpGet("info")]
        public ActionResult<ProfileInfoDto> GetProfileInfos()
        {
            _log.LogDebug("REST request to get profile informations");
            return Ok(new ProfileInfoDto(GetDisplayRibbonOnProfiles(), GetActiveProfile()));
        }

        private List<string> GetActiveProfile()
        {
            List<string> activeProfiles = new() {"api-docs"};

            if (_environment.IsDevelopment())
            {
                activeProfiles.Add("dev");
            }
            else if (_environment.IsProduction())
            {
                activeProfiles.Add("prod");
            }
            else if (_environment.IsStaging())
            {
                activeProfiles.Add("stag");
            }

            return activeProfiles;
        }

        private string GetDisplayRibbonOnProfiles() =>
            _configuration.GetSection("RibbonInfo")["display-ribbon-on-profiles"];
    }
}
