using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using HP_Gaming_Hub.Services;
using System.Threading.Tasks;
using Windows.UI;
namespace HP_Gaming_Hub
{
    public partial class MainWindow
    {
        // Keyboard Lighting Event Handlers
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
                // KeyboardSettingsInfoBar.IsOpen = true;
                // KeyboardSettingsInfoBar.Message = "Keyboard data refreshed successfully.";
                // KeyboardSettingsInfoBar.Severity = InfoBarSeverity.Success;
            }
            else
            {
                // KeyboardSettingsInfoBar.IsOpen = true;
                // KeyboardSettingsInfoBar.Message = "Failed to refresh keyboard data. Please check the error messages and try again.";
                // KeyboardSettingsInfoBar.Severity = InfoBarSeverity.Error;
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

        // Helper methods for keyboard functionality
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
    }
}