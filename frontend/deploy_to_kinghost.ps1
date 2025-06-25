# Deploy Flutter App to KingHost
Write-Host "========================================" -ForegroundColor Green
Write-Host "   Deploying Flutter App to KingHost" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# Build the Flutter web app first
Write-Host "Building Flutter web app..." -ForegroundColor Yellow
flutter build web --release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Flutter build failed!" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host ""

# Get FTP credentials
Write-Host "Please enter your KingHost FTP credentials:" -ForegroundColor Yellow
$ftpUsername = Read-Host "FTP Username"
$ftpPassword = Read-Host "FTP Password" -AsSecureString
$ftpPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($ftpPassword))

# Create FTP script
Write-Host "Creating FTP script..." -ForegroundColor Yellow
$ftpScript = @"
open ftp.eutonafila.com.br
user $ftpUsername $ftpPasswordPlain
binary
cd public_html
lcd build\web
mput *
bye
"@

$ftpScript | Out-File -FilePath "ftpscript.txt" -Encoding ASCII

Write-Host ""
Write-Host "Starting FTP upload..." -ForegroundColor Yellow
ftp -s:ftpscript.txt

# Clean up
Remove-Item "ftpscript.txt" -Force

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "   Deployment completed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Your app should be available at: https://eutonafila.com.br" -ForegroundColor Cyan
Write-Host ""
Read-Host "Press Enter to exit" 