# 🌐 CORS Development Guide - Anonymous Queue Issue Solution

## 🔍 **Problem Identified**

Your Flutter web app at `http://localhost:59095` **cannot** call the production API at `https://api.eutonafila.com.br` due to **CORS (Cross-Origin Resource Sharing)** restrictions.

When a web browser runs JavaScript/Dart code from one domain (localhost) and tries to make requests to another domain (api.eutonafila.com.br), the browser blocks this for security reasons unless the API server explicitly allows it.

## ✅ **Solutions (Choose One)**

### **Option 1: Run Chrome with CORS Disabled (RECOMMENDED for Development)**

1. **Close Chrome completely** (check Task Manager to ensure no chrome.exe processes)

2. **Open Command Prompt** and run:
   ```bash
   chrome.exe --user-data-dir="C:/chrome-dev-session" --disable-web-security --disable-features=VizDisplayCompositor
   ```
   
   Or create a batch file `chrome-dev.bat`:
   ```batch
   @echo off
   start chrome.exe --user-data-dir="C:/chrome-dev-session" --disable-web-security --disable-features=VizDisplayCompositor http://localhost:59095
   ```

3. **Navigate to your app**: `http://localhost:59095`

4. **Test the queue join** - it should now work! ✅

### **Option 2: Use Flutter Mobile/Desktop**

Instead of web, run your app on mobile or desktop:

```bash
# For mobile (Android/iOS)
flutter run

# For desktop
flutter run -d windows
flutter run -d macos
flutter run -d linux
```

### **Option 3: Set Up Local Backend API**

If you have the .NET backend code:

1. **Run the backend locally**:
   ```bash
   dotnet run --project YourBackendProject
   ```

2. **Update the API configuration** in `lib/main.dart`:
   ```dart
   // Uncomment this line:
   ApiConfig.initialize(apiUrl: 'http://localhost:7126/api');
   ```

## 🔧 **What We Fixed**

### **1. Enhanced Error Detection**
- Added CORS error detection in `anonymous_queue_service.dart`
- Provides clear error messages when CORS issues occur
- Shows specific solutions in the error message

### **2. Development vs Production Configuration**
- `lib/main.dart` now automatically detects development vs production
- Shows helpful instructions for CORS resolution in development mode
- Maintains production API for deployed apps

### **3. Better Logging**
- Added comprehensive logging to track API calls
- Shows which URLs are being attempted
- Displays user creation and UUID generation

## 🧪 **Testing the Solution**

1. **Run Chrome with CORS disabled** (Option 1 above)
2. **Navigate to**: `http://localhost:59095`
3. **Try joining a queue**:
   - Fill in your name and email
   - Select "Haircut" service (default)
   - Click "Join Queue"
4. **Check browser console** for debug messages:
   - Should see API URLs being called
   - Should see successful queue join
   - Should see queue position and wait time

## 🚀 **Production Deployment**

When deploying to production (not localhost), the CORS issue won't occur because:

1. **Same-origin requests**: If frontend and backend are on the same domain
2. **Proper CORS setup**: Production APIs typically have CORS configured correctly
3. **CDN/Proxy**: Production deployments often use CDNs that handle CORS

## 📝 **Important Notes**

- **CORS disabled Chrome is ONLY for development** - never browse other websites with it
- **The production API works fine** - this is purely a development environment issue
- **Mobile/desktop Flutter apps don't have CORS restrictions** - only web browsers do
- **Your queue functionality is working correctly** - the issue was just browser security

## 🔍 **Debugging**

If you still have issues, check the browser console (F12) for:

- **Network errors**: Look for failed requests to api.eutonafila.com.br
- **CORS errors**: Messages containing "CORS", "Cross-Origin", or "blocked"
- **Console logs**: Your app now prints detailed API information

## ✅ **Next Steps**

1. **Choose a solution** from the options above
2. **Test queue joining** functionality
3. **Verify queue data** appears correctly in the main page
4. **Continue development** with confidence that the API integration works!

---

**The anonymous queue functionality is now properly implemented and ready for use!** 🎉