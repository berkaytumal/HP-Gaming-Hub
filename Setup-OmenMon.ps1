# OmenMon Setup Script for HP Gaming Hub
# This script downloads and sets up OmenMon.exe for integration

param(
    [string]$InstallPath = "$env:ProgramFiles\OmenMon",
    [switch]$Force
)

# Check if running as Administrator
function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Download latest OmenMon release
function Get-LatestOmenMonRelease {
    try {
        Write-Host "Fetching latest OmenMon release information..." -ForegroundColor Yellow
        
        $apiUrl = "https://api.github.com/repos/OmenMon/OmenMon/releases/latest"
        $response = Invoke-RestMethod -Uri $apiUrl -Method Get
        
        # Look for the main executable
        $asset = $response.assets | Where-Object { $_.name -eq "OmenMon.exe" }
        
        if (-not $asset) {
            # If no direct exe, look for zip file
            $asset = $response.assets | Where-Object { $_.name -like "*OmenMon*.zip" }
        }
        
        if (-not $asset) {
            throw "Could not find OmenMon executable in latest release"
        }
        
        return @{
            DownloadUrl = $asset.browser_download_url
            Version = $response.tag_name
            FileName = $asset.name
        }
    }
    catch {
        throw "Failed to get release information: $($_.Exception.Message)"
    }
}

# Download file
function Download-File {
    param(
        [string]$Url,
        [string]$OutputPath
    )
    
    try {
        Write-Host "Downloading from: $Url" -ForegroundColor Yellow
        Write-Host "Saving to: $OutputPath" -ForegroundColor Yellow
        
        $webClient = New-Object System.Net.WebClient
        $webClient.DownloadFile($Url, $OutputPath)
        
        Write-Host "Download completed successfully!" -ForegroundColor Green
    }
    catch {
        throw "Download failed: $($_.Exception.Message)"
    }
}

# Extract if ZIP file
function Extract-Archive {
    param(
        [string]$ArchivePath,
        [string]$DestinationPath
    )
    
    try {
        Write-Host "Extracting archive..." -ForegroundColor Yellow
        Expand-Archive -Path $ArchivePath -DestinationPath $DestinationPath -Force
        
        # Find the OmenMon.exe in extracted files
        $exePath = Get-ChildItem -Path $DestinationPath -Name "OmenMon.exe" -Recurse | Select-Object -First 1
        
        if ($exePath) {
            $fullExePath = Join-Path $DestinationPath $exePath
            $targetPath = Join-Path $InstallPath "OmenMon.exe"
            
            Copy-Item -Path $fullExePath -Destination $targetPath -Force
            Write-Host "Extracted OmenMon.exe to: $targetPath" -ForegroundColor Green
        }
        else {
            throw "OmenMon.exe not found in extracted archive"
        }
    }
    catch {
        throw "Extraction failed: $($_.Exception.Message)"
    }
}

# Test OmenMon installation
function Test-OmenMonInstallation {
    param([string]$ExePath)
    
    try {
        Write-Host "Testing OmenMon installation..." -ForegroundColor Yellow
        
        $process = Start-Process -FilePath $ExePath -ArgumentList "-?" -Wait -PassThru -WindowStyle Hidden
        
        if ($process.ExitCode -eq 0) {
            Write-Host "OmenMon.exe is working correctly!" -ForegroundColor Green
            return $true
        }
        else {
            Write-Warning "OmenMon.exe test failed with exit code: $($process.ExitCode)"
            return $false
        }
    }
    catch {
        Write-Warning "Failed to test OmenMon: $($_.Exception.Message)"
        return $false
    }
}

# Main setup function
function Install-OmenMon {
    Write-Host "=== OmenMon Setup for HP Gaming Hub ===" -ForegroundColor Cyan
    Write-Host ""
    
    # Check if administrator
    if (-not (Test-Administrator)) {
        Write-Warning "This script should be run as Administrator for best results."
        Write-Host "Some features may not work without elevated privileges." -ForegroundColor Yellow
        Write-Host ""
    }
    
    # Check if already installed
    $omenMonPath = Join-Path $InstallPath "OmenMon.exe"
    
    if ((Test-Path $omenMonPath) -and -not $Force) {
        Write-Host "OmenMon.exe already exists at: $omenMonPath" -ForegroundColor Green
        
        if (Test-OmenMonInstallation -ExePath $omenMonPath) {
            Write-Host "Installation appears to be working. Use -Force to reinstall." -ForegroundColor Green
            return
        }
        else {
            Write-Warning "Existing installation is not working properly. Reinstalling..."
        }
    }
    
    try {
        # Create install directory
        if (-not (Test-Path $InstallPath)) {
            Write-Host "Creating installation directory: $InstallPath" -ForegroundColor Yellow
            New-Item -Path $InstallPath -ItemType Directory -Force | Out-Null
        }
        
        # Get latest release info
        $release = Get-LatestOmenMonRelease
        Write-Host "Found OmenMon version: $($release.Version)" -ForegroundColor Green
        
        # Download file
        $downloadPath = Join-Path $env:TEMP $release.FileName
        Download-File -Url $release.DownloadUrl -OutputPath $downloadPath
        
        # Handle different file types
        if ($release.FileName -like "*.zip") {
            # Extract ZIP file
            $extractPath = Join-Path $env:TEMP "OmenMon_Extract"
            Extract-Archive -ArchivePath $downloadPath -DestinationPath $extractPath
        }
        else {
            # Direct executable
            Copy-Item -Path $downloadPath -Destination $omenMonPath -Force
            Write-Host "Copied OmenMon.exe to: $omenMonPath" -ForegroundColor Green
        }
        
        # Test installation
        if (Test-OmenMonInstallation -ExePath $omenMonPath) {
            Write-Host ""
            Write-Host "=== Installation Successful! ===" -ForegroundColor Green
            Write-Host "OmenMon.exe is installed at: $omenMonPath" -ForegroundColor Green
            Write-Host ""
            Write-Host "You can now run HP Gaming Hub and it will detect OmenMon automatically." -ForegroundColor Cyan
            Write-Host ""
            Write-Host "Note: Some features may require running HP Gaming Hub as Administrator." -ForegroundColor Yellow
        }
        else {
            Write-Error "Installation completed but OmenMon.exe is not working properly."
            Write-Host "Please check if your device is supported and try running as Administrator." -ForegroundColor Yellow
        }
        
        # Cleanup
        if (Test-Path $downloadPath) {
            Remove-Item $downloadPath -Force
        }
    }
    catch {
        Write-Error "Installation failed: $($_.Exception.Message)"
        Write-Host ""
        Write-Host "Manual installation steps:" -ForegroundColor Yellow
        Write-Host "1. Download OmenMon.exe from: https://github.com/OmenMon/OmenMon/releases/latest" -ForegroundColor Yellow
        Write-Host "2. Place it in: $InstallPath" -ForegroundColor Yellow
        Write-Host "3. Run HP Gaming Hub to test the integration" -ForegroundColor Yellow
    }
}

# Run the installation
Install-OmenMon

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")