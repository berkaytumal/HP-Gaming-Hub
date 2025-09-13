using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;

namespace HP_Gaming_Hub.Services
{
    /// <summary>
    /// Windows API-based temperature monitoring service using LibreHardwareMonitor
    /// Alternative to OmenMon for systems that support standard Windows hardware monitoring
    /// </summary>
    public class WindowsApiTemperatureService : IDisposable
    {
        private Computer _computer;
        private readonly UpdateVisitor _updateVisitor;
        private bool _isInitialized;
        private bool _disposed;

        public WindowsApiTemperatureService()
        {
            _updateVisitor = new UpdateVisitor();
            InitializeComputer();
        }

        private void InitializeComputer()
        {
            try
            {
                _computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = false, // Not needed for temperature monitoring
                    IsMotherboardEnabled = false, // Not needed for basic temperature monitoring
                    IsControllerEnabled = false, // Not needed for temperature monitoring
                    IsNetworkEnabled = false, // Not needed for temperature monitoring
                    IsStorageEnabled = false // Not needed for temperature monitoring
                };

                _computer.Open();
                _isInitialized = true;
                Debug.WriteLine("[WindowsApiTemperatureService] Successfully initialized LibreHardwareMonitor");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WindowsApiTemperatureService] Failed to initialize: {ex.Message}");
                _isInitialized = false;
            }
        }

        /// <summary>
        /// Get CPU and GPU temperatures using Windows API
        /// </summary>
        public async Task<TemperatureData> GetTemperaturesAsync()
        {
            return await Task.Run(() =>
            {
                var tempData = new TemperatureData();

                if (!_isInitialized || _computer == null)
                {
                    Debug.WriteLine("[WindowsApiTemperatureService] Service not initialized");
                    return tempData;
                }

                try
                {
                    // Update all hardware sensors
                    _computer.Accept(_updateVisitor);

                    // Get CPU temperature
                    foreach (var hardware in _computer.Hardware)
                    {
                        if (hardware.HardwareType == HardwareType.Cpu)
                        {
                            hardware.Update();
                            foreach (var sensor in hardware.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                                {
                                    // Get the package/core temperature (usually the first temperature sensor)
                                    if (sensor.Name.Contains("Package") || sensor.Name.Contains("Core") || 
                                        sensor.Name.Contains("CPU") || tempData.CpuTemperature == 0)
                                    {
                                        tempData.CpuTemperature = (int)Math.Round(sensor.Value.Value);
                                        Debug.WriteLine($"[WindowsApiTemperatureService] CPU Temperature: {tempData.CpuTemperature}°C from {sensor.Name}");
                                        break; // Use the first valid CPU temperature
                                    }
                                }
                            }
                        }
                        else if (hardware.HardwareType == HardwareType.GpuNvidia || 
                                hardware.HardwareType == HardwareType.GpuAmd || 
                                hardware.HardwareType == HardwareType.GpuIntel)
                        {
                            hardware.Update();
                            foreach (var sensor in hardware.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                                {
                                    // Get GPU core temperature
                                    if (sensor.Name.Contains("GPU") || sensor.Name.Contains("Core") || 
                                        sensor.Name.Contains("Temperature") || tempData.GpuTemperature == 0)
                                    {
                                        tempData.GpuTemperature = (int)Math.Round(sensor.Value.Value);
                                        Debug.WriteLine($"[WindowsApiTemperatureService] GPU Temperature: {tempData.GpuTemperature}°C from {sensor.Name}");
                                        break; // Use the first valid GPU temperature
                                    }
                                }
                            }
                        }
                    }

                    Debug.WriteLine($"[WindowsApiTemperatureService] Final temperatures - CPU: {tempData.CpuTemperature}°C, GPU: {tempData.GpuTemperature}°C");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[WindowsApiTemperatureService] Error getting temperatures: {ex.Message}");
                }

                return tempData;
            });
        }

        /// <summary>
        /// Check if the service is available and working
        /// </summary>
        public bool IsAvailable()
        {
            return _isInitialized && _computer != null;
        }

        /// <summary>
        /// Get detailed hardware information for debugging
        /// </summary>
        public async Task<string> GetHardwareInfoAsync()
        {
            return await Task.Run(() =>
            {
                if (!_isInitialized || _computer == null)
                    return "Service not initialized";

                var info = new List<string>();
                
                try
                {
                    _computer.Accept(_updateVisitor);
                    
                    foreach (var hardware in _computer.Hardware)
                    {
                        info.Add($"Hardware: {hardware.Name} ({hardware.HardwareType})");
                        
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                            {
                                info.Add($"  - {sensor.Name}: {sensor.Value.Value:F1}°C");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    info.Add($"Error: {ex.Message}");
                }

                return string.Join(Environment.NewLine, info);
            });
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _computer?.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[WindowsApiTemperatureService] Error during disposal: {ex.Message}");
                }
                finally
                {
                    _disposed = true;
                }
            }
        }
    }

    /// <summary>
    /// Visitor pattern implementation for updating hardware sensors
    /// </summary>
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware)
                subHardware.Accept(this);
        }

        public void VisitSensor(ISensor sensor)
        {
            // No action needed for sensors in this implementation
        }

        public void VisitParameter(IParameter parameter)
        {
            // No action needed for parameters in this implementation
        }
    }
}