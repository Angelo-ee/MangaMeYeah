# Build script to generate all release packages for MangaMeYeah v0.4.2
# 1. Standalone single-file exe (MangaMeYeah-v0.4.2-win-x64-standalone.exe)
# 2. Portable ZIP (MangaMeYeah-v0.4.2-win-x64-portable.zip)
# 3. Windows Installer (MangaMeYeah-v0.4.2-Setup.exe)
# 4. Source code ZIP (MangaMeYeah-v0.4.2-source.zip)
# Also writes SHA256SUMS.txt.
#
# 注意: docs/ 下含日文文件名，Compress-Archive 在不同 PowerShell 版本下可能
#       以本地代码页写入 ZIP 条目名；发布前请核对包内非 ASCII 文件名。

param (
    [string]$Version = "0.4.2",
    [string]$PublishDir = "publish\win-x64"
)

$ErrorActionPreference = "Stop"
$rootDir = $PSScriptRoot
Set-Location $rootDir

Write-Host "=== Building MangaMeYeah v$Version Release Packages ===" -ForegroundColor Cyan

$releaseDir = Join-Path $rootDir "release"
if (-not (Test-Path $releaseDir)) {
    New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null
}
$publishAbs = Join-Path $rootDir $PublishDir

# 1. dotnet publish single-file win-x64
Write-Host "[1/5] Publishing single-file win-x64 binary..." -ForegroundColor Yellow
dotnet publish "$rootDir\src\MangaMeYeah\MangaMeYeah.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o $publishAbs
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed." }

# 2. Standalone single-file exe
Write-Host "[2/5] Copying standalone single-file exe..." -ForegroundColor Yellow
$standaloneExe = Join-Path $releaseDir "MangaMeYeah-v$Version-win-x64-standalone.exe"
Copy-Item "$publishAbs\MangaMeYeah.exe" -Destination $standaloneExe -Force
Write-Host " -> Created: $standaloneExe" -ForegroundColor Green

# 3. Portable ZIP
Write-Host "[3/5] Creating Portable ZIP..." -ForegroundColor Yellow
$portableTemp = Join-Path $rootDir "temp_portable\MangaMeYeah"
if (Test-Path $portableTemp) { Remove-Item -Recurse -Force $portableTemp }
New-Item -ItemType Directory -Path $portableTemp -Force | Out-Null

Copy-Item "$publishAbs\MangaMeYeah.exe" -Destination $portableTemp
Copy-Item "$rootDir\README.md" -Destination $portableTemp
Copy-Item "$rootDir\LICENSE" -Destination $portableTemp
Copy-Item "$rootDir\HISTORY.md" -Destination $portableTemp
Copy-Item "$rootDir\docs" -Destination $portableTemp -Recurse

$portableZip = Join-Path $releaseDir "MangaMeYeah-v$Version-win-x64-portable.zip"
if (Test-Path $portableZip) { Remove-Item -Force $portableZip }
Compress-Archive -Path $portableTemp -DestinationPath $portableZip -CompressionLevel Optimal
Remove-Item -Recurse -Force "$rootDir\temp_portable"
Write-Host " -> Created: $portableZip ($([Math]::Round((Get-Item $portableZip).Length / 1MB, 2)) MB)" -ForegroundColor Green

# 4. Compile Windows Installer using Inno Setup
Write-Host "[4/5] Compiling Inno Setup Windows Installer..." -ForegroundColor Yellow
$isccPaths = @(
    (Join-Path $rootDir "out\innosetup\app\ISCC.exe"),
    "C:\Users\emmma\AppData\Local\Programs\Inno Setup 6\ISCC.exe",
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe"
)
$iscc = $isccPaths | Where-Object { Test-Path $_ } | Select-Object -First 1
if ($iscc) {
    & $iscc "/DMyPublishDir=$PublishDir" "$rootDir\installer\MangaMeYeah.iss"
    $installerExe = Join-Path $releaseDir "MangaMeYeah-v$Version-Setup.exe"
    Write-Host " -> Created: $installerExe ($([Math]::Round((Get-Item $installerExe).Length / 1MB, 2)) MB)" -ForegroundColor Green
} else {
    Write-Warning "ISCC.exe not found. Inno Setup installer skipped."
}

# 5. Create Source code ZIP from Git HEAD
Write-Host "[5/5] Creating Source code ZIP from Git HEAD..." -ForegroundColor Yellow
$sourceZip = Join-Path $releaseDir "MangaMeYeah-v$Version-source.zip"
if (Test-Path $sourceZip) { Remove-Item -Force $sourceZip }
git archive --format=zip --prefix="MangaMeYeah-$Version/" HEAD -o $sourceZip
Write-Host " -> Created: $sourceZip ($([Math]::Round((Get-Item $sourceZip).Length / 1KB, 2)) KB)" -ForegroundColor Green

# 6. Write SHA256SUMS.txt (UTF-8 without BOM, lowercase hashes, git sha256sum style)
Write-Host "[6/6] Writing SHA256SUMS.txt..." -ForegroundColor Yellow
$assets = @(
    "MangaMeYeah-v$Version-Setup.exe",
    "MangaMeYeah-v$Version-win-x64-portable.zip",
    "MangaMeYeah-v$Version-win-x64-standalone.exe",
    "MangaMeYeah-v$Version-source.zip"
)
$lines = foreach ($asset in $assets) {
    $p = Join-Path $releaseDir $asset
    if (Test-Path $p) {
        "{0}  {1}" -f (Get-FileHash $p -Algorithm SHA256).Hash.ToLowerInvariant(), $asset
    }
}
[System.IO.File]::WriteAllLines(
    (Join-Path $releaseDir "SHA256SUMS.txt"),
    $lines,
    (New-Object System.Text.UTF8Encoding($false)))

Write-Host "`nAll release packages ready in $releaseDir :" -ForegroundColor Cyan
Get-ChildItem $releaseDir | Format-Table Name, Length, LastWriteTime
