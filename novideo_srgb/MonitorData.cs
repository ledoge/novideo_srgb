using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using EDIDParser;
using EDIDParser.Descriptors;
using EDIDParser.Enums;
using NvAPIWrapper.Display;
using NvAPIWrapper.GPU;

namespace novideo_srgb
{
    public class MonitorData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly GPUOutput _output;
        private readonly Novideo.ColorSpaceConversion? _csc;
        private bool _clamped;
        private bool _illegalChromaticies;

        public MonitorData(int number, Display display, uint id)
        {
            Number = number;
            _output = display.Output;

            Edid = Novideo.GetEDID(display);

            Name = Edid.Descriptors.OfType<StringDescriptor>()
                .FirstOrDefault(x => x.Type == StringDescriptorType.MonitorName)?.Value ?? "<no name>";

            ID = id;

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

            _clamped = Novideo.IsColorSpaceConversionActive(_output);

            var matrix = Colorimetry.RGBToRGB(Colorimetry.sRGB, colorSpace);
            _csc = Novideo.MatrixToColorSpaceConversion(matrix);

            CustomGamma = 2.2f;
            ProfilePath = "";
        }

        public MonitorData(int number, Display display, uint id, bool useIcc, string profilePath, bool calibrateGamma,
            int selectedGamma, double customGamma, bool ignoreTRC) : this(number, display, id)
        {
            UseIcc = useIcc;
            ProfilePath = profilePath;
            CalibrateGamma = calibrateGamma;
            SelectedGamma = selectedGamma;
            CustomGamma = customGamma;
            IgnoreTRC = ignoreTRC;
        }

        public int Number { get; }
        public string Name { get; }
        public EDID Edid { get; }
        public uint ID { get; }

        private void UpdateClamp(bool doClamp)
        {
            if (_clamped)
            {
                Novideo.DisableColorSpaceConversion(_output);
            }

            if (!doClamp) return;

            if (_clamped) Thread.Sleep(100);
            if (UseEdid && _csc != null) Novideo.SetColorSpaceConversion(_output, (Novideo.ColorSpaceConversion)_csc);
            else if (UseIcc)
            {
                var profile = ICCMatrixProfile.FromFile(ProfilePath);
                if (CalibrateGamma)
                {
                    var black = !IgnoreTRC ? profile.trcs.Min(x => x.SampleAt(0)) : 0;
                    ToneCurve gamma;
                    switch (SelectedGamma)
                    {
                        case 0:
                            gamma = new SrgbEOTF(black);
                            break;
                        case 1:
                            gamma = new Bt1886(black);
                            break;
                        case 2:
                            gamma = new GammaToneCurve(CustomGamma, black);
                            break;
                        case 3:
                            gamma = new GammaToneCurve(CustomGamma, black, true);
                            break;
                        default:
                            throw new NotSupportedException("Unsupported gamma type " + SelectedGamma);
                    }

                    Novideo.SetColorSpaceConversion(_output, profile, gamma, IgnoreTRC);
                }
                else
                {
                    Novideo.SetColorSpaceConversion(_output, profile);
                }
            }
        }

        public bool Clamped
        {
            set
            {
                try
                {
                    UpdateClamp(value);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    _clamped = Novideo.IsColorSpaceConversionActive(_output);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanClamp));
                    return;
                }

                _clamped = value;
            }
            get => _clamped;
        }

        public void ReapplyClamp()
        {
            Clamped = CanClamp && Clamped;

            OnPropertyChanged(nameof(CanClamp));
            OnPropertyChanged(nameof(Clamped));
        }

        public bool CanClamp => UseEdid && _csc != null || UseIcc && File.Exists(ProfilePath);

        public string GPU => _output.PhysicalGPU.FullName;

        public bool IllegalChromaticities => _illegalChromaticies;

        public bool UseEdid
        {
            set => UseIcc = !value;
            get => !UseIcc;
        }

        public bool UseIcc { set; get; }

        public string ProfilePath { set; get; }

        public bool CalibrateGamma { set; get; }

        public int SelectedGamma { set; get; }

        public double CustomGamma { set; get; }

        public bool IgnoreTRC { set; get; }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}