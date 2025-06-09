# Eutonafila Frontend - Queue Management App

## Project Overview
A Flutter web application for managing queues, designed with a mobile-first approach and optimized for iPhone 15 web display.

## Key Technologies
- **Framework**: Flutter Web
- **State Management**: Provider + ChangeNotifier
- **Target Platform**: Web (mobile-responsive)
- **Target Device**: iPhone 15 web (~390-430 dp width)
- **Development IDE**: VS Code with Dart & Flutter extensions

## Project Goals
- Create a responsive queue management interface
- Implement clean separation of concerns (UI → ViewModel → Data)
- Use minimal mocking to focus on UI development
- Maintain layer-by-layer development approach

## Current Phase
**Phase 1: Presentation Layer** - Focus on UI components and responsive design before moving to business logic.

## Repository Structure
```
eutonafila_frontend/
├── lib/
│   ├── main.dart
│   └── ui/
│       ├── screens/          # Main application screens
│       ├── widgets/          # Reusable UI components
│       ├── view_models/      # State management
│       └── data/            # Mock data for development
├── pubspec.yaml
├── test/                    # Widget and integration tests
├── web/                     # Flutter web output
├── PROJECT_OVERVIEW.md      # This file - project summary
├── USE_CASES.md            # Complete use case specifications
├── FEATURE_MAPPING.md      # UI components to use cases mapping
├── DEVELOPMENT_GUIDE.md    # Implementation instructions
├── IMPLEMENTATION_CHECKLIST.md  # Development progress tracking
├── UI_SPECIFICATIONS.md    # Design system and component specs
└── TESTING_GUIDE.md        # Testing strategies and examples
```

## Development Workflow
1. **Prerequisites Setup** - Flutter web configuration
2. **UI Layer Complete** - All screens and widgets
3. **State Management** - Provider integration
4. **Testing** - Widget tests for each component
5. **ViewModel Layer** - Business logic integration
6. **Data Layer** - API integration and error handling

## Use Cases & Requirements
The application implements a comprehensive set of use cases organized by priority:
- **MVP Core Features (Priority 1)**: Essential queue management functionality
- **MVP Enhanced Features (Priority 2)**: Advanced features for better user experience
- **Future Features (V2+)**: Long-term enhancements and optimizations

For detailed use case specifications, see `USE_CASES.md`.
For UI component to use case mapping, see `FEATURE_MAPPING.md`.

## Next Steps
Refer to `DEVELOPMENT_GUIDE.md` for detailed implementation instructions.
