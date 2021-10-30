using System;
using System.Linq;
using System.Windows;
using EDIDParser;
using EDIDParser.Descriptors;
using EDIDParser.Enums;
using NvAPIWrapper.GPU;

namespace novideo_srgb
{
    public class MonitorData
    {
        private readonly GPUOutput _output;
        private readonly Novideo.ColorSpaceConversion? _csc;
        private bool _clamped;
        private bool _illegalChromaticies;

        public MonitorData(int number, GPUOutput output)
        {
            Number = number;
            _output = output;

            Edid = Novideo.GetEDID(output);

            Name = Edid.Descriptors.OfType<StringDescriptor>()
                .FirstOrDefault(x => x.Type == StringDescriptorType.MonitorName)?.Value ?? "<no name>";

            var coords = Edid.DisplayParameters.ChromaticityCoordinates;
            var colorSpace = new Colorimetry.ColorSpace
            {
                Red = new Colorimetry.Point { X = Math.Round(coords.RedX, 3), Y = Math.Round(coords.RedY, 3) },
                Green = new Colorimetry.Point { X = Math.Round(coords.GreenX, 3), Y = Math.Round(coords.GreenY, 3) },
                Blue = new Colorimetry.Point { X = Math.Round(coords.BlueX, 3), Y = Math.Round(coords.BlueY, 3) },
                White = Colorimetry.D65
            };

            if (colorSpace.Equals(Colorimetry.sRGB))
            {
                _clamped = true;
                return;
            }

            if (Edid.DisplayParameters.IsStandardSRGBColorSpace)
            {
                _illegalChromaticies = true;
            }

            _clamped = Novideo.IsColorSpaceConversionActive(output);

            var matrix = Colorimetry.RGBToRGB(Colorimetry.sRGB, colorSpace);
            _csc = Novideo.MatrixToColorSpaceConversion(matrix);
        }

        public int Number { get; }
        public string Name { get; }

        public EDID Edid { get; }

        public bool Clamped
        {
            set
            {
                if (value == _clamped) return;

                try
                {
                    if (value)
                    {
                        if (_csc != null) Novideo.SetColorSpaceConversion(_output, (Novideo.ColorSpaceConversion)_csc);
                    }
                    else
                    {
                        Novideo.DisableColorSpaceConversion(_output);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    return;
                }

                _clamped = value;
            }
            get => _clamped;
        }

        public bool CanClamp => _csc != null;

        public string GPU => _output.PhysicalGPU.FullName;

        public bool IllegalChromaticities => _illegalChromaticies;
    }
}