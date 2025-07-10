# Flutter Frontend Deployment Script

This script automates the deployment of the Flutter frontend to KingHost based on the working deployment instructions.

## Prerequisites

- Flutter SDK installed
- PowerShell (Windows)
- WinSCP.com (included in this directory)
- KingHost FTP credentials

## Usage

1. Open PowerShell as Administrator
2. Navigate to the frontend directory
3. Run the deployment script:

```powershell
.\scripts\deploy.ps1
```

Or provide the FTP password as a parameter:

```powershell
.\scripts\deploy.ps1 -FtpPassword "your_password_here"
```

## What the script does

1. **Builds the Flutter web app** - Runs `flutter build web`
2. **Prepares the www folder** - Renames `build/web` to `www`
3. **Creates ZIP file** - Compresses the `www` folder into `www.zip`
4. **Uploads to FTP** - Uses WinSCP to upload `www.zip` to the FTP root
5. **Provides unzip instructions** - Shows you how to unzip on the server

## Manual step required

After the script completes, you need to:

1. Go to [http://webftp.kinghost.com.br/](http://webftp.kinghost.com.br/)
2. Log in with your credentials
3. Find `www.zip` in the root directory
4. Click on it and select "Descompactar" (Unzip)
5. Visit [https://www.eutonafila.com.br/](https://www.eutonafila.com.br/) to confirm deployment

## Files

- `deploy.ps1` - Main deployment script
- `WinSCP.com` - Command-line FTP client for uploads
- `README.md` - This documentation file

## Notes

- The script automatically restores your `build/web` folder for local development
- Old `www` and `www.zip` files are cleaned up before each deployment
- The script uses secure password input if not provided as parameter 