using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Win32;
using NvAPIWrapper.Display;

namespace novideo_srgb
{
    public class MainViewModel
    {
        private readonly static SemaphoreSlim _saveSemaphoreSlim = new SemaphoreSlim(1, 1);
        public ObservableCollection<MonitorData> Monitors { get; }

        private string _configPath;

        private string _startupName;
        private RegistryKey _startupKey;
        private string _startupValue;

        public MainViewModel()
        {
            Monitors = new ObservableCollection<MonitorData>();
            _configPath = AppDomain.CurrentDomain.BaseDirectory + "config.xml";

            _startupName = "novideo_srgb";
            _startupKey = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            _startupValue = Application.ExecutablePath + " -minimize";

            UpdateMonitors();
        }

        public bool? RunAtStartup
        {
            get
            {
                var keyValue = _startupKey.GetValue(_startupName);

                if (keyValue == null)
                {
                    return false;
                }

                if ((string)keyValue == _startupValue)
                {
                    return true;
                }

                return null;
            }
            set
            {
                if (value == true)
                {
                    _startupKey.SetValue(_startupName, _startupValue);
                }
                else
                {
                    _startupKey.DeleteValue(_startupName);
                }
            }
        }

        private void UpdateMonitors()
        {
            Monitors.Clear();
            List<XElement> config = null;
            if (File.Exists(_configPath))
            {
                config = XElement.Load(_configPath).Descendants("monitor").ToList();
            }

            var hdrPaths = DisplayConfigManager.GetHdrDisplayPaths();

            var number = 1;
            foreach (var display in Display.GetDisplays())
            {
                var displays = WindowsDisplayAPI.Display.GetDisplays();
                var path = displays.First(x => x.DisplayName == display.Name).DevicePath;

                var hdrActive = hdrPaths.Contains(path);

                var settings = config?.FirstOrDefault(x => (string)x.Attribute("path") == path);
                MonitorData monitor;
                if (settings != null)
                {
                    monitor = new MonitorData(this, number++, display, path, hdrActive,
                        (bool)settings.Attribute("clamp_sdr"),
                        (bool)settings.Attribute("use_icc"),
                        (string)settings.Attribute("icc_path"),
                        (bool)settings.Attribute("calibrate_gamma"),
                        (int)settings.Attribute("selected_gamma"),
                        (double)settings.Attribute("custom_gamma"),
                        (double)settings.Attribute("custom_percentage"),
                        (int)settings.Attribute("target"),
                        (bool)settings.Attribute("disable_optimization"),
                        (string)settings.Attribute("ignore"));
                }
                else
                {
                    monitor = new MonitorData(this, number++, display, path, hdrActive, false);
                }

                Monitors.Add(monitor);
            }

            foreach (var monitor in Monitors)
            {
                monitor.ReapplyClamp();
            }
        }

        public void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateMonitors();
            }
            catch
            {
            }
        }

        public void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode != PowerModes.Resume) return;
            OnDisplaySettingsChanged(null, null);
        }

        public void SaveConfig()
        {
            try
            {
                if(_saveSemaphoreSlim.Wait(100))
                {
                    if (File.Exists(_configPath))
                    {
                        var monitors = XElement.Load(_configPath);
                        foreach (var monitor in Monitors)
                        {
                            var current = monitors.Elements().FirstOrDefault(x => x.Attribute("path").Value == monitor.Path);
                            if (current != default) // If current exist in config, update
                            {
                                current.SetAttributeValue("clamp_sdr", monitor.ClampSdr);
                                current.SetAttributeValue("use_icc", monitor.UseIcc);
                                current.SetAttributeValue("icc_path", monitor.ProfilePath);
                                current.SetAttributeValue("calibrate_gamma", monitor.CalibrateGamma);
                                current.SetAttributeValue("selected_gamma", monitor.SelectedGamma);
                                current.SetAttributeValue("custom_gamma", monitor.CustomGamma);
                                current.SetAttributeValue("custom_percentage", monitor.CustomPercentage);
                                current.SetAttributeValue("target", monitor.Target);
                                current.SetAttributeValue("disable_optimization", monitor.DisableOptimization);
                                current.SetAttributeValue("ignore", monitor.Ignore);
                            }
                            else // otherwise, add
                            {
                                monitors.Add(new XElement("monitor", new XAttribute("path", monitor.Path),
                                    new XAttribute("clamp_sdr", monitor.ClampSdr),
                                    new XAttribute("use_icc", monitor.UseIcc),
                                    new XAttribute("icc_path", monitor.ProfilePath),
                                    new XAttribute("calibrate_gamma", monitor.CalibrateGamma),
                                    new XAttribute("selected_gamma", monitor.SelectedGamma),
                                    new XAttribute("custom_gamma", monitor.CustomGamma),
                                    new XAttribute("custom_percentage", monitor.CustomPercentage),
                                    new XAttribute("target", monitor.Target),
                                    new XAttribute("disable_optimization", monitor.DisableOptimization),
                                    new XAttribute("ignore", monitor.Ignore)));
                            }
                        }
                        monitors.Save(_configPath);
                    }
                    else
                    {
                        var xElem = new XElement("monitors",
                            Monitors.Select(x =>
                                new XElement("monitor", new XAttribute("path", x.Path),
                                    new XAttribute("clamp_sdr", x.ClampSdr),
                                    new XAttribute("use_icc", x.UseIcc),
                                    new XAttribute("icc_path", x.ProfilePath),
                                    new XAttribute("calibrate_gamma", x.CalibrateGamma),
                                    new XAttribute("selected_gamma", x.SelectedGamma),
                                    new XAttribute("custom_gamma", x.CustomGamma),
                                    new XAttribute("custom_percentage", x.CustomPercentage),
                                    new XAttribute("target", x.Target),
                                    new XAttribute("disable_optimization", x.DisableOptimization),
                                    new XAttribute("ignore", x.Ignore))));
                        xElem.Save(_configPath);
                    }
                    _saveSemaphoreSlim.Release();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\nTry extracting the program elsewhere.");
                Environment.Exit(1);
            }
        }
    }
}