using novideo_srgb.core.Models;
using NvAPIWrapper.Display;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace novideo_srgb
{
    internal class MainViewModel
    {
        public ObservableCollection<MonitorData> Monitors { get; }

        private string _configPath;

        public MainViewModel()
        {
            Monitors = new ObservableCollection<MonitorData>();
            _configPath = AppDomain.CurrentDomain.BaseDirectory + "config.xml";
            UpdateMonitors();
        }

        public void UpdateMonitors()
        {
            Monitors.Clear();
            List<XElement> config = null;
            if (File.Exists(_configPath))
            {
                config = XElement.Load(_configPath).Descendants("monitor").ToList();
            }

            var number = 1;
            foreach (var display in Display.GetDisplays())
            {
                var id = display.DisplayDevice.DisplayId;
                var settings = config?.FirstOrDefault(x => (uint)x.Attribute("id") == id);
                MonitorData monitor;
                if (settings != null)
                {
                    monitor = new MonitorData(number++, display, display.DisplayDevice.DisplayId,
                        (bool)settings.Attribute("use_icc"),
                        (string)settings.Attribute("icc_path"),
                        (bool)settings.Attribute("calibrate_gamma"),
                        (int)settings.Attribute("selected_gamma"),
                        (double)settings.Attribute("custom_gamma"),
                        (double?)settings.Attribute("custom_percentage") ?? 100,
                        (int?)settings.Attribute("target") ?? 0);
                }
                else
                {
                    monitor = new MonitorData(number++, display, display.DisplayDevice.DisplayId);
                }

                Monitors.Add(monitor);
            }
        }

        public void SaveConfig()
        {
            var xElem = new XElement("monitors",
                Monitors.Select(x =>
                    new XElement("monitor", new XAttribute("id", x.ID),
                        new XAttribute("use_icc", x.UseIcc),
                        new XAttribute("icc_path", x.ProfilePath),
                        new XAttribute("calibrate_gamma", x.CalibrateGamma),
                        new XAttribute("selected_gamma", x.SelectedGamma),
                        new XAttribute("custom_gamma", x.CustomGamma),
                        new XAttribute("custom_percentage", x.CustomPercentage),
                        new XAttribute("target", x.Target))));
            xElem.Save(_configPath);
        }
    }
}