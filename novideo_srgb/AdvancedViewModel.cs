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

        public AdvancedViewModel()
        {
            throw new NotSupportedException();
        }

        public AdvancedViewModel(MonitorData monitor)
        {
            _monitor = monitor;
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
                Changed = true;
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
                Changed = true;
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
                Changed = true;
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
                Changed = true;
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
                Changed = true;
            }
            get => _monitor.SelectedGamma;
        }

        public Visibility UseCustomGamma => SelectedGamma == 2 ? Visibility.Visible : Visibility.Collapsed;

        public float CustomGamma
        {
            set
            {
                if (value == _monitor.CustomGamma) return;
                _monitor.CustomGamma = value;
                OnPropertyChanged();
                Changed = true;
            }
            get => _monitor.CustomGamma;
        }

        public bool Changed { get; set; }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}