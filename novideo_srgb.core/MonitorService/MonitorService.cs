using Microsoft.Extensions.Options;
using novideo_srgb.core.Configuration;
using novideo_srgb.core.Models;
using System.Collections.ObjectModel;

namespace novideo_srgb.core.MonitorService
{
    //Just a stub for now, will absorb MonitorData
    public class MonitorService
    {
        public ObservableCollection<MonitorData> Monitors { get; }

        private readonly MonitorsOptions _monitorsOptions;

        public MonitorService(IOptions<MonitorsOptions> monitorsOptions) : this(monitorsOptions.Value) { }
        public MonitorService(MonitorsOptions monitorsOptions) => _monitorsOptions = monitorsOptions;


    }
}
