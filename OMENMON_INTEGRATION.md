# OmenMon Integration Setup

This HP Gaming Hub application integrates with the OmenMon.exe utility to provide hardware monitoring and control functionality for HP Omen devices.

## Prerequisites

1. **OmenMon.exe**: Download from the official OmenMon repository: https://github.com/OmenMon/OmenMon
2. **HP Omen Device**: This integration works specifically with HP Omen laptops and desktops
3. **Windows 10/11**: Required for WinUI 3 application and OmenMon hardware access
4. **Administrator Rights**: Some OmenMon operations require elevated privileges

## Installation

### Option 1: Automatic Setup (Recommended)

1. Run the HP Gaming Hub application
2. Navigate to Settings page
3. Click "Test OmenMon Connection" 
4. If OmenMon.exe is not found, the app will guide you through download and setup

### Option 2: Manual Setup

1. Download OmenMon.exe from the official GitHub release:
   ```
   https://github.com/OmenMon/OmenMon/releases/latest
   ```

2. Place OmenMon.exe in one of these locations:
   - Same folder as HP Gaming Hub executable
   - `C:\Program Files\OmenMon\OmenMon.exe`
   - `C:\Program Files (x86)\OmenMon\OmenMon.exe`
   - Or add OmenMon.exe to your system PATH

3. Test the installation by running in Command Prompt:
   ```cmd
   OmenMon.exe -?
   ```

## Features

### Fan Control
- **Fan Programs**: Select from predefined fan control programs
- **Manual Control**: Adjust individual fan speeds (CPU and GPU fans)
- **Real-time Monitoring**: View current fan speeds and modes

### Graphics Settings
- **GPU Mode**: Switch between Hybrid, Discrete, and Optimus modes
- **Performance Presets**: Choose from Minimum, Medium, or Maximum performance
- **Temperature Monitoring**: Real-time GPU temperature and usage

### Keyboard Control
- **Backlight Control**: Toggle keyboard backlight on/off
- **RGB Colors**: Set keyboard color presets (Red, Green, Blue, Purple, Orange, White)
- **Status Display**: View current backlight and color settings

### System Monitoring
- **CPU Monitoring**: Temperature, usage, and clock speed
- **GPU Monitoring**: Temperature, usage, and memory usage
- **Real-time Updates**: Automatic refresh every 5 seconds
- **Connection Status**: Monitor OmenMon.exe availability

## Supported OmenMon Commands

The application uses these OmenMon.exe CLI commands:

```bash
# Fan Control
OmenMon.exe -Prog                           # List fan programs
OmenMon.exe -Prog "ProgramName"            # Run fan program
OmenMon.exe -Bios FanLevel=speed1,speed2   # Set fan speeds

# GPU Control  
OmenMon.exe -Bios GpuMode=Hybrid          # Set GPU mode
OmenMon.exe -Bios Gpu=Maximum             # Set GPU preset

# Keyboard Control
OmenMon.exe -Bios Backlight=On            # Enable backlight
OmenMon.exe -Bios Color=FF0000            # Set color (hex)

# System Monitoring
OmenMon.exe -Bios Temp                    # Get temperatures
OmenMon.exe -Ec                           # Get EC register data
```

## Troubleshooting

### "OmenMon.exe not found"
- Ensure OmenMon.exe is installed and accessible
- Try running as Administrator
- Check Windows antivirus isn't blocking the executable

### "Connection Failed"
- Verify your device is a supported HP Omen model
- Ensure all HP Omen drivers are installed
- Try running HP Gaming Hub as Administrator

### Fan Control Not Working
- Some fan control requires Administrator privileges
- Verify your device supports the requested fan modes
- Check that no other fan control software is running

### GPU Settings Not Applying
- GPU mode changes may require a system restart
- Ensure your device has switchable graphics (hybrid GPU setup)
- Some settings may not be available on all GPU configurations

## Advanced Configuration

### Custom OmenMon Path
You can specify a custom path to OmenMon.exe in `appsettings.json`:

```json
{
  "OmenMonPath": "C:\\Custom\\Path\\To\\OmenMon.exe"
}
```

### Monitoring Interval
Adjust the system monitoring refresh rate:

```json
{
  "MonitoringIntervalSeconds": 5
}
```

## Safety Notes

⚠️ **Important Safety Information:**

- **Fan Control**: Manual fan control can cause overheating if set too low. Use with caution.
- **GPU Settings**: GPU mode changes affect system performance and battery life.
- **Administrator Rights**: Some operations require elevated privileges for hardware access.
- **Hardware Compatibility**: This integration is designed for HP Omen devices only.

## Support

For issues with the HP Gaming Hub application, please create an issue in this repository.

For OmenMon.exe specific issues, please refer to the official OmenMon repository:
https://github.com/OmenMon/OmenMon

## License

This integration respects the licensing of both projects:
- HP Gaming Hub: [Your License]
- OmenMon: GPL-3.0 License