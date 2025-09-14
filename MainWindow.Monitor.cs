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


    }
}