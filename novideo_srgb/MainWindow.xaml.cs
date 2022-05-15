using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace novideo_srgb
{
    public partial class MainWindow
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = (MainViewModel)DataContext;
            SystemEvents.DisplaySettingsChanged += _viewModel.OnDisplaySettingsChanged;
        }

        private void AboutButton_Click(object sender, RoutedEventArgs o)
        {
            var window = new AboutWindow
            {
                Owner = this
            };
            window.ShowDialog();
        }

        private void AdvancedButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.Cast<Window>().Any(x => x is AdvancedWindow)) return;
            var monitor = ((FrameworkElement)sender).DataContext as MonitorData;
            var window = new AdvancedWindow(monitor)
            {
                Owner = this
            };
            if (window.ShowDialog() == false) return;
            if (window.ChangedCalibration)
            {
                try
                {
                    _viewModel.SaveConfig();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n\nTry extracting the program elsewhere.");
                    Environment.Exit(1);
                }

                monitor?.ReapplyClamp();
            }

            if (window.ChangedDither)
            {
                monitor?.ApplyDither(window.DitherState.SelectedIndex, Math.Max(window.DitherBits.SelectedIndex, 0),
                    Math.Max(window.DitherMode.SelectedIndex, 0));
            }
        }
    }
}