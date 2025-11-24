# PEAK Competitive - Installation Guide

## Prerequisites

1. **PEAK** installed via Steam
2. **Windows PC** (PEAK is Windows-only)

---

## Step 1: Install BepInEx (Mod Loader)

BepInEx is required to run any PEAK mods.

### Download BepInEx

1. Go to: https://github.com/BepInEx/BepInEx/releases
2. Download **BepInEx_x64_5.4.23.2.zip** (or latest 5.x version)
   - ⚠️ Make sure it's the **x64** version, not x86!

### Install BepInEx

1. **Extract the ZIP** - you should see:
   ```
   BepInEx/
   doorstop_config.ini
   winhttp.dll
   changelog.txt
   ```

2. **Copy everything** to your PEAK game folder:
   ```
   C:\Program Files (x86)\Steam\steamapps\common\PEAK\
   ```

3. **Verify folder structure** looks like:
   ```
   PEAK/
   ├── BepInEx/           ← New folder
   ├── PEAK_Data/
   ├── PEAK.exe
   ├── winhttp.dll        ← New file
   └── doorstop_config.ini ← New file
   ```

### First Launch (Initialize BepInEx)

1. **Launch PEAK** from Steam
2. Wait for game to fully load (may be slower first time)
3. **Close PEAK**
4. **Verify BepInEx initialized:**
   ```
   PEAK/BepInEx/plugins/    ← This folder should now exist
   PEAK/BepInEx/config/     ← This folder should now exist
   ```

✅ BepInEx is now installed!

---

## Step 2: Install PEAK Competitive Mod

### Build the Mod

If you haven't built it yet:

```bash
cd C:\Users\della\PEAK-Competitive
dotnet build src/PEAKCompetitive/PEAKCompetitive.csproj -c Release
```

This creates: `artifacts/bin/Release/PEAKCompetitive.dll`

### Install the DLL

1. **Create plugin folder:**
   ```
   C:\Program Files (x86)\Steam\steamapps\common\PEAK\BepInEx\plugins\PEAKCompetitive\
   ```

2. **Copy the DLL:**
   ```
   From: C:\Users\della\PEAK-Competitive\artifacts\bin\Release\PEAKCompetitive.dll
   To:   C:\Program Files (x86)\Steam\steamapps\common\PEAK\BepInEx\plugins\PEAKCompetitive\PEAKCompetitive.dll
   ```

### Verify Installation

Your folder structure should look like:
```
PEAK/
├── BepInEx/
│   ├── plugins/
│   │   └── PEAKCompetitive/
│   │       └── PEAKCompetitive.dll  ← Your mod
│   ├── config/
│   └── core/
├── PEAK_Data/
└── PEAK.exe
```

---

## Step 3: Launch and Configure

### First Launch

1. **Launch PEAK** from Steam
2. **Check BepInEx console** (should appear if BepInEx is working)
3. Look for log message:
   ```
   [Info   :   BepInEx] Loading [PEAKCompetitive 1.0.0]
   [Info   : PEAKCompetitive] PEAK Competitive initialized!
   [Info   : PEAKCompetitive] === PEAK Competitive Configuration ===
   [Info   : PEAKCompetitive] Competitive Mode: True
   [Info   : PEAKCompetitive] Teams: 2 teams of 2
   ```

### Open Settings Menu

1. **In-game, press F3** to open competitive settings
2. You should see:
   ```
   ╔══════════════════════════════════════╗
   ║  PEAK Competitive Settings           ║
   ╠══════════════════════════════════════╣
   ║  === Match Settings ===              ║
   ║  [✓] Enable Competitive Mode         ║
   ║  [✓] Show Scoreboard                 ║
   ║  [✓] Items Persist Between Rounds    ║
   ║                                      ║
   ║  === Biome Point Values ===          ║
   ║  Shore (★☆☆☆☆):     [1]             ║
   ║  Tropics (★★☆☆☆):   [2]             ║
   ║  ...                                 ║
   ╚══════════════════════════════════════╝
   ```

✅ Mod is installed and working!

---

## Step 4: Test in Multiplayer

### Host a Match

1. **Host a multiplayer game** (need at least 2 players)
2. **Teams auto-assign** when match starts
3. **Scoreboard appears** in top-left corner
4. **Race to checkpoint** - winner gets points!

### Check Logs

BepInEx logs are here:
```
C:\Program Files (x86)\Steam\steamapps\common\PEAK\BepInEx\LogOutput.log
```

Look for:
```
[Info   : PEAKCompetitive] Team Red reaches checkpoint!
[Info   : PEAKCompetitive]   Base points: 4
[Info   : PEAKCompetitive]   Survivor bonus: +4 pts (2/2 survived)
[Info   : PEAKCompetitive]   Full team bonus: +4 pts
[Info   : PEAKCompetitive]   TOTAL POINTS: 12 (300% of base)
```

---

## Troubleshooting

### "Mod doesn't load"

**Check BepInEx is installed:**
```
PEAK/winhttp.dll exists?
PEAK/BepInEx/ folder exists?
```

**Check DLL is in correct location:**
```
PEAK/BepInEx/plugins/PEAKCompetitive/PEAKCompetitive.dll
```

**Check BepInEx console:**
- Console should appear when game launches
- If no console, BepInEx isn't installed correctly

### "F3 doesn't open menu"

**Check config:**
```
PEAK/BepInEx/config/PEAKCompetitive.cfg
```

Look for:
```ini
[General]
MenuKey = F3
```

Change to different key if F3 conflicts with other mods.

### "Scoreboard doesn't appear"

**Press F3** and check:
- [ ] "Show Scoreboard" is enabled
- [ ] "Enable Competitive Mode" is enabled

**Check you're in multiplayer:**
- Scoreboard only shows in multiplayer matches
- Need at least 2 players

### "Build errors when compiling"

**Make sure you have:**
- .NET SDK 8.0+ installed
- PEAK game DLLs in correct location (see ManagedDir in .csproj)

**Clean and rebuild:**
```bash
dotnet clean
dotnet restore
dotnet build -c Release
```

---

## Uninstalling

### Remove Mod Only
Delete:
```
PEAK/BepInEx/plugins/PEAKCompetitive/
PEAK/BepInEx/config/PEAKCompetitive.cfg
```

### Remove BepInEx Entirely
Delete:
```
PEAK/BepInEx/
PEAK/winhttp.dll
PEAK/doorstop_config.ini
```

Then **verify game files** in Steam to restore originals.

---

## Quick Install Script (PowerShell)

```powershell
# Run this from PEAK-Competitive project folder
$PEAKPath = "C:\Program Files (x86)\Steam\steamapps\common\PEAK"
$PluginPath = "$PEAKPath\BepInEx\plugins\PEAKCompetitive"

# Build mod
dotnet build src/PEAKCompetitive/PEAKCompetitive.csproj -c Release

# Create plugin folder
New-Item -ItemType Directory -Path $PluginPath -Force

# Copy DLL
Copy-Item "artifacts\bin\Release\PEAKCompetitive.dll" "$PluginPath\PEAKCompetitive.dll"

Write-Host "Installed to: $PluginPath" -ForegroundColor Green
Write-Host "Launch PEAK and press F3 to configure!" -ForegroundColor Cyan
```

Save as `install.ps1` and run:
```powershell
.\install.ps1
```

---

## Next Steps

1. ✅ Install BepInEx
2. ✅ Build and copy mod DLL
3. ✅ Launch PEAK and verify mod loads
4. ✅ Press F3 to configure settings
5. ✅ Host multiplayer match to test
6. 📖 Read `SCORING_SYSTEM.md` for gameplay details

---

**Need help?** Check the BepInEx logs at:
```
PEAK/BepInEx/LogOutput.log
```

**Last Updated:** 2025-11-22
