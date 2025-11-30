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

# Build Release version (for playing - NO fly mode)
dotnet build --configuration Release

# Build Debug version (for testing - WITH fly mode)
dotnet build --configuration Debug

# Clean and rebuild
dotnet clean && dotnet restore && dotnet build --configuration Release
```

### Build Configurations

| Configuration | Command | Fly Mode | Output Location |
|---------------|---------|----------|-----------------|
| **Release** | `dotnet build --configuration Release` | ❌ No | `artifacts/bin/Release/` |
| **Debug** | `dotnet build --configuration Debug` | ✅ Yes (F4) | `artifacts/bin/Debug/` |

**Use Release for playing with friends** - no cheats available!
**Use Debug for solo testing** - fly mode enabled for testing campfire detection.

### Build Output
- **Release DLL:** `artifacts/bin/Release/PEAKCompetitive.dll`
- **Debug DLL:** `artifacts/bin/Debug/PEAKCompetitive.dll`
- **PDB:** Debug symbols included for debugging

## GitHub Actions CI

The project includes GitHub Actions workflows for automated building.

### Workflows

| Workflow | Trigger | Description |
|----------|---------|-------------|
| `build.yml` | Push to main/multiplayer-mod, PRs | Builds Release and Debug artifacts |
| `release.yml` | Push tag `v*` | Creates GitHub Release with artifacts |

### Setup Options

The CI requires PEAK game DLLs to compile. Choose one option:

#### Option 1: Self-Hosted Runner (Recommended)
1. Set up a [self-hosted runner](https://docs.github.com/en/actions/hosting-your-own-runners) on a machine with PEAK installed
2. Add repository variable: `USE_SELF_HOSTED` = `true`
3. The runner will use the local game installation

#### Option 2: Upload Game DLLs
1. Create a ZIP of required DLLs from `PEAK_Data/Managed/`:
   - `Assembly-CSharp.dll`
   - `Photon*.dll`
   - `Unity*.dll` (Unity modules)
2. Either:
   - Host the ZIP publicly and add variable: `GAME_DLLS_URL` = `<url>`
   - Or base64 encode and add secret: `GAME_DLLS_BASE64`

### Environment Variable Override

For local builds with non-standard PEAK installation:
```bash
# Override game DLL location
set PEAK_MANAGED_DIR=D:\Games\PEAK\PEAK_Data\Managed\
dotnet build --configuration Release
```

### Creating a Release

```bash
# Tag and push to trigger release workflow
git tag v1.0.0
git push origin v1.0.0
```

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

## Decompiled Game Source

**Location:** `C:\Users\della\Desktop\Assembly-CSharp`

The complete decompiled PEAK source code (using dnSpy) is available for reference. Key files to examine:

**Networking Patterns:**
- `RunManager.cs` - Main game state manager, uses RPCs for time sync
- `OrbFogHandler.cs` - Example of `AddCallbackTarget` pattern with `IInRoomCallbacks`
- `Peak/Network/CachedPlayerList.cs` - Example of `OnRoomPropertiesUpdate` usage
- `Character.cs` - Character system with ghost detection (`character.IsGhost`)

**How to Use:**
1. Download dnSpy: https://github.com/dnSpy/dnSpy/releases
2. Open `Steam\steamapps\common\PEAK\PEAK_Data\Managed\Assembly-CSharp.dll`
3. Search for classes/methods to understand game internals
4. Export to project (right-click → Export to Project) for full source browsing

**Key Discoveries from Decompiled Code:**
- `character.IsGhost` property for ghost detection (line 52-56 in Character.cs)
- `character.data.dead` for death state
- `WarpPlayer(Vector3 position, bool poof)` for teleportation (line 166)
- `RPCA_Die()` and `RPCA_Revive()` for death/revival
- Photon patterns: `AddCallbackTarget` in `OnEnable()`, `RemoveCallbackTarget` in `OnDisable()`

## References

- **PEAK-Unlimited:** Reference mod that inspired build configuration
- **BepInEx Docs:** https://docs.bepinex.dev/
- **Harmony Docs:** https://harmony.pardeike.net/
- **Photon PUN 2 Docs:** https://doc.photonengine.com/pun/current/
- **dnSpy:** https://github.com/dnSpy/dnSpy - .NET decompiler for examining game code

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

## PEAK Game Systems - Critical Knowledge

### Character System (MUST KNOW!)

**Key Concepts:**
- `Character.AllCharacters` - Static list of ALL character instances in the game
- Each character has a `PhotonView` component (`character.view`)
- Character's `view.IsMine` indicates if it's the local player
- `character.refs.afflictions` - CharacterAfflictions system for health/status

**Getting Characters:**
```csharp
// Get local player's character
foreach (var character in Character.AllCharacters)
{
    if (character.view.IsMine)
    {
        return character; // This is the local player
    }
}

// Get character by Photon player
foreach (var character in Character.AllCharacters)
{
    if (character.view.Owner.ActorNumber == player.ActorNumber)
    {
        return character;
    }
}
```

**Character Death/Health:**
```csharp
// Kill a character
character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Injury, 100f);

// Revive a character
character.refs.afflictions.ClearAllStatus(false);
```

**Important:** Use reflection to discover PEAK's internal methods - they may have dedicated death/revive methods that aren't documented.

### Campfire System

**Critical Discovery:** PEAK has a `Campfire` class that spawns at biome checkpoints.

**Key Properties:**
- `campfire.advanceToSegment` - Which biome this campfire advances to (use for progression)
- `campfire.transform.position` - Location of the campfire
- Campfires spawn via `Campfire.Awake()` - patch this to detect them!

**Finding Campfires:**
```csharp
// Find all campfires in current scene
var campfires = UnityEngine.Object.FindObjectsByType<Campfire>(FindObjectsSortMode.None);

// Get next biome's campfire
var nextCampfire = campfires.OrderByDescending(c => c.advanceToSegment).FirstOrDefault();
```

**Campfire Interaction:**
```csharp
[HarmonyPatch(typeof(Campfire), "Awake")]
public class CampfireAwakePatch
{
    static void Postfix(Campfire __instance)
    {
        // Add custom interaction component
        __instance.gameObject.AddComponent<YourInteractionComponent>();
    }
}
```

### Network Synchronization - CRITICAL!

**THE MOST IMPORTANT THING:** `PhotonNetwork.AddCallbackTarget(this)`

**Problem:** MonoBehaviourPunCallbacks doesn't automatically register with PhotonNetwork!
**Solution:** Explicitly register in OnEnable():

```csharp
public class NetworkSyncManager : MonoBehaviourPunCallbacks
{
    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this); // CRITICAL!
        Plugin.Logger.LogInfo("Registered with Photon callbacks");
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // Now this will actually be called!
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        // Handle property changes
    }
}
```

**Without `AddCallbackTarget`, callbacks WILL NOT FIRE!** This caused hours of debugging.

**Room Custom Properties Pattern:**
```csharp
// Host sets properties
var props = new ExitGames.Client.Photon.Hashtable
{
    { "MatchActive", true },
    { "TeamAssignments", "0:1,2;1:3,4" } // team:players
};
PhotonNetwork.CurrentRoom.SetCustomProperties(props);

// All clients receive via OnRoomPropertiesUpdate
public override void OnRoomPropertiesUpdate(Hashtable changes)
{
    if (changes.ContainsKey("MatchActive"))
    {
        bool active = (bool)changes["MatchActive"];
        // Update local state
    }
}
```

### Steamworks Integration

**Problem:** `player.NickName` shows SHA hashes instead of Steam names

**Solution:** Use Steamworks API to get real Steam names:
```csharp
using Steamworks;

public static string GetPlayerDisplayName(Photon.Realtime.Player player)
{
    // Parse Steam ID from Photon UserId
    if (ulong.TryParse(player.UserId, out ulong steamId64))
    {
        CSteamID steamId = new CSteamID(steamId64);
        string steamName = SteamFriends.GetFriendPersonaName(steamId);

        if (!string.IsNullOrEmpty(steamName))
        {
            return steamName; // Real Steam name!
        }
    }

    // Fallback
    return $"Player {player.ActorNumber}";
}
```

**Key Insight:** Photon's `player.UserId` contains the Steam ID as a string. Parse it as ulong, wrap in CSteamID, then use SteamFriends API.

### Round System Architecture

**Game Flow:**
1. Teams assigned via `TeamManager.AssignPlayersToTeams()`
2. Match starts → `MatchState.Instance.StartMatch()` → `IsMatchActive = true`
3. Players race to campfire
4. **First team reaches campfire:**
   - Points awarded based on `ScoringCalculator.CalculateRoundPoints()`
   - 10-minute timer starts via `RoundTimerManager.Instance.StartTimer()`
   - Synced to all clients via `NetworkSyncManager.Instance.SyncTimerStart()`
5. **Other teams can finish during timer**
6. **Timer expires or all teams finish:**
   - `RoundTransitionManager.Instance.StartTransition()`
   - Kill all players → Wait 2s → Revive all → Wait 1s → Teleport to next campfire → Start new round
7. **Repeat** through biome progression: Shore → Tropics → Mesa → Alpine → Roots → Caldera → Kiln
8. **At Kiln:** Match ends, winner determined

**Manager Responsibilities:**
- `RoundTimerManager` - Countdown timer, expiration detection
- `RoundTransitionManager` - Kill/revive/teleport coordination
- `NetworkSyncManager` - State synchronization to all clients
- `CharacterHelper` - PEAK game integration (reflection + direct API)
- `TeamManager` - Team assignment and queries

### Reflection Strategy for PEAK Integration

**Why:** PEAK's internal methods aren't documented, and may change between versions.

**Pattern:**
```csharp
static CharacterHelper()
{
    Type characterType = typeof(Character);
    MethodInfo[] methods = characterType.GetMethods(
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    // Find methods by name pattern
    _killMethod = methods.FirstOrDefault(m =>
        m.Name.Contains("Die") ||
        m.Name.Contains("Kill") ||
        m.Name.Contains("Death"));

    Plugin.Logger.LogInfo($"Found Kill Method: {_killMethod?.Name ?? "None"}");
}

// Later, invoke if found
if (_killMethod != null)
{
    _killMethod.Invoke(character, null);
}
```

**Fallback Strategy:**
1. Try reflection to find PEAK's native methods
2. If not found, use direct API (`CharacterAfflictions`)
3. Log what was used for debugging

### Thunderstore Deployment

**CRITICAL:** Users install via Thunderstore Mod Manager (r2modman), not manually!

**Profile-based Installation:**
- Mods go to: `AppData/Roaming/Thunderstore Mod Manager/DataFolder/PEAK/profiles/{profile}/BepInEx/plugins/`
- NOT the game's direct BepInEx folder!
- Each profile is isolated

**When testing:**
1. Build DLL: `dotnet build --configuration Release`
2. Copy to Thunderstore profile: `{profile}/BepInEx/plugins/PEAKCompetitive/`
3. Launch game via Thunderstore Mod Manager
4. Check logs: `{profile}/BepInEx/LogOutput.log`

### Common Pitfalls & Solutions

**1. "Callbacks not firing"**
- ✅ **Solution:** Add `PhotonNetwork.AddCallbackTarget(this)` in OnEnable()

**2. "Seeing SHA hashes instead of player names"**
- ✅ **Solution:** Use `SteamFriends.GetFriendPersonaName(CSteamID)` with parsed UserId

**3. "Type 'Player' is ambiguous"**
- ✅ **Solution:** Always use `Photon.Realtime.Player` explicitly

**4. "Scoreboard only shows for host"**
- ✅ **Solution:** Sync via Room Custom Properties + AddCallbackTarget

**5. "FindObjectsOfType deprecated warning"**
- ✅ **Solution:** Use `FindObjectsByType<T>(FindObjectsSortMode.None)`

**6. "Character not found"**
- ✅ **Solution:** Iterate `Character.AllCharacters`, check `character.view.IsMine` or `character.view.Owner.ActorNumber`

**7. "Players not teleporting"**
- ✅ **Solution:** Only teleport if `character.view.IsMine`, each client teleports their own character

**8. "Changes not appearing in-game"**
- ✅ **Solution:** Make sure DLL is copied to Thunderstore profile folder, not game folder

### Key Files Reference

**Core Systems:**
- `Plugin.cs` - Entry point, initializes managers
- `MatchState.cs` - Global match state singleton
- `TeamData.cs` - Team info and members

**Managers:**
- `TeamManager.cs` - Team operations, player name display
- `NetworkSyncManager.cs` - Photon synchronization
- `RoundTimerManager.cs` - Timer system
- `RoundTransitionManager.cs` - Round transitions
- `CharacterHelper.cs` - PEAK game integration

**Patches:**
- `CampfireInteractionPatch.cs` - Detects when campfires are LIT (not proximity)
- `RespawnChestPatch.cs` - Team-based revival at checkpoint statues
- `ProximityChatPatch.cs` - Global voice for FFA mode only

**UI:**
- `ScoreboardUI.cs` - Live scoreboard display
- `RoundTimerUI.cs` - Timer display
- `CompetitiveMenuUI.cs` - F3 configuration menu

### PEAKLib Integration (Optional)

**Located at:** `../PEAKLib` (sibling directory)

**Useful Modules:**
- `PEAKLib.Core.Networking` - NetworkManager component (attached to Characters)
- `PEAKLib.Core.Hooks.CharacterRegistrationHooks` - Detects character spawn
- `PEAKLib.Stats.CharacterAfflictionsHooks` - Status effect system

**Pattern for hooking character registration:**
```csharp
CharacterRegistrationHooks.OnCharacterAdded += (Character character) =>
{
    // Character just spawned
    character.gameObject.AddComponent<YourComponent>();
};
```

### Debug Mode Features (Debug Build Only)

**Fly Mode** - For testing campfire detection and point scoring
- Toggle: **F4** (keyboard) or **L3+R3** (controller - click both sticks)
- Movement: **WASD / Left Stick**
- Up: **Space / A button (Cross)**
- Down: **Left Shift / B button (Circle)**
- Speed Boost: **Left Ctrl / LT (L2)**

Fly mode is compiled out in Release builds - no temptation when playing for real!

---

## CRITICAL: Revival System (RPCA_ReviveAtPosition)

**THIS IS ESSENTIAL KNOWLEDGE - READ CAREFULLY!**

The game's revival RPC has a critical parameter that's easy to misunderstand:

```csharp
// Decompiled from Character.cs lines 1441-1449
internal void RPCA_ReviveAtPosition(Vector3 position, bool applyStatus)
{
    this.refs.items.DropAllItems(true);
    this.RPCA_Revive(applyStatus);  // <-- The key!
    this.WarpPlayer(position, true);
    // ...
}

// And RPCA_Revive (lines 1416-1437):
internal void RPCA_Revive(bool applyStatus)
{
    // ... clears death state, afflictions, etc ...

    if (applyStatus)  // <-- WATCH OUT!
    {
        this.refs.afflictions.AddStatus(CharacterAfflictions.STATUSTYPE.Curse, 0.05f, false, true);
        this.refs.afflictions.AddStatus(CharacterAfflictions.STATUSTYPE.Hunger, 0.3f, false, true);
    }
}
```

**The `applyStatus` parameter:**
- `true` = Add 30% Hunger + 5% Curse after revival (vanilla punishment for dying)
- `false` = Clean revival with NO penalties

**For competitive mode, ALWAYS use `false`:**
```csharp
character.photonView.RPC("RPCA_ReviveAtPosition", RpcTarget.All, new object[]
{
    position,
    false  // applyStatus=false means NO hunger/curse penalty!
});
```

**Common mistake:** The parameter was incorrectly documented as "poof" (visual effect) in early code. It is NOT a visual toggle - it controls status penalties!

---

## Campfire Detection System

### Why Light_Rpc, Not Proximity Detection

**WRONG approach (causes issues):**
```csharp
// DON'T DO THIS - causes repeated triggers, wrong campfire detection
[HarmonyPatch(typeof(Campfire), "Awake")]
public class BadCampfirePatch
{
    static void Postfix(Campfire __instance)
    {
        // Add proximity detection component - BAD!
        __instance.gameObject.AddComponent<ProximityDetector>();
    }
}
```

**RIGHT approach (hook when campfire is actually lit):**
```csharp
// Campfire.Light_Rpc is called ONLY when vanilla confirms all players are in range
[HarmonyPatch(typeof(Campfire), "Light_Rpc")]
public class CampfireLightPatch
{
    static void Postfix(Campfire __instance)
    {
        // Only fires when campfire is ACTUALLY lit
        // Vanilla already validated all players are nearby
        ProcessCampfireCompletion(__instance);
    }
}
```

**Why this matters:**
1. Vanilla game shows "Player X is 50m away" until everyone is in range
2. Vanilla handles the "waiting for players" logic
3. `Light_Rpc` only fires when the campfire animation plays and everyone advances
4. No need to track distances ourselves - vanilla does it correctly

### Target Segment Tracking

Each campfire has `advanceToSegment` indicating which biome it leads to:

```csharp
// Segment enum (from PEAK):
// Beach, Tropics, Mesa, Alpine, Roots, Caldera, TheKiln, Peak

// When on Shore, target the Tropics campfire
// When on Tropics, target the Alpine campfire (skips Mesa internally)
// etc.

private static Segment GetTargetSegmentForMap(string mapName)
{
    mapName = mapName?.ToLower() ?? "";

    if (mapName.Contains("shore")) return Segment.Tropics;
    if (mapName.Contains("tropic")) return Segment.Alpine;
    if (mapName.Contains("alpine")) return Segment.Caldera;
    if (mapName.Contains("caldera")) return Segment.TheKiln;
    if (mapName.Contains("kiln")) return Segment.Peak;

    return Segment.Tropics; // Default
}
```

---

## Voice Chat / Proximity System

### How Proximity Chat Works in PEAK

PEAK uses Photon Voice with spatial audio. Key file: `CharacterVoiceHandler.cs`

```csharp
// CharacterVoiceHandler.LateUpdate() - line 130
this.m_source.spatialBlend = (float)(flag ? 0 : 1);
```

- `spatialBlend = 1.0` = Full 3D spatial audio (proximity-based, can only hear nearby)
- `spatialBlend = 0.0` = No spatial blend (global, hear everyone equally)

### Disabling Proximity for FFA Mode

```csharp
[HarmonyPatch(typeof(CharacterVoiceHandler), "LateUpdate")]
public class ProximityChatPatch
{
    static void Postfix(CharacterVoiceHandler __instance)
    {
        // ONLY apply in Free-for-All mode!
        // Team mode MUST keep proximity chat
        if (!ConfigurationHandler.EnableCompetitiveMode) return;
        if (!ConfigurationHandler.FreeForAllMode) return;
        if (!ConfigurationHandler.DisableProximityChat) return;

        var audioSource = __instance.audioSource;
        if (audioSource != null)
        {
            audioSource.spatialBlend = 0f;  // Global audio
        }
    }
}
```

**CRITICAL:** Only disable proximity in FFA mode! Team mode requires proximity chat so you can only hear nearby teammates/enemies.

---

## Harmony Patch Re-Entry Detection

When patching methods that might call themselves (or methods you also patch), use a re-entry flag:

```csharp
[HarmonyPatch(typeof(RespawnChest), "SpawnItems")]
public class RespawnChestPatch
{
    // Flag to detect when we're re-entering
    private static bool _allowOriginal = false;

    static bool Prefix(RespawnChest __instance, ref List<PhotonView> __result)
    {
        // Check for re-entry
        if (_allowOriginal)
        {
            // We set this flag - let original run
            return true;
        }

        // ... do custom logic ...

        // If we need original behavior for some code path:
        if (needOriginalBehavior)
        {
            return true;  // Let original run
        }

        // If calling methods that might trigger this patch again:
        _allowOriginal = true;
        try
        {
            // Call something that might re-enter
            someMethod.Invoke(...);
        }
        finally
        {
            _allowOriginal = false;  // Always reset!
        }

        __result = new List<PhotonView>();
        return false;  // Skip original
    }
}
```

---

## Round Transition Sequence (Detailed)

The full sequence when transitioning between rounds:

```
1. CampfireLightPatch detects target campfire is lit
   ↓
2. ProcessCampfireCompletion() awards points to nearby teams
   ↓
3. If first team: RoundTimerManager.StartTimer() (10 min countdown)
   ↓
4. NetworkSyncManager.SyncTeamScores() updates all clients
   ↓
5. If AllTeamsFinished() or timer expires:
   ↓
6. RoundTransitionManager.StartTransition() begins coroutine:
   │
   ├─ MatchState.IsRoundActive = false (prevents re-detection)
   │
   ├─ NetworkSyncManager.SyncKillAllPlayers() → RPC_KillAllPlayers
   │   └─ Each client kills their own character
   │
   ├─ Wait 2 seconds
   │
   ├─ CharacterHelper.GetNextCampfirePosition() finds target campfire
   │
   ├─ NetworkSyncManager.SyncReviveAllPlayers(position)
   │   └─ Host calls RPCA_ReviveAtPosition for each character
   │   └─ Uses applyStatus=false (no hunger penalty!)
   │
   ├─ Wait 1 second
   │
   └─ StartNextRound():
       ├─ Reset timer, team states, chest tracking
       ├─ CampfireInteraction.SetRoundTarget(nextSegment)
       ├─ MatchState.StartRound(nextMapName)
       └─ NetworkSyncManager.SyncRoundStart(nextMapName)
```

---

## Recent Changes

### 2025-11-25 - Fly Mode & Campfire Detection Fixes
- ✅ Added debug-only fly mode (F4 / L3+R3) with full controller support
- ✅ Fly mode only included in Debug builds (compile-time flag)
- ✅ Fixed campfire detection - now only triggers for NEXT campfire, not spawn campfire
- ✅ Added segment-based validation (Beach → Tropics → Alpine → Caldera → TheKiln → Peak)
- ✅ Added 5-second grace period after round start before detection begins
- ✅ Reduced detection radius from 15m to 10m for precision

### 2025-11-25 - Placement-Based Scoring System
- ✅ Implemented placement-based scoring (1st=100%, 2nd=70%, 3rd=50%, 4th+=30%)
- ✅ Added ghost detection using `character.IsGhost` from decompiled source
- ✅ Track individual player arrivals at campfires (not just team-based)
- ✅ Award points per non-ghost member: `Placement Multiplier × Base Points × Non-Ghost Members`
- ✅ Teams with all ghost members get 0 points
- ✅ Fixed network synchronization - all players see scoreboard updates
- ✅ Added comprehensive debug logging for score sync verification

### 2025-11-29 - Campfire Detection Rewrite & Revival Fixes
- ✅ Rewrote campfire detection to use `Light_Rpc` instead of proximity detection
- ✅ Vanilla game now handles "waiting for all players" - we just hook when campfire is lit
- ✅ Fixed revival hunger penalty - `applyStatus=false` prevents 30% hunger + 5% curse
- ✅ Added global voice chat option for Free-for-All mode only
- ✅ Fixed teleportation to use correct campfire position based on current map
- ✅ Team mode proximity chat is completely unaffected by global voice setting

**Last Updated:** 2025-11-29
**Build Status:** ✅ Passing (0 Warnings, 0 Errors)
**Output:** PEAKCompetitive.dll (Release) / PEAKCompetitive.dll (Debug with fly mode)
**Decompiled Source:** C:\Users\della\Desktop\Assembly-CSharp
