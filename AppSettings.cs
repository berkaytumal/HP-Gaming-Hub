using System;
using System.IO;
using System.Text.Json;
using Windows.Storage;
using Microsoft.Extensions.Configuration;

namespace HP_Gaming_Hub
{
    public class AppSettings
    {
        private static AppSettings? _instance;
        private static readonly object _lock = new object();
        
        public static AppSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new AppSettings();
                    }
                }
                return _instance;
            }
        }
        
        private const string SettingsFileName = "appsettings.json";
        private SettingsData _settings;
        
        private AppSettings()
        {
            LoadSettings();
        }
        
        public bool IsWelcomeCompleted
        {
            get => _settings.IsWelcomeCompleted;
            set
            {
                _settings.IsWelcomeCompleted = value;
                SaveSettings();
            }
        }
        
        public bool IsAppDevelopment
        {
            get => _settings.IsAppDevelopment || GetAppConfigValue("ForceWelcomeScreen", false);
            set
            {
                _settings.IsAppDevelopment = value;
                SaveSettings();
            }
        }
        
        public bool IsOmenMonDownloaded => CheckOmenMonExists();
        
        private void LoadSettings()
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var settingsPath = Path.Combine(localFolder.Path, SettingsFileName);
                
                if (File.Exists(settingsPath))
                {
                    var json = File.ReadAllText(settingsPath);
                    _settings = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
                }
                else
                {
                    _settings = new SettingsData();
                }
            }
            catch
            {
                _settings = new SettingsData();
            }
        }
        
        private void SaveSettings()
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var settingsPath = Path.Combine(localFolder.Path, SettingsFileName);
                
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsPath, json);
            }
            catch
            {
                // Ignore save errors for now
            }
        }
        
        private bool CheckOmenMonExists()
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var omenMonPath = Path.Combine(localFolder.Path, "Dependencies", "OmenMon", "OmenMon.exe");
                var omenMonXmlPath = Path.Combine(localFolder.Path, "Dependencies", "OmenMon", "OmenMon.xml");
                
                return File.Exists(omenMonPath) && File.Exists(omenMonXmlPath);
            }
            catch
            {
                return false;
            }
        }
        
        public string GetOmenMonPath()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            return Path.Combine(localFolder.Path, "Dependencies", "OmenMon", "OmenMon.exe");
        }
        
        public string GetOmenMonXmlPath()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            return Path.Combine(localFolder.Path, "Dependencies", "OmenMon", "OmenMon.xml");
        }
        
        public bool HideCmdWindows => GetProcessConfigValue("HideCmdWindows", true);
        
        // Settings Page Preferences
        public int BackdropSelectedIndex
        {
            get => _settings.SettingsPage.BackdropSelectedIndex;
            set
            {
                _settings.SettingsPage.BackdropSelectedIndex = value;
                SaveSettings();
            }
        }
        
        public int SelectedWallpaperIndex
        {
            get => _settings.SettingsPage.SelectedWallpaperIndex;
            set
            {
                _settings.SettingsPage.SelectedWallpaperIndex = value;
                SaveSettings();
            }
        }
        
        public void LoadSettingsPagePreferences()
        {
            // This method can be called when navigating to settings page
            // The preferences are already loaded in the constructor
        }
        
        private bool GetAppConfigValue(string key, bool defaultValue)
        {
            try
            {
                var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (File.Exists(appSettingsPath))
                {
                    var json = File.ReadAllText(appSettingsPath);
                    using var document = JsonDocument.Parse(json);
                    
                    if (document.RootElement.TryGetProperty("AppConfiguration", out var appConfig) &&
                        appConfig.TryGetProperty(key, out var property))
                    {
                        return property.GetBoolean();
                    }
                }
            }
            catch
            {
                // Ignore errors and return default
            }
            return defaultValue;
        }
        
        private bool GetProcessConfigValue(string key, bool defaultValue)
        {
            try
            {
                var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (File.Exists(appSettingsPath))
                {
                    var json = File.ReadAllText(appSettingsPath);
                    using var document = JsonDocument.Parse(json);
                    
                    if (document.RootElement.TryGetProperty("ProcessSettings", out var processConfig) &&
                        processConfig.TryGetProperty(key, out var property))
                    {
                        return property.GetBoolean();
                    }
                }
            }
            catch
            {
                // Ignore errors and return default
            }
            return defaultValue;
        }
    }
    
    public class SettingsData
    {
        public bool IsWelcomeCompleted { get; set; } = false;
        public bool IsAppDevelopment { get; set; } = false;
        public SettingsPagePreferences SettingsPage { get; set; } = new SettingsPagePreferences();
    }
    
    public class SettingsPagePreferences
    {
        public int BackdropSelectedIndex { get; set; } = 0;
        public int SelectedWallpaperIndex { get; set; } = 0;
    }
}