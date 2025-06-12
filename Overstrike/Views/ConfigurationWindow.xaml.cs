using Overstrike.ViewModels;
using System.Windows;

namespace Overstrike.Views;

/// <summary>
/// Configuration window for Overstrike settings
/// </summary>
public partial class ConfigurationWindow : Window
{
    public ConfigurationWindow(ConfigurationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
