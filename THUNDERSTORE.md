# Publishing to Thunderstore

Complete guide for publishing PEAK Competitive to Thunderstore.

## What is Thunderstore?

**Thunderstore** is the primary mod hosting platform for Unity games. It provides:
- Easy mod installation via mod managers (r2modman, Thunderstore Mod Manager)
- Automatic dependency management
- Version control and updates
- Community ratings and feedback

**URL:** https://thunderstore.io

---

## Prerequisites

Before publishing, you need:

1. ✅ **Thunderstore account** - Sign up at https://thunderstore.io
2. ✅ **Icon image** - 256x256 PNG (see Icon Requirements below)
3. ✅ **Tested mod** - Verify it works in-game
4. ✅ **Version number** - Semantic versioning (e.g., 1.0.0)

---

## Package Structure

Thunderstore requires this exact structure:

```
PEAKCompetitive-1.0.0.zip
├── manifest.json          ← Package metadata (REQUIRED)
├── README.md             ← Mod description (REQUIRED)
├── icon.png              ← 256x256 PNG (REQUIRED)
├── CHANGELOG.md          ← Version history (optional)
└── plugins/              ← DLL files
    └── PEAKCompetitive.dll
```

**All files in the root**, except DLLs which go in `plugins/` folder.

---

## Icon Requirements

### Specifications
- **Size:** Exactly 256x256 pixels
- **Format:** PNG with transparency
- **Content:** Mod branding/logo
- **Style:** Clear, readable, professional

### Icon Ideas for PEAK Competitive
- Mountain peak with checkered flag
- Two teams racing up mountain
- Trophy with star ratings (★★★★★)
- "PEAK" text with competitive theme
- Minimal geometric mountain design

### Creating an Icon

**Option 1: Use AI Generation**
```
Prompt: "256x256 icon for competitive mountain racing game mod,
         minimalist design, mountain peak with checkered flag,
         blue and red team colors, gaming logo style"
```

**Option 2: Use Free Tools**
- Canva (free tier)
- GIMP (free, open-source)
- Photopea (free, browser-based)

**Save as:** `thunderstore/icon.png`

---

## Building the Package

### Automated Build (Recommended)

```powershell
.\package-thunderstore.ps1 -Version "1.0.0"
```

This will:
1. Build the mod in Release mode
2. Copy files to package structure
3. Update version in manifest.json
4. Create `PEAKCompetitive-1.0.0.zip`
5. Verify contents

### Manual Build

1. **Build the mod:**
   ```bash
   dotnet build src/PEAKCompetitive/PEAKCompetitive.csproj -c Release
   ```

2. **Create folder structure:**
   ```
   thunderstore-package/
   ├── plugins/
   └── (root files)
   ```

3. **Copy files:**
   ```powershell
   # DLL
   cp artifacts/bin/Release/PEAKCompetitive.dll thunderstore-package/plugins/

   # Metadata
   cp thunderstore/manifest.json thunderstore-package/
   cp thunderstore/README.md thunderstore-package/
   cp thunderstore/icon.png thunderstore-package/
   cp thunderstore/CHANGELOG.md thunderstore-package/
   ```

4. **Update version** in `manifest.json`:
   ```json
   {
     "version_number": "1.0.0"
   }
   ```

5. **Create ZIP:**
   - Select **all files** in `thunderstore-package/`
   - Right-click → "Compress to ZIP"
   - Name: `PEAKCompetitive-1.0.0.zip`

---

## Manifest.json Fields

Update these fields in `thunderstore/manifest.json`:

```json
{
    "name": "PEAKCompetitive",          // Mod ID (no spaces)
    "version_number": "1.0.0",          // SemVer format
    "website_url": "https://...",       // GitHub/docs URL
    "description": "Short desc...",     // Max 250 chars
    "dependencies": [
        "BepInEx-BepInExPack-5.4.2100"  // Required mods
    ]
}
```

**Important:**
- `name` must be unique on Thunderstore
- `version_number` uses format: `MAJOR.MINOR.PATCH`
- `dependencies` must use exact Thunderstore package names

---

## Uploading to Thunderstore

### Step 1: Login
1. Go to https://thunderstore.io
2. Click "Sign In" (top-right)
3. Login with Discord, GitHub, or email

### Step 2: Find PEAK
1. Search for "PEAK" in game list
2. If PEAK isn't listed, you may need to:
   - Request game addition
   - Or publish under "Other" category

### Step 3: Upload Package
1. Click your profile → "Upload"
2. Select game: **PEAK**
3. Click "Upload file"
4. Choose `PEAKCompetitive-1.0.0.zip`
5. Fill in submission form:

**Team Name:** Your username or team name
**Categories:**
- Gameplay Mechanics
- Multiplayer
- Server-side

**NSFW:** No
**Submit**

### Step 4: Verify Upload
- Check mod page appears correctly
- Test "Install with Mod Manager" button
- Verify README displays properly
- Check icon shows up

---

## Version Updates

When releasing updates:

1. **Update version** in these files:
   ```
   thunderstore/manifest.json     → version_number
   thunderstore/CHANGELOG.md      → Add new section
   src/PEAKCompetitive/Plugin.cs → [BepInPlugin version]
   ```

2. **Build new package:**
   ```powershell
   .\package-thunderstore.ps1 -Version "1.0.1"
   ```

3. **Upload to Thunderstore** (same process)

**Thunderstore will:**
- Detect new version automatically
- Notify users who installed via mod manager
- Keep old versions available

---

## Mod Manager Testing

Before publishing, test with mod manager:

### Using r2modman

1. **Install r2modman:** https://thunderstore.io/package/ebkr/r2modman/
2. **Select PEAK** as game
3. **Install from file:**
   - Settings → Profile → "Import local mod"
   - Select your ZIP file
4. **Launch game** via r2modman
5. **Verify mod works**

---

## Best Practices

### Before First Upload
- [ ] Test mod with at least 2 players
- [ ] Verify all 7 biomes score correctly
- [ ] Check F3 menu works
- [ ] Test with/without other mods
- [ ] Check BepInEx logs for errors
- [ ] Create 256x256 icon.png
- [ ] Write clear README
- [ ] Add screenshots/GIFs to README

### Version Numbering
- **1.0.0** - Initial release
- **1.0.1** - Bug fixes
- **1.1.0** - New features (backward compatible)
- **2.0.0** - Breaking changes

### README Tips
- Add screenshots of scoreboard
- Show scoring examples
- Include configuration guide
- List known issues
- Provide support links

---

## Troubleshooting

### "Package rejected: Invalid manifest"
- Check `manifest.json` is valid JSON
- Verify all required fields present
- Ensure version format is `X.Y.Z`

### "Icon must be 256x256"
```bash
# Check icon size
file thunderstore/icon.png

# Should show: PNG image data, 256 x 256
```

### "Package too large"
- Remove unnecessary files from ZIP
- Only include DLL, not source code
- Thunderstore limit: 500 MB (you're well under)

### "Dependency not found"
- Check dependency name exactly matches Thunderstore
- Format: `Author-PackageName-Version`
- Example: `BepInEx-BepInExPack-5.4.2100`

---

## After Publishing

### Monitor Feedback
- Check comments on Thunderstore page
- Respond to issues
- Update mod based on feedback

### Promote Your Mod
- Post on PEAK Discord/Reddit
- Add to modding wikis
- Create video showcase

### Maintain Updates
- Fix bugs promptly
- Add requested features
- Keep dependencies updated

---

## Quick Checklist

Before uploading:

- [ ] Mod builds without errors
- [ ] Tested in multiplayer match
- [ ] All 7 biomes score correctly
- [ ] F3 menu opens and saves settings
- [ ] Scoreboard displays properly
- [ ] Icon.png is 256x256 PNG
- [ ] manifest.json has correct version
- [ ] README.md is descriptive
- [ ] CHANGELOG.md updated
- [ ] GitHub URL updated (if applicable)
- [ ] Tested with mod manager

---

## Useful Links

- **Thunderstore:** https://thunderstore.io
- **Mod Manager (r2modman):** https://thunderstore.io/package/ebkr/r2modman/
- **Thunderstore Package Format:** https://thunderstore.io/package/create/docs/
- **BepInEx Docs:** https://docs.bepinex.dev/
- **Harmony Docs:** https://harmony.pardeike.net/

---

**Ready to publish?** Run `.\package-thunderstore.ps1` and upload!

**Last Updated:** 2025-11-22
