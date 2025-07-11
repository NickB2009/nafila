# Manual Deployment Instructions for Flutter Frontend (KingHost)

## 1. Build the Flutter Web App

Open a terminal in your project root and run:

```
flutter build web
```

This will generate the production-ready files in `build/web`.

**Next:**
- Rename the `build/web` folder to `www` (so the output is now `build/www`)
- Zip the `www` folder:
  - Right-click the `www` folder and choose "Send to > Compressed (zipped) folder" (or use your preferred zip tool)
  - This creates `www.zip`

---

## 2. Upload Files to KingHost via WebFTP

1. Go to [http://webftp.kinghost.com.br/](http://webftp.kinghost.com.br/)
2. Log in with:
   - **Host:** ftp.eutonafila.com.br
   - **User:** eutonafila
   - **Password:** (see your credentials)
3. Navigate to the `/` (root) directory.
4. On your computer, open the folder: `C:\Users\romme\source\repos\nafila\frontend\build`
5. Upload the zipped `www.zip` file:
   - Use the "Enviar Arquivo para seu FTP" (Upload File) button to upload `www.zip`.
6. Unzip on the server:
   - Use the "Descompactar" (Unzip) option in WebFTP
   - KingHost will extract the contents into the `/www` directory, replacing all files and folders with your new build
7. After upload and extraction, your `/www` directory should contain:
   - `index.html`, `main.dart.js`, `flutter.js`, `manifest.json`, `favicon.png`, etc.
   - Folders: `assets`, `canvaskit`, `icons`, etc.

---

## 3. Test Your Deployment

- Visit [https://www.eutonafila.com.br/](https://www.eutonafila.com.br/)
- Your Flutter web app should load.
- If you see a directory listing or an old site, make sure you overwrote all files and that `index.html` is present in `/www`.

---

## 4. Notes
- You do **not** need to delete `/www` before each upload, but it can help avoid old files lingering.
- If you want to automate this in the future, consider using WinSCP CLI or FileZilla for bulk uploads.
- For SPA routing, ensure your `.htaccess` is correct (see KingHost docs if needed).

---
**This method is fast, reliable, and ensures all files and folders are placed correctly.** 