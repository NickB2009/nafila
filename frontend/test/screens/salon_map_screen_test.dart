import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';
import 'package:eutonafila_frontend/ui/screens/salon_map_screen.dart';
import 'package:eutonafila_frontend/ui/view_models/mock_queue_notifier.dart';
import 'package:eutonafila_frontend/theme/app_theme.dart';

void main() {
  group('SalonMapScreen Tests', () {
    Widget buildTestable() {
      return MultiProvider(
        providers: [
          ChangeNotifierProvider(create: (_) => ThemeProvider()),
          ChangeNotifierProvider(create: (_) => MockQueueNotifier()),
        ],
        child: const MaterialApp(
          home: SalonMapScreen(),
        ),
      );
    }

    testWidgets('displays salon information', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump();

      // Check that salon information is displayed
      expect(find.text('Market at Mirada'), findsOneWidget);
      expect(find.text('Aberto'), findsWidgets);
      expect(find.text('24 min'), findsOneWidget);
    });

    testWidgets('displays map controls', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump();

      // Check map control buttons
      expect(find.byIcon(Icons.my_location), findsOneWidget);
      expect(find.byIcon(Icons.add), findsOneWidget);
      expect(find.byIcon(Icons.remove), findsOneWidget);
    });

    testWidgets('displays multiple salons', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump();

      // Check that multiple salons are displayed
      expect(find.text('Aberto'), findsWidgets);
    });

    testWidgets('displays salon addresses', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump();

      // Check that salon addresses are displayed
      expect(find.textContaining('Mirada'), findsWidgets);
    });

    testWidgets('displays wait times', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump();

      // Check that wait times are displayed
      expect(find.text('24 min'), findsOneWidget);
    });

    testWidgets('displays salon status', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump();

      // Check that salon status is displayed
      expect(find.text('Aberto'), findsWidgets);
    });

    testWidgets('displays distance information', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump();

      // Check that distance is displayed
      expect(find.textContaining('km'), findsWidgets);
    });

    testWidgets('accessibility elements are present', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump();

      // Check that all main UI elements are accessible
      expect(find.text('Market at Mirada'), findsOneWidget);
    });
  });
} 