#!/usr/bin/env nu
# PEAK Competitive - Nushell Install Script
# Cross-platform mod installer

def main [
    --peak-path (-p): string = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\PEAK" # Path to PEAK installation
] {
    print $"(ansi cyan_bold)=====================================
  PEAK Competitive Installer
=====================================(ansi reset)\n"

    # Check if PEAK is installed
    if not ($peak_path | path exists) {
        print $"(ansi red_bold)Error: PEAK not found at: ($peak_path)(ansi reset)\n"
        print $"(ansi yellow)If PEAK is installed elsewhere, run:(ansi reset)"
        print $"(ansi default_dimmed)  ./install.nu --peak-path \"D:\\Games\\Steam\\steamapps\\common\\PEAK\"(ansi reset)"
        exit 1
    }

    print $"(ansi green)PEAK found at: ($peak_path)(ansi reset)\n"

    # Check if BepInEx is installed
    let bepinex_path = $peak_path | path join "BepInEx"
    if not ($bepinex_path | path exists) {
        print $"(ansi yellow_bold)Warning: BepInEx not found!(ansi reset)\n"
        print $"(ansi yellow)You need to install BepInEx first:(ansi reset)"
        print $"(ansi default_dimmed)  1. Download: https://github.com/BepInEx/BepInEx/releases(ansi reset)"
        print $"(ansi default_dimmed)  2. Get BepInEx_x64_5.4.23.2.zip (or latest 5.x)(ansi reset)"
        print $"(ansi default_dimmed)  3. Extract to PEAK folder(ansi reset)"
        print $"(ansi default_dimmed)  4. Launch PEAK once to initialize BepInEx(ansi reset)"
        print $"(ansi default_dimmed)  5. Run this script again(ansi reset)\n"
        exit 1
    }

    print $"(ansi green)BepInEx found!(ansi reset)\n"

    # Build the mod
    print $"(ansi white)Building PEAK Competitive...(ansi reset)"
    try {
        dotnet build src/PEAKCompetitive/PEAKCompetitive.csproj -c Release --nologo -v minimal
        if $env.LAST_EXIT_CODE != 0 {
            error make {msg: "Build failed"}
        }
    } catch {
        print $"\n(ansi red_bold)Build failed!(ansi reset)"
        print $"(ansi yellow)Make sure you have .NET SDK 8.0+ installed(ansi reset)"
        exit 1
    }

    print $"\n(ansi green)Build successful!(ansi reset)\n"

    # Create plugin folder
    let plugin_path = $bepinex_path | path join "plugins" "PEAKCompetitive"
    print $"(ansi white)Creating plugin folder...(ansi reset)"
    mkdir $plugin_path

    # Copy DLL
    print $"(ansi white)Installing mod...(ansi reset)"
    let dll_source = "artifacts/bin/Release/PEAKCompetitive.dll"
    if not ($dll_source | path exists) {
        print $"(ansi red_bold)Error: Built DLL not found at: ($dll_source)(ansi reset)"
        exit 1
    }

    let dll_dest = $plugin_path | path join "PEAKCompetitive.dll"
    cp $dll_source $dll_dest

    let dll_size = (ls $dll_dest | get size | first) / 1kb | math round

    print $"\n(ansi green_bold)=====================================
  Installation Complete!
=====================================(ansi reset)\n"
    print $"(ansi cyan)Installed to:(ansi reset)"
    print $"(ansi default_dimmed)  ($dll_dest)(ansi reset)"
    print $"(ansi default_dimmed)Size: ($dll_size) KB(ansi reset)\n"
    print $"(ansi yellow)Next steps:(ansi reset)"
    print $"(ansi default_dimmed)  1. Launch PEAK from Steam(ansi reset)"
    print $"(ansi default_dimmed)  2. Press F3 to open competitive settings(ansi reset)"
    print $"(ansi default_dimmed)  3. Host multiplayer match to test(ansi reset)\n"
    print $"(ansi cyan)Config file will be created at:(ansi reset)"
    print $"(ansi default_dimmed)  ($bepinex_path | path join 'config' 'PEAKCompetitive.cfg')(ansi reset)\n"
}
