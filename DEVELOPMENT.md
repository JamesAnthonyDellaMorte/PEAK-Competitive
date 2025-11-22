# PEAK Competitive - Development Guide

## Current Status

The mod structure is complete with the following features implemented:

### ✅ Completed
- Configuration system with BepInEx config file
- Team assignment system (configurable teams and players per team)
- Scoreboard UI showing live scores
- In-game configuration menu (F3)
- Match state tracking
- Round-based gameplay logic
- Map point value system
- Visual notifications for round/match events
- Player connection handling

### ⚠️ Needs Implementation (Game-Specific Integration)

The following patches are **placeholders** and need to be connected to PEAK's actual game code:

1. **SummitDetectionPatch** - Detect when a player reaches the summit
2. **MatchStartPatch** - Hook into level/round start
3. **PlayerDeathPatch** - Detect when players die/fall
4. **EndGamePatch** - Detect when players reach the Kiln

---

## Next Steps: Connecting to PEAK's Game Code

### Step 1: Decompile PEAK

Use **dnSpy** or **ILSpy** to decompile PEAK's game code:

```bash
# Open this file in dnSpy:
C:\Program Files (x86)\Steam\steamapps\common\PEAK\PEAK_Data\Managed\Assembly-CSharp.dll
```

### Step 2: Find Summit Detection

Search for classes/methods containing:
- "Summit", "Peak", "Top", "Finish"
- `OnTriggerEnter`, `OnTriggerStay`
- Collider/trigger zone names

**Example of what to look for:**
```csharp
// Hypothetical PEAK code
public class SummitZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            // Player reached summit!
        }
    }
}
```

**Create the patch in `SummitDetectionPatch.cs`:**
```csharp
[HarmonyPatch(typeof(SummitZone), "OnTriggerEnter")]
public class SummitDetectionPatch
{
    static void Postfix(Collider other)
    {
        // Your implementation here
    }
}
```

### Step 3: Find Level/Map Loading

From PEAK Unlimited, we know:
- `AirportCheckInKiosk.LoadIslandMaster()` exists
- This is where levels are loaded

**Patch example in `MatchStartPatch.cs`:**
```csharp
[HarmonyPatch(typeof(AirportCheckInKiosk), "LoadIslandMaster")]
public class LoadIslandPatch
{
    static void Postfix()
    {
        // Start new round when level loads
    }
}
```

### Step 4: Find Player Death/Ragdoll System

Search for:
- "Die", "Death", "Ragdoll", "Fall"
- Player health/state classes
- Respawn logic

### Step 5: Find Kiln/End Game Detection

Search for:
- "Kiln", "End", "Complete", "Finish"
- Scene names
- End game triggers

---

## Configuration System

Config file location: `BepInEx/config/PEAKCompetitive.cfg`

### Available Settings

```ini
[General]
EnableCompetitiveMode = true
MenuKey = F3

[Teams]
MaxTeams = 2                  # Number of teams
PlayersPerTeam = 2            # Players per team

[Match]
ItemsPersist = true           # Items carry between rounds
ShowScoreboard = true         # Display scoreboard

[MapPoints]
Map1Points = 1                # Points for Map 1
Map2Points = 1                # Points for Map 2
Map3Points = 2                # Points for Map 3
Map4Points = 2                # Points for Map 4
RuthsMapPoints = 3            # Points for Ruth's Map

[UI]
ScoreboardX = 10              # Scoreboard position X
ScoreboardY = 10              # Scoreboard position Y
ScoreboardScale = 1.0         # Scoreboard size

[Debug]
EnableDebugLogging = false
```

---

## Architecture Overview

### Core Classes

#### **Plugin.cs**
- Main BepInEx plugin entry point
- Initializes configuration
- Adds UI components
- Applies Harmony patches

#### **ConfigurationHandler.cs**
- Loads and manages all config values
- Provides static access to settings
- Validates configuration ranges

#### **MatchState.cs** (Singleton)
- Tracks current match/round state
- Manages teams and scores
- Handles round/match start/end logic
- Triggers UI notifications

#### **TeamData.cs**
- Represents a single team
- Tracks members, score, and round state
- Manages team-specific logic

#### **TeamManager.cs**
- Static utility for team operations
- Assigns players to teams
- Balances teams
- Team color management

### UI Components

#### **ScoreboardUI.cs**
- Displays live scoreboard during match
- Shows team scores, members, and round info
- Positioned and scaled via config

#### **CompetitiveMenuUI.cs**
- In-game settings menu (F3)
- Allows host to configure match settings
- Start/end match controls
- Team reassignment

#### **MatchNotificationUI.cs**
- Large center-screen notifications
- Round win announcements
- Match start/end messages
- Color-coded team notifications

### Harmony Patches

#### **PlayerConnectionPatch.cs** (WORKING)
- Handles player join/leave
- Auto-assigns players to teams
- Monitors player count for match viability

#### **SummitDetectionPatch.cs** (PLACEHOLDER)
- **TODO**: Hook into summit trigger
- Detect when team reaches summit
- Award points and end round

#### **MatchStartPatch.cs** (PLACEHOLDER)
- **TODO**: Hook into level loading
- Start new rounds
- Track map names

#### **PlayerDeathPatch.cs** (PLACEHOLDER)
- **TODO**: Hook into player death
- Track team alive count
- End round if all teams dead

#### **EndGamePatch.cs** (PLACEHOLDER)
- **TODO**: Hook into Kiln/end game
- Determine match winner
- Display final results

---

## Building the Mod

### Prerequisites

1. **Update `.csproj` with your PEAK path:**
   ```xml
   <ManagedDir>C:\Program Files (x86)\Steam\steamapps\common\PEAK\PEAK_Data\Managed</ManagedDir>
   ```

2. **Build:**
   ```bash
   cd PEAK-Competitive
   dotnet build -c Release
   ```

3. **Output:**
   ```
   artifacts/bin/Release/PEAKCompetitive.dll
   ```

### Testing

1. Copy DLL to PEAK:
   ```
   PEAK/BepInEx/plugins/PEAKCompetitive.dll
   ```

2. Launch PEAK

3. Check logs:
   ```
   PEAK/BepInEx/LogOutput.log
   ```

### Packaging for Thunderstore

```bash
tcli build
```

Output: `artifacts/thunderstore/PEAKCompetitive.zip`

---

## Debugging Tips

### Enable Debug Logging

Set in `BepInEx/config/PEAKCompetitive.cfg`:
```ini
[Debug]
EnableDebugLogging = true
```

### Check Logs

Look for:
```
[Info   : PEAK Competitive] Plugin loaded!
[Info   : PEAK Competitive] Configuration loaded successfully!
[Info   : PEAK Competitive] Competitive Mode: True
```

### Common Issues

**Mod doesn't load:**
- Check BepInEx is installed
- Verify DLL is in `plugins/` folder
- Check `LogOutput.log` for errors

**Patches not applying:**
- Verify class/method names are correct
- Check for typos in `[HarmonyPatch]` attributes
- Enable debug logging

**UI not showing:**
- Verify `EnableCompetitiveMode = true`
- Check `ShowScoreboard = true`
- Try different menu key if F3 conflicts

---

## Code Examples

### Detecting Summit (Example Implementation)

```csharp
[HarmonyPatch(typeof(SummitZone), "OnTriggerEnter")]
public class SummitDetectionPatch
{
    static void Postfix(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!ConfigurationHandler.EnableCompetitiveMode) return;
        if (!MatchState.Instance.IsRoundActive) return;

        // Get player from collider
        var playerController = other.GetComponent<PlayerController>();
        if (playerController == null) return;

        // Get photon player
        var player = playerController.photonView.Owner;

        // Get player's team
        var team = TeamManager.GetPlayerTeam(player);
        if (team == null) return;

        // First team to summit wins
        if (!team.HasReachedSummit)
        {
            Plugin.Logger.LogInfo($"{player.NickName} reached summit!");
            MatchState.Instance.TeamReachedSummit(team);
        }
    }
}
```

### Starting Round on Level Load

```csharp
[HarmonyPatch(typeof(SceneLoader), "LoadScene")]
public class SceneLoadPatch
{
    static void Postfix(string sceneName)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!ConfigurationHandler.EnableCompetitiveMode) return;

        var matchState = MatchState.Instance;

        if (!matchState.IsMatchActive)
        {
            TeamManager.AssignPlayersToTeams();
            matchState.StartMatch();
        }

        matchState.StartRound(sceneName);
    }
}
```

---

## Testing Checklist

- [ ] Mod loads without errors
- [ ] F3 menu opens and closes
- [ ] Teams are assigned when match starts
- [ ] Scoreboard displays during match
- [ ] Round win detection works
- [ ] Points are awarded correctly
- [ ] Match end detection works
- [ ] Visual notifications appear
- [ ] Items persist between rounds
- [ ] Player join/leave handled correctly
- [ ] Config file changes apply
- [ ] Host-only settings enforced

---

## Future Enhancements

Potential features to add:

1. **Spectator Mode** - Allow spectating after death
2. **Team Chat** - In-game team communication
3. **Replay System** - Save and replay matches
4. **Global Leaderboards** - Track best teams/times
5. **Custom Game Modes** - Time trials, elimination, etc.
6. **Team Balancing** - Auto-balance based on skill
7. **Match Statistics** - Detailed post-match stats
8. **Tournament Mode** - Bracket-style competitions
9. **Team Skins** - Visual team identification
10. **Voice Lines** - Audio callouts for events

---

## Contributing

When you find PEAK's actual game code:

1. Document the class/method names
2. Create proper patches in the Patches/ folder
3. Test thoroughly in multiplayer
4. Update this documentation
5. Commit changes

---

## Resources

- [PEAK Modding Guide](docs/PEAK_MODDING_GUIDE.md) - Full modding reference
- [BepInEx Docs](https://docs.bepinex.dev/)
- [Harmony Docs](https://harmony.pardeike.net/)
- [Photon PUN 2 Docs](https://doc.photonengine.com/pun/current/getting-started/pun-intro)

---

## License

MIT License - See LICENSE file for details
