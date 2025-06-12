using Overstrike.Models;
using Overstrike.ViewModels;
using System.Windows;

namespace Overstrike.Views;

/// <summary>
/// DPS window for displaying real-time damage metrics
/// </summary>
public partial class DpsWindow : Window
{
    private readonly DpsViewModel _viewModel;

    public DpsWindow()
    {
        InitializeComponent();
        _viewModel = new DpsViewModel();
        DataContext = _viewModel;

        // Configure window properties for overlay
        Topmost = true;
        ShowInTaskbar = false;
        WindowStyle = WindowStyle.ToolWindow;
    }

    public void UpdateDpsData(Dictionary<string, DpsData> dpsData)
    {
        _viewModel.UpdateDpsData(dpsData);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
