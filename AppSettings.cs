using System;
using System.IO;
using System.Text.Json;
using Windows.Storage;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using Serilog;

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
                        Log.Debug("Creating new AppSettings instance");
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
            Log.Debug("Initializing AppSettings singleton instance");
            LoadSettings();
        }
        
        public bool IsWelcomeCompleted
        {
            get => _settings.IsWelcomeCompleted;
            set
            {
                Log.Debug("Setting IsWelcomeCompleted from {OldValue} to {NewValue}", _settings.IsWelcomeCompleted, value);
                _settings.IsWelcomeCompleted = value;
                SaveSettings();
            }
        }
        
        public bool IsAppDevelopment
        {
            get => _settings.IsAppDevelopment || GetAppConfigValue("ForceWelcomeScreen", false);
            set
            {
                Log.Debug("Setting IsAppDevelopment from {OldValue} to {NewValue}", _settings.IsAppDevelopment, value);
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
                Log.Debug("Setting BackdropSelectedIndex from {OldValue} to {NewValue}", _settings.SettingsPage.BackdropSelectedIndex, value);
                _settings.SettingsPage.BackdropSelectedIndex = value;
                SaveSettings();
            }
        }
        
        public int SelectedWallpaperIndex
        {
            get => _settings.SettingsPage.SelectedWallpaperIndex;
            set
            {
                Log.Debug("Setting SelectedWallpaperIndex from {OldValue} to {NewValue}", _settings.SettingsPage.SelectedWallpaperIndex, value);
                _settings.SettingsPage.SelectedWallpaperIndex = value;
                SaveSettings();
            }
        }
        
        // Monitor Settings Properties
        public bool AutoStartMonitoring
        {
            get => _settings.SettingsPage.AutoStartMonitoring;
            set
            {
                Log.Debug("Setting AutoStartMonitoring from {OldValue} to {NewValue}", _settings.SettingsPage.AutoStartMonitoring, value);
                _settings.SettingsPage.AutoStartMonitoring = value;
                SaveSettings();
            }
        }
        
        // AutoRefresh is now a runtime-only setting, not persisted to preferences
        // It gets enabled automatically when AutoStartMonitoring is true
        
        public int FocusedRefreshInterval
        {
            get => _settings.SettingsPage.FocusedRefreshInterval;
            set
            {
                Log.Debug("Setting FocusedRefreshInterval from {OldValue} to {NewValue}", _settings.SettingsPage.FocusedRefreshInterval, value);
                _settings.SettingsPage.FocusedRefreshInterval = value;
                SaveSettings();
            }
        }
        
        public int BlurredRefreshInterval
        {
            get => _settings.SettingsPage.BlurredRefreshInterval;
            set
            {
                Log.Debug("Setting BlurredRefreshInterval from {OldValue} to {NewValue}", _settings.SettingsPage.BlurredRefreshInterval, value);
                _settings.SettingsPage.BlurredRefreshInterval = value;
                SaveSettings();
            }
        }
        
        private void LoadSettings()
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var settingsPath = Path.Combine(localFolder.Path, SettingsFileName);
                
                Log.Debug("Loading settings from: {SettingsPath}", settingsPath);
                Log.Debug("Full preferences file path: {FullPath}", Path.GetFullPath(settingsPath));
                
                if (File.Exists(settingsPath))
                {
                    var json = File.ReadAllText(settingsPath);
                    Log.Debug("Settings file content: {Json}", json);
                    
                    _settings = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
                    
                    Log.Debug("Loaded settings - BackdropSelectedIndex: {BackdropIndex}, SelectedWallpaperIndex: {WallpaperIndex}, IsWelcomeCompleted: {WelcomeCompleted}", _settings.SettingsPage.BackdropSelectedIndex, _settings.SettingsPage.SelectedWallpaperIndex, _settings.IsWelcomeCompleted);
                }
                else
                {
                    Log.Debug("Settings file does not exist, creating default settings");
                    _settings = new SettingsData();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading settings");
                _settings = new SettingsData();
            }
        }
        
        public void SaveSettings()
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var settingsPath = Path.Combine(localFolder.Path, SettingsFileName);
                
                Log.Debug("Saving settings to: {SettingsPath}", settingsPath);
                Log.Debug("Full preferences file path: {FullPath}", Path.GetFullPath(settingsPath));
                Log.Debug("Current settings - BackdropSelectedIndex: {BackdropIndex}, SelectedWallpaperIndex: {WallpaperIndex}, IsWelcomeCompleted: {WelcomeCompleted}", _settings.SettingsPage.BackdropSelectedIndex, _settings.SettingsPage.SelectedWallpaperIndex, _settings.IsWelcomeCompleted);
                
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsPath, json);
                
                Log.Debug("Settings saved successfully. File content: {Json}", json);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving settings");
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
            Log.Debug("LoadSettingsPagePreferences called");
            Log.Debug("Current preferences - BackdropSelectedIndex: {BackdropIndex}, SelectedWallpaperIndex: {WallpaperIndex}", _settings.SettingsPage.BackdropSelectedIndex, _settings.SettingsPage.SelectedWallpaperIndex);
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
                Log.Debug("Getting SettingsPage property");
                return _settingsPage;
            }
            set 
            {
                Log.Debug("Setting SettingsPage property");
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
                Log.Debug("Getting BackdropSelectedIndex: {BackdropIndex}", _backdropSelectedIndex);
                return _backdropSelectedIndex;
            }
            set 
            {
                Log.Debug("Setting BackdropSelectedIndex from {OldValue} to {NewValue}", _backdropSelectedIndex, value);
                _backdropSelectedIndex = value;
            }
        }
        public int SelectedWallpaperIndex { get; set; } = 6;
        
        // Monitor Settings
        public bool AutoStartMonitoring { get; set; } = true;
        // AutoRefresh removed - now runtime-only setting
        public int FocusedRefreshInterval { get; set; } = 2;
        public int BlurredRefreshInterval { get; set; } = 10;
    }
}