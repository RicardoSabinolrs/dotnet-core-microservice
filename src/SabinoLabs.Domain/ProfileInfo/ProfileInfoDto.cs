using System.Collections.Generic;
using Newtonsoft.Json;

namespace SabinoLabs.Domain.ProfileInfo
{
    public class ProfileInfoDto
    {
        public ProfileInfoDto(string displayRibbonOnProfiles, List<string> activeProfiles)
        {
            DisplayRibbonOnProfiles = displayRibbonOnProfiles;
            ActiveProfiles = activeProfiles;
        }

        [JsonProperty("display-ribbon-on-profiles")]
        public string DisplayRibbonOnProfiles { get; set; }

        [JsonProperty("activeProfiles")] public List<string> ActiveProfiles { get; set; }
    }
}
