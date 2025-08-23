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

      // Check for main heading greeting on hero section
      expect(find.textContaining('Transforme seu'), findsOneWidget);
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

      // Check for sliver app bar elements
      expect(find.byType(SliverAppBar), findsOneWidget);
      expect(find.text('1'), findsOneWidget); // User indicator
      expect(find.text('Q0'), findsOneWidget); // Queue position badge when no active queues
    });

    testWidgets('has navigation entry to map', (WidgetTester tester) async {
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

      // Check for map CTA in section header
      expect(find.text('Ver mapa'), findsWidgets);
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
      expect(find.text('Barbearia Moderna'), findsOneWidget);
      expect(find.text('Studio Hair'), findsOneWidget);
      expect(find.text('Barbearia Clássica'), findsOneWidget);
    });

    testWidgets('has section headers', (WidgetTester tester) async {
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

      // Check for section header elements
      expect(find.text('Salões mais próximos'), findsOneWidget);
      expect(find.text('Tempo real'), findsOneWidget);
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

      // Check for the "find salon" card content (updated labels)
      expect(find.text('Explorar mais salões'), findsOneWidget);
      expect(find.text('Ver mapa interativo'), findsOneWidget);
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

      // Check for wait time information
      expect(find.text('25 min'), findsOneWidget);
      expect(find.text('35 min'), findsOneWidget);
      expect(find.text('20 min'), findsOneWidget);
    });

    testWidgets('shows check-in buttons for salons', (WidgetTester tester) async {
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

      // Check for check-in buttons
      expect(find.text('Check-in'), findsAtLeastNWidgets(3));
      expect(find.text('Fazer check-in'), findsAtLeastNWidgets(1));
    });

    testWidgets('displays queue information', (WidgetTester tester) async {
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

      // Check for queue information
      expect(find.text('3 fila'), findsOneWidget);
      expect(find.text('2 fila'), findsOneWidget);
      expect(find.text('4 fila'), findsOneWidget);
    });

    testWidgets('shows salon addresses', (WidgetTester tester) async {
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

      // Check for salon addresses
      expect(find.text('Rua das Flores, 123'), findsOneWidget);
      expect(find.text('Av. Paulista, 456'), findsOneWidget);
      expect(find.text('Rua Augusta, 789'), findsOneWidget);
    });
  });
} 