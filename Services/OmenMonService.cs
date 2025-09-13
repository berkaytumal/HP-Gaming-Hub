using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace HP_Gaming_Hub.Services
{
    public class OmenMonService
    {
        private readonly string _omenMonPath;
        private readonly IConfiguration _configuration;

        public OmenMonService(IConfiguration configuration)
        {
            _configuration = configuration;
            _omenMonPath = FindOmenMonExecutable();
        }

        private string FindOmenMonExecutable()
        {
            // Check common locations for OmenMon.exe
            var possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OmenMon.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "OmenMon", "OmenMon.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "OmenMon", "OmenMon.exe"),
                // For testing/development - check for mock script
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mock-omenmon.sh"),
                "OmenMon.exe" // If in PATH
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                    return path;
            }

            return "OmenMon.exe"; // Fallback to PATH lookup
        }

        public async Task<bool> IsOmenMonAvailableAsync()
        {
            try
            {
                var result = await ExecuteOmenMonCommandAsync("-?");
                return result.Success;
            }
            catch
            {
                return false;
            }
        }

        public async Task<OmenMonResult> ExecuteOmenMonCommandAsync(string arguments)
        {
            try
            {
                using var process = new Process();
                process.StartInfo.FileName = _omenMonPath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();

                return new OmenMonResult
                {
                    Success = process.ExitCode == 0,
                    Output = output,
                    Error = error,
                    ExitCode = process.ExitCode
                };
            }
            catch (Exception ex)
            {
                return new OmenMonResult
                {
                    Success = false,
                    Error = ex.Message,
                    ExitCode = -1
                };
            }
        }

        // Fan Control Methods
        public async Task<List<string>> GetFanProgramsAsync()
        {
            var result = await ExecuteOmenMonCommandAsync("-Prog");
            if (!result.Success)
                return new List<string>();

            var programs = new List<string>();
            var lines = result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith("OmenMon") && !trimmed.StartsWith("Usage"))
                {
                    programs.Add(trimmed);
                }
            }

            return programs;
        }

        public async Task<bool> RunFanProgramAsync(string programName)
        {
            var result = await ExecuteOmenMonCommandAsync($"-Prog \"{programName}\"");
            return result.Success;
        }

        public async Task<FanInfo> GetFanInfoAsync()
        {
            var result = await ExecuteOmenMonCommandAsync("-Bios FanCount FanLevel FanMax FanMode FanType");
            if (!result.Success)
                return new FanInfo();

            return ParseFanInfo(result.Output);
        }

        public async Task<bool> SetFanLevelAsync(int fan1Speed, int fan2Speed)
        {
            var result = await ExecuteOmenMonCommandAsync($"-Bios FanLevel={fan1Speed},{fan2Speed}");
            return result.Success;
        }

        // GPU Control Methods
        public async Task<GpuInfo> GetGpuInfoAsync()
        {
            var result = await ExecuteOmenMonCommandAsync("-Bios Gpu GpuMode");
            if (!result.Success)
                return new GpuInfo();

            return ParseGpuInfo(result.Output);
        }

        public async Task<bool> SetGpuModeAsync(string mode)
        {
            var result = await ExecuteOmenMonCommandAsync($"-Bios GpuMode={mode}");
            return result.Success;
        }

        public async Task<bool> SetGpuPresetAsync(string preset)
        {
            var result = await ExecuteOmenMonCommandAsync($"-Bios Gpu={preset}");
            return result.Success;
        }

        // Keyboard Control Methods
        public async Task<KeyboardInfo> GetKeyboardInfoAsync()
        {
            var result = await ExecuteOmenMonCommandAsync("-Bios KbdType HasBacklight Backlight Color");
            if (!result.Success)
                return new KeyboardInfo();

            return ParseKeyboardInfo(result.Output);
        }

        public async Task<bool> SetBacklightAsync(bool enabled)
        {
            var state = enabled ? "On" : "Off";
            var result = await ExecuteOmenMonCommandAsync($"-Bios Backlight={state}");
            return result.Success;
        }

        public async Task<bool> SetColorAsync(string color)
        {
            var result = await ExecuteOmenMonCommandAsync($"-Bios Color={color}");
            return result.Success;
        }

        // System Monitoring Methods
        public async Task<SystemInfo> GetSystemInfoAsync()
        {
            var result = await ExecuteOmenMonCommandAsync("-Bios Temp System Cpu:PL1 Cpu:PL4 Cpu:PLGpu");
            if (!result.Success)
                return new SystemInfo();

            return ParseSystemInfo(result.Output);
        }

        public async Task<EcMonitorData> StartEcMonitoringAsync()
        {
            var result = await ExecuteOmenMonCommandAsync("-Ec");
            if (!result.Success)
                return new EcMonitorData();

            return ParseEcData(result.Output);
        }

        // Parsing Methods
        private FanInfo ParseFanInfo(string output)
        {
            var fanInfo = new FanInfo();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Contains("FanCount"))
                {
                    var parts = trimmed.Split(':');
                    if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int count))
                        fanInfo.FanCount = count;
                }
                else if (trimmed.Contains("FanLevel"))
                {
                    var parts = trimmed.Split(':');
                    if (parts.Length > 1)
                    {
                        var levels = parts[1].Split(',');
                        if (levels.Length >= 2)
                        {
                            int.TryParse(levels[0].Trim(), out fanInfo.Fan1Speed);
                            int.TryParse(levels[1].Trim(), out fanInfo.Fan2Speed);
                        }
                    }
                }
                else if (trimmed.Contains("FanMode"))
                {
                    var parts = trimmed.Split(':');
                    if (parts.Length > 1)
                        fanInfo.FanMode = parts[1].Trim();
                }
            }

            return fanInfo;
        }

        private GpuInfo ParseGpuInfo(string output)
        {
            var gpuInfo = new GpuInfo();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Contains("GpuMode"))
                {
                    var parts = trimmed.Split(':');
                    if (parts.Length > 1)
                        gpuInfo.Mode = parts[1].Trim();
                }
                else if (trimmed.Contains("Gpu"))
                {
                    var parts = trimmed.Split(':');
                    if (parts.Length > 1)
                        gpuInfo.Preset = parts[1].Trim();
                }
            }

            return gpuInfo;
        }

        private KeyboardInfo ParseKeyboardInfo(string output)
        {
            var kbdInfo = new KeyboardInfo();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Contains("HasBacklight"))
                {
                    var parts = trimmed.Split(':');
                    if (parts.Length > 1)
                        kbdInfo.HasBacklight = parts[1].Trim().ToLower() == "true";
                }
                else if (trimmed.Contains("Backlight"))
                {
                    var parts = trimmed.Split(':');
                    if (parts.Length > 1)
                        kbdInfo.BacklightEnabled = parts[1].Trim().ToLower() == "on";
                }
                else if (trimmed.Contains("Color"))
                {
                    var parts = trimmed.Split(':');
                    if (parts.Length > 1)
                        kbdInfo.CurrentColor = parts[1].Trim();
                }
            }

            return kbdInfo;
        }

        private SystemInfo ParseSystemInfo(string output)
        {
            var sysInfo = new SystemInfo();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Contains("Temp"))
                {
                    // Parse temperature data
                    var parts = trimmed.Split(':');
                    if (parts.Length > 1)
                    {
                        var tempData = parts[1].Trim();
                        // Extract CPU and GPU temperatures from the data
                        if (tempData.Contains("CPU"))
                        {
                            var cpuIndex = tempData.IndexOf("CPU");
                            var cpuTemp = ExtractNumberFromString(tempData.Substring(cpuIndex));
                            if (cpuTemp.HasValue) sysInfo.CpuTemperature = cpuTemp.Value;
                        }
                        if (tempData.Contains("GPU"))
                        {
                            var gpuIndex = tempData.IndexOf("GPU");
                            var gpuTemp = ExtractNumberFromString(tempData.Substring(gpuIndex));
                            if (gpuTemp.HasValue) sysInfo.GpuTemperature = gpuTemp.Value;
                        }
                    }
                }
            }

            return sysInfo;
        }

        private EcMonitorData ParseEcData(string output)
        {
            var ecData = new EcMonitorData();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            ecData.Registers = new Dictionary<string, int>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Contains(":"))
                {
                    var parts = trimmed.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        var register = parts[0].Trim();
                        var valueStr = parts[1].Trim();
                        if (int.TryParse(valueStr, System.Globalization.NumberStyles.HexNumber, null, out int value))
                        {
                            ecData.Registers[register] = value;
                        }
                    }
                }
            }

            return ecData;
        }

        private int? ExtractNumberFromString(string input)
        {
            var numbers = System.Text.RegularExpressions.Regex.Matches(input, @"\d+");
            if (numbers.Count > 0 && int.TryParse(numbers[0].Value, out int result))
                return result;
            return null;
        }
    }

    // Data Models
    public class OmenMonResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int ExitCode { get; set; }
    }

    public class FanInfo
    {
        public int FanCount { get; set; }
        public int Fan1Speed { get; set; }
        public int Fan2Speed { get; set; }
        public string FanMode { get; set; } = string.Empty;
        public bool IsMaxPerformance { get; set; }
    }

    public class GpuInfo
    {
        public string Mode { get; set; } = string.Empty;
        public string Preset { get; set; } = string.Empty;
        public int Temperature { get; set; }
        public int Usage { get; set; }
        public string Memory { get; set; } = string.Empty;
    }

    public class KeyboardInfo
    {
        public bool HasBacklight { get; set; }
        public bool BacklightEnabled { get; set; }
        public string CurrentColor { get; set; } = string.Empty;
        public List<string> AvailableColors { get; set; } = new();
    }

    public class SystemInfo
    {
        public int CpuTemperature { get; set; }
        public int GpuTemperature { get; set; }
        public int CpuUsage { get; set; }
        public int GpuUsage { get; set; }
        public string CpuClockSpeed { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class EcMonitorData
    {
        public Dictionary<string, int> Registers { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}