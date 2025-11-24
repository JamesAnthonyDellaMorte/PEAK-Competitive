# Package PEAK Competitive for Thunderstore
# Creates a .zip file ready to upload to Thunderstore

param(
    [string]$Version = "1.0.0"
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Thunderstore Package Builder" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Build the mod
Write-Host "Building mod..." -ForegroundColor White
dotnet build src/PEAKCompetitive/PEAKCompetitive.csproj -c Release --nologo -v minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "Build successful!" -ForegroundColor Green
Write-Host ""

# Create package directory structure
$PackageDir = "thunderstore-package"
$PluginDir = "$PackageDir/plugins"

Write-Host "Creating package structure..." -ForegroundColor White
Remove-Item -Path $PackageDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $PluginDir -Force | Out-Null

# Copy files to package
Write-Host "Copying files..." -ForegroundColor White

# Copy DLL to plugins folder
Copy-Item "artifacts/bin/Release/PEAKCompetitive.dll" "$PluginDir/PEAKCompetitive.dll"

# Copy Thunderstore metadata files
Copy-Item "thunderstore/manifest.json" "$PackageDir/manifest.json"
Copy-Item "thunderstore/README.md" "$PackageDir/README.md"
Copy-Item "thunderstore/CHANGELOG.md" "$PackageDir/CHANGELOG.md"

# Check for icon
if (Test-Path "thunderstore/icon.png") {
    Copy-Item "thunderstore/icon.png" "$PackageDir/icon.png"
} else {
    Write-Host "Warning: icon.png not found in thunderstore/" -ForegroundColor Yellow
    Write-Host "  You'll need to add a 256x256 PNG icon before uploading" -ForegroundColor Yellow
}

# Update version in manifest
Write-Host "Updating version to $Version..." -ForegroundColor White
$ManifestPath = "$PackageDir/manifest.json"
$Manifest = Get-Content $ManifestPath -Raw | ConvertFrom-Json
$Manifest.version_number = $Version
$Manifest | ConvertTo-Json -Depth 10 | Set-Content $ManifestPath

# Create ZIP
$ZipName = "PEAKCompetitive-$Version.zip"
Write-Host "Creating archive..." -ForegroundColor White
Compress-Archive -Path "$PackageDir/*" -DestinationPath $ZipName -Force

# Calculate size
$ZipSize = (Get-Item $ZipName).Length / 1KB

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "  Package Created!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Package: $ZipName" -ForegroundColor Cyan
Write-Host "Size: $([math]::Round($ZipSize, 2)) KB" -ForegroundColor Cyan
Write-Host ""

# Verify package contents
Write-Host "Package contents:" -ForegroundColor Yellow
Get-ChildItem -Path $PackageDir -Recurse | ForEach-Object {
    $RelativePath = $_.FullName.Replace((Get-Item $PackageDir).FullName, "").TrimStart('\')
    Write-Host "  $RelativePath" -ForegroundColor Gray
}
Write-Host ""

# Check for icon
if (-not (Test-Path "$PackageDir/icon.png")) {
    Write-Host "⚠️  MISSING: icon.png (256x256 PNG required)" -ForegroundColor Red
} else {
    Write-Host "✅ icon.png included" -ForegroundColor Green
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Test the mod locally first" -ForegroundColor Gray
Write-Host "  2. Create icon.png (256x256) if missing" -ForegroundColor Gray
Write-Host "  3. Upload $ZipName to https://thunderstore.io" -ForegroundColor Gray
Write-Host "  4. Fill in Thunderstore submission form" -ForegroundColor Gray
Write-Host ""

# Cleanup temp directory
Remove-Item -Path $PackageDir -Recurse -Force

# Open folder
$OpenFolder = Read-Host "Open folder with package? (Y/n)"
if ($OpenFolder -ne 'n') {
    explorer.exe /select,$ZipName
}
