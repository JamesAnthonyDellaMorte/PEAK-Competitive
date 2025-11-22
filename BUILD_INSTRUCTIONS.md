# Build Instructions for PEAK Competitive

## Prerequisites

### 1. Install .NET SDK

**Option A: Homebrew (macOS)**
```bash
brew install --cask dotnet-sdk
```

**Option B: Direct Download**
Download from: https://dotnet.microsoft.com/download/dotnet/8.0
- For macOS: Download `.pkg` installer
- Run the installer
- Restart terminal

**Verify installation:**
```bash
dotnet --version
# Should show: 8.x.x or higher
```

### 2. Update Game Path (If Needed)

Edit `src/PEAKCompetitive/PEAKCompetitive.csproj`:

```xml
<!-- Line 12: Update this to your PEAK installation -->
<ManagedDir>C:\Program Files (x86)\Steam\steamapps\common\PEAK\PEAK_Data\Managed\</ManagedDir>
```

**Note:** This path is for Windows. On Mac, it might be:
```xml
<ManagedDir>~/Library/Application Support/Steam/steamapps/common/PEAK/PEAK.app/Contents/Resources/Data/Managed/</ManagedDir>
```

Or if you're cross-compiling for Windows, just leave the Windows path.

---

## Building

### Method 1: Build Script (Recommended)

```bash
cd PEAK-Competitive
./build.sh
```

Output: `artifacts/bin/Release/PEAKCompetitive.dll`

### Method 2: Manual Build

```bash
cd PEAK-Competitive

# Restore dependencies
dotnet restore src/PEAKCompetitive/PEAKCompetitive.csproj

# Build Release
dotnet build src/PEAKCompetitive/PEAKCompetitive.csproj -c Release

# Or build Debug
dotnet build src/PEAKCompetitive/PEAKCompetitive.csproj -c Debug
```

---

## Cross-Compiling for Windows (from Mac/Linux)

The mod is built with .NET Standard 2.1, which is **platform-independent**.

The DLL built on Mac **will work on Windows** because:
- .NET Standard 2.1 is cross-platform
- BepInEx runs on Unity (which abstracts the OS)
- No native code is used

**To build for Windows from Mac:**
```bash
./build.sh  # Same script, same output!
```

The resulting `PEAKCompetitive.dll` works on **both Windows and macOS**.

---

## Installation

### Windows
```
Copy: artifacts/bin/Release/PEAKCompetitive.dll
To:   C:\Program Files (x86)\Steam\steamapps\common\PEAK\BepInEx\plugins\
```

### macOS (if PEAK supports Mac)
```bash
cp artifacts/bin/Release/PEAKCompetitive.dll \
   ~/Library/Application\ Support/Steam/steamapps/common/PEAK/PEAK.app/Contents/Resources/BepInEx/plugins/
```

---

## Troubleshooting

### "dotnet: command not found"
- .NET SDK not installed
- Terminal needs restart after install
- Run: `export PATH=$PATH:/usr/local/share/dotnet`

### "error CS0006: Metadata file ... could not be found"
- Update `ManagedDir` path in `.csproj`
- Ensure PEAK is installed
- Check path has correct game DLLs

### "The reference assemblies ... were not found"
- Wrong .NET version
- Need .NET SDK 8.0+ (not just runtime)
- Run: `dotnet --list-sdks` to verify

### "Package BepInEx.Core not found"
- BepInEx NuGet feed not configured
- `.csproj` should have:
  ```xml
  <RestoreAdditionalProjectSources>
    https://nuget.bepinex.dev/v3/index.json
  </RestoreAdditionalProjectSources>
  ```

### Build succeeds but DLL is missing
- Check output path: `artifacts/bin/Release/`
- Verify `.csproj` OutputPath setting
- Try clean build: `rm -rf artifacts/bin && ./build.sh`

---

## Packaging for Thunderstore

```bash
# After building successfully
tcli build

# Output: artifacts/thunderstore/PEAKCompetitive.zip
```

This creates a `.zip` with:
- `plugins/PEAKCompetitive.dll`
- `manifest.json`
- `README.md`
- `CHANGELOG.md`
- `icon.png`

---

## Development Tips

### Watch Mode (Auto-rebuild on changes)
```bash
dotnet watch --project src/PEAKCompetitive/PEAKCompetitive.csproj build
```

### Clean Build
```bash
rm -rf artifacts/bin obj
./build.sh
```

### Check DLL Dependencies
```bash
# macOS/Linux
otool -L artifacts/bin/Release/PEAKCompetitive.dll

# Or use a .NET tool
dotnet tool install -g dotnet-references
dotnet-references artifacts/bin/Release/PEAKCompetitive.dll
```

### Test Without PEAK
You can verify it compiles without installing PEAK:
1. Comment out game references in `.csproj`
2. Build to check C# syntax errors
3. Uncomment references for final build

---

## CI/CD (GitHub Actions)

Create `.github/workflows/build.yml`:

```yaml
name: Build Mod

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore src/PEAKCompetitive/PEAKCompetitive.csproj
      - run: dotnet build src/PEAKCompetitive/PEAKCompetitive.csproj -c Release
      - uses: actions/upload-artifact@v3
        with:
          name: PEAKCompetitive-DLL
          path: artifacts/bin/Release/PEAKCompetitive.dll
```

This auto-builds on every commit!

---

## Quick Reference

| Command | Purpose |
|---------|---------|
| `./build.sh` | Build Release DLL |
| `dotnet build -c Debug` | Build Debug version |
| `dotnet clean` | Clean build artifacts |
| `dotnet restore` | Restore NuGet packages |
| `tcli build` | Package for Thunderstore |

---

Happy modding! 🏔️
