# Deployment Guide - Flutter App to KingHost

## Quick Deployment

### Option 1: PowerShell Script (Recommended)
```powershell
.\deploy_to_kinghost.ps1
```

### Option 2: Batch Script
```cmd
deploy_to_kinghost.bat
```

### Option 3: Manual Deployment

#### Step 1: Build the App
```bash
flutter build web --release
```

#### Step 2: Navigate to Build Directory
```bash
cd build/web
```

#### Step 3: Connect via FTP
```bash
ftp ftp.eutonafila.com.br
```

#### Step 4: Login and Upload
```
user YOUR_FTP_USERNAME YOUR_FTP_PASSWORD
binary
cd public_html
mput *
bye
```

## KingHost Configuration

### FTP Details
- **Host:** `ftp.eutonafila.com.br`
- **Username:** Your KingHost FTP username
- **Password:** Your KingHost FTP password
- **Directory:** `public_html`

### Domain Configuration
- **Domain:** `eutonafila.com.br`
- **URL:** `https://eutonafila.com.br`

## Important Notes

### File Structure
The build creates these files in `build/web/`:
- `index.html` - Main entry point
- `main.dart.js` - Compiled Dart code
- `flutter.js` - Flutter runtime
- `assets/` - App assets
- `icons/` - App icons
- `manifest.json` - PWA manifest

### Browser Compatibility
- Modern browsers (Chrome, Firefox, Safari, Edge)
- Mobile browsers supported
- Progressive Web App (PWA) features included

### Performance
- Tree-shaking enabled for smaller bundle size
- Assets optimized for web delivery
- Service worker for offline functionality

## Troubleshooting

### Common Issues

1. **Build Fails**
   - Ensure Flutter is up to date: `flutter upgrade`
   - Clean build: `flutter clean && flutter build web --release`

2. **FTP Upload Fails**
   - Verify FTP credentials
   - Check if `public_html` directory exists
   - Ensure sufficient disk space on server

3. **App Not Loading**
   - Check browser console for errors
   - Verify all files uploaded correctly
   - Check server error logs

### Support
- KingHost Support: https://king.host/suporte
- Flutter Web Documentation: https://docs.flutter.dev/web

## Security Notes

- Never commit FTP credentials to version control
- Use secure FTP (SFTP) if available
- Regularly update Flutter and dependencies
- Monitor for security vulnerabilities

## Next Steps

After deployment:
1. Test the app thoroughly
2. Set up monitoring and analytics
3. Configure SSL certificate (if not already done)
4. Set up backup procedures 