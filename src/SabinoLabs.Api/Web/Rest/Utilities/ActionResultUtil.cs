using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace SabinoLabs.Web.Rest.Utilities
{
    public static class ActionResultUtil
    {
        public static ActionResult WrapOrNotFoundAsDto<TDtoType>(object value, IMapper mapper)
        {
            if (value != null)
            {
                TDtoType resultAsDto = mapper.Map<TDtoType>(value);
                return new OkObjectResult(resultAsDto);
            }

            return new NotFoundResult();
        }

        public static ActionResult WrapOrNotFound(object value) =>
            value != null ? new OkObjectResult(value) : new NotFoundResult();
    }
}
