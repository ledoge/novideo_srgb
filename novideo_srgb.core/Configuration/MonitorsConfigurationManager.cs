using Microsoft.Extensions.Options;
using System.Text.Json;

namespace novideo_srgb.core.Configuration
{
    public class MonitorsConfigurationManager : IMonitorsConfigurationManager
    {
        private readonly MonitorsOptions _monitorsOptions;

        public MonitorsConfigurationManager(IOptions<MonitorsOptions> monitorsOptions) : this(monitorsOptions.Value) { }
        public MonitorsConfigurationManager(MonitorsOptions monitorsOptions) => _monitorsOptions = monitorsOptions;

        public Task UpdateConfigurationSection(MonitorsOptions newConfig)
        {
            _monitorsOptions.MonitorOptions = newConfig?.MonitorOptions;

            var container = new
            {
                MonitorsOptions = _monitorsOptions
            };

            return File.WriteAllTextAsync(Constants.ConfigFileName, JsonSerializer.Serialize(container));
        }
    }
}
