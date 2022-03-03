namespace novideo_srgb.core.Configuration
{
    public class MonitorsOptions
    {
        public IEnumerable<MonitorOptions>? MonitorOptions { get; set; }
    }

    public class MonitorOptions
    {
        public uint Id { get; set; }
        public bool UseIcc { get; set; }
        public string IccPath { get; set; } = string.Empty;
        public bool CalibrateGamma { get; set; }
        public int SelectedGamma { get; set; }
        public double CustomGamma { get; set; }
        public double CustomPercentage { get; set; } = 100;
        public int Target { get; set; } = 0;
    }
}
