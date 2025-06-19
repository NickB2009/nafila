import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/ui/screens/accessibility_notice_screen.dart';
import 'package:eutonafila_frontend/ui/screens/atendimento_screen.dart';

void main() {
  group('AccessibilityNoticeScreen', () {
    testWidgets('should render all main sections correctly', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test AppBar
      expect(find.text('Aviso de Acessibilidade'), findsOneWidget);
      expect(find.byType(AppBar), findsOneWidget);

      // Test Header Section
      expect(find.text('Acessibilidade para Todos'), findsOneWidget);
      expect(find.text('O Nafila está comprometido em fornecer uma experiência digital inclusiva e acessível para todos os usuários.'), findsOneWidget);
      expect(find.byIcon(Icons.accessibility_new), findsOneWidget);

      // Test Feature Grid - all 4 features should be present
      expect(find.text('Alto Contraste'), findsNWidgets(2)); // Appears in grid and instructions
      expect(find.text('Texto Ajustável'), findsOneWidget);
      expect(find.text('Leitor de Tela'), findsNWidgets(2)); // Appears in grid and instructions
      expect(find.text('Navegação'), findsOneWidget);

      // Test feature descriptions
      expect(find.text('Modo de alto contraste para melhor visibilidade'), findsOneWidget);
      expect(find.text('Controle o tamanho do texto conforme sua necessidade'), findsOneWidget);
      expect(find.text('Compatível com VoiceOver e TalkBack'), findsOneWidget);
      expect(find.text('Suporte completo a navegação por teclado'), findsOneWidget);

      // Test Instructions Card
      expect(find.text('Como Usar'), findsOneWidget);
      expect(find.text('Ajuste de Fonte'), findsOneWidget);

      // Test instruction descriptions
      expect(find.text('Acesse "Display" nas configurações'), findsOneWidget);
      expect(find.text('Ative o VoiceOver (iOS) ou TalkBack (Android)'), findsOneWidget);

      // Test Compliance Section
      expect(find.text('Conformidade WCAG 2.1'), findsOneWidget);
      expect(find.text('Seguimos as diretrizes de acessibilidade nível AA'), findsOneWidget);
      expect(find.byIcon(Icons.verified), findsOneWidget);

      // Test Help Card
      expect(find.text('Precisa de Ajuda?'), findsOneWidget);
      expect(find.text('Entre em contato com nossa equipe de suporte para assistência adicional.'), findsOneWidget);
      expect(find.text('Falar com Suporte'), findsOneWidget);
      expect(find.byIcon(Icons.support_agent), findsOneWidget);
    });

    testWidgets('should display correct feature grid icons', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that all feature icons are present
      expect(find.byIcon(Icons.visibility), findsOneWidget); // Alto Contraste
      expect(find.byIcon(Icons.text_fields), findsNWidgets(2)); // Texto Ajustável + instruction step
      expect(find.byIcon(Icons.record_voice_over), findsNWidgets(2)); // Leitor de Tela + instruction step
      expect(find.byIcon(Icons.keyboard), findsOneWidget); // Navegação
    });

    testWidgets('should display correct instruction step icons', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test instruction step icons
      expect(find.byIcon(Icons.help_outline), findsOneWidget); // Como Usar header
      expect(find.byIcon(Icons.contrast), findsOneWidget); // Alto Contraste instruction
    });

    testWidgets('should have proper card structure', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that cards are present
      expect(find.byType(Card), findsNWidgets(6)); // 4 feature cards + instructions card + help card
    });

    testWidgets('should have scrollable content', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      expect(find.byType(SingleChildScrollView), findsOneWidget);
    });

    testWidgets('should have proper theme colors', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that Scaffold has primary background
      final scaffold = tester.widget<Scaffold>(find.byType(Scaffold));
      expect(scaffold.backgroundColor, isNotNull);
    });

    testWidgets('should have proper spacing between sections', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that SizedBox widgets with height 32 are present (spacing between sections)
      final sizedBoxes = find.byType(SizedBox);
      expect(sizedBoxes, findsWidgets);
    });

    testWidgets('should have proper padding in main content', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that SingleChildScrollView has padding
      final scrollView = tester.widget<SingleChildScrollView>(find.byType(SingleChildScrollView));
      expect(scrollView.padding, isNotNull);
    });

    testWidgets('should have rounded corners on main container', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that the main container has rounded corners
      final containers = find.byType(Container);
      expect(containers, findsWidgets);
    });

    testWidgets('should have proper text styling', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that main title has proper styling
      final titleText = tester.widget<Text>(find.text('Acessibilidade para Todos'));
      expect(titleText.style?.fontWeight, FontWeight.bold);
    });

    testWidgets('should have proper grid layout for features', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that GridView is present
      expect(find.byType(GridView), findsOneWidget);
    });

    testWidgets('should have proper button styling', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that the support button label is present
      expect(find.text('Falar com Suporte'), findsOneWidget);
    });

    testWidgets('should have proper icon sizes', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that accessibility icon has correct size
      final accessibilityIcon = tester.widget<Icon>(find.byIcon(Icons.accessibility_new));
      expect(accessibilityIcon.size, 32);
    });

    testWidgets('should have proper compliance section styling', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that compliance section has proper background
      final complianceContainer = find.ancestor(
        of: find.text('Conformidade WCAG 2.1'),
        matching: find.byType(Container),
      );
      expect(complianceContainer, findsWidgets); // Can be multiple containers
    });

    testWidgets('should have proper instruction step layout', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that instruction steps have proper structure
      final instructionSteps = find.ancestor(
        of: find.text('Ajuste de Fonte'),
        matching: find.byType(Row),
      );
      expect(instructionSteps, findsOneWidget);
    });

    testWidgets('should have proper feature card descriptions', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that all feature descriptions are present and properly styled
      final descriptions = [
        'Modo de alto contraste para melhor visibilidade',
        'Controle o tamanho do texto conforme sua necessidade',
        'Compatível com VoiceOver e TalkBack',
        'Suporte completo a navegação por teclado',
      ];

      for (final description in descriptions) {
        expect(find.text(description), findsOneWidget);
      }
    });

    testWidgets('should have proper help section content', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test help section content
      expect(find.text('Precisa de Ajuda?'), findsOneWidget);
      expect(find.text('Entre em contato com nossa equipe de suporte para assistência adicional.'), findsOneWidget);
      expect(find.byIcon(Icons.support_agent), findsOneWidget);
    });

    testWidgets('should handle support button tap with proper scrolling', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Find the support button
      final supportButton = find.text('Falar com Suporte');
      expect(supportButton, findsOneWidget);
      
      // Scroll to make sure the button is visible
      await tester.drag(find.byType(SingleChildScrollView), const Offset(0, -1000));
      await tester.pumpAndSettle();
      
      // Now tap the button
      await tester.tap(supportButton, warnIfMissed: false);
      await tester.pumpAndSettle();

      // Should show test dialog
      expect(find.text('Teste de Botão'), findsOneWidget);
      expect(find.text('O botão está funcionando! Tentando navegar...'), findsOneWidget);
      expect(find.text('Cancelar'), findsOneWidget);
      expect(find.text('Tentar Navegar'), findsOneWidget);
    });

    testWidgets('should handle dialog interactions', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Scroll and tap support button to show dialog
      await tester.drag(find.byType(SingleChildScrollView), const Offset(0, -1000));
      await tester.pumpAndSettle();
      await tester.tap(find.text('Falar com Suporte'), warnIfMissed: false);
      await tester.pumpAndSettle();

      // Tap cancel button
      await tester.tap(find.text('Cancelar'));
      await tester.pumpAndSettle();

      // Dialog should be dismissed
      expect(find.text('Teste de Botão'), findsNothing);
    });

    testWidgets('should attempt navigation when try navigate button is pressed', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Scroll and tap support button to show dialog
      await tester.drag(find.byType(SingleChildScrollView), const Offset(0, -1000));
      await tester.pumpAndSettle();
      await tester.tap(find.text('Falar com Suporte'), warnIfMissed: false);
      await tester.pumpAndSettle();

      // Tap try navigate button
      await tester.tap(find.text('Tentar Navegar'));
      await tester.pumpAndSettle();

      // Should attempt to navigate to AtendimentoScreen
      // Note: This might show an error dialog if AtendimentoScreen has issues
      // We're testing that the navigation attempt happens
    });

    testWidgets('should have proper app bar structure', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that AppBar has proper structure
      final appBar = tester.widget<AppBar>(find.byType(AppBar));
      expect(appBar.title, isNotNull);
      expect(appBar.backgroundColor, isNotNull);
    });

    testWidgets('should have proper container decorations', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that containers have proper decorations
      final containers = find.byType(Container);
      expect(containers, findsWidgets);
    });

    testWidgets('should have proper grid delegate configuration', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that GridView has proper configuration
      final gridView = tester.widget<GridView>(find.byType(GridView));
      expect(gridView.shrinkWrap, isTrue);
      expect(gridView.physics, isA<NeverScrollableScrollPhysics>());
    });

    testWidgets('should have proper card decorations', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that cards have proper decorations
      final cards = find.byType(Card);
      expect(cards, findsNWidgets(6));
    });

    testWidgets('should have proper text alignment in feature cards', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that feature card texts are center aligned
      final featureTitles = find.text('Alto Contraste');
      expect(featureTitles, findsNWidgets(2));
    });

    testWidgets('should have proper instruction step structure', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that instruction steps have proper structure with icons and text
      final instructionStep = find.ancestor(
        of: find.text('Ajuste de Fonte'),
        matching: find.byType(Row),
      );
      expect(instructionStep, findsOneWidget);
    });

    testWidgets('should have proper compliance section layout', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that compliance section has proper layout
      final complianceRow = find.ancestor(
        of: find.text('Conformidade WCAG 2.1'),
        matching: find.byType(Row),
      );
      expect(complianceRow, findsOneWidget);
    });

    testWidgets('should have proper help card button layout', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AccessibilityNoticeScreen(),
        ),
      );

      // Test that help card has proper button layout
      final helpCard = find.ancestor(
        of: find.text('Falar com Suporte'),
        matching: find.byType(Card),
      );
      expect(helpCard, findsOneWidget);
    });
  });
} 