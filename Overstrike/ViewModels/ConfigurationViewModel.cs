using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Overstrike.Models;
using Overstrike.Services;
using System;
using System.Windows;
using System.Linq;

namespace Overstrike.ViewModels
{
    public class ConfigurationViewModel : ObservableObject
    {
        private readonly IConfigurationService _configurationService;
        private OverstrikeConfiguration _configuration;

        public string LogFilePath
        {
            get => _configuration.LogFilePath;
            set
            {
                _configuration.LogFilePath = value;
                OnPropertyChanged();
            }
        }

        public bool ShowDamagePopups
        {
            get => _configuration.ShowDamagePopups;
            set
            {
                _configuration.ShowDamagePopups = value;
                OnPropertyChanged();
            }
        }

        public bool ShowDpsWindow
        {
            get => _configuration.ShowDpsWindow;
            set
            {
                _configuration.ShowDpsWindow = value;
                OnPropertyChanged();
            }
        }

        public bool EnableSoundEffects
        {
            get => _configuration.EnableSoundEffects;
            set
            {
                _configuration.EnableSoundEffects = value;
                OnPropertyChanged();
            }
        }

        public IRelayCommand BrowseLogFileCommand { get; }
        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ConfigurationViewModel(IConfigurationService configurationService)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _configuration = _configurationService.GetConfiguration();

            BrowseLogFileCommand = new RelayCommand(BrowseLogFile);
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        // Constructor for design-time data
        public ConfigurationViewModel() : this(App.Current?.Services?.GetService(typeof(IConfigurationService)) as IConfigurationService ?? new ConfigurationService())
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

        private void Save()
        {
            try
            {
                _configurationService.SaveConfiguration(_configuration);
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
