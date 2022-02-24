using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using novideo_srgb.core.Configuration;

namespace novideo_srgb.core
{
    public static class ServiceHelpers
    {
        public static IServiceProvider GetServiceProvider()
        {
            var services = new ServiceCollection();
            var config = GetConfig();

            BuildServices(services, config);

            return services.BuildServiceProvider();
        }

        private static IConfiguration GetConfig() => new ConfigurationBuilder().AddJsonFile(Constants.ConfigFileName, false, true)
                                                                               .Build();

        private static void BuildServices(IServiceCollection services, IConfiguration config)
        {
            services.AddOptions<MonitorsOptions>()
                    .Bind(config.GetSection(nameof(MonitorsOptions)));

            services.AddSingleton<IMonitorsConfigurationManager, MonitorsConfigurationManager>();
        }
    }
}
