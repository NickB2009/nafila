# Backend API Specification

## API Base Information
**Base URL**: `https://localhost:7126/api`
**Swagger Documentation**: `https://localhost:7126/swagger/index.html`
**OpenAPI Version**: 3.0.4
**API Title**: GrandeTech.QueueHub.API
**Description**: REST API for Queue Hub Application
**Version**: v1

## Integration Status
✅ **Frontend Anonymous Access**: **WORKING** - Successfully connected to live backend API
✅ **Backend Public Endpoints**: **IMPLEMENTED** - API returning real salon data
✅ **Real Data Display**: Application shows actual salon data from database
🚧 **Frontend Authenticated Access**: Ready - Controllers prepared for backend integration
❌ **User Authentication UI**: Not yet implemented - login/register screens needed

## Current Working Features

### ✅ Anonymous Access (Live and Working)
The frontend successfully connects to the backend and displays real data:

- **Live API Connection**: `GET https://localhost:7126/api/Public/salons`
- **Real Data**: Displays actual salons from the database
- **Error Handling**: Graceful fallback mechanisms
- **No Authentication Required**: Users can browse salons without logging in

**Current Data Being Displayed**:
- Classic Cuts Main (Rio de Janeiro)
- Grande Tech Downtown (São Paulo)  
- Grande Tech Mall (São Paulo)

```dart
// Working public endpoints:
GET  /api/Public/salons              // ✅ WORKING - Returns real salon data
GET  /api/Public/queue-status/{id}   // ✅ IMPLEMENTED - Ready for use
POST /api/Public/salons/nearby       // ✅ IMPLEMENTED - Ready for use

// Frontend services working:
PublicSalonService.getPublicSalons()     // ✅ WORKING
PublicSalonService.getQueueStatus(salonId)
PublicSalonService.getNearbySlons(lat, lng, radius)
```

### Authenticated Access (Controllers Ready)
All backend endpoints have corresponding frontend services and controllers ready for integration:

```dart
// Frontend services implemented and ready:
AuthService         // → /api/Auth endpoints
QueueService        // → /api/Queues endpoints  
LocationService     // → /api/Locations endpoints
OrganizationService // → /api/Organizations endpoints
StaffService        // → /api/Staff endpoints
ServicesService     // → /api/ServicesOffered endpoints
```

## API Endpoints by Module

### 🔐 Authentication Module (/api/Auth)
**Frontend Service**: `AuthService` ✅ Ready for backend integration

| Method | Endpoint | Frontend Method | Status |
|--------|----------|-----------------|---------|
| POST | `/Auth/login` | `AuthService.login()` | 🔧 Service Ready |
| POST | `/Auth/register` | `AuthService.register()` | 🔧 Service Ready |
| POST | `/Auth/verify-two-factor` | `AuthService.verifyTwoFactor()` | 🔧 Service Ready |
| GET | `/Auth/profile` | `AuthService.getProfile()` | 🔧 Service Ready |
| POST | `/Auth/logout` | `AuthService.logout()` | 🔧 Service Ready |

### 🏢 Organizations Module (/api/Organizations)  
**Frontend Service**: `OrganizationService` ✅ Ready for backend integration

| Method | Endpoint | Frontend Method | Status |
|--------|----------|-----------------|---------|
| POST | `/Organizations` | `OrganizationService.createOrganization()` | 🔧 Service Ready |
| GET | `/Organizations` | `OrganizationService.getOrganizations()` | 🔧 Service Ready |
| GET | `/Organizations/{id}` | `OrganizationService.getOrganization()` | 🔧 Service Ready |
| PUT | `/Organizations/{id}` | `OrganizationService.updateOrganization()` | 🔧 Service Ready |
| PUT | `/Organizations/{id}/branding` | `OrganizationService.updateBranding()` | 🔧 Service Ready |
| PUT | `/Organizations/{id}/subscription` | `OrganizationService.updateSubscriptionPlan()` | 🔧 Service Ready |
| PUT | `/Organizations/{id}/analytics-sharing` | `OrganizationService.updateAnalyticsSharing()` | 🔧 Service Ready |
| PUT | `/Organizations/{id}/activate` | `OrganizationService.activateOrganization()` | 🔧 Service Ready |
| PUT | `/Organizations/{id}/deactivate` | `OrganizationService.deactivateOrganization()` | 🔧 Service Ready |
| GET | `/Organizations/by-slug/{slug}` | `OrganizationService.getOrganizationBySlug()` | 🔧 Service Ready |
| GET | `/Organizations/{id}/live-activity` | `OrganizationService.getLiveActivity()` | 🔧 Service Ready |

### 📍 Locations Module (/api/Locations)
**Frontend Service**: `LocationService` ✅ Ready for backend integration

| Method | Endpoint | Frontend Method | Status |
|--------|----------|-----------------|---------|
| GET | `/Locations` | `LocationService.getLocations()` | 🔧 Service Ready |
| POST | `/Locations` | `LocationService.createLocation()` | 🔧 Service Ready |
| GET | `/Locations/{id}` | `LocationService.getLocation()` | 🔧 Service Ready |
| PUT | `/Locations/{id}` | `LocationService.updateLocation()` | 🔧 Service Ready |
| DELETE | `/Locations/{id}` | `LocationService.deleteLocation()` | 🔧 Service Ready |
| PUT | `/Locations/{id}/toggle-queue` | `LocationService.toggleQueueStatus()` | 🔧 Service Ready |

### 🎯 Queues Module (/api/Queues)
**Frontend Service**: `QueueService` ✅ Ready for backend integration

| Method | Endpoint | Frontend Method | Status |
|--------|----------|-----------------|---------|
| POST | `/Queues` | `QueueService.addQueue()` | 🔧 Service Ready |
| GET | `/Queues/{id}` | `QueueService.getQueue()` | 🔧 Service Ready |
| POST | `/Queues/{id}/join` | `QueueService.joinQueue()` | 🔧 Service Ready |
| POST | `/Queues/{id}/call-next` | `QueueService.callNext()` | 🔧 Service Ready |
| POST | `/Queues/{id}/check-in` | `QueueService.checkIn()` | 🔧 Service Ready |
| POST | `/Queues/{id}/finish-service` | `QueueService.finishService()` | 🔧 Service Ready |
| DELETE | `/Queues/{id}` | `QueueService.cancelQueue()` | 🔧 Service Ready |
| GET | `/Queues/{id}/wait-time` | `QueueService.getWaitTime()` | 🔧 Service Ready |
| POST | `/Queues/{id}/haircut-details` | `QueueService.saveHaircutDetails()` | 🔧 Service Ready |

### 👥 Staff Module (/api/Staff)
**Frontend Service**: `StaffService` ✅ Ready for backend integration

| Method | Endpoint | Frontend Method | Status |
|--------|----------|-----------------|---------|
| POST | `/Staff/barber` | `StaffService.addBarber()` | 🔧 Service Ready |
| PUT | `/Staff/barber/{id}` | `StaffService.editBarber()` | 🔧 Service Ready |
| PUT | `/Staff/{id}/status` | `StaffService.updateStaffStatus()` | 🔧 Service Ready |
| POST | `/Staff/{id}/start-break` | `StaffService.startBreak()` | 🔧 Service Ready |
| POST | `/Staff/{id}/end-break` | `StaffService.endBreak()` | 🔧 Service Ready |

### 🛠️ Services Module (/api/ServicesOffered)
**Frontend Service**: `ServicesService` ✅ Ready for backend integration

| Method | Endpoint | Frontend Method | Status |
|--------|----------|-----------------|---------|
| POST | `/ServicesOffered` | `ServicesService.addService()` | 🔧 Service Ready |
| GET | `/ServicesOffered` | `ServicesService.getServices()` | 🔧 Service Ready |
| GET | `/ServicesOffered/{id}` | `ServicesService.getService()` | 🔧 Service Ready |
| PUT | `/ServicesOffered/{id}` | `ServicesService.updateService()` | 🔧 Service Ready |
| DELETE | `/ServicesOffered/{id}` | `ServicesService.deleteService()` | 🔧 Service Ready |
| PUT | `/ServicesOffered/{id}/activate` | `ServicesService.activateService()` | 🔧 Service Ready |

### 📊 Analytics Module (/api/Analytics)
**Frontend Service**: Not yet implemented ❌

| Method | Endpoint | Description | Status |
|--------|----------|-------------|---------|
| POST | `/Analytics/calculate-wait` | Calculate wait time estimates | ❌ Needs Frontend Service |
| POST | `/Analytics/cross-barbershop` | Cross-barbershop analytics | ❌ Needs Frontend Service |
| POST | `/Analytics/organization` | Organization analytics | ❌ Needs Frontend Service |
| POST | `/Analytics/top-organizations` | Top performing organizations | ❌ Needs Frontend Service |

## Required Backend Development

### 🌐 Public Endpoints (High Priority)
The frontend is ready for these endpoints but they need to be implemented on the backend:

```csharp
[AllowAnonymous]
public class PublicController : Controller 
{
    [HttpGet("salons")]
    public async Task<IActionResult> GetPublicSalons() 
    {
        // Return list of public salon information
        // Maps to: PublicSalon model in frontend
    }
    
    [HttpPost("salons/nearby")]
    public async Task<IActionResult> GetNearbySalons([FromBody] NearbySearchRequest request) 
    {
        // Search salons by location/radius
        // Input: latitude, longitude, radiusKm, limit, openOnly
        // Output: List<PublicSalon>
    }
    
    [HttpGet("queue-status/{salonId}")]  
    public async Task<IActionResult> GetQueueStatus(string salonId) 
    {
        // Return public queue information
        // Maps to: PublicQueueStatus model in frontend
    }
}
```

### 📱 Frontend Data Models Ready
The frontend has these models ready for your backend responses:

```dart
// Anonymous browsing models
class PublicSalon {
  String id, name, address;
  double? latitude, longitude, distanceKm;
  bool isOpen, isFast, isPopular;
  int? currentWaitTimeMinutes, queueLength;
  // ... other fields ready
}

class PublicQueueStatus {
  String salonId, salonName;
  int queueLength, estimatedWaitTimeMinutes;
  bool isAcceptingCustomers;
  DateTime lastUpdated;
  // ... other fields ready
}

// All authenticated models also implemented:
// LoginRequest, RegisterRequest, CreateLocationRequest, etc.
```

## Error Handling

### Frontend Error Handling ✅ Implemented
```dart
// All services handle errors gracefully:
try {
  final result = await apiClient.post(endpoint, data: data);
  return result.data;
} catch (e) {
  // Logs error and rethrows for controller handling
  rethrow;
}
```

### Expected Backend Error Format
The frontend expects standard HTTP status codes and JSON error responses:

```json
{
  "error": "Error message",
  "details": "Additional error details",
  "timestamp": "2025-01-XX...",
  "path": "/api/endpoint"
}
```

## Authentication Flow

### Current Implementation ✅
```dart
// JWT token management ready:
AuthService.login() → stores JWT token
ApiClient.setAuthToken() → adds "Bearer {token}" to requests
AuthService.logout() → clears stored token
```

### Backend Requirements
- JWT token generation on login
- Token validation on protected endpoints  
- Refresh token support (optional for MVP)

## Testing Status

### Frontend Tests ✅ Complete
- ✅ Unit tests for all services
- ✅ Integration tests for complete workflows
- ✅ Anonymous browsing functionality tests
- ✅ Mock data fallback system tests

### Backend Tests ❌ Required
The backend needs corresponding tests for:
- Public endpoint functionality
- Authentication flows
- All CRUD operations
- Error handling scenarios

## Next Steps for Backend Integration

### Priority 1: Anonymous Access
1. Implement `/api/Public/salons` endpoint
2. Implement `/api/Public/queue-status/{id}` endpoint  
3. Implement `/api/Public/salons/nearby` endpoint
4. Test with frontend anonymous browsing

### Priority 2: Authentication
1. Verify JWT token generation matches frontend expectations
2. Test login/register flows with frontend
3. Confirm token validation on protected endpoints

### Priority 3: Core Operations
1. Test all CRUD operations with frontend services
2. Verify error responses match frontend error handling
3. Performance testing with real data

## Backend Development Prompts

When you need new backend features, use these prompts:

### For Public Endpoints
```
Create a Public API controller in ASP.NET Core that provides anonymous access to salon data. 
The controller should implement these endpoints:
- GET /api/Public/salons - returns public salon information
- POST /api/Public/salons/nearby - searches salons by location  
- GET /api/Public/queue-status/{salonId} - returns queue status

Use [AllowAnonymous] attribute and return data matching these frontend models:
[Include PublicSalon and PublicQueueStatus model definitions]
```

### For Authentication Issues
```
Fix the JWT authentication in the backend to work with this frontend login flow:
[Include frontend login request/response models and expected token format]
```

### For Missing Endpoints
```
Implement the missing backend endpoint for [specific operation] that matches this frontend service call:
[Include specific frontend service method and expected request/response format]
```