# PEAK-Competitive Build Documentation

## Project Overview

PEAK-Competitive is a BepInEx mod for PEAK that adds competitive gameplay features including:
- **Duo Team Racing**: 2v2 (or configurable) teams race to checkpoints
- **Round-Based Gameplay**: First team to checkpoint wins the round
- **Point Scoring System**: Different biomes worth different points
- **Team-Isolated Ghosts**: Losing teams become ghosts but only see/help their own team
- **Match Victory**: First to Kiln with most points wins

## Build System Setup

### Project Structure
```
PEAK-Competitive/
├── src/PEAKCompetitive/
│   ├── Configuration/      # UI and config handlers
│   ├── Model/              # Game state models (MatchState, TeamData)
│   ├── Patches/            # Harmony patches
│   ├── Util/               # Helper utilities
│   └── Plugin.cs           # Main plugin entry point
├── artifacts/bin/Release/  # Build output
└── PEAKCompetitive.csproj
```

### Dependencies

**NuGet Packages:**
- `BepInEx.Core` v5.4.21
- `BepInEx.PluginInfoProps` v2.1.0 (generates MyPluginInfo class)
- `BepInEx.AssemblyPublicizer.MSBuild` v0.4.3 (accesses internal game types)

**Game References:**
- Uses wildcard pattern to auto-include all PEAK game DLLs
- Publicizes `Assembly-CSharp.dll` for internal access
- Excludes system DLLs (Mono*, System*, netstandard, mscorlib)

## Build Fixes Applied

### 1. BepInEx NuGet Source
Added custom NuGet source for BepInEx packages:
```xml
<RestoreAdditionalProjectSources>
  https://nuget.bepinex.dev/v3/index.json
</RestoreAdditionalProjectSources>
```

### 2. Wildcard DLL References
Adopted PEAK-Unlimited's proven pattern for automatic DLL discovery:
```xml
<LocalReferences
  Include="$(ManagedDir)*.dll"
  Exclude="$(ManagedDir)Mono*.dll;$(ManagedDir)netstandard.dll;$(ManagedDir)System*.dll;$(ManagedDir)mscorlib.dll"/>
<Reference Include="@(LocalReferences)" Private="false" />
<Reference Include="$(ManagedDir)Assembly-CSharp.dll" Private="false" Publicize="true" />
```

**Benefits:**
- No need to manually list each DLL
- Auto-includes Photon, Unity modules, and all game dependencies
- Easier maintenance as game updates

### 3. Type Conflict Resolution

**Problem:** The PEAK game has its own `Player` class that conflicts with `Photon.Realtime.Player`

**Solution:** Use fully qualified type names throughout:
```csharp
// Before (ambiguous)
public List<Player> Members { get; set; }

// After (explicit)
public List<Photon.Realtime.Player> Members { get; set; }
```

**Files Updated:**
- `Model/TeamData.cs` - Team member storage
- `Model/MatchState.cs` - Player team assignment
- `Util/TeamManager.cs` - Team management logic
- `Patches/PlayerConnectionPatch.cs` - Player join/leave handlers

### 4. Photon API Corrections

**Changed:** `.NickName` → `.UserId`

Photon PUN 2 uses `UserId` property, not `NickName`. Updated all player identification code.

### 5. Configuration Fixes

**Property Access:**
```csharp
// Made properties publicly settable for UI
public static bool EnableCompetitiveMode { get; set; }
public static bool ItemsPersist { get; set; }
public static bool ShowScoreboard { get; set; }
```

**Method Signatures:**
```csharp
// Changed from ref parameter (can't use with properties)
private void DrawPointSlider(string label, ref int value)

// To return value pattern
private int DrawPointSlider(string label, int value)
```

### 6. Missing Unity References

Added `UnityEngine.TextRenderingModule` for UI types:
- `FontStyle`
- `TextAnchor`

## Building the Project

### Prerequisites
- .NET SDK 8.0+
- PEAK installed via Steam
- Update `ManagedDir` in `.csproj` if PEAK is not in default location

### Build Commands

```bash
# Restore NuGet packages
dotnet restore

# Build Release version
dotnet build --configuration Release

# Clean and rebuild
dotnet clean && dotnet restore && dotnet build --configuration Release
```

### Build Output
- **DLL:** `artifacts/bin/Release/PEAKCompetitive.dll` (~35 KB)
- **PDB:** `artifacts/bin/Release/PEAKCompetitive.pdb` (debug symbols)

## Installation

1. Copy `PEAKCompetitive.dll` to `PEAK/BepInEx/plugins/PEAKCompetitive/`
2. Launch PEAK
3. Press **F3** in-game to open competitive menu

## Comparison with PEAK-Unlimited

### Similarities (Good!)
✅ Same project structure (Configuration/, Model/, Patches/, Util/)
✅ Same .csproj wildcard DLL reference pattern
✅ Same BepInEx dependencies and versions
✅ Same target framework (netstandard2.1)
✅ Both use Harmony for patching
✅ Both initialize in `Awake()`

### Differences (Both Valid!)

| Aspect | PEAK-Unlimited | PEAK-Competitive | Notes |
|--------|----------------|------------------|-------|
| **Plugin Metadata** | `[BepInAutoPlugin]` | `[BepInPlugin(...)]` | Competitive uses standard approach |
| **Patch Application** | Individual `_harmony.PatchAll(typeof(X))` | Single `_harmony.PatchAll()` | Competitive is cleaner |
| **Logging** | Custom UnlimitedLogger | Standard BepInEx Logger | Both work fine |
| **Class Modifiers** | `partial class` | Regular class | AutoPlugin requires partial |

**Verdict:** PEAK-Competitive follows more standard BepInEx patterns while achieving the same result.

## Build Quality

**Current Status:** ✅ **0 Warnings, 0 Errors**

Successfully compiles with no issues. All type conflicts resolved, all dependencies properly referenced.

## Development Notes

### Player Type Handling
Always use `Photon.Realtime.Player` explicitly - never just `Player` alone:
```csharp
// Good
public void AssignPlayerToTeam(Photon.Realtime.Player player, int teamId)

// Bad - ambiguous!
public void AssignPlayerToTeam(Player player, int teamId)
```

### Photon PUN 2 API
- Use `player.UserId` for unique player identifier
- Use `player.ActorNumber` for in-room player number
- Access `PhotonNetwork.CurrentRoom.PlayerCount` for player count

### Adding New Patches
1. Create patch class in `Patches/` directory
2. Use `[HarmonyPatch]` attributes
3. No need to register - `_harmony.PatchAll()` auto-discovers

Example:
```csharp
[HarmonyPatch(typeof(GameClass), "MethodName")]
public class MyPatch
{
    [HarmonyPrefix]
    static bool Prefix() { ... }
}
```

## Troubleshooting

### "Player does not contain definition for X"
- Make sure using `Photon.Realtime.Player` not game's `Player` class
- Check if using correct Photon API (`.UserId` not `.NickName`)

### "Unable to find package BepInEx.Core"
- Verify BepInEx NuGet source is added to `.csproj`
- Run `dotnet restore` again

### "Assembly-CSharp.dll not found"
- Check `ManagedDir` path in `.csproj`
- Ensure trailing backslash: `C:\...\Managed\`

## References

- **PEAK-Unlimited:** Reference mod that inspired build configuration
- **BepInEx Docs:** https://docs.bepinex.dev/
- **Harmony Docs:** https://harmony.pardeike.net/
- **Photon PUN 2 Docs:** https://doc.photonengine.com/pun/current/

## Commit History

### 8db296d - Fix build system and type conflicts
- Added BepInEx NuGet source
- Adopted wildcard DLL reference pattern from PEAK-Unlimited
- Fixed Photon.Realtime.Player vs game Player type conflicts
- Updated API calls (.NickName → .UserId)
- Fixed configuration property access
- Added missing Unity module references
- **Result:** Clean build with 0 warnings, 0 errors

---

**Last Updated:** 2025-11-22
**Build Status:** ✅ Passing
**Output:** PEAKCompetitive.dll (35 KB)
