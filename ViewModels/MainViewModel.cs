using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HP_Gaming_Hub.Services;
using Microsoft.UI.Dispatching;

namespace HP_Gaming_Hub.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly OmenMonService _omenMonService;
        private readonly DispatcherQueue _dispatcherQueue;

        // Fan Properties
        private FanInfo _fanInfo = new();
        private ObservableCollection<string> _fanPrograms = new();
        private string _selectedFanProgram = string.Empty;
        private bool _isFanControlEnabled = true;

        // GPU Properties
        private GpuInfo _gpuInfo = new();
        private string _selectedGpuMode = string.Empty;
        private string _selectedGpuPreset = string.Empty;

        // Keyboard Properties
        private KeyboardInfo _keyboardInfo = new();
        private bool _isBacklightEnabled;
        private string _selectedColor = string.Empty;

        // System Monitoring Properties
        private SystemInfo _systemInfo = new();
        private bool _isMonitoring;
        private string _connectionStatus = "Disconnected";

        public MainViewModel(OmenMonService omenMonService)
        {
            _omenMonService = omenMonService;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            
            // Initialize with default/placeholder data
            InitializeDefaultData();
            
            // Start initialization
            _ = InitializeAsync();
        }

        // Fan Properties
        public FanInfo FanInfo
        {
            get => _fanInfo;
            set => SetProperty(ref _fanInfo, value);
        }

        public ObservableCollection<string> FanPrograms
        {
            get => _fanPrograms;
            set => SetProperty(ref _fanPrograms, value);
        }

        public string SelectedFanProgram
        {
            get => _selectedFanProgram;
            set
            {
                if (SetProperty(ref _selectedFanProgram, value))
                {
                    _ = RunFanProgramAsync(value);
                }
            }
        }

        public bool IsFanControlEnabled
        {
            get => _isFanControlEnabled;
            set => SetProperty(ref _isFanControlEnabled, value);
        }

        // GPU Properties
        public GpuInfo GpuInfo
        {
            get => _gpuInfo;
            set => SetProperty(ref _gpuInfo, value);
        }

        public string SelectedGpuMode
        {
            get => _selectedGpuMode;
            set
            {
                if (SetProperty(ref _selectedGpuMode, value))
                {
                    _ = SetGpuModeAsync(value);
                }
            }
        }

        public string SelectedGpuPreset
        {
            get => _selectedGpuPreset;
            set
            {
                if (SetProperty(ref _selectedGpuPreset, value))
                {
                    _ = SetGpuPresetAsync(value);
                }
            }
        }

        // Keyboard Properties
        public KeyboardInfo KeyboardInfo
        {
            get => _keyboardInfo;
            set => SetProperty(ref _keyboardInfo, value);
        }

        public bool IsBacklightEnabled
        {
            get => _isBacklightEnabled;
            set
            {
                if (SetProperty(ref _isBacklightEnabled, value))
                {
                    _ = SetBacklightAsync(value);
                }
            }
        }

        public string SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (SetProperty(ref _selectedColor, value))
                {
                    _ = SetColorAsync(value);
                }
            }
        }

        // System Monitoring Properties
        public SystemInfo SystemInfo
        {
            get => _systemInfo;
            set => SetProperty(ref _systemInfo, value);
        }

        public bool IsMonitoring
        {
            get => _isMonitoring;
            set => SetProperty(ref _isMonitoring, value);
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        // Commands and Methods
        public async Task RefreshAllDataAsync()
        {
            await RefreshFanDataAsync();
            await RefreshGpuDataAsync();
            await RefreshKeyboardDataAsync();
            await RefreshSystemDataAsync();
        }

        public async Task RefreshFanDataAsync()
        {
            try
            {
                var fanInfo = await _omenMonService.GetFanInfoAsync();
                var fanPrograms = await _omenMonService.GetFanProgramsAsync();

                await _dispatcherQueue.EnqueueAsync(() =>
                {
                    FanInfo = fanInfo;
                    FanPrograms.Clear();
                    foreach (var program in fanPrograms)
                    {
                        FanPrograms.Add(program);
                    }
                });
            }
            catch (Exception ex)
            {
                // Handle error - could show notification to user
                System.Diagnostics.Debug.WriteLine($"Error refreshing fan data: {ex.Message}");
            }
        }

        public async Task RefreshGpuDataAsync()
        {
            try
            {
                var gpuInfo = await _omenMonService.GetGpuInfoAsync();
                await _dispatcherQueue.EnqueueAsync(() =>
                {
                    GpuInfo = gpuInfo;
                    SelectedGpuMode = gpuInfo.Mode;
                    SelectedGpuPreset = gpuInfo.Preset;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing GPU data: {ex.Message}");
            }
        }

        public async Task RefreshKeyboardDataAsync()
        {
            try
            {
                var keyboardInfo = await _omenMonService.GetKeyboardInfoAsync();
                await _dispatcherQueue.EnqueueAsync(() =>
                {
                    KeyboardInfo = keyboardInfo;
                    IsBacklightEnabled = keyboardInfo.BacklightEnabled;
                    SelectedColor = keyboardInfo.CurrentColor;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing keyboard data: {ex.Message}");
            }
        }

        public async Task RefreshSystemDataAsync()
        {
            try
            {
                var systemInfo = await _omenMonService.GetSystemInfoAsync();
                await _dispatcherQueue.EnqueueAsync(() =>
                {
                    SystemInfo = systemInfo;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing system data: {ex.Message}");
            }
        }

        public async Task SetFanSpeedsAsync(int fan1Speed, int fan2Speed)
        {
            try
            {
                var success = await _omenMonService.SetFanLevelAsync(fan1Speed, fan2Speed);
                if (success)
                {
                    await RefreshFanDataAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting fan speeds: {ex.Message}");
            }
        }

        private async Task RunFanProgramAsync(string programName)
        {
            if (string.IsNullOrEmpty(programName)) return;

            try
            {
                var success = await _omenMonService.RunFanProgramAsync(programName);
                if (success)
                {
                    await RefreshFanDataAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error running fan program: {ex.Message}");
            }
        }

        private async Task SetGpuModeAsync(string mode)
        {
            if (string.IsNullOrEmpty(mode)) return;

            try
            {
                var success = await _omenMonService.SetGpuModeAsync(mode);
                if (success)
                {
                    await RefreshGpuDataAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting GPU mode: {ex.Message}");
            }
        }

        private async Task SetGpuPresetAsync(string preset)
        {
            if (string.IsNullOrEmpty(preset)) return;

            try
            {
                var success = await _omenMonService.SetGpuPresetAsync(preset);
                if (success)
                {
                    await RefreshGpuDataAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting GPU preset: {ex.Message}");
            }
        }

        private async Task SetBacklightAsync(bool enabled)
        {
            try
            {
                var success = await _omenMonService.SetBacklightAsync(enabled);
                if (success)
                {
                    await RefreshKeyboardDataAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting backlight: {ex.Message}");
            }
        }

        private async Task SetColorAsync(string color)
        {
            if (string.IsNullOrEmpty(color)) return;

            try
            {
                var success = await _omenMonService.SetColorAsync(color);
                if (success)
                {
                    await RefreshKeyboardDataAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting color: {ex.Message}");
            }
        }

        private async Task InitializeAsync()
        {
            try
            {
                var isAvailable = await _omenMonService.IsOmenMonAvailableAsync();
                await _dispatcherQueue.EnqueueAsync(() =>
                {
                    ConnectionStatus = isAvailable ? "Connected" : "OmenMon.exe not found";
                    IsFanControlEnabled = isAvailable;
                });

                if (isAvailable)
                {
                    await RefreshAllDataAsync();
                    
                    // Start monitoring loop
                    _ = StartMonitoringLoopAsync();
                }
            }
            catch (Exception ex)
            {
                await _dispatcherQueue.EnqueueAsync(() =>
                {
                    ConnectionStatus = $"Error: {ex.Message}";
                });
            }
        }

        private async Task StartMonitoringLoopAsync()
        {
            IsMonitoring = true;
            
            while (IsMonitoring)
            {
                try
                {
                    await RefreshSystemDataAsync();
                    await Task.Delay(5000); // Update every 5 seconds
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in monitoring loop: {ex.Message}");
                    await Task.Delay(10000); // Wait longer if there's an error
                }
            }
        }

        private void InitializeDefaultData()
        {
            // Initialize with some default data for design-time and fallback
            SystemInfo = new SystemInfo
            {
                CpuTemperature = 65,
                GpuTemperature = 72,
                CpuUsage = 45,
                GpuUsage = 78,
                CpuClockSpeed = "3.2 GHz"
            };

            FanInfo = new FanInfo
            {
                FanCount = 2,
                Fan1Speed = 50,
                Fan2Speed = 45,
                FanMode = "Performance"
            };

            GpuInfo = new GpuInfo
            {
                Mode = "Discrete",
                Preset = "Performance",
                Temperature = 72,
                Usage = 78,
                Memory = "6.2/8 GB"
            };

            KeyboardInfo = new KeyboardInfo
            {
                HasBacklight = true,
                BacklightEnabled = true,
                CurrentColor = "FF0000"
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}