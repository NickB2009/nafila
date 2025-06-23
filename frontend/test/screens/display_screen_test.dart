import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';
import 'package:eutonafila_frontend/ui/screens/display_screen.dart';
import 'package:eutonafila_frontend/theme/app_theme.dart';

void main() {
  group('DisplayScreen', () {
    late ThemeProvider themeProvider;

    setUp(() {
      themeProvider = ThemeProvider();
    });

    Widget buildTestable({ThemeProvider? provider}) {
      return ChangeNotifierProvider<ThemeProvider>.value(
        value: provider ?? themeProvider,
        child: MaterialApp(
          home: const DisplayScreen(),
        ),
      );
    }

    testWidgets('renders all main UI elements', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());

      expect(find.text('Display'), findsOneWidget);
      expect(find.textContaining('Personalize a aparência'), findsOneWidget);
      expect(find.text('Tema'), findsOneWidget);
      expect(find.text('Tamanho da Fonte'), findsOneWidget);
      expect(find.text('Modo alto contraste'), findsOneWidget);
      expect(find.text('Animações'), findsOneWidget);
      expect(find.text('Pré-visualização'), findsOneWidget);
      expect(find.text('Exemplo de visualização'), findsOneWidget);
      expect(find.text('Veja como suas preferências afetam a aparência do app.'), findsOneWidget);
    });

    testWidgets('theme chips reflect provider state and can be tapped', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());

      // Initially, 'Claro' should be selected
      final claroChip = find.widgetWithText(ChoiceChip, 'Claro');
      expect(tester.widget<ChoiceChip>(claroChip).selected, isTrue);

      // Tap 'Escuro' chip
      final escuroChip = find.widgetWithText(ChoiceChip, 'Escuro');
      await tester.tap(escuroChip);
      await tester.pumpAndSettle();
      expect(themeProvider.themeMode, ThemeMode.dark);
      expect(tester.widget<ChoiceChip>(escuroChip).selected, isTrue);

      // Tap 'Sistema' chip
      final sistemaChip = find.widgetWithText(ChoiceChip, 'Sistema');
      await tester.tap(sistemaChip);
      await tester.pumpAndSettle();
      expect(themeProvider.themeMode, ThemeMode.system);
      expect(tester.widget<ChoiceChip>(sistemaChip).selected, isTrue);
    });

    testWidgets('font size chips reflect provider state and can be tapped', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());

      // Initially, 'Médio' should be selected
      final medioChip = find.widgetWithText(ChoiceChip, 'Médio');
      expect(tester.widget<ChoiceChip>(medioChip).selected, isTrue);

      // Tap 'Pequeno' chip
      final pequenoChip = find.widgetWithText(ChoiceChip, 'Pequeno');
      await tester.tap(pequenoChip);
      await tester.pumpAndSettle();
      expect(themeProvider.fontSize, 0.9);
      expect(tester.widget<ChoiceChip>(pequenoChip).selected, isTrue);

      // Tap 'Grande' chip
      final grandeChip = find.widgetWithText(ChoiceChip, 'Grande');
      await tester.tap(grandeChip);
      await tester.pumpAndSettle();
      expect(themeProvider.fontSize, 1.2);
      expect(tester.widget<ChoiceChip>(grandeChip).selected, isTrue);
    });

    testWidgets('high contrast switch toggles provider state', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      final switchFinder = find.widgetWithText(SwitchListTile, 'Modo alto contraste');
      expect(themeProvider.highContrast, isFalse);
      await tester.tap(switchFinder);
      await tester.pumpAndSettle();
      expect(themeProvider.highContrast, isTrue);
    });

    testWidgets('animations switch toggles provider state', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      final switchFinder = find.widgetWithText(SwitchListTile, 'Animações');
      expect(themeProvider.animations, isTrue);
      await tester.tap(switchFinder);
      await tester.pumpAndSettle();
      expect(themeProvider.animations, isFalse);
    });

    testWidgets('preview card updates with high contrast and font size', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      // Default: not high contrast, font size 1.0
      expect(
        tester.widget<Text>(find.text('Exemplo de visualização')).style?.color,
        isNot(equals(Colors.yellow)),
      );
      // Enable high contrast
      themeProvider.setHighContrast(true);
      await tester.pumpAndSettle();
      expect(
        tester.widget<Text>(find.text('Exemplo de visualização')).style?.color,
        equals(Colors.yellow),
      );
      // Change font size
      themeProvider.setFontSize(1.2);
      await tester.pumpAndSettle();
      expect(
        tester.widget<Text>(find.text('Exemplo de visualização')).style?.fontSize,
        closeTo(18 * 1.2, 0.01),
      );
    });
  });
} 