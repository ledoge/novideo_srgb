using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using novideo_srgb.core.Configuration;
using System;
using System.Windows;

namespace novideo_srgb;

public partial class App : Application
{
    private ServiceProvider serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        BuildServices(services, GetConfig());
        serviceProvider = services.BuildServiceProvider();
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var mainWindow = serviceProvider.GetService<MainWindow>();
        mainWindow.Show();
    }

    private static IConfiguration GetConfig() => new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true)
                                                                           .Build();

    private static void BuildServices(IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<AppOptions>()
                .Bind(config.GetSection(nameof(AppOptions)));

        services.AddOptions<MonitorsOptions>()
                .Bind(config.GetSection(nameof(MonitorsOptions)));

        services.AddSingleton<MainWindow>();
    }
}