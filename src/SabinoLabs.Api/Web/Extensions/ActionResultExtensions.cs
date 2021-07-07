using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SabinoLabs.Web.Extensions
{
    public static class ActionResultExtensions
    {
        public static ActionResult WithHeaders(this ActionResult receiver, IHeaderDictionary headers) =>
            new ActionResultWithHeaders(receiver, headers);
    }
}
