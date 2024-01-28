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
            var bitDepth = monitor.BitDepth;
            if (bitDepth != 0 && dither.state == 0 && dither.mode == 0 && dither.bits == 0)
            {
                dither.mode = 4;
                dither.bits = bitDepth == 8 ? 1 : 2;
            }
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

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ApplyChanges();
            DialogResult = true;
        }

        public bool ChangedCalibration => _viewModel.ChangedCalibration;
        public bool ChangedDither => _viewModel.ChangedDither;
    }
}