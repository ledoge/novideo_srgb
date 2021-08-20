using System;
using System.Linq;
using System.Windows;
using EDIDParser;
using EDIDParser.Descriptors;
using EDIDParser.Enums;
using NvAPIWrapper.GPU;

namespace novideo_srgb
{
    internal class MonitorData
    {
        private readonly GPUOutput _output;
        private readonly Novideo.ColorSpaceConversion? _csc;
        private bool _clamped;

        public MonitorData(int number, GPUOutput output)
        {
            Number = number;
            _output = output;

            Edid = Novideo.GetEDID(output);

            Name = Edid.Descriptors.OfType<StringDescriptor>()
                .FirstOrDefault(x => x.Type == StringDescriptorType.MonitorName)?.Value ?? "<no name>";

            if (Edid.DisplayParameters.IsStandardSRGBColorSpace)
            {
                _clamped = true;
                return;
            }
            
            _clamped = Novideo.IsColorSpaceConversionActive(output);

            //var coords = Edid.DisplayParameters.ChromaticityCoordinates;
            var colorSpace = new Colorimetry.ColorSpace
            {
                Red = new Colorimetry.Point {X = 0.6883, Y = 0.3030},
                Green = new Colorimetry.Point {X = 0.1745, Y = 0.7426},
                Blue = new Colorimetry.Point {X = 0.1438, Y = 0.0619},
                White = Colorimetry.D65
            };

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
                        if (_csc != null) Novideo.SetColorSpaceConversion(_output, (Novideo.ColorSpaceConversion) _csc);
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
    }
}