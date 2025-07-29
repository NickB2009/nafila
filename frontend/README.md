# EutoNafila Frontend

A Flutter application for queue management in barbershops and salons.

## Backend Integration

This application now includes complete backend integration with the QueueHub API. The integration includes:

### 🏗️ Architecture

- **Models**: Type-safe data models with JSON serialization for all API entities
- **Services**: HTTP client services for each API domain (Auth, Queue, Location, Organization, Staff, Services)
- **Controllers**: State management controllers using ChangeNotifier pattern
- **Configuration**: Centralized API configuration with endpoint management

### 📊 API Coverage

The application integrates with all major API endpoints:

#### Authentication
- User registration and login
- Two-factor authentication
- Profile management
- JWT token handling

#### Queue Management
- Create and manage queues
- Join queues with customer information
- Call next customer
- Check-in process
- Service completion
- Real-time queue status

#### Location Management
- CRUD operations for salon/barbershop locations
- Business hours management
- Address and contact information
- Queue capacity settings

#### Organization Management
- Organization profile management
- Branding and customization
- Subscription management
- Live activity tracking

#### Staff Management
- Add and edit barber profiles
- Status management (Available, Busy, On Break)
- Service type associations
- Break management

#### Services Management
- Service offerings CRUD
- Pricing and duration management
- Service activation/deactivation

### 🧪 Testing Strategy

Following TDD methodology:
1. **Unit Tests**: Comprehensive test coverage for all models and services
2. **Integration Tests**: End-to-end workflow testing
3. **Mock Testing**: Isolated testing with mock API responses

### 🔧 Usage

Initialize the app controller in your main application:

```dart
final appController = AppController();
await appController.initialize();

// Access authentication
final authController = appController.auth;
await authController.login(LoginRequest(username: 'user', password: 'pass'));

// Access queue management
final queueController = appController.queue;
await queueController.addQueue(AddQueueRequest(locationId: 'loc-123'));
```

### 📱 State Management

Controllers provide reactive state management:
- Loading states
- Error handling
- Real-time updates
- Authentication state persistence

### 🚀 Getting Started

1. Ensure the backend API is running at `https://localhost:7126`
2. Run `flutter packages get` to install dependencies
3. Run `flutter packages pub run build_runner build` to generate serialization code
4. Start the app with `flutter run`

## Project Structure

```
lib/
├── config/          # API configuration
├── models/          # Data models with JSON serialization
├── services/        # HTTP client services
├── controllers/     # State management controllers
├── ui/             # Flutter UI components
└── utils/          # Utility functions

test/
├── models/         # Model unit tests
├── services/       # Service unit tests
├── integration/    # Integration tests
└── widgets/        # Widget tests
```

## Dependencies

- `dio`: HTTP client for API communication
- `json_annotation`: JSON serialization support
- `mockito`: Testing framework for mocking
- `flutter_test`: Flutter testing utilities

For complete API documentation, refer to the backend Swagger documentation at `https://localhost:7126/swagger/index.html`.
