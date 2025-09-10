import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';
import 'package:eutonafila_frontend/ui/screens/register_screen.dart';
import 'package:eutonafila_frontend/ui/screens/login_screen.dart';
import 'package:eutonafila_frontend/controllers/app_controller.dart';
import 'package:eutonafila_frontend/services/auth_service.dart';
import 'package:eutonafila_frontend/services/api_client.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Widget integration tests for the new phone number authentication screens
/// These tests verify the UI validation and user interaction flow
void main() {
  group('Authentication Screens Integration Tests', () {
    late AppController appController;

  setUpAll(() async {
    SharedPreferences.setMockInitialValues({});
    appController = AppController();
    await appController.initialize();
  });

    Widget createTestApp(Widget child) {
      return MaterialApp(
        home: ChangeNotifierProvider<AppController>.value(
          value: appController,
          child: child,
        ),
      );
    }

    group('Registration Screen Tests', () {
      testWidgets('should display all required fields for phone number registration', (WidgetTester tester) async {
        await tester.pumpWidget(createTestApp(const RegisterScreen()));
        await tester.pumpAndSettle();

        // Verify all new fields are present
        expect(find.text('Nome Completo'), findsOneWidget);
        expect(find.text('E-mail'), findsOneWidget);
        expect(find.text('Número de Telefone'), findsOneWidget);
        expect(find.text('Senha'), findsOneWidget);
        
        // Verify old fields are not present
        expect(find.text('Usuário'), findsNothing);
        expect(find.text('Confirmar senha'), findsNothing);
        
        // Verify password requirements helper text
        expect(find.text('Mín. 8 caracteres, 1 maiúscula, 1 minúscula, 1 número'), findsOneWidget);
        
        // Verify terms checkbox
        expect(find.text('Eu li e aceito os termos de uso e privacidade'), findsOneWidget);
        
        // Verify create account button
        expect(find.text('Criar conta'), findsOneWidget);
      });

      testWidgets('should validate full name field correctly', (WidgetTester tester) async {
        await tester.pumpWidget(createTestApp(const RegisterScreen()));
        await tester.pumpAndSettle();

        final fullNameField = find.byType(TextFormField).first;
        
        // Test empty full name
        await tester.tap(fullNameField);
        await tester.enterText(fullNameField, '');
        await tester.tap(find.text('Criar conta'));
        await tester.pumpAndSettle();
        
        expect(find.text('Nome completo é obrigatório'), findsOneWidget);
        
        // Test single name (should fail)
        await tester.enterText(fullNameField, 'João');
        await tester.tap(find.text('Criar conta'));
        await tester.pumpAndSettle();
        
        expect(find.text('Informe seu nome completo'), findsOneWidget);
        
        // Test valid full name
        await tester.enterText(fullNameField, 'João Silva');
        await tester.pumpAndSettle();
        
        // Should not show error for valid name
        expect(find.text('Nome completo é obrigatório'), findsNothing);
        expect(find.text('Informe seu nome completo'), findsNothing);
      });

      testWidgets('should validate phone number field correctly', (WidgetTester tester) async {
        await tester.pumpWidget(createTestApp(const RegisterScreen()));
        await tester.pumpAndSettle();

        // Find phone number field (third TextFormField)
        final phoneFields = find.byType(TextFormField);
        expect(phoneFields, findsNWidgets(4)); // fullName, email, phone, password
        final phoneField = phoneFields.at(2);
        
        // Test empty phone number
        await tester.tap(phoneField);
        await tester.enterText(phoneField, '');
        await tester.tap(find.text('Criar conta'));
        await tester.pumpAndSettle();
        
        expect(find.text('Número de telefone é obrigatório'), findsOneWidget);
        
        // Test invalid phone number (too short)
        await tester.enterText(phoneField, '123');
        await tester.tap(find.text('Criar conta'));
        await tester.pumpAndSettle();
        
        expect(find.text('Número de telefone inválido'), findsOneWidget);
        
        // Test valid phone number
        await tester.enterText(phoneField, '11999999999');
        await tester.pumpAndSettle();
        
        // Should not show error for valid phone
        expect(find.text('Número de telefone é obrigatório'), findsNothing);
        expect(find.text('Número de telefone inválido'), findsNothing);
      });

      testWidgets('should validate password strength in real-time', (WidgetTester tester) async {
        await tester.pumpWidget(createTestApp(const RegisterScreen()));
        await tester.pumpAndSettle();

        // Find password field (fourth TextFormField)
        final passwordField = find.byType(TextFormField).last;
        
        // Test weak password
        await tester.tap(passwordField);
        await tester.enterText(passwordField, 'weak');
        await tester.pumpAndSettle();
        
        // Should show error icon (red)
        expect(find.byIcon(Icons.error), findsOneWidget);
        
        // Test strong password
        await tester.enterText(passwordField, 'StrongPass123!');
        await tester.pumpAndSettle();
        
        // Should show check icon (green)
        expect(find.byIcon(Icons.check_circle), findsOneWidget);
      });

      testWidgets('should require terms acceptance', (WidgetTester tester) async {
        await tester.pumpWidget(createTestApp(const RegisterScreen()));
        await tester.pumpAndSettle();

        // Fill all fields but don't accept terms
        final fields = find.byType(TextFormField);
        await tester.enterText(fields.at(0), 'João Silva');
        await tester.enterText(fields.at(1), 'joao@example.com');
        await tester.enterText(fields.at(2), '11999999999');
        await tester.enterText(fields.at(3), 'StrongPass123!');
        
        // Try to submit without accepting terms
        await tester.tap(find.text('Criar conta'));
        await tester.pumpAndSettle();
        
        expect(find.text('Você precisa aceitar os termos para continuar'), findsOneWidget);
        
        // Accept terms
        await tester.tap(find.byType(Checkbox));
        await tester.pumpAndSettle();
        
        // Error should be gone
        expect(find.text('Você precisa aceitar os termos para continuar'), findsNothing);
      });
    });

    group('Login Screen Tests', () {
      testWidgets('should display phone number login fields', (WidgetTester tester) async {
        await tester.pumpWidget(createTestApp(const LoginScreen()));
        await tester.pumpAndSettle();

        // Verify phone number field is present
        expect(find.text('Número de Telefone'), findsOneWidget);
        expect(find.text('Senha'), findsOneWidget);
        expect(find.text('Entrar'), findsAtLeastNWidgets(1));
        
        // Verify old username field is not present
        expect(find.text('Usuário'), findsNothing);
        
        // Verify demo login button
        expect(find.text('Entrar em modo demo'), findsOneWidget);
        
        // Verify create account link
        expect(find.text('Criar conta'), findsOneWidget);
      });

      testWidgets('should validate phone number login field', (WidgetTester tester) async {
        await tester.pumpWidget(createTestApp(const LoginScreen()));
        await tester.pumpAndSettle();

        final phoneField = find.byType(TextFormField).first;
        
        // Test empty phone number
        await tester.tap(phoneField);
        await tester.enterText(phoneField, '');
        await tester.tap(find.text('Entrar').first);
        await tester.pumpAndSettle();
        
        expect(find.text('Informe o número de telefone'), findsOneWidget);
        
        // Test valid phone number
        await tester.enterText(phoneField, '11999999999');
        await tester.pumpAndSettle();
        
        expect(find.text('Informe o número de telefone'), findsNothing);
      });

      testWidgets('should handle demo login with phone number', (WidgetTester tester) async {
        await tester.pumpWidget(createTestApp(const LoginScreen()));
        await tester.pumpAndSettle();

        // Enter a phone number
        final phoneField = find.byType(TextFormField).first;
        await tester.enterText(phoneField, '11999999999');
        
        // Tap demo login
        await tester.tap(find.text('Entrar em modo demo'));
        await tester.pumpAndSettle();
        
        // Should trigger demo login with the entered phone number
        expect(appController.auth.isAuthenticated, isTrue);
        expect(appController.auth.currentPhoneNumber, equals('11999999999'));
      });
    });

    group('Navigation Between Screens', () {
      testWidgets('should navigate from login to registration', (WidgetTester tester) async {
        await tester.pumpWidget(MaterialApp(
          routes: {
            '/login': (context) => ChangeNotifierProvider<AppController>.value(
              value: appController,
              child: const LoginScreen(),
            ),
            '/register': (context) => ChangeNotifierProvider<AppController>.value(
              value: appController,
              child: const RegisterScreen(),
            ),
          },
          initialRoute: '/login',
        ));
        await tester.pumpAndSettle();

        // Tap create account link
        await tester.tap(find.text('Criar conta'));
        await tester.pumpAndSettle();

        // Should navigate to registration screen
        expect(find.text('Nome Completo'), findsOneWidget);
        expect(find.text('Número de Telefone'), findsOneWidget);
      });

      testWidgets('should navigate from registration to login', (WidgetTester tester) async {
        await tester.pumpWidget(MaterialApp(
          routes: {
            '/login': (context) => ChangeNotifierProvider<AppController>.value(
              value: appController,
              child: const LoginScreen(),
            ),
            '/register': (context) => ChangeNotifierProvider<AppController>.value(
              value: appController,
              child: const RegisterScreen(),
            ),
          },
          initialRoute: '/register',
        ));
        await tester.pumpAndSettle();

        // Tap already have account link
        await tester.tap(find.text('Já tenho conta'));
        await tester.pumpAndSettle();

        // Should navigate to login screen
        expect(find.text('Número de Telefone'), findsOneWidget);
        expect(find.text('Entrar'), findsAtLeastNWidgets(1));
      });
    });

    tearDown(() async {
      await appController.auth.logout();
    });
  });
}
