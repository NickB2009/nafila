# Flutter App Deployment Script for KingHost (PowerShell) with Config File
# Usage: .\deploy_to_kinghost_with_config.ps1

param(
    [string]$ConfigFile = "deploy_config.json",
    [switch]$UseConfigCredentials,
    [switch]$Interactive
)

# Function to write colored output
function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-Header {
    Write-Host "========================================" -ForegroundColor Blue
    Write-Host "   Deploying Flutter App to KingHost" -ForegroundColor Blue
    Write-Host "========================================" -ForegroundColor Blue
    Write-Host ""
}

# Function to load configuration
function Load-Configuration {
    param([string]$ConfigPath)
    
    if (-not (Test-Path $ConfigPath)) {
        Write-Error "Configuration file not found: $ConfigPath"
        Write-Status "Please create $ConfigPath with your FTP credentials"
        return $null
    }
    
    try {
        $config = Get-Content $ConfigPath | ConvertFrom-Json
        Write-Status "Configuration loaded from $ConfigPath"
        return $config
    } catch {
        Write-Error "Failed to parse configuration file: $($_.Exception.Message)"
        return $null
    }
}

# Function to get credentials
function Get-FtpCredentials {
    param($Config)
    
    if ($Interactive -or -not $UseConfigCredentials) {
        Write-Status "Please enter your KingHost FTP credentials:"
        $script:FtpUsername = Read-Host "FTP Username"
        $script:FtpPassword = Read-Host "FTP Password" -AsSecureString
        $script:FtpPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($script:FtpPassword))
    } else {
        if ($Config.credentials.username -eq "YOUR_FTP_USERNAME_HERE" -or $Config.credentials.password -eq "YOUR_FTP_PASSWORD_HERE") {
            Write-Error "Please update the credentials in $ConfigFile"
            return $false
        }
        
        $script:FtpUsername = $Config.credentials.username
        $script:FtpPasswordPlain = $Config.credentials.password
    }
    
    if (-not $script:FtpUsername -or -not $script:FtpPasswordPlain) {
        Write-Error "Username and password are required"
        return $false
    }
    
    return $true
}

# Function to verify Flutter installation
function Test-FlutterInstallation {
    Write-Status "Verifying Flutter installation..."
    
    try {
        $flutterVersion = flutter --version 2>$null | Select-Object -First 1
        if ($LASTEXITCODE -eq 0) {
            Write-Status "Flutter version: $flutterVersion"
            return $true
        } else {
            Write-Error "Flutter is not installed or not in PATH"
            Write-Error "Please install Flutter: https://docs.flutter.dev/get-started/install"
            return $false
        }
    } catch {
        Write-Error "Flutter is not installed or not in PATH"
        Write-Error "Please install Flutter: https://docs.flutter.dev/get-started/install"
        return $false
    }
}

# Function to verify DNS resolution
function Test-DnsResolution {
    param([string]$FtpHost)
    Write-Status "Verifying DNS resolution for $FtpHost..."
    
    try {
        $dnsResult = Resolve-DnsName -Name $FtpHost -ErrorAction Stop | Select-Object -First 1
        Write-Status "DNS resolved: $FtpHost -> $($dnsResult.IPAddress)"
        return $true
    } catch {
        Write-Warning "DNS resolution failed for $FtpHost, but continuing..."
        return $false
    }
}

# Function to test FTP connection
function Test-FtpConnection {
    param([string]$FtpHost, [string]$Port)
    Write-Status "Testing FTP connection..."
    
    $ftpScript = @"
open $FtpHost $Port
user $FtpUsername $FtpPasswordPlain
pwd
bye
"@
    
    $ftpScript | Out-File -FilePath "test_ftp.txt" -Encoding ASCII
    
    try {
        Get-Content "test_ftp.txt" | ftp -inv | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Status "FTP connection test successful"
            Remove-Item "test_ftp.txt" -Force
            return $true
        } else {
            Write-Error "FTP connection test failed"
            Remove-Item "test_ftp.txt" -Force -ErrorAction SilentlyContinue
            return $false
        }
    } catch {
        Write-Error "FTP connection test failed"
        Remove-Item "test_ftp.txt" -Force -ErrorAction SilentlyContinue
        return $false
    }
}

# Function to build Flutter app
function Build-FlutterApp {
    param([string]$LocalBuildDir, [bool]$CleanBeforeBuild)
    Write-Status "Building Flutter web app..."
    
    if ($CleanBeforeBuild) {
        Write-Status "Cleaning previous build..."
        flutter clean
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Flutter clean failed!"
            return $false
        }
    }
    
    Write-Status "Building for web release..."
    flutter build web --release
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Flutter build failed!"
        return $false
    }
    
    if (-not (Test-Path $LocalBuildDir)) {
        Write-Error "Build directory $LocalBuildDir not found!"
        return $false
    }
    
    Write-Status "Build completed successfully!"
    Write-Status "Build files ready in $LocalBuildDir"
    return $true
}

# Function to create FTP script
function New-FtpScript {
    param([string]$FtpHost, [string]$Port, [string]$RemoteDir, [string]$LocalBuildDir)
    Write-Status "Creating FTP upload script..."
    
    $ftpScript = @"
open $FtpHost $Port
user $FtpUsername $FtpPasswordPlain
binary
cd $RemoteDir
lcd $LocalBuildDir
mput *
bye
"@
    
    $ftpScript | Out-File -FilePath "ftpscript.txt" -Encoding ASCII
    Write-Status "FTP script created"
}

# Function to upload files
function Upload-Files {
    param([string]$FtpHost)
    Write-Status "Starting FTP upload to $FtpHost..."
    
    try {
        Get-Content "ftpscript.txt" | ftp -inv
        if ($LASTEXITCODE -eq 0) {
            Write-Status "Upload completed successfully!"
            return $true
        } else {
            Write-Error "FTP upload failed with exit code $LASTEXITCODE"
            return $false
        }
    } catch {
        Write-Error "FTP upload failed: $($_.Exception.Message)"
        return $false
    }
}

# Function to cleanup
function Remove-TemporaryFiles {
    Write-Status "Cleaning up temporary files..."
    Remove-Item "ftpscript.txt" -Force -ErrorAction SilentlyContinue
}

# Main deployment function
function Start-Deployment {
    Write-Header
    
    # Load configuration
    $config = Load-Configuration -ConfigPath $ConfigFile
    if (-not $config) {
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    # Verify Flutter installation
    if (-not (Test-FlutterInstallation)) {
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    # Verify DNS
    Test-DnsResolution -FtpHost $config.ftp.host | Out-Null
    
    # Get credentials
    if (-not (Get-FtpCredentials -Config $config)) {
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    # Test connection
    if (-not (Test-FtpConnection -FtpHost $config.ftp.host -Port $config.ftp.port)) {
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    # Build the app
    if (-not (Build-FlutterApp -LocalBuildDir $config.build.local_build_dir -CleanBeforeBuild $config.build.clean_before_build)) {
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    # Create FTP script
    New-FtpScript -FtpHost $config.ftp.host -Port $config.ftp.port -RemoteDir $config.ftp.remote_dir -LocalBuildDir $config.build.local_build_dir
    
    # Upload files
    if (-not (Upload-Files -FtpHost $config.ftp.host)) {
        Remove-TemporaryFiles
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    # Cleanup
    Remove-TemporaryFiles
    
    # Success message
    Write-Host ""
    Write-Status "========================================"
    Write-Status "   Deployment completed successfully!"
    Write-Status "========================================"
    Write-Host ""
    Write-Status "Your app should be available at: https://$($config.ftp.domain)"
    Write-Status "Please allow a few minutes for changes to propagate."
    Write-Host ""
}

# Handle script interruption
trap {
    Write-Error "Deployment interrupted: $($_.Exception.Message)"
    Remove-TemporaryFiles
    Read-Host "Press Enter to exit"
    exit 1
}

# Run main deployment function
Start-Deployment 