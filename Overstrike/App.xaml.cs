using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Overstrike.Services;
using Overstrike.Utilities;
using Overstrike.ViewModels;
using Overstrike.Views;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Overstrike;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    // Static accessor for the services provider
    public IServiceProvider? Services => _host?.Services;

    // Static accessor for the current App instance
    public new static App Current => (App)Application.Current;

    protected override void OnStartup(StartupEventArgs e)
    {
        DebugHelper.LogInfo("Application starting...");
        try
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
                    services.AddTransient<PopupConfigViewModel>();

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

            try
            {
                // First attempt to get and show the main window
                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Topmost = true;  // Make sure it appears on top initially
                mainWindow.Show();
                mainWindow.Topmost = false; // Then allow it to be covered by other windows
            }
            catch (Exception ex)
            {
                // If dependency injection fails, fall back to creating window manually
                MessageBox.Show($"Error creating main window: {ex.Message}\n\nWill try fallback method.",
                    "Overstrike - Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                var viewModel = new MainViewModel(
                    _host.Services.GetService<ILogger<MainViewModel>>()!,
                    _host.Services.GetService<IConfigurationService>()!,
                    _host.Services.GetService<ILogTrackerService>()!,
                    _host.Services.GetService<IAudioService>()!,
                    _host.Services.GetService<IDpsCalculatorService>()!,
                    _host.Services.GetService<IOverlayService>()!
                );

                var window = new MainWindow(viewModel)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowState = WindowState.Normal
                };
                window.Show();
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogError("Application startup error", ex);

            // Create a more detailed error message
            string errorMessage =
                $"Error starting application: {ex.Message}\n\n" +
                $"Please check the log file for more details.\n\n" +
                $"Technical Details: {ex.GetType().Name}";

            MessageBox.Show(errorMessage, "Overstrike Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // As a last resort fallback, try to create a basic window
            try
            {
                var basicWindow = new Window
                {
                    Title = "Overstrike - Fallback Mode",
                    Width = 600,
                    Height = 400,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                var content = new System.Windows.Controls.StackPanel { Margin = new Thickness(20) };
                content.Children.Add(new System.Windows.Controls.TextBlock
                {
                    Text = "Overstrike encountered an error while starting.",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 20)
                });

                content.Children.Add(new System.Windows.Controls.TextBlock
                {
                    Text = $"Error: {ex.Message}",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 20)
                });

                var button = new System.Windows.Controls.Button
                {
                    Content = "Exit Application",
                    Padding = new Thickness(10, 5, 10, 5),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                button.Click += (s, args) => Application.Current.Shutdown();
                content.Children.Add(button);

                basicWindow.Content = content;
                basicWindow.Show();

                // Don't throw - let the application run in fallback mode
                return;
            }
            catch
            {
                // If even the fallback fails, just exit
                Application.Current.Shutdown();
            }
        }

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }
}
