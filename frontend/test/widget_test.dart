import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';

import 'package:eutonafila_frontend/main.dart';
// QueueEntry import removed - no longer needed
// MockQueueNotifier removed - using real queue service
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

  // MockQueueNotifier tests removed - using real queue service
}
