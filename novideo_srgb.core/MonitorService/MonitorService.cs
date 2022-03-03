using Microsoft.Extensions.Options;
using novideo_srgb.core.Configuration;
using novideo_srgb.core.Models;
using NvAPIWrapper.Display;
using System.Collections.ObjectModel;

namespace novideo_srgb.core.MonitorService
{
    //Just a stub for now, will absorb MonitorData
    public class MonitorService
    {
        public ObservableCollection<MonitorData> Monitors { get; }

        private readonly MonitorsOptions _monitorsOptions;

        public MonitorService(IOptions<MonitorsOptions> monitorsOptions) : this(monitorsOptions.Value) { }
        public MonitorService(MonitorsOptions monitorsOptions)
        {
            _monitorsOptions = monitorsOptions;
            RefreshMonitors();
        }

        public void RefreshMonitors()
        {
            Monitors.Clear();

            int index = 0;
            foreach(var display in Display.GetDisplays())
            {
                index++;
                var existingSettings = _monitorsOptions?.MonitorOptions?.FirstOrDefault(x => x.Id == display.DisplayDevice.DisplayId);
                if (existingSettings != null)
                {
                    Monitors.Add(new MonitorData(index, display, existingSettings));
                }
                else
                {
                    Monitors.Add(new MonitorData(index, display));
                }
            }


        }
    }
}
