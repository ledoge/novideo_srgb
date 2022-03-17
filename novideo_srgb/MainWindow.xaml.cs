using Microsoft.Extensions.Options;
using Microsoft.Win32;
using novideo_srgb.core.Configuration;
using novideo_srgb.core.DisplayConfig;
using novideo_srgb.core.Models;
using novideo_srgb.core.PowerBroadcast;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Application = System.Windows.Application;

namespace novideo_srgb;

public partial class MainWindow
{
    private readonly MainViewModel _viewModel;
    private System.Windows.Forms.NotifyIcon _trayIcon;
    private AppOptions _options;

    public MainWindow(IOptions<AppOptions> options) : this(options.Value) { }

    public MainWindow(AppOptions options)
    {
        _options = options;

        InitializeComponent();

        _viewModel = (MainViewModel)DataContext;

        InitializeTrayIcon();
        //ReapplyMonitorSettings();
    }

    private void InitializeTrayIcon()
    {
        if(_options.MinimizeToTray)
        {
            _trayIcon = new System.Windows.Forms.NotifyIcon
            {
                Text = "Novideo sRGB",
                Icon = new Icon("icon.ico"),
                Visible = true
            };


            _trayIcon.Click += new EventHandler(TrayIcon_Click);

            if (_options.StartHidden)
            {
                _ = ExecuteWithDelay(() =>
                {
                    Hide();
                    ShowInTaskbar = false;
                }, 150);
            }
        }
    }

    private void TrayIcon_Click(object sender, EventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        ShowInTaskbar = true;
    }

    private void ReapplySettingsButton_Click(object sender, RoutedEventArgs e) => ReapplyMonitorSettings();
    private void MonitorRefreshButton_Click(object sender, RoutedEventArgs e) => _viewModel.UpdateMonitors();

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

    private void ReapplyMonitorSettings()
    {
        foreach (var monitor in _viewModel.Monitors)
        {
            monitor.ReapplyClamp();
        }
    }

    //I don't like this but I needed to move some GUI logic out of MonitorData
    private static void TryWithMessage(Action action)
    {
        try
        {
            action();
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

    protected override void OnSourceInitialized(EventArgs e)
    {
        SystemEvents.DisplaySettingsChanged += new EventHandler(OnDisplaySettingsChanged);
        base.OnSourceInitialized(e);
        var handle = new WindowInteropHelper(this).Handle;
        PowerBroadcastNotificationHelpers.RegisterPowerBroadcastNotification(handle, PowerSettingGuids.CONSOLE_DISPLAY_STATE);
        HwndSource.FromHwnd(handle)?.AddHook(HandleMessages);
    }

    private void OnDisplaySettingsChanged(object sender, EventArgs e)
    {
        /*
         * TODO: This needs to work on a per-monitor basis.
         * GetDisplayHdrStatus loops through all monitors and returns the result only for the last one.
         * I'm not seeing an obvious way to tie monitor data grabbed from the Nvidia API to that gathered from the Windows API.
         * And I don't have extra displays to experiment with right now.
         * Maybe investigate whether the Nvidia API can tell us whether HDR is enabled on a display?
        */
        var hdrStatus = DisplayConfigManager.GetDisplayHdrStatus();
        var clamp = hdrStatus == DisplayHdrStatus.Disabled ? true : false;

        foreach (var monitor in _viewModel.Monitors)
        {
            monitor.Clamped = clamp;
            monitor.ReapplyClamp();
        }
    }

    private IntPtr HandleMessages(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) => PowerBroadcastNotificationHelpers.HandleBroadcastNotification(msg, lParam, (message) =>
    {
        if (((char)message.Data) == (char)ConsoleDisplayState.TurnedOn)
        {
            _ = ExecuteWithDelay(ReapplyMonitorSettings, 150);
        }
    });

    private async Task ExecuteWithDelay(Action action, int seconds)
    {
        await Task.Delay(seconds);
        action();
    }

    protected override void OnClosed(EventArgs e)
    {
        _trayIcon.Dispose();
        base.OnClosed(e);
    }
}
