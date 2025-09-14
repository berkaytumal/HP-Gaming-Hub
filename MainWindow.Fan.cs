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
        private void FanModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton && clickedButton.Tag is string mode)
            {
                // Update button selection visual state
                UpdateFanModeButtonSelection(clickedButton);
                
                // Hide manual controls since we removed Manual mode
                if (ManualFanControlPanel != null)
                    ManualFanControlPanel.Visibility = Visibility.Collapsed;
                if (FanControlInfoBar != null)
                    FanControlInfoBar.IsOpen = false;
                    
                // Apply the selected fan mode
                _ = ApplyFanModeAsync(mode);
            }
        }
        
        private void UpdateFanModeButtonSelection(Button selectedButton)
        {
            // Reset all buttons to default style
            var defaultStyle = (Style)Application.Current.Resources["DefaultButtonStyle"];
            
            if (QuietModeButton != null)
            {
                QuietModeButton.Style = defaultStyle;
            }
            if (AutoModeButton != null)
            {
                AutoModeButton.Style = defaultStyle;
            }
            if (MaxModeButton != null)
            {
                MaxModeButton.Style = defaultStyle;
            }
                
            // Set selected button to accent style
            if (selectedButton != null)
            {
                selectedButton.Style = (Style)Application.Current.Resources["AccentButtonStyle"];
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
                
                // Reset to Auto mode (Default)
                if (AutoModeButton != null)
                    UpdateFanModeButtonSelection(AutoModeButton);
                    
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




    }
}