# Anonymous Access Implementation Plan

## ✅ IMPLEMENTATION COMPLETE

**Status**: Successfully implemented and working in production

### ✅ Working Features
- **Anonymous Salon Browsing**: Users can view salons without login
- **Real Backend Data**: Connected to live API endpoints
- **Public Queue Status**: Can view salon wait times and availability  
- **Responsive UI**: Works across all screen sizes
- **Error Handling**: Graceful fallback mechanisms

### ✅ Live Endpoints Working
- `GET /api/Public/salons` - Returns real salon data ✅
- `GET /api/Public/queue-status/{id}` - Queue status lookup ✅
- `POST /api/Public/salons/nearby` - Location-based search ✅

### ✅ Real Data Currently Displayed
- Classic Cuts Main (Rio de Janeiro)
- Grande Tech Downtown (São Paulo)
- Grande Tech Mall (São Paulo)

---

## Original Implementation Plan (Completed)

## Current State (Before Implementation)
❌ **All operations require authentication**
❌ **No anonymous/guest account exists**
❌ **No public endpoints**

## Common Anonymous Use Cases for Queue Apps

### 1. **Public Salon Directory**
- Browse salons without login
- View salon details, hours, services
- Check queue status/wait times

### 2. **Public Queue Status**
- View current queue without joining
- Check wait times
- See salon availability

### 3. **Salon TV Dashboard** 
- Public display showing queue status
- No authentication needed
- Real-time updates

## Implementation Options

### Option A: Anonymous API Access

#### 1. Backend Changes Needed
```csharp
// Add public endpoints to your backend
[AllowAnonymous]
public class PublicController : Controller 
{
    [HttpGet("salons")]
    public async Task<IActionResult> GetPublicSalons() { ... }
    
    [HttpGet("salons/{id}/queue-status")]  
    public async Task<IActionResult> GetQueueStatus(string id) { ... }
}
```

#### 2. Frontend API Client Changes
```dart
class ApiClient {
  // Add methods that don't require auth token
  Future<Response<T>> getPublic<T>(String path) async {
    // Temporarily remove auth header for this request
    final options = Options(headers: {'Authorization': null});
    return await _dio.get<T>(path, options: options);
  }
}
```

#### 3. Create Anonymous Services
```dart
class PublicSalonService {
  final ApiClient _apiClient;
  
  // Get salons without authentication
  Future<List<dynamic>> getPublicSalons() async {
    final response = await _apiClient.getPublic('/public/salons');
    return response.data as List<dynamic>;
  }
  
  // Get queue status without authentication  
  Future<Map<String, dynamic>> getQueueStatus(String salonId) async {
    final response = await _apiClient.getPublic('/public/salons/$salonId/queue-status');
    return response.data;
  }
}
```

### Option B: Mock/Static Data for Anonymous Users

#### 1. Create Anonymous Mode
```dart
class AppController extends ChangeNotifier {
  bool _anonymousMode = true; // Start in anonymous mode
  
  Future<List<dynamic>> getLocations() async {
    if (_anonymousMode) {
      // Return mock data for anonymous users
      return _getMockSalons();
    }
    
    // Existing authenticated flow
    if (!_authController.isAuthenticated) {
      throw Exception('User not authenticated');
    }
    // ... rest of authenticated logic
  }
}
```

#### 2. Anonymous Controller
```dart
class AnonymousController extends ChangeNotifier {
  // Provides read-only access to public data
  Future<List<Salon>> getBrowsableSalons() async {
    return _mockSalons; // Or call public API
  }
  
  Future<QueueStatus> getPublicQueueStatus(String salonId) async {
    return _mockQueueStatus; // Or call public API  
  }
}
```

### Option C: Guest Account Pattern

#### 1. Create Guest User
```dart
class AuthService {
  Future<LoginResult> loginAsGuest() async {
    // Create temporary guest session
    final guestToken = await _createGuestSession();
    await _storeAuthData(
      token: guestToken,
      username: 'guest_${DateTime.now().millisecondsSinceEpoch}',
      role: 'guest',
    );
    return LoginResult(success: true, token: guestToken);
  }
}
```

#### 2. Permission-Based Access
```dart
class AppController {
  Future<List<dynamic>> getLocations() async {
    if (!_authController.isAuthenticated) {
      // Auto-login as guest
      await _authController.loginAsGuest();
    }
    
    if (_authController.currentRole == 'guest') {
      // Limited access for guests
      return await _getGuestVisibleLocations();
    }
    
    // Full access for authenticated users
    return await _locationService.getLocations();
  }
}
```

## Specific Use Cases to Consider

### 1. **Salon TV Dashboard (Public Display)**
```dart
// Should work without any authentication
class SalonTvController {
  Future<QueueStatus> getDisplayData(String salonId) async {
    // Call public API or use display-specific endpoint
    return await _publicService.getDisplayData(salonId);
  }
}
```

### 2. **Browse Salons (Anonymous Users)**
```dart
// Allow browsing salon directory without login
class PublicSalonController {
  Future<List<Salon>> getBrowsableSalons() async {
    // Public salon directory
    return await _publicService.getSalons();
  }
  
  Future<SalonDetails> getSalonDetails(String salonId) async {
    // Public salon info (no sensitive data)
    return await _publicService.getSalonDetails(salonId);
  }
}
```

### 3. **Queue Status Viewing (No Joining)**
```dart
// Allow viewing queue status without joining
class PublicQueueController {
  Future<QueueStatus> getQueueStatus(String salonId) async {
    // Public queue status (positions, wait times)
    return await _publicService.getQueueStatus(salonId);
  }
  
  // Note: Joining queue would still require authentication
}
```

## Recommended Approach

**For your queue management app, I recommend Option A + Option C:**

1. **Add public endpoints** for salon browsing and queue status viewing
2. **Implement guest accounts** for users who want to browse before registering
3. **Keep sensitive operations authenticated** (joining queues, managing appointments)

This provides the best user experience while maintaining security.

## Questions for You:

1. **What operations should work anonymously?**
   - Browse salon directory?
   - View queue status?
   - Salon TV dashboard?

2. **Should anonymous users be able to join queues?**
   - Or require registration first?

3. **Do you want guest accounts or purely anonymous access?**

Let me know which approach you prefer and I can implement it! 🚀