# PEAK Competitive - Quick Install Script
# Builds and installs the mod to your PEAK game folder

param(
    [string]$PEAKPath = "C:\Program Files (x86)\Steam\steamapps\common\PEAK"
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  PEAK Competitive Installer" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if PEAK is installed
if (-not (Test-Path $PEAKPath)) {
    Write-Host "Error: PEAK not found at: $PEAKPath" -ForegroundColor Red
    Write-Host ""
    Write-Host "If PEAK is installed elsewhere, run:" -ForegroundColor Yellow
    Write-Host '  .\install.ps1 -PEAKPath "D:\Games\Steam\steamapps\common\PEAK"' -ForegroundColor Gray
    exit 1
}

Write-Host "PEAK found at: $PEAKPath" -ForegroundColor Green
Write-Host ""

# Check if BepInEx is installed
if (-not (Test-Path "$PEAKPath\BepInEx")) {
    Write-Host "Warning: BepInEx not found!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "You need to install BepInEx first:" -ForegroundColor Yellow
    Write-Host "  1. Download: https://github.com/BepInEx/BepInEx/releases" -ForegroundColor Gray
    Write-Host "  2. Get BepInEx_x64_5.4.23.2.zip (or latest 5.x)" -ForegroundColor Gray
    Write-Host "  3. Extract to PEAK folder" -ForegroundColor Gray
    Write-Host "  4. Launch PEAK once to initialize BepInEx" -ForegroundColor Gray
    Write-Host "  5. Run this script again" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host "BepInEx found!" -ForegroundColor Green
Write-Host ""

# Build the mod
Write-Host "Building PEAK Competitive..." -ForegroundColor White
try {
    dotnet build src/PEAKCompetitive/PEAKCompetitive.csproj -c Release --nologo -v minimal
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
} catch {
    Write-Host ""
    Write-Host "Build failed!" -ForegroundColor Red
    Write-Host "Make sure you have .NET SDK 8.0+ installed" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Build successful!" -ForegroundColor Green
Write-Host ""

# Create plugin folder
$PluginPath = "$PEAKPath\BepInEx\plugins\PEAKCompetitive"
Write-Host "Creating plugin folder..." -ForegroundColor White
New-Item -ItemType Directory -Path $PluginPath -Force | Out-Null

# Copy DLL
Write-Host "Installing mod..." -ForegroundColor White
$DllPath = "artifacts\bin\Release\PEAKCompetitive.dll"
if (-not (Test-Path $DllPath)) {
    Write-Host "Error: Built DLL not found at: $DllPath" -ForegroundColor Red
    exit 1
}

Copy-Item $DllPath "$PluginPath\PEAKCompetitive.dll" -Force

$DllSize = (Get-Item "$PluginPath\PEAKCompetitive.dll").Length / 1KB

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "  Installation Complete!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Installed to:" -ForegroundColor Cyan
Write-Host "  $PluginPath\PEAKCompetitive.dll" -ForegroundColor Gray
Write-Host "Size: $([math]::Round($DllSize, 1)) KB" -ForegroundColor Gray
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Launch PEAK from Steam" -ForegroundColor Gray
Write-Host "  2. Press F3 to open competitive settings" -ForegroundColor Gray
Write-Host "  3. Host multiplayer match to test" -ForegroundColor Gray
Write-Host ""
Write-Host "Config file will be created at:" -ForegroundColor Cyan
Write-Host "  $PEAKPath\BepInEx\config\PEAKCompetitive.cfg" -ForegroundColor Gray
Write-Host ""

# Offer to open plugin folder
$OpenFolder = Read-Host "Open plugin folder? (Y/n)"
if ($OpenFolder -ne 'n') {
    explorer.exe $PluginPath
}
