using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using NvAPIWrapper.GPU;

namespace novideo_srgb
{
    internal class MonitorData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly GPUOutput _output;
        private readonly Novideo.ColorSpaceConversion? _csc;
        private bool _clamped;

        public MonitorData(int number, string name, bool clamped, GPUOutput output, Novideo.ColorSpaceConversion? csc)
        {
            Number = number;
            Name = name;
            _clamped = clamped;
            _output = output;
            _csc = csc;
        }

        public int Number { get; }
        public string Name { get; }

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
                OnPropertyChanged();
            }
            get => _clamped;
        }

        public bool CanClamp => _csc != null;

        public string GPU => _output.PhysicalGPU.FullName;

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}