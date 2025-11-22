# PEAK Competitive - Development Guide

This document contains the PEAK modding guide for reference during development.

See the complete guide that was generated from analyzing PEAK Unlimited in the parent directory.

## Quick Reference

### Building
```bash
dotnet build -c Release
```

### Testing
1. Build the project
2. Copy `artifacts/bin/Release/PEAKCompetitive.dll` to `PEAK/BepInEx/plugins/`
3. Launch PEAK
4. Check `BepInEx/LogOutput.log` for errors

### Packaging
```bash
tcli build
```

### Key Concepts

- **Harmony Patches**: Modify game behavior by intercepting methods
- **BepInEx Config**: User-configurable settings
- **Photon Networking**: Only host needs mod for multiplayer
- **Master Client**: Use `PhotonNetwork.IsMasterClient` for world modifications

### Common Patch Pattern

```csharp
[HarmonyPatch(typeof(ClassName), "MethodName")]
public class MyPatch
{
    static void Postfix(ClassName __instance)
    {
        // Your code here
    }
}
```

### Spawning Network Objects

```csharp
if (!PhotonNetwork.IsMasterClient) return;

PhotonNetwork.Instantiate(
    "PrefabName",
    position,
    rotation
);
```
