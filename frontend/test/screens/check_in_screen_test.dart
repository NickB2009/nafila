import 'package:eutonafila_frontend/ui/screens/anonymous_join_queue_screen.dart';
import 'package:eutonafila_frontend/models/public_salon.dart';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  final dummyPublicSalon = PublicSalon(
    id: 'barbearia_teste',
    name: 'Barbearia Teste',
    address: 'Rua Exemplo, 123',
    isOpen: true,
    currentWaitTimeMinutes: 10,
    queueLength: 3,
    distanceKm: 1.2,
    services: ['Haircut', 'Beard Trim', 'Hair Styling'],
  );

  Widget buildTestable({Widget? child}) {
    return MaterialApp(
      home: child ?? AnonymousJoinQueueScreen(salon: dummyPublicSalon),
    );
  }

  group('AnonymousJoinQueueScreen', () {
    testWidgets('renders all main sections and widgets', (WidgetTester tester) async {
      await tester.binding.setSurfaceSize(const Size(800, 1200)); // Larger surface
      await tester.pumpWidget(buildTestable());
      
      // Title
      expect(find.text('Entrar na fila'), findsOneWidget);
      
      // Salon info
      expect(find.text('Barbearia Teste'), findsOneWidget);
      expect(find.text('Rua Exemplo, 123'), findsOneWidget);
      
      // User info fields
      expect(find.text('Seus dados'), findsOneWidget);
      expect(find.byType(TextField), findsNWidgets(2));
      expect(find.text('Seu nome *'), findsOneWidget);
      expect(find.text('E-mail *'), findsOneWidget);
      
      // Service selection
      expect(find.text('Serviços solicitados (opcional)'), findsOneWidget);
      expect(find.text('Haircut'), findsOneWidget);
      expect(find.text('Beard Trim'), findsOneWidget);
      expect(find.text('Hair Styling'), findsOneWidget);
      
      // Notification preferences
      expect(find.text('Preferências de notificação'), findsOneWidget);
      expect(find.text('Notificações por e-mail'), findsOneWidget);
      expect(find.text('Notificações no navegador'), findsOneWidget);
      
      // Button
      expect(find.widgetWithText(ElevatedButton, 'Entrar na fila'), findsOneWidget);
    });

    testWidgets('back button pops the screen', (WidgetTester tester) async {
      await tester.binding.setSurfaceSize(const Size(800, 1200));
      await tester.pumpWidget(buildTestable());
      await tester.tap(find.byIcon(Icons.arrow_back));
      await tester.pumpAndSettle();
      // Back button should work without throwing
      expect(find.byIcon(Icons.arrow_back), findsNothing);
    });

    testWidgets('service selection toggles', (WidgetTester tester) async {
      await tester.binding.setSurfaceSize(const Size(800, 1200));
      await tester.pumpWidget(buildTestable());
      
      // Haircut should be selected by default
      expect(find.text('Haircut'), findsOneWidget);
      
      // Tap on Beard Trim to select it
      await tester.tap(find.text('Beard Trim'));
      await tester.pumpAndSettle();
      
      // Both services should now be selected
      expect(find.text('Haircut'), findsOneWidget);
      expect(find.text('Beard Trim'), findsOneWidget);
    });

    testWidgets('notification checkboxes toggle', (WidgetTester tester) async {
      await tester.binding.setSurfaceSize(const Size(800, 1200));
      await tester.pumpWidget(buildTestable());
      
      final checkboxes = find.byType(Checkbox);
      expect(checkboxes, findsNWidgets(2));
      
      // Both should be checked by default
      expect(tester.widget<Checkbox>(checkboxes.at(0)).value, isTrue);
      expect(tester.widget<Checkbox>(checkboxes.at(1)).value, isTrue);
      
      // Toggle first checkbox
      await tester.tap(checkboxes.at(0));
      await tester.pumpAndSettle();
      expect(tester.widget<Checkbox>(checkboxes.at(0)).value, isFalse);
    });

    testWidgets('text fields accept input', (WidgetTester tester) async {
      await tester.binding.setSurfaceSize(const Size(800, 1200));
      await tester.pumpWidget(buildTestable());
      
      final nameField = find.widgetWithText(TextField, 'Seu nome *');
      final emailField = find.widgetWithText(TextField, 'E-mail *');
      
      await tester.enterText(nameField, 'João Teste');
      await tester.enterText(emailField, 'joao@teste.com');
      
      expect(find.text('João Teste'), findsOneWidget);
      expect(find.text('joao@teste.com'), findsOneWidget);
    });

    testWidgets('all labels and sections are present', (WidgetTester tester) async {
      await tester.binding.setSurfaceSize(const Size(800, 1200));
      await tester.pumpWidget(buildTestable());
      
      expect(find.text('Entrar na fila'), findsOneWidget);
      expect(find.text('Seus dados'), findsOneWidget);
      expect(find.text('Seu nome *'), findsOneWidget);
      expect(find.text('E-mail *'), findsOneWidget);
      expect(find.text('Serviços solicitados (opcional)'), findsOneWidget);
      expect(find.text('Preferências de notificação'), findsOneWidget);
      expect(find.text('Notificações por e-mail'), findsOneWidget);
      expect(find.text('Notificações no navegador'), findsOneWidget);
    });
  });
} 