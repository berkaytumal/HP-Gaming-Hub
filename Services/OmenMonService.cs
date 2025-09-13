using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using Windows.Storage;

namespace HP_Gaming_Hub.Services
{
    public class OmenMonService : IDisposable
    {
        private readonly string _omenMonPath;
        private const int CommandTimeoutMs = 10000; // 10 seconds timeout
        private readonly WindowsApiTemperatureService _windowsApiService;
        private bool _useWindowsApi = true; // Try Windows API first

        public OmenMonService(string omenMonPath = null)
        {
            if (string.IsNullOrEmpty(omenMonPath))
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                _omenMonPath = Path.Combine(localFolder.Path, "Dependencies", "OmenMon", "OmenMon.exe");
            }
            else
            {
                _omenMonPath = omenMonPath;
            }
            
            // Initialize Windows API temperature service
            try
            {
                _windowsApiService = new WindowsApiTemperatureService();
                Debug.WriteLine("[OmenMonService] Windows API temperature service initialized");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OmenMonService] Failed to initialize Windows API service: {ex.Message}");
                _useWindowsApi = false;
            }
            
            ValidateConfiguration();
        }

        /// <summary>
        /// Validate the service configuration
        /// </summary>
        private void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_omenMonPath))
            {
                throw new ArgumentException("OmenMon path cannot be null or empty", nameof(_omenMonPath));
            }

            // Log configuration
            Debug.WriteLine($"[ValidateConfiguration] OmenMonService initialized with path: {_omenMonPath}");
            Debug.WriteLine($"[ValidateConfiguration] Command timeout: {CommandTimeoutMs}ms");
            
            // Check if OmenMon executable exists
            var fullPath = Path.GetFullPath(_omenMonPath);
            Debug.WriteLine($"[ValidateConfiguration] Full path: {fullPath}");
            Debug.WriteLine($"[ValidateConfiguration] File exists: {File.Exists(fullPath)}");
            
            if (!File.Exists(fullPath))
            {
                Debug.WriteLine($"[ValidateConfiguration] WARNING: OmenMon executable not found at {fullPath}");
                Debug.WriteLine($"[ValidateConfiguration] Current directory: {Directory.GetCurrentDirectory()}");
                
                // List files in current directory for debugging
                try
                {
                    var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.exe");
                    Debug.WriteLine($"[ValidateConfiguration] Available .exe files in current directory: {string.Join(", ", files.Select(Path.GetFileName))}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ValidateConfiguration] Error listing files: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Check if the OmenMon service is available
        /// </summary>
        public async Task<bool> IsServiceAvailableAsync()
        {
            try
            {
                Debug.WriteLine("[IsServiceAvailableAsync] Testing OmenMon availability");
                var result = await ExecuteCommandAsync("--version");
                Debug.WriteLine($"[IsServiceAvailableAsync] Version check result - Success: {result.Success}, Output: {result.Output}");
                return result.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IsServiceAvailableAsync] Exception during availability check: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Test OmenMon connectivity and basic functionality
        /// </summary>
        public async Task<OmenMonResult> TestConnectivityAsync()
        {
            Debug.WriteLine("[TestConnectivityAsync] Starting comprehensive OmenMon test");
            
            // Test 1: Check if executable exists
            var fullPath = Path.GetFullPath(_omenMonPath);
            if (!File.Exists(fullPath))
            {
                var errorMsg = $"OmenMon executable not found at: {fullPath}";
                Debug.WriteLine($"[TestConnectivityAsync] {errorMsg}");
                return new OmenMonResult
                {
                    Success = false,
                    ErrorMessage = errorMsg,
                    ErrorType = ErrorType.ProcessNotFound
                };
            }
            
            // Test 2: Try version command
            Debug.WriteLine("[TestConnectivityAsync] Testing version command");
            var versionResult = await ExecuteCommandAsync("--version");
            if (!versionResult.Success)
            {
                Debug.WriteLine($"[TestConnectivityAsync] Version command failed: {versionResult.ErrorMessage}");
                return versionResult;
            }
            
            // Test 3: Try basic BIOS query
            Debug.WriteLine("[TestConnectivityAsync] Testing basic BIOS query");
            var biosResult = await ExecuteCommandAsync("-Bios");
            Debug.WriteLine($"[TestConnectivityAsync] BIOS query result - Success: {biosResult.Success}");
            
            if (biosResult.Success)
            {
                Debug.WriteLine($"[TestConnectivityAsync] BIOS query output: {biosResult.Output}");
            }
            else
            {
                Debug.WriteLine($"[TestConnectivityAsync] BIOS query failed: {biosResult.ErrorMessage}");
            }
            
            return new OmenMonResult
            {
                Success = true,
                Output = $"Version: {versionResult.Output}\nBIOS Test: {(biosResult.Success ? "Success" : "Failed")}",
                ErrorMessage = string.Empty,
                ErrorType = ErrorType.None
            };
        }

        /// <summary>
        /// Validate numeric parameters
        /// </summary>
        private bool ValidateNumericRange(int value, int min, int max, string paramName)
        {
            if (value < min || value > max)
            {
                Debug.WriteLine($"Invalid {paramName}: {value}. Must be between {min} and {max}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validate string parameters
        /// </summary>
        private bool ValidateStringParameter(string value, string[] allowedValues, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Debug.WriteLine($"Invalid {paramName}: cannot be null or empty");
                return false;
            }

            if (allowedValues != null && allowedValues.Length > 0)
            {
                if (!Array.Exists(allowedValues, v => string.Equals(v, value, StringComparison.OrdinalIgnoreCase)))
                {
                    Debug.WriteLine($"Invalid {paramName}: {value}. Allowed values: {string.Join(", ", allowedValues)}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Execute OmenMon command and return the output with enhanced error handling
        /// </summary>
        public async Task<OmenMonResult> ExecuteCommandAsync(string arguments)
        {
            var startTime = DateTime.Now;
            
            // Validate inputs
            if (string.IsNullOrWhiteSpace(arguments))
            {
                return new OmenMonResult
                {
                    Success = false,
                    ErrorMessage = "Arguments cannot be null or empty",
                    ErrorType = ErrorType.InvalidArguments,
                    ExitCode = -1,
                    ExecutionTime = startTime,
                    Duration = TimeSpan.Zero
                };
            }

            // Check if OmenMon executable exists
            if (!File.Exists(_omenMonPath))
            {
                return new OmenMonResult
                {
                    Success = false,
                    ErrorMessage = $"OmenMon executable not found at: {_omenMonPath}",
                    ErrorType = ErrorType.ProcessNotFound,
                    ExitCode = -1,
                    ExecutionTime = startTime,
                    Duration = DateTime.Now - startTime
                };
            }

            try
            {
                Debug.WriteLine($"Executing OmenMon command: {arguments}");
            MainWindow.Instance?.LogDebug($"Executing OmenMon: {arguments}");
                
                var processInfo = new ProcessStartInfo
                {
                    FileName = _omenMonPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = AppSettings.Instance.HideCmdWindows,
                    WindowStyle = AppSettings.Instance.HideCmdWindows ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };
                
                using var process = new Process { StartInfo = processInfo };
                
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();
                
                process.OutputDataReceived += (sender, e) => {
                    if (e.Data != null) outputBuilder.AppendLine(e.Data);
                };
                
                process.ErrorDataReceived += (sender, e) => {
                    if (e.Data != null) errorBuilder.AppendLine(e.Data);
                };
                
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                
                var completed = await Task.Run(() => process.WaitForExit(CommandTimeoutMs));
                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                
                if (!completed)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception killEx)
                    {
                        Debug.WriteLine($"Error killing process: {killEx.Message}");
                    }
                    
                    return new OmenMonResult
                    {
                        Success = false,
                        ErrorMessage = $"Command timed out after {CommandTimeoutMs}ms",
                        ErrorType = ErrorType.Timeout,
                        ExitCode = -1,
                        ExecutionTime = startTime,
                        Duration = duration
                    };
                }

                var output = outputBuilder.ToString();
                var error = errorBuilder.ToString();
                    
                    Debug.WriteLine($"OmenMon Output: {output}");
                    if (!string.IsNullOrEmpty(output.Trim()))
                    {
                        MainWindow.Instance?.LogInfo($"OmenMon Output: {output.Trim()}");
                    }
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.WriteLine($"OmenMon Error: {error}");
                        MainWindow.Instance?.LogError($"OmenMon Error: {error.Trim()}");
                    }
                    
                    var exitCode = process.ExitCode;
                    
                    Debug.WriteLine($"OmenMon completed with exit code: {exitCode}, duration: {duration.TotalMilliseconds}ms");
                if (exitCode == 0)
                {
                    MainWindow.Instance?.LogInfo($"OmenMon command completed successfully in {duration.TotalMilliseconds:F0}ms");
                }
                else
                {
                    MainWindow.Instance?.LogWarning($"OmenMon command failed with exit code {exitCode} after {duration.TotalMilliseconds:F0}ms");
                }

                    // Determine error type based on output and exit code
                    var errorType = DetermineErrorType(exitCode, error, output);
                    
                    return new OmenMonResult
                    {
                        Success = exitCode == 0 && errorType == ErrorType.None,
                        Output = output,
                        ErrorMessage = exitCode != 0 ? error : string.Empty,
                        ErrorType = errorType,
                        ExitCode = exitCode,
                        ExecutionTime = startTime,
                        Duration = duration
                    };

            }
            catch (UnauthorizedAccessException ex)
            {
                MainWindow.Instance?.LogError($"Permission denied executing OmenMon: {ex.Message}");
                return new OmenMonResult
                {
                    Success = false,
                    ErrorMessage = $"Permission denied: {ex.Message}",
                    ErrorType = ErrorType.PermissionDenied,
                    ExitCode = -1,
                    ExecutionTime = startTime,
                    Duration = DateTime.Now - startTime
                };
            }
            catch (FileNotFoundException ex)
            {
                MainWindow.Instance?.LogError($"OmenMon executable not found: {ex.Message}");
                return new OmenMonResult
                {
                    Success = false,
                    ErrorMessage = $"File not found: {ex.Message}",
                    ErrorType = ErrorType.ProcessNotFound,
                    ExitCode = -1,
                    ExecutionTime = startTime,
                    Duration = DateTime.Now - startTime
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error executing OmenMon: {ex}");
                MainWindow.Instance?.LogError($"Unexpected error executing OmenMon: {ex.Message}");
                return new OmenMonResult
                {
                    Success = false,
                    ErrorMessage = $"Unexpected error: {ex.Message}",
                    ErrorType = ErrorType.UnknownError,
                    ExitCode = -1,
                    ExecutionTime = startTime,
                    Duration = DateTime.Now - startTime
                };
            }
        }

        /// <summary>
        /// Determine the error type based on exit code and output
        /// </summary>
        private ErrorType DetermineErrorType(int exitCode, string error, string output)
        {
            if (exitCode == 0)
                return ErrorType.None;
                
            var errorLower = error.ToLowerInvariant();
            var outputLower = output.ToLowerInvariant();
            
            if (errorLower.Contains("access denied") || errorLower.Contains("permission"))
                return ErrorType.PermissionDenied;
                
            if (errorLower.Contains("not supported") || errorLower.Contains("hardware") || 
                outputLower.Contains("not supported") || outputLower.Contains("unavailable"))
                return ErrorType.HardwareNotSupported;
                
            if (errorLower.Contains("invalid") || errorLower.Contains("argument"))
                return ErrorType.InvalidArguments;
                
            return ErrorType.UnknownError;
        }

        /// <summary>
        /// Get system temperatures
        /// </summary>
        public async Task<TemperatureData> GetTemperaturesAsync()
        {
            Debug.WriteLine("[GetTemperaturesAsync] Starting temperature retrieval");
            
            // Try Windows API first if available
            if (_useWindowsApi && _windowsApiService != null && _windowsApiService.IsAvailable())
            {
                try
                {
                    Debug.WriteLine("[GetTemperaturesAsync] Trying Windows API temperature monitoring");
                    var windowsApiData = await _windowsApiService.GetTemperaturesAsync();
                    
                    // Check if we got valid temperature data
                    if (windowsApiData.CpuTemperature > 0 || windowsApiData.GpuTemperature > 0)
                    {
                        Debug.WriteLine($"[GetTemperaturesAsync] Windows API succeeded - CPU: {windowsApiData.CpuTemperature}°C, GPU: {windowsApiData.GpuTemperature}°C");
                        return windowsApiData;
                    }
                    else
                    {
                        Debug.WriteLine("[GetTemperaturesAsync] Windows API returned no valid temperature data, falling back to OmenMon");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[GetTemperaturesAsync] Windows API failed: {ex.Message}, falling back to OmenMon");
                    _useWindowsApi = false; // Disable Windows API for future calls
                }
            }
            
            // Fall back to OmenMon
            Debug.WriteLine("[GetTemperaturesAsync] Using OmenMon for temperature retrieval");
            var result = await ExecuteCommandAsync("-Bios");
            
            var tempData = new TemperatureData();
            
            if (result.Success)
            {
                Debug.WriteLine($"[GetTemperaturesAsync] OmenMon BIOS command succeeded, parsing output");
                tempData = ParseTemperatureData(result.Output);
            }
            else
            {
                Debug.WriteLine($"[GetTemperaturesAsync] OmenMon BIOS command failed - Error: {result.ErrorMessage}");
            }
            
            Debug.WriteLine($"[GetTemperaturesAsync] Final temperatures - CPU: {tempData.CpuTemperature}°C, GPU: {tempData.GpuTemperature}°C");
            return tempData;
        }

        /// <summary>
        /// Get fan information
        /// </summary>
        public async Task<FanData> GetFanDataAsync()
        {
            Debug.WriteLine("[GetFanDataAsync] Starting fan data retrieval");
            var result = await ExecuteCommandAsync("-Bios");
            
            if (!result.Success)
            {
                Debug.WriteLine($"[GetFanDataAsync] BIOS command failed - Error: {result.ErrorMessage}, Type: {result.ErrorType}, ExitCode: {result.ExitCode}");
                return new FanData();
            }
            
            Debug.WriteLine($"[GetFanDataAsync] BIOS command succeeded, parsing output");
            return ParseFanData(result.Output);
        }

        /// <summary>
        /// Set fan speed levels
        /// </summary>
        public async Task<bool> SetFanLevelsAsync(int fan1Level, int fan2Level)
        {
            // Validate fan levels (0-100%)
            if (!ValidateNumericRange(fan1Level, 0, 100, "fan1Level") ||
                !ValidateNumericRange(fan2Level, 0, 100, "fan2Level"))
            {
                return false;
            }

            var result = await ExecuteCommandAsync($"-Bios FanLevel={fan1Level},{fan2Level}");
            return result.Success;
        }

        /// <summary>
        /// Set fan mode
        /// </summary>
        public async Task<bool> SetFanModeAsync(string fanMode)
        {
            // Validate fan mode
            var allowedModes = new[] { "auto", "manual", "performance", "quiet" };
            if (!ValidateStringParameter(fanMode, allowedModes, "fanMode"))
            {
                return false;
            }

            var result = await ExecuteCommandAsync($"-Bios FanMode={fanMode}");
            return result.Success;
        }

        /// <summary>
        /// Get GPU information
        /// </summary>
        public async Task<GpuData> GetGpuDataAsync()
        {
            var result = await ExecuteCommandAsync("-Bios Gpu GpuMode");
            if (!result.Success)
                return new GpuData();

            return ParseGpuData(result.Output);
        }

        /// <summary>
        /// Set GPU mode
        /// </summary>
        public async Task<bool> SetGpuModeAsync(string gpuMode)
        {
            // Validate GPU mode
            var allowedModes = new[] { "hybrid", "discrete", "optimus" };
            if (!ValidateStringParameter(gpuMode, allowedModes, "gpuMode"))
            {
                return false;
            }

            var result = await ExecuteCommandAsync($"-Bios GpuMode={gpuMode}");
            return result.Success;
        }

        /// <summary>
        /// Set GPU preset
        /// </summary>
        public async Task<bool> SetGpuPresetAsync(string preset)
        {
            // Validate GPU preset
            var allowedPresets = new[] { "minimum", "medium", "maximum" };
            if (!ValidateStringParameter(preset, allowedPresets, "preset"))
            {
                return false;
            }

            var result = await ExecuteCommandAsync($"-Bios Gpu={preset}");
            return result.Success;
        }

        /// <summary>
        /// Get keyboard backlight status
        /// </summary>
        public async Task<KeyboardData> GetKeyboardDataAsync()
        {
            var result = await ExecuteCommandAsync("-Bios KbdType HasBacklight Backlight Color");
            if (!result.Success)
                return new KeyboardData();

            return ParseKeyboardData(result.Output);
        }

        /// <summary>
        /// Set keyboard backlight
        /// </summary>
        public async Task<bool> SetKeyboardBacklightAsync(bool enabled)
        {
            var result = await ExecuteCommandAsync($"-Bios Backlight={enabled}");
            return result.Success;
        }

        /// <summary>
        /// Set maximum fan speed
        /// </summary>
        public async Task<bool> SetMaxFanAsync(bool enabled)
        {
            var result = await ExecuteCommandAsync($"-Bios FanMax={enabled}");
            return result.Success;
        }

        /// <summary>
        /// Get fan table data
        /// </summary>
        public async Task<FanTableData> GetFanTableAsync()
        {
            var result = await ExecuteCommandAsync("-Bios FanTable");
            if (!result.Success)
                return new FanTableData();

            return ParseFanTableData(result.Output);
        }

        /// <summary>
        /// Set fan table data
        /// </summary>
        public async Task<bool> SetFanTableAsync(string fanTableData)
        {
            var result = await ExecuteCommandAsync($"-Bios FanTable={fanTableData}");
            return result.Success;
        }

        // Duplicate methods removed - using earlier definitions with proper validation

        /// <summary>
        /// Set CPU power limits
        /// </summary>
        public async Task<bool> SetCpuPowerLimitsAsync(int pl1, int pl4)
        {
            // Validate CPU power limits (typical range 15-100W for PL1, 25-150W for PL4)
            if (!ValidateNumericRange(pl1, 15, 100, "pl1") ||
                !ValidateNumericRange(pl4, 25, 150, "pl4"))
            {
                return false;
            }

            // Ensure PL4 >= PL1
            if (pl4 < pl1)
            {
                Debug.WriteLine($"Invalid power limits: PL4 ({pl4}W) must be >= PL1 ({pl1}W)");
                return false;
            }

            try
            {
                var result1 = await ExecuteCommandAsync($"-Bios Cpu:PL1={pl1}");
                var result2 = await ExecuteCommandAsync($"-Bios Cpu:PL4={pl4}");
                return result1.Success && result2.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting CPU power limits: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Set GPU power limit
        /// </summary>
        public async Task<bool> SetGpuPowerLimitAsync(int powerLimit)
        {
            // Validate GPU power limit (typical range 50-200W)
            if (!ValidateNumericRange(powerLimit, 50, 200, "powerLimit"))
            {
                return false;
            }

            try
            {
                var result = await ExecuteCommandAsync($"-Bios Cpu:PLGpu={powerLimit}");
                return result.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting GPU power limit: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Set XMP memory profile
        /// </summary>
        public async Task<bool> SetXmpAsync(bool enabled)
        {
            try
            {
                var flag = enabled ? "On" : "Off";
                var result = await ExecuteCommandAsync($"-Bios Xmp={flag}");
                return result.Success;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Keyboard Control Methods (duplicate SetKeyboardBacklightAsync removed - using earlier definition)

        public async Task<bool> SetKeyboardColorPresetAsync(string preset)
        {
            // Validate keyboard color preset
            var allowedPresets = new[] { "rainbow", "red", "green", "blue", "white", "custom" };
            if (!ValidateStringParameter(preset, allowedPresets, "preset"))
            {
                return false;
            }

            try
            {
                var result = await ExecuteCommandAsync($"--set-keyboard-preset {preset.ToLower()}");
                return result.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting keyboard color preset: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetKeyboardZoneColorsAsync(int zone1R, int zone1G, int zone1B, int zone2R, int zone2G, int zone2B, int zone3R, int zone3G, int zone3B, int zone4R, int zone4G, int zone4B)
        {
            // Validate RGB values (0-255)
            if (!ValidateNumericRange(zone1R, 0, 255, "zone1R") || !ValidateNumericRange(zone1G, 0, 255, "zone1G") || !ValidateNumericRange(zone1B, 0, 255, "zone1B") ||
                !ValidateNumericRange(zone2R, 0, 255, "zone2R") || !ValidateNumericRange(zone2G, 0, 255, "zone2G") || !ValidateNumericRange(zone2B, 0, 255, "zone2B") ||
                !ValidateNumericRange(zone3R, 0, 255, "zone3R") || !ValidateNumericRange(zone3G, 0, 255, "zone3G") || !ValidateNumericRange(zone3B, 0, 255, "zone3B") ||
                !ValidateNumericRange(zone4R, 0, 255, "zone4R") || !ValidateNumericRange(zone4G, 0, 255, "zone4G") || !ValidateNumericRange(zone4B, 0, 255, "zone4B"))
            {
                return false;
            }

            try
            {
                var result = await ExecuteCommandAsync($"--set-keyboard-zones {zone1R},{zone1G},{zone1B} {zone2R},{zone2G},{zone2B} {zone3R},{zone3G},{zone3B} {zone4R},{zone4G},{zone4B}");
                return result.Success;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SetKeyboardAnimationAsync(string animation)
        {
            // Validate keyboard animation
            var allowedAnimations = new[] { "static", "breathing", "wave", "ripple", "rainbow" };
            if (!ValidateStringParameter(animation, allowedAnimations, "animation"))
            {
                return false;
            }

            try
            {
                var result = await ExecuteCommandAsync($"--set-keyboard-animation {animation.ToLower()}");
                return result.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting keyboard animation: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResetKeyboardColorsAsync()
        {
            try
            {
                var result = await ExecuteCommandAsync("--reset-keyboard-colors");
                return result.Success;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Set keyboard color
        /// </summary>
        public async Task<bool> SetKeyboardColorAsync(string color)
        {
            var result = await ExecuteCommandAsync($"-Bios Color={color}");
            return result.Success;
        }

        /// <summary>
        /// Get system information
        /// </summary>
        public async Task<SystemData> GetSystemDataAsync()
        {
            var result = await ExecuteCommandAsync("-Bios System BornDate Adapter HasOverclock HasMemoryOverclock HasUndervolt");
            if (!result.Success)
                return new SystemData();

            return ParseSystemData(result.Output);
        }

        /// <summary>
        /// Get system information as string
        /// </summary>
        public async Task<OmenMonResult<string>> GetSystemInfoAsync()
        {
            try
            {
                var result = await ExecuteCommandAsync("-System");
                if (result.Success)
                {
                    return new OmenMonResult<string> { IsSuccess = true, Data = result.Output };
                }
                return new OmenMonResult<string> { IsSuccess = false, ErrorMessage = result.ErrorMessage };
            }
            catch (Exception ex)
            {
                return new OmenMonResult<string> { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// Set overclocking support
        /// </summary>
        public async Task<OmenMonResult> SetOverclockAsync(bool enabled)
        {
            try
            {
                var command = enabled ? "-Bios HasOverclock=true" : "-Bios HasOverclock=false";
                var result = await ExecuteCommandAsync(command);
                return new OmenMonResult { IsSuccess = result.Success, ErrorMessage = result.ErrorMessage };
            }
            catch (Exception ex)
            {
                return new OmenMonResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// Set memory overclocking support
        /// </summary>
        public async Task<OmenMonResult> SetMemoryOverclockAsync(bool enabled)
        {
            try
            {
                var command = enabled ? "-Bios HasMemoryOverclock=true" : "-Bios HasMemoryOverclock=false";
                var result = await ExecuteCommandAsync(command);
                return new OmenMonResult { IsSuccess = result.Success, ErrorMessage = result.ErrorMessage };
            }
            catch (Exception ex)
            {
                return new OmenMonResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// Set CPU undervolting support
        /// </summary>
        public async Task<OmenMonResult> SetUndervoltAsync(bool enabled)
        {
            try
            {
                var command = enabled ? "-Bios HasUndervolt=true" : "-Bios HasUndervolt=false";
                var result = await ExecuteCommandAsync(command);
                return new OmenMonResult { IsSuccess = result.Success, ErrorMessage = result.ErrorMessage };
            }
            catch (Exception ex)
            {
                return new OmenMonResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        // Parsing methods
        private TemperatureData ParseTemperatureData(string output)
        {
            var tempData = new TemperatureData();
            
            Debug.WriteLine($"[ParseTemperatureData] Raw output: {output}");
            
            if (string.IsNullOrWhiteSpace(output))
            {
                Debug.WriteLine("[ParseTemperatureData] Output is null or empty");
                return tempData;
            }
            
            // Parse temperature values from output
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Debug.WriteLine($"[ParseTemperatureData] Found {lines.Length} lines to parse");
            
            foreach (var line in lines)
            {
                Debug.WriteLine($"[ParseTemperatureData] Processing line: {line}");
                
                // Parse OmenMon temperature format: "- Temperature: 0x00 = 0b00000000 = 0 [°C]"
                if (line.Contains("Temperature:") && line.Contains("[°C]"))
                {
                    // Extract the decimal value before [°C]
                    var match = Regex.Match(line, @"=\s*(\d+)\s*\[°C\]");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int temp))
                    {
                        // For now, assume this is CPU temperature since OmenMon -Bios Temp returns CPU temp
                        // We'll need separate calls for GPU temperature if available
                        tempData.CpuTemperature = temp;
                        Debug.WriteLine($"[ParseTemperatureData] Found CPU temperature: {temp}°C");
                    }
                    else
                    {
                        Debug.WriteLine($"[ParseTemperatureData] Failed to parse temperature from: {line}");
                    }
                }
                // Also check for GPU Peak Temperature if present
                else if (line.Contains("GPU Peak Temperature") && line.Contains("[°C]"))
                {
                    var match = Regex.Match(line, @"=\s*(\d+)\s*\[°C\]");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int temp))
                    {
                        tempData.GpuTemperature = temp;
                        Debug.WriteLine($"[ParseTemperatureData] Found GPU temperature: {temp}°C");
                    }
                }
            }
            
            Debug.WriteLine($"[ParseTemperatureData] Final result - CPU: {tempData.CpuTemperature}°C, GPU: {tempData.GpuTemperature}°C");
            return tempData;
        }

        private FanData ParseFanData(string output)
        {
            var fanData = new FanData();
            
            Debug.WriteLine($"[ParseFanData] Raw output: {output}");
            
            if (string.IsNullOrWhiteSpace(output))
            {
                Debug.WriteLine("[ParseFanData] Output is null or empty");
                return fanData;
            }
            
            // Parse fan data from output
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Debug.WriteLine($"[ParseFanData] Found {lines.Length} lines to parse");
            
            foreach (var line in lines)
            {
                Debug.WriteLine($"[ParseFanData] Processing line: {line}");
                
                // Parse OmenMon fan count format: "- Fan Count: 0x02 = 0b00000010 = 2"
                if (line.Contains("Fan Count:"))
                {
                    var match = Regex.Match(line, @"=\s*(\d+)\s*$");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int count))
                    {
                        fanData.FanCount = count;
                        Debug.WriteLine($"[ParseFanData] Found fan count: {count}");
                    }
                    else
                    {
                        Debug.WriteLine($"[ParseFanData] Failed to parse fan count from: {line}");
                    }
                }
                // Parse individual fan levels: "- Fan #1 Level: 0x1f = 0b00011111 = 31 [Cpu]"
                else if (line.Contains("Fan #1 Level:"))
                {
                    var match = Regex.Match(line, @"=\s*(\d+)\s*\[");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int level))
                    {
                        // Convert fan level (0-255) to approximate RPM
                        // Typical conversion: RPM = (level / 255) * max_rpm
                        // Assuming max RPM around 4000-5000 for gaming laptops
                        fanData.Fan1Speed = (int)((level / 255.0) * 4500);
                        fanData.Fan1Level = level;
                        Debug.WriteLine($"[ParseFanData] Found Fan1 level: {level}, calculated speed: {fanData.Fan1Speed} RPM");
                    }
                    else
                    {
                        Debug.WriteLine($"[ParseFanData] Failed to parse Fan1 level from: {line}");
                    }
                }
                else if (line.Contains("Fan #2 Level:"))
                {
                    var match = Regex.Match(line, @"=\s*(\d+)\s*\[");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int level))
                    {
                        // Convert fan level (0-255) to approximate RPM
                        fanData.Fan2Speed = (int)((level / 255.0) * 4500);
                        fanData.Fan2Level = level;
                        Debug.WriteLine($"[ParseFanData] Found Fan2 level: {level}, calculated speed: {fanData.Fan2Speed} RPM");
                    }
                    else
                    {
                        Debug.WriteLine($"[ParseFanData] Failed to parse Fan2 level from: {line}");
                    }
                }
                // Parse maximum fan speed setting: "- Maximum Fan Speed: No"
                else if (line.Contains("Maximum Fan Speed:"))
                {
                    var isMaxSpeed = line.Contains("Yes");
                    Debug.WriteLine($"[ParseFanData] Maximum fan speed mode: {isMaxSpeed}");
                }
            }
            
            Debug.WriteLine($"[ParseFanData] Final result - Count: {fanData.FanCount}, Fan1: {fanData.Fan1Speed} RPM (Level: {fanData.Fan1Level}), Fan2: {fanData.Fan2Speed} RPM (Level: {fanData.Fan2Level}), Mode: {fanData.FanMode}");
            return fanData;
        }

        private GpuData ParseGpuData(string output)
        {
            var gpuData = new GpuData();
            
            // Parse GPU data from output
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.Contains("GPU Mode:"))
                {
                    var match = Regex.Match(line, @"GPU Mode:\s*(.+)");
                    if (match.Success)
                        gpuData.GpuMode = match.Groups[1].Value.Trim();
                }
                else if (line.Contains("GPU Preset:"))
                {
                    var match = Regex.Match(line, @"GPU Preset:\s*(.+)");
                    if (match.Success)
                        gpuData.GpuPreset = match.Groups[1].Value.Trim();
                }
            }
            
            return gpuData;
        }

        private KeyboardData ParseKeyboardData(string output)
        {
            var kbdData = new KeyboardData();
            
            // Parse keyboard data from output
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.Contains("Has Backlight:"))
                {
                    kbdData.HasBacklight = line.Contains("True") || line.Contains("Yes") || line.Contains("On");
                }
                else if (line.Contains("Backlight:"))
                {
                    kbdData.BacklightEnabled = line.Contains("True") || line.Contains("Yes") || line.Contains("On");
                }
                else if (line.Contains("Color:"))
                {
                    var match = Regex.Match(line, @"Color:\s*(.+)");
                    if (match.Success)
                        kbdData.CurrentColor = match.Groups[1].Value.Trim();
                }
            }
            
            return kbdData;
        }

        private SystemData ParseSystemData(string output)
        {
            var sysData = new SystemData();
            
            // Parse system data from output
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.Contains("System:"))
                {
                    var match = Regex.Match(line, @"System:\s*(.+)");
                    if (match.Success)
                        sysData.SystemInfo = match.Groups[1].Value.Trim();
                }
                else if (line.Contains("Born Date:"))
                {
                    var match = Regex.Match(line, @"Born Date:\s*(.+)");
                    if (match.Success)
                        sysData.BornDate = match.Groups[1].Value.Trim();
                }
                else if (line.Contains("Has Overclock:"))
                {
                    sysData.HasOverclock = line.Contains("True") || line.Contains("Yes") || line.Contains("On");
                }
                else if (line.Contains("Has Memory Overclock:"))
                {
                    sysData.HasMemoryOverclock = line.Contains("True") || line.Contains("Yes") || line.Contains("On");
                }
                else if (line.Contains("Has Undervolt:"))
                {
                    sysData.HasUndervolt = line.Contains("True") || line.Contains("Yes") || line.Contains("On");
                }
            }
            
            return sysData;
        }

        private FanTableData ParseFanTableData(string output)
        {
            var fanTableData = new FanTableData();
            
            // Parse fan table data from output
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.Contains("Fan Table:"))
                {
                    var match = Regex.Match(line, @"Fan Table:\s*(.+)");
                    if (match.Success)
                        fanTableData.TableData = match.Groups[1].Value.Trim();
                }
            }
            
            return fanTableData;
        }

        /// <summary>
        /// Test both Windows API and OmenMon temperature monitoring methods
        /// </summary>
        public async Task<string> TestTemperatureMethodsAsync()
        {
            var results = new List<string>();
            
            // Test Windows API
            if (_windowsApiService != null && _windowsApiService.IsAvailable())
            {
                try
                {
                    var windowsApiData = await _windowsApiService.GetTemperaturesAsync();
                    results.Add($"Windows API - CPU: {windowsApiData.CpuTemperature}°C, GPU: {windowsApiData.GpuTemperature}°C");
                    
                    var hardwareInfo = await _windowsApiService.GetHardwareInfoAsync();
                    results.Add($"Available Hardware:\n{hardwareInfo}");
                }
                catch (Exception ex)
                {
                    results.Add($"Windows API Error: {ex.Message}");
                }
            }
            else
            {
                results.Add("Windows API: Not available");
            }
            
            // Test OmenMon
            try
            {
                var omenResult = await ExecuteCommandAsync("-Bios");
                if (omenResult.Success)
                {
                    var omenData = ParseTemperatureData(omenResult.Output);
                    results.Add($"OmenMon - CPU: {omenData.CpuTemperature}°C, GPU: {omenData.GpuTemperature}°C");
                }
                else
                {
                    results.Add($"OmenMon Error: {omenResult.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"OmenMon Exception: {ex.Message}");
            }
            
            return string.Join(Environment.NewLine, results);
        }

        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                _windowsApiService?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OmenMonService] Error during disposal: {ex.Message}");
            }
        }
    }

    // Result and data model classes
    public enum ErrorType
    {
        None,
        Timeout,
        ProcessNotFound,
        InvalidArguments,
        PermissionDenied,
        HardwareNotSupported,
        UnknownError
    }

    public class OmenMonResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public int ExitCode { get; set; }
        public ErrorType ErrorType { get; set; } = ErrorType.None;
        public DateTime ExecutionTime { get; set; } = DateTime.Now;
        public TimeSpan Duration { get; set; }
        
        public bool IsTimeout => ErrorType == ErrorType.Timeout;
        public bool IsProcessError => ErrorType == ErrorType.ProcessNotFound;
        public bool IsPermissionError => ErrorType == ErrorType.PermissionDenied;
        public bool IsHardwareError => ErrorType == ErrorType.HardwareNotSupported;
        public bool IsSuccess { get; set; }
    }

    public class OmenMonResult<T> : OmenMonResult
    {
        public T Data { get; set; }
    }

    public class TemperatureData
    {
        public int CpuTemperature { get; set; }
        public int GpuTemperature { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class FanData
    {
        public int FanCount { get; set; }
        public int Fan1Speed { get; set; }
        public int Fan2Speed { get; set; }
        public int? Fan1Level { get; set; }
        public int? Fan2Level { get; set; }
        public string FanMode { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class GpuData
    {
        public string GpuMode { get; set; } = string.Empty;
        public string GpuPreset { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class KeyboardData
    {
        public bool HasBacklight { get; set; }
        public bool BacklightEnabled { get; set; }
        public string CurrentColor { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class SystemData
    {
        public string SystemInfo { get; set; } = string.Empty;
        public string BornDate { get; set; } = string.Empty;
        public bool HasOverclock { get; set; }
        public bool HasMemoryOverclock { get; set; }
        public bool HasUndervolt { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class FanTableData
    {
        public string TableData { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}