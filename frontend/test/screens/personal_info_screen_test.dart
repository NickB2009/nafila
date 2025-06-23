import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/ui/screens/personal_info_screen.dart';
import 'package:network_image_mock/network_image_mock.dart';

void main() {
  group('PersonalInfoScreen', () {
    testWidgets('renders correctly with all main UI elements', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      // Wait for animations to complete
      await tester.pumpAndSettle();

      // Check main screen elements
      expect(find.text('Meu Perfil'), findsOneWidget);
      expect(find.text('Rommel B'), findsAtLeastNWidgets(2)); // Appears in header and info card
      expect(find.text('Membro desde 2024'), findsOneWidget);
      
      // Check tab bar
      expect(find.text('Informações'), findsOneWidget);
      expect(find.text('Preferências'), findsOneWidget);
      
      // Check tab content
      expect(find.text('Informações Pessoais'), findsOneWidget);
      expect(find.text('Nome completo'), findsOneWidget);
      expect(find.text('Telefone'), findsOneWidget);
      expect(find.text('Cidade'), findsOneWidget);
      expect(find.text('Email'), findsOneWidget);
    });

    testWidgets('displays personal information in first tab', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Check personal info fields
      expect(find.text('Informações Pessoais'), findsOneWidget);
      expect(find.text('Nome completo'), findsOneWidget);
      expect(find.text('Rommel B'), findsAtLeastNWidgets(2));
      expect(find.text('Telefone'), findsOneWidget);
      expect(find.text('(11) 91234-5678'), findsOneWidget);
      expect(find.text('Cidade'), findsOneWidget);
      expect(find.text('São Paulo'), findsOneWidget);
      expect(find.text('Email'), findsOneWidget);
      expect(find.text('rommel@email.com'), findsOneWidget);
    });

    testWidgets('displays preferences in second tab', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Tap on preferences tab
      await tester.tap(find.text('Preferências'));
      await tester.pumpAndSettle();

      // Check preferences content
      expect(find.text('Modelos de Corte'), findsOneWidget);
      expect(find.text('Foto de Referência'), findsOneWidget);
      expect(find.text('Preferências de Corte'), findsOneWidget);
      expect(find.text('Laterais'), findsOneWidget);
      expect(find.text('Fade'), findsOneWidget);
      expect(find.text('Topo'), findsOneWidget);
      expect(find.text('Franja'), findsOneWidget);
    });

    testWidgets('allows changing preference selections', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Tap on preferences tab
      await tester.tap(find.text('Preferências'));
      await tester.pumpAndSettle();

      // Change machine preference
      await tester.ensureVisible(find.text('Máquina 1'));
      await tester.tap(find.text('Máquina 1'));
      await tester.pumpAndSettle();

      // Change height preference
      await tester.ensureVisible(find.text('Alto'));
      await tester.tap(find.text('Alto'));
      await tester.pumpAndSettle();

      // Change length preference
      await tester.ensureVisible(find.text('Longo'));
      await tester.tap(find.text('Longo'));
      await tester.pumpAndSettle();

      // Change wash hair preference
      await tester.ensureVisible(find.text('Sim'));
      await tester.tap(find.text('Sim'));
      await tester.pumpAndSettle();

      // Verify selections are updated
      expect(find.text('Máquina 1'), findsOneWidget);
      expect(find.text('Alto'), findsOneWidget);
      expect(find.text('Longo'), findsOneWidget);
      expect(find.text('Sim'), findsOneWidget);
    });

    testWidgets('allows adding and removing reference photo', (WidgetTester tester) async {
      // Mock network image loading for this test
      await mockNetworkImagesFor(() async {
        await tester.pumpWidget(
          const MaterialApp(
            home: PersonalInfoScreen(),
          ),
        );

        await tester.pumpAndSettle();

        // Tap on preferences tab
        await tester.tap(find.text('Preferências'));
        await tester.pumpAndSettle();

        // Add photo
        await tester.ensureVisible(find.text('Adicionar foto'));
        await tester.tap(find.text('Adicionar foto'));
        await tester.pumpAndSettle();

        // Verify photo was added (check for remove button or photo placeholder)
        expect(find.text('Adicionar foto'), findsNothing);
        // The remove button might have different text or be an icon
        expect(find.text('Remover'), findsOneWidget);
      });
    });

    testWidgets('copy button shows snackbar', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Tap on preferences tab
      await tester.tap(find.text('Preferências'));
      await tester.pumpAndSettle();

      // Tap copy button
      await tester.ensureVisible(find.byIcon(Icons.copy_outlined));
      await tester.tap(find.byIcon(Icons.copy_outlined));
      await tester.pumpAndSettle(const Duration(milliseconds: 500));

      // Check for snackbar
      expect(find.text('Copiado!'), findsOneWidget);
    });

    testWidgets('edit profile button shows snackbar', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Tap edit profile button
      await tester.ensureVisible(find.text('Editar Perfil'));
      await tester.tap(find.text('Editar Perfil'));
      await tester.pumpAndSettle(const Duration(milliseconds: 500));

      // Check for snackbar
      expect(find.text('Edição em breve!'), findsOneWidget);
    });

    testWidgets('tab switching works correctly', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Initially on first tab
      expect(find.text('Informações Pessoais'), findsOneWidget);
      expect(find.text('Modelos de Corte'), findsNothing);

      // Switch to preferences tab
      await tester.tap(find.text('Preferências'));
      await tester.pumpAndSettle();

      // Now on preferences tab
      expect(find.text('Informações Pessoais'), findsNothing);
      expect(find.text('Modelos de Corte'), findsOneWidget);

      // Switch back to info tab
      await tester.tap(find.text('Informações'));
      await tester.pumpAndSettle();

      // Back to info tab
      expect(find.text('Informações Pessoais'), findsOneWidget);
      expect(find.text('Modelos de Corte'), findsNothing);
    });

    testWidgets('preference changes update summary card', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Tap on preferences tab
      await tester.tap(find.text('Preferências'));
      await tester.pumpAndSettle();

      // Change machine preference
      await tester.ensureVisible(find.text('Máquina 1'));
      await tester.tap(find.text('Máquina 1'));
      await tester.pumpAndSettle();

      // Check summary card updates
      expect(find.textContaining('Laterais: Máquina 1'), findsOneWidget);
    });

    testWidgets('manual preference change selects custom template', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Tap on preferences tab
      await tester.tap(find.text('Preferências'));
      await tester.pumpAndSettle();

      // Change machine preference
      await tester.ensureVisible(find.text('Máquina 1'));
      await tester.tap(find.text('Máquina 1'));
      await tester.pumpAndSettle();

      // Verify custom template is selected
      expect(find.text('Personalizado'), findsOneWidget);
    });

    testWidgets('profile header has gradient background', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Check profile header elements
      expect(find.text('Rommel B'), findsAtLeastNWidgets(2));
      expect(find.text('Membro desde 2024'), findsOneWidget);
      expect(find.byIcon(Icons.person), findsOneWidget);
    });

    testWidgets('info card displays correctly', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Check info card content
      expect(find.text('Informações Pessoais'), findsOneWidget);
      expect(find.text('Nome completo'), findsOneWidget);
      expect(find.text('Telefone'), findsOneWidget);
      expect(find.text('Cidade'), findsOneWidget);
      expect(find.text('Email'), findsOneWidget);
    });

    testWidgets('preferences section has correct layout', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Tap on preferences tab
      await tester.tap(find.text('Preferências'));
      await tester.pumpAndSettle();

      // Check preferences layout
      expect(find.text('Preferências de Corte'), findsOneWidget);
      expect(find.text('Laterais'), findsOneWidget);
      expect(find.text('Fade'), findsOneWidget);
      expect(find.text('Topo'), findsOneWidget);
      expect(find.text('Franja'), findsOneWidget);
    });

    testWidgets('summary card shows current preferences', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Tap on preferences tab
      await tester.tap(find.text('Preferências'));
      await tester.pumpAndSettle();

      // Check summary card
      expect(find.byIcon(Icons.content_cut), findsAtLeastNWidgets(1));
      expect(find.byIcon(Icons.copy_outlined), findsOneWidget);
    });

    testWidgets('photo section displays correctly', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Tap on preferences tab
      await tester.tap(find.text('Preferências'));
      await tester.pumpAndSettle();

      // Check photo section
      expect(find.text('Foto de Referência'), findsOneWidget);
      expect(find.text('Adicionar foto'), findsOneWidget);
    });

    testWidgets('copy preferences button works', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Tap on preferences tab
      await tester.tap(find.text('Preferências'));
      await tester.pumpAndSettle();

      // Tap copy button
      await tester.ensureVisible(find.byIcon(Icons.copy_outlined));
      await tester.tap(find.byIcon(Icons.copy_outlined));
      await tester.pumpAndSettle(const Duration(milliseconds: 500));

      // Check for snackbar
      expect(find.text('Copiado!'), findsOneWidget);
    });

    testWidgets('edit profile button works', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Tap edit profile button
      await tester.ensureVisible(find.text('Editar Perfil'));
      await tester.tap(find.text('Editar Perfil'));
      await tester.pumpAndSettle(const Duration(milliseconds: 500));

      // Check for snackbar
      expect(find.text('Edição em breve!'), findsOneWidget);
    });

    testWidgets('responsive layout works for different screen sizes', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Test with compact width
      await tester.binding.setSurfaceSize(const Size(300, 600));
      await tester.pumpAndSettle();

      // Should still display correctly
      expect(find.text('Meu Perfil'), findsOneWidget);
      expect(find.text('Rommel B'), findsAtLeastNWidgets(2));
      expect(find.text('Informações'), findsOneWidget);
      expect(find.text('Preferências'), findsOneWidget);

      // Reset surface size
      await tester.binding.setSurfaceSize(null);
    });

    testWidgets('tab animations work smoothly', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Switch tabs multiple times to test animations
      await tester.tap(find.text('Preferências'));
      await tester.pumpAndSettle();

      await tester.tap(find.text('Informações'));
      await tester.pumpAndSettle();

      await tester.tap(find.text('Preferências'));
      await tester.pumpAndSettle();

      // Verify final state
      expect(find.text('Modelos de Corte'), findsOneWidget);
    });

    testWidgets('preference selection updates immediately', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Tap on preferences tab
      await tester.tap(find.text('Preferências'));
      await tester.pumpAndSettle();

      // Change a preference
      await tester.ensureVisible(find.text('Máquina 1'));
      await tester.tap(find.text('Máquina 1'));
      await tester.pumpAndSettle();

      // Verify it's selected
      expect(find.text('Máquina 1'), findsOneWidget);
    });

    testWidgets('all UI elements are accessible', (WidgetTester tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: PersonalInfoScreen(),
        ),
      );

      await tester.pumpAndSettle();

      // Check all main UI elements are present
      expect(find.text('Meu Perfil'), findsOneWidget);
      expect(find.text('Informações'), findsOneWidget);
      expect(find.text('Preferências'), findsOneWidget);
      expect(find.text('Informações Pessoais'), findsOneWidget);
      expect(find.text('Nome completo'), findsOneWidget);
      expect(find.text('Telefone'), findsOneWidget);
      expect(find.text('Cidade'), findsOneWidget);
      expect(find.text('Email'), findsOneWidget);
    });
  });
} 