using EDIDParser;

namespace novideo_srgb
{
    public partial class InfoWindow
    {
        public InfoWindow(EDID edid)
        {
            Coords = edid.DisplayParameters.ChromaticityCoordinates;
            InitializeComponent();
        }

        public ChromaticityCoordinates Coords { get; }
    }
}