using System.Linq;
using System.Windows;

namespace novideo_srgb
{
    public partial class MainWindow
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
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
            window.ShowDialog();
            if (!window.Changed) return;
            
            _viewModel.SaveConfig();
            monitor?.ReapplyClamp();
        }
    }
}