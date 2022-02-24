namespace novideo_srgb.core.Configuration
{
    public interface IMonitorsConfigurationManager
    {
        Task UpdateConfigurationSection(MonitorsOptions newConfig);
    }
}
