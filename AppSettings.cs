using System;
using System.IO;
using System.Text.Json;
using Windows.Storage;

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
            get => _settings.IsAppDevelopment;
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
                var omenMonPath = Path.Combine(localFolder.Path, "OmenMon.exe");
                var omenMonXmlPath = Path.Combine(localFolder.Path, "OmenMon.xml");
                
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
            return Path.Combine(localFolder.Path, "OmenMon.exe");
        }
        
        public string GetOmenMonXmlPath()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            return Path.Combine(localFolder.Path, "OmenMon.xml");
        }
    }
    
    public class SettingsData
    {
        public bool IsWelcomeCompleted { get; set; } = false;
        public bool IsAppDevelopment { get; set; } = false;
    }
}