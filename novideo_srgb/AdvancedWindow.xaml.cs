using System;
using System.Windows;
using System.Windows.Controls;

namespace novideo_srgb
{
    public partial class AdvancedWindow
    {
        private AdvancedViewModel _viewModel;

        public AdvancedWindow(MonitorData monitor)
        {
            var dither = monitor.DitherControl;
            _viewModel = new AdvancedViewModel(monitor, dither);
            DataContext = _viewModel;
            InitializeComponent();

            for (var i = 0; i < 5; i++)
            {
                ((ComboBoxItem)DitherMode.Items[i]).IsEnabled = ((dither.modeCaps >> i) & 1) == 1;
            }

            for (var i = 0; i < 3; i++)
            {
                ((ComboBoxItem)DitherBits.Items[i]).IsEnabled = ((dither.bitsCaps >> i) & 1) == 1;
            }
        }

        private static string BrowseProfiles()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "ICC Profiles|*.icc;*.icm"
            };

            var result = dlg.ShowDialog();

            return result == true ? dlg.FileName : null;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var profilePath = BrowseProfiles();
            if (!string.IsNullOrEmpty(profilePath))
            {
                _viewModel.ProfilePath = profilePath;
            }
        }

        public bool ChangedCalibration => _viewModel.ChangedCalibration;
        public bool ChangedDither => _viewModel.ChangedDither;
    }
}