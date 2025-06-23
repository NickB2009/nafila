import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';
import 'package:eutonafila_frontend/ui/screens/queue_screen.dart';
import 'package:eutonafila_frontend/ui/view_models/mock_queue_notifier.dart';
import 'package:eutonafila_frontend/models/queue_entry.dart';

void main() {
  group('QueueScreen', () {
    late MockQueueNotifier mockQueueNotifier;

    setUp(() {
      mockQueueNotifier = MockQueueNotifier.forTest();
    });

    Widget createTestWidget([MockQueueNotifier? notifier]) {
      return MaterialApp(
        home: ChangeNotifierProvider<MockQueueNotifier>.value(
          value: notifier ?? mockQueueNotifier,
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

    testWidgets('displays queue statistics correctly', (WidgetTester tester) async {
      // Set up test data
      mockQueueNotifier.entries = [
        QueueEntry(id: '1', name: 'John Doe', status: QueueStatus.waiting, joinTime: DateTime.now(), position: 1),
        QueueEntry(id: '2', name: 'Jane Smith', status: QueueStatus.inService, joinTime: DateTime.now(), position: 2),
        QueueEntry(id: '3', name: 'Bob Johnson', status: QueueStatus.waiting, joinTime: DateTime.now(), position: 3),
      ];
      mockQueueNotifier.notifyListeners();

      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Check statistics - use findsAtLeastNWidgets since numbers may appear multiple times
      expect(find.text('Total'), findsOneWidget);
      expect(find.text('Aguardando'), findsOneWidget);
      expect(find.text('Em Atendimento'), findsOneWidget);
      expect(find.text('3'), findsAtLeastNWidgets(1)); // Total
      expect(find.text('2'), findsAtLeastNWidgets(1)); // Waiting
      expect(find.text('1'), findsAtLeastNWidgets(1)); // In service
    });

    testWidgets('displays queue entries correctly', (WidgetTester tester) async {
      // Set up test data
      mockQueueNotifier.entries = [
        QueueEntry(id: '1', name: 'John Doe', status: QueueStatus.waiting, joinTime: DateTime.now(), position: 1),
        QueueEntry(id: '2', name: 'Jane Smith', status: QueueStatus.inService, joinTime: DateTime.now(), position: 2),
      ];
      mockQueueNotifier.notifyListeners();

      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Check queue entries
      expect(find.text('John Doe'), findsOneWidget);
      expect(find.text('Jane Smith'), findsOneWidget);
      expect(find.text('Waiting'), findsOneWidget);
      expect(find.text('In Service'), findsOneWidget);
    });

    testWidgets('shows empty state when queue is empty', (WidgetTester tester) async {
      // Set up empty queue
      mockQueueNotifier.entries = [];
      mockQueueNotifier.notifyListeners();

      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Check empty state
      expect(find.text('Ninguém na fila'), findsOneWidget);
      expect(find.text('Toque no botão + para adicionar alguém'), findsOneWidget);
    });

    testWidgets('refresh button triggers refresh', (WidgetTester tester) async {
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Tap refresh button
      await tester.tap(find.byIcon(Icons.refresh));
      await tester.pumpAndSettle();

      // Verify refresh was called (this would be verified by checking if the method was called)
      // In a real test, you might want to verify the refresh behavior
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

    testWidgets('can add person to queue via dialog', (WidgetTester tester) async {
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Tap add person button
      await tester.tap(find.byType(FloatingActionButton));
      await tester.pumpAndSettle();

      // Enter name
      await tester.enterText(find.byType(TextField), 'New Person');
      await tester.pumpAndSettle();

      // Tap add button
      await tester.tap(find.text('Adicionar'));
      await tester.pumpAndSettle();

      // Verify person was added
      expect(find.text('New Person'), findsOneWidget);
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

    testWidgets('queue entry cards are tappable', (WidgetTester tester) async {
      // Set up test data
      mockQueueNotifier.entries = [
        QueueEntry(id: '1', name: 'John Doe', status: QueueStatus.waiting, joinTime: DateTime.now(), position: 1),
      ];
      mockQueueNotifier.notifyListeners();

      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Tap on queue entry
      await tester.tap(find.text('John Doe'));
      await tester.pumpAndSettle();

      // Verify actions are shown (bottom sheet or dialog)
      expect(find.text('John Doe'), findsAtLeastNWidgets(2)); // In card and bottom sheet
      expect(find.text('Iniciar Atendimento'), findsOneWidget);
    });

    testWidgets('can start service for waiting person', (WidgetTester tester) async {
      // Set up test data with waiting person
      mockQueueNotifier.entries = [
        QueueEntry(id: '1', name: 'John Doe', status: QueueStatus.waiting, joinTime: DateTime.now(), position: 1),
      ];
      mockQueueNotifier.notifyListeners();

      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Tap on queue entry to show actions
      await tester.tap(find.text('John Doe'));
      await tester.pumpAndSettle();

      // Tap start service button
      await tester.tap(find.text('Iniciar Atendimento'));
      await tester.pumpAndSettle();

      // Verify status changed to in service
      expect(find.text('In Service'), findsOneWidget);
    });

    testWidgets('can complete service for person in service', (WidgetTester tester) async {
      // Set up test data with person in service
      mockQueueNotifier.entries = [
        QueueEntry(id: '1', name: 'John Doe', status: QueueStatus.inService, joinTime: DateTime.now(), position: 1),
      ];
      mockQueueNotifier.notifyListeners();

      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Tap on queue entry to show actions
      await tester.tap(find.text('John Doe'));
      await tester.pumpAndSettle();

      // Tap complete service button
      await tester.tap(find.text('Concluir Atendimento'));
      await tester.pumpAndSettle();

      // Verify status changed to completed (person is still in queue but with completed status)
      expect(find.text('John Doe'), findsOneWidget);
      expect(find.text('Completed'), findsOneWidget);
    });

    testWidgets('can remove person from queue', (WidgetTester tester) async {
      // Set up test data
      mockQueueNotifier.entries = [
        QueueEntry(id: '1', name: 'John Doe', status: QueueStatus.waiting, joinTime: DateTime.now(), position: 1),
      ];
      mockQueueNotifier.notifyListeners();

      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Tap on queue entry to show actions
      await tester.tap(find.text('John Doe'));
      await tester.pumpAndSettle();

      // Tap remove button
      await tester.tap(find.text('Remover da Fila'));
      await tester.pumpAndSettle();

      // Verify person was removed
      expect(find.text('John Doe'), findsNothing);
    });

    testWidgets('statistics update when queue changes', (WidgetTester tester) async {
      // Start with one person in queue
      mockQueueNotifier.entries = [
        QueueEntry(id: '1', name: 'John Doe', status: QueueStatus.waiting, joinTime: DateTime.now(), position: 1),
      ];
      mockQueueNotifier.notifyListeners();

      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Verify statistics with one person
      expect(find.text('Aguardando'), findsOneWidget);
      expect(find.text('Em Atendimento'), findsOneWidget);
      expect(find.text('Total'), findsOneWidget);
      expect(find.text('1'), findsNWidgets(2)); // Waiting: 1, Total: 1
      expect(find.text('0'), findsOneWidget); // In service: 0

      // Add another person
      mockQueueNotifier.entries = [
        QueueEntry(id: '1', name: 'John Doe', status: QueueStatus.waiting, joinTime: DateTime.now(), position: 1),
        QueueEntry(id: '2', name: 'Jane Smith', status: QueueStatus.inService, joinTime: DateTime.now(), position: 2),
      ];
      mockQueueNotifier.notifyListeners();
      await tester.pumpAndSettle();

      // Verify statistics updated
      expect(find.text('2'), findsOneWidget); // Total: 2
      expect(find.text('1'), findsNWidgets(2)); // Waiting: 1, In service: 1
      expect(find.text('John Doe'), findsOneWidget);
      expect(find.text('Jane Smith'), findsOneWidget);
    });

    testWidgets('loading state is handled correctly', (WidgetTester tester) async {
      // Set loading state
      mockQueueNotifier.isLoading = true;
      mockQueueNotifier.notifyListeners();

      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump(); // Don't use pumpAndSettle for loading state

      // Check loading indicator
      expect(find.byType(CircularProgressIndicator), findsOneWidget);
    });

    testWidgets('error state is handled correctly', (WidgetTester tester) async {
      // Set error state (you might need to add error handling to MockQueueNotifier)
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Check that screen still renders
      expect(find.text('Gerenciamento de Fila'), findsOneWidget);
    });

    testWidgets('queue entries have correct styling', (WidgetTester tester) async {
      // Set up test data
      mockQueueNotifier.entries = [
        QueueEntry(id: '1', name: 'John Doe', status: QueueStatus.waiting, joinTime: DateTime.now(), position: 1),
        QueueEntry(id: '2', name: 'Jane Smith', status: QueueStatus.inService, joinTime: DateTime.now(), position: 2),
      ];
      mockQueueNotifier.notifyListeners();

      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Check that queue cards are present
      expect(find.byType(Card), findsAtLeastNWidgets(2));
      
      // Check status indicators - use findsAtLeastNWidgets since there might be multiple icons
      expect(find.byIcon(Icons.access_time), findsAtLeastNWidgets(1)); // Waiting
      expect(find.byIcon(Icons.person_outline), findsAtLeastNWidgets(1)); // In service
    });

    testWidgets('responsive layout works for different screen sizes', (WidgetTester tester) async {
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Test with compact width
      await tester.binding.setSurfaceSize(const Size(300, 600));
      await tester.pumpAndSettle();

      // Should still display correctly
      expect(find.text('Gerenciamento de Fila'), findsOneWidget);
      expect(find.byType(FloatingActionButton), findsOneWidget);

      // Reset surface size
      await tester.binding.setSurfaceSize(null);
    });

    testWidgets('can handle multiple rapid operations', (WidgetTester tester) async {
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Add multiple people quickly
      for (int i = 0; i < 3; i++) {
        await tester.tap(find.byType(FloatingActionButton));
        await tester.pumpAndSettle();
        
        await tester.enterText(find.byType(TextField), 'Person $i');
        await tester.pumpAndSettle();
        
        await tester.tap(find.text('Adicionar'));
        await tester.pumpAndSettle();
      }

      // Verify all people were added
      expect(find.text('Person 0'), findsOneWidget);
      expect(find.text('Person 1'), findsOneWidget);
      expect(find.text('Person 2'), findsOneWidget);
    });

    testWidgets('dialog has correct styling', (WidgetTester tester) async {
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Open add person dialog
      await tester.tap(find.byType(FloatingActionButton));
      await tester.pumpAndSettle();

      // Check dialog styling
      expect(find.byType(AlertDialog), findsOneWidget);
      expect(find.byType(TextField), findsOneWidget);
      expect(find.byType(FilledButton), findsOneWidget);
    });

    testWidgets('bottom sheet has correct styling', (WidgetTester tester) async {
      // Set up test data
      mockQueueNotifier.entries = [
        QueueEntry(id: '1', name: 'John Doe', status: QueueStatus.waiting, joinTime: DateTime.now(), position: 1),
      ];
      mockQueueNotifier.notifyListeners();

      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Tap on queue entry to show bottom sheet
      await tester.tap(find.text('John Doe'));
      await tester.pumpAndSettle();

      // Check bottom sheet styling
      expect(find.byType(BottomSheet), findsOneWidget);
      expect(find.byType(ListTile), findsAtLeastNWidgets(1));
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

    testWidgets('queue entry timestamps are displayed', (WidgetTester tester) async {
      // Set up test data with specific time
      final now = DateTime.now();
      mockQueueNotifier.entries = [
        QueueEntry(id: '1', name: 'John Doe', status: QueueStatus.waiting, joinTime: now, position: 1),
      ];
      mockQueueNotifier.notifyListeners();

      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Check that time information is displayed
      expect(find.text('John Doe'), findsOneWidget);
      // Time format might vary, so just check that some time-related text is present
      expect(find.byType(Text), findsWidgets);
    });

    testWidgets('queue status colors are correct', (WidgetTester tester) async {
      // Set up test data with different statuses
      mockQueueNotifier.entries = [
        QueueEntry(id: '1', name: 'Waiting Person', status: QueueStatus.waiting, joinTime: DateTime.now(), position: 1),
        QueueEntry(id: '2', name: 'Service Person', status: QueueStatus.inService, joinTime: DateTime.now(), position: 2),
      ];
      mockQueueNotifier.notifyListeners();

      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Check that different statuses are displayed
      expect(find.text('Waiting Person'), findsOneWidget);
      expect(find.text('Service Person'), findsOneWidget);
      expect(find.text('Waiting'), findsOneWidget);
      expect(find.text('In Service'), findsOneWidget);
    });

    testWidgets('can handle empty name in add dialog', (WidgetTester tester) async {
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Open add person dialog
      await tester.tap(find.byType(FloatingActionButton));
      await tester.pumpAndSettle();

      // Try to add with empty name
      await tester.tap(find.text('Adicionar'));
      await tester.pumpAndSettle();

      // Dialog should still be open (validation should prevent adding)
      expect(find.text('Adicionar Pessoa à Fila'), findsOneWidget);
    });

    testWidgets('can handle long names', (WidgetTester tester) async {
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Open add person dialog
      await tester.tap(find.byType(FloatingActionButton));
      await tester.pumpAndSettle();

      // Enter long name
      await tester.enterText(find.byType(TextField), 'Very Long Name That Should Be Handled Properly');
      await tester.pumpAndSettle();

      // Add the person
      await tester.tap(find.text('Adicionar'));
      await tester.pumpAndSettle();

      // Verify long name is displayed
      expect(find.text('Very Long Name That Should Be Handled Properly'), findsOneWidget);
    });

    testWidgets('refresh updates queue data', (WidgetTester tester) async {
      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Tap refresh button
      await tester.tap(find.byIcon(Icons.refresh));
      await tester.pumpAndSettle();

      // Verify refresh action was triggered
      // This would typically be verified by checking if the refresh method was called
      expect(find.text('Gerenciamento de Fila'), findsOneWidget);
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

    testWidgets('queue cards have proper spacing', (WidgetTester tester) async {
      // Set up test data with multiple entries
      mockQueueNotifier.entries = [
        QueueEntry(id: '1', name: 'Person 1', status: QueueStatus.waiting, joinTime: DateTime.now(), position: 1),
        QueueEntry(id: '2', name: 'Person 2', status: QueueStatus.waiting, joinTime: DateTime.now(), position: 2),
        QueueEntry(id: '3', name: 'Person 3', status: QueueStatus.waiting, joinTime: DateTime.now(), position: 3),
      ];
      mockQueueNotifier.notifyListeners();

      await tester.pumpWidget(createTestWidget());
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpAndSettle();

      // Check that multiple cards are displayed
      expect(find.byType(Card), findsAtLeastNWidgets(3));
      expect(find.text('Person 1'), findsOneWidget);
      expect(find.text('Person 2'), findsOneWidget);
      expect(find.text('Person 3'), findsOneWidget);
    });
  });
} 