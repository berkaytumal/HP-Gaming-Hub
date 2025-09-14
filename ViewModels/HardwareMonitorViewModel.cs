using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HP_Gaming_Hub.Services;
using Microsoft.UI.Dispatching;
using System.Collections.Generic;
using System.Linq;

namespace HP_Gaming_Hub.ViewModels
{
    public class HardwareMonitorViewModel : INotifyPropertyChanged
    {
        private readonly OmenMonService _omenMonService;
        private readonly DispatcherQueueTimer _omenTimer; // Timer for OmenMon (5 seconds)
        private bool _isMonitoring;
        private bool _isUpdating; // Flag to prevent concurrent updates
        private DateTime _lastOmenUpdate = DateTime.MinValue;

        // Temperature properties
        private int _cpuTemperature;
        private int _gpuTemperature;
        private string _temperatureStatus = "Normal";

        // Fan properties
        private int _fan1Speed;
        private int _fan2Speed;
        private string _fanMode = "Auto";
        private int _fanCount;

        // GPU properties
        private string _gpuMode = "Unknown";
        private string _gpuPreset = "Unknown";

        // System properties
        private string _systemInfo = "Loading...";
        private bool _hasOverclock;
        private bool _hasMemoryOverclock;
        private bool _hasUndervolt;

        // Keyboard properties
        private bool _hasBacklight;
        private bool _backlightEnabled;
        private string _currentColor = "#FFFFFF";

        // Status properties
        private bool _isConnected;
        private string _lastUpdateTime = "Never";
        private string _connectionStatus = "Disconnected";

        public HardwareMonitorViewModel()
        {
            _omenMonService = new OmenMonService();
            
            // Setup timer for unified OmenMon polling
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            
            // OmenMon timer - 5 seconds for all hardware monitoring
            _omenTimer = dispatcherQueue.CreateTimer();
            _omenTimer.Interval = TimeSpan.FromSeconds(5);
            _omenTimer.Tick += async (s, e) => await UpdateOmenDataAsync();
        }

        #region Temperature Properties
        public int CpuTemperature
        {
            get => _cpuTemperature;
            set
            {
                if (SetProperty(ref _cpuTemperature, value))
                {
                    OnPropertyChanged(nameof(CpuTemperatureText));
                    UpdateTemperatureStatus();
                }
            }
        }

        public int GpuTemperature
        {
            get => _gpuTemperature;
            set
            {
                if (SetProperty(ref _gpuTemperature, value))
                {
                    OnPropertyChanged(nameof(GpuTemperatureText));
                    UpdateTemperatureStatus();
                }
            }
        }

        public string CpuTemperatureText => $"{CpuTemperature}°C";
        public string GpuTemperatureText => $"{GpuTemperature}°C";

        public string TemperatureStatus
        {
            get => _temperatureStatus;
            set => SetProperty(ref _temperatureStatus, value);
        }
        #endregion

        #region Fan Properties
        public int Fan1Speed
        {
            get => _fan1Speed;
            set
            {
                if (SetProperty(ref _fan1Speed, value))
                {
                    OnPropertyChanged(nameof(Fan1SpeedText));
                    OnPropertyChanged(nameof(Fan1SpeedPercentage));
                }
            }
        }

        public int Fan2Speed
        {
            get => _fan2Speed;
            set
            {
                if (SetProperty(ref _fan2Speed, value))
                {
                    OnPropertyChanged(nameof(Fan2SpeedText));
                    OnPropertyChanged(nameof(Fan2SpeedPercentage));
                }
            }
        }

        public string Fan1SpeedText => $"{Fan1Speed} RPM";
        public string Fan2SpeedText => $"{Fan2Speed} RPM";
        public double Fan1SpeedPercentage => (Fan1Speed / 255.0) * 100;
        public double Fan2SpeedPercentage => (Fan2Speed / 255.0) * 100;

        public string FanMode
        {
            get => _fanMode;
            set => SetProperty(ref _fanMode, value);
        }

        public int FanCount
        {
            get => _fanCount;
            set => SetProperty(ref _fanCount, value);
        }
        #endregion

        #region GPU Properties
        public string GpuMode
        {
            get => _gpuMode;
            set => SetProperty(ref _gpuMode, value);
        }

        public string GpuPreset
        {
            get => _gpuPreset;
            set => SetProperty(ref _gpuPreset, value);
        }
        #endregion

        #region System Properties
        public string SystemInfo
        {
            get => _systemInfo;
            set => SetProperty(ref _systemInfo, value);
        }

        public bool HasOverclock
        {
            get => _hasOverclock;
            set => SetProperty(ref _hasOverclock, value);
        }

        public bool HasMemoryOverclock
        {
            get => _hasMemoryOverclock;
            set => SetProperty(ref _hasMemoryOverclock, value);
        }

        public bool HasUndervolt
        {
            get => _hasUndervolt;
            set => SetProperty(ref _hasUndervolt, value);
        }
        #endregion

        #region Keyboard Properties
        public bool HasBacklight
        {
            get => _hasBacklight;
            set => SetProperty(ref _hasBacklight, value);
        }

        public bool BacklightEnabled
        {
            get => _backlightEnabled;
            set => SetProperty(ref _backlightEnabled, value);
        }

        public string CurrentColor
        {
            get => _currentColor;
            set => SetProperty(ref _currentColor, value);
        }
        #endregion

        #region Status Properties
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (SetProperty(ref _isConnected, value))
                {
                    ConnectionStatus = value ? "Connected" : "Disconnected";
                }
            }
        }

        public string LastUpdateTime
        {
            get => _lastUpdateTime;
            set => SetProperty(ref _lastUpdateTime, value);
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        public bool IsMonitoring
        {
            get => _isMonitoring;
            set => SetProperty(ref _isMonitoring, value);
        }
        
        // Fan control properties
        private int? _fan1Level;
        private int? _fan2Level;
        private int _fan1MaxSpeed;
        private int _fan2MaxSpeed;

        public int? Fan1Level
        {
            get => _fan1Level;
            set => SetProperty(ref _fan1Level, value);
        }

        public int? Fan2Level
        {
            get => _fan2Level;
            set => SetProperty(ref _fan2Level, value);
        }

        public int Fan1MaxSpeed
        {
            get => _fan1MaxSpeed;
            set => SetProperty(ref _fan1MaxSpeed, value);
        }

        public int Fan2MaxSpeed
        {
            get => _fan2MaxSpeed;
            set => SetProperty(ref _fan2MaxSpeed, value);
        }

        // GPU control properties (GpuMode and GpuPreset already defined above)
        private int _cpuPL1;
        private int _cpuPL4;
        private int _gpuPowerLimit;
        private bool _xmpEnabled;

        public int CpuPL1
        {
            get => _cpuPL1;
            set => SetProperty(ref _cpuPL1, value);
        }

        public int CpuPL4
        {
            get => _cpuPL4;
            set => SetProperty(ref _cpuPL4, value);
        }

        public int GpuPowerLimit
        {
            get => _gpuPowerLimit;
            set => SetProperty(ref _gpuPowerLimit, value);
        }

        public bool XmpEnabled
        {
            get => _xmpEnabled;
            set => SetProperty(ref _xmpEnabled, value);
        }

        // Keyboard Control Properties
        private bool _keyboardBacklightEnabled;
        public bool KeyboardBacklightEnabled
        {
            get => _keyboardBacklightEnabled;
            set => SetProperty(ref _keyboardBacklightEnabled, value);
        }

        private string _keyboardColorPreset = "Rainbow";
        public string KeyboardColorPreset
        {
            get => _keyboardColorPreset;
            set => SetProperty(ref _keyboardColorPreset, value);
        }

        private int _zone1Red = 255;
        public int Zone1Red
        {
            get => _zone1Red;
            set => SetProperty(ref _zone1Red, value);
        }

        private int _zone1Green = 0;
        public int Zone1Green
        {
            get => _zone1Green;
            set => SetProperty(ref _zone1Green, value);
        }

        private int _zone1Blue = 0;
        public int Zone1Blue
        {
            get => _zone1Blue;
            set => SetProperty(ref _zone1Blue, value);
        }

        private int _zone2Red = 0;
        public int Zone2Red
        {
            get => _zone2Red;
            set => SetProperty(ref _zone2Red, value);
        }

        private int _zone2Green = 255;
        public int Zone2Green
        {
            get => _zone2Green;
            set => SetProperty(ref _zone2Green, value);
        }

        private int _zone2Blue = 0;
        public int Zone2Blue
        {
            get => _zone2Blue;
            set => SetProperty(ref _zone2Blue, value);
        }

        private int _zone3Red = 0;
        public int Zone3Red
        {
            get => _zone3Red;
            set => SetProperty(ref _zone3Red, value);
        }

        private int _zone3Green = 0;
        public int Zone3Green
        {
            get => _zone3Green;
            set => SetProperty(ref _zone3Green, value);
        }

        private int _zone3Blue = 255;
        public int Zone3Blue
        {
            get => _zone3Blue;
            set => SetProperty(ref _zone3Blue, value);
        }

        private int _zone4Red = 255;
        public int Zone4Red
        {
            get => _zone4Red;
            set => SetProperty(ref _zone4Red, value);
        }

        private int _zone4Green = 255;
        public int Zone4Green
        {
            get => _zone4Green;
            set => SetProperty(ref _zone4Green, value);
        }

        private int _zone4Blue = 0;
        public int Zone4Blue
        {
            get => _zone4Blue;
            set => SetProperty(ref _zone4Blue, value);
        }

        private string _keyboardAnimation = "Static";
        public string KeyboardAnimation
        {
            get => _keyboardAnimation;
            set => SetProperty(ref _keyboardAnimation, value);
        }
        #endregion

        #region Commands and Methods
        public async Task StartMonitoringAsync()
        {
            if (_isMonitoring) return;

            IsMonitoring = true;
            await UpdateOmenDataAsync(); // Initial update
            _omenTimer.Start();
        }

        public void StopMonitoring()
        {
            if (!_isMonitoring) return;

            IsMonitoring = false;
            _omenTimer.Stop();
        }

        public async Task<bool> RefreshDataAsync()
        {
            try
            {
                await UpdateHardwareDataAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task SetFanSpeedAsync(int fan1, int fan2)
        {
            var success = await _omenMonService.SetFanLevelsAsync(fan1, fan2);
            if (success)
            {
                // Update local values immediately for responsiveness
                Fan1Speed = fan1;
                Fan2Speed = fan2;
            }
        }

        // Removed duplicate methods - using the newer versions with proper error handling

        public async Task SetKeyboardColorAsync(string color)
        {
            var success = await _omenMonService.SetKeyboardColorAsync(color);
            if (success)
            {
                CurrentColor = color;
            }
        }

        public async Task<bool> SetFanLevelsAsync(int fan1Level, int fan2Level)
        {
            try
            {
                var result = await _omenMonService.SetFanLevelsAsync(fan1Level, fan2Level);
                if (result)
                {
                    Fan1Level = fan1Level;
                    Fan2Level = fan2Level;
                    OnPropertyChanged(nameof(Fan1Level));
                    OnPropertyChanged(nameof(Fan2Level));
                    await RefreshDataAsync();
                }
                return result;
            }
            catch (Exception ex)
            {
                // Handle error appropriately
                return false;
            }
        }

        // Removed duplicate methods - using the newer versions with proper error handling

        public async Task<bool> SetCpuPowerLimitsAsync(int pl1, int pl4)
        {
            try
            {
                var result = await _omenMonService.SetCpuPowerLimitsAsync(pl1, pl4);
                if (result)
                {
                    CpuPL1 = pl1;
                    CpuPL4 = pl4;
                }
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SetGpuPowerLimitAsync(int powerLimit)
        {
            try
            {
                var result = await _omenMonService.SetGpuPowerLimitAsync(powerLimit);
                if (result)
                {
                    GpuPowerLimit = powerLimit;
                }
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SetXmpAsync(bool enabled)
        {
            try
            {
                var result = await _omenMonService.SetXmpAsync(enabled);
                if (result)
                {
                    XmpEnabled = enabled;
                }
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Keyboard Control Methods
        public async Task<bool> SetKeyboardBacklightAsync(bool enabled)
        {
            var result = await _omenMonService.SetKeyboardBacklightAsync(enabled);
            if (result)
            {
                KeyboardBacklightEnabled = enabled;
            }
            return result;
        }

        public async Task<bool> SetKeyboardColorPresetAsync(string preset)
        {
            var result = await _omenMonService.SetKeyboardColorPresetAsync(preset);
            if (result)
            {
                KeyboardColorPreset = preset;
            }
            return result;
        }

        public async Task<bool> SetKeyboardZoneColorsAsync(int zone1R, int zone1G, int zone1B, int zone2R, int zone2G, int zone2B, int zone3R, int zone3G, int zone3B, int zone4R, int zone4G, int zone4B)
        {
            var result = await _omenMonService.SetKeyboardZoneColorsAsync(zone1R, zone1G, zone1B, zone2R, zone2G, zone2B, zone3R, zone3G, zone3B, zone4R, zone4G, zone4B);
            if (result)
            {
                Zone1Red = zone1R; Zone1Green = zone1G; Zone1Blue = zone1B;
                Zone2Red = zone2R; Zone2Green = zone2G; Zone2Blue = zone2B;
                Zone3Red = zone3R; Zone3Green = zone3G; Zone3Blue = zone3B;
                Zone4Red = zone4R; Zone4Green = zone4G; Zone4Blue = zone4B;
            }
            return result;
        }

        public async Task<bool> SetKeyboardAnimationAsync(string animation)
        {
            var result = await _omenMonService.SetKeyboardAnimationAsync(animation);
            if (result)
            {
                KeyboardAnimation = animation;
            }
            return result;
        }

        public async Task<bool> ResetKeyboardColorsAsync()
        {
            var result = await _omenMonService.ResetKeyboardColorsAsync();
            if (result)
            {
                Zone1Red = 255; Zone1Green = 0; Zone1Blue = 0;
                Zone2Red = 0; Zone2Green = 255; Zone2Blue = 0;
                Zone3Red = 0; Zone3Green = 0; Zone3Blue = 255;
                Zone4Red = 255; Zone4Green = 255; Zone4Blue = 0;
                KeyboardColorPreset = "Custom";
            }
            return result;
        }

        public async Task<bool> SetKeyboardCustomColorsAsync(int[] colors)
        {
            if (colors == null || colors.Length != 4)
            {
                return false;
            }

            try
            {
                // Convert hex colors to RGB values
                var zone1R = (colors[0] >> 16) & 0xFF;
                var zone1G = (colors[0] >> 8) & 0xFF;
                var zone1B = colors[0] & 0xFF;

                var zone2R = (colors[1] >> 16) & 0xFF;
                var zone2G = (colors[1] >> 8) & 0xFF;
                var zone2B = colors[1] & 0xFF;

                var zone3R = (colors[2] >> 16) & 0xFF;
                var zone3G = (colors[2] >> 8) & 0xFF;
                var zone3B = colors[2] & 0xFF;

                var zone4R = (colors[3] >> 16) & 0xFF;
                var zone4G = (colors[3] >> 8) & 0xFF;
                var zone4B = colors[3] & 0xFF;

                // Call the existing SetKeyboardZoneColorsAsync method
                var result = await SetKeyboardZoneColorsAsync(
                    zone1R, zone1G, zone1B,
                    zone2R, zone2G, zone2B,
                    zone3R, zone3G, zone3B,
                    zone4R, zone4G, zone4B
                );

                return result;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetKeyboardSettingsAsync()
        {
            try
            {
                // Reset keyboard colors and settings
                var result = await ResetKeyboardColorsAsync();
                if (result)
                {
                    // Reset additional keyboard settings
                    KeyboardBacklightEnabled = true;
                    KeyboardColorPreset = "Rainbow";
                    KeyboardAnimation = "Static";
                }
                return result;
            }
            catch
            {
                return false;
            }
        }



        /// <summary>
        /// Update all hardware data using unified OmenMon command - called every 5 seconds
        /// </summary>
        private async Task UpdateOmenDataAsync()
        {
            if (_isUpdating) return;

            try
            {
                _isUpdating = true;
                System.Diagnostics.Debug.WriteLine("[UpdateOmenDataAsync] Starting unified OmenMon hardware data update");
                
                // Get all hardware data in one unified call
                var (tempData, fanData, gpuData, keyboardData) = await _omenMonService.GetUnifiedHardwareDataAsync();
                
                // Update temperatures - only if we get valid data
                if (tempData != null && (tempData.CpuTemperature > 0 || tempData.GpuTemperature > 0))
                {
                    CpuTemperature = tempData.CpuTemperature;
                    GpuTemperature = tempData.GpuTemperature;
                    System.Diagnostics.Debug.WriteLine($"[UpdateOmenDataAsync] Updated temperatures from unified call - CPU: {CpuTemperature}°C, GPU: {GpuTemperature}°C");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[UpdateOmenDataAsync] Unified call returned no valid temperature data, keeping existing values");
                }
                _lastOmenUpdate = DateTime.Now;
                
                // Update fan data
                Fan1Speed = fanData.Fan1Speed;
                Fan2Speed = fanData.Fan2Speed;
                FanCount = fanData.FanCount;
                FanMode = fanData.FanMode;
                
                // Update GPU data
                GpuMode = gpuData.GpuMode;
                GpuPreset = gpuData.GpuPreset;
                
                // Update keyboard data
                HasBacklight = keyboardData.HasBacklight;
                BacklightEnabled = keyboardData.BacklightEnabled;
                
                // Update system data (still need separate call for this)
                var systemData = await _omenMonService.GetSystemDataAsync();
                HasOverclock = systemData.HasOverclock;
                HasMemoryOverclock = systemData.HasMemoryOverclock;
                HasUndervolt = systemData.HasUndervolt;
                
                LastUpdateTime = DateTime.Now.ToString("HH:mm:ss");
                IsConnected = true;
                ConnectionStatus = "Connected";
                
                System.Diagnostics.Debug.WriteLine($"[UpdateOmenDataAsync] Unified OmenMon update completed - CPU: {CpuTemperature}°C, GPU: {GpuTemperature}°C, Fan1: {Fan1Speed}RPM");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateOmenDataAsync] Error: {ex.Message}");
                IsConnected = false;
                ConnectionStatus = "Disconnected";
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private async Task UpdateHardwareDataAsync()
        {
            // Prevent concurrent updates
            if (_isUpdating)
            {
                System.Diagnostics.Debug.WriteLine("[UpdateHardwareDataAsync] Update already in progress, skipping");
                return;
            }

            _isUpdating = true;
            try
            {
                System.Diagnostics.Debug.WriteLine("[UpdateHardwareDataAsync] Starting hardware data update");
            MainWindow.Instance?.LogDebug("Starting hardware data update");
                
                // Update temperatures - only if we get valid data
                System.Diagnostics.Debug.WriteLine("[UpdateHardwareDataAsync] Fetching temperature data");
                var tempData = await _omenMonService.GetTemperaturesAsync();
                if (tempData != null && (tempData.CpuTemperature > 0 || tempData.GpuTemperature > 0))
                {
                    CpuTemperature = tempData.CpuTemperature;
                    GpuTemperature = tempData.GpuTemperature;
                    System.Diagnostics.Debug.WriteLine($"[UpdateHardwareDataAsync] Updated temperatures - CPU: {CpuTemperature}°C, GPU: {GpuTemperature}°C");
                    MainWindow.Instance?.LogInfo($"Temperature update - CPU: {CpuTemperature}°C, GPU: {GpuTemperature}°C");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[UpdateHardwareDataAsync] No valid temperature data received, keeping existing values");
                    MainWindow.Instance?.LogInfo("Temperature update - No valid data received, keeping existing values");
                }

                // Update fan data
                System.Diagnostics.Debug.WriteLine("[UpdateHardwareDataAsync] Fetching fan data");
                var fanData = await _omenMonService.GetFanDataAsync();
                Fan1Speed = fanData.Fan1Speed;
                Fan2Speed = fanData.Fan2Speed;
                Fan1Level = fanData.Fan1Level;
                Fan2Level = fanData.Fan2Level;
                FanMode = fanData.FanMode;
                FanCount = fanData.FanCount;
                System.Diagnostics.Debug.WriteLine($"[UpdateHardwareDataAsync] Fan data - Fan1: {Fan1Speed} RPM, Fan2: {Fan2Speed} RPM, Mode: {FanMode}");

                // Update GPU data
                System.Diagnostics.Debug.WriteLine("[UpdateHardwareDataAsync] Fetching GPU data");
                var gpuData = await _omenMonService.GetGpuDataAsync();
                GpuMode = gpuData.GpuMode;
                GpuPreset = gpuData.GpuPreset;
                System.Diagnostics.Debug.WriteLine($"[UpdateHardwareDataAsync] GPU data - Mode: {GpuMode}, Preset: {GpuPreset}");

                // Update keyboard data
                System.Diagnostics.Debug.WriteLine("[UpdateHardwareDataAsync] Fetching keyboard data");
                var kbdData = await _omenMonService.GetKeyboardDataAsync();
                HasBacklight = kbdData.HasBacklight;
                BacklightEnabled = kbdData.BacklightEnabled;
                CurrentColor = kbdData.CurrentColor;
                System.Diagnostics.Debug.WriteLine($"[UpdateHardwareDataAsync] Keyboard data - HasBacklight: {HasBacklight}, Enabled: {BacklightEnabled}");

                // Update system data
                System.Diagnostics.Debug.WriteLine("[UpdateHardwareDataAsync] Fetching system data");
                var sysData = await _omenMonService.GetSystemDataAsync();
                SystemInfo = sysData.SystemInfo;
                HasOverclock = sysData.HasOverclock;
                HasMemoryOverclock = sysData.HasMemoryOverclock;
                HasUndervolt = sysData.HasUndervolt;
                System.Diagnostics.Debug.WriteLine($"[UpdateHardwareDataAsync] System data - Overclock: {HasOverclock}, MemOC: {HasMemoryOverclock}, Undervolt: {HasUndervolt}");

                // Update status
                IsConnected = true;
                LastUpdateTime = DateTime.Now.ToString("HH:mm:ss");
                System.Diagnostics.Debug.WriteLine($"[UpdateHardwareDataAsync] Hardware data update completed successfully at {LastUpdateTime}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateHardwareDataAsync] Error during hardware data update: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[UpdateHardwareDataAsync] Stack trace: {ex.StackTrace}");
                MainWindow.Instance?.LogError($"Hardware data update failed: {ex.Message}");
                IsConnected = false;
                LastUpdateTime = "Error";
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void UpdateTemperatureStatus()
        {
            var maxTemp = Math.Max(CpuTemperature, GpuTemperature);
            
            if (maxTemp >= 85)
                TemperatureStatus = "Critical";
            else if (maxTemp >= 75)
                TemperatureStatus = "High";
            else if (maxTemp >= 65)
                TemperatureStatus = "Warm";
            else
                TemperatureStatus = "Normal";
        }
        #endregion

        #region Settings Page Methods
        public async Task<bool> SetMaxFanAsync(bool enabled)
        {
            try
            {
                var result = await _omenMonService.SetMaxFanAsync(enabled);
                return result;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetOverclockAsync(bool enabled)
        {
            try
            {
                var result = await _omenMonService.SetOverclockAsync(enabled);
                if (result.IsSuccess)
                {
                    HasOverclock = enabled;
                }
                return result.IsSuccess;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetMemoryOverclockAsync(bool enabled)
        {
            try
            {
                var result = await _omenMonService.SetMemoryOverclockAsync(enabled);
                if (result.IsSuccess)
                {
                    HasMemoryOverclock = enabled;
                }
                return result.IsSuccess;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetUndervoltAsync(bool enabled)
        {
            try
            {
                var result = await _omenMonService.SetUndervoltAsync(enabled);
                if (result.IsSuccess)
                {
                    HasUndervolt = enabled;
                }
                return result.IsSuccess;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetFanModeAsync(string mode)
        {
            try
            {
                var result = await _omenMonService.SetFanModeAsync(mode);
                if (result)
                {
                    FanMode = mode;
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetGpuModeAsync(string mode)
        {
            try
            {
                var result = await _omenMonService.SetGpuModeAsync(mode);
                if (result)
                {
                    GpuMode = mode;
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetGpuPresetAsync(string preset)
        {
            try
            {
                var result = await _omenMonService.SetGpuPresetAsync(preset);
                if (result)
                {
                    GpuPreset = preset;
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CheckServiceStatusAsync()
        {
            try
            {
                var result = await _omenMonService.GetSystemInfoAsync();
                IsConnected = result.IsSuccess;
                ConnectionStatus = result.IsSuccess ? "Connected" : "Disconnected";
                return result.IsSuccess;
            }
            catch
            {
                IsConnected = false;
                ConnectionStatus = "Disconnected";
                return false;
            }
        }

        public async Task<bool> RefreshSystemInfoAsync()
        {
            try
            {
                var result = await _omenMonService.GetSystemInfoAsync();
                if (result.IsSuccess && result.Data != null)
                {
                    SystemInfo = result.Data;
                    LastUpdateTime = DateTime.Now.ToString("HH:mm:ss");
                }
                return result.IsSuccess;
            }
            catch
            {
                return false;
            }
        }

        // System information properties for Settings page
        private string _omenMonVersion = "Unknown";
        private string _systemBornDate = "Unknown";
        private string _adapterInfo = "Unknown";

        public string OmenMonVersion
        {
            get => _omenMonVersion;
            set => SetProperty(ref _omenMonVersion, value);
        }

        public string SystemBornDate
        {
            get => _systemBornDate;
            set => SetProperty(ref _systemBornDate, value);
        }

        public string AdapterInfo
        {
            get => _adapterInfo;
            set => SetProperty(ref _adapterInfo, value);
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        #region Cleanup
        public void Dispose()
        {
            StopMonitoring();
            _omenTimer?.Stop();
            _omenMonService?.Dispose();
        }
        #endregion
    }
}