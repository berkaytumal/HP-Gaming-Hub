# Contributing to HP Gaming Hub

Thank you for your interest in contributing to HP Gaming Hub! This document provides guidelines and best practices for contributing to the project.

## Getting Started

1. Fork the repository
2. Clone your fork locally
3. Create a new branch for your feature or bug fix
4. Make your changes following the guidelines below
5. Test your changes thoroughly
6. Submit a pull request

## Development Guidelines

### Logging Standards

**⚠️ Important: Use Serilog for all logging operations**

This project uses [Serilog](https://serilog.net/) for structured logging. Please follow these guidelines:

#### ✅ DO:
```csharp
// Use Serilog directly
Log.Information("Application started successfully");
Log.Warning("GPU temperature is high: {Temperature}°C", temperature);
Log.Error("Failed to initialize hardware monitoring");
Log.Debug("Processing fan curve data: {@FanCurve}", fanCurve);
```

#### ❌ DON'T:
```csharp
// Avoid these - they don't integrate with our logging system
Console.WriteLine("Debug message");
Debug.WriteLine("Debug info");
System.Diagnostics.Trace.WriteLine("Trace message");
```

#### Available Logging Methods:
- `Log.Information(string message, params object[] args)` - General information
- `Log.Warning(string message, params object[] args)` - Warnings and potential issues
- `Log.Error(string message, params object[] args)` - Errors and exceptions
- `Log.Debug(string message, params object[] args)` - Debug information (development only)

#### Log Configuration
Logs are automatically managed with:
- **Daily rotation**: New log file created each day
- **7-day retention**: Older logs are automatically deleted
- **Location**: `%LOCALAPPDATA%\Packages\[AppPackage]\LocalState\Logs\`
- **Format**: Structured JSON with timestamps and log levels

### Code Style

- Follow C# naming conventions
- Use meaningful variable and method names
- Add XML documentation comments for public methods
- Keep methods focused and concise
- Use async/await for I/O operations

### UI Guidelines

- Follow the existing XAML structure and naming conventions
- Use the established color scheme and styling
- Ensure UI elements are accessible
- Test on different screen resolutions
- Maintain consistency with existing UI patterns

### Hardware Integration

- Always handle hardware communication errors gracefully
- Use appropriate logging levels for hardware events
- Test with different HP hardware configurations when possible
- Follow the existing service patterns in `Services/` folder

## Project Structure

```
HP Gaming Hub/
├── MainWindow.*.cs          # Main window partial classes
├── Services/                # Hardware communication services
├── ViewModels/             # MVVM view models
├── Assets/                 # Images, icons, and resources
└── App.xaml.cs            # Application startup and configuration
```

## Testing

- Test your changes on actual HP gaming hardware when possible
- Verify that existing functionality still works
- Check that logs are properly generated
- Test both elevated and non-elevated scenarios

## Pull Request Guidelines

1. **Clear Description**: Explain what your PR does and why
2. **Small, Focused Changes**: Keep PRs focused on a single feature or fix
3. **Follow Conventions**: Ensure your code follows the project's style
4. **Test Thoroughly**: Verify your changes work as expected
5. **Update Documentation**: Update relevant documentation if needed

### PR Checklist

- [ ] Code follows the logging guidelines (uses Serilog, not Console.WriteLine)
- [ ] No debug code or temporary changes left in
- [ ] UI changes are consistent with existing design
- [ ] Hardware operations include proper error handling
- [ ] Changes have been tested on target hardware
- [ ] Documentation updated if necessary

## Common Issues

### Hardware Communication
Always wrap hardware communication in try-catch blocks and log appropriate messages:

```csharp
try
{
    // Hardware operation
    var result = await hardwareService.GetDataAsync();
    Log.Information("Hardware data retrieved successfully");
}
catch (Exception ex)
{
    Log.Error("Failed to retrieve hardware data: {Error}", ex.Message);
    // Handle gracefully
}
```

## Questions?

If you have questions about contributing, please:
1. Check existing issues and discussions
2. Create a new issue with the "question" label
3. Provide as much context as possible

Thank you for contributing to HP Gaming Hub! 🎮