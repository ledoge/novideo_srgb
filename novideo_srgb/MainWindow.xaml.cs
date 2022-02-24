using novideo_srgb.core;
using novideo_srgb.core.Models;
using System;
using System.Linq;
using System.Windows;

namespace novideo_srgb
{
    public partial class MainWindow
    {
        private readonly MainViewModel _viewModel;
        private readonly IServiceProvider _serviceProvider;

        public MainWindow()
        {
            _serviceProvider = ServiceHelpers.GetServiceProvider();

            InitializeComponent();
            _viewModel = (MainViewModel)DataContext;
        }

        private void MonitorRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.UpdateMonitors();
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
                _viewModel.SaveConfig();
                TryWithMessage(() => monitor?.ReapplyClamp());
            }

            if (window.ChangedDither)
            {
                TryWithMessage(() => monitor?.ApplyDither(window.DitherState.SelectedIndex,
                                                          Math.Max(window.DitherBits.SelectedIndex, 0),
                                                          Math.Max(window.DitherMode.SelectedIndex, 0)));
            }
        }

        //I don't like this but I needed to move some GUI logic out of MonitorData
        private void TryWithMessage(Action doStuff)
        {
            try
            {
                doStuff();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}