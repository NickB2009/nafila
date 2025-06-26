# Flutter App Deployment Script for KingHost (PowerShell)
# Usage: .\deploy_to_kinghost_improved.ps1

param(
    [string]$FtpUsername,
    [string]$FtpPassword
)

# Configuration
$Host = "ftp.eutonafila.com.br"
$Port = "21"
$RemoteDir = "/public_html"
$LocalBuildDir = "build\web"
$Domain = "eutonafila.com.br"

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
    Write-Status "Verifying DNS resolution for $Host..."
    
    try {
        $dnsResult = Resolve-DnsName -Name $Host -ErrorAction Stop | Select-Object -First 1
        Write-Status "DNS resolved: $Host -> $($dnsResult.IPAddress)"
        return $true
    } catch {
        Write-Warning "DNS resolution failed for $Host, but continuing..."
        return $false
    }
}

# Function to get FTP credentials
function Get-FtpCredentials {
    if (-not $FtpUsername -or -not $FtpPassword) {
        Write-Status "Please enter your KingHost FTP credentials:"
        $script:FtpUsername = Read-Host "FTP Username"
        $script:FtpPassword = Read-Host "FTP Password" -AsSecureString
        $script:FtpPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($script:FtpPassword))
    } else {
        $script:FtpPasswordPlain = $FtpPassword
    }
    
    if (-not $script:FtpUsername -or -not $script:FtpPasswordPlain) {
        Write-Error "Username and password are required"
        return $false
    }
    
    return $true
}

# Function to test FTP connection
function Test-FtpConnection {
    Write-Status "Testing FTP connection..."
    
    $ftpScript = @"
open $Host $Port
user $FtpUsername $FtpPasswordPlain
pwd
bye
"@
    
    $ftpScript | Out-File -FilePath "test_ftp.txt" -Encoding ASCII
    
    try {
        ftp -inv < test_ftp.txt | Out-Null
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
    Write-Status "Building Flutter web app..."
    
    # Clean previous build
    Write-Status "Cleaning previous build..."
    flutter clean
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Flutter clean failed!"
        return $false
    }
    
    # Build for web
    Write-Status "Building for web release..."
    flutter build web --release
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Flutter build failed!"
        return $false
    }
    
    # Verify build directory exists
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
    Write-Status "Creating FTP upload script..."
    
    $ftpScript = @"
open $Host $Port
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
    Write-Status "Starting FTP upload to $Host..."
    
    try {
        ftp -inv < ftpscript.txt
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
    
    # Verify Flutter installation
    if (-not (Test-FlutterInstallation)) {
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    # Verify DNS
    Test-DnsResolution | Out-Null
    
    # Get credentials
    if (-not (Get-FtpCredentials)) {
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    # Test connection
    if (-not (Test-FtpConnection)) {
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    # Build the app
    if (-not (Build-FlutterApp)) {
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    # Create FTP script
    New-FtpScript
    
    # Upload files
    if (-not (Upload-Files)) {
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
    Write-Status "Your app should be available at: https://$Domain"
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