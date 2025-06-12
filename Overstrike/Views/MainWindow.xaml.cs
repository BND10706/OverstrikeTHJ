using Microsoft.Extensions.DependencyInjection;
using Overstrike.ViewModels;
using System.Windows;

namespace Overstrike.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
