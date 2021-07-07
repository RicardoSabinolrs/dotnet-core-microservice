using Microsoft.Extensions.Configuration;

namespace SabinoLabs.Configuration
{
    public static class ConfigurationHelper
    {
        public static TModel GetOptions<TModel>(this IConfiguration configuration, string section) where TModel : new()
        {
            TModel model = new TModel();
            configuration.GetSection(section).Bind(model);

            return model;
        }
    }
}
