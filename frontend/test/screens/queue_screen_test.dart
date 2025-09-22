import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';
import 'package:eutonafila_frontend/ui/screens/queue_screen.dart';
import 'package:eutonafila_frontend/controllers/queue_controller.dart';
import 'package:eutonafila_frontend/services/queue_service.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';

@GenerateMocks([QueueService])

void main() {
  group('QueueScreen', () {
    late MockQueueService mockQueueService;
    late QueueController queueController;

    setUp(() {
      mockQueueService = MockQueueService();
      queueController = QueueController(queueService: mockQueueService);
    });

    Widget createTestWidget([QueueController? controller]) {
      return MaterialApp(
        home: ChangeNotifierProvider<QueueController>.value(
          value: controller ?? queueController,
          child: const QueueScreen(),
        ),
      );
    }

    testWidgets('renders correctly with all main UI elements', (WidgetTester tester) async {
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Check main screen elements
      expect(find.text('Gerenciamento de Fila'), findsOneWidget);
      expect(find.byIcon(Icons.refresh), findsOneWidget);
      expect(find.byType(FloatingActionButton), findsOneWidget);
    });

    testWidgets('shows empty state when queue is empty', (WidgetTester tester) async {
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Check empty state
      expect(find.text('Ninguém na fila'), findsOneWidget);
      expect(find.text('Toque no botão + para adicionar alguém'), findsOneWidget);
    });

    testWidgets('add person button shows dialog', (WidgetTester tester) async {
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Tap add person button
      await tester.tap(find.byType(FloatingActionButton));
      await tester.pumpAndSettle();

      // Check dialog appears
      expect(find.text('Adicionar Pessoa à Fila'), findsOneWidget);
      expect(find.text('Nome'), findsOneWidget);
      expect(find.text('Cancelar'), findsOneWidget);
      expect(find.text('Adicionar'), findsOneWidget);
    });

    testWidgets('can cancel adding person', (WidgetTester tester) async {
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Tap add person button
      await tester.tap(find.byType(FloatingActionButton));
      await tester.pumpAndSettle();

      // Tap cancel button
      await tester.tap(find.text('Cancelar'));
      await tester.pumpAndSettle();

      // Verify dialog is closed
      expect(find.text('Adicionar Pessoa à Fila'), findsNothing);
    });

    testWidgets('loading state is handled correctly', (WidgetTester tester) async {
      // Set loading state
      queueController._isLoading = true;
      queueController.notifyListeners();

      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump(); // Don't use pumpAndSettle for loading state

      // Check loading indicator
      expect(find.byType(CircularProgressIndicator), findsOneWidget);
    });

    testWidgets('all UI elements are accessible', (WidgetTester tester) async {
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Check all main UI elements are present
      expect(find.text('Gerenciamento de Fila'), findsOneWidget);
      expect(find.byIcon(Icons.refresh), findsOneWidget);
      expect(find.byType(FloatingActionButton), findsOneWidget);
      expect(find.byType(AppBar), findsOneWidget);
    });

    testWidgets('floating action button has correct tooltip', (WidgetTester tester) async {
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Check floating action button tooltip
      expect(find.byTooltip('Adicionar Pessoa'), findsOneWidget);
    });

    testWidgets('app bar has correct title and actions', (WidgetTester tester) async {
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Check app bar elements
      expect(find.text('Gerenciamento de Fila'), findsOneWidget);
      expect(find.byIcon(Icons.refresh), findsOneWidget);
    });
  });
}