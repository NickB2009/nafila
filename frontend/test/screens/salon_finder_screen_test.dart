import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';
import 'package:eutonafila_frontend/ui/screens/salon_finder_screen.dart';
import 'package:eutonafila_frontend/ui/view_models/mock_queue_notifier.dart';
import 'package:eutonafila_frontend/theme/app_theme.dart';

void main() {
  group('SalonFinderScreen Tests', () {
    testWidgets('displays welcome message', (WidgetTester tester) async {
      await tester.pumpWidget(
        MultiProvider(
          providers: [
            ChangeNotifierProvider(create: (_) => ThemeProvider()),
            ChangeNotifierProvider(create: (_) => MockQueueNotifier()),
          ],
          child: const MaterialApp(
            home: SalonFinderScreen(),
          ),
        ),
      );

      await tester.pumpAndSettle();

      // Check for welcome message
      expect(find.text('Olá, Rommel!'), findsOneWidget);
    });

    testWidgets('displays app bar with user info', (WidgetTester tester) async {
      await tester.pumpWidget(
        MultiProvider(
          providers: [
            ChangeNotifierProvider(create: (_) => ThemeProvider()),
            ChangeNotifierProvider(create: (_) => MockQueueNotifier()),
          ],
          child: const MaterialApp(
            home: SalonFinderScreen(),
          ),
        ),
      );

      await tester.pumpAndSettle();

      // Check for app bar elements
      expect(find.byType(AppBar), findsOneWidget);
      expect(find.text('1'), findsOneWidget); // User position
      expect(find.text('Q2'), findsOneWidget); // Queue position
    });

    testWidgets('has navigation buttons', (WidgetTester tester) async {
      await tester.pumpWidget(
        MultiProvider(
          providers: [
            ChangeNotifierProvider(create: (_) => ThemeProvider()),
            ChangeNotifierProvider(create: (_) => MockQueueNotifier()),
          ],
          child: const MaterialApp(
            home: SalonFinderScreen(),
          ),
        ),
      );

      await tester.pumpAndSettle();

      // Check for navigation buttons
      expect(find.byIcon(Icons.tv), findsOneWidget);
      expect(find.byIcon(Icons.notifications_outlined), findsOneWidget);
    });

    testWidgets('displays salon containers', (WidgetTester tester) async {
      await tester.pumpWidget(
        MultiProvider(
          providers: [
            ChangeNotifierProvider(create: (_) => ThemeProvider()),
            ChangeNotifierProvider(create: (_) => MockQueueNotifier()),
          ],
          child: const MaterialApp(
            home: SalonFinderScreen(),
          ),
        ),
      );

      await tester.pumpAndSettle();

      // Check for salon containers (they use Container, not Card)
      expect(find.byType(Container), findsWidgets);
      
      // Check for specific salon names
      expect(find.text('Market at Mirada'), findsOneWidget);
      expect(find.text('Cortez Commons'), findsOneWidget);
    });

    testWidgets('has search functionality', (WidgetTester tester) async {
      await tester.pumpWidget(
        MultiProvider(
          providers: [
            ChangeNotifierProvider(create: (_) => ThemeProvider()),
            ChangeNotifierProvider(create: (_) => MockQueueNotifier()),
          ],
          child: const MaterialApp(
            home: SalonFinderScreen(),
          ),
        ),
      );

      await tester.pumpAndSettle();

      // Check for search elements
      expect(find.byIcon(Icons.search), findsOneWidget);
    });

    testWidgets('displays find salon card', (WidgetTester tester) async {
      await tester.pumpWidget(
        MultiProvider(
          providers: [
            ChangeNotifierProvider(create: (_) => ThemeProvider()),
            ChangeNotifierProvider(create: (_) => MockQueueNotifier()),
          ],
          child: const MaterialApp(
            home: SalonFinderScreen(),
          ),
        ),
      );

      await tester.pumpAndSettle();

      // Check for the "find salon" card content
      expect(find.text('Encontre um salão próximo'), findsOneWidget);
      expect(find.text('Ver mapa'), findsOneWidget);
    });

    testWidgets('displays salon information', (WidgetTester tester) async {
      await tester.pumpWidget(
        MultiProvider(
          providers: [
            ChangeNotifierProvider(create: (_) => ThemeProvider()),
            ChangeNotifierProvider(create: (_) => MockQueueNotifier()),
          ],
          child: const MaterialApp(
            home: SalonFinderScreen(),
          ),
        ),
      );

      await tester.pumpAndSettle();

      // Check for salon status
      expect(find.text('Aberto'), findsWidgets);
      
      // Check for wait time information
      expect(find.text('24 min'), findsOneWidget);
      expect(find.text('8 min'), findsOneWidget);
    });
  });
} 