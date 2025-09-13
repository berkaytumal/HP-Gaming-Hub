using System;
using System.IO;
using System.Text.Json;
using Windows.Storage;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

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
                        Debug.WriteLine("[AppSettings] Creating new AppSettings instance");
                        Console.WriteLine("[AppSettings] Creating new AppSettings instance");
                        _instance ??= new AppSettings();
                    }
                }
                return _instance;
            }
        }
        
        private const string SettingsFileName = "preferences.json";
        private SettingsData _settings;
        
        private AppSettings()
        {
            Debug.WriteLine("[AppSettings] Initializing AppSettings singleton instance");
            Console.WriteLine("[AppSettings] Initializing AppSettings singleton instance");
            LoadSettings();
        }
        
        public bool IsWelcomeCompleted
        {
            get => _settings.IsWelcomeCompleted;
            set
            {
                Debug.WriteLine($"[AppSettings] Setting IsWelcomeCompleted from {_settings.IsWelcomeCompleted} to {value}");
                _settings.IsWelcomeCompleted = value;
                SaveSettings();
            }
        }
        
        public bool IsAppDevelopment
        {
            get => _settings.IsAppDevelopment || GetAppConfigValue("ForceWelcomeScreen", false);
            set
            {
                Debug.WriteLine($"[AppSettings] Setting IsAppDevelopment from {_settings.IsAppDevelopment} to {value}");
                _settings.IsAppDevelopment = value;
                SaveSettings();
            }
        }
        
        public bool IsOmenMonDownloaded => CheckOmenMonExists();
        
        public int BackdropSelectedIndex
        {
            get => _settings.SettingsPage.BackdropSelectedIndex;
            set
            {
                Debug.WriteLine($"[AppSettings] Setting BackdropSelectedIndex from {_settings.SettingsPage.BackdropSelectedIndex} to {value}");
                _settings.SettingsPage.BackdropSelectedIndex = value;
                SaveSettings();
            }
        }
        
        public int SelectedWallpaperIndex
        {
            get => _settings.SettingsPage.SelectedWallpaperIndex;
            set
            {
                Debug.WriteLine($"[AppSettings] Setting SelectedWallpaperIndex from {_settings.SettingsPage.SelectedWallpaperIndex} to {value}");
                _settings.SettingsPage.SelectedWallpaperIndex = value;
                SaveSettings();
            }
        }
        
        private void LoadSettings()
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var settingsPath = Path.Combine(localFolder.Path, SettingsFileName);
                
                Debug.WriteLine($"[AppSettings] Loading settings from: {settingsPath}");
                Debug.WriteLine($"[AppSettings] Full preferences file path: {Path.GetFullPath(settingsPath)}");
                Console.WriteLine($"[AppSettings] Loading settings from: {settingsPath}");
                Console.WriteLine($"[AppSettings] Full preferences file path: {Path.GetFullPath(settingsPath)}");
                
                if (File.Exists(settingsPath))
                {
                    var json = File.ReadAllText(settingsPath);
                    Debug.WriteLine($"[AppSettings] Settings file content: {json}");
                    
                    _settings = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
                    
                    Debug.WriteLine($"[AppSettings] Loaded settings - BackdropSelectedIndex: {_settings.SettingsPage.BackdropSelectedIndex}, SelectedWallpaperIndex: {_settings.SettingsPage.SelectedWallpaperIndex}, IsWelcomeCompleted: {_settings.IsWelcomeCompleted}");
                }
                else
                {
                    Debug.WriteLine($"[AppSettings] Settings file does not exist, creating default settings");
                    _settings = new SettingsData();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppSettings] Error loading settings: {ex.Message}");
                _settings = new SettingsData();
            }
        }
        
        public void SaveSettings()
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var settingsPath = Path.Combine(localFolder.Path, SettingsFileName);
                
                Debug.WriteLine($"[AppSettings] Saving settings to: {settingsPath}");
                Debug.WriteLine($"[AppSettings] Full preferences file path: {Path.GetFullPath(settingsPath)}");
                Console.WriteLine($"[AppSettings] Saving settings to: {settingsPath}");
                Console.WriteLine($"[AppSettings] Full preferences file path: {Path.GetFullPath(settingsPath)}");
                Debug.WriteLine($"[AppSettings] Current settings - BackdropSelectedIndex: {_settings.SettingsPage.BackdropSelectedIndex}, SelectedWallpaperIndex: {_settings.SettingsPage.SelectedWallpaperIndex}, IsWelcomeCompleted: {_settings.IsWelcomeCompleted}");
                
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsPath, json);
                
                Debug.WriteLine($"[AppSettings] Settings saved successfully. File content: {json}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppSettings] Error saving settings: {ex.Message}");
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
        
        public void LoadSettingsPagePreferences()
        {
            Debug.WriteLine("[AppSettings] LoadSettingsPagePreferences called");
            Debug.WriteLine($"[AppSettings] Current preferences - BackdropSelectedIndex: {_settings.SettingsPage.BackdropSelectedIndex}, SelectedWallpaperIndex: {_settings.SettingsPage.SelectedWallpaperIndex}");
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
        private SettingsPagePreferences _settingsPage = new SettingsPagePreferences();
        public SettingsPagePreferences SettingsPage 
        { 
            get 
            {
                Debug.WriteLine("[AppSettings] Getting SettingsPage property");
                Console.WriteLine("[AppSettings] Getting SettingsPage property");
                return _settingsPage;
            }
            set 
            {
                Debug.WriteLine("[AppSettings] Setting SettingsPage property");
                Console.WriteLine("[AppSettings] Setting SettingsPage property");
                _settingsPage = value;
            }
        }
    }
    
    public class SettingsPagePreferences
    {
        private int _backdropSelectedIndex = 0;
        public int BackdropSelectedIndex 
        { 
            get 
            {
                Debug.WriteLine($"[AppSettings] Getting BackdropSelectedIndex: {_backdropSelectedIndex}");
                Console.WriteLine($"[AppSettings] Getting BackdropSelectedIndex: {_backdropSelectedIndex}");
                return _backdropSelectedIndex;
            }
            set 
            {
                Debug.WriteLine($"[AppSettings] Setting BackdropSelectedIndex from {_backdropSelectedIndex} to {value}");
                Console.WriteLine($"[AppSettings] Setting BackdropSelectedIndex from {_backdropSelectedIndex} to {value}");
                _backdropSelectedIndex = value;
            }
        }
        public int SelectedWallpaperIndex { get; set; } = 0;
    }
}