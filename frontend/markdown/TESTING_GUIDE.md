# Testing Strategy & Implementation

## Overview
Comprehensive testing approach for the Eutonafila Flutter queue management application, focusing on widget tests, integration tests, and quality assurance.

## Testing Philosophy

### Layer-by-Layer Testing
1. **Widget Tests**: Individual component behavior
2. **Integration Tests**: Screen-level user flows
3. **Golden Tests**: Visual regression prevention
4. **Performance Tests**: Responsiveness validation

### Test-Driven Development
- Write tests immediately after widget creation
- Use tests to validate requirements
- Ensure accessibility compliance
- Maintain high test coverage

## Widget Testing Strategy

### 1. QueueCard Widget Tests

**Test File**: `test/widgets/queue_card_test.dart`

**Test Scenarios**:
```dart
group('QueueCard Widget', () {
  testWidgets('displays name and status correctly', (tester) async {
    // Test basic rendering
  });
  
  testWidgets('handles tap interaction', (tester) async {
    // Test onTap callback
  });
  
  testWidgets('adapts to different widths', (tester) async {
    // Test responsive behavior
  });
  
  testWidgets('shows correct accessibility labels', (tester) async {
    // Test semantic labels
  });
  
  testWidgets('updates when status changes', (tester) async {
    // Test state updates
  });
});
```

**Sample Implementation**:
```dart
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/ui/widgets/queue_card.dart';

void main() {
  group('QueueCard Widget Tests', () {
    testWidgets('displays name and status correctly', (WidgetTester tester) async {
      // Arrange
      const testName = 'João Silva';
      const testStatus = 'waiting';
      const testWidth = 400.0;
      
      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: QueueCard(
              name: testName,
              status: testStatus,
              width: testWidth,
            ),
          ),
        ),
      );
      
      // Assert
      expect(find.text(testName), findsOneWidget);
      expect(find.byType(StatusPanel), findsOneWidget);
    });
    
    testWidgets('triggers onTap callback when tapped', (WidgetTester tester) async {
      // Arrange
      bool tapped = false;
      void onTap() => tapped = true;
      
      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: QueueCard(
              name: 'Test User',
              status: 'waiting',
              width: 400.0,
              onTap: onTap,
            ),
          ),
        ),
      );
      
      await tester.tap(find.byType(QueueCard));
      await tester.pump();
      
      // Assert
      expect(tapped, isTrue);
    });
  });
}
```

### 2. StatusPanel Widget Tests

**Test File**: `test/widgets/status_panel_test.dart`

**Test Scenarios**:
```dart
group('StatusPanel Widget', () {
  testWidgets('shows correct icon and color for waiting status', (tester) async {
    // Test waiting status styling
  });
  
  testWidgets('shows correct icon and color for in_service status', (tester) async {
    // Test in_service status styling
  });
  
  testWidgets('shows correct icon and color for completed status', (tester) async {
    // Test completed status styling
  });
  
  testWidgets('supports compact mode', (tester) async {
    // Test compact display variation
  });
});
```

### 3. QueueScreen Widget Tests

**Test File**: `test/screens/queue_screen_test.dart`

**Test Scenarios**:
```dart
group('QueueScreen Widget', () {
  testWidgets('renders empty state when no queue entries', (tester) async {
    // Test empty list handling
  });
  
  testWidgets('displays list of queue entries', (tester) async {
    // Test list rendering with mock data
  });
  
  testWidgets('shows loading indicator when loading', (tester) async {
    // Test loading state
  });
  
  testWidgets('handles pull-to-refresh', (tester) async {
    // Test refresh functionality
  });
  
  testWidgets('adapts layout for different screen widths', (tester) async {
    // Test responsive behavior
  });
});
```

## State Management Testing

### MockQueueNotifier Tests

**Test File**: `test/view_models/mock_queue_notifier_test.dart`

**Test Coverage**:
```dart
group('MockQueueNotifier', () {
  late MockQueueNotifier notifier;
  
  setUp(() {
    notifier = MockQueueNotifier();
  });
  
  test('initializes with empty queue', () {
    expect(notifier.entries, isEmpty);
    expect(notifier.isLoading, isFalse);
  });
  
  test('adds new queue entry', () {
    // Test addEntry method
  });
  
  test('updates existing queue entry status', () {
    // Test updateStatus method
  });
  
  test('removes queue entry', () {
    // Test removeEntry method
  });
  
  test('notifies listeners on state changes', () {
    // Test ChangeNotifier behavior
  });
});
```

## Integration Testing

### User Flow Tests

**Test File**: `integration_test/queue_management_test.dart`

**Test Scenarios**:
1. **Complete Queue Flow**:
   - Load app
   - View queue list
   - Tap on queue entry
   - Update status
   - Verify UI updates

2. **Responsive Design Flow**:
   - Test at different viewport sizes
   - Verify layout adaptations
   - Check touch target sizes

**Sample Integration Test**:
```dart
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:eutonafila_frontend/main.dart' as app;

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();
  
  group('Queue Management Flow', () {
    testWidgets('complete queue interaction flow', (tester) async {
      // Launch app
      app.main();
      await tester.pumpAndSettle();
      
      // Verify queue screen loads
      expect(find.text('Queue Management'), findsOneWidget);
      
      // Verify queue entries are displayed
      expect(find.byType(QueueCard), findsAtLeastNWidgets(1));
      
      // Tap on first queue entry
      await tester.tap(find.byType(QueueCard).first);
      await tester.pumpAndSettle();
      
      // Verify interaction feedback
      // Add specific assertions based on expected behavior
    });
  });
}
```

## Golden Testing (Visual Regression)

### Setup
```yaml
# pubspec.yaml dev_dependencies
dev_dependencies:
  flutter_test:
    sdk: flutter
  golden_toolkit: ^0.15.0
```

### Golden Tests Implementation

**Test File**: `test/golden/widget_golden_test.dart`

```dart
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:golden_toolkit/golden_toolkit.dart';
import 'package:eutonafila_frontend/ui/widgets/queue_card.dart';

void main() {
  group('Golden Tests', () {
    testGoldens('QueueCard appears correctly', (tester) async {
      final builder = DeviceBuilder()
        ..overrideDevicesForAllScenarios(devices: [
          Device.phone,
          Device.iphone11,
        ])
        ..addScenario(
          widget: QueueCard(
            name: 'João Silva',
            status: 'waiting',
            width: 390,
          ),
          name: 'waiting_status',
        )
        ..addScenario(
          widget: QueueCard(
            name: 'Maria Santos',
            status: 'in_service',
            width: 390,
          ),
          name: 'in_service_status',
        );
      
      await tester.pumpDeviceBuilder(builder);
      await screenMatchesGolden(tester, 'queue_card_states');
    });
  });
}
```

## Accessibility Testing

### Accessibility Test Checklist

**Screen Reader Tests**:
```dart
testWidgets('has proper semantic labels', (tester) async {
  await tester.pumpWidget(testWidget);
  
  // Check semantic labels
  expect(
    tester.getSemantics(find.byType(QueueCard)),
    matchesSemantics(
      label: 'Queue entry for João Silva, status waiting',
      button: true,
    ),
  );
});
```

**Touch Target Tests**:
```dart
testWidgets('has adequate touch targets', (tester) async {
  await tester.pumpWidget(testWidget);
  
  final cardSize = tester.getSize(find.byType(QueueCard));
  expect(cardSize.height, greaterThanOrEqualTo(48.0)); // Minimum touch target
});
```

## Performance Testing

### Widget Performance Tests

```dart
testWidgets('renders efficiently with large datasets', (tester) async {
  // Create large mock dataset
  final largeDataset = List.generate(1000, (index) => 
    QueueEntry(id: '$index', name: 'User $index', status: 'waiting')
  );
  
  // Measure rendering time
  final stopwatch = Stopwatch()..start();
  
  await tester.pumpWidget(
    MaterialApp(
      home: QueueScreen(entries: largeDataset),
    ),
  );
  
  stopwatch.stop();
  
  // Assert reasonable render time
  expect(stopwatch.elapsedMilliseconds, lessThan(1000));
});
```

## Test Data Management

### Mock Data Factory

**File**: `test/helpers/mock_data_factory.dart`

```dart
class MockDataFactory {
  static QueueEntry createQueueEntry({
    String? id,
    String? name,
    String? status,
    DateTime? joinTime,
    int? position,
  }) {
    return QueueEntry(
      id: id ?? 'test_id',
      name: name ?? 'Test User',
      status: status ?? 'waiting',
      joinTime: joinTime ?? DateTime.now(),
      position: position ?? 1,
    );
  }
  
  static List<QueueEntry> createQueueList(int count) {
    return List.generate(count, (index) => createQueueEntry(
      id: 'id_$index',
      name: 'User ${index + 1}',
      position: index + 1,
    ));
  }
}
```

## Test Execution

### Local Testing Commands

```bash
# Run all tests
flutter test

# Run widget tests only
flutter test test/widgets/

# Run tests with coverage
flutter test --coverage
genhtml coverage/lcov.info -o coverage/html

# Run integration tests
flutter drive --driver=test_driver/integration_test.dart --target=integration_test/app_test.dart

# Run golden tests
flutter test --update-goldens
```

### CI/CD Testing Pipeline

```yaml
# .github/workflows/test.yml
name: Test Suite
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: subosito/flutter-action@v2
        with:
          flutter-version: '3.x'
      - run: flutter pub get
      - run: flutter analyze
      - run: flutter test --coverage
      - run: flutter test integration_test/
```

## Quality Metrics

### Coverage Targets
- **Widget Tests**: 90%+ coverage
- **Integration Tests**: All critical user flows
- **Golden Tests**: All reusable widgets
- **Accessibility**: 100% compliance

### Performance Benchmarks
- **Widget render time**: <100ms
- **Screen transition**: <300ms
- **Large list scroll**: 60 FPS
- **Memory usage**: <50MB baseline

## Testing Best Practices

### 1. Test Organization
- Group related tests logically
- Use descriptive test names
- Follow AAA pattern (Arrange, Act, Assert)
- Keep tests independent and isolated

### 2. Mock Strategy
- Mock external dependencies
- Use real objects for UI testing
- Create reusable mock factories
- Avoid over-mocking

### 3. Maintenance
- Update tests with code changes
- Review test coverage regularly
- Refactor tests for clarity
- Document complex test scenarios
