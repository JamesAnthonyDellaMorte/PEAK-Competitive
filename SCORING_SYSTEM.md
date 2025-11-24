# PEAK Competitive Scoring System

## Overview

**MORE SURVIVORS = MORE POINTS!**

The scoring system rewards teams based on:
1. **Biome Difficulty** - Harder biomes are worth more base points
2. **Survivor Count** - Each living team member multiplies your points
3. **Full Team Bonus** - Extra points if everyone survives

---

## Biome Point Values (Difficulty-Based)

| Rank | Biome   | Difficulty | Hazards | Base Points |
|------|---------|------------|---------|-------------|
| 1 | Shore   | ★☆☆☆☆ Easy | Sea urchins, jellyfish | **1 pt** |
| 2 | Tropics | ★★☆☆☆ Moderate | Bees, ticks, poison plants | **2 pts** |
| 3 | Mesa    | ★★★☆☆ Moderate+ | Sun, antlions, scorpions | **3 pts** |
| 4 | Alpine  | ★★★★☆ Hard | Ice, wind, cold, ledges | **4 pts** |
| 4 | Roots   | ★★★★☆ Hard | Spiders, zombies, poison | **4 pts** |
| 5 | Caldera | ★★★★★ Very Hard | Lava, wind, eruptions | **5 pts** |
| 6 | Kiln    | ★★★★★+ Extreme | Lava, vertical, no recovery | **6 pts** |

---

## Scoring Formula

```
Total Points = Base Points + Survivor Bonus + Full Team Bonus

Where:
  Survivor Bonus = Base Points × Multiplier × Living Members
  Full Team Bonus = Base Points (if ALL members alive)

Default Multiplier: 0.5 (configurable)
```

---

## Scoring Examples

### Alpine (4 pts base), 2-player team, 0.5x multiplier

| Survivors | Calculation | Total Points | % of Base |
|-----------|-------------|--------------|-----------|
| **2/2 alive** | 4 + (4×0.5×2) + 4 | **12 pts** | 300% |
| **1/2 alive** | 4 + (4×0.5×1) + 0 | **6 pts** | 150% |
| **0/2 alive** | 4 + 0 + 0 | **4 pts** | 100% |

**Translation:** Keeping your team alive **triples** your points!

### Kiln (6 pts base), 3-player team, 0.5x multiplier

| Survivors | Calculation | Total Points | % of Base |
|-----------|-------------|--------------|-----------|
| **3/3 alive** | 6 + (6×0.5×3) + 6 | **21 pts** | 350% |
| **2/3 alive** | 6 + (6×0.5×2) + 0 | **12 pts** | 200% |
| **1/3 alive** | 6 + (6×0.5×1) + 0 | **9 pts** | 150% |
| **0/3 alive** | 6 + 0 + 0 | **6 pts** | 100% |

**Translation:** Each survivor matters! Losing 1 member costs you 9 points.

### Shore (1 pt base), Solo player (1v1), 0.5x multiplier

| Survivors | Calculation | Total Points | % of Base |
|-----------|-------------|--------------|-----------|
| **1/1 alive** | 1 + (1×0.5×1) + 1 | **2.5 pts** | 250% |
| **0/1 alive** | 1 + 0 + 0 | **1 pt** | 100% |

**Translation:** Solo players still get survival bonuses!

---

## Configuration Options

All values are configurable by the host via in-game menu (F3):

### Biome Point Values
- **ShorePoints**: Default 1 (Range: 1-20)
- **TropicsPoints**: Default 2 (Range: 1-20)
- **MesaPoints**: Default 3 (Range: 1-20)
- **AlpinePoints**: Default 4 (Range: 1-20)
- **RootsPoints**: Default 4 (Range: 1-20)
- **CalderaPoints**: Default 5 (Range: 1-20)
- **KilnPoints**: Default 6 (Range: 1-20)

### Bonus Multipliers
- **IndividualCompletionMultiplier**: Default 0.5 (Range: 0-2.0)
  - 0 = No survivor bonus
  - 0.5 = Half base points per survivor (default)
  - 1.0 = Full base points per survivor
  - 2.0 = Double base points per survivor

- **EnableFullTeamBonus**: Default true
  - If enabled, teams get extra base points when ALL members survive

---

## Strategic Implications

### Risk vs Reward
- **Hard Biomes** = More Points = Higher Stakes
- **Keeping team alive** = 2x-3x point multiplier
- **Aggressive play** might win round but lose points (dead teammates)

### Team Tactics
1. **Protect the weak** - One death loses massive points
2. **Revive ghosts** - Help teammates stay alive for bonuses
3. **Retreat if needed** - Survival > Speed in high-point biomes

### Point Management
- **Shore (1pt)**: Low risk practice - survival matters less
- **Alpine/Roots (4pts)**: Medium stakes - each death = -4-6 pts
- **Kiln (6pts)**: High stakes - one death = -9 pts, total wipe = -15 pts!

---

## Solo Play Support

The system fully supports **1 player per team** (1v1, 1v1v1, etc.):

```
Solo Player on Alpine (4 pts, 0.5x multiplier):
- Alive:  4 + (4×0.5×1) + 4 = 10 pts
- Dead:   4 + 0 + 0         = 4 pts

Difference: 6 points (2.5x multiplier for staying alive)
```

---

## N-Player Flexibility

Teams can be **1-4 players** each with **2-10 teams**:

| Mode | Teams | Players/Team | Total Players |
|------|-------|--------------|---------------|
| 1v1 | 2 | 1 | 2 |
| 2v2 | 2 | 2 | 4 |
| 3v3 | 2 | 3 | 6 |
| 2v2v2 | 3 | 2 | 6 |
| 1v1v1v1 | 4 | 1 | 4 |
| 4v4 | 2 | 4 | 8 |

All configurations use the same scoring formula - **more survivors always = more points!**

---

## Display Format

In-game scoring shows detailed breakdown:

```
Alpine (4 pts base)
Team Red Wins!

  Base points: 4
  Survivor bonus: 4 × 0.5 × 2 survivors = +4 pts
  Full team bonus: +4 pts (all 2 members alive!)
  TOTAL POINTS: 12 (300% of base)

Scoreboard:
  Red Team: 12 pts
  Blue Team: 8 pts
```

---

## Implementation Details

### Files Modified
1. **ConfigurationHandler.cs** - Added 7 biome point configs
2. **ScoringCalculator.cs** - Enhanced survivor bonus logging
3. **CompetitiveMenuUI.cs** - Updated UI to show all 7 biomes with star ratings

### Backward Compatibility
Old config files with `Map1Points` etc. will be **ignored**. New configs use:
- `BiomePoints.ShorePoints`
- `BiomePoints.TropicsPoints`
- etc.

First-time players will see default values (1, 2, 3, 4, 4, 5, 6).

---

## Testing Checklist

- [ ] Shore (1pt) awards correct base points
- [ ] Tropics (2pts) awards correct points
- [ ] Mesa (3pts) awards correct points
- [ ] Alpine (4pts) awards correct points
- [ ] Roots (4pts) awards correct points
- [ ] Caldera (5pts) awards correct points
- [ ] Kiln (6pts) awards correct points
- [ ] Survivor bonus scales with living members
- [ ] Full team bonus only awarded if ALL survive
- [ ] Solo players (1v1) get survival bonuses
- [ ] Larger teams (3v3, 4v4) calculate correctly
- [ ] Ghost teams (all dead) get base points only

---

**Last Updated:** 2025-11-22
**Version:** 2.0 (Difficulty-Based Scoring)
