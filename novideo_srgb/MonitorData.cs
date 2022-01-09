using System;
using System.ComponentModel;
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
            EdidColorSpace = new Colorimetry.ColorSpace
            {
                Red = new Colorimetry.Point { X = Math.Round(coords.RedX, 3), Y = Math.Round(coords.RedY, 3) },
                Green = new Colorimetry.Point { X = Math.Round(coords.GreenX, 3), Y = Math.Round(coords.GreenY, 3) },
                Blue = new Colorimetry.Point { X = Math.Round(coords.BlueX, 3), Y = Math.Round(coords.BlueY, 3) },
                White = Colorimetry.D65
            };

            if (Edid.DisplayParameters.IsStandardSRGBColorSpace)
            {
                _illegalChromaticies = true;
            }

            _clamped = Novideo.IsColorSpaceConversionActive(_output);

            ProfilePath = "";
            CustomGamma = 2.2;
            CustomPercentage = 100;
        }

        public MonitorData(int number, Display display, uint id, bool useIcc, string profilePath, bool calibrateGamma,
            int selectedGamma, double customGamma, double customPercentage, int target) : this(number, display, id)
        {
            UseIcc = useIcc;
            ProfilePath = profilePath;
            CalibrateGamma = calibrateGamma;
            SelectedGamma = selectedGamma;
            CustomGamma = customGamma;
            CustomPercentage = customPercentage;
            Target = target;
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
            if (UseEdid)
                Novideo.SetColorSpaceConversion(_output, Colorimetry.RGBToRGB(TargetColorSpace, EdidColorSpace));
            else if (UseIcc)
            {
                var profile = ICCMatrixProfile.FromFile(ProfilePath);
                if (CalibrateGamma)
                {
                    var black = profile.trcs.Min(x => x.SampleAt(0));
                    ToneCurve gamma;
                    switch (SelectedGamma)
                    {
                        case 0:
                            gamma = new SrgbEOTF(black);
                            break;
                        case 1:
                            gamma = new GammaToneCurve(2.4, black, 0);
                            break;
                        case 2:
                            gamma = new GammaToneCurve(CustomGamma, black, CustomPercentage / 100);
                            break;
                        case 3:
                            gamma = new GammaToneCurve(CustomGamma, black, CustomPercentage / 100, true);
                            break;
                        default:
                            throw new NotSupportedException("Unsupported gamma type " + SelectedGamma);
                    }

                    Novideo.SetColorSpaceConversion(_output, profile, TargetColorSpace, gamma);
                }
                else
                {
                    Novideo.SetColorSpaceConversion(_output, profile, TargetColorSpace);
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

        public bool CanClamp => UseEdid && !EdidColorSpace.Equals(TargetColorSpace) || UseIcc && ProfilePath != "";

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

        public double CustomPercentage { set; get; }

        public int Target { set; get; }

        private Colorimetry.ColorSpace EdidColorSpace { get; }

        private Colorimetry.ColorSpace TargetColorSpace
        {
            get
            {
                switch (Target)
                {
                    case 1: return Colorimetry.DisplayP3;
                    case 2: return Colorimetry.AdobeRGB;
                    default: return Colorimetry.sRGB;
                }
            }
        }

        public Novideo.DitherControl DitherControl => Novideo.GetDitherControl(_output);

        public void ApplyDither(int state, int bits, int mode)
        {
            try
            {
                Novideo.SetDitherControl(_output, state, bits, mode);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}