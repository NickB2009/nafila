import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:eutonafila_frontend/main.dart';
import 'package:eutonafila_frontend/models/queue_entry.dart';
import 'package:eutonafila_frontend/ui/view_models/mock_queue_notifier.dart';

void main() {
  group('Eutonafila App Tests', () {
    testWidgets('App loads and displays salon finder screen',
        (WidgetTester tester) async {
      // Build our app and trigger a frame.
      await tester.pumpWidget(const MyApp());

      // Wait for the async timer to complete and pump again
      await tester.pumpAndSettle();

      // Verify that the app bar is displayed
      expect(find.text('1'), findsOneWidget);

      // Verify that the TV dashboard button is present
      expect(find.byIcon(Icons.tv), findsOneWidget);

      // Verify that the notifications button is present
      expect(find.byIcon(Icons.notifications_outlined), findsOneWidget);
    });
    testWidgets('Loading state is displayed initially',
        (WidgetTester tester) async {
      await tester.pumpWidget(const MyApp());

      // Should show loading indicator initially
      expect(find.byType(CircularProgressIndicator), findsOneWidget);

      // Wait for the async operations to complete
      await tester.pumpAndSettle();
    });
    testWidgets('Salon finder content is displayed after loading',
        (WidgetTester tester) async {
      await tester.pumpWidget(const MyApp());

      // Wait for the mock data to load
      await tester.pumpAndSettle();

      // Should no longer show loading indicator
      expect(find.byType(CircularProgressIndicator), findsNothing);

      // Should show salon finder content
      expect(find.text('Olá, Rommel!'), findsOneWidget);
      expect(find.text('Faça cada dia'), findsOneWidget);
    });
    testWidgets('TV dashboard navigation works',
        (WidgetTester tester) async {
      await tester.pumpWidget(const MyApp());

      // Wait for loading to complete
      await tester.pumpAndSettle();

      // Tap the TV dashboard button
      await tester.tap(find.byIcon(Icons.tv));
      await tester.pumpAndSettle();

      // Should navigate to TV dashboard
      expect(find.text('TV Dashboard'), findsOneWidget);
    });
    testWidgets('Notifications screen navigation works',
        (WidgetTester tester) async {
      await tester.pumpWidget(const MyApp());

      // Wait for loading to complete
      await tester.pumpAndSettle();

      // Tap the notifications button
      await tester.tap(find.byIcon(Icons.notifications_outlined));
      await tester.pumpAndSettle();

      // Should navigate to notifications screen
      expect(find.text('Notificações'), findsOneWidget);
    });
  });

  group('MockQueueNotifier Tests', () {
    late MockQueueNotifier notifier;

    setUp(() {
      notifier = MockQueueNotifier();
    });

    test('Initial state is loading', () {
      expect(notifier.isLoading, isTrue);
      expect(notifier.entries, isEmpty);
    });

    test('Mock data loads correctly', () async {
      // Wait for mock data to load
      await Future.delayed(const Duration(milliseconds: 600));

      expect(notifier.isLoading, isFalse);
      expect(notifier.entries.length, equals(5));
      expect(notifier.waitingCount, equals(3));
      expect(notifier.inServiceCount, equals(1));
    });

    test('Add person to queue works', () async {
      // Wait for initial load
      await Future.delayed(const Duration(milliseconds: 600));

      final initialCount = notifier.entries.length;
      notifier.addToQueue('Test Person');

      expect(notifier.entries.length, equals(initialCount + 1));
      expect(notifier.entries.last.name, equals('Test Person'));
    });

    test('Update status works', () async {
      // Wait for initial load
      await Future.delayed(const Duration(milliseconds: 600));

      final firstEntry = notifier.entries.first;
      notifier.updateStatus(firstEntry.id, QueueStatus.inService);

      final updatedEntry =
          notifier.entries.firstWhere((e) => e.id == firstEntry.id);
      expect(updatedEntry.status, equals(QueueStatus.inService));
    });

    test('Remove from queue works', () async {
      // Wait for initial load
      await Future.delayed(const Duration(milliseconds: 600));

      final initialCount = notifier.entries.length;
      final firstEntryId = notifier.entries.first.id;

      notifier.removeFromQueue(firstEntryId);

      expect(notifier.entries.length, equals(initialCount - 1));
      expect(notifier.entries.any((e) => e.id == firstEntryId), isFalse);
    });
  });
}
