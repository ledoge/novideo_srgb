using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using EDIDParser;

namespace novideo_srgb
{
    public class AdvancedViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MonitorData _monitor;

        private int _ditherState;
        private int _ditherMode;
        private int _ditherBits;

        public AdvancedViewModel()
        {
            throw new NotSupportedException();
        }

        public AdvancedViewModel(MonitorData monitor, Novideo.DitherControl dither)
        {
            _monitor = monitor;
            _ditherBits = dither.bits;
            _ditherMode = dither.mode;
            _ditherState = dither.state;
        }

        public ChromaticityCoordinates Coords => _monitor.Edid.DisplayParameters.ChromaticityCoordinates;

        public bool UseEdid
        {
            set
            {
                if (value == _monitor.UseEdid) return;
                _monitor.UseEdid = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UseIcc));
                ChangedCalibration = true;
            }
            get => _monitor.UseEdid;
        }

        public bool UseIcc
        {
            set
            {
                if (value == _monitor.UseIcc) return;
                _monitor.UseIcc = value;
                OnPropertyChanged(nameof(UseEdid));
                OnPropertyChanged();
                ChangedCalibration = true;
            }
            get => _monitor.UseIcc;
        }

        public string ProfilePath
        {
            set
            {
                if (value == _monitor.ProfilePath) return;
                _monitor.ProfilePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProfileName));
                ChangedCalibration = true;
            }
            get => _monitor.ProfilePath;
        }

        public string ProfileName => Path.GetFileName(ProfilePath);

        public bool CalibrateGamma
        {
            set
            {
                if (value == _monitor.CalibrateGamma) return;
                _monitor.CalibrateGamma = value;
                OnPropertyChanged();
                ChangedCalibration = true;
            }
            get => _monitor.CalibrateGamma;
        }

        public int SelectedGamma
        {
            set
            {
                if (value == _monitor.SelectedGamma) return;
                _monitor.SelectedGamma = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UseCustomGamma));
                ChangedCalibration = true;
            }
            get => _monitor.SelectedGamma;
        }

        public Visibility UseCustomGamma =>
            SelectedGamma == 2 || SelectedGamma == 3 ? Visibility.Visible : Visibility.Collapsed;

        public double CustomGamma
        {
            set
            {
                if (value == _monitor.CustomGamma) return;
                _monitor.CustomGamma = value;
                OnPropertyChanged();
                ChangedCalibration = true;
            }
            get => _monitor.CustomGamma;
        }

        public bool IgnoreTRC
        {
            set
            {
                if (value == _monitor.IgnoreTRC) return;
                _monitor.IgnoreTRC = value;
                OnPropertyChanged();
                ChangedCalibration = true;
            }
            get => _monitor.IgnoreTRC;
        }

        public double CustomPercentage
        {
            set
            {
                if (value == _monitor.CustomPercentage) return;
                _monitor.CustomPercentage = value;
                OnPropertyChanged();
                ChangedCalibration = true;
            }
            get => _monitor.CustomPercentage;
        }

        public bool ChangedCalibration { get; set; }

        public int DitherState
        {
            set
            {
                if (value == _ditherState) return;
                _ditherState = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CustomDither));
                ChangedDither = true;
            }
            get => _ditherState;
        }

        public int DitherMode
        {
            set
            {
                if (value == _ditherMode) return;
                _ditherMode = value;
                OnPropertyChanged();
                ChangedDither = true;
            }
            get => _ditherMode;
        }

        public int DitherBits
        {
            set
            {
                if (value == _ditherBits) return;
                _ditherBits = value;
                OnPropertyChanged();
                ChangedDither = true;
            }
            get => _ditherBits;
        }

        public bool CustomDither => DitherState == 1;

        public bool ChangedDither { get; set; }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}