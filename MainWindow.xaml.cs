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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HP_Gaming_Hub
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private int _previousSelectedIndex = 3; // Default to Monitor page (index 3)
        private HardwareMonitorViewModel _hardwareMonitorViewModel;
        public static MainWindow Instance { get; private set; }
        
        public MainWindow()
        {
            this.InitializeComponent();
            Instance = this;
            // Extend content into title bar and set custom title bar
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            _hardwareMonitorViewModel = new HardwareMonitorViewModel();
            LogInfo("HP Gaming Hub started - Initializing monitoring...");
            InitializeMonitoring();
        }

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
                UpdateMonitoringUI();
                
                // Start auto monitoring if enabled
                if (MonitoringToggle.IsChecked == true)
                {
                    LogInfo("Starting automatic monitoring...");
                    await _hardwareMonitorViewModel.StartMonitoringAsync();
                }
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

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ConnectionStatusText.Text = "Refreshing..."; // Badge removed
            await _hardwareMonitorViewModel.RefreshDataAsync();
            // ConnectionStatusText.Text = "Connected"; // Badge removed
                UpdateMonitoringUI();
            }
            catch (Exception ex)
            {
                // ConnectionStatusText.Text = "Error"; // Badge removed
            }
        }

        private async void MonitoringToggle_Click(object sender, RoutedEventArgs e)
        {
            if (MonitoringToggle.IsChecked == true)
            {
                await _hardwareMonitorViewModel.StartMonitoringAsync();
                // ConnectionStatusText.Text = "Monitoring"; // Badge removed
            }
            else
            {
                _hardwareMonitorViewModel.StopMonitoring();
                // ConnectionStatusText.Text = "Stopped"; // Badge removed
            }
        }

        private async void TestOmenMonButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TestOmenMonButton.IsEnabled = false;
                TestOmenMonButton.Content = "Testing...";
                // ConnectionStatusText.Text = "Testing OmenMon..."; // Badge removed
                
                var omenMonService = new HP_Gaming_Hub.Services.OmenMonService();
                var testResult = await omenMonService.TestConnectivityAsync();
                
                if (testResult.Success)
                {
                    // ConnectionStatusText.Text = "OmenMon Test: Success"; // Badge removed
                    if (SettingsInfoBar != null)
                    {
                        SettingsInfoBar.IsOpen = true;
                        SettingsInfoBar.Message = $"OmenMon connectivity test successful!\n{testResult.Output}";
                        SettingsInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success;
                    }
                }
                else
                {
                    // ConnectionStatusText.Text = "OmenMon Test: Failed"; // Badge removed
                    if (SettingsInfoBar != null)
                    {
                        SettingsInfoBar.IsOpen = true;
                        SettingsInfoBar.Message = $"OmenMon connectivity test failed: {testResult.ErrorMessage}\nError Type: {testResult.ErrorType}";
                        SettingsInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"[TestOmenMonButton_Click] Test completed - Success: {testResult.Success}");
            }
            catch (Exception ex)
            {
                // ConnectionStatusText.Text = "Test Error"; // Badge removed
                if (SettingsInfoBar != null)
                {
                    SettingsInfoBar.IsOpen = true;
                    SettingsInfoBar.Message = $"Error during OmenMon test: {ex.Message}";
                    SettingsInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
                }
                System.Diagnostics.Debug.WriteLine($"[TestOmenMonButton_Click] Exception: {ex.Message}");
            }
            finally
            {
                TestOmenMonButton.IsEnabled = true;
                TestOmenMonButton.Content = "Test OmenMon";
            }
        }

        // Fan Control Event Handlers
        private void FanModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FanModeComboBox.SelectedItem is ComboBoxItem selectedItem && ManualFanControlPanel != null && FanControlInfoBar != null)
            {
                string mode = selectedItem.Tag?.ToString() ?? "Default";
                
                // Show/hide manual controls based on selection
                if (mode == "Manual")
                {
                    ManualFanControlPanel.Visibility = Visibility.Visible;
                    FanControlInfoBar.IsOpen = true;
                }
                else
                {
                    ManualFanControlPanel.Visibility = Visibility.Collapsed;
                    FanControlInfoBar.IsOpen = false;
                    
                    // Apply the selected fan mode
                    _ = ApplyFanModeAsync(mode);
                }
            }
        }

        private async System.Threading.Tasks.Task ApplyFanModeAsync(string mode)
        {
            try
            {
                // FanConnectionStatusText.Text = "Applying..."; // Badge removed
                await _hardwareMonitorViewModel.SetFanModeAsync(mode);
                // FanConnectionStatusText.Text = "Connected"; // Badge removed
                UpdateFanUI();
            }
            catch (Exception ex)
            {
                // FanConnectionStatusText.Text = "Error"; // Badge removed
            }
        }

        private void Fan1SpeedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Fan1SliderValueText != null)
            {
                Fan1SliderValueText.Text = $"Level: {(int)e.NewValue}";
            }
        }

        private void Fan2SpeedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Fan2SliderValueText != null)
            {
                Fan2SliderValueText.Text = $"Level: {(int)e.NewValue}";
            }
        }

        private async void ApplyFanSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            // FanConnectionStatusText.Text = "Applying..."; // Badge removed
            int fan1Level = (int)Fan1SpeedSlider.Value;
            int fan2Level = (int)Fan2SpeedSlider.Value;
            
            var success = await HandleServiceOperationAsync(
                () => _hardwareMonitorViewModel.SetFanLevelsAsync(fan1Level, fan2Level),
                "apply fan speed settings"
            );
            
            if (success)
            {
                // FanConnectionStatusText.Text = "Connected"; // Badge removed
                await ShowSuccessMessageAsync("Fan Settings Applied", "Fan speed levels have been successfully updated.");
                UpdateFanUI();
            }
            else
            {
                // FanConnectionStatusText.Text = "Error"; // Badge removed
            }
        }

        private async void ResetFanSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // if (FanConnectionStatusText != null)
                //     FanConnectionStatusText.Text = "Resetting..."; // Badge removed
                    
                await _hardwareMonitorViewModel.SetFanModeAsync("Default");
                
                if (FanModeComboBox != null)
                    FanModeComboBox.SelectedIndex = 0;
                    
                if (ManualFanControlPanel != null)
                    ManualFanControlPanel.Visibility = Visibility.Collapsed;
                    
                if (FanControlInfoBar != null)
                    FanControlInfoBar.IsOpen = false;
                    
                // if (FanConnectionStatusText != null)
                //     FanConnectionStatusText.Text = "Connected"; // Badge removed
                    
                UpdateFanUI();
            }
            catch (Exception ex)
            {
                // if (FanConnectionStatusText != null)
                //     FanConnectionStatusText.Text = "Error"; // Badge removed
            }
        }

        private async void MaxFanButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // FanConnectionStatusText.Text = "Enabling Max..."; // Badge removed
                await _hardwareMonitorViewModel.SetMaxFanAsync(true);
                // FanConnectionStatusText.Text = "Connected"; // Badge removed
                UpdateFanUI();
            }
            catch (Exception ex)
            {
                // FanConnectionStatusText.Text = "Error"; // Badge removed
            }
        }

        // GPU Settings Event Handlers
        private async void GpuModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GpuModeComboBox?.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string mode)
            {
                try
                {
                    // Check if _hardwareMonitorViewModel is initialized
                    if (_hardwareMonitorViewModel != null)
                    {
                        await _hardwareMonitorViewModel.SetGpuModeAsync(mode);
                        UpdateGpuUI();
                    }
                }
                catch (Exception ex)
                {
                    if (GpuSettingsInfoBar != null)
                    {
                        GpuSettingsInfoBar.IsOpen = true;
                        GpuSettingsInfoBar.Message = $"Failed to set GPU mode: {ex.Message}";
                        GpuSettingsInfoBar.Severity = InfoBarSeverity.Error;
                    }
                }
            }
        }



        private void CpuPL1Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (CpuPL1ValueText != null)
            {
                CpuPL1ValueText.Text = $"{(int)e.NewValue}W";
            }
        }

        private void CpuPL4Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (CpuPL4ValueText != null)
            {
                CpuPL4ValueText.Text = $"{(int)e.NewValue}W";
            }
        }

        private void GpuPLSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (GpuPLValueText != null)
            {
                GpuPLValueText.Text = $"{(int)e.NewValue}W";
            }
        }

        private async void XmpToggle_Toggled(object sender, RoutedEventArgs e)
        {
            var success = await HandleServiceOperationAsync(
                () => _hardwareMonitorViewModel.SetXmpAsync(XmpToggle.IsOn),
                "toggle XMP profile"
            );
            
            if (success)
            {
                if (GpuSettingsInfoBar != null)
                {
                    GpuSettingsInfoBar.IsOpen = true;
                    GpuSettingsInfoBar.Message = $"XMP profile {(XmpToggle.IsOn ? "enabled" : "disabled")} successfully.";
                    GpuSettingsInfoBar.Severity = InfoBarSeverity.Success;
                }
            }
            else
            {
                if (GpuSettingsInfoBar != null)
                {
                    GpuSettingsInfoBar.IsOpen = true;
                    GpuSettingsInfoBar.Message = "Failed to toggle XMP profile. Please check the error messages and try again.";
                    GpuSettingsInfoBar.Severity = InfoBarSeverity.Error;
                }
            }
        }

        private async void ApplyGpuSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var cpuPL1Success = await HandleServiceOperationAsync(
                () => _hardwareMonitorViewModel.SetCpuPowerLimitsAsync((int)CpuPL1Slider.Value, (int)CpuPL4Slider.Value),
                "apply CPU power limits"
            );
            
            var gpuPLSuccess = await HandleServiceOperationAsync(
                () => _hardwareMonitorViewModel.SetGpuPowerLimitAsync((int)GpuPLSlider.Value),
                "apply GPU power limit"
            );
            
            if (cpuPL1Success && gpuPLSuccess)
            {
                if (GpuSettingsInfoBar != null)
                {
                    GpuSettingsInfoBar.IsOpen = true;
                    GpuSettingsInfoBar.Message = "GPU settings applied successfully. Changes may require a system restart.";
                    GpuSettingsInfoBar.Severity = InfoBarSeverity.Success;
                }
                UpdateGpuUI();
            }
            else
            {
                if (GpuSettingsInfoBar != null)
                {
                    GpuSettingsInfoBar.IsOpen = true;
                    GpuSettingsInfoBar.Message = "Some GPU settings failed to apply. Please check the error messages and try again.";
                    GpuSettingsInfoBar.Severity = InfoBarSeverity.Error;
                }
            }
        }

        private async void RefreshGpuDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _hardwareMonitorViewModel.RefreshDataAsync();
                UpdateGpuUI();
                if (GpuSettingsInfoBar != null)
                {
                    GpuSettingsInfoBar.IsOpen = true;
                    GpuSettingsInfoBar.Message = "GPU data refreshed successfully.";
                    GpuSettingsInfoBar.Severity = InfoBarSeverity.Success;
                }
            }
            catch (Exception ex)
            {
                if (GpuSettingsInfoBar != null)
                {
                    GpuSettingsInfoBar.IsOpen = true;
                    GpuSettingsInfoBar.Message = $"Failed to refresh GPU data: {ex.Message}";
                    GpuSettingsInfoBar.Severity = InfoBarSeverity.Error;
                }
            }
        }

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

        // Keyboard Settings Event Handlers
        private async void BacklightToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // UI elements removed - method kept for compatibility
            /*
            try
            {
                await _hardwareMonitorViewModel.SetKeyboardBacklightAsync(BacklightToggle.IsOn);
                if (KeyboardSettingsInfoBar != null)
                {
                    KeyboardSettingsInfoBar.IsOpen = true;
                    KeyboardSettingsInfoBar.Message = $"Keyboard backlight {(BacklightToggle.IsOn ? "enabled" : "disabled")} successfully.";
                    KeyboardSettingsInfoBar.Severity = InfoBarSeverity.Success;
                }
                UpdateKeyboardUI();
            }
            catch (Exception ex)
            {
                if (KeyboardSettingsInfoBar != null)
                {
                    KeyboardSettingsInfoBar.IsOpen = true;
                    KeyboardSettingsInfoBar.Message = $"Failed to toggle backlight: {ex.Message}";
                    KeyboardSettingsInfoBar.Severity = InfoBarSeverity.Error;
                }
            }
            */
        }

        private async void ColorPresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // UI elements removed - method kept for compatibility
            /*
            if (ColorPresetComboBox?.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string preset)
            {
                try
                {
                    if (_hardwareMonitorViewModel != null)
                    {
                        await _hardwareMonitorViewModel.SetKeyboardColorPresetAsync(preset);
                        UpdateColorSlidersFromPreset(preset);
                        UpdateKeyboardUI();
                    }
                }
                catch (Exception ex)
                {
                    if (KeyboardSettingsInfoBar != null)
                    {
                        KeyboardSettingsInfoBar.IsOpen = true;
                        KeyboardSettingsInfoBar.Message = $"Failed to set color preset: {ex.Message}";
                        KeyboardSettingsInfoBar.Severity = InfoBarSeverity.Error;
                    }
                }
            }
            */
        }

        private void ColorSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            UpdateColorPreviews();
        }

        private async void AnimationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // UI elements removed - method kept for compatibility
            /*
            if (AnimationComboBox?.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string animation)
            {
                try
                {
                    await _hardwareMonitorViewModel.SetKeyboardAnimationAsync(animation);
                    UpdateKeyboardUI();
                }
                catch (Exception ex)
                {
                    if (KeyboardSettingsInfoBar != null)
                    {
                        KeyboardSettingsInfoBar.IsOpen = true;
                        KeyboardSettingsInfoBar.Message = $"Failed to set animation: {ex.Message}";
                        KeyboardSettingsInfoBar.Severity = InfoBarSeverity.Error;
                    }
                }
            }
            */
        }

        private async void ApplyKeyboardSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // UI elements removed - method kept for compatibility
            /*
            // Apply custom colors
            var colors = new int[]
            {
                GetColorFromSliders(Zone1RedSlider, Zone1GreenSlider, Zone1BlueSlider),
                GetColorFromSliders(Zone2RedSlider, Zone2GreenSlider, Zone2BlueSlider),
                GetColorFromSliders(Zone3RedSlider, Zone3GreenSlider, Zone3BlueSlider),
                GetColorFromSliders(Zone4RedSlider, Zone4GreenSlider, Zone4BlueSlider)
            };
            
            var success = await HandleServiceOperationAsync(
                () => _hardwareMonitorViewModel.SetKeyboardCustomColorsAsync(colors),
                "apply keyboard settings"
            );
            
            if (success)
            {
                KeyboardSettingsInfoBar.IsOpen = true;
                KeyboardSettingsInfoBar.Message = "Keyboard settings applied successfully.";
                KeyboardSettingsInfoBar.Severity = InfoBarSeverity.Success;
                UpdateKeyboardUI();
            }
            else
            {
                KeyboardSettingsInfoBar.IsOpen = true;
                KeyboardSettingsInfoBar.Message = "Failed to apply keyboard settings. Please check the error messages and try again.";
                KeyboardSettingsInfoBar.Severity = InfoBarSeverity.Error;
            }
            */
        }

        private async void RefreshKeyboardDataButton_Click(object sender, RoutedEventArgs e)
        {
            var success = await HandleServiceOperationAsync(
                () => _hardwareMonitorViewModel.RefreshDataAsync(),
                "refresh keyboard data"
            );
            
            if (success)
            {
                UpdateKeyboardUI();
                KeyboardSettingsInfoBar.IsOpen = true;
                KeyboardSettingsInfoBar.Message = "Keyboard data refreshed successfully.";
                KeyboardSettingsInfoBar.Severity = InfoBarSeverity.Success;
            }
            else
            {
                KeyboardSettingsInfoBar.IsOpen = true;
                KeyboardSettingsInfoBar.Message = "Failed to refresh keyboard data. Please check the error messages and try again.";
                KeyboardSettingsInfoBar.Severity = InfoBarSeverity.Error;
            }
        }

        private async void ResetKeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            // UI elements removed - method kept for compatibility
            /*
            var success = await HandleServiceOperationAsync(
                () => _hardwareMonitorViewModel.ResetKeyboardSettingsAsync(),
                "reset keyboard settings"
            );
            
            if (success)
            {
                ResetColorSliders();
                BacklightToggle.IsOn = true;
                ColorPresetComboBox.SelectedIndex = 0;
                AnimationComboBox.SelectedIndex = 0;
                
                KeyboardSettingsInfoBar.IsOpen = true;
                KeyboardSettingsInfoBar.Message = "Keyboard settings reset to default successfully.";
                KeyboardSettingsInfoBar.Severity = InfoBarSeverity.Success;
                UpdateKeyboardUI();
            }
            else
            {
                KeyboardSettingsInfoBar.IsOpen = true;
                KeyboardSettingsInfoBar.Message = "Failed to reset keyboard settings. Please check the error messages and try again.";
                KeyboardSettingsInfoBar.Severity = InfoBarSeverity.Error;
            }
            */
        }

        private void UpdateKeyboardUI()
        {
            // UI elements removed - method kept for compatibility
            /*
            // Update keyboard status display
            // if (KeyboardConnectionStatusText != null)
            //     KeyboardConnectionStatusText.Text = _hardwareMonitorViewModel.IsConnected ? "Connected" : "Disconnected"; // Badge removed
            if (BacklightToggle != null)
                BacklightToggle.IsOn = _hardwareMonitorViewModel.KeyboardBacklightEnabled;
            */
        }

        private void UpdateColorPreviews()
        {
            // UI elements removed - method kept for compatibility
            /*
            if (Zone1ColorPreview != null && Zone1RedSlider != null && Zone1GreenSlider != null && Zone1BlueSlider != null)
                Zone1ColorPreview.Background = new SolidColorBrush(Color.FromArgb(255, (byte)Zone1RedSlider.Value, (byte)Zone1GreenSlider.Value, (byte)Zone1BlueSlider.Value));
            if (Zone2ColorPreview != null && Zone2RedSlider != null && Zone2GreenSlider != null && Zone2BlueSlider != null)
                Zone2ColorPreview.Background = new SolidColorBrush(Color.FromArgb(255, (byte)Zone2RedSlider.Value, (byte)Zone2GreenSlider.Value, (byte)Zone2BlueSlider.Value));
            if (Zone3ColorPreview != null && Zone3RedSlider != null && Zone3GreenSlider != null && Zone3BlueSlider != null)
                Zone3ColorPreview.Background = new SolidColorBrush(Color.FromArgb(255, (byte)Zone3RedSlider.Value, (byte)Zone3GreenSlider.Value, (byte)Zone3BlueSlider.Value));
            if (Zone4ColorPreview != null && Zone4RedSlider != null && Zone4GreenSlider != null && Zone4BlueSlider != null)
                Zone4ColorPreview.Background = new SolidColorBrush(Color.FromArgb(255, (byte)Zone4RedSlider.Value, (byte)Zone4GreenSlider.Value, (byte)Zone4BlueSlider.Value));
            */
        }

        private int GetColorFromSliders(Slider redSlider, Slider greenSlider, Slider blueSlider)
        {
            var r = (int)redSlider.Value;
            var g = (int)greenSlider.Value;
            var b = (int)blueSlider.Value;
            return (r << 16) | (g << 8) | b; // Convert to RGB hex format
        }

        private void UpdateColorSlidersFromPreset(string preset)
        {
            // UI elements removed - method kept for compatibility
            /*
            switch (preset.ToLower())
            {
                case "red":
                    SetAllZoneSliders(255, 0, 0);
                    break;
                case "green":
                    SetAllZoneSliders(0, 255, 0);
                    break;
                case "blue":
                    SetAllZoneSliders(0, 0, 255);
                    break;
                case "yellow":
                    SetAllZoneSliders(255, 255, 0);
                    break;
                case "purple":
                    SetAllZoneSliders(128, 0, 128);
                    break;
                case "cyan":
                    SetAllZoneSliders(0, 255, 255);
                    break;
                case "white":
                    SetAllZoneSliders(255, 255, 255);
                    break;
                case "orange":
                    SetAllZoneSliders(255, 165, 0);
                    break;
            }
            */
        }

        private void SetAllZoneSliders(int red, int green, int blue)
        {
            // UI elements removed - method kept for compatibility
            /*
            Zone1RedSlider.Value = red; Zone1GreenSlider.Value = green; Zone1BlueSlider.Value = blue;
            Zone2RedSlider.Value = red; Zone2GreenSlider.Value = green; Zone2BlueSlider.Value = blue;
            Zone3RedSlider.Value = red; Zone3GreenSlider.Value = green; Zone3BlueSlider.Value = blue;
            Zone4RedSlider.Value = red; Zone4GreenSlider.Value = green; Zone4BlueSlider.Value = blue;
            UpdateColorPreviews();
            */
        }

        private void ResetColorSliders()
        {
            // UI elements removed - method kept for compatibility
            /*
            Zone1RedSlider.Value = 255; Zone1GreenSlider.Value = 0; Zone1BlueSlider.Value = 0;
            Zone2RedSlider.Value = 0; Zone2GreenSlider.Value = 255; Zone2BlueSlider.Value = 0;
            Zone3RedSlider.Value = 0; Zone3GreenSlider.Value = 0; Zone3BlueSlider.Value = 255;
            Zone4RedSlider.Value = 255; Zone4GreenSlider.Value = 255; Zone4BlueSlider.Value = 0;
            UpdateColorPreviews();
            */
        }

        private async void RefreshFanDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // FanConnectionStatusText.Text = "Refreshing..."; // Badge removed
                await _hardwareMonitorViewModel.RefreshDataAsync();
                // FanConnectionStatusText.Text = "Connected"; // Badge removed
                UpdateFanUI();
            }
            catch (Exception ex)
            {
                // FanConnectionStatusText.Text = "Error"; // Badge removed
            }
        }

        private async void FanMonitoringToggle_Click(object sender, RoutedEventArgs e)
        {
            if (FanMonitoringToggle.IsChecked == true)
            {
                await _hardwareMonitorViewModel.StartMonitoringAsync();
                // FanConnectionStatusText.Text = "Monitoring"; // Badge removed
            }
            else
            {
                _hardwareMonitorViewModel.StopMonitoring();
                // FanConnectionStatusText.Text = "Connected"; // Badge removed
            }
        }

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

        private void MainNavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            // Find currently visible page
            FrameworkElement currentPage = null;
            if (FanPage.Visibility == Visibility.Visible) currentPage = FanPage;
            else if (GraphicsPage.Visibility == Visibility.Visible) currentPage = GraphicsPage;
            else if (KeyboardPage.Visibility == Visibility.Visible) currentPage = KeyboardPage;
            else if (MonitorPage.Visibility == Visibility.Visible) currentPage = MonitorPage;
            else if (ConsolePage.Visibility == Visibility.Visible) currentPage = ConsolePage;
            else if (SettingsPage.Visibility == Visibility.Visible) currentPage = SettingsPage;

            // Determine target page and index
            FrameworkElement targetPage = null;
            int currentSelectedIndex = -1;
            if (args.SelectedItem == FanNavItem) { targetPage = FanPage; currentSelectedIndex = 0; }
            else if (args.SelectedItem == GraphicsNavItem) { targetPage = GraphicsPage; currentSelectedIndex = 1; }
            else if (args.SelectedItem == KeyboardNavItem) { targetPage = KeyboardPage; currentSelectedIndex = 2; }
            else if (args.SelectedItem == MonitorNavItem) { targetPage = MonitorPage; currentSelectedIndex = 3; }
            else if (args.SelectedItem == ConsoleNavItem) { targetPage = ConsolePage; currentSelectedIndex = 4; }
            else if (args.SelectedItem == SettingsNavItem) { targetPage = SettingsPage; currentSelectedIndex = 5; }

            // If same page is selected, do nothing
            if (currentPage == targetPage) return;
            
            // Determine animation direction based on index comparison
            bool isMovingDown = currentSelectedIndex < _previousSelectedIndex;
            double slideOutDistance = isMovingDown ? 30.0 : -30.0;
            double slideInStartPosition = isMovingDown ? -30.0 : 30.0;
            
            // Update previous selected index
            _previousSelectedIndex = currentSelectedIndex;

            if (currentPage != null && targetPage != null)
             {
                 // Create slide out and fade out storyboard for current page
                 var slideOutStoryboard = new Storyboard();
                 
                 var fadeOut = new DoubleAnimation
                  {
                      From = 1.0,
                      To = 0.0,
                      Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                      EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                  };
                 Storyboard.SetTarget(fadeOut, currentPage);
                 Storyboard.SetTargetProperty(fadeOut, "Opacity");
                 slideOutStoryboard.Children.Add(fadeOut);

                 var slideOut = new DoubleAnimation
                  {
                      From = 0.0,
                      To = slideOutDistance,
                      Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                      EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                  };
                 Storyboard.SetTarget(slideOut, currentPage);
                 Storyboard.SetTargetProperty(slideOut, "(UIElement.RenderTransform).(TranslateTransform.Y)");
                 slideOutStoryboard.Children.Add(slideOut);

                 // Ensure current page has a TranslateTransform
                 if (currentPage.RenderTransform == null || !(currentPage.RenderTransform is TranslateTransform))
                     currentPage.RenderTransform = new TranslateTransform();

                 // When slide out completes, start slide in
                 slideOutStoryboard.Completed += (s, e) =>
                 {
                     // Hide current page and show target page
                     currentPage.Visibility = Visibility.Collapsed;
                     currentPage.Opacity = 1.0; // Reset opacity
                     ((TranslateTransform)currentPage.RenderTransform).Y = 0; // Reset transform
                     
                     targetPage.Visibility = Visibility.Visible;
                     targetPage.Opacity = 0.0;
                     
                     // Ensure target page has a TranslateTransform
                     if (targetPage.RenderTransform == null || !(targetPage.RenderTransform is TranslateTransform))
                         targetPage.RenderTransform = new TranslateTransform();
                     
                     ((TranslateTransform)targetPage.RenderTransform).Y = slideInStartPosition; // Start from calculated position

                     // Create slide in and fade in storyboard for target page
                     var slideInStoryboard = new Storyboard();
                     
                     var fadeIn = new DoubleAnimation
                      {
                          From = 0.0,
                          To = 1.0,
                          Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                          EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                      };
                     Storyboard.SetTarget(fadeIn, targetPage);
                     Storyboard.SetTargetProperty(fadeIn, "Opacity");
                     slideInStoryboard.Children.Add(fadeIn);

                     var slideIn = new DoubleAnimation
                      {
                          From = slideInStartPosition,
                          To = 0.0,
                          Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                          EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                      };
                     Storyboard.SetTarget(slideIn, targetPage);
                     Storyboard.SetTargetProperty(slideIn, "(UIElement.RenderTransform).(TranslateTransform.Y)");
                     slideInStoryboard.Children.Add(slideIn);
                     
                     slideInStoryboard.Begin();
                 };

                 slideOutStoryboard.Begin();
             }
            else if (targetPage != null)
             {
                 // No current page, just show target page with slide up and fade in
                 FanPage.Visibility = Visibility.Collapsed;
                 GraphicsPage.Visibility = Visibility.Collapsed;
                 KeyboardPage.Visibility = Visibility.Collapsed;
                 MonitorPage.Visibility = Visibility.Collapsed;
                 SettingsPage.Visibility = Visibility.Collapsed;

                 targetPage.Visibility = Visibility.Visible;
                 targetPage.Opacity = 0.0;
                 
                 // Ensure target page has a TranslateTransform
                 if (targetPage.RenderTransform == null || !(targetPage.RenderTransform is TranslateTransform))
                     targetPage.RenderTransform = new TranslateTransform();
                 
                 ((TranslateTransform)targetPage.RenderTransform).Y = 30.0; // Start from below

                 var slideInStoryboard = new Storyboard();
                 
                 var fadeIn = new DoubleAnimation
                  {
                      From = 0.0,
                      To = 1.0,
                      Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                      EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                  };
                 Storyboard.SetTarget(fadeIn, targetPage);
                 Storyboard.SetTargetProperty(fadeIn, "Opacity");
                 slideInStoryboard.Children.Add(fadeIn);

                 var slideIn = new DoubleAnimation
                  {
                      From = 30.0,
                      To = 0.0,
                      Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                      EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                  };
                 Storyboard.SetTarget(slideIn, targetPage);
                 Storyboard.SetTargetProperty(slideIn, "(UIElement.RenderTransform).(TranslateTransform.Y)");
                 slideInStoryboard.Children.Add(slideIn);
                 
                 slideInStoryboard.Begin();
             }
        }

        // Settings Page Event Handlers
        private void BackdropComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (BackdropComboBox?.SelectedItem is ComboBoxItem selectedItem)
                {
                    var backdrop = selectedItem.Tag?.ToString();
                    
                    // Apply the selected backdrop
                    ApplyBackdropSettings(backdrop);
                    
                    // Show/hide wallpaper selection based on backdrop choice
                    if (backdrop == "Image")
                    {
                        if (WallpaperPanel != null)
                        {
                            WallpaperPanel.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            LogError("WallpaperPanel is null when trying to show it");
                        }
                    }
                    else
                    {
                        if (WallpaperPanel != null)
                        {
                            WallpaperPanel.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            LogError("WallpaperPanel is null when trying to hide it");
                        }
                    }
                    
                    if (SettingsInfoBar != null)
                    {
                        SettingsInfoBar.IsOpen = true;
                        SettingsInfoBar.Message = $"Backdrop changed to {selectedItem.Content}.";
                        SettingsInfoBar.Severity = InfoBarSeverity.Success;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in BackdropComboBox_SelectionChanged: {ex.Message}");
                if (SettingsInfoBar != null)
                {
                    SettingsInfoBar.IsOpen = true;
                    SettingsInfoBar.Message = "An error occurred while changing backdrop settings.";
                    SettingsInfoBar.Severity = InfoBarSeverity.Error;
                }
            }
        }
        
        private void WallpaperBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Handle wallpaper selection
            if (sender is Border border && border.Tag is string wallpaperIndex)
            {
                // Apply selected wallpaper
                ApplyWallpaperSettings(wallpaperIndex);
            }
        }

        private void ApplyWallpaperSettings(string wallpaperIndex)
         {
             try
             {
                 // Apply the selected wallpaper as window background
                 var wallpaperPath = $"/Assets/wallpapers/{wallpaperIndex}.png";
                 var imageSource = new BitmapImage(new Uri($"ms-appx://{wallpaperPath}"));
                 
                 // Create ImageBrush for the background
                 var imageBrush = new ImageBrush
                 {
                     ImageSource = imageSource,
                     Stretch = Stretch.UniformToFill
                 };
                 
                 // Apply to the main grid background
                 if (this.Content is Grid mainGrid)
                 {
                     mainGrid.Background = imageBrush;
                 }
                 
                 LogInfo($"Wallpaper applied: {wallpaperPath}");
                 
                 if (SettingsInfoBar != null)
                 {
                     SettingsInfoBar.IsOpen = true;
                     SettingsInfoBar.Message = $"Wallpaper changed to wallpaper {wallpaperIndex}.";
                     SettingsInfoBar.Severity = InfoBarSeverity.Success;
                 }
             }
             catch (Exception ex)
             {
                 LogError($"Failed to apply wallpaper {wallpaperIndex}: {ex.Message}");
                 if (SettingsInfoBar != null)
                 {
                     SettingsInfoBar.IsOpen = true;
                     SettingsInfoBar.Message = $"Failed to apply wallpaper {wallpaperIndex}.";
                     SettingsInfoBar.Severity = InfoBarSeverity.Error;
                 }
             }
          }

         private void ApplyBackdropSettings(string backdropType)
          {
              try
              {
                  // Get the main grid once
                  Grid? mainGrid = this.Content as Grid;
                  
                  switch (backdropType)
                  {
                      case "Mica":
                          this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
                          // Clear any custom background
                          if (mainGrid != null)
                          {
                              mainGrid.Background = null;
                          }
                          break;
                      case "Acrylic":
                          this.SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
                          // Clear any custom background
                          if (mainGrid != null)
                          {
                              mainGrid.Background = null;
                          }
                          break;
                      case "Image":
                          // Remove system backdrop for image background
                          this.SystemBackdrop = null;
                          break;
                      default:
                          this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
                          // Clear any custom background
                          if (mainGrid != null)
                          {
                              mainGrid.Background = null;
                          }
                          break;
                  }
                  
                  LogInfo($"Backdrop changed to: {backdropType}");
              }
              catch (Exception ex)
              {
                  LogError($"Failed to apply backdrop {backdropType}: {ex.Message}");
              }
           }

        // Old BIOS event handlers removed

        // All old BIOS event handlers removed

        // Old task management event handlers removed

        // All old task management and debug event handlers removed

        // All old system info and about section event handlers removed

        // Console functionality
        private void ClearConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            ConsoleOutput.Text = "HP Gaming Hub Console - Ready\n";
        }

        private void CopyConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                dataPackage.SetText(ConsoleOutput.Text);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                
                // Optional: Show a brief confirmation
                AppendToConsole("Console content copied to clipboard", "INFO");
            }
            catch (Exception ex)
            {
                AppendToConsole($"Failed to copy to clipboard: {ex.Message}", "ERROR");
            }
        }

        public void AppendToConsole(string message, string level = "INFO")
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] [{level}] {message}\n";
                ConsoleOutput.Text += logEntry;
                
                // Auto-scroll to bottom if enabled
                if (AutoScrollToggle.IsChecked == true)
                {
                    ConsoleScrollViewer.ScrollToVerticalOffset(ConsoleScrollViewer.ScrollableHeight);
                }
            });
        }

        public void LogDebug(string message)
        {
            AppendToConsole(message, "DEBUG");
            Debug.WriteLine($"[DEBUG] {message}");
        }

        public void LogInfo(string message)
        {
            AppendToConsole(message, "INFO");
            Debug.WriteLine($"[INFO] {message}");
        }

        public void LogWarning(string message)
        {
            AppendToConsole(message, "WARNING");
            Debug.WriteLine($"[WARNING] {message}");
        }

        public void LogError(string message)
        {
            AppendToConsole(message, "ERROR");
            Debug.WriteLine($"[ERROR] {message}");
        }

        private async void CustomTitleBarButton_Click(object sender, RoutedEventArgs e)
        {
            // Quick settings action - navigate to settings page
            if (MainNavigationView != null && SettingsNavItem != null)
            {
                MainNavigationView.SelectedItem = SettingsNavItem;
                LogInfo("Quick Settings button clicked - navigated to Settings page");
            }
        }
    }
}
