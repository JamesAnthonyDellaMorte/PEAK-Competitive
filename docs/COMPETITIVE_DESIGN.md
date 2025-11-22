# PEAK Competitive - Design Document

## Game Mode: Duo Team Race

### Core Concept
2v2 (or configurable team sizes) race to the summit. First team to reach the checkpoint wins the round and earns points. Match continues until all players reach the Kiln or all teams are eliminated.

---

## Round Flow

### 1. Round Start
- All players spawn at base of biome
- Timer/announcement displays round number and map
- Teams race to the checkpoint

### 2. Round Win Condition
**First team to reach checkpoint campfire wins**

When winning team reaches campfire:
- Round ends immediately
- Winning team gets points based on map difficulty
- Losing teams turn to ghosts

### 3. Ghost System - Team Localized

**When your team loses the round:**
- All team members become ghosts
- **Ghosts can ONLY interact with their own team:**
  - Can only see/hear teammates (living or ghost)
  - Cannot see or interact with other teams
  - Can still talk to teammates via proximity chat
  - Can help living teammates with limited interactions
  - Spectate from teammate perspectives

**Purpose:**
- Keeps losing team engaged
- Prevents interference with winning teams
- Maintains team communication
- Adds tactical element (ghost teammates can scout/guide)

### 4. Round End & Reset
- Points awarded to winning team
- Scoreboard updates
- All players (including ghosts) teleport to next biome checkpoint
- Ghosts revive at Ancient Statue for next round
- New round begins

### 5. Items Between Rounds
**Items persist for ALL players** (both winners and losers)
- Fair for both teams
- No snowball effect
- Rewards resource management
- Everyone keeps their gear

---

## Match End Conditions

### Condition A: Reach the Kiln
- All living players reach the Kiln (final biome)
- Team with most accumulated points wins
- Victory screen displays final scores

### Condition B: All Teams Eliminated
- If all teams die before reaching Kiln
- Team with most points at time of elimination wins
- Match ends immediately

---

## Team-Based Ghost Mechanics

### Ghost Limitations (Per Team)
```
Ghost Properties:
- Translucent appearance (team colored)
- Can move freely (no stamina)
- Cannot climb to finish (round already lost)
- Cannot interact with world objects
- CAN see and talk to own team only
- CAN see environment/hazards

Ghost Abilities:
- Scout ahead for team
- Call out hazards to living teammates
- Provide moral support
- Learn the route for next attempt
- Limited object interaction (move small items to help team)
```

### Implementation Approach

**Option 1: Visibility Filters**
- Hide all non-team members from ghosts
- Hide ghosts from other teams
- Team-only voice chat range

**Option 2: Ghost Layers**
- Each team has separate ghost "layer"
- Ghosts only exist in their team's layer
- Prevents cross-team ghost interactions

**Option 3: Spatial Separation**
- Teleport losing team ghosts to separate spectator area
- Can view team's perspective
- Rejoin when round ends

**Recommended: Option 1 (Visibility Filters)**
- Most elegant
- Least disruptive to PEAK's existing ghost system
- Ghosts can still help their team navigate
- No cross-team trolling/interference

---

## Scoring System

### Point Values by Biome

| Biome | Difficulty | Default Points |
|-------|-----------|----------------|
| Shore | Easy (Tutorial) | 1 |
| Tropics/Roots | Medium (Jungle/Forest) | 2 |
| Alpine/Mesa | Hard (Snow/Desert) | 3 |
| Caldera | Very Hard (Volcano) | 4 |
| Kiln | Extreme (Inner Volcano) | 5 |

**Host Configurable:** Points can be adjusted per map (1-10 range)

### Point Award Rules

1. **Normal Round Win:**
   - First team to checkpoint gets full points
   - Example: Win Caldera round = +4 points

2. **All Teams Die:**
   - Team with highest current score gets points
   - Prevents stalling/griefing
   - Rewards leading team

3. **Match End at Kiln:**
   - No additional points for reaching Kiln
   - Total accumulated points determine winner
   - Tiebreaker: First team to Kiln wins

---

## Match Scenarios

### Scenario 1: Clean Victory
```
Round 1 (Shore): Red Team reaches checkpoint first → Red +1
Round 2 (Tropics): Blue Team reaches checkpoint first → Blue +2
Round 3 (Alpine): Red Team reaches checkpoint first → Red +3
Round 4 (Caldera): Red Team reaches checkpoint first → Red +4
Round 5 (Kiln): Red Team reaches checkpoint first → Red +5

Final Scores:
Red Team: 13 points - WINNERS!
Blue Team: 2 points
```

### Scenario 2: Everyone Dies Mid-Match
```
Round 1 (Shore): Red Team wins → Red +1 (Total: Red 1, Blue 0)
Round 2 (Tropics): Blue Team wins → Blue +2 (Total: Red 1, Blue 2)
Round 3 (Alpine): Both teams die before checkpoint
  → Blue team has higher score → Blue +3 (Total: Red 1, Blue 5)

All teams dead, match ends:
Blue Team: 5 points - WINNERS!
Red Team: 1 point
```

### Scenario 3: Close Race
```
Round 1: Red +1 (Red: 1, Blue: 0)
Round 2: Blue +2 (Red: 1, Blue: 2)
Round 3: Red +3 (Red: 4, Blue: 2)
Round 4: Blue +4 (Red: 4, Blue: 6)
Round 5: Red +5 (Red: 9, Blue: 6)

Red Team: 9 points - WINNERS!
Blue Team: 6 points
```

---

## UI/UX Flow

### During Round
1. **Scoreboard** (Top-left)
   - Team names with colors
   - Current scores
   - Round number
   - Current biome

2. **Match Notification** (Center)
   - Round start: "Round 3 - Alpine"
   - Round end: "Red Team Wins! +3 points"
   - Ghost notification: "You are now a ghost - Help your team!"

3. **Ghost HUD** (When ghosted)
   - "Spectating - Round Lost"
   - Team status indicator
   - Living teammates locations
   - "Revive next round" countdown/info

### Between Rounds
1. **Scoreboard Update**
   - Points awarded animation
   - Updated team totals
   - Leading team highlighted

2. **Transition**
   - "Next Round Starting..."
   - Brief pause at checkpoint
   - Ghosts revive at Ancient Statue

### Match End
1. **Victory Screen**
   - Winning team celebration
   - Final scoreboard
   - Match statistics
   - MVP badges (most checkpoints, most revives, etc.)

---

## Competitive Settings

### Host Configuration Menu (F3)

**Team Settings:**
- Number of Teams (2-10)
- Players Per Team (1-4)
- Auto-balance teams

**Match Settings:**
- Enable/Disable Competitive Mode
- Items Persist (on/off)
- Show Scoreboard (on/off)

**Map Points:**
- Shore Points (1-10)
- Tropics/Roots Points (1-10)
- Alpine/Mesa Points (1-10)
- Caldera Points (1-10)
- Kiln Points (1-10)

**Ghost Settings:**
- Ghost Team Isolation (on/off)
- Ghost Voice Chat Range (team-only/global)
- Ghost Interaction Level (none/limited/full)

---

## Technical Implementation

### Ghost Team Isolation

```csharp
// When round ends and team loses
public void OnRoundLost(TeamData losingTeam)
{
    foreach (var player in losingTeam.Members)
    {
        // Player becomes ghost (PEAK's existing system)
        ConvertToGhost(player);

        // Apply team isolation
        SetGhostVisibilityFilter(player, losingTeam.TeamId);
        SetVoiceChatFilter(player, losingTeam.TeamId);
    }
}

// Visibility filter
private void SetGhostVisibilityFilter(Player ghostPlayer, int teamId)
{
    // Hide all players not on same team
    foreach (var otherPlayer in PhotonNetwork.PlayerList)
    {
        if (GetPlayerTeam(otherPlayer)?.TeamId != teamId)
        {
            // Hide this player from ghost
            HidePlayerFromGhost(ghostPlayer, otherPlayer);
        }
    }
}

// Voice chat filter
private void SetVoiceChatFilter(Player ghostPlayer, int teamId)
{
    // Only hear teammates
    // Mute all non-team voice chat for this ghost
}
```

### Round Win Detection

```csharp
// When player reaches checkpoint
[HarmonyPatch(typeof(Campfire), "OnPlayerReached")]
public class CheckpointPatch
{
    static void Postfix(Player player)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var team = TeamManager.GetPlayerTeam(player);
        var matchState = MatchState.Instance;

        // First team to checkpoint wins
        if (!matchState.IsRoundActive) return;

        // Award points
        int points = ConfigurationHandler.GetMapPoints(matchState.CurrentMapName);
        matchState.TeamReachedCheckpoint(team, points);

        // Convert losing teams to ghosts
        foreach (var losingTeam in matchState.Teams)
        {
            if (losingTeam.TeamId != team.TeamId)
            {
                ConvertTeamToGhosts(losingTeam);
            }
        }

        // End round
        matchState.EndRound(team, points);
    }
}
```

---

## Balance Considerations

### Advantages of Ghost Team Isolation

**Prevents:**
- Cross-team trolling
- Ghost interference with winners
- Information leaking between teams
- Unfair scouting advantages

**Enables:**
- Team strategy and coaching
- Learning routes for next attempt
- Maintaining team cohesion
- Continued engagement for losers

### Fairness

**Items Persist = Fair:**
- Both teams accumulate gear
- No winner snowball
- Skill remains primary factor
- Resource management matters

**Point Progression:**
- Later biomes worth more points
- Rewards consistency across all rounds
- Comeback potential exists
- No single round determines match

---

## Player Experience

### Winning Team
- Pride in victory
- Immediate feedback
- Points accumulation
- Continue climbing with confidence

### Losing Team (Ghosts)
- Stay engaged with team
- Scout ahead for team info
- Learn the route
- Moral support role
- Anticipation for next round revival

### Spectator Perspective
- See team's color-coded ghosts
- Hear team strategy discussions
- Feel included in match
- Build anticipation for revival

---

## Future Enhancements

### Ghost Features
- Ghost chat commands to ping hazards
- Ghost waypoint markers (visible to team only)
- Ghost "recon mode" to view multiple teammates
- Achievement for "best ghost assist"

### Match Variations
- Best of X rounds mode
- Time trial mode (fastest to Kiln)
- Elimination mode (lose round = lose player)
- Handicap mode (point multipliers)

---

## Implementation Priority

### Phase 1: Core Mechanics ✅
- Team system
- Score tracking
- Match state management
- Configuration system

### Phase 2: Game Integration (In Progress)
- Summit/checkpoint detection
- Round start on level load
- Ghost conversion on round loss
- Match end at Kiln

### Phase 3: Ghost Isolation
- Team visibility filters
- Voice chat filtering
- Ghost UI updates
- Team-only interactions

### Phase 4: Polish
- Victory animations
- Better notifications
- Statistics tracking
- Achievement integration

---

## Summary

PEAK Competitive transforms PEAK into a team-based racing game where:
1. Teams race to checkpoints each round
2. Losers become team-localized ghosts
3. Points accumulate across biomes
4. First to Kiln (or last standing) with most points wins
5. Items persist to keep gameplay fair
6. Ghost system keeps everyone engaged

**Key Innovation:** Team-isolated ghosts prevent trolling while maintaining PEAK's signature cooperative ghost mechanic, adapted for competitive play.
