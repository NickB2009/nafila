# KingHost Deployment Scripts for Flutter Frontend

This directory contains improved deployment scripts for deploying your Flutter web app to KingHost via FTP.

## Available Scripts

### 1. Interactive PowerShell Script (Recommended)
**File:** `deploy_to_kinghost_improved.ps1`
- Prompts for FTP credentials interactively
- Includes comprehensive error handling
- Colored output for better readability
- Tests connection before deployment

**Usage:**
```powershell
.\deploy_to_kinghost_improved.ps1
```

### 2. Configuration-Based PowerShell Script
**File:** `deploy_to_kinghost_with_config.ps1`
- Reads credentials from configuration file
- Supports automated deployments
- Can be used with CI/CD pipelines

**Usage:**
```powershell
# Use configuration file (default: deploy_config.json)
.\deploy_to_kinghost_with_config.ps1 -UseConfigCredentials

# Use interactive mode
.\deploy_to_kinghost_with_config.ps1 -Interactive

# Use custom configuration file
.\deploy_to_kinghost_with_config.ps1 -ConfigFile "my_config.json" -UseConfigCredentials
```

### 3. Windows Batch Script
**File:** `deploy_to_kinghost_improved.bat`
- Compatible with Windows Command Prompt
- Interactive credential input
- Basic error handling

**Usage:**
```cmd
deploy_to_kinghost_improved.bat
```

### 4. Bash Script (Linux/macOS)
**File:** `deploy_to_kinghost_improved.sh`
- For Linux and macOS systems
- Comprehensive error handling
- Colored output

**Usage:**
```bash
./deploy_to_kinghost_improved.sh
```

## Configuration File

**File:** `deploy_config.json`

Update this file with your KingHost FTP credentials:

```json
{
  "ftp": {
    "host": "ftp.eutonafila.com.br",
    "port": 21,
    "remote_dir": "/public_html",
    "domain": "eutonafila.com.br"
  },
  "build": {
    "local_build_dir": "build/web",
    "clean_before_build": true
  },
  "credentials": {
    "username": "YOUR_FTP_USERNAME_HERE",
    "password": "YOUR_FTP_PASSWORD_HERE"
  }
}
```

## Prerequisites

1. **Flutter SDK** installed and in PATH
2. **FTP client** (built into Windows, available on Linux/macOS)
3. **KingHost FTP credentials**:
   - Host: `ftp.eutonafila.com.br`
   - Port: `21`
   - Username: Your KingHost FTP username
   - Password: Your KingHost FTP password
   - Directory: `/public_html`

## Getting Your KingHost FTP Credentials

1. Log into your KingHost control panel
2. Navigate to FTP Management
3. Either use the default FTP account or create a new one
4. Note down the username, password, and home directory

## Deployment Process

The scripts perform the following steps:

1. **Verify Flutter Installation** - Checks if Flutter is available
2. **DNS Resolution Test** - Verifies the FTP host is reachable
3. **Get Credentials** - Prompts for or reads FTP credentials
4. **Test FTP Connection** - Verifies credentials work
5. **Build Flutter App** - Runs `flutter build web --release`
6. **Upload Files** - Uploads all files from `build/web/` to `/public_html/`
7. **Cleanup** - Removes temporary files

## Security Notes

- **Never commit credentials** to version control
- The configuration file (`deploy_config.json`) should be added to `.gitignore`
- Use interactive mode for one-time deployments
- Use configuration file for automated deployments (store securely)

## Troubleshooting

### Common Issues

1. **"Flutter is not installed"**
   - Install Flutter: https://docs.flutter.dev/get-started/install
   - Add Flutter to your PATH

2. **"FTP connection failed"**
   - Verify your FTP credentials
   - Check if the FTP host is correct
   - Ensure your KingHost account is active

3. **"Build failed"**
   - Run `flutter doctor` to check for issues
   - Try `flutter clean` before building
   - Check for compilation errors in your Flutter code

4. **"Upload failed"**
   - Verify the remote directory exists (`/public_html`)
   - Check disk space on the server
   - Ensure FTP permissions are correct

### Manual FTP Test

Test your FTP connection manually:

```bash
ftp ftp.eutonafila.com.br
# Enter your username and password when prompted
# Type 'pwd' to see current directory
# Type 'bye' to exit
```

## File Structure After Deployment

Your deployed app will have this structure on KingHost:

```
/public_html/
├── index.html          # Main entry point
├── main.dart.js        # Compiled Dart code
├── flutter.js          # Flutter runtime
├── assets/             # App assets
├── icons/              # App icons
└── manifest.json       # PWA manifest
```

## Post-Deployment

After successful deployment:

1. **Test the app** at https://eutonafila.com.br
2. **Check browser console** for any errors
3. **Verify PWA features** work correctly
4. **Test on mobile devices**

## Automation

For automated deployments (CI/CD):

1. Use the configuration-based script
2. Store credentials securely (environment variables, secrets)
3. Add deployment to your build pipeline

Example GitHub Actions workflow:

```yaml
- name: Deploy to KingHost
  run: |
    echo '{"ftp":{"host":"ftp.eutonafila.com.br","port":21,"remote_dir":"/public_html","domain":"eutonafila.com.br"},"build":{"local_build_dir":"build/web","clean_before_build":true},"credentials":{"username":"${{ secrets.FTP_USERNAME }}","password":"${{ secrets.FTP_PASSWORD }}"}}' > deploy_config.json
    .\deploy_to_kinghost_with_config.ps1 -UseConfigCredentials
```

## Support

- **KingHost Support**: https://king.host/suporte
- **Flutter Web Documentation**: https://docs.flutter.dev/web
- **FTP Troubleshooting**: Check your hosting provider's documentation 