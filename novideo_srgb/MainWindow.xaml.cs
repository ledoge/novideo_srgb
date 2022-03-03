using Microsoft.Extensions.Options;
using novideo_srgb.core.Configuration;
using novideo_srgb.core.Models;
using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace novideo_srgb
{
    public partial class MainWindow
    {
        private readonly MainViewModel _viewModel;
        private NotifyIcon _trayIcon;
        private AppOptions _options;

        public MainWindow(IOptions<AppOptions> options) : this(options.Value) { }

        public MainWindow(AppOptions options)
        {
            _options = options;

            InitializeComponent();

            _viewModel = (MainViewModel)DataContext;

            initializeTrayIcon();
        }

        private void initializeTrayIcon()
        {
            if(_options.MinimizeToTray)
            {
                _trayIcon = new NotifyIcon();
                _trayIcon.Text = "Novideo sRGB";
                _trayIcon.Icon = new Icon("icon.ico");
                _trayIcon.Click += new EventHandler(TrayIcon_Click);
                _trayIcon.Visible = true;

                if (_options.StartHidden)
                {
                    WindowState = WindowState.Minimized;
                    Hide();
                    ShowInTaskbar = false;
                }
            }
        }

        private void TrayIcon_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            ShowInTaskbar = true;
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
        private static void TryWithMessage(Action doStuff)
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

        protected override void OnStateChanged(EventArgs e)
        {
            if(_options.MinimizeToTray && WindowState == WindowState.Minimized)
            {
                Hide();
            }

            base.OnStateChanged(e);
        }
    }
}