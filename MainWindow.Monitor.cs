using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using HP_Gaming_Hub.Services;

namespace HP_Gaming_Hub
{
    /// <summary>
    /// MainWindow partial class containing monitoring-related event handlers
    /// </summary>
    public sealed partial class MainWindow : Window
    {
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
    }
}