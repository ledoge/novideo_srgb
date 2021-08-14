using System.Collections.ObjectModel;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Native;

namespace novideo_srgb
{
    internal class MainViewModel
    {
        public ObservableCollection<MonitorData> Monitors { get; }

        public MainViewModel()
        {
            Monitors = new ObservableCollection<MonitorData>();
            UpdateMonitors();
        }

        public void UpdateMonitors()
        {
            Monitors.Clear();
            var number = 1;
            foreach (var gpuHandle in GPUApi.EnumPhysicalGPUs())
            {
                var gpu = new PhysicalGPU(gpuHandle);
                foreach (var output in gpu.ActiveOutputs)
                {
                    Monitors.Add(new MonitorData(number++, output));
                }
            }
        }
    }
}