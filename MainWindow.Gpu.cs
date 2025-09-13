using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Threading.Tasks;

namespace HP_Gaming_Hub
{
    public partial class MainWindow
    {
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
    }
}