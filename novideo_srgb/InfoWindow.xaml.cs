using EDIDParser;

namespace novideo_srgb
{
    public partial class InfoWindow
    {
        public InfoWindow(MonitorData monitor)
        {
            Coords = monitor.Edid.DisplayParameters.ChromaticityCoordinates;
            IsIllegal = monitor.IllegalChromaticities;
            InitializeComponent();
        }

        public ChromaticityCoordinates Coords { get; }
        
        public bool IsIllegal { get; }
    }
}