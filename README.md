# Overstrike

A modern EverQuest overlay application built with C# and WPF, converted from the original Go version.

## Features

- **Real-time Log Parsing**: Monitors EverQuest log files for damage events
- **Damage Popups**: Configurable floating damage notifications
- **DPS Meter**: Real-time damage per second calculations and display
- **Audio Notifications**: Sound alerts for critical hits and damage events
- **Customizable Overlays**: Configurable placement, colors, and fonts
- **Multi-damage Types**: Support for melee, spell, heal, and rune damage

## Requirements

- .NET 8.0 or later
- Windows 10/11
- EverQuest (for log file generation)

## Installation

1. Download the latest release from the [Releases page](https://github.com/BND10706/OverstrikeTHJ/releases)
2. Extract the archive to your desired location
3. Run `Overstrike.exe`
4. Configure your EverQuest log file path in the settings

## Configuration

1. Launch Overstrike
2. Click "Select Log File" and browse to your EverQuest log file (typically `eqlog_CharacterName_ServerName.txt`)
3. Configure damage popup placements in the Configuration window
4. Adjust audio and display settings as needed

## Building from Source

```bash
git clone https://github.com/BND10706/OverstrikeTHJ.git
cd OverstrikeTHJ
dotnet build
```

## Usage

1. Start EverQuest and ensure logging is enabled (`/log on`)
2. Launch Overstrike
3. Select your character's log file
4. Click "Start Tracking" to begin monitoring
5. The DPS window and damage popups will appear during combat

## Features Overview

### Damage Popups

- Configurable for different damage types (melee, spell, heal, miss)
- Separate settings for incoming and outgoing damage
- Customizable colors, fonts, and animation directions
- Critical hit highlighting with special effects

### DPS Meter

- Real-time damage calculations
- Player ranking and statistics
- Hit rate and critical hit percentages
- Configurable calculation windows

### Audio System

- Different sounds for damage types
- Critical hit audio notifications
- Volume control and enable/disable options

## Configuration Files

- `overstrike.ini` - Main configuration file
- Contains all placement, audio, and display settings
- Automatically created on first run

## Original Project

This C# version is converted from the original Go project: [CritSprinkler](https://github.com/xackery/critsprinkler)

## License

This project is open source. See the LICENSE file for details.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## Support

For issues, questions, or feature requests, please use the [GitHub Issues](https://github.com/BND10706/OverstrikeTHJ/issues) page.
