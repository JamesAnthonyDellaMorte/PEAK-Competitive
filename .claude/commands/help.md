# PEAK Competitive Mod - Development Commands

## Available Commands

- `/build` - Build the mod in Release configuration
- `/test` - Build and copy DLL to PEAK installation for testing
- `/package` - Create Thunderstore package
- `/docs` - View the PEAK modding guide

## Project Structure

```
PEAK-Competitive/
├── src/PEAKCompetitive/
│   ├── Configuration/       # Config handling and UI
│   ├── Patches/            # Harmony patches
│   ├── Util/               # Helper utilities
│   ├── Model/              # Data models
│   └── Plugin.cs           # Main entry point
├── .claude/                # Claude Code configuration
├── docs/                   # Documentation
├── manifest.json           # Mod metadata
├── README.md              # User documentation
└── CHANGELOG.md           # Version history
```

## Competitive Features

This mod adds competitive gameplay features to PEAK:
- Timer system for speedruns
- Leaderboards
- Race modes
- Scoring system
- Match statistics
