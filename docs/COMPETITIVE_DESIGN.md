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

1. **Base Round Win:**
   - First team to checkpoint gets base points
   - Example: Win Caldera round = +4 base points

2. **Individual Completion Bonus:**
   - EACH living team member that reaches checkpoint = +bonus points
   - Bonus = base points × 0.5 (configurable multiplier)
   - Example: Caldera (4pts base) with 2 living teammates = 4 + (4×0.5×2) = 8 total points
   - Creates incentive to keep all teammates alive!

3. **Full Team Bonus:**
   - If ALL team members reach checkpoint alive = additional multiplier
   - Full team bonus = base points × 1.0
   - Example: Caldera (4pts) with all teammates alive = 4 (base) + 4 (individual: 4×0.5×2) + 4 (full team) = 12 points!

4. **Scoring Formula:**
   ```
   Total Points = Base Points + (Base × Individual Multiplier × Living Members) + (Full Team Bonus if all alive)

   Where:
   - Base Points = map difficulty value
   - Individual Multiplier = 0.5 (configurable)
   - Living Members = count of alive teammates at checkpoint
   - Full Team Bonus = Base Points (if all members alive)
   ```

5. **Example Scenarios:**

   **Scenario A: Both teammates alive (Caldera = 4pts base)**
   - Base: 4 points
   - Individual: 4 × 0.5 × 2 = 4 points
   - Full Team: 4 points
   - **Total: 12 points**

   **Scenario B: One teammate died (Caldera = 4pts base)**
   - Base: 4 points
   - Individual: 4 × 0.5 × 1 = 2 points
   - Full Team: 0 points (not all alive)
   - **Total: 6 points**

   **Scenario C: Both died but team had lead (Caldera = 4pts base)**
   - Leading team gets only base points (no bonuses)
   - **Total: 4 points**

6. **All Teams Die:**
   - Team with highest current score gets base points only
   - No individual or full team bonuses
   - Prevents stalling/griefing

7. **Match End at Kiln:**
   - Total accumulated points determine winner
   - Tiebreaker: Team with most individual completions wins
   - Second tiebreaker: First team to Kiln wins

---

## Match Scenarios

### Scenario 1: Perfect Victory (All Teammates Alive)
```
Round 1 (Shore, 1pt base, both alive):
  Red Team: 1 + (1×0.5×2) + 1 = 3 points

Round 2 (Tropics, 2pts base, both alive):
  Blue Team: 2 + (2×0.5×2) + 2 = 6 points
  (Red Total: 3, Blue Total: 6)

Round 3 (Alpine, 3pts base, both Red alive):
  Red Team: 3 + (3×0.5×2) + 3 = 9 points
  (Red Total: 12, Blue Total: 6)

Round 4 (Caldera, 4pts base, only 1 Red alive):
  Red Team: 4 + (4×0.5×1) + 0 = 6 points
  (Red Total: 18, Blue Total: 6)

Round 5 (Kiln, 5pts base, both Red alive again):
  Red Team: 5 + (5×0.5×2) + 5 = 15 points

Final Scores:
Red Team: 33 points - WINNERS!
Blue Team: 6 points

Note: Red's teammate death in Round 4 cost them 6 points (2 individual + 4 full team bonus)!
```

### Scenario 2: Comeback Victory Through Teamwork
```
Round 1 (Shore, 1pt, Red: both alive, Blue: 1 dead):
  Red: 1 + 1 + 1 = 3
  Blue: 1 + 0.5 + 0 = 1.5 ✗ (Lost round)
  (Red: 3, Blue: 0)

Round 2 (Tropics, 2pts, Blue: both alive, Red: 1 dead):
  Blue: 2 + 2 + 2 = 6
  Red: Lost
  (Red: 3, Blue: 6)

Round 3 (Alpine, 3pts, Blue: both alive):
  Blue: 3 + 3 + 3 = 9
  (Red: 3, Blue: 15)

Round 4 (Caldera, 4pts, Red: both alive - comeback!):
  Red: 4 + 4 + 4 = 12
  (Red: 15, Blue: 15) - TIED!

Round 5 (Kiln, 5pts, Red: both alive):
  Red: 5 + 5 + 5 = 15

Final Scores:
Red Team: 30 points - WINNERS!
Blue Team: 15 points

Lesson: Blue lost because they let a teammate die in Rounds 1-2, while Red kept both alive in final rounds!
```

### Scenario 3: The Cost of Carelessness
```
Round 1 (Shore, 1pt):
  Red: both alive = 1 + 1 + 1 = 3
  Blue: Lost

Round 2 (Tropics, 2pts):
  Red: both alive = 2 + 2 + 2 = 6
  (Red: 9, Blue: 0)

Round 3 (Alpine, 3pts):
  Red: ONE DIES during climb!
  Red: only 1 reaches = 3 + 1.5 + 0 = 4.5
  (Red: 13.5, Blue: 0)

Round 4 (Caldera, 4pts):
  Red: revived teammate, BOTH alive = 4 + 4 + 4 = 12
  (Red: 25.5, Blue: 0)

Round 5 (Kiln, 5pts):
  Blue makes incredible comeback, both alive!
  Blue: 5 + 5 + 5 = 15
  (Red: 25.5, Blue: 15)

Final Score:
Red Team: 25.5 points - WINNERS
Blue Team: 15 points

Analysis: Red's teammate death in Round 3 cost them 4.5 points!
(Lost: 1.5 individual + 3 full team = 4.5 points)
This shows the importance of keeping teammates alive!
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
