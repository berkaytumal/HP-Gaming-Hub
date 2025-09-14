<div align="center">
  <img src="Assets/icon.svg" alt="Fluent Gaming Hub" width="128" height="128">
  
  # HP Gaming Hub
  
  A comprehensive Windows application for controlling HP gaming laptop hardware features including fan control, GPU performance, keyboard lighting, and system monitoring.
  
  **Powered by OmenMon**
</div>

## Currently Supported Features

### Fan Control
- [x] **Quiet Mode**: Minimal fan speed for silent operation
- [x] **Auto Mode**: Automatic fan speed based on temperature sensors
- [x] **Max Mode**: Maximum fan speed for intensive workloads
- [x] Real-time temperature monitoring
- [x] Hardware monitoring with dynamic refresh intervals
- [x] Window focus-aware monitoring (different refresh rates when focused/unfocused)

### System Monitoring
- [x] Real-time hardware temperature display
- [x] CPU and GPU monitoring
- [x] Automatic monitoring startup (configurable)
- [x] Customizable refresh intervals
- [x] Comprehensive logging system

### Settings & Customization
- [x] Multiple backdrop options (Mica, Acrylic, Custom Wallpapers)
- [x] Persistent user preferences
- [x] Auto-start monitoring configuration
- [x] Adjustable monitoring intervals

## Planned Features

### Advanced Fan Control
- [ ] **Custom Fan Curves**: Create personalized fan speed profiles based on temperature thresholds with visual curve editor
- [ ] **Multiple Profiles**: Save and switch between different fan configurations

### GPU Performance Control
- [ ] **Performance Modes**: Switch between power-saving, balanced, and performance modes
- [ ] **Power Limit Management**: Configure TDP settings

### Keyboard Lighting Control
- [ ] **RGB Customization**: Full color spectrum control
- [ ] **Lighting Effects**: Breathing, wave, static, and custom patterns
- [ ] **Per-Key Control**: Individual key lighting (on supported models)
- [ ] **Game Integration**: Reactive lighting based on system events

### User Experience Enhancements
- [ ] **Quick Control Popup**: Compact overlay for instant access to key controls
- [ ] **System Tray Integration**: Minimize to tray with quick actions
- [ ] **Hotkey Support**: Keyboard shortcuts for common functions

### System Integration
- [ ] **Windows Startup Service**: Automatic launch with Windows
- [ ] **Background Operation**: Run silently in the background
- [ ] **Windows Notifications**: System alerts for temperature warnings
- [ ] **Game Mode Detection**: Automatic profile switching during gaming

## Technical Requirements

- Windows 10/11 (x64)
- HP Gaming Laptop with OmenMon support
- .NET 8.0 Runtime
- Administrator privileges (for hardware control)

## Installation

1. Download the latest release from the releases page
2. Run the MSIX with App Installer
3. Launch HP Gaming Hub from the Start Menu
4. Configure your preferences in the Settings page

## Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Disclaimer

This software directly controls hardware components. Use at your own risk. Always monitor temperatures and ensure proper cooling when using custom fan curves or overclocking features.

---

**Note**: This application is designed specifically for HP gaming laptops and requires compatible hardware. Some features may not be available on all models.