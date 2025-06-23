import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/ui/screens/comunicacoes_screen.dart';

void main() {
  group('ComunicacoesScreen', () {
    testWidgets('should render with correct app bar', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const ComunicacoesScreen(),
        ),
      );

      // Check app bar title
      expect(find.text('Configurações de Comunicação'), findsOneWidget);
    });

    testWidgets('should display main question text', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const ComunicacoesScreen(),
        ),
      );

      expect(
        find.text('Como você gostaria de receber notificações e atualizações?'),
        findsOneWidget,
      );
    });

    testWidgets('should display email notification switch', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const ComunicacoesScreen(),
        ),
      );

      // Check email switch exists
      expect(find.text('Receber notificações por e-mail'), findsOneWidget);
      expect(find.byIcon(Icons.email_outlined), findsOneWidget);
      
      // Check switch is initially on
      final emailSwitch = tester.widget<SwitchListTile>(
        find.ancestor(
          of: find.text('Receber notificações por e-mail'),
          matching: find.byType(SwitchListTile),
        ),
      );
      expect(emailSwitch.value, isTrue);
    });

    testWidgets('should display SMS notification switch', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const ComunicacoesScreen(),
        ),
      );

      // Check SMS switch exists
      expect(find.text('Receber notificações por SMS'), findsOneWidget);
      expect(find.byIcon(Icons.sms_outlined), findsOneWidget);
      
      // Check switch is initially on
      final smsSwitch = tester.widget<SwitchListTile>(
        find.ancestor(
          of: find.text('Receber notificações por SMS'),
          matching: find.byType(SwitchListTile),
        ),
      );
      expect(smsSwitch.value, isTrue);
    });

    testWidgets('should display help text at bottom', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const ComunicacoesScreen(),
        ),
      );

      expect(
        find.text('Você pode alterar essas preferências a qualquer momento.'),
        findsOneWidget,
      );
    });

    testWidgets('should toggle email switch when tapped', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const ComunicacoesScreen(),
        ),
      );

      // Find email switch and tap it
      final emailSwitch = find.ancestor(
        of: find.text('Receber notificações por e-mail'),
        matching: find.byType(SwitchListTile),
      );
      
      await tester.tap(emailSwitch);
      await tester.pump();

      // Check switch is now off
      final emailSwitchWidget = tester.widget<SwitchListTile>(emailSwitch);
      expect(emailSwitchWidget.value, isFalse);
    });

    testWidgets('should toggle SMS switch when tapped', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const ComunicacoesScreen(),
        ),
      );

      // Find SMS switch and tap it
      final smsSwitch = find.ancestor(
        of: find.text('Receber notificações por SMS'),
        matching: find.byType(SwitchListTile),
      );
      
      await tester.tap(smsSwitch);
      await tester.pump();

      // Check switch is now off
      final smsSwitchWidget = tester.widget<SwitchListTile>(smsSwitch);
      expect(smsSwitchWidget.value, isFalse);
    });

    testWidgets('should maintain state when toggling switches multiple times', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const ComunicacoesScreen(),
        ),
      );

      // Find both switches
      final emailSwitch = find.ancestor(
        of: find.text('Receber notificações por e-mail'),
        matching: find.byType(SwitchListTile),
      );
      final smsSwitch = find.ancestor(
        of: find.text('Receber notificações por SMS'),
        matching: find.byType(SwitchListTile),
      );

      // Toggle email switch off
      await tester.tap(emailSwitch);
      await tester.pump();
      expect(tester.widget<SwitchListTile>(emailSwitch).value, isFalse);

      // Toggle SMS switch off
      await tester.tap(smsSwitch);
      await tester.pump();
      expect(tester.widget<SwitchListTile>(smsSwitch).value, isFalse);

      // Toggle email switch back on
      await tester.tap(emailSwitch);
      await tester.pump();
      expect(tester.widget<SwitchListTile>(emailSwitch).value, isTrue);

      // Toggle SMS switch back on
      await tester.tap(smsSwitch);
      await tester.pump();
      expect(tester.widget<SwitchListTile>(smsSwitch).value, isTrue);
    });

    testWidgets('should have correct visual structure', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const ComunicacoesScreen(),
        ),
      );

      // Check for main container structure
      expect(find.byType(Scaffold), findsOneWidget);
      expect(find.byType(AppBar), findsOneWidget);
      expect(find.byType(SafeArea), findsAtLeastNWidgets(1));
      expect(find.byType(ListView), findsOneWidget);
      expect(find.byType(Card), findsOneWidget);
      
      // Check for divider between switches
      expect(find.byType(Divider), findsOneWidget);
    });

    testWidgets('should have proper spacing between elements', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const ComunicacoesScreen(),
        ),
      );

      // Check for SizedBox widgets that provide spacing (there are more than 2 due to internal widget spacing)
      expect(find.byType(SizedBox), findsAtLeastNWidgets(2));
    });

    testWidgets('should have correct theme colors', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const ComunicacoesScreen(),
        ),
      );

      // Check app bar background color
      final appBar = tester.widget<AppBar>(find.byType(AppBar));
      expect(appBar.backgroundColor, isNotNull);

      // Check card has elevation
      final card = tester.widget<Card>(find.byType(Card));
      expect(card.elevation, equals(2));
    });
  });
} 