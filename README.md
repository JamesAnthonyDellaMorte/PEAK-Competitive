# PEAK Competitive

A BepInEx mod for PEAK that adds competitive gameplay features.

## Features

- **Duo Team Racing**: 2v2 (or configurable) teams race to checkpoints
- **Round-Based Gameplay**: First team to checkpoint wins the round
- **Point Scoring System**: Different biomes worth different points (Shore=1pt, Kiln=5pts)
- **Team-Isolated Ghosts**: Losing teams become ghosts but only see/help their own team
- **Items Persist**: All players keep gear between rounds (fair competition)
- **Match Victory**: First to Kiln with most points wins, or last team standing
- **Live Scoreboard**: Track team scores and round progress
- **Configurable Settings**: Customize team sizes, point values, and match rules

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

- **Enable Competitive Mode**: Toggle team race features
- **Max Teams**: Number of teams (2-10, default: 2)
- **Players Per Team**: Team size (1-4, default: 2)
- **Items Persist**: Items carry between rounds (default: true)
- **Show Scoreboard**: Display live scoreboard (default: true)
- **Map Points**: Configure points per biome (Shore, Tropics/Roots, Alpine/Mesa, Caldera, Kiln)
- **Config Menu Key**: Key to open settings (default: F3)

## How It Works

### Match Flow
1. **Host starts match** via F3 menu
2. **Teams assigned** - Players split into teams (auto or manual)
3. **Round begins** - Teams race to checkpoint campfire
4. **First team wins** - First to checkpoint gets points based on biome difficulty
5. **Losers become ghosts** - Can only see/help their own team
6. **Round resets** - All players revive at next biome checkpoint
7. **Repeat** until Kiln reached or all teams eliminated
8. **Match ends** - Team with most points wins!

### Team Ghost System
When your team loses a round:
- You become a ghost (PEAK's existing ghost mode)
- You can ONLY see and interact with your own team
- Help your teammates navigate, scout, and strategize
- Revive at the next checkpoint for the next round
- Other teams cannot see or hear you

### Scoring
- **Shore** (Beach): 1 point
- **Tropics/Roots** (Jungle/Forest): 2 points
- **Alpine/Mesa** (Snow/Desert): 3 points
- **Caldera** (Volcano): 4 points
- **Kiln** (Inner Volcano): 5 points

Points are configurable per map!

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
