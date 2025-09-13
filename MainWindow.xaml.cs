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
            
            // Load and apply saved settings on startup
            Debug.WriteLine("[MainWindow] Loading saved settings on startup");
            LoadSettingsPagePreferences();
            
            InitializeMonitoring();
        }

        // InitializeMonitoring method moved to MainWindow.Core.cs

        // Utility methods moved to MainWindow.Core.cs

        // UpdateMonitoringUI method moved to MainWindow.Core.cs

        // Monitoring event handlers moved to MainWindow.Monitor.cs

        // Fan Control Event Handlers
        // Fan control event handlers moved to MainWindow.Fan.cs

        // Additional fan event handlers moved to MainWindow.Fan.cs

        // GPU Settings Event Handlers
        // GPU Settings Event Handlers moved to MainWindow.Gpu.cs

        // UpdateGpuUI method moved to MainWindow.Core.cs

        // Keyboard Settings Event Handlers
        // Keyboard lighting event handlers moved to MainWindow.Keyboard.cs

        // Additional keyboard event handlers moved to MainWindow.Keyboard.cs

        // Keyboard helper methods moved to MainWindow.Keyboard.cs

        // RefreshFanDataButton_Click and FanMonitoringToggle_Click moved to MainWindow.Fan.cs

        // UpdateFanUI method moved to MainWindow.Core.cs

        // Navigation event handler moved to MainWindow.Settings.cs

        // Settings event handlers moved to MainWindow.Settings.cs

        // Old BIOS event handlers removed

        // All old BIOS event handlers removed

        // Old task management event handlers removed

        // All old task management and debug event handlers removed

        // All old system info and about section event handlers removed

        // Console functionality and custom title bar button moved to MainWindow.Settings.cs
    }
}
