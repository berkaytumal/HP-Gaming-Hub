# HP Gaming Hub with OmenMon Integration

A modern WinUI 3 application that provides a user-friendly interface for HP Omen hardware monitoring and control through integration with [OmenMon.exe](https://github.com/OmenMon/OmenMon).

## 🚀 Features

### Hardware Control
- **Fan Management**: Control fan speeds with predefined programs or manual adjustment
- **GPU Settings**: Switch GPU modes (Hybrid/Discrete/Optimus) and performance presets
- **Keyboard RGB**: Control backlight and color settings
- **Real-time Monitoring**: Live CPU/GPU temperature, usage, and system stats

### Modern UI
- **WinUI 3**: Native Windows 11 design with Mica backdrop
- **Responsive Layout**: Adaptive interface with smooth page transitions
- **Dark/Light Theme**: Follows system theme preferences
- **Real-time Updates**: Automatic data refresh every 5 seconds

## 📋 Requirements

- **Windows 10 1903+** or **Windows 11**
- **HP Omen Device** (laptop or desktop)
- **OmenMon.exe** (downloaded automatically or manually)
- **.NET 8 Runtime** (Windows App Runtime included)

## 🛠️ Installation

### Quick Setup

1. **Download** the latest release from the [Releases page](../../releases)
2. **Extract** the ZIP file to your preferred location
3. **Run** `Setup-OmenMon.ps1` as Administrator to automatically download OmenMon.exe
4. **Launch** `HP Gaming Hub.exe`

### Manual Setup

If the automatic setup doesn't work:

1. Download OmenMon.exe from [OmenMon Releases](https://github.com/OmenMon/OmenMon/releases/latest)
2. Place it in one of these locations:
   - Same folder as HP Gaming Hub
   - `C:\Program Files\OmenMon\OmenMon.exe`
   - Add to system PATH

See [OMENMON_INTEGRATION.md](OMENMON_INTEGRATION.md) for detailed setup instructions.

## 🎮 Usage

### Fan Control
Navigate to the **Fan** page to:
- Select from available fan programs (Silent, Performance, Cool, Auto)
- Manually adjust individual fan speeds
- Monitor current fan status and mode

### Graphics Settings  
Use the **Graphics** page to:
- Switch GPU modes for power/performance balance
- Apply performance presets (Minimum, Medium, Maximum)
- Monitor GPU temperature and usage

### Keyboard RGB
Configure RGB lighting on the **Keyboard** page:
- Toggle backlight on/off
- Choose from color presets
- View current lighting status

### System Monitoring
The **Monitor** page provides:
- Real-time CPU and GPU temperatures
- Usage percentages and clock speeds
- Connection status with OmenMon
- Manual data refresh options

## 🔧 Technical Details

### Architecture
```
HP Gaming Hub (WinUI 3)
├── ViewModels/ (MVVM Pattern)
├── Services/ (OmenMon Integration)
├── Converters/ (Data Binding)
└── App Shell (Navigation)
```

### OmenMon Integration
The app communicates with OmenMon.exe using these commands:
- `-Prog` - Fan program management
- `-Bios` - Hardware control (GPU, fans, keyboard)
- `-Ec` - Embedded controller monitoring
- `-Temp` - Temperature sensors

### Safety Features
- **Temperature Monitoring**: Prevents dangerous fan speed settings
- **Error Handling**: Graceful fallback when OmenMon is unavailable
- **Admin Detection**: Warns when elevated privileges are needed
- **Connection Status**: Real-time monitoring of OmenMon availability

## 🐛 Troubleshooting

### Common Issues

**"OmenMon.exe not found"**
- Run the Setup-OmenMon.ps1 script as Administrator
- Verify your antivirus isn't blocking the executable
- Check the paths listed in the integration guide

**Fan control not working**
- Run HP Gaming Hub as Administrator
- Ensure no other fan control software is running
- Verify your device supports the requested fan modes

**GPU settings not applying**
- Some changes require a system restart
- Ensure your device has switchable graphics
- Check that you have the latest GPU drivers

### Debug Mode
Set `EnableDebugMode: true` in appsettings.json for detailed logging.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [OmenMon](https://github.com/OmenMon/OmenMon) - The core hardware monitoring utility
- [WinUI 3](https://docs.microsoft.com/en-us/windows/apps/winui/) - Modern Windows application framework
- HP Omen Community - For hardware insights and testing

## 📞 Support

- **Issues**: [GitHub Issues](../../issues)
- **Discussions**: [GitHub Discussions](../../discussions)
- **OmenMon Support**: [OmenMon Repository](https://github.com/OmenMon/OmenMon)

---

⚠️ **Important**: This software controls hardware directly. Use responsibly and ensure proper cooling when adjusting fan speeds manually.