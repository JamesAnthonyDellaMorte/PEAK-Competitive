# PEAK Competitive

A BepInEx mod for PEAK that adds competitive gameplay features.

## Features

- **Timer System**: Track your climb times with precision
- **Leaderboards**: Compare times with other players
- **Race Modes**: Compete against others in real-time
- **Match Statistics**: Detailed stats for each climb
- **Configurable Settings**: Customize competitive features to your preference

## Installation

### Automatic (Mod Manager)
1. Install via Thunderstore Mod Manager
2. Launch PEAK

### Manual Installation
1. Install [BepInEx 5.4.21+](https://thunderstore.io/c/peak/p/BepInEx/BepInExPack_PEAK/)
2. Download the latest release
3. Extract to your PEAK game directory
4. The file structure should look like:
   ```
   PEAK/
   ├── BepInEx/
   │   └── plugins/
   │       └── PEAKCompetitive/
   │           └── PEAKCompetitive.dll
   ```

## Configuration

Press **F3** in-game to open the competitive settings menu (key rebindable in config).

Configuration file location: `BepInEx/config/PEAKCompetitive.cfg`

### Settings

- **Enable Timer**: Show timer during climbs
- **Enable Leaderboards**: Track and display leaderboards
- **Race Mode**: Enable competitive race features
- **Show Stats**: Display match statistics
- **Config Menu Key**: Key to open settings (default: F3)

## Usage

### Timer System
- Timer starts automatically when you begin climbing
- Displays current time in corner of screen
- Saves personal best times

### Race Mode
- Host enables race mode in settings
- All players see synchronized timer
- First to summit wins
- Results displayed at end screen

### Leaderboards
- Automatic tracking of best times
- Per-mountain leaderboards
- View with `/leaderboard` command or in-game menu

## Compatibility

- **Host-Only**: Only the host needs this mod installed
- **Multiplayer**: Fully compatible with multiplayer gameplay
- **Other Mods**: Compatible with PEAK Unlimited and other popular mods

## Support

Report issues at: [GitHub Issues](https://github.com/yourusername/PEAK-Competitive/issues)

## Credits

Developed by [Your Name]

## License

MIT License - See LICENSE file for details
