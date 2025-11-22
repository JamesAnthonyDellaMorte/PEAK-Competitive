# PEAK Game Mechanics Reference

## For Competitive Mod Integration

### Biome Sequence
1. **Shore** (Beach/Coast) - Tutorial area
2. **Tropics** OR **Roots** (Jungle/Redwood Forest) - Alternates daily
3. **Alpine** OR **Mesa** (Snow/Desert) - Alternates daily
4. **Caldera** (Volcanic Slopes)
5. **Kiln** (Inner Volcano)
6. **Summit (PEAK)** - Final destination

### Key Game Systems

#### Checkpoints/Camps
- Each biome ends with a **campfire checkpoint**
- **Ancient Statue** at camps revives dead players
- This is where we should detect "summit reached" = checkpoint reached

#### Player States
- **Alive** - Normal climbing
- **Down** - Unconscious with death timer (can be revived by teammates)
- **Dead** - Skeleton on ground (needs Ancient Statue or Scout Effigy)
- **Ghost** - Spectator mode after death (can still help team)

#### Death/Revival System
- Down state has timer
- Teammates can revive with food/medical supplies
- Can carry unconscious players
- Ancient Statues revive all dead at checkpoint
- Scout Effigy item revives one player anywhere

#### Items System
- 3 items in hand
- 4 items in backpack (must place backpack to access)
- Found in **luggage crates**
- Our mod: Items persist between rounds for all players

### Important Classes to Look For

#### Likely Class Names (from PEAK Unlimited analysis)
- `Campfire` - Checkpoint campfires
- `PlayerController` - Player movement/state
- `PlayerConnectionLog` - Player join/leave (confirmed working)
- `AirportCheckInKiosk` - Level selection/loading
- `EndScreen` / `EndSequence` - End of level/game

#### What We Need to Find

**1. Summit/Checkpoint Detection:**
```csharp
// Look for:
// - Campfire.OnPlayerReached()
// - Checkpoint trigger zones
// - "Ancient Statue" activation
// - Scene transition at biome end
```

**2. Map/Biome Names:**
```csharp
// Map name strings to look for:
// "Shore", "Tropics", "Roots", "Alpine", "Mesa", "Caldera", "Kiln", "Summit"
// Or numbered: "Map1", "Map2", etc.
```

**3. Player Death/Down State:**
```csharp
// Look for:
// - Player death/down state flags
// - Death timer
// - Revive methods
// - Ghost mode activation
```

**4. Kiln/Summit Detection:**
```csharp
// Look for:
// - Final biome check
// - "Escaped island" flag
// - Victory/completion trigger
```

### Competitive Mode Integration Points

#### Round Win Condition
**First team to reach biome checkpoint wins round**

Options:
1. Hook into campfire checkpoint trigger
2. Detect Ancient Statue activation
3. Monitor biome transition

#### Round Start
**When level loads**

Options:
1. Hook `AirportCheckInKiosk.LoadIslandMaster()` (known from PEAK Unlimited)
2. Scene loading events
3. Campfire spawn/initialization

#### All Teams Dead Detection
**When all players are in ghost mode**

Options:
1. Count living (non-ghost) players
2. Check team alive counts
3. Hook ghost mode activation

#### Match End
**All players reach Kiln summit OR all teams eliminated**

Options:
1. Detect final "PEAK" biome/scene
2. Check for "escaped island" completion
3. Hook evacuation/rescue sequence

### Map Point Value Mapping

Update `ConfigurationHandler.GetMapPoints()`:

```csharp
public static int GetMapPoints(string mapName)
{
    // Actual PEAK biome names
    if (mapName.Contains("Shore")) return Map1Points;
    if (mapName.Contains("Tropics") || mapName.Contains("Roots")) return Map2Points;
    if (mapName.Contains("Alpine") || mapName.Contains("Mesa")) return Map3Points;
    if (mapName.Contains("Caldera")) return Map4Points;
    if (mapName.Contains("Kiln")) return RuthsMapPoints; // Kiln is the hardest

    return 1; // Default
}
```

### Special Considerations

#### Ghost Mode
- Our design says "losers turn to skeletons"
- PEAK has ghost mode where dead players can still interact
- **Option 1**: Embrace ghosts, they're part of PEAK's charm
- **Option 2**: Track "alive" vs "ghost" to determine team status
- **Recommended**: Use ghost mode as-is, just track which team has living players

#### Items Persistence
- Default PEAK: Items reset between biomes at checkpoints
- Our mod: Items persist for all players between rounds
- **Implementation**: May need to prevent item clearing at checkpoints

#### Daily Map System
- PEAK uses procedurally generated daily maps
- Layout changes every 24 hours
- Our mod should work with any daily variation

#### Scoutmaster
- Spawns when players separate too far
- Throws players off mountain
- Should still work normally in competitive mode
- Adds extra challenge: teams must stay together!

### Configuration Recommendations

Based on PEAK's biome structure:

```ini
[MapPoints]
ShorePoints = 1          # Easy tutorial area
TropicsRootsPoints = 2   # Jungle/Forest (medium)
AlpineMesaPoints = 3     # Snow/Desert (hard)
CalderaPoints = 4        # Volcano slopes (very hard)
KilnPoints = 5           # Inner volcano (extreme)
```

### Testing Checklist

When you implement the patches:

- [ ] Test on each biome (Shore, Tropics, Roots, Alpine, Mesa, Caldera, Kiln)
- [ ] Verify checkpoint detection works
- [ ] Confirm team assignment persists across biomes
- [ ] Check score updates correctly
- [ ] Test with daily map variations
- [ ] Verify items persist as intended
- [ ] Test ghost mode interaction
- [ ] Confirm Scoutmaster still works
- [ ] Test 2v2, 2v2v2, and other team configs
- [ ] Verify all teams dead scenario
- [ ] Test Kiln/Summit match end

### Known PEAK Code (from PEAK Unlimited)

These are confirmed to exist:

```csharp
// Player connection
PlayerConnectionLog.OnPlayerEnteredRoom(Player)
PlayerConnectionLog.OnPlayerLeftRoom(Player)

// Level loading
AirportCheckInKiosk.LoadIslandMaster()

// Campfires
Campfire.Awake()

// End game
EndScreen.Start()
EndSequence.Routine()

// Networking
NetworkingUtilities.HostRoomOptions()
```

Use these as starting points for your patches!

### Photon Networking Notes

- PEAK uses Photon PUN 2 for multiplayer
- Only host/master client should modify game state
- Use `PhotonNetwork.IsMasterClient` checks
- `PhotonNetwork.Instantiate()` for networked objects
- `PhotonNetwork.PlayerList` for all players
- `PhotonNetwork.CurrentRoom.PlayerCount` for count

---

## Quick Implementation Guide

1. **Find Campfire checkpoint trigger** → Hook for round win
2. **Find biome/scene name** → Map to point values
3. **Count non-ghost players per team** → All teams dead check
4. **Detect Kiln/Summit completion** → Match end

Good luck! 🏔️
