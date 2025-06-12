using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Overstrike.Models;
using Overstrike.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Overstrike.ViewModels
{
    public class ConfigurationViewModel : ObservableObject
    {
        private readonly IConfigurationService _configurationService;
        private OverstrikeConfiguration _configuration;

        public string LogFilePath
        {
            get => _configuration.LogPath;
            set
            {
                _configuration.LogPath = value;
                OnPropertyChanged();
            }
        }

        public bool ShowDamagePopups
        {
            get => _configuration.LiveParseEnabled;
            set
            {
                _configuration.LiveParseEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool ShowDpsWindow
        {
            get => _configuration.DpsWindowEnabled;
            set
            {
                _configuration.DpsWindowEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool EnableSoundEffects
        {
            get => _configuration.AudioEnabled;
            set
            {
                _configuration.AudioEnabled = value;
                OnPropertyChanged();
            }
        }

        public IRelayCommand BrowseLogFileCommand { get; }
        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ConfigurationViewModel(IConfigurationService configurationService)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _configuration = _configurationService.Configuration;

            BrowseLogFileCommand = new RelayCommand(BrowseLogFile);
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        // Constructor for design-time data
        public ConfigurationViewModel() : this(App.Current?.Services?.GetService(typeof(IConfigurationService)) as IConfigurationService ??
            new ConfigurationService(App.Current?.Services?.GetService(typeof(ILogger<ConfigurationService>)) as ILogger<ConfigurationService> ??
            (ILogger<ConfigurationService>)new Microsoft.Extensions.Logging.LoggerFactory().CreateLogger<ConfigurationService>()))
        {
        }

        private void BrowseLogFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Select EverQuest Log File"
            };

            if (dialog.ShowDialog() == true)
            {
                LogFilePath = dialog.FileName;
            }
        }

        private async void Save()
        {
            try
            {
                // The configuration is already updated through the properties
                await _configurationService.SaveConfigurationAsync();
                MessageBox.Show("Configuration saved successfully.", "Overstrike", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Overstrike", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            if (System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(w => w.DataContext == this) is System.Windows.Window window)
            {
                window.Close();
            }
        }
    }
}
