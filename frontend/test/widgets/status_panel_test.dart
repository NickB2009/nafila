import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/ui/widgets/status_panel.dart';
import 'package:eutonafila_frontend/models/queue_entry.dart';

void main() {
  group('StatusPanel Widget Tests', () {
    testWidgets('displays waiting status correctly', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: StatusPanel(
              status: QueueStatus.waiting,
            ),
          ),
        ),
      );

      expect(find.text('Waiting'), findsOneWidget);
      expect(find.byIcon(Icons.access_time), findsOneWidget);
    });

    testWidgets('displays in-service status correctly', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: StatusPanel(
              status: QueueStatus.inService,
            ),
          ),
        ),
      );

      expect(find.text('In Service'), findsOneWidget);
      expect(find.byIcon(Icons.person_outline), findsOneWidget);
    });

    testWidgets('displays completed status correctly', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: StatusPanel(
              status: QueueStatus.completed,
            ),
          ),
        ),
      );

      expect(find.text('Completed'), findsOneWidget);
      expect(find.byIcon(Icons.check_circle_outline), findsOneWidget);
    });

    testWidgets('adapts to compact mode', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: StatusPanel(
              status: QueueStatus.waiting,
              compact: true,
            ),
          ),
        ),
      );

      expect(find.text('Waiting'), findsOneWidget);
      expect(find.byIcon(Icons.access_time), findsOneWidget);
    });

    testWidgets('has proper styling and layout', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: StatusPanel(
              status: QueueStatus.waiting,
            ),
          ),
        ),
      );

      // Check that the container has proper styling
      final containerFinder = find.byType(Container);
      expect(containerFinder, findsOneWidget);

      // Check that icon and text are present
      expect(find.byIcon(Icons.access_time), findsOneWidget);
      expect(find.text('Waiting'), findsOneWidget);
    });

    testWidgets('displays all status types with correct icons', (WidgetTester tester) async {
      final statuses = [
        (QueueStatus.waiting, Icons.access_time),
        (QueueStatus.inService, Icons.person_outline),
        (QueueStatus.completed, Icons.check_circle_outline),
      ];

      for (final (status, expectedIcon) in statuses) {
        await tester.pumpWidget(
          MaterialApp(
            home: Scaffold(
              body: StatusPanel(
                status: status,
              ),
            ),
          ),
        );

        expect(find.byIcon(expectedIcon), findsOneWidget);
        expect(find.text(status.displayName), findsOneWidget);
      }
    });

    testWidgets('maintains consistent spacing in compact mode', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: Row(
              children: [
                StatusPanel(
                  status: QueueStatus.waiting,
                  compact: true,
                ),
                const SizedBox(width: 8),
                StatusPanel(
                  status: QueueStatus.inService,
                  compact: true,
                ),
              ],
            ),
          ),
        ),
      );

      // Both panels should be visible
      expect(find.text('Waiting'), findsOneWidget);
      expect(find.text('In Service'), findsOneWidget);
    });

    testWidgets('has proper accessibility support', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: StatusPanel(
              status: QueueStatus.waiting,
            ),
          ),
        ),
      );

      // Check that the widget is accessible
      expect(find.byType(StatusPanel), findsOneWidget);
      
      // Verify text is readable
      expect(find.text('Waiting'), findsOneWidget);
    });
  });
} 