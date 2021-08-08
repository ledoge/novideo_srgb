using System.Collections.ObjectModel;
using System.Linq;
using EDIDParser.Descriptors;
using EDIDParser.Enums;
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
                    var edid = Novideo.GetEDID(output);
                    var name = edid.Descriptors.OfType<StringDescriptor>()
                        .FirstOrDefault(x => x.Type == StringDescriptorType.MonitorName)?.Value ?? "<no name>";

                    if (edid.DisplayParameters.IsStandardSRGBColorSpace)
                    {
                        Monitors.Add(new MonitorData(number++, name, true, output, null));
                        continue;
                    }

                    var isCscActive = Novideo.IsColorSpaceConversionActive(output);
                    var coords = edid.DisplayParameters.ChromaticityCoordinates;
                    var colorSpace = new Colorimetry.ColorSpace
                    {
                        red = new Colorimetry.Point {x = coords.RedX, y = coords.RedY},
                        green = new Colorimetry.Point {x = coords.GreenX, y = coords.GreenY},
                        blue = new Colorimetry.Point {x = coords.BlueX, y = coords.BlueY},
                        white = Colorimetry.D65
                    };

                    var matrix = Colorimetry.ColorSpaceToColorSpace(Colorimetry.sRGB, colorSpace);
                    var csc = Novideo.MatrixToColorSpaceConversion(matrix);

                    Monitors.Add(new MonitorData(number++, name, isCscActive, output, csc));
                }
            }
        }
    }
}