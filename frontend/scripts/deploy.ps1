# Flutter Frontend Deployment Script for KingHost
# Based on the working deployment instructions

param(
    [string]$FtpPassword = $null
)

# Colors for output
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }

# Get FTP password if not provided
if (-not $FtpPassword) {
    $SecurePassword = Read-Host "Enter FTP password for eutonafila@ftp.eutonafila.com.br" -AsSecureString
    $FtpPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecurePassword))
}

try {
    Write-Info "Starting Flutter frontend deployment to KingHost..."
    
    # Step 1: Build Flutter web app
    Write-Info "Step 1: Building Flutter web app..."
    Set-Location -Path $PSScriptRoot\..
    
    if (-not (Test-Path "pubspec.yaml")) {
        Write-Error "pubspec.yaml not found. Make sure you're in the Flutter project root."
        exit 1
    }
    
    # Check if WASM build is supported and preferred
    Write-Info "Building Flutter web app with WASM support for Edge browser..."
    $buildResult = flutter build web --wasm --release
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "WASM build failed, falling back to standard build..."
        $buildResult = flutter build web --release
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Flutter build failed!"
            exit 1
        }
    }
    Write-Success "Flutter build completed successfully!"
    
    # Step 2: Prepare the www folder
    Write-Info "Step 2: Preparing www folder..."
    
    # Remove old www folder if it exists
    if (Test-Path "www") {
        Remove-Item -Recurse -Force "www"
        Write-Info "Removed old www folder"
    }
    
    # Remove old www.zip if it exists
    if (Test-Path "www.zip") {
        Remove-Item -Force "www.zip"
        Write-Info "Removed old www.zip"
    }
    
    # Rename build/web to www
    if (-not (Test-Path "build/web")) {
        Write-Error "build/web folder not found. Build may have failed."
        exit 1
    }
    
    Move-Item "build/web" "www"
    Write-Success "Renamed build/web to www"
    
    # Step 3: Create ZIP file
    Write-Info "Step 3: Creating www.zip..."
    Compress-Archive -Path "www" -DestinationPath "www.zip" -Force
    Write-Success "Created www.zip successfully!"
    
    # Step 4: Upload to KingHost FTP
    Write-Info "Step 4: Uploading to KingHost FTP..."
    
    # Create WinSCP script
    $winscpScript = @"
open ftp://eutonafila:$FtpPassword@ftp.eutonafila.com.br/
option batch abort
option confirm off
put www.zip /
close
exit
"@
    
    $scriptPath = "temp_upload_script.txt"
    $winscpScript | Out-File -FilePath $scriptPath -Encoding UTF8
    
    # Use WinSCP to upload
    $winscpPath = Join-Path $PSScriptRoot "WinSCP.com"
    if (-not (Test-Path $winscpPath)) {
        Write-Error "WinSCP.com not found at $winscpPath"
        exit 1
    }
    
    & $winscpPath /script=$scriptPath
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "FTP upload failed!"
        exit 1
    }
    
    # Clean up temp script
    Remove-Item $scriptPath -Force
    Write-Success "www.zip uploaded successfully to FTP root!"
    
    # Step 5: Instructions for server-side unzip
    Write-Info "Step 5: Server-side unzip instructions..."
    Write-Warning "IMPORTANT: You need to manually unzip the file on the server:"
    Write-Warning "1. Go to http://webftp.kinghost.com.br/"
    Write-Warning "2. Log in with your credentials"
    Write-Warning "3. You should see www.zip in the root directory"
    Write-Warning "4. Click on www.zip and select 'Descompactar' (Unzip)"
    Write-Warning "5. This will extract the contents into the /www directory"
    Write-Warning "6. Visit https://www.eutonafila.com.br/ to confirm deployment"
    
    Write-Success "Deployment preparation completed successfully!"
    Write-Info "Your site will be live after you unzip the file on the server."
    
} catch {
    Write-Error "Deployment failed: $($_.Exception.Message)"
    exit 1
} finally {
    # Clean up temp files
    if (Test-Path "temp_upload_script.txt") {
        Remove-Item "temp_upload_script.txt" -Force
    }
    
    # Restore build/web folder for local development
    if (Test-Path "www") {
        if (Test-Path "build") {
            Move-Item "www" "build/web"
            Write-Info "Restored build/web folder for local development"
        }
    }
} 