# Mac Development Setup for PEAK Competitive

Since PEAK is Windows-only but you're developing on Mac, here's how to set up your environment.

## Step 1: Get PEAK Game DLLs

You need the game's DLL files to compile against. There are two ways to get them:

### Method A: Copy from Windows PC (Recommended)

1. **On Windows PC with PEAK installed:**
   ```
   Navigate to: C:\Program Files (x86)\Steam\steamapps\common\PEAK\PEAK_Data\Managed\
   ```

2. **Copy ALL .dll files** from that folder

3. **Transfer to your Mac:**
   - ZIP the folder: `Managed.zip`
   - Transfer via USB drive, cloud storage, email, etc.

4. **On Mac, extract to your project:**
   ```bash
   cd PEAK-Competitive
   unzip ~/Downloads/Managed.zip -d lib/PEAK/
   ```

5. **Verify you have the files:**
   ```bash
   ls lib/PEAK/Assembly-CSharp.dll
   # Should show: lib/PEAK/Assembly-CSharp.dll
   ```

### Method B: Use dnSpy to Extract (Advanced)

If you want to decompile and explore the code:

1. **Install dnSpy on Windows:**
   - Download: https://github.com/dnSpy/dnSpy/releases
   - Extract and run `dnSpy.exe`

2. **Open PEAK's DLL:**
   - File → Open
   - Navigate to: `C:\...\PEAK\PEAK_Data\Managed\Assembly-CSharp.dll`

3. **Explore the code:**
   - Browse classes like `Campfire`, `PlayerConnectionLog`, etc.
   - This helps you understand what to patch!

4. **Copy DLLs as in Method A** (dnSpy is just for viewing, you still need the actual DLLs)

## Step 2: Install .NET SDK

```bash
brew install --cask dotnet-sdk
```

Verify:
```bash
dotnet --version
# Should show: 8.x.x or 10.x.x
```

## Step 3: Build the Mod

```bash
cd PEAK-Competitive
./build.sh
```

Expected output:
```
======================================
  PEAK Competitive - Build Script
======================================

✓ .NET SDK found: 10.0.100
✓ Cleaned
✓ Restored
✓ Built

======================================
  Build Successful!
======================================

Output: artifacts/bin/Release/PEAKCompetitive.dll
Size: 15K
```

## Step 4: Test on Windows

The DLL you built on Mac works on Windows!

1. **Copy to Windows PC:**
   ```
   artifacts/bin/Release/PEAKCompetitive.dll
   ```

2. **Install on Windows:**
   ```
   Copy to: C:\Program Files (x86)\Steam\steamapps\common\PEAK\BepInEx\plugins\
   ```

3. **Launch PEAK** and test!

## Troubleshooting

### "Assembly-CSharp.dll could not be found"

**Problem:** Build can't find game DLLs

**Solution:** Make sure you copied the DLLs to `lib/PEAK/`:
```bash
ls lib/PEAK/Assembly-CSharp.dll
# If "No such file or directory", you need to copy the DLLs
```

### "Package 'UnityEngine.Modules' not found"

**Problem:** NuGet can't find Unity packages

**Solution:** This is fine, we don't use those if you have real game DLLs

### "59 errors" about missing types

**Problem:** Game DLLs not properly referenced

**Solution:**
1. Check DLLs are in `lib/PEAK/`
2. Check `.csproj` ManagedDir path
3. Run `dotnet clean` then `./build.sh` again

## Project Structure After Setup

```
PEAK-Competitive/
├── lib/
│   └── PEAK/                    ← Game DLLs go here
│       ├── Assembly-CSharp.dll  ← PEAK game code
│       ├── PhotonRealtime.dll
│       ├── UnityEngine.dll
│       └── ... (100+ more DLLs)
├── src/
│   └── PEAKCompetitive/
│       └── ... (your mod code)
├── artifacts/
│   └── bin/
│       └── Release/
│           └── PEAKCompetitive.dll  ← Built mod (works on Windows!)
└── build.sh
```

## Development Workflow

1. **Edit code on Mac** (any editor: VS Code, Rider, etc.)
2. **Build with `./build.sh`**
3. **Copy DLL to Windows PC** (via shared folder, USB, cloud, etc.)
4. **Test in PEAK on Windows**
5. **Iterate!**

## Remote Testing Setup (Optional)

If you have a Windows PC on the same network:

### Option A: Shared Folder

**On Windows:**
1. Share your PEAK plugins folder
2. Network path: `\\WINDOWS-PC\PEAK\BepInEx\plugins\`

**On Mac:**
```bash
# Mount Windows share
mount -t smbfs //WINDOWS-PC/PEAK /Volumes/PEAK

# Auto-copy after build
cp artifacts/bin/Release/PEAKCompetitive.dll /Volumes/PEAK/BepInEx/plugins/
```

### Option B: SSH/SCP

**On Windows:** Install OpenSSH Server

**On Mac:**
```bash
# After building, auto-deploy
scp artifacts/bin/Release/PEAKCompetitive.dll \
    user@windows-pc:"C:/Program Files (x86)/Steam/steamapps/common/PEAK/BepInEx/plugins/"
```

## Quick Reference

| Task | Command |
|------|---------|
| Build mod | `./build.sh` |
| Clean build | `rm -rf artifacts && ./build.sh` |
| Check DLLs | `ls lib/PEAK/ \| wc -l` (should be 100+) |
| Test build compiles | `dotnet build -c Debug` |

## Next Steps

Once you have the DLLs:
1. Run `./build.sh`
2. If successful, you're ready to develop!
3. See `DEVELOPMENT.md` for next steps

---

Need help? Check the build output for specific error messages.
