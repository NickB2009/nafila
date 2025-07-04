# UI Component Specifications

## Overview
This document defines the detailed specifications for all UI components in the Eutonafila queue management application.

## Design System

### Color Palette
```dart
class AppColors {
  static const Color primary = Color(0xFF2196F3);
  static const Color secondary = Color(0xFF03DAC6);
  static const Color background = Color(0xFFF5F5F5);
  static const Color surface = Color(0xFFFFFFFF);
  static const Color error = Color(0xFFB00020);
  
  // Status Colors
  static const Color waiting = Color(0xFFFF9800);
  static const Color inService = Color(0xFF4CAF50);
  static const Color completed = Color(0xFF9E9E9E);
}
```

### Theme Customization
The application supports organization-specific theming through a centralized theme system:

```dart
class OrganizationTheme {
  final Color primary;
  final Color secondary;
  final Color background;
  final Color surface;
  final Color error;
  final Color waiting;
  final Color inService;
  final Color completed;
  final String logoPath;
  final String fontFamily;
  
  const OrganizationTheme({
    required this.primary,
    required this.secondary,
    required this.background,
    required this.surface,
    required this.error,
    required this.waiting,
    required this.inService,
    required this.completed,
    required this.logoPath,
    required this.fontFamily,
  });
}
```

#### Theme Implementation
- All UI components reference theme values instead of hardcoded colors
- Theme changes are applied dynamically without requiring app restart
- Brand assets (logos, icons) are loaded from organization-specific paths
- Typography can be customized per organization while maintaining readability

#### Theme Switching
- Theme configuration is stored in organization settings
- Theme changes are applied through the ThemeProvider
- All components automatically update when theme changes
- Theme preview available in settings for testing

### Typography
```dart
class AppTextStyles {
  static const TextStyle heading = TextStyle(
    fontSize: 24,
    fontWeight: FontWeight.bold,
    color: Colors.black87,
  );
  
  static const TextStyle subheading = TextStyle(
    fontSize: 18,
    fontWeight: FontWeight.w600,
    color: Colors.black87,
  );
  
  static const TextStyle body = TextStyle(
    fontSize: 16,
    fontWeight: FontWeight.normal,
    color: Colors.black87,
  );
  
  static const TextStyle caption = TextStyle(
    fontSize: 14,
    fontWeight: FontWeight.normal,
    color: Colors.black54,
  );
}
```

### Spacing
```dart
class AppSpacing {
  static const double xs = 4.0;
  static const double sm = 8.0;
  static const double md = 16.0;
  static const double lg = 24.0;
  static const double xl = 32.0;
}
```

## Component Specifications

### 1. QueueScreen

**Purpose**: Main screen displaying the queue list and controls

**Layout Structure**:
```
AppBar
├── Title: "Queue Management"
├── Actions: [RefreshButton, SettingsButton]
└── Body
    ├── StatusSummary (optional)
    └── ListView.builder
        └── QueueCard items
```

**Responsive Behavior**:
- Width 320-430dp: Single column, full width cards
- Padding: 16dp horizontal, 8dp vertical
- Card spacing: 12dp between items

**State Dependencies**:
- `MockQueueNotifier.entries` for queue data
- `MockQueueNotifier.isLoading` for loading state

### 2. QueueCard

**Purpose**: Display individual queue entry with name, status, and actions

**Properties**:
```dart
class QueueCard extends StatelessWidget {
  final String name;
  final String status;
  final double width;
  final VoidCallback? onTap;
  final VoidCallback? onStatusChange;
  
  const QueueCard({
    Key? key,
    required this.name,
    required this.status,
    required this.width,
    this.onTap,
    this.onStatusChange,
  }) : super(key: key);
}
```

**Visual Design**:
- Material Card with elevation 2
- Rounded corners: 12dp
- Padding: 16dp
- Height: 80dp minimum

**Layout**:
```
Card
├── Row
│   ├── CircleAvatar (40x40)
│   │   └── Initials or Icon
│   ├── Expanded
│   │   ├── Text (name) - AppTextStyles.body
│   │   └── StatusPanel
│   └── IconButton (actions)
```

**Responsive Adaptations**:
- Width < 360dp: Reduce padding to 12dp
- Width > 400dp: Increase card height to 88dp

### 3. StatusPanel

**Purpose**: Display queue status with appropriate styling and icon

**Properties**:
```dart
class StatusPanel extends StatelessWidget {
  final String status;
  final bool compact;
  
  const StatusPanel({
    Key? key,
    required this.status,
    this.compact = false,
  }) : super(key: key);
}
```

**Status Mappings**:
```dart
enum QueueStatus {
  waiting('Waiting', Icons.access_time, AppColors.waiting),
  inService('In Service', Icons.person, AppColors.inService),
  completed('Completed', Icons.check_circle, AppColors.completed);
  
  const QueueStatus(this.label, this.icon, this.color);
  final String label;
  final IconData icon;
  final Color color;
}
```

**Visual Design**:
- Container with rounded background
- Status color background with 0.1 opacity
- Icon + Text in status color
- Padding: 8x12dp
- Border radius: 20dp

## Mock Data Structure

### Queue Entry Model
```dart
class QueueEntry {
  final String id;
  final String name;
  final String status;
  final DateTime joinTime;
  final int position;
  
  const QueueEntry({
    required this.id,
    required this.name,
    required this.status,
    required this.joinTime,
    required this.position,
  });
}
```

### Sample Data
```dart
final List<QueueEntry> mockQueueData = [
  QueueEntry(
    id: '1',
    name: 'João Silva',
    status: 'waiting',
    joinTime: DateTime.now().subtract(Duration(minutes: 5)),
    position: 1,
  ),
  QueueEntry(
    id: '2',
    name: 'Maria Santos',
    status: 'in_service',
    joinTime: DateTime.now().subtract(Duration(minutes: 15)),
    position: 2,
  ),
  QueueEntry(
    id: '3',
    name: 'Pedro Costa',
    status: 'waiting',
    joinTime: DateTime.now().subtract(Duration(minutes: 2)),
    position: 3,
  ),
];
```

## Accessibility Requirements

### Semantic Labels
- QueueCard: "Queue entry for [name], status [status]"
- StatusPanel: "Status: [status]"
- Action buttons: Clear action descriptions

### Touch Targets
- Minimum 48x48dp for all interactive elements
- Adequate spacing between touch targets
- Visual feedback for interactions

### Color Contrast
- Text contrast ratio: minimum 4.5:1
- Icon contrast ratio: minimum 3:1
- Alternative indicators for color-dependent information

## Animation Specifications

### Card Interactions
- Tap: Scale down to 0.98 over 100ms
- State changes: Cross-fade over 200ms
- List updates: Slide in/out over 300ms with easing

### Loading States
- Shimmer effect for loading cards
- Fade in when data loads
- Pull-to-refresh with standard Material behavior

## Testing Scenarios

### Widget Tests Required
1. **QueueCard**: Displays correct name and status
2. **QueueCard**: Handles tap interactions
3. **StatusPanel**: Shows correct status styling
4. **StatusPanel**: Updates when status changes
5. **QueueScreen**: Renders list of queue entries
6. **QueueScreen**: Handles empty state
7. **QueueScreen**: Responds to width changes

### Visual Regression Tests
- Card layout at different widths
- Status panel color accuracy
- Loading state appearance
- Empty state display
