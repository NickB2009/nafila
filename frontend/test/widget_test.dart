import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';

import 'package:eutonafila_frontend/main.dart';
import 'package:eutonafila_frontend/models/queue_entry.dart';
import 'package:eutonafila_frontend/ui/view_models/mock_queue_notifier.dart';

void main() {
  group('Eutonafila App Tests', () {    testWidgets('App loads and displays queue screen',
        (WidgetTester tester) async {
      // Build our app and trigger a frame.
      await tester.pumpWidget(const EutonautilaApp());

      // Wait for the async timer to complete and pump again
      await tester.pumpAndSettle();

      // Verify that the app bar is displayed
      expect(find.text('Queue Management'), findsOneWidget);

      // Verify that the refresh button is present
      expect(find.byIcon(Icons.refresh), findsOneWidget);

      // Verify that the floating action button is present
      expect(find.byIcon(Icons.add), findsOneWidget);
    });    testWidgets('Loading state is displayed initially',
        (WidgetTester tester) async {
      await tester.pumpWidget(const EutonautilaApp());

      // Should show loading indicator initially
      expect(find.byType(CircularProgressIndicator), findsOneWidget);
      
      // Wait for the async operations to complete
      await tester.pumpAndSettle();
    });    testWidgets('Queue entries are displayed after loading',
        (WidgetTester tester) async {
      await tester.pumpWidget(const EutonautilaApp());

      // Wait for the mock data to load
      await tester.pumpAndSettle();

      // Should no longer show loading indicator
      expect(find.byType(CircularProgressIndicator), findsNothing);

      // Should show queue entries
      expect(find.text('John Doe'), findsOneWidget);
      expect(find.text('Jane Smith'), findsOneWidget);
      expect(find.text('Bob Johnson'), findsOneWidget);
    });    testWidgets('Add person dialog opens when FAB is tapped',
        (WidgetTester tester) async {
      await tester.pumpWidget(const EutonautilaApp());

      // Wait for loading to complete
      await tester.pumpAndSettle();

      // Tap the floating action button
      await tester.tap(find.byIcon(Icons.add));
      await tester.pumpAndSettle();

      // Should show the add person dialog
      expect(find.text('Add Person to Queue'), findsOneWidget);
      expect(find.text('Name'), findsOneWidget);
      expect(find.text('Cancel'), findsOneWidget);
      expect(find.text('Add'), findsOneWidget);
    });    testWidgets('Queue stats are displayed correctly',
        (WidgetTester tester) async {
      await tester.pumpWidget(const EutonautilaApp());

      // Wait for loading to complete
      await tester.pumpAndSettle();

      // Should show queue statistics labels - be more specific to avoid finding status text in queue entries
      expect(find.widgetWithText(Container, 'Waiting'), findsWidgets);
      expect(find.widgetWithText(Container, 'In Service'), findsWidgets);
      expect(find.widgetWithText(Container, 'Total'), findsWidgets);

      // Should show correct counts (based on mock data)
      expect(find.text('3'), findsOneWidget); // Waiting count
      expect(find.text('1'), findsOneWidget); // In service count
      expect(find.text('5'), findsOneWidget); // Total count
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
