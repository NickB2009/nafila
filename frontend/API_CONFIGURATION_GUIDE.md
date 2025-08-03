# 🔧 API Configuration Guide

## ✅ **Fixed Issues**

All hardcoded API URLs have been **centralized** and **removed**! Here's what was fixed:

### **Problems Found:**
1. ❌ Hardcoded `'https://localhost:7126/api/Public/salons'` in `public_salon_service.dart`
2. ❌ Fixed localhost configuration with no environment flexibility  
3. ❌ No easy way to change API URL for different environments

### **Solutions Implemented:**
1. ✅ **Centralized API Configuration** in `lib/config/api_config.dart`
2. ✅ **Environment-aware URL detection**
3. ✅ **Easy API URL configuration** in `main.dart`
4. ✅ **Real database data only** (no mock fallbacks)

---

## 🚀 **How to Configure API URL**

### **Option 1: Default localhost (Backend on same machine)**
```dart
// In lib/main.dart (line 17)
ApiConfig.initialize(); // Uses https://localhost:7126/api
```

### **Option 2: Custom API URL**
```dart
// In lib/main.dart - uncomment and modify:

// For production server:
ApiConfig.initialize(apiUrl: 'https://your-production-api.com');

// For local network (different computer):
ApiConfig.initialize(apiUrl: 'http://192.168.1.100:7126');

// For development server:
ApiConfig.initialize(apiUrl: 'https://dev-api.eutonafila.com.br');
```

---

## 🖥️ **Running from Your Laptop**

### **Scenario 1: Backend on Same Laptop**
```dart
// No changes needed - uses default
ApiConfig.initialize(); 
```
**Backend URL**: `https://localhost:7126/api`

### **Scenario 2: Backend on Different Computer (Local Network)**
```dart
// Replace 192.168.1.100 with your backend computer's IP
ApiConfig.initialize(apiUrl: 'http://192.168.1.100:7126');
```

### **Scenario 3: Remote Backend Server**
```dart
// Replace with your actual backend URL
ApiConfig.initialize(apiUrl: 'https://api.eutonafila.com.br');
```

---

## 📱 **Quick Setup Steps**

### **Step 1: Find Your Backend IP**
```bash
# On backend computer, find IP:
ipconfig          # Windows
ifconfig          # macOS/Linux
```

### **Step 2: Update main.dart**
```dart
// In lib/main.dart, replace line 17:
ApiConfig.initialize(apiUrl: 'http://YOUR_BACKEND_IP:7126');
```

### **Step 3: Build and Run**
```bash
flutter build web
flutter run -d chrome
```

---

## 🔍 **API Endpoints Used**

The app uses these backend endpoints:

| Endpoint | Purpose | Real Data |
|----------|---------|-----------|
| `GET /api/Public/salons` | Get all salons | ✅ Database |
| `POST /api/Public/queue/join` | Join queue | ✅ Database |
| `GET /api/Public/queue/status/{id}` | Get queue status | ✅ Database |
| `POST /api/Public/queue/leave/{id}` | Leave queue | ✅ Database |

**All endpoints now use REAL database data** - no mock data fallbacks!

---

## 🐛 **Troubleshooting**

### **"Application Error" Issues:**

1. **CORS Error**: Backend needs to allow your frontend domain
2. **Connection Refused**: Check if backend is running on correct port
3. **SSL Issues**: Use `http://` instead of `https://` for local development

### **API Not Found (404):**
```dart
// Make sure your backend has these endpoints:
// GET  /api/Public/salons
// POST /api/Public/queue/join
// GET  /api/Public/queue/status/{id}
```

### **Debug API Calls:**
The app will print the configured API URL on startup:
```
🔗 API configured for: https://localhost:7126/api (default)
🔗 API configured for: http://192.168.1.100:7126/api
```

---

## 📋 **Configuration Examples**

### **Local Development**
```dart
void main() {
  WidgetsFlutterBinding.ensureInitialized();
  ApiConfig.initialize(); // localhost:7126
  runApp(/* ... */);
}
```

### **Team Development**
```dart
void main() {
  WidgetsFlutterBinding.ensureInitialized();
  ApiConfig.initialize(apiUrl: 'http://192.168.1.50:7126'); // Team backend
  runApp(/* ... */);
}
```

### **Production Deployment**
```dart
void main() {
  WidgetsFlutterBinding.ensureInitialized();
  ApiConfig.initialize(apiUrl: 'https://api.eutonafila.com.br'); // Production
  runApp(/* ... */);
}
```

---

## ✨ **Benefits of Centralized Configuration**

1. **Single source of truth**: All API URLs in one place
2. **Environment flexibility**: Easy switching between dev/prod
3. **Real data only**: No mock data fallbacks
4. **Type safety**: Centralized endpoint constants
5. **Easy debugging**: Clear API URL logging

---

## 🎯 **Next Steps**

1. **Update `lib/main.dart`** with your backend URL
2. **Ensure backend is running** on the configured port
3. **Test API connection** by viewing salon list
4. **Check browser network tab** for API call details

Your Flutter app will now successfully connect to the real backend database! 🚀