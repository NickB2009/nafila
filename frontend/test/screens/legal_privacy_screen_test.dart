import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/ui/screens/legal_privacy_screen.dart';

void main() {
  group('LegalPrivacyScreen', () {
    testWidgets('renders app bar with correct title', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const LegalPrivacyScreen(),
        ),
      );

      expect(find.text('Legal e Privacidade'), findsOneWidget);
    });

    testWidgets('renders header section with icon and description', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const LegalPrivacyScreen(),
        ),
      );

      expect(find.text('Informações Legais'), findsOneWidget);
      expect(find.byIcon(Icons.gavel), findsOneWidget);
      expect(
        find.textContaining('Esta página contém informações importantes'),
        findsOneWidget,
      );
    });

    testWidgets('renders privacy policy section with all items', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const LegalPrivacyScreen(),
        ),
      );

      expect(find.text('Política de Privacidade'), findsOneWidget);
      expect(find.byIcon(Icons.privacy_tip), findsOneWidget);
      expect(find.text('Coleta de Dados'), findsOneWidget);
      expect(find.text('Segurança'), findsOneWidget);
      expect(find.text('Compartilhamento'), findsOneWidget);
      expect(find.text('Ler Política Completa'), findsOneWidget);
      expect(find.byIcon(Icons.data_usage), findsAtLeastNWidgets(1));
      expect(find.byIcon(Icons.security), findsOneWidget);
      expect(find.byIcon(Icons.share), findsAtLeastNWidgets(1));
    });

    testWidgets('renders terms of service section with all items', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const LegalPrivacyScreen(),
        ),
      );

      expect(find.text('Termos de Uso'), findsOneWidget);
      expect(find.byIcon(Icons.description), findsOneWidget);
      expect(find.text('Uso Aceitável'), findsOneWidget);
      expect(find.text('Uso Proibido'), findsOneWidget);
      expect(find.text('Responsabilidades'), findsOneWidget);
      expect(find.text('Ler Termos Completos'), findsOneWidget);
      expect(find.byIcon(Icons.check_circle), findsOneWidget);
      expect(find.byIcon(Icons.block), findsOneWidget);
      expect(find.byIcon(Icons.cancel), findsOneWidget);
    });

    testWidgets('renders data protection section with all items', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const LegalPrivacyScreen(),
        ),
      );

      expect(find.text('Proteção de Dados (LGPD)'), findsOneWidget);
      expect(find.byIcon(Icons.shield), findsOneWidget);
      expect(find.text('Seus Direitos'), findsOneWidget);
      expect(find.text('Consentimento'), findsOneWidget);
      expect(find.text('Exclusão'), findsOneWidget);
      expect(find.text('Solicitar Meus Dados'), findsOneWidget);
      expect(find.byIcon(Icons.person), findsOneWidget);
      expect(find.byIcon(Icons.check_circle_outline), findsOneWidget);
      expect(find.byIcon(Icons.delete_forever), findsOneWidget);
    });

    testWidgets('renders contact section with all items', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const LegalPrivacyScreen(),
        ),
      );

      expect(find.text('Contato'), findsOneWidget);
      expect(find.byIcon(Icons.contact_support), findsOneWidget);
      expect(find.text('Email'), findsOneWidget);
      expect(find.text('legal@nafila.com'), findsOneWidget);
      expect(find.text('Telefone'), findsOneWidget);
      expect(find.text('+55 (11) 99999-9999'), findsOneWidget);
      expect(find.text('Endereço'), findsOneWidget);
      expect(find.text('São Paulo, SP - Brasil'), findsOneWidget);
      expect(find.byIcon(Icons.email), findsOneWidget);
      expect(find.byIcon(Icons.phone), findsOneWidget);
      expect(find.byIcon(Icons.location_on), findsOneWidget);
    });

    testWidgets('renders version info section', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const LegalPrivacyScreen(),
        ),
      );

      expect(find.text('Versão do Aplicativo'), findsOneWidget);
      expect(find.text('1.0.0'), findsOneWidget);
      expect(find.byIcon(Icons.info_outline), findsOneWidget);
      expect(find.textContaining('Última atualização:'), findsOneWidget);
    });

    testWidgets('has correct visual structure with cards', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const LegalPrivacyScreen(),
        ),
      );

      // Check for multiple cards (privacy, terms, data protection, contact, version)
      expect(find.byType(Card), findsNWidgets(5));
      
      // Check for scrollable content
      expect(find.byType(SingleChildScrollView), findsOneWidget);
      
      // Check for proper spacing
      expect(find.byType(SizedBox), findsAtLeastNWidgets(5));
    });

    testWidgets('contact items have proper tap indicators', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const LegalPrivacyScreen(),
        ),
      );

      // Check for arrow icons on tappable contact items
      final arrowIcons = find.byIcon(Icons.arrow_forward_ios);
      expect(arrowIcons, findsNWidgets(2)); // Email and phone have arrows
    });

    testWidgets('all buttons are present and tappable', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const LegalPrivacyScreen(),
        ),
      );

      // Check for all buttons
      expect(find.text('Ler Política Completa'), findsOneWidget);
      expect(find.text('Ler Termos Completos'), findsOneWidget);
      expect(find.text('Solicitar Meus Dados'), findsOneWidget);
    });

    testWidgets('privacy policy items have correct descriptions', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const LegalPrivacyScreen(),
        ),
      );

      expect(
        find.textContaining('Coletamos apenas dados necessários'),
        findsOneWidget,
      );
      expect(
        find.textContaining('Seus dados são protegidos com criptografia'),
        findsOneWidget,
      );
      expect(
        find.textContaining('Não compartilhamos seus dados pessoais'),
        findsOneWidget,
      );
    });

    testWidgets('terms of service items have correct descriptions', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const LegalPrivacyScreen(),
        ),
      );

      expect(
        find.textContaining('O aplicativo deve ser usado apenas'),
        findsOneWidget,
      );
      expect(
        find.textContaining('É proibido usar o aplicativo para atividades ilegais'),
        findsOneWidget,
      );
      expect(
        find.textContaining('O usuário é responsável pela veracidade'),
        findsOneWidget,
      );
    });

    testWidgets('LGPD items have correct descriptions', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const LegalPrivacyScreen(),
        ),
      );

      expect(
        find.textContaining('Você tem direito de acessar, corrigir'),
        findsOneWidget,
      );
      expect(
        find.textContaining('Seu consentimento é necessário'),
        findsOneWidget,
      );
      expect(
        find.textContaining('Você pode solicitar a exclusão'),
        findsOneWidget,
      );
    });
  });
} 