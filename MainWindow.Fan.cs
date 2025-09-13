using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Threading.Tasks;

namespace HP_Gaming_Hub
{
    public partial class MainWindow
    {
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

        private async Task ApplyFanModeAsync(string mode)
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
    }
}