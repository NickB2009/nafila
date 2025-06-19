import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';

import 'package:eutonafila_frontend/main.dart';
import 'package:eutonafila_frontend/models/queue_entry.dart';
import 'package:eutonafila_frontend/ui/view_models/mock_queue_notifier.dart';
import 'package:eutonafila_frontend/theme/app_theme.dart';

void main() {
  group('Eutonafila App Tests', () {
    testWidgets('App loads without crashing',
        (WidgetTester tester) async {
      // Build our app with proper provider setup
      await tester.pumpWidget(
        ChangeNotifierProvider(
          create: (_) => ThemeProvider(),
          child: const MyApp(),
        ),
      );

      // Wait for the async timer to complete and pump again
      await tester.pumpAndSettle();

      // Verify that the app loads without crashing
      expect(find.byType(MaterialApp), findsOneWidget);
    });

    testWidgets('App displays welcome message after loading',
        (WidgetTester tester) async {
      await tester.pumpWidget(
        ChangeNotifierProvider(
          create: (_) => ThemeProvider(),
          child: const MyApp(),
        ),
      );

      // Wait for the mock data to load
      await tester.pumpAndSettle();

      // Should show salon finder content
      expect(find.text('Olá, Rommel!'), findsOneWidget);
    });

    testWidgets('App has navigation buttons',
        (WidgetTester tester) async {
      await tester.pumpWidget(
        ChangeNotifierProvider(
          create: (_) => ThemeProvider(),
          child: const MyApp(),
        ),
      );

      // Wait for loading to complete
      await tester.pumpAndSettle();

      // Should have navigation buttons
      expect(find.byIcon(Icons.tv), findsOneWidget);
      expect(find.byIcon(Icons.notifications_outlined), findsOneWidget);
    });

    testWidgets('App displays salon information',
        (WidgetTester tester) async {
      await tester.pumpWidget(
        ChangeNotifierProvider(
          create: (_) => ThemeProvider(),
          child: const MyApp(),
        ),
      );

      // Wait for loading to complete
      await tester.pumpAndSettle();

      // Should display salon information
      expect(find.text('Market at Mirada'), findsOneWidget);
      expect(find.text('Cortez Commons'), findsOneWidget);
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
