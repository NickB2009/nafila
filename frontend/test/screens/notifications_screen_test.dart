import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/ui/screens/notifications_screen.dart';

void main() {
  group('NotificationsScreen', () {
    testWidgets('renders app bar with correct elements', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const NotificationsScreen(),
        ),
      );

      // Check back button
      expect(find.byIcon(Icons.arrow_back_ios_new), findsOneWidget);
      
      // Check avatar and count
      expect(find.byIcon(Icons.person), findsOneWidget);
      expect(find.text('1'), findsOneWidget);
      
      // Check edit button
      expect(find.text('Editar'), findsOneWidget);
    });

    testWidgets('renders header with correct title', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const NotificationsScreen(),
        ),
      );

      expect(find.text('Notificações'), findsOneWidget);
    });

    testWidgets('renders all notifications with correct content', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const NotificationsScreen(),
        ),
      );

      // Check all notification titles
      expect(find.text('Dois cortes = R\$5 de desconto*'), findsOneWidget);
      expect(find.text('Quer R\$2 de desconto? Oferta exclusiva para você*!'), findsOneWidget);
      expect(find.text('Fazendo você ficar incrível, esse é nosso objetivo.'), findsOneWidget);
      
      // Check all notification dates
      expect(find.text('14 de maio de 2025'), findsOneWidget);
      expect(find.text('8 de maio de 2025'), findsOneWidget);
      expect(find.text('6 de maio de 2025'), findsOneWidget);
    });

    testWidgets('renders notification icons and chevrons', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const NotificationsScreen(),
        ),
      );

      // Check for notification icons (3 notifications)
      expect(find.byIcon(Icons.notifications), findsNWidgets(3));
      
      // Check for chevron icons (3 notifications)
      expect(find.byIcon(Icons.chevron_right), findsNWidgets(3));
    });

    testWidgets('tapping back button pops the route', (WidgetTester tester) async {
      final observer = _MockNavigatorObserver();
      await tester.pumpWidget(
        MaterialApp(
          home: const NotificationsScreen(),
          navigatorObservers: [observer],
        ),
      );

      await tester.tap(find.byIcon(Icons.arrow_back_ios_new));
      await tester.pumpAndSettle();
      
      expect(observer.didPopCalled, isTrue);
    });

    testWidgets('edit button is tappable', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const NotificationsScreen(),
        ),
      );

      final editButton = find.text('Editar');
      expect(editButton, findsOneWidget);
      
      // Tap the edit button (it has an empty onPressed for now)
      await tester.tap(editButton);
      await tester.pumpAndSettle();
      
      // No error should occur
    });

    testWidgets('notification list items are tappable', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const NotificationsScreen(),
        ),
      );

      // Find all ListTile widgets
      final listTiles = find.byType(ListTile);
      expect(listTiles, findsNWidgets(3));
      
      // Tap the first notification
      await tester.tap(listTiles.first);
      await tester.pumpAndSettle();
      
      // No error should occur (onTap is empty for now)
    });

    testWidgets('has correct visual structure', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const NotificationsScreen(),
        ),
      );

      // Check for main structure
      expect(find.byType(Scaffold), findsOneWidget);
      expect(find.byType(AppBar), findsOneWidget);
      expect(find.byType(SafeArea), findsAtLeastNWidgets(1));
      expect(find.byType(ListView), findsOneWidget);
      
      // Check for dividers between notifications
      expect(find.byType(Divider), findsNWidgets(2)); // 3 items = 2 dividers
    });

    testWidgets('app bar has correct styling', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const NotificationsScreen(),
        ),
      );

      final appBar = tester.widget<AppBar>(find.byType(AppBar));
      expect(appBar.elevation, equals(0));
      expect(appBar.backgroundColor, isNotNull);
    });

    testWidgets('notification items have correct styling', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const NotificationsScreen(),
        ),
      );

      // Check that all ListTiles have the correct structure
      final listTiles = find.byType(ListTile);
      expect(listTiles, findsNWidgets(3));
      
      // Check that each ListTile has leading, title, subtitle, and trailing
      for (int i = 0; i < 3; i++) {
        final listTile = tester.widget<ListTile>(listTiles.at(i));
        expect(listTile.leading, isNotNull);
        expect(listTile.title, isNotNull);
        expect(listTile.subtitle, isNotNull);
        expect(listTile.trailing, isNotNull);
      }
    });

    testWidgets('avatar container has correct styling', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const NotificationsScreen(),
        ),
      );

      // Find the avatar container in the app bar
      final avatarContainer = find.descendant(
        of: find.byType(AppBar),
        matching: find.byType(Container),
      ).first;
      
      final container = tester.widget<Container>(avatarContainer);
      expect(container.decoration, isNotNull);
    });

    testWidgets('notification icons have correct styling', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const NotificationsScreen(),
        ),
      );

      // Find notification icon containers
      final iconContainers = find.descendant(
        of: find.byType(ListTile),
        matching: find.byType(Container),
      );
      
      expect(iconContainers, findsNWidgets(3));
      
      // Check that each container has decoration
      for (int i = 0; i < 3; i++) {
        final container = tester.widget<Container>(iconContainers.at(i));
        expect(container.decoration, isNotNull);
      }
    });

    testWidgets('list has proper padding and spacing', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const NotificationsScreen(),
        ),
      );

      // Check for SizedBox widgets that provide spacing
      expect(find.byType(SizedBox), findsAtLeastNWidgets(3));
      
      // Check for Padding widgets
      expect(find.byType(Padding), findsAtLeastNWidgets(2));
    });
  });
}

// Mock navigator observer for testing navigation
class _MockNavigatorObserver extends NavigatorObserver {
  bool didPopCalled = false;
  
  @override
  void didPop(Route route, Route? previousRoute) {
    didPopCalled = true;
    super.didPop(route, previousRoute);
  }
} 