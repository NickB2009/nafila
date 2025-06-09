# Frontend Development Guide

## Development Context & Use Cases

This frontend implements a queue management system with specific use cases organized by priority. The current development phase focuses on **MVP Core Features (Priority 1)**, specifically:

- **UC-ENTRY**: Client queue entry interface
- **UC-QUEUELISTCLI**: Real-time queue status display
- **UC-BARBERQUEUE**: Barber queue management view
- **UC-CALLNEXT**: Next client call interface
- **UC-CANCEL**: Queue cancellation functionality

For complete use case specifications, see `USE_CASES.md`.

## 1. Prerequisites Setup

### Required Tools
- **VS Code** with extensions:
  - Dart
  - Flutter
  - Flutter Widget Snippets (recommended)
- **Flutter SDK** with web support enabled

### Initial Configuration
```bash
# Enable Flutter web support
flutter config --enable-web

# Verify web support is enabled
flutter devices
```

### Repository Setup
```bash
# Create feature branch for each development session
git checkout -b feature/ui-20250608

# After session completion
git add .
git commit -m "feat: implement queue screen UI components"
git push origin feature/ui-20250608
```

## 2. Project Structure Implementation

### Create Core Directories
```
lib/
├── main.dart                           # Application entry point
└── ui/
    ├── screens/
    │   └── queue_screen.dart           # Main queue management screen
    ├── widgets/
    │   ├── queue_card.dart             # Individual queue entry widget
    │   └── status_panel.dart           # Status display widget
    ├── view_models/
    │   └── mock_queue_notifier.dart    # State management
    └── data/
        └── mock_data.dart              # Development mock data
```

## 3. State Management Setup

### Provider Configuration
Add to `pubspec.yaml`:
```yaml
dependencies:
  flutter:
    sdk: flutter
  provider: ^6.1.1
```

### Main App Structure
```dart
// main.dart template
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'ui/view_models/mock_queue_notifier.dart';
import 'ui/screens/queue_screen.dart';

void main() {
  runApp(
    ChangeNotifierProvider(
      create: (_) => MockQueueNotifier(),
      child: MyApp(),
    ),
  );
}

class MyApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Eutonafila Queue Manager',
      theme: ThemeData(
        primarySwatch: Colors.blue,
        visualDensity: VisualDensity.adaptivePlatformDensity,
      ),
      home: QueueScreen(),
      debugShowCheckedModeBanner: false,
    );
  }
}
```

## 4. UI Layer Development Priorities

### Phase 1: Core Components
1. **QueueScreen** - Main layout with responsive design
2. **QueueCard** - Individual queue entry display
3. **StatusPanel** - Status indicator component

### Design Constraints
- **Target Width**: 390-430 dp (iPhone 15 web)
- **Responsive**: Use `LayoutBuilder` for width adaptation
- **Mobile-First**: Design for touch interaction
- **Accessibility**: Proper semantic labels

### Component Properties
- **QueueCard**: `name`, `status`, `width`
- **StatusPanel**: `status`, `iconData`, `color`

## 5. Testing Strategy

### Widget Testing
Create test for each widget immediately after implementation:
```dart
// Example test structure
testWidgets('QueueCard displays name and status correctly', (WidgetTester tester) async {
  // Test implementation
});
```

### Test Files Structure
```
test/
├── widget_test.dart           # Main widget tests
└── widgets/
    ├── queue_card_test.dart   # QueueCard widget tests
    └── status_panel_test.dart # StatusPanel widget tests
```

## 6. AI Agent Integration Workflow

### Context Preparation
1. Prepare UI mockups or screenshots
2. Identify specific component to implement
3. Define component inputs and constraints

### Effective AI Prompts
```
Goal: Generate a QueueCard widget matching this screenshot
Inputs: name (String), status (String), width (double)
Constraints: 
- Mobile-first design
- Provider for state management
- Minimal mocking
- Material Design components
Output: Widget code + basic widget test
```

### Image Integration
- Attach PNG/JPG screenshots to Cursor
- Reference specific UI elements in prompts
- Request responsive behavior specifications

## 7. Development Phases

### Current Phase: Presentation Layer
- ✅ Project structure
- 🔄 QueueScreen implementation
- ⏳ QueueCard widget
- ⏳ StatusPanel widget
- ⏳ Responsive design testing

### Next Phase: ViewModel Layer
- Replace mocks with real data structures
- Implement business logic
- Add error handling

### Future Phase: Data Layer
- API integration
- HTTP client setup
- Repository pattern implementation

## 8. Quality Guidelines

### Code Standards
- Use meaningful variable names
- Follow Dart style guide
- Add documentation comments for public APIs
- Implement proper error handling

### UI/UX Standards
- Consistent spacing and typography
- Proper loading states
- Accessibility compliance
- Smooth animations and transitions

### Testing Standards
- Widget tests for all custom widgets
- Integration tests for user flows
- Golden tests for critical UI components
- Performance testing on target devices

### Design System Implementation
- [ ] `lib/ui/theme/app_colors.dart` - Color constants defined
- [ ] `lib/ui/theme/app_text_styles.dart` - Typography scale created
- [ ] `lib/ui/theme/app_spacing.dart` - Spacing constants defined
- [ ] `lib/ui/theme/app_theme.dart` - Material theme configuration

### Theme System Implementation
- [ ] `lib/ui/theme/organization_theme.dart` - Organization theme model
- [ ] `lib/ui/theme/theme_provider.dart` - Theme state management
- [ ] `lib/ui/theme/theme_utils.dart` - Theme helper functions
- [ ] `lib/ui/theme/theme_constants.dart` - Default theme values

#### Theme Implementation Guidelines
1. **Theme-Aware Components**
   - Use `Theme.of(context)` for all styling
   - Avoid hardcoded colors or styles
   - Support dynamic theme switching
   - Implement theme preview functionality

2. **Organization Branding**
   - Support custom color schemes
   - Allow custom typography
   - Enable organization logo integration
   - Maintain accessibility standards

3. **Theme Testing**
   - Test theme switching behavior
   - Verify color contrast ratios
   - Validate typography scaling
   - Check brand asset loading
