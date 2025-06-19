import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';
import 'package:eutonafila_frontend/ui/screens/atendimento_screen.dart';
import 'package:eutonafila_frontend/theme/app_theme.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

Widget wrapWithProviders(Widget child) {
  return ChangeNotifierProvider(
    create: (_) => ThemeProvider(),
    child: MaterialApp(
      home: child,
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      supportedLocales: const [
        Locale('pt', 'BR'),
        Locale('en', ''),
      ],
    ),
  );
}

void main() {
  group('AtendimentoScreen', () {
    testWidgets('should render basic structure', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AtendimentoScreen(),
        ),
      );

      // Check for basic structure
      expect(find.byType(Scaffold), findsOneWidget);
      expect(find.byType(AppBar), findsOneWidget);
      expect(find.byType(SafeArea), findsWidgets);
      expect(find.byType(ListView), findsOneWidget);
    });

    testWidgets('should have app bar title', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AtendimentoScreen(),
        ),
      );

      expect(find.text('Atendimento ao Cliente'), findsOneWidget);
    });

    testWidgets('should have FAQ section title', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AtendimentoScreen(),
        ),
      );

      expect(find.text('Perguntas Frequentes'), findsOneWidget);
    });

    testWidgets('should have help section title', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AtendimentoScreen(),
        ),
      );

      expect(find.text('Precisa de mais ajuda?'), findsOneWidget);
    });

    testWidgets('should have contact options text (after scroll)', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AtendimentoScreen(),
        ),
      );
      // Scroll to bottom to reveal contact section
      await tester.drag(find.byType(ListView), const Offset(0, -800));
      await tester.pumpAndSettle();
      expect(find.textContaining('Escolha uma opção para falar conosco'), findsOneWidget);
    });

    testWidgets('should have contact buttons (after scroll)', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AtendimentoScreen(),
        ),
      );
      await tester.drag(find.byType(ListView), const Offset(0, -800));
      await tester.pumpAndSettle();
      expect(find.textContaining('Enviar e-mail'), findsOneWidget);
      expect(find.textContaining('WhatsApp'), findsOneWidget);
      expect(find.textContaining('Ligar para suporte'), findsOneWidget);
    });

    testWidgets('should have FAQ questions', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AtendimentoScreen(),
        ),
      );

      expect(find.text('Como faço para entrar na fila de um salão?'), findsOneWidget);
      expect(find.text('Posso cancelar meu check-in?'), findsOneWidget);
      expect(find.text('Como recebo notificações sobre minha vez?'), findsOneWidget);
      expect(find.text('Como adiciono um salão aos favoritos?'), findsOneWidget);
      expect(find.text('O que é o lembrete de corte?'), findsOneWidget);
    });

    testWidgets('should have expansion tiles for FAQ', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AtendimentoScreen(),
        ),
      );

      expect(find.byType(ExpansionTile), findsNWidgets(5));
    });

    testWidgets('should expand FAQ when tapped', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AtendimentoScreen(),
        ),
      );

      // Tap the first FAQ item
      final firstFaqTile = find.byType(ExpansionTile).first;
      await tester.tap(firstFaqTile);
      await tester.pumpAndSettle();

      // Check that the answer is now visible
      expect(find.textContaining('Basta selecionar o salão desejado'), findsOneWidget);
    });

    testWidgets('should have contact button icons (after scroll)', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AtendimentoScreen(),
        ),
      );
      await tester.drag(find.byType(ListView), const Offset(0, -800));
      await tester.pumpAndSettle();
      expect(find.byIcon(Icons.email_outlined), findsOneWidget);
      expect(find.byIcon(Icons.chat), findsOneWidget);
      expect(find.byIcon(Icons.phone), findsOneWidget);
    });

    testWidgets('should have section icons', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AtendimentoScreen(),
        ),
      );

      expect(find.byIcon(Icons.question_answer), findsWidgets);
      expect(find.byIcon(Icons.support_agent), findsOneWidget);
      expect(find.byIcon(Icons.help_outline), findsNWidgets(5));
    });

    testWidgets('should have cards for styling', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AtendimentoScreen(),
        ),
      );

      expect(find.byType(Card), findsWidgets);
    });

    testWidgets('should have proper spacing elements', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AtendimentoScreen(),
        ),
      );

      expect(find.byType(SizedBox), findsWidgets);
      expect(find.byType(Divider), findsOneWidget);
    });

    testWidgets('should be scrollable with ListView', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: const AtendimentoScreen(),
        ),
      );

      expect(find.byType(ListView), findsOneWidget);
    });
  });
} 