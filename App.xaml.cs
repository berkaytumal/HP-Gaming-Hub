using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HP_Gaming_Hub.Services;
using HP_Gaming_Hub.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HP_Gaming_Hub
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        private IHost? _host;

        public static IServiceProvider Services => ((App)Current)._host?.Services ?? throw new InvalidOperationException("Services not initialized");

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            InitializeServices();
        }

        private void InitializeServices()
        {
            var builder = Host.CreateApplicationBuilder();
            
            // Add configuration
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            
            // Register services
            builder.Services.AddSingleton<OmenMonService>();
            builder.Services.AddTransient<MainViewModel>();
            
            _host = builder.Build();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var appSettings = AppSettings.Instance;
            
            // Show welcome window if:
            // 1. Welcome hasn't been completed yet, OR
            // 2. App development mode is enabled (forces welcome screen)
            if (!appSettings.IsWelcomeCompleted || appSettings.IsAppDevelopment)
            {
                _window = new WelcomeWindow();
            }
            else
            {
                _window = new MainWindow();
            }
            
            _window.Activate();
        }
    }
}
