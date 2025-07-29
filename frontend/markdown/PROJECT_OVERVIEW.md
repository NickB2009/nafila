# Eutonafila Frontend - Queue Management App

## Project Overview
A Flutter web application for managing queues, designed with a mobile-first approach and optimized for iPhone 15 web display.

## Key Technologies
- **Framework**: Flutter Web
- **State Management**: Provider + ChangeNotifier
- **HTTP Client**: Dio for API communication
- **Architecture**: Clean Architecture with Controllers, Services, and Models
- **Target Platform**: Web (mobile-responsive)
- **Target Device**: iPhone 15 web (~390-430 dp width)
- **Development IDE**: VS Code with Dart & Flutter extensions

## Backend Integration
- **API Base URL**: `https://localhost:7126/api`
- **API Documentation**: `https://localhost:7126/swagger/index.html`
- **Authentication**: JWT tokens with Bearer authorization
- **Anonymous Access**: Implemented for salon browsing before login

## Theming and Branding
The application is designed with a flexible theming system to support organization-specific branding:
- **Theme Customization**: All UI components are built using a centralized theme system
- **Branding Support**: Colors, typography, and visual elements can be customized per organization
- **Theme Implementation**: Uses Flutter's ThemeData for consistent styling across all screens
- **Brand Assets**: Support for custom logos, icons, and brand-specific imagery

## Project Goals
- Create a responsive queue management interface
- Implement clean separation of concerns (UI → Controllers → Services → Models)
- Provide anonymous salon browsing before login
- Full backend API integration with comprehensive error handling
- Maintain layer-by-layer development approach

## Current Phase
**Phase 4: Production Ready** - Backend integration complete, application fully operational.

**Status**:
- ✅ **Anonymous Access**: Implemented and working with real backend data
- ✅ **Frontend Services**: All API services and controllers implemented and tested
- ✅ **Authentication Flow**: JWT token management ready for implementation
- ✅ **Backend Integration**: Successfully connected to live API endpoints
- ✅ **Real Data Display**: App shows actual salon data from database
- ❌ **User Authentication**: Login/register flow not yet implemented
- ❌ **Analytics Module**: Frontend service not yet implemented

## Integration Success
The application successfully connects to the backend API and displays real salon data:
- **Live API Connection**: `https://localhost:7126/api/Public/salons`
- **Real Salon Data**: Displays actual salons from the database
- **Error Handling**: Graceful fallback and retry mechanisms
- **Anonymous Browsing**: Users can view salons without authentication

## Next Development Phase
With backend integration complete, the next priorities are:
1. Implement user authentication (login/register screens)
2. Complete authenticated user features (check-in, queue management)
3. Add analytics and reporting features
4. Production deployment configuration

## Repository Structure
```
eutonafila_frontend/
├── lib/
│   ├── main.dart
│   ├── config/
│   │   └── api_config.dart         # API endpoints and configuration
│   ├── models/                     # Data models for API requests/responses
│   │   ├── auth_models.dart
│   │   ├── location_models.dart
│   │   ├── organization_models.dart
│   │   ├── queue_models.dart
│   │   ├── staff_models.dart
│   │   ├── services_models.dart
│   │   └── public_salon.dart       # Anonymous browsing models
│   ├── services/                   # API communication layer
│   │   ├── api_client.dart         # HTTP client with auth handling
│   │   ├── auth_service.dart       # Authentication operations
│   │   ├── queue_service.dart      # Queue management
│   │   ├── location_service.dart   # Location operations
│   │   ├── organization_service.dart # Organization management
│   │   ├── staff_service.dart      # Staff operations
│   │   ├── services_service.dart   # Service offerings
│   │   └── public_salon_service.dart # Anonymous browsing
│   ├── controllers/                # State management layer
│   │   ├── app_controller.dart     # Main application controller
│   │   ├── auth_controller.dart    # Authentication state
│   │   ├── queue_controller.dart   # Queue state management
│   │   └── anonymous_controller.dart # Anonymous browsing state
│   └── ui/
│       ├── screens/               # Main application screens
│       ├── widgets/               # Reusable UI components
│       │   ├── nearby_salons_widget.dart # Anonymous salon browsing
│       │   └── home_screen.dart   # Multi-mode home screen
│       ├── view_models/           # Legacy state management
│       └── theme/                 # Theme and styling
├── test/                          # Comprehensive test suite
│   ├── models/                    # Model unit tests
│   ├── services/                  # Service unit tests
│   └── integration/               # Integration tests
├── markdown/                      # Documentation
│   ├── BACKEND_API_SPECIFICATION.md # Backend API documentation
│   ├── PROJECT_OVERVIEW.md       # This file
│   ├── IMPLEMENTATION_CHECKLIST.md # Progress tracking
│   └── [other documentation files]
└── web/                          # Flutter web output
```

## Development Workflow
1. ✅ **Prerequisites Setup** - Flutter web configuration
2. ✅ **UI Layer Complete** - All screens and widgets
3. ✅ **State Management** - Provider integration with controllers
4. ✅ **Service Layer** - API services for all backend modules
5. ✅ **Model Layer** - Complete data models with JSON serialization
6. ✅ **Anonymous Access** - Public salon browsing without authentication
7. ✅ **Testing** - Comprehensive unit and integration tests
8. 🚧 **Backend Integration** - Connecting services to live API endpoints
9. ❌ **Analytics Module** - Frontend service implementation pending
10. ❌ **Production Deployment** - Build optimization and deployment

## API Integration Status

### ✅ Ready for Backend Connection
All these modules have complete frontend implementation:
- **Authentication**: Login, register, 2FA, profile management
- **Organizations**: CRUD operations, branding, subscription management  
- **Locations**: CRUD operations, queue toggle
- **Queues**: Join, call next, check-in, finish service, wait times
- **Staff**: Barber management, status updates, breaks
- **Services**: Service offerings CRUD operations
- **Public Access**: Anonymous salon browsing

### 🚧 Backend Development Required
These endpoints need implementation on the backend:
- `/api/Public/salons` - Public salon directory
- `/api/Public/queue-status/{id}` - Queue status without auth
- `/api/Public/salons/nearby` - Location-based salon search

### ❌ Not Yet Implemented
- **Analytics Module**: Frontend service and UI components
- **Advanced Features**: Real-time updates, notifications, reporting

## Use Cases & Requirements
The application implements a comprehensive set of use cases organized by priority:
- **MVP Core Features (Priority 1)**: Essential queue management functionality ✅
- **MVP Enhanced Features (Priority 2)**: Advanced features for better user experience ✅
- **Backend Integration (Priority 3)**: Live API connectivity 🚧
- **Future Features (V2+)**: Long-term enhancements and optimizations ❌

For detailed use case specifications, see `USE_CASES.md`.
For backend API details, see `BACKEND_API_SPECIFICATION.md`.
For implementation progress, see `IMPLEMENTATION_CHECKLIST.md`.

## Next Steps
1. **Backend Public Endpoints**: Implement anonymous access endpoints
2. **Authentication Testing**: Verify JWT flows with backend
3. **Service Integration**: Connect all frontend services to live backend
4. **Analytics Module**: Implement frontend analytics service
5. **Performance Testing**: Load testing with real backend data
6. **Production Deployment**: Build optimization and hosting setup

Refer to `DEVELOPMENT_GUIDE.md` for detailed implementation instructions.
Refer to `BACKEND_API_SPECIFICATION.md` for backend integration requirements.
