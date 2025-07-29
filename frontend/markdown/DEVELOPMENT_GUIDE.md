
# Frontend Development Guide

## 🌐 System Context

This frontend is for a SaaS queue management platform with a multi-tenant architecture structured around **Organizations** and **Locations**. Access control is enforced through tenant-aware RBAC, with roles as follows:

- **Platform-level operations** (`PlatformAdmin`)
- **Organization-level operations** (`Admin`)
- **Location-level operations** (`Barber`)
- **Client-level operations** (`Client`)
- **Service operations** (`ServiceAccount`)

The frontend is scoped for **Client**, **Barber**, and partial **Admin** interfaces in the MVP.

## 🧱 Current Development Context

We've successfully completed **Backend Integration** for anonymous access. The application is now fully operational with real backend data.

### ✅ Completed Phases
- **Phase 1**: UI Layer Development - All screens and widgets complete
- **Phase 2**: State Management - Controllers and Provider integration complete  
- **Phase 3**: Service Layer - All API services implemented and tested
- **Phase 4**: Anonymous Access - Public salon browsing implemented and working
- **Phase 5**: Backend Integration (Anonymous) - Successfully connected to live API

### 🚧 Current Status: Production Ready (Anonymous Features)
- **Frontend Status**: Anonymous features fully functional with real backend data
- **Backend Status**: Public endpoints implemented and working
- **Live Integration**: Application displays real salon data from database
- **Next Steps**: Implement user authentication (login/register screens)

## 🎉 Successfully Working Features

### ✅ Live Anonymous Access
- **Real API Connection**: `GET https://localhost:7126/api/Public/salons`
- **Real Data Display**: Shows actual salons from the database
- **Error Handling**: Graceful fallback and retry mechanisms
- **Responsive UI**: Fully functional across all screen sizes

**Current Live Data**:
- Classic Cuts Main (Rio de Janeiro)
- Grande Tech Downtown (São Paulo)
- Grande Tech Mall (São Paulo)

## 🔌 Backend API Integration

### API Configuration
**Base URL**: `https://localhost:7126/api`
**Swagger Docs**: `https://localhost:7126/swagger/index.html`

### Frontend Services Ready for Integration

```dart
// All services implemented and ready:
AuthService         // → /api/Auth/* endpoints
QueueService        // → /api/Queues/* endpoints  
LocationService     // → /api/Locations/* endpoints
OrganizationService // → /api/Organizations/* endpoints
StaffService        // → /api/Staff/* endpoints
ServicesService     // → /api/ServicesOffered/* endpoints
PublicSalonService  // → /api/Public/* endpoints (anonymous access)

// Controllers managing state:
AppController       // Main application coordinator
AuthController      // Authentication state management
QueueController     // Queue operations state
AnonymousController // Anonymous browsing state
```

### Testing Backend Integration

#### 1. Start with Anonymous Access
```bash
# Test public endpoints first (no auth required)
curl https://localhost:7126/api/Public/salons
curl https://localhost:7126/api/Public/queue-status/salon-1
```

#### 2. Test Authentication Flow
```dart
// Frontend login flow
final authService = await AuthService.create();
final result = await authService.login(LoginRequest(
  username: 'testuser',
  password: 'testpass',
));
// Should receive JWT token and store it automatically
```

#### 3. Test Authenticated Endpoints
```dart
// All services will automatically include JWT token
final queueService = await QueueService.create();
final queues = await queueService.getQueue('queue-id');
```

---

## 🧰 Prerequisites Setup

### Required Tools
- **VS Code** with:
  - Dart
  - Flutter
  - Flutter Widget Snippets (recommended)
- **Flutter SDK** with web enabled

### Initial Configuration
```bash
flutter config --enable-web
flutter devices
```

### Repository Setup

```bash
git checkout -b feature/backend-integration-YYYYMMDD
# Example:
# git checkout -b feature/backend-integration-20250611

# After your changes:
git add .
git commit -m "feat: integrate [service] with backend API"
git push origin feature/backend-integration-YYYYMMDD
```

---

## 🧩 Project Structure

```plaintext
lib/
├── main.dart
├── config/
│   └── api_config.dart              # API endpoints and configuration
├── models/                          # Request/Response models
│   ├── auth_models.dart            # Login, register, profile
│   ├── location_models.dart        # Location CRUD operations
│   ├── organization_models.dart    # Organization management
│   ├── queue_models.dart           # Queue operations
│   ├── staff_models.dart           # Staff management
│   ├── services_models.dart        # Service offerings
│   └── public_salon.dart           # Anonymous browsing
├── services/                        # API communication layer
│   ├── api_client.dart             # HTTP client with auth
│   ├── auth_service.dart           # Authentication operations
│   ├── queue_service.dart          # Queue management
│   ├── location_service.dart       # Location operations
│   ├── organization_service.dart   # Organization management
│   ├── staff_service.dart          # Staff operations
│   ├── services_service.dart       # Service offerings
│   └── public_salon_service.dart   # Anonymous browsing
├── controllers/                     # State management
│   ├── app_controller.dart         # Main coordinator
│   ├── auth_controller.dart        # Auth state
│   ├── queue_controller.dart       # Queue state
│   └── anonymous_controller.dart   # Anonymous state
└── ui/
    ├── screens/                    # Application screens
    └── widgets/                    # Reusable components
```

---

## 📦 State Management

### Provider Setup

In `pubspec.yaml`:

```yaml
dependencies:
  flutter:
    sdk: flutter
  provider: ^6.1.1
  dio: ^5.4.0
  shared_preferences: ^2.2.2
  json_annotation: ^4.8.1

dev_dependencies:
  build_runner: ^2.4.7
  json_serializable: ^6.7.1
```

### App Entry with Backend Integration

```dart
void main() {
  runApp(
    FutureBuilder<AppController>(
      future: _initializeApp(),
      builder: (context, snapshot) {
        if (snapshot.hasData) {
          return ChangeNotifierProvider<AppController>.value(
            value: snapshot.data!,
            child: MyApp(),
          );
        }
        return MaterialApp(
          home: Scaffold(
            body: Center(child: CircularProgressIndicator()),
          ),
        );
      },
    ),
  );
}

Future<AppController> _initializeApp() async {
  final appController = AppController();
  await appController.initialize(); // Loads auth state, connects to backend
  return appController;
}
```

---

## 🔌 Backend Integration Implementation

### Service Integration Pattern

All services follow this pattern for backend integration:

```dart
class ExampleService {
  final ApiClient _apiClient;

  // Service automatically handles auth tokens
  Future<ResponseModel> performOperation(RequestModel request) async {
    try {
      final response = await _apiClient.post(
        ApiConfig.exampleEndpoint,
        data: request.toJson(),
      );
      return ResponseModel.fromJson(response.data);
    } catch (e) {
      // Error logged automatically, rethrown for controller handling
      rethrow;
    }
  }
}
```

### Controller Integration Pattern

Controllers manage state and coordinate service calls:

```dart
class ExampleController extends ChangeNotifier {
  final ExampleService _service;
  bool _isLoading = false;
  String? _error;
  List<DataModel> _data = [];

  // Getters
  bool get isLoading => _isLoading;
  String? get error => _error;
  List<DataModel> get data => _data;

  Future<void> performOperation() async {
    try {
      _setLoading(true);
      _setError(null);
      
      final result = await _service.performOperation(request);
      _data = result.data;
      
      notifyListeners();
    } catch (e) {
      _setError('Operation failed: ${e.toString()}');
    } finally {
      _setLoading(false);
    }
  }
}
```

---

## 🧪 Backend Integration Testing

### Testing Strategy

```dart
// Integration test example
testWidgets('should handle complete authentication workflow', (WidgetTester tester) async {
  // Arrange
  final appController = AppController();
  await appController.initialize();
  
  // Act
  final result = await appController.auth.login(LoginRequest(
    username: 'testuser',
    password: 'testpass',
  ));
  
  // Assert
  expect(result.success, isTrue);
  expect(appController.auth.isAuthenticated, isTrue);
  expect(appController.isAnonymousMode, isFalse);
});
```

### Manual Testing Workflow

1. **Test Anonymous Access First**
   ```dart
   // Should work without authentication
   await anonymousController.loadPublicSalons();
   expect(anonymousController.nearbySalons.isNotEmpty, isTrue);
   ```

2. **Test Authentication Flow**
   ```dart
   // Should authenticate and switch modes
   await authController.login(validCredentials);
   expect(authController.isAuthenticated, isTrue);
   ```

3. **Test Authenticated Operations**
   ```dart
   // Should include auth token automatically
   await queueController.addQueue(queueRequest);
   expect(queueController.error, isNull);
   ```

---

## 🚨 Backend Requirements for Frontend Integration

### Priority 1: Public Endpoints (Required for Anonymous Access)

```csharp
[AllowAnonymous]
public class PublicController : Controller 
{
    [HttpGet("salons")]
    public async Task<IActionResult> GetPublicSalons() 
    {
        // Return List<PublicSalon> format
        // See BACKEND_API_SPECIFICATION.md for model definition
    }
    
    [HttpPost("salons/nearby")]
    public async Task<IActionResult> GetNearbySalons([FromBody] NearbySearchRequest request) 
    {
        // Input: latitude, longitude, radiusKm, limit, openOnly
        // Output: List<PublicSalon>
    }
    
    [HttpGet("queue-status/{salonId}")]  
    public async Task<IActionResult> GetQueueStatus(string salonId) 
    {
        // Return PublicQueueStatus format
    }
}
```

### Priority 2: Authentication Verification

Ensure JWT tokens match frontend expectations:

```json
{
  "success": true,
  "token": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9...",
  "username": "testuser",
  "role": "Client",
  "message": "Login successful"
}
```

### Priority 3: All Other Endpoints

All endpoints documented in `BACKEND_API_SPECIFICATION.md` with corresponding frontend services ready.

---

## 🤖 AI Workflow for Backend Integration

### Backend Development Prompts

When requesting backend features, use these structured prompts:

```txt
Implement the [endpoint name] backend endpoint to match this frontend service:

Frontend Service: [ServiceClass.methodName()]
Expected Request: [RequestModel definition]
Expected Response: [ResponseModel definition]
Authentication: [Required/Anonymous]

Additional Requirements:
- [Specific business logic requirements]
- [Error handling scenarios]
- [Performance considerations]

Reference: BACKEND_API_SPECIFICATION.md for complete context.
```

### Integration Testing Prompts

```txt
Test the integration between frontend service [ServiceName] and backend endpoint [endpoint].

Frontend Implementation:
[Include specific service method code]

Expected Backend Behavior:
[Include expected request/response flow]

Verify:
- Request format matches backend expectations
- Response format matches frontend models
- Error handling works correctly
- Authentication tokens are validated
```

---

## 📅 Development Phases

### ✅ Completed Phases

* **Phase 1: Presentation Layer** - All UI components implemented
* **Phase 2: State Management** - Controllers and Provider setup complete
* **Phase 3: Service Layer** - All API services implemented and tested
* **Phase 4: Anonymous Access** - Public salon browsing complete

### 🚧 Current Phase: Backend Integration

* Connect frontend services to live backend endpoints
* Verify authentication flows
* Test all CRUD operations
* Performance testing with real data

### 🔜 Next Phase: Analytics & Production

* Implement analytics frontend service
* Production deployment preparation
* Performance optimization
* Monitoring and error tracking

---

## 🧼 Quality Standards

### Code
* Descriptive naming
* Follow Dart style guide
* Public methods = documented
* Handle errors gracefully
* Comprehensive test coverage

### Backend Integration
* All services tested with live backend
* Error scenarios handled appropriately
* Performance meets requirements (<2s response times)
* Authentication security verified
* Data validation on both frontend and backend

### Production Readiness
* No mock data in production builds
* Error tracking and monitoring
* Performance benchmarks established
* Security audit completed
* Documentation updated for deployment

---

For detailed backend API specifications, see `BACKEND_API_SPECIFICATION.md`.
For implementation progress tracking, see `IMPLEMENTATION_CHECKLIST.md`.
