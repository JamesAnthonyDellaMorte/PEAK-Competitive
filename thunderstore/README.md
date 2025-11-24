# PEAK Competitive

Competitive multiplayer racing mod for PEAK with team-based scoring, survivor bonuses, and difficulty-scaled points.

## Features

### 🏁 Competitive Team Racing
- **Duo Teams (2v2)** - Configurable team sizes (1-4 players per team)
- **Round-Based Gameplay** - First team to checkpoint wins the round
- **Match Victory** - First to summit with most points wins

### 📊 Difficulty-Based Scoring

| Biome | Difficulty | Base Points |
|-------|------------|-------------|
| Shore | ★☆☆☆☆ | 1 pt |
| Tropics | ★★☆☆☆ | 2 pts |
| Mesa | ★★★☆☆ | 3 pts |
| Alpine | ★★★★☆ | 4 pts |
| Roots | ★★★★☆ | 4 pts |
| Caldera | ★★★★★ | 5 pts |
| Kiln | ★★★★★+ | 6 pts |

### 💀 Survivor Bonus System

**MORE SURVIVORS = MORE POINTS!**

Formula: `Total = Base + (Base × 0.5 × Survivors) + Full Team Bonus`

Example - Alpine (4 pts), 2-player team:
- 2/2 survive: **12 pts** (300% of base!)
- 1/2 survive: **6 pts** (150% of base)
- 0/2 survive: **4 pts** (100% of base)

**Keeping your team alive triples your score!**

### 👻 Team Ghost Mechanics
- Losing teams become ghosts
- Ghosts only see/help their own teammates
- Isolated from opposing teams
- Can revive to help team survive

## Installation

### Automatic (Recommended)

1. Install with a mod manager (r2modman, Thunderstore Mod Manager)
2. Launch PEAK
3. Press **F3** to configure settings

### Manual

1. Install [BepInEx 5.4.21+](https://github.com/BepInEx/BepInEx/releases)
2. Download `PEAKCompetitive.dll`
3. Place in `PEAK/BepInEx/plugins/PEAKCompetitive/`
4. Launch PEAK

## Usage

### In-Game Controls
- **F3** - Open/close competitive settings menu
- **Scoreboard** - Automatically displays during matches

### Configuration

Press F3 in-game to access:

**Match Settings:**
- Enable/Disable competitive mode
- Show/hide scoreboard
- Items persist between rounds

**Biome Points (1-20 range):**
- Shore, Tropics, Mesa, Alpine, Roots, Caldera, Kiln
- Customize difficulty rewards

**Team Settings:**
- Max Teams: 2-10
- Players Per Team: 1-4

**Survivor Bonuses:**
- Individual Multiplier: 0-2.0 (default 0.5)
- Full Team Bonus: On/Off

**Scoreboard Position:**
- X/Y Position sliders
- Scale: 0.5x - 3.0x

### Game Modes

**2v2 (Default)** - Two teams of 2 players
**1v1** - Solo competitive racing
**3v3** - Three-player teams
**2v2v2** - Three teams of 2
**Custom** - Configure any N-team setup

## Scoring Examples

### Example 1: Alpine (4 pts), 2v2
Team Red wins with both alive:
```
Base points:        4
Survivor bonus:    +4 (2 survivors)
Full team bonus:   +4
Total:             12 pts
```

Team Blue wins with one dead:
```
Base points:        4
Survivor bonus:    +2 (1 survivor)
Full team bonus:    0
Total:              6 pts
```

### Example 2: Kiln (6 pts), 3v3
All survive:
```
6 + (6×0.5×3) + 6 = 21 pts (350%!)
```

One dies:
```
6 + (6×0.5×2) + 0 = 12 pts (200%)
```

**Strategic implication:** Protecting teammates is worth more than rushing!

## Compatibility

- **BepInEx:** 5.4.21+ required
- **PEAK Version:** Any (patches game classes directly)
- **Other Mods:** Should be compatible with most mods
  - May conflict with mods that modify scoring/progression
  - Report issues on GitHub

## Support

**Issues/Bugs:** [GitHub Issues](https://github.com/yourusername/PEAK-Competitive/issues)

**Config File:** `BepInEx/config/PEAKCompetitive.cfg`

**Logs:** `BepInEx/LogOutput.log`

## Changelog

### 1.0.0
- Initial release
- 7 biome difficulty-based scoring
- Survivor bonus system
- Team ghost isolation
- Configurable team sizes
- In-game F3 settings menu
- Real-time scoreboard

## Credits

**Developer:** Your Name
**Framework:** BepInEx 5.4.21
**Patcher:** Harmony 2.x

## License

See LICENSE file for details.

---

**Made for PEAK** - Competitive multiplayer mountaineering
