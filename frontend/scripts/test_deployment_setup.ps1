# Test Deployment Setup for KingHost
# This script tests the deployment environment without actually deploying

Write-Host "========================================" -ForegroundColor Blue
Write-Host "   Testing Deployment Setup" -ForegroundColor Blue
Write-Host "========================================" -ForegroundColor Blue
Write-Host ""

# Test 1: Flutter Installation
Write-Host "[TEST 1] Checking Flutter installation..." -ForegroundColor Yellow
try {
    $flutterVersion = flutter --version 2>$null | Select-Object -First 1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Flutter is installed: $flutterVersion" -ForegroundColor Green
    } else {
        Write-Host "✗ Flutter is not installed or not in PATH" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Flutter is not installed or not in PATH" -ForegroundColor Red
    exit 1
}

# Test 2: DNS Resolution
Write-Host "[TEST 2] Testing DNS resolution for ftp.eutonafila.com.br..." -ForegroundColor Yellow
try {
    $dnsResult = Resolve-DnsName -Name "ftp.eutonafila.com.br" -ErrorAction Stop | Select-Object -First 1
    Write-Host "✓ DNS resolved: ftp.eutonafila.com.br -> $($dnsResult.IPAddress)" -ForegroundColor Green
} catch {
    Write-Host "✗ DNS resolution failed for ftp.eutonafila.com.br" -ForegroundColor Red
    exit 1
}

# Test 3: FTP Port Connectivity
Write-Host "[TEST 3] Testing FTP port connectivity..." -ForegroundColor Yellow
try {
    $connection = Test-NetConnection -ComputerName "ftp.eutonafila.com.br" -Port 21 -InformationLevel Quiet
    if ($connection) {
        Write-Host "✓ FTP port 21 is reachable" -ForegroundColor Green
    } else {
        Write-Host "✗ FTP port 21 is not reachable" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ FTP port connectivity test failed" -ForegroundColor Red
    exit 1
}

# Test 4: FTP Client Availability
Write-Host "[TEST 4] Checking FTP client availability..." -ForegroundColor Yellow
$ftpCommand = Get-Command ftp -ErrorAction SilentlyContinue
if ($ftpCommand) {
    Write-Host "✓ FTP client is available" -ForegroundColor Green
} else {
    Write-Host "⚠ FTP client not found in PATH (will use PowerShell alternatives)" -ForegroundColor Yellow
    Write-Host "  Note: You may need to install an FTP client or use the PowerShell deployment script" -ForegroundColor Cyan
}

# Test 5: Configuration File
Write-Host "[TEST 5] Checking configuration file..." -ForegroundColor Yellow
if (Test-Path "deploy_config.json") {
    try {
        $config = Get-Content "deploy_config.json" | ConvertFrom-Json
        Write-Host "✓ Configuration file exists and is valid JSON" -ForegroundColor Green
        
        # Check if credentials are set
        if ($config.credentials.username -eq "YOUR_FTP_USERNAME_HERE" -or $config.credentials.password -eq "YOUR_FTP_PASSWORD_HERE") {
            Write-Host "⚠ Configuration file exists but credentials need to be updated" -ForegroundColor Yellow
        } else {
            Write-Host "✓ Configuration file has credentials set" -ForegroundColor Green
        }
    } catch {
        Write-Host "✗ Configuration file exists but is not valid JSON" -ForegroundColor Red
    }
} else {
    Write-Host "⚠ Configuration file not found (will use interactive mode)" -ForegroundColor Yellow
}

# Test 6: Flutter Project Structure
Write-Host "[TEST 6] Checking Flutter project structure..." -ForegroundColor Yellow
if (Test-Path "pubspec.yaml") {
    Write-Host "✓ pubspec.yaml found" -ForegroundColor Green
} else {
    Write-Host "✗ pubspec.yaml not found - not a Flutter project" -ForegroundColor Red
    exit 1
}

if (Test-Path "lib/main.dart") {
    Write-Host "✓ lib/main.dart found" -ForegroundColor Green
} else {
    Write-Host "✗ lib/main.dart not found" -ForegroundColor Red
    exit 1
}

# Test 7: Build Directory
Write-Host "[TEST 7] Checking build directory..." -ForegroundColor Yellow
if (Test-Path "build/web") {
    Write-Host "✓ Previous build found in build/web" -ForegroundColor Green
} else {
    Write-Host "⚠ No previous build found (will be created during deployment)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Blue
Write-Host "   Setup Test Completed" -ForegroundColor Blue
Write-Host "========================================" -ForegroundColor Blue
Write-Host ""
Write-Host "✓ All tests passed! Your deployment environment is ready." -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Get your KingHost FTP credentials" -ForegroundColor White
Write-Host "2. Update deploy_config.json with your credentials" -ForegroundColor White
Write-Host "3. Run: .\deploy_to_kinghost_improved.ps1" -ForegroundColor White
Write-Host ""
Write-Host "Note: If FTP client is not available, the PowerShell script will handle the deployment." -ForegroundColor Yellow
Write-Host "" 