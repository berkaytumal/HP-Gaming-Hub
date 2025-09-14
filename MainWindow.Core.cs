using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using HP_Gaming_Hub.ViewModels;
using HP_Gaming_Hub.Services;
using System.Diagnostics;
using Windows.UI;

namespace HP_Gaming_Hub
{
    /// <summary>
    /// Core initialization and utility methods for MainWindow
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// Initialize hardware monitoring and UI
        /// </summary>
        private async void InitializeMonitoring()
        {
            try
            {
                // Subscribe to property changes for real-time updates
                _hardwareMonitorViewModel.PropertyChanged += (s, e) => 
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        UpdateMonitoringUI();
                        UpdateFanUI();
                        UpdateGpuUI();
                        UpdateKeyboardUI();
                    });
                };
                
                // ConnectionStatusText.Text = "Connecting..."; // Badge removed
                LogInfo("Connecting to OmenMon service...");
                await _hardwareMonitorViewModel.RefreshDataAsync();
                // ConnectionStatusText.Text = "Connected"; // Badge removed
                LogInfo("Successfully connected to OmenMon service");
                
                // Start monitoring based on user preference
                if (AppSettings.Instance.AutoStartMonitoring && AppSettings.Instance.AutoRefresh)
                {
                    await _hardwareMonitorViewModel.StartMonitoringAsync();
                    LogInfo("Automatic monitoring started based on user preferences");
                }
                else
                {
                    LogInfo("Automatic monitoring disabled by user preferences");
                }
                
                UpdateMonitoringUI();

            }
            catch (Exception ex)
            {
                // ConnectionStatusText.Text = "Error"; // Badge removed
                LogError($"Error initializing monitoring: {ex.Message}");
                await ShowErrorMessageAsync("Initialization Error", 
                    "Failed to initialize hardware monitoring. Some features may not work properly.", 
                    ex.Message);
            }
        }

        /// <summary>
        /// Show error message to user with details
        /// </summary>
        private async System.Threading.Tasks.Task ShowErrorMessageAsync(string title, string message, string details = null)
        {
            try
            {
                var dialog = new ContentDialog()
                {
                    Title = title,
                    Content = details != null ? $"{message}\n\nDetails: {details}" : message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing error dialog: {ex.Message}");
            }
        }

        /// <summary>
        /// Show success message to user
        /// </summary>
        private async System.Threading.Tasks.Task ShowSuccessMessageAsync(string title, string message)
        {
            try
            {
                var dialog = new ContentDialog()
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing success dialog: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle service operation with error handling
        /// </summary>
        private async System.Threading.Tasks.Task<bool> HandleServiceOperationAsync(Func<System.Threading.Tasks.Task<bool>> operation, string operationName)
        {
            try
            {
                var result = await operation();
                if (!result)
                {
                    await ShowErrorMessageAsync("Operation Failed", 
                        $"Failed to {operationName}. Please check your hardware compatibility and try again.");
                }
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in {operationName}: {ex.Message}");
                await ShowErrorMessageAsync("Unexpected Error", 
                    $"An unexpected error occurred while trying to {operationName}.", 
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Update monitoring UI elements
        /// </summary>
        private void UpdateMonitoringUI()
        {
            var data = _hardwareMonitorViewModel;
            
            // Update temperature displays
            CpuTempText.Text = data.CpuTemperature > 0 ? $"{data.CpuTemperature}°C" : "--°C";
            GpuTempText.Text = data.GpuTemperature > 0 ? $"{data.GpuTemperature}°C" : "--°C";
            
            // Update temperature status
            var maxTemp = Math.Max(data.CpuTemperature, data.GpuTemperature);
            TempStatusText.Text = maxTemp > 80 ? "High" : maxTemp > 70 ? "Warm" : "Normal";
            TempStatusText.Foreground = maxTemp > 80 ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 69, 58)) :
                                      maxTemp > 70 ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 159, 10)) :
                                      new SolidColorBrush(Windows.UI.Color.FromArgb(255, 52, 199, 89));
            
            // Update fan speeds
            Fan1SpeedText.Text = data.Fan1Speed > 0 ? $"{data.Fan1Speed} RPM" : "-- RPM";
            Fan2SpeedText.Text = data.Fan2Speed > 0 ? $"{data.Fan2Speed} RPM" : "-- RPM";
            FanModeText.Text = data.FanMode ?? "Auto";
            
            // Update GPU info
            GpuModeText.Text = data.GpuMode ?? "Unknown";
            
            // Update system status
            OverclockStatusText.Text = data.HasOverclock ? "Yes" : "No";
            MemoryOcStatusText.Text = data.HasMemoryOverclock ? "Yes" : "No";
            UndervoltStatusText.Text = data.HasUndervolt ? "Yes" : "No";
            
            // Update last update time
            LastUpdateText.Text = $"Last Update: {DateTime.Now:HH:mm:ss}";
        }

        /// <summary>
        /// Update GPU UI elements
        /// </summary>
        private void UpdateGpuUI()
        {
            // Update GPU status display
            if (CurrentGpuModeText != null)
                CurrentGpuModeText.Text = _hardwareMonitorViewModel.GpuMode ?? "Unknown";

            if (CurrentGpuTempText != null)
                CurrentGpuTempText.Text = $"{_hardwareMonitorViewModel.GpuTemperature}°C";
            
            // Update connection status
            // if (GpuConnectionStatusText != null)
            //     GpuConnectionStatusText.Text = _hardwareMonitorViewModel.IsConnected ? "Connected" : "Disconnected"; // Badge removed
        }

        /// <summary>
        /// Update Fan UI elements
        /// </summary>
        private void UpdateFanUI()
        {
            var data = _hardwareMonitorViewModel;
            
            // Update current fan speeds
            if (Fan1CurrentSpeedText != null)
                Fan1CurrentSpeedText.Text = data.Fan1Speed > 0 ? $"{data.Fan1Speed} RPM" : "-- RPM";
            if (Fan2CurrentSpeedText != null)
                Fan2CurrentSpeedText.Text = data.Fan2Speed > 0 ? $"{data.Fan2Speed} RPM" : "-- RPM";
            
            // Update fan levels (assuming we can get this from the service)
            if (Fan1LevelText != null)
                Fan1LevelText.Text = data.Fan1Level?.ToString() ?? "--";
            if (Fan2LevelText != null)
                Fan2LevelText.Text = data.Fan2Level?.ToString() ?? "--";
            
            // Update max speeds (assuming we can get this from the service)
            if (Fan1MaxSpeedText != null)
                Fan1MaxSpeedText.Text = data.Fan1MaxSpeed > 0 ? $"{data.Fan1MaxSpeed} RPM" : "-- RPM";
            if (Fan2MaxSpeedText != null)
                Fan2MaxSpeedText.Text = data.Fan2MaxSpeed > 0 ? $"{data.Fan2MaxSpeed} RPM" : "-- RPM";
        }

        /// <summary>
        /// Update Keyboard UI elements
        /// </summary>
        private void UpdateKeyboardUI()
        {
            // Placeholder for keyboard UI updates
            // This method can be expanded when keyboard UI elements are available
        }

        /// <summary>
        /// Append message to console output
        /// </summary>
        private void AppendToConsole(string message)
        {
            try
            {   
                if (LogsOutput != null)
                {
                    LogsOutput.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
                    
                    // Auto-scroll to bottom
                    if (LogsScrollViewer != null)
                    {
                        LogsScrollViewer.ScrollToVerticalOffset(LogsScrollViewer.ScrollableHeight);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error appending to console: {ex.Message}");
            }
        }

        /// <summary>
        /// Log debug message
        /// </summary>
        public void LogDebug(string message)
        {
            Debug.WriteLine($"[DEBUG] {message}");
            AppendToConsole($"[DEBUG] {message}");
        }

        /// <summary>
        /// Log info message
        /// </summary>
        public void LogInfo(string message)
        {
            Debug.WriteLine($"[INFO] {message}");
            AppendToConsole($"[INFO] {message}");
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public void LogWarning(string message)
        {
            Debug.WriteLine($"[WARNING] {message}");
            AppendToConsole($"[WARNING] {message}");
        }

        /// <summary>
        /// Log error message
        /// </summary>
        public void LogError(string message)
        {
            Debug.WriteLine($"[ERROR] {message}");
            AppendToConsole($"[ERROR] {message}");
        }
    }
}