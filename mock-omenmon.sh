#!/bin/bash
# Mock OmenMon.exe for testing integration

if [[ "$1" == "-?" || "$1" == "-help" ]]; then
    echo "OmenMon Hardware Monitoring & Control Utility Version 0.61-Release"
    echo "Usage Information"
    echo "Usage: OmenMon [-<Arg1> [...] [-<ArgN> [...]]]"
    exit 0
elif [[ "$1" == "-Prog" ]]; then
    echo "Available fan programs:"
    echo "Silent"
    echo "Performance" 
    echo "Cool"
    echo "Auto"
    exit 0
elif [[ "$1" == "-Bios" ]]; then
    if [[ "$2" == *"Temp"* ]]; then
        echo "Temperature Data:"
        echo "CPU: 65°C"
        echo "GPU: 72°C"
    elif [[ "$2" == *"FanCount"* ]]; then
        echo "FanCount: 2"
    elif [[ "$2" == *"FanLevel"* ]]; then
        echo "FanLevel: 128,120"
    elif [[ "$2" == *"FanMode"* ]]; then
        echo "FanMode: Performance"
    elif [[ "$2" == *"Gpu"* ]]; then
        echo "Gpu: Performance"
        echo "GpuMode: Discrete"
    elif [[ "$2" == *"Backlight"* ]]; then
        echo "HasBacklight: True"
        echo "Backlight: On"
    elif [[ "$2" == *"Color"* ]]; then
        echo "Color: FF0000"
    fi
    exit 0
elif [[ "$1" == "-Ec" ]]; then
    echo "EC Register Data:"
    echo "0x30: 0x42"
    echo "0x31: 0x48"
    echo "0x32: 0x55"
    exit 0
fi

echo "Unknown command: $*"
exit 1