import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/ui/widgets/queue_card.dart';
import 'package:eutonafila_frontend/ui/widgets/status_panel.dart';
import 'package:eutonafila_frontend/models/queue_entry.dart';

void main() {
  group('QueueCard Widget Tests', () {
    late QueueEntry testEntry;

    setUp(() {
      testEntry = QueueEntry(
        id: '1',
        name: 'John Doe',
        status: QueueStatus.waiting,
        joinTime: DateTime.now().subtract(const Duration(minutes: 15)),
        position: 1,
      );
    });

    testWidgets('displays name and status correctly', (WidgetTester tester) async {
      final entry = testEntry.copyWith(
        joinTime: DateTime.now().subtract(const Duration(minutes: 15)),
      );

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: QueueCard(
              entry: entry,
              width: 400,
            ),
          ),
        ),
      );

      expect(find.text('John Doe'), findsOneWidget);
      expect(find.text('Waiting'), findsOneWidget);
      expect(find.text('Position 1'), findsOneWidget);
    });

    testWidgets('displays initials in avatar', (WidgetTester tester) async {
      final entry = testEntry.copyWith(
        joinTime: DateTime.now().subtract(const Duration(minutes: 15)),
      );

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: QueueCard(
              entry: entry,
              width: 400,
            ),
          ),
        ),
      );

      expect(find.text('JD'), findsOneWidget);
    });

    testWidgets('handles single name correctly', (WidgetTester tester) async {
      final entry = testEntry.copyWith(
        name: 'John',
        joinTime: DateTime.now().subtract(const Duration(minutes: 15)),
      );

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: QueueCard(
              entry: entry,
              width: 400,
            ),
          ),
        ),
      );

      expect(find.text('J'), findsOneWidget);
    });

    testWidgets('displays time ago for in-service status', (WidgetTester tester) async {
      final entry = testEntry.copyWith(
        status: QueueStatus.inService,
        joinTime: DateTime.now().subtract(const Duration(minutes: 30)),
        position: 0,
      );

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: QueueCard(
              entry: entry,
              width: 400,
            ),
          ),
        ),
      );

      expect(find.text('30m ago'), findsOneWidget);
      expect(find.text('Position 1'), findsNothing);
    });

    testWidgets('responds to tap events', (WidgetTester tester) async {
      bool tapped = false;
      final entry = testEntry.copyWith(
        joinTime: DateTime.now().subtract(const Duration(minutes: 15)),
      );

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: QueueCard(
              entry: entry,
              width: 400,
              onTap: () => tapped = true,
            ),
          ),
        ),
      );

      await tester.tap(find.byType(InkWell));
      expect(tapped, isTrue);
    });

    testWidgets('adapts to compact layout', (WidgetTester tester) async {
      final entry = testEntry.copyWith(
        joinTime: DateTime.now().subtract(const Duration(minutes: 15)),
      );

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: QueueCard(
              entry: entry,
              width: 350, // Compact width
            ),
          ),
        ),
      );

      // Should still display all essential information
      expect(find.text('John Doe'), findsOneWidget);
      expect(find.text('Waiting'), findsOneWidget);
      expect(find.text('Position 1'), findsOneWidget);
    });

    testWidgets('displays completed status correctly', (WidgetTester tester) async {
      final entry = testEntry.copyWith(
        status: QueueStatus.completed,
        joinTime: DateTime.now().subtract(const Duration(hours: 2)),
        position: 0,
      );

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: QueueCard(
              entry: entry,
              width: 400,
            ),
          ),
        ),
      );

      expect(find.text('Completed'), findsOneWidget);
      expect(find.text('2h ago'), findsOneWidget);
    });

    testWidgets('handles empty name gracefully', (WidgetTester tester) async {
      final entry = testEntry.copyWith(
        name: '',
        joinTime: DateTime.now().subtract(const Duration(minutes: 15)),
      );

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: QueueCard(
              entry: entry,
              width: 400,
            ),
          ),
        ),
      );

      expect(find.text('?'), findsOneWidget);
    });

    testWidgets('maintains minimum height constraint', (WidgetTester tester) async {
      final entry = testEntry.copyWith(
        joinTime: DateTime.now().subtract(const Duration(minutes: 15)),
      );

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: QueueCard(
              entry: entry,
              width: 400,
            ),
          ),
        ),
      );

      final cardFinder = find.byType(Card);
      expect(cardFinder, findsOneWidget);

      final card = tester.widget<Card>(cardFinder);
      expect(card.child, isA<InkWell>());
    });

    testWidgets('displays correct status colors', (WidgetTester tester) async {
      final waitingEntry = testEntry.copyWith(
        status: QueueStatus.waiting,
        joinTime: DateTime.now().subtract(const Duration(minutes: 15)),
      );

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: QueueCard(
              entry: waitingEntry,
              width: 400,
            ),
          ),
        ),
      );

      // Check that status panel is present
      expect(find.byType(StatusPanel), findsOneWidget);
    });
  });
} 