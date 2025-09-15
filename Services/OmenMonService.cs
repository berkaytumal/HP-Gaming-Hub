using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
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
        private bool _useWindowsApi = false; // Disabled Windows API, using only OmenMon

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
            
            // Windows API temperature service disabled - using only OmenMon
            Log.Debug("Using OmenMon only for temperature monitoring");
            
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
            Log.Debug("OmenMonService initialized with path: {OmenMonPath}", _omenMonPath);
            Log.Debug("Command timeout: {TimeoutMs}ms", CommandTimeoutMs);
            
            // Check if OmenMon executable exists
            var fullPath = Path.GetFullPath(_omenMonPath);
            Log.Debug("Full path: {FullPath}", fullPath);
                Log.Debug("File exists: {FileExists}", File.Exists(fullPath));
            
            if (!File.Exists(fullPath))
            {
                Log.Warning("OmenMon executable not found at {FullPath}", fullPath);
                Log.Debug("Current directory: {CurrentDirectory}", Directory.GetCurrentDirectory());
                
                // List files in current directory for debugging
                try
                {
                    var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.exe");
                    Log.Debug("Available .exe files in current directory: {ExeFiles}", string.Join(", ", files.Select(Path.GetFileName)));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error listing files");
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
                Log.Debug("Testing OmenMon availability");
                var result = await ExecuteCommandAsync("--version");
                Log.Debug("Version check result - Success: {Success}, Output: {Output}", result.Success, result.Output);
                return result.Success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception during availability check");
                return false;
            }
        }
        
        /// <summary>
        /// Test OmenMon connectivity and basic functionality
        /// </summary>
        public async Task<OmenMonResult> TestConnectivityAsync()
        {
            Log.Debug("Starting comprehensive OmenMon test");
            
            // Test 1: Check if executable exists
            var fullPath = Path.GetFullPath(_omenMonPath);
            if (!File.Exists(fullPath))
            {
                var errorMsg = $"OmenMon executable not found at: {fullPath}";
                Log.Warning("{ErrorMessage}", errorMsg);
                return new OmenMonResult
                {
                    Success = false,
                    ErrorMessage = errorMsg,
                    ErrorType = ErrorType.ProcessNotFound
                };
            }
            
            // Test 2: Try version command
            Log.Debug("Testing version command");
            var versionResult = await ExecuteCommandAsync("--version");
            if (!versionResult.Success)
            {
                Log.Warning("Version command failed: {ErrorMessage}", versionResult.ErrorMessage);
                return versionResult;
            }
            
            // Test 3: Try basic BIOS query
            Log.Debug("Testing basic BIOS query");
            var biosResult = await ExecuteCommandAsync("BIOS");
            Log.Debug("BIOS query result - Success: {Success}", biosResult.Success);
            
            if (biosResult.Success)
            {
                Log.Debug("BIOS query output: {Output}", biosResult.Output);
            }
            else
            {
                Log.Warning("BIOS query failed: {ErrorMessage}", biosResult.ErrorMessage);
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
                Log.Warning("Invalid {ParamName}: {Value}. Must be between {Min} and {Max}", paramName, value, min, max);
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
                Log.Warning("Invalid {ParamName}: cannot be null or empty", paramName);
                return false;
            }

            if (allowedValues != null && allowedValues.Length > 0)
            {
                if (!Array.Exists(allowedValues, v => string.Equals(v, value, StringComparison.OrdinalIgnoreCase)))
                {
                    Log.Warning("Invalid {ParamName}: {Value}. Allowed values: {AllowedValues}", paramName, value, string.Join(", ", allowedValues));
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
                Log.Debug("Executing OmenMon command: {Arguments}", arguments);
            MainWindow.Instance?.LogDebug($"Executing OmenMon: {arguments}");
                
                var processInfo = new ProcessStartInfo
                {
                    FileName = _omenMonPath,
                    Arguments = arguments,
                    //FileName = "cmd.exe",
                    //Arguments = $"/q /c start \"\" /b \"{_omenMonPath}\" {arguments}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
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
                        Log.Error(killEx, "Error killing process");
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
                    
                    Log.Debug("OmenMon Output: {Output}", output);
                    if (!string.IsNullOrEmpty(output.Trim()))
                    {
                        Log.Information("OmenMon Output: {Output}", output.Trim());
                    }
                    if (!string.IsNullOrEmpty(error))
                    {
                        Log.Warning("OmenMon Error: {Error}", error);
                        Log.Error("OmenMon Error: {Error}", error.Trim());
                    }
                    
                    var exitCode = process.ExitCode;
                    
                    Log.Debug("OmenMon completed with exit code: {ExitCode}, duration: {DurationMs}ms", exitCode, duration.TotalMilliseconds);
                if (exitCode == 0)
                {
                    Log.Information("OmenMon command completed successfully in {DurationMs:F0}ms", duration.TotalMilliseconds);
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
                Log.Error(ex, "Permission denied executing OmenMon: {Message}", ex.Message);
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
                Log.Error(ex, "OmenMon executable not found: {Message}", ex.Message);
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
                Log.Error(ex, "Unexpected error executing OmenMon");
                Log.Error(ex, "Unexpected error executing OmenMon: {Message}", ex.Message);
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
            Log.Debug("Starting temperature retrieval using OmenMon EC command");
            
            // Use OmenMon EC command for temperature retrieval
            var result = await ExecuteCommandAsync("-Ec CPUT GPTM");
            
            var tempData = new TemperatureData();
            
            if (result.Success)
            {
                Log.Debug("OmenMon EC command succeeded, parsing output");
                tempData = ParseEcTemperatureData(result.Output);
            }
            else
            {
                Log.Warning("OmenMon EC command failed - Error: {ErrorMessage}", result.ErrorMessage);
            }
            
            Log.Debug("Final temperatures - CPU: {CpuTemp}°C, GPU: {GpuTemp}°C", tempData.CpuTemperature, tempData.GpuTemperature);
            return tempData;
        }

        /// <summary>
        /// Get temperatures using Windows API (LibreHardwareMonitor) only
        /// </summary>
        /// <summary>
        /// Get all hardware data using unified OmenMon commands
        /// </summary>
        public async Task<(TemperatureData temperatures, FanData fanData, GpuData gpuData, KeyboardData keyboardData, SystemData systemData)> GetUnifiedHardwareDataAsync()
        {
            Log.Debug("Starting unified hardware data retrieval");
            
            // Single unified OmenMon call with all required arguments
            var unifiedResult = await ExecuteCommandAsync("-Ec CPUT GPTM RPM1 RPM2 RPM3 RPM4 XGS1 XGS2 -Bios Gpu GpuMode KbdType HasBacklight Backlight Color System BornDate Adapter HasOverclock HasMemoryOverclock HasUndervolt FanCount FanMode");
            
            var tempData = new TemperatureData();
            var fanData = new FanData();
            var gpuData = new GpuData();
            var keyboardData = new KeyboardData();
            var systemData = new SystemData();
            
            if (unifiedResult.Success)
            {
                // Parse temperature data from EC output
                tempData = ParseEcTemperatureData(unifiedResult.Output);
                
                // Parse fan data from BIOS output
                fanData = ParseFanData(unifiedResult.Output);
                
                // Parse GPU data from BIOS output
                gpuData = ParseGpuData(unifiedResult.Output);
                
                // Parse keyboard data from BIOS output
                keyboardData = ParseKeyboardData(unifiedResult.Output);
                
                // Parse system data from BIOS output
                systemData = ParseSystemData(unifiedResult.Output);
            }
            else
            {
                Log.Warning("Unified command failed: {ErrorMessage}", unifiedResult.ErrorMessage);
            }
            
            Log.Debug("Unified data retrieval complete - CPU: {CpuTemp}°C, GPU: {GpuTemp}°C, Fan1: {Fan1Speed}RPM, Fan2: {Fan2Speed}RPM", tempData.CpuTemperature, tempData.GpuTemperature, fanData.Fan1Speed, fanData.Fan2Speed);
            return (tempData, fanData, gpuData, keyboardData, systemData);
        }

        /// <summary>
        /// Get temperatures using OmenMon EC command
        /// </summary>
        public async Task<TemperatureData> GetOmenMonTemperaturesAsync()
        {
            Log.Debug("Using OmenMon EC command for temperature retrieval");
            var result = await ExecuteCommandAsync("-Ec CPUT GPTM");
            
            var tempData = new TemperatureData();
            
            if (result.Success)
            {
                Log.Debug("OmenMon EC command succeeded, parsing output");
                tempData = ParseEcTemperatureData(result.Output);
            }
            else
            {
                Log.Warning("OmenMon EC command failed - Error: {ErrorMessage}", result.ErrorMessage);
            }
            
            Log.Debug("Final temperatures - CPU: {CpuTemp}°C, GPU: {GpuTemp}°C", tempData.CpuTemperature, tempData.GpuTemperature);
            return tempData;
        }

        /// <summary>
        /// Get fan information
        /// </summary>
        public async Task<FanData> GetFanDataAsync()
        {
            Log.Debug("Starting fan data retrieval");
            var result = await ExecuteCommandAsync("-Bios");
            
            if (!result.Success)
            {
                Log.Warning("BIOS command failed - Error: {ErrorMessage}, Type: {ErrorType}, ExitCode: {ExitCode}", result.ErrorMessage, result.ErrorType, result.ExitCode);
                return new FanData();
            }
            
            Log.Debug("BIOS command succeeded, parsing output");
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
            var allowedModes = new[] { "Quiet", "Default", "Auto", "Performance" };
            if (!ValidateStringParameter(fanMode, allowedModes, "fanMode"))
            {
                return false;
            }

            bool success = true;
            
            switch (fanMode.ToLower())
            {
                case "quiet":
                    // Quiet mode: LegacyQuiet + FanMax=false
                    var quietResult1 = await ExecuteCommandAsync("-Bios FanMode=LegacyQuiet");
                    var quietResult2 = await ExecuteCommandAsync("-Bios FanMax=false");
                    success = quietResult1.Success && quietResult2.Success;
                    break;
                    
                case "default":
                case "auto":
                    // Default/Auto mode: LegacyDefault + FanMax=false
                    var defaultResult1 = await ExecuteCommandAsync("-Bios FanMode=LegacyDefault");
                    var defaultResult2 = await ExecuteCommandAsync("-Bios FanMax=false");
                    success = defaultResult1.Success && defaultResult2.Success;
                    break;
                    
                case "performance":
                    // Maximum mode: Performance + FanMax=true
                    var perfResult1 = await ExecuteCommandAsync("-Bios FanMode=Performance");
                    var perfResult2 = await ExecuteCommandAsync("-Bios FanMax=true");
                    success = perfResult1.Success && perfResult2.Success;
                    break;
                    
                default:
                    success = false;
                    break;
            }
            
            return success;
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
                Log.Warning("Invalid power limits: PL4 ({PL4}W) must be >= PL1 ({PL1}W)", pl4, pl1);
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
                Log.Error(ex, "Error setting CPU power limits");
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
                Log.Error(ex, "Error setting GPU power limit");
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
                Log.Error(ex, "Error setting keyboard color preset");
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
                Log.Error(ex, "Error setting keyboard animation");
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
            
            Log.Debug("ParseTemperatureData - Raw output: {Output}", output);
            
            if (string.IsNullOrWhiteSpace(output))
            {
                Log.Debug("ParseTemperatureData - Output is null or empty");
                return tempData;
            }
            
            // Parse temperature values from output
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Log.Debug("ParseTemperatureData - Found {LineCount} lines to parse", lines.Length);
            
            foreach (var line in lines)
            {
                Log.Debug("ParseTemperatureData - Processing line: {Line}", line);
                
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
                        Log.Debug("ParseTemperatureData - Found CPU temperature: {Temp}°C", temp);
                    }
                    else
                    {
                        Log.Debug("ParseTemperatureData - Failed to parse temperature from: {Line}", line);
                    }
                }
                // Also check for GPU Peak Temperature if present
                else if (line.Contains("GPU Peak Temperature") && line.Contains("[°C]"))
                {
                    var match = Regex.Match(line, @"=\s*(\d+)\s*\[°C\]");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int temp))
                    {
                        tempData.GpuTemperature = temp;
                        Log.Debug("ParseTemperatureData - Found GPU temperature: {Temp}°C", temp);
                    }
                }
            }
            
            Log.Debug("ParseTemperatureData - Final result - CPU: {CpuTemp}°C, GPU: {GpuTemp}°C", tempData.CpuTemperature, tempData.GpuTemperature);
            return tempData;
        }

        /// <summary>
        /// Parse temperature data from OmenMon EC output
        /// </summary>
        private TemperatureData ParseEcTemperatureData(string output)
        {
            var tempData = new TemperatureData();
            
            if (string.IsNullOrEmpty(output))
            {
                Log.Debug("ParseEcTemperatureData - Output is null or empty");
                return tempData;
            }

            Log.Debug("ParseEcTemperatureData - Parsing EC output: {Output}", output);
            
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                Log.Debug("ParseEcTemperatureData - Processing line: {Line}", trimmedLine);
                
                // Look for CPUT (CPU Temperature) - format: "- Register 0x57 Byte: 0x2c = 0b00101100 = 44 [CPUT]"
                if (trimmedLine.Contains("[CPUT]"))
                {
                    var match = Regex.Match(trimmedLine, @"=\s*(\d+)\s*\[CPUT\]");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int temp))
                    {
                        tempData.CpuTemperature = temp;
                        Log.Debug("ParseEcTemperatureData - Found CPU temperature: {Temp}°C", temp);
                    }
                }
                // Look for GPTM (GPU Temperature) - format: "- Register 0xb7 Byte: 0x2f = 0b00101111 = 47 [GPTM]"
                else if (trimmedLine.Contains("[GPTM]"))
                {
                    var match = Regex.Match(trimmedLine, @"=\s*(\d+)\s*\[GPTM\]");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int temp))
                    {
                        tempData.GpuTemperature = temp;
                        Log.Debug("ParseEcTemperatureData - Found GPU temperature: {Temp}°C", temp);
                    }
                }
            }
            
            Log.Debug("ParseEcTemperatureData - Final result - CPU: {CpuTemp}°C, GPU: {GpuTemp}°C", tempData.CpuTemperature, tempData.GpuTemperature);
            return tempData;
        }

        private FanData ParseFanData(string output)
        {
            var fanData = new FanData();
            
            Log.Debug("ParseFanData - Raw output: {Output}", output);
            
            if (string.IsNullOrWhiteSpace(output))
            {
                Log.Debug("ParseFanData - Output is null or empty");
                return fanData;
            }
            
            // Parse fan data from output
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Log.Debug("ParseFanData - Found {LineCount} lines to parse", lines.Length);
            
            int rpm1 = 0, rpm2 = 0, rpm3 = 0, rpm4 = 0;
            int xgs1 = 0, xgs2 = 0;
            
            foreach (var line in lines)
            {
                Log.Debug("ParseFanData - Processing line: {Line}", line);
                
                // Parse OmenMon fan count format: "- Fan Count: 0x02 = 0b00000010 = 2"
                if (line.Contains("Fan Count:"))
                {
                    var match = Regex.Match(line, @"=\s*(\d+)\s*$");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int count))
                    {
                        fanData.FanCount = count;
                        Log.Debug("ParseFanData - Found fan count: {Count}", count);
                    }
                    else
                    {
                        Log.Debug("ParseFanData - Failed to parse fan count from: {Line}", line);
                    }
                }
                // Parse RPM registers: "- Register 0x## Byte: 0x## = 0b######## = ## [RPM#]"
                else if (line.Contains("[RPM1]"))
                {
                    var match = Regex.Match(line, @"=\s*(\d+)\s*\[RPM1\]");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
                    {
                        rpm1 = value;
                        Log.Debug("ParseFanData - Found RPM1: {Rpm1}", rpm1);
                    }
                }
                else if (line.Contains("[RPM2]"))
                {
                    var match = Regex.Match(line, @"=\s*(\d+)\s*\[RPM2\]");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
                    {
                        rpm2 = value;
                        Log.Debug("ParseFanData - Found RPM2: {Rpm2}", rpm2);
                    }
                }
                else if (line.Contains("[RPM3]"))
                {
                    var match = Regex.Match(line, @"=\s*(\d+)\s*\[RPM3\]");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
                    {
                        rpm3 = value;
                        Log.Debug("ParseFanData - Found RPM3: {Rpm3}", rpm3);
                    }
                }
                else if (line.Contains("[RPM4]"))
                {
                    var match = Regex.Match(line, @"=\s*(\d+)\s*\[RPM4\]");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
                    {
                        rpm4 = value;
                        Log.Debug("ParseFanData - Found RPM4: {Rpm4}", rpm4);
                    }
                }
                else if (line.Contains("[XGS1]"))
                {
                    var match = Regex.Match(line, @"=\s*(\d+)\s*\[XGS1\]");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
                    {
                        xgs1 = value;
                        Log.Debug("ParseFanData - Found XGS1: {Xgs1}", xgs1);
                    }
                }
                else if (line.Contains("[XGS2]"))
                {
                    var match = Regex.Match(line, @"=\s*(\d+)\s*\[XGS2\]");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
                    {
                        xgs2 = value;
                        Log.Debug("ParseFanData - Found XGS2: {Xgs2}", xgs2);
                    }
                }
                // Parse fan mode: "- FanMode: 0x01 = 0b00000001 = 1 [Performance]"
                else if (line.Contains("FanMode:"))
                {
                    var match = Regex.Match(line, @"\[(\w+)\]");
                    if (match.Success)
                    {
                        fanData.FanMode = match.Groups[1].Value;
                        Log.Debug("ParseFanData - Found fan mode: {FanMode}", fanData.FanMode);
                    }
                }
            }
            
            // Combine RPM registers as little-endian 16-bit integers
            // RPM1/RPM2 for CPU fan (Fan1), RPM3/RPM4 for GPU fan (Fan2)
            fanData.Fan1Speed = rpm1 | (rpm2 << 8); // Little-endian: low byte + (high byte << 8)
            fanData.Fan2Speed = rpm3 | (rpm4 << 8); // Little-endian: low byte + (high byte << 8)
            
            // Store fan levels from XGS registers
            fanData.Fan1Level = xgs1;
            fanData.Fan2Level = xgs2;
            
            Log.Debug("ParseFanData - Final result - Count: {FanCount}, Fan1: {Fan1Speed} RPM (Level: {Xgs1}), Fan2: {Fan2Speed} RPM (Level: {Xgs2}), Mode: {FanMode}", fanData.FanCount, fanData.Fan1Speed, xgs1, fanData.Fan2Speed, xgs2, fanData.FanMode);
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
        /// Test OmenMon temperature monitoring method
        /// </summary>
        public async Task<string> TestTemperatureMethodsAsync()
        {
            var results = new List<string>();
            
            // Test OmenMon EC command
            try
            {
                var omenResult = await ExecuteCommandAsync("-Ec CPUT GPTM");
                if (omenResult.Success)
                {
                    var omenData = ParseEcTemperatureData(omenResult.Output);
                    results.Add($"OmenMon EC - CPU: {omenData.CpuTemperature}°C, GPU: {omenData.GpuTemperature}°C");
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
            // No resources to dispose currently
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