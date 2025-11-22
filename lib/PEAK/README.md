# PEAK Game DLLs

## How to Get These Files

### From Windows PC (Recommended)
1. Install PEAK via Steam on Windows
2. Navigate to: `C:\Program Files (x86)\Steam\steamapps\common\PEAK\PEAK_Data\Managed\`
3. Copy ALL `.dll` files from that folder
4. Place them in this directory (`lib/PEAK/`)

### What You Need
Copy the entire `Managed` folder contents, including:
- `Assembly-CSharp.dll` (PEAK's main game code) **REQUIRED**
- `PhotonRealtime.dll` (Networking)
- `PhotonUnityNetworking.dll` (Networking)
- `UnityEngine.dll` (Unity engine)
- `UnityEngine.*.dll` (Unity modules - CoreModule, UI, Physics, etc.)
- All other DLLs

### File List
After copying, you should have approximately 100-200 DLL files here.

Key files needed:
```
lib/PEAK/
├── Assembly-CSharp.dll          ← PEAK game code (MOST IMPORTANT)
├── PhotonRealtime.dll           ← Networking
├── PhotonUnityNetworking.dll    ← Networking
├── UnityEngine.dll              ← Unity engine
├── UnityEngine.CoreModule.dll   ← Unity core
├── UnityEngine.UI.dll           ← Unity UI
├── UnityEngine.IMGUIModule.dll  ← Unity GUI
├── UnityEngine.PhysicsModule.dll ← Physics
├── Unity.TextMeshPro.dll        ← Text rendering
└── ... (many more)
```

## Alternative: Download from Game Install

If you have access to Windows but not on the same machine:
1. Use Remote Desktop or file share
2. ZIP the entire `Managed` folder
3. Transfer to Mac
4. Extract to `lib/PEAK/`

## .gitignore Note

These DLLs are **NOT** committed to git (they're in `.gitignore`).
Each developer needs their own copy from the game installation.

This is standard practice for game mods to avoid distributing copyrighted game code.
