# Flutter App Deployment Script for KingHost (PowerShell)
# Deploys the Flutter web build to www.eutonafila.com.br
# Usage: .\scripts\deploy_to_kinghost_improved.ps1

# --- Configuration ---
$FtpHost          = "ftp.eutonafila.com.br"
$FtpPort          = "21"
$RemoteDir        = "/www" # Kinghost web root
$LocalBuildDir    = "build\web"
$Domain           = "eutonafila.com.br"
$FtpUsername      = "eutonafila"
# WARNING: Storing your password in plaintext is a security risk.
# For better security, consider using environment variables or a secrets management tool.
$FtpPasswordPlain = "EJR827hfffhfjsjsiior"
# --- End of Configuration ---

# Ensure the script is run from the project root
if (-not (Test-Path "pubspec.yaml")) {
    Write-Error "This script must be run from the root of your Flutter project."
    Write-Error "Please 'cd' to your project directory and run '.\scripts\deploy_to_kinghost_improved.ps1'"
    exit 1
}

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
    
    $flutterPath = "C:\sdk\flutter\bin\flutter.bat"
    
    try {
        if (Test-Path $flutterPath) {
            $flutterResult = & $flutterPath --version 2>&1
            if ($LASTEXITCODE -eq 0) {
                $flutterVersion = $flutterResult | Select-Object -First 1
                Write-Status "Flutter version: $flutterVersion"
                return $true
            } else {
                Write-Error "Flutter is not working properly. See error details below:"
                Write-Error $flutterResult
                Write-Error "Please check Flutter installation: https://docs.flutter.dev/get-started/install"
                return $false
            }
        } else {
            Write-Error "Flutter not found at: $flutterPath"
            Write-Error "Please install Flutter: https://docs.flutter.dev/get-started/install"
            return $false
        }
    } catch {
        Write-Error "Flutter is not installed or not working"
        Write-Error "Please install Flutter: https://docs.flutter.dev/get-started/install"
        return $false
    }
}

# Function to verify DNS resolution
function Test-DnsResolution {
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
    Write-Status "Testing FTP connection..."
    
    $ftpScript = @"
open $FtpHost $FtpPort
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
    Write-Status "Building Flutter web app..."
    
    $flutterPath = "C:\sdk\flutter\bin\flutter.bat"
    
    # Clean previous build
    Write-Status "Cleaning previous build..."
    & $flutterPath clean
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Flutter clean failed!"
        return $false
    }
    
    # Build for web
    Write-Status "Building for web release..."
    & $flutterPath build web --release
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
open $FtpHost $FtpPort
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
    Remove-Item "test_ftp.txt" -Force -ErrorAction SilentlyContinue
}

# Main deployment function
function Start-Deployment {
    Write-Header
    
    # Step 1: Verify Flutter installation
    if (-not (Test-FlutterInstallation)) {
        return $false
    }
    
    # Step 2: Verify DNS resolution
    Test-DnsResolution
    
    # Step 3: Test FTP connection
    if (-not (Test-FtpConnection)) {
        return $false
    }
    
    # Step 4: Build Flutter app
    if (-not (Build-FlutterApp)) {
        return $false
    }
    
    # Step 5: Create FTP script
    New-FtpScript
    
    # Step 6: Upload files
    if (-not (Upload-Files)) {
        Remove-TemporaryFiles
        return $false
    }
    
    # Step 7: Cleanup
    Remove-TemporaryFiles
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "   Deployment completed successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Status "Your app should be available at: https://$Domain"
    Write-Status "Please wait a few minutes for changes to propagate."
    
    return $true
}

# Execute deployment
try {
    Start-Deployment
} catch {
    Write-Error "Deployment interrupted: $($_.Exception.Message)"
    Remove-TemporaryFiles
    exit 1
} 