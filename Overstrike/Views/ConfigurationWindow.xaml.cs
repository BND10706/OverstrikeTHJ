using Overstrike.ViewModels;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Overstrike.Views;

/// <summary>
/// Configuration window for Overstrike settings
/// </summary>
public partial class ConfigurationWindow : Window
{
    public ConfigurationWindow()
    {
        InitializeComponent();

        // Get the service from the DI container if available, otherwise create a new instance
        var viewModel = App.Current.Services?.GetService<ConfigurationViewModel>() ?? new ConfigurationViewModel();
        DataContext = viewModel;
    }

    public ConfigurationWindow(ConfigurationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
    }
}
