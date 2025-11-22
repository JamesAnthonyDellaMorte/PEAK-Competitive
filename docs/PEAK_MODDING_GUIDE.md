# PEAK Modding Guide - Understanding PEAK Unlimited

> A comprehensive guide to understanding how to create mods for PEAK, based on the PEAK Unlimited mod architecture.

---

## Table of Contents

1. [What is PEAK and How Do Mods Work?](#what-is-peak-and-how-do-mods-work)
2. [Required Frameworks and Tools](#required-frameworks-and-tools)
3. [Project Structure](#project-structure)
4. [Core Concepts](#core-concepts)
5. [Harmony Patching System](#harmony-patching-system)
6. [Configuration Management](#configuration-management)
7. [Networking Considerations](#networking-considerations)
8. [Building and Deploying](#building-and-deploying)
9. [Common Patterns](#common-patterns)
10. [Code Examples](#code-examples)

---

## What is PEAK and How Do Mods Work?

### About PEAK
- PEAK is an indie co-op climbing/mountaineering game built in Unity
- Originally supports up to 4 players
- Uses **Photon PUN 2** for networking
- Uses **Steamworks** for lobby management
- Built on Unity 2021+

### Modding Architecture
PEAK uses **BepInEx** as its modding framework:
- **BepInEx** is a plugin loader that hooks into Unity game startup
- Mods are loaded as plugins before the game initializes
- Uses **HarmonyLib** to patch game methods at runtime
- Allows modifying game behavior without changing game files
- Non-destructive: original game files remain untouched

### How Mods Execute
1. Game launches
2. BepInEx loads first
3. BepInEx scans `BepInEx/plugins/` for mod DLLs
4. Each mod's `Awake()` method is called
5. Mods apply Harmony patches to game methods
6. Game runs with modified behavior

---

## Required Frameworks and Tools

### Essential Dependencies

#### 1. BepInEx (Plugin Framework)
- **Version**: 5.4.21+
- **Purpose**: Loads and manages mods
- **Installation**: Extract to game root directory
- **Creates folder structure**:
  ```
  PEAK/
  ├── BepInEx/
  │   ├── core/
  │   ├── plugins/      <- Your mod goes here
  │   └── config/       <- Auto-generated configs
  ```

#### 2. HarmonyLib (Runtime Patching)
- **Included with**: BepInEx
- **Purpose**: Intercept and modify game methods
- **Types of patches**:
  - **Prefix**: Runs before original method (can prevent execution)
  - **Postfix**: Runs after original method (can modify results)
  - **Transpiler**: Modifies IL code (advanced)

#### 3. Game References
Required DLL references from PEAK installation:
```xml
<Reference Include="Assembly-CSharp">
  <HintPath>$(ManagedDir)\Assembly-CSharp.dll</HintPath>
  <Private>False</Private>
</Reference>
<Reference Include="Photon.Pun">
  <HintPath>$(ManagedDir)\PhotonUnity.dll</HintPath>
</Reference>
<Reference Include="UnityEngine">
  <HintPath>$(ManagedDir)\UnityEngine.dll</HintPath>
</Reference>
<!-- Additional Unity and game DLLs -->
```

#### 4. Development Tools
- **Visual Studio 2022** or **Rider**
- **.NET Standard 2.1 SDK** (Unity compatible)
- **BepInEx.AssemblyPublicizer** (to access internal game types)
- **tcli** (Thunderstore CLI for packaging)

---

## Project Structure

### Recommended Folder Organization

```
YourMod/
├── src/YourMod/
│   ├── Configuration/              # Config handling and UI
│   │   ├── ConfigurationHandler.cs
│   │   └── ModConfigurationUI.cs
│   ├── Patches/                    # Harmony patches
│   │   ├── ExamplePatch.cs
│   │   └── AnotherPatch.cs
│   ├── Util/                       # Helper utilities
│   │   └── Logger.cs
│   ├── Model/                      # Data models
│   ├── Plugin.cs                   # Main entry point
│   ├── YourMod.csproj             # Project file
│   └── thunderstore.toml          # Build config
├── manifest.json                   # Mod metadata
├── README.md                       # User documentation
├── CHANGELOG.md                    # Version history
├── icon.png                        # 256x256 thumbnail
└── YourMod.sln                    # Visual Studio solution
```

### Key Files Explained

#### manifest.json (Mod Metadata)
```json
{
    "name": "YourMod",
    "version_number": "1.0.0",
    "website_url": "https://github.com/you/YourMod",
    "description": "What your mod does",
    "dependencies": ["BepInEx-BepInExPack_PEAK-5.4.2403"]
}
```

#### YourMod.csproj (Build Configuration)
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <AssemblyName>YourMod</AssemblyName>
    <RootNamespace>YourMod</RootNamespace>
    <ManagedDir>C:\Program Files\Steam\steamapps\common\PEAK\PEAK_Data\Managed</ManagedDir>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="BepInEx.Core" Version="5.4.21" />
    <PackageReference Include="BepInEx.PluginInfoProps" Version="2.1.0" />
    <PackageReference Include="HarmonyX" Version="2.10.1" />
  </ItemGroup>
</Project>
```

#### thunderstore.toml (Publishing Configuration)
```toml
[package]
namespace = "YourName"
name = "YourMod"
versionFile = "manifest.json"

[package.dependencies]
BepInEx-BepInExPack_PEAK = "5.4.2403"

[build]
icon = "icon.png"
readme = "README.md"
outdir = "artifacts/thunderstore"

[[build.copy]]
source = "manifest.json"
target = "manifest.json"

[[build.copy]]
source = "CHANGELOG.md"
target = "CHANGELOG.md"

[[build.copy]]
source = "artifacts/bin/YourMod/release/YourMod.dll"
target = "plugins/YourMod.dll"

[publish]
communities = ["peak"]
categories = ["mods"]
```

---

## Core Concepts

### 1. Plugin Entry Point

Every mod needs a main plugin class:

```csharp
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace YourMod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        private Harmony _harmony;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            // Apply all patches
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            _harmony.PatchAll();

            Logger.LogInfo("All patches applied successfully!");
        }
    }
}
```

**Key Points**:
- Inherits from `BaseUnityPlugin`
- Decorated with `[BepInPlugin]` attribute
- `Awake()` is called when mod loads
- `Harmony.PatchAll()` applies all patches in assembly

### 2. Configuration System

BepInEx provides built-in configuration:

```csharp
using BepInEx.Configuration;

public class ConfigurationHandler
{
    private static ConfigFile _config;

    public static int MaxPlayers { get; private set; }
    public static bool EnableFeature { get; private set; }
    public static float SpawnChance { get; private set; }

    public static void Initialize(ConfigFile config)
    {
        _config = config;

        // Bind configuration entries
        var maxPlayersEntry = config.Bind(
            "General",                      // Section
            "MaxPlayers",                   // Key
            20,                             // Default value
            "Maximum number of players (1-30)"  // Description
        );
        MaxPlayers = Mathf.Clamp(maxPlayersEntry.Value, 1, 30);

        var enableFeatureEntry = config.Bind(
            "Features",
            "EnableFeature",
            true,
            "Enable the custom feature"
        );
        EnableFeature = enableFeatureEntry.Value;

        var spawnChanceEntry = config.Bind(
            "Features",
            "SpawnChance",
            0.5f,
            new ConfigDescription(
                "Chance to spawn item (0-1)",
                new AcceptableValueRange<float>(0f, 1f)
            )
        );
        SpawnChance = spawnChanceEntry.Value;
    }
}
```

**Generated Config File** (`BepInEx/config/YourMod.cfg`):
```ini
[General]

## Maximum number of players (1-30)
# Setting type: Int32
# Default value: 20
MaxPlayers = 20

[Features]

## Enable the custom feature
# Setting type: Boolean
# Default value: true
EnableFeature = true

## Chance to spawn item (0-1)
# Setting type: Single
# Default value: 0.5
# Acceptable value range: From 0 to 1
SpawnChance = 0.5
```

### 3. Logging System

```csharp
public static class ModLogger
{
    private static ManualLogSource _logger;

    public static void Initialize(ManualLogSource logger)
    {
        _logger = logger;
    }

    public static void Info(string message)
    {
        _logger?.LogInfo(message);
    }

    public static void Warning(string message)
    {
        _logger?.LogWarning(message);
    }

    public static void Error(string message)
    {
        _logger?.LogError(message);
    }

    public static void Debug(string message)
    {
        _logger?.LogDebug(message);
    }
}
```

Logs appear in `BepInEx/LogOutput.log`

---

## Harmony Patching System

### Understanding Patches

Harmony allows you to modify game behavior by intercepting method calls.

### Patch Types

#### 1. Prefix Patch (Before Method)

```csharp
using HarmonyLib;

[HarmonyPatch(typeof(NetworkingUtilities), nameof(NetworkingUtilities.HostRoomOptions))]
public class NetworkingPatch
{
    // Prefix can prevent original method from running
    // Return false to skip original, true to continue
    static bool Prefix(ref RoomOptions __result)
    {
        // Create custom room options
        __result = new RoomOptions
        {
            MaxPlayers = ConfigurationHandler.MaxPlayers,
            IsVisible = true,
            IsOpen = true
        };

        Plugin.Logger.LogInfo($"Room created with max {__result.MaxPlayers} players");

        // Return false to skip original method
        return false;
    }
}
```

**Use Cases**:
- Override method completely
- Add validation before method runs
- Conditionally prevent execution

#### 2. Postfix Patch (After Method)

```csharp
[HarmonyPatch(typeof(Campfire), "Awake")]
public class CampfireAwakePatch
{
    // Postfix runs after original method
    // Can access and modify results
    static void Postfix(Campfire __instance)
    {
        // __instance is the object instance (this)
        if (!PhotonNetwork.IsMasterClient) return;

        Plugin.Logger.LogInfo($"Campfire awakened: {__instance.name}");

        // Spawn extra items around campfire
        SpawnExtraItems(__instance);
    }

    private static void SpawnExtraItems(Campfire campfire)
    {
        // Your custom logic here
    }
}
```

**Use Cases**:
- Add behavior after original method
- Modify results
- Track when methods are called

#### 3. Multiple Patches on Same Method

```csharp
[HarmonyPatch(typeof(PlayerConnectionLog), "OnPlayerEnteredRoom")]
public class PlayerJoinPatch
{
    // Priority: Higher = runs first (default is 400)
    [HarmonyPriority(Priority.High)]
    static void Prefix(Player newPlayer)
    {
        Plugin.Logger.LogInfo($"Player joining: {newPlayer.NickName}");
    }

    [HarmonyPriority(Priority.Low)]
    static void Postfix(Player newPlayer)
    {
        Plugin.Logger.LogInfo($"Player joined: {newPlayer.NickName}");

        // Respawn items for late joiner
        if (ConfigurationHandler.LateJoinSpawns)
        {
            RespawnItems();
        }
    }
}
```

### Special Harmony Parameters

```csharp
// __instance: The object instance (this)
// __result: The return value (can modify in postfix)
// __args: Array of all arguments
// Specific parameters by name: just use parameter name
```

Example:
```csharp
[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
public class SomePatch
{
    static void Postfix(
        SomeClass __instance,        // The instance
        ref int __result,            // Return value (can modify)
        string paramName,            // Original parameter
        int anotherParam)            // Another original parameter
    {
        Plugin.Logger.LogInfo($"Instance: {__instance}");
        Plugin.Logger.LogInfo($"Original result: {__result}");
        Plugin.Logger.LogInfo($"Params: {paramName}, {anotherParam}");

        // Modify return value
        __result = 999;
    }
}
```

### Finding Methods to Patch

Use tools like:
- **dnSpy** (decompiler for .NET assemblies)
- **ILSpy** (alternative decompiler)
- **Unity Explorer** (runtime inspector mod)

Steps:
1. Open `Assembly-CSharp.dll` from PEAK installation
2. Search for relevant classes
3. Find methods that control behavior you want to modify
4. Note the class, method name, and parameters
5. Create Harmony patch

---

## Configuration Management

### In-Game Configuration UI

PEAK Unlimited provides an in-game menu (F2) for configuration:

```csharp
using UnityEngine;

public class ModConfigurationUI : MonoBehaviour
{
    private bool _showMenu = false;
    private Rect _windowRect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 300, 400, 600);

    private void Update()
    {
        // Toggle menu with F2
        if (Input.GetKeyDown(KeyCode.F2))
        {
            _showMenu = !_showMenu;
        }
    }

    private void OnGUI()
    {
        if (!_showMenu) return;

        // Create a window
        _windowRect = GUI.Window(0, _windowRect, DrawWindow, "Mod Configuration");
    }

    private void DrawWindow(int windowID)
    {
        GUILayout.BeginVertical();

        // Max Players slider
        GUILayout.Label($"Max Players: {ConfigurationHandler.MaxPlayers}");
        ConfigurationHandler.MaxPlayers = (int)GUILayout.HorizontalSlider(
            ConfigurationHandler.MaxPlayers, 1, 30
        );

        // Toggle
        ConfigurationHandler.EnableFeature = GUILayout.Toggle(
            ConfigurationHandler.EnableFeature,
            "Enable Feature"
        );

        // Button
        if (GUILayout.Button("Apply Settings"))
        {
            ApplySettings();
        }

        GUILayout.EndVertical();

        // Make window draggable
        GUI.DragWindow();
    }

    private void ApplySettings()
    {
        Plugin.Logger.LogInfo("Settings applied!");
        // Save to config file or apply changes
    }
}
```

Add to your Plugin.cs:
```csharp
private void Awake()
{
    // ... other initialization ...

    // Add UI component
    gameObject.AddComponent<ModConfigurationUI>();
}
```

---

## Networking Considerations

PEAK uses **Photon PUN 2** for multiplayer. Understanding networking is crucial.

### Host vs Client

```csharp
using Photon.Pun;

// Check if this player is the host
if (PhotonNetwork.IsMasterClient)
{
    // Only host should spawn items, modify world state
    SpawnItems();
}

// Get local player
Player localPlayer = PhotonNetwork.LocalPlayer;

// Get all players
Player[] allPlayers = PhotonNetwork.PlayerList;

// Current room
Room currentRoom = PhotonNetwork.CurrentRoom;
```

### Spawning Networked Objects

```csharp
using Photon.Pun;
using UnityEngine;

public static void SpawnItem(int itemId, Vector3 position, Quaternion rotation)
{
    // Only host spawns
    if (!PhotonNetwork.IsMasterClient) return;

    // Get item prefab name from game's item database
    string itemName = GetItemPrefabName(itemId);

    // Spawn over network (all clients see it)
    GameObject spawnedItem = PhotonNetwork.Instantiate(
        itemName,
        position,
        rotation
    );

    Plugin.Logger.LogInfo($"Spawned {itemName} at {position}");
}
```

### Synchronization Best Practices

1. **Only host modifies world state**
   - Host spawns items
   - Host manages game logic
   - Clients receive updates automatically

2. **Use PhotonNetwork.Instantiate for visible objects**
   - Ensures all players see the same objects
   - Photon handles synchronization

3. **Check IsMasterClient before modifications**
   ```csharp
   if (!PhotonNetwork.IsMasterClient) return;
   ```

4. **Room configuration must happen on host**
   ```csharp
   [HarmonyPatch(typeof(NetworkingUtilities), "HostRoomOptions")]
   public class RoomPatch
   {
       static bool Prefix(ref RoomOptions __result)
       {
           // Host creates room with custom options
           __result = new RoomOptions
           {
               MaxPlayers = ConfigurationHandler.MaxPlayers
           };
           return false;
       }
   }
   ```

### Host-Only Mod Pattern

PEAK Unlimited only requires the host to have the mod:

```csharp
// Host spawns items, clients see them via Photon
public static void SpawnMarshmallows(Campfire campfire)
{
    if (!PhotonNetwork.IsMasterClient)
    {
        // Non-hosts don't need the mod
        return;
    }

    // Host spawns items
    int extraPlayers = PhotonNetwork.CurrentRoom.MaxPlayers - 4;
    for (int i = 0; i < extraPlayers; i++)
    {
        Vector3 spawnPos = CalculateSpawnPosition(campfire, i);
        SpawnItem(46, spawnPos, Quaternion.identity); // Item 46 = marshmallow
    }
}
```

---

## Building and Deploying

### Build Process

1. **Compile the Project**
   ```bash
   dotnet build -c Release
   ```

   Output: `artifacts/bin/YourMod/release/YourMod.dll`

2. **Package for Distribution**

   Using Thunderstore CLI:
   ```bash
   tcli build
   ```

   This creates a `.zip` in `artifacts/thunderstore/` containing:
   ```
   YourMod.zip
   ├── plugins/
   │   └── YourMod.dll
   ├── manifest.json
   ├── README.md
   ├── CHANGELOG.md
   ├── LICENSE
   └── icon.png
   ```

3. **Manual Installation**

   Users extract the zip to:
   ```
   PEAK/BepInEx/plugins/YourMod/
   ```

### Publishing to Thunderstore

1. Create account at [thunderstore.io](https://thunderstore.io)
2. Use `tcli` to publish:
   ```bash
   tcli publish
   ```
3. Follow prompts to upload

### Version Management

Update versions in **two places**:

1. **manifest.json**
   ```json
   {
       "version_number": "1.0.1"
   }
   ```

2. **YourMod.csproj**
   ```xml
   <PropertyGroup>
       <Version>1.0.1</Version>
   </PropertyGroup>
   ```

---

## Common Patterns

### Pattern 1: Spawning Items Around Objects

```csharp
public static void SpawnItemsAroundObject(Vector3 center, float radius, int count, int itemId)
{
    if (!PhotonNetwork.IsMasterClient) return;

    for (int i = 0; i < count; i++)
    {
        // Calculate evenly spaced positions in circle
        float angle = (360f / count) * i;
        float radians = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(radians) * radius,
            0f,
            Mathf.Sin(radians) * radius
        );

        Vector3 spawnPos = center + offset;

        // Raycast to ground
        if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
        {
            spawnPos = hit.point + Vector3.up * 0.1f; // Slightly above ground
        }

        SpawnItem(itemId, spawnPos, Quaternion.identity);
    }
}
```

### Pattern 2: Singleton Logger

```csharp
public class ModLogger
{
    private static ModLogger _instance;
    private ManualLogSource _logger;

    private ModLogger(ManualLogSource logger)
    {
        _logger = logger;
    }

    public static ModLogger GetInstance(ManualLogSource logger = null)
    {
        if (_instance == null && logger != null)
        {
            _instance = new ModLogger(logger);
        }
        return _instance;
    }

    public void Log(string message)
    {
        _logger?.LogInfo(message);
    }
}

// Usage
ModLogger.GetInstance(Logger).Log("Hello from mod!");
```

### Pattern 3: Configuration with Validation

```csharp
public static class Config
{
    public static int MaxPlayers { get; private set; }

    public static void Initialize(ConfigFile config)
    {
        var entry = config.Bind("General", "MaxPlayers", 20);

        // Validate and clamp
        MaxPlayers = Mathf.Clamp(entry.Value, 1, 30);

        if (entry.Value != MaxPlayers)
        {
            Plugin.Logger.LogWarning($"MaxPlayers {entry.Value} out of range, clamped to {MaxPlayers}");
        }
    }
}
```

### Pattern 4: Conditional Patching

```csharp
[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
public class ConditionalPatch
{
    static bool Prefix()
    {
        // Only apply patch if feature is enabled
        if (!ConfigurationHandler.EnableFeature)
        {
            return true; // Run original method
        }

        // Custom logic
        DoCustomThing();

        return false; // Skip original method
    }
}
```

### Pattern 5: Dynamic Player Count Adjustments

```csharp
[HarmonyPatch(typeof(PlayerConnectionLog), "OnPlayerEnteredRoom")]
public class PlayerJoinPatch
{
    static void Postfix(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;

        Plugin.Logger.LogInfo($"Player joined: {newPlayer.NickName} ({currentPlayers} total)");

        // Respawn items based on new count
        RespawnItems(currentPlayers);
    }
}

[HarmonyPatch(typeof(PlayerConnectionLog), "OnPlayerLeftRoom")]
public class PlayerLeavePatch
{
    static void Postfix(Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;

        Plugin.Logger.LogInfo($"Player left: {otherPlayer.NickName} ({currentPlayers} remaining)");

        // Remove excess items
        RespawnItems(currentPlayers);
    }
}
```

---

## Code Examples

### Example 1: Simple Item Spawner Mod

```csharp
using BepInEx;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace SimpleSpawner
{
    [BepInPlugin("com.yourname.simplespawner", "Simple Spawner", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo("Simple Spawner loaded!");

            Harmony harmony = new Harmony("com.yourname.simplespawner");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Campfire), "Awake")]
    public class CampfireSpawnPatch
    {
        static void Postfix(Campfire __instance)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Plugin.Logger.LogInfo($"Spawning items at campfire: {__instance.name}");

            // Spawn 3 marshmallows in circle
            Vector3 center = __instance.transform.position;
            float radius = 2f;

            for (int i = 0; i < 3; i++)
            {
                float angle = (360f / 3) * i;
                float radians = angle * Mathf.Deg2Rad;

                Vector3 spawnPos = center + new Vector3(
                    Mathf.Cos(radians) * radius,
                    1f,
                    Mathf.Sin(radians) * radius
                );

                PhotonNetwork.Instantiate("Marshmallow", spawnPos, Quaternion.identity);
            }
        }
    }
}
```

### Example 2: Player Count Logger

```csharp
using BepInEx;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;

namespace PlayerCounter
{
    [BepInPlugin("com.yourname.playercounter", "Player Counter", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPatch(typeof(PlayerConnectionLog), "OnPlayerEnteredRoom")]
        [HarmonyPostfix]
        static void OnPlayerJoin(Player newPlayer)
        {
            int count = PhotonNetwork.CurrentRoom.PlayerCount;
            Logger.LogInfo($"Player joined: {newPlayer.NickName} | Total: {count}");
        }

        [HarmonyPatch(typeof(PlayerConnectionLog), "OnPlayerLeftRoom")]
        [HarmonyPostfix]
        static void OnPlayerLeave(Player otherPlayer)
        {
            int count = PhotonNetwork.CurrentRoom.PlayerCount;
            Logger.LogInfo($"Player left: {otherPlayer.NickName} | Total: {count}");
        }
    }
}
```

### Example 3: Custom Room Settings

```csharp
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Photon.Realtime;

namespace CustomRoomSettings
{
    [BepInPlugin("com.yourname.customroom", "Custom Room Settings", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private static ConfigEntry<int> _maxPlayers;
        private static ConfigEntry<bool> _isVisible;

        private void Awake()
        {
            // Bind config
            _maxPlayers = Config.Bind("Room", "MaxPlayers", 10, "Maximum players");
            _isVisible = Config.Bind("Room", "IsVisible", true, "Room visibility");

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPatch(typeof(NetworkingUtilities), "HostRoomOptions")]
        [HarmonyPrefix]
        static bool OverrideRoomOptions(ref RoomOptions __result)
        {
            __result = new RoomOptions
            {
                MaxPlayers = (byte)_maxPlayers.Value,
                IsVisible = _isVisible.Value,
                IsOpen = true
            };

            Logger.LogInfo($"Room created: {_maxPlayers.Value} max players, visible: {_isVisible.Value}");

            return false; // Skip original
        }
    }
}
```

---

## Additional Resources

### Documentation
- [BepInEx Documentation](https://docs.bepinex.dev/)
- [Harmony Documentation](https://harmony.pardeike.net/)
- [Photon PUN 2 Docs](https://doc.photonengine.com/pun/current/getting-started/pun-intro)

### Tools
- [dnSpy](https://github.com/dnSpy/dnSpy) - .NET debugger and assembly editor
- [ILSpy](https://github.com/icsharpcode/ILSpy) - .NET decompiler
- [Thunderstore CLI](https://github.com/thunderstore-io/thunderstore-cli) - Package manager
- [Unity Explorer](https://github.com/sinai-dev/UnityExplorer) - Runtime inspector

### Community
- [PEAK Modding Discord](https://discord.gg/peak) - Modding community
- [Thunderstore](https://thunderstore.io/c/peak/) - Mod repository

---

## Best Practices Checklist

- [ ] Only host/master client modifies world state
- [ ] Use `PhotonNetwork.Instantiate()` for networked objects
- [ ] Validate all configuration values
- [ ] Log important events for debugging
- [ ] Handle null references gracefully
- [ ] Test with multiple players
- [ ] Document configuration options
- [ ] Keep manifest.json and .csproj versions in sync
- [ ] Include clear README with installation instructions
- [ ] Test compatibility with other mods
- [ ] Use meaningful patch class names
- [ ] Avoid over-patching (only patch what you need)
- [ ] Clean up resources when mod unloads
- [ ] Provide helpful error messages

---

## Troubleshooting

### Common Issues

**Mod doesn't load**
- Check BepInEx is installed correctly
- Verify DLL is in `BepInEx/plugins/`
- Check `BepInEx/LogOutput.log` for errors
- Ensure .NET Standard 2.1 target

**Patches don't apply**
- Verify method names are correct
- Check if method is internal (use publicizer)
- Ensure namespace and class names match
- Check Harmony syntax

**Networking issues**
- Only master client should spawn items
- Use `PhotonNetwork.Instantiate()` not `Instantiate()`
- Check `PhotonNetwork.IsMasterClient` before modifications

**Config not saving**
- Ensure config is bound in `Awake()`
- Check file permissions on config folder
- Verify config file path is correct

---

## Conclusion

This guide provides a foundation for creating PEAK mods. The PEAK Unlimited mod demonstrates:

1. **Proper mod structure** with organized folders
2. **Harmony patching** to modify game behavior
3. **Configuration management** with validation
4. **Networking considerations** for multiplayer
5. **Build and deployment** workflows

Start small, test frequently, and join the modding community for support!

Happy modding!
