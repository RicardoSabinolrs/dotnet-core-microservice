using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace SabinoLabs.Test.Configuration
{
    public class TestMvcStartup
    {
        public static Action<MvcOptions> ConfigureMvcAuthorization() => options =>
        {
            options.Filters.Add(new AllowAnonymousFilter());
        };
    }
}
