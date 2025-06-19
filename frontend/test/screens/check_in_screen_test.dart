import '../../lib/ui/screens/check_in_screen.dart';
import '../../lib/ui/screens/check_in_success_screen.dart';
import '../../lib/models/salon.dart';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  final dummySalon = Salon(
    name: 'Barbearia Teste',
    address: 'Rua Exemplo, 123',
    waitTime: 10,
    distance: 1.2,
    isOpen: true,
    closingTime: '18:00',
    isFavorite: false,
    queueLength: 3,
  );

  Widget buildTestable({Widget? child}) {
    return MaterialApp(
      home: child ?? CheckInScreen(salon: dummySalon),
    );
  }

  group('CheckInScreen', () {
    testWidgets('renders all main sections and widgets', (WidgetTester tester) async {
      await tester.binding.setSurfaceSize(const Size(800, 1200)); // Larger surface
      await tester.pumpWidget(buildTestable());
      
      // Title (there are 2 "Check-in" texts - title and button)
      expect(find.text('Check-in'), findsNWidgets(2));
      
      // Salon info
      expect(find.text('Barbearia Teste'), findsOneWidget);
      expect(find.text('Rua Exemplo, 123'), findsOneWidget);
      
      // User info fields
      expect(find.text('Nome completo'), findsOneWidget);
      expect(find.byType(TextField), findsNWidgets(2));
      expect(find.text('Digite seu nome'), findsOneWidget);
      expect(find.text('Digite seu telefone'), findsOneWidget);
      
      // Dropdown
      expect(find.text('Número de pessoas cortando o cabelo'), findsOneWidget);
      expect(find.text('1 pessoa'), findsOneWidget);
      
      // Checkbox
      expect(find.byType(Checkbox), findsOneWidget);
      expect(find.textContaining('Receba uma mensagem avisando'), findsOneWidget);
      
      // Footer
      expect(find.text('Powered by ICS'), findsOneWidget);
      expect(find.text('Net Check In™'), findsOneWidget);
      
      // Button
      expect(find.widgetWithText(ElevatedButton, 'Check-in'), findsOneWidget);
    });

    testWidgets('close button pops the screen', (WidgetTester tester) async {
      await tester.binding.setSurfaceSize(const Size(800, 1200));
      await tester.pumpWidget(buildTestable());
      await tester.tap(find.byIcon(Icons.close));
      await tester.pumpAndSettle();
      // Close button should work without throwing
      expect(find.byIcon(Icons.close), findsNothing);
    });

    testWidgets('dropdown changes value', (WidgetTester tester) async {
      await tester.binding.setSurfaceSize(const Size(800, 1200));
      await tester.pumpWidget(buildTestable());
      await tester.tap(find.text('1 pessoa'));
      await tester.pumpAndSettle();
      await tester.tap(find.text('2 pessoas').last);
      await tester.pumpAndSettle();
      expect(find.text('2 pessoas'), findsOneWidget);
    });

    testWidgets('checkbox toggles', (WidgetTester tester) async {
      await tester.binding.setSurfaceSize(const Size(800, 1200));
      await tester.pumpWidget(buildTestable());
      final checkbox = find.byType(Checkbox);
      expect(tester.widget<Checkbox>(checkbox).value, isTrue);
      await tester.tap(checkbox);
      await tester.pumpAndSettle();
      expect(tester.widget<Checkbox>(checkbox).value, isFalse);
    });

    testWidgets('text fields accept input', (WidgetTester tester) async {
      await tester.binding.setSurfaceSize(const Size(800, 1200));
      await tester.pumpWidget(buildTestable());
      final nameField = find.widgetWithText(TextField, 'Digite seu nome');
      final phoneField = find.widgetWithText(TextField, 'Digite seu telefone');
      await tester.enterText(nameField, 'João Teste');
      await tester.enterText(phoneField, '11999999999');
      expect(find.text('João Teste'), findsOneWidget);
      expect(find.text('11999999999'), findsOneWidget);
    });

    testWidgets('Check-in button navigates to CheckInSuccessScreen', (WidgetTester tester) async {
      await tester.binding.setSurfaceSize(const Size(800, 1200));
      await tester.pumpWidget(buildTestable());
      
      // The button should be visible with the larger surface
      await tester.tap(find.widgetWithText(ElevatedButton, 'Check-in'));
      await tester.pumpAndSettle();
      await tester.pump(const Duration(seconds: 3)); // Let timer finish
      expect(find.byType(CheckInSuccessScreen), findsOneWidget);
    });

    testWidgets('all labels and hints are present', (WidgetTester tester) async {
      await tester.binding.setSurfaceSize(const Size(800, 1200));
      await tester.pumpWidget(buildTestable());
      
      expect(find.text('Nome completo'), findsOneWidget);
      expect(find.text('Digite seu nome'), findsOneWidget);
      expect(find.text('Número de pessoas cortando o cabelo'), findsOneWidget);
      expect(find.text('Telefone'), findsOneWidget);
      expect(find.text('Digite seu telefone'), findsOneWidget);
      expect(find.textContaining('Receba uma mensagem avisando'), findsOneWidget);
      expect(
        find.byWidgetPredicate(
          (widget) => widget is RichText && widget.text.toPlainText().contains('Política de Privacidade'),
        ),
        findsOneWidget,
      );
    });
  });
} 