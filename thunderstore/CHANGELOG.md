# Changelog

All notable changes to PEAK Competitive will be documented in this file.

## [1.0.0] - 2025-11-22

### Added
- **Difficulty-based scoring system** with 7 biome tiers (Shore 1pt → Kiln 6pts)
- **Survivor bonus multiplier** - more living teammates = more points
- **Full team bonus** - extra points if entire team survives
- **Team ghost isolation** - ghosts only see their own team
- **Configurable team sizes** - 1-4 players per team, 2-10 teams
- **In-game F3 settings menu** for host configuration
- **Real-time scoreboard** with team scores and positioning
- **Point breakdown logging** showing calculation details
- **N-player support** - works with solo (1v1) up to large teams

### Scoring Formula
```
Total Points = Base Points + (Base × Multiplier × Survivors) + Full Team Bonus
```

### Biome Points (Default)
- Shore: 1 pt (★☆☆☆☆)
- Tropics: 2 pts (★★☆☆☆)
- Mesa: 3 pts (★★★☆☆)
- Alpine: 4 pts (★★★★☆)
- Roots: 4 pts (★★★★☆)
- Caldera: 5 pts (★★★★★)
- Kiln: 6 pts (★★★★★+)

### Configuration Options
- Individual completion multiplier: 0-2.0 (default 0.5)
- Full team bonus: On/Off (default On)
- All biome points: 1-20 range
- Team settings: Max teams, players per team
- UI settings: Scoreboard position and scale

### Technical
- Built on BepInEx 5.4.21
- Harmony patching for game events
- Zero warnings/errors in build
- Compatible with most other mods

---

## Planned Features (Future Versions)

### v1.1.0
- [ ] Network sync for point values
- [ ] Custom team colors
- [ ] Match history/statistics
- [ ] Configurable win conditions

### v1.2.0
- [ ] Spectator mode
- [ ] Replay system
- [ ] Achievements/medals
- [ ] Tournament bracket support

### v2.0.0
- [ ] Ranked matchmaking
- [ ] ELO rating system
- [ ] Seasonal leaderboards
- [ ] Custom game modes
