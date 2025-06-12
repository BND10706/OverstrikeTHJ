using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Overstrike.Services;
using Overstrike.ViewModels;
using Overstrike.Views;
using System.Windows;

namespace Overstrike;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register services
                services.AddSingleton<IConfigurationService, ConfigurationService>();
                services.AddSingleton<ILogTrackerService, LogTrackerService>();
                services.AddSingleton<IAudioService, AudioService>();
                services.AddSingleton<IDpsCalculatorService, DpsCalculatorService>();
                services.AddSingleton<IOverlayService, OverlayService>();

                // Register ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<ConfigurationViewModel>();
                services.AddTransient<DpsViewModel>();

                // Register Views
                services.AddTransient<MainWindow>();
                services.AddTransient<ConfigurationWindow>();
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            })
            .Build();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }
}
