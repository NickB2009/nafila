import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import '../../lib/ui/screens/anonymous_queue_status_screen.dart';
import '../../lib/services/anonymous_queue_service.dart';
import '../../lib/models/anonymous_user.dart';
import '../../lib/models/public_salon.dart';

import 'anonymous_queue_status_screen_test.mocks.dart';

@GenerateMocks([AnonymousQueueService])
void main() {
  group('AnonymousQueueStatusScreen', () {
    late MockAnonymousQueueService mockQueueService;
    late AnonymousQueueEntry testQueueEntry;
    late PublicSalon testSalon;

    setUp(() {
      mockQueueService = MockAnonymousQueueService();
      
      testSalon = const PublicSalon(
        id: 'salon123',
        name: 'Test Salon',
        address: '123 Test St',
        latitude: -23.5505,
        longitude: -46.6333,
        isOpen: true,
        queueLength: 5,
        currentWaitTimeMinutes: 25,
        rating: 4.5,
        reviewCount: 100,
        distance: 0.5,
        services: ['Haircut', 'Beard Trim'],
        phoneNumber: '(11) 91234-5678',
      );

      testQueueEntry = AnonymousQueueEntry(
        id: 'entry123',
        anonymousUserId: 'user123',
        salonId: 'salon123',
        salonName: 'Test Salon',
        position: 3,
        estimatedWaitMinutes: 15,
        joinedAt: DateTime.now().subtract(const Duration(minutes: 10)),
        lastUpdated: DateTime.now(),
        status: QueueEntryStatus.waiting,
        serviceRequested: 'Haircut',
      );
    });

    testWidgets('displays real queue data from backend', (WidgetTester tester) async {
      // Arrange
      when(mockQueueService.getQueueStatus('entry123'))
          .thenAnswer((_) async => testQueueEntry);

      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: AnonymousQueueStatusScreen(
            queueEntry: testQueueEntry,
            salon: testSalon,
            queueService: mockQueueService,
          ),
        ),
      );
      await tester.pumpAndSettle();

      // Assert
      expect(find.text('15 min'), findsOneWidget);
      expect(find.text('3º'), findsOneWidget);
      expect(find.text('Test Salon'), findsOneWidget);
      expect(find.text('123 Test St'), findsOneWidget);
    });

    testWidgets('shows loading state while fetching queue status', (WidgetTester tester) async {
      // Arrange
      when(mockQueueService.getQueueStatus('entry123'))
          .thenAnswer((_) async {
        await Future.delayed(const Duration(milliseconds: 100));
        return testQueueEntry;
      });

      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: AnonymousQueueStatusScreen(
            queueEntry: testQueueEntry,
            salon: testSalon,
            queueService: mockQueueService,
          ),
        ),
      );

      // Assert - loading state
      expect(find.byType(CircularProgressIndicator), findsAtLeastNWidget(1));

      // Wait for loading to complete
      await tester.pumpAndSettle();

      // Assert - loaded state
      expect(find.text('15 min'), findsOneWidget);
    });

    testWidgets('updates queue position when backend data changes', (WidgetTester tester) async {
      // Arrange
      final updatedEntry = testQueueEntry.copyWith(
        position: 2,
        estimatedWaitMinutes: 10,
        lastUpdated: DateTime.now(),
      );

      when(mockQueueService.getQueueStatus('entry123'))
          .thenAnswer((_) async => testQueueEntry);

      // Act - initial load
      await tester.pumpWidget(
        MaterialApp(
          home: AnonymousQueueStatusScreen(
            queueEntry: testQueueEntry,
            salon: testSalon,
            queueService: mockQueueService,
          ),
        ),
      );
      await tester.pumpAndSettle();

      // Assert initial state
      expect(find.text('15 min'), findsOneWidget);
      expect(find.text('3º'), findsOneWidget);

      // Arrange - simulate backend update
      when(mockQueueService.getQueueStatus('entry123'))
          .thenAnswer((_) async => updatedEntry);

      // Act - trigger refresh (simulate timer)
      final state = tester.state<AnonymousQueueStatusScreenState>(
        find.byType(AnonymousQueueStatusScreen),
      );
      await state.refreshQueueStatus();
      await tester.pumpAndSettle();

      // Assert updated state
      expect(find.text('10 min'), findsOneWidget);
      expect(find.text('2º'), findsOneWidget);
    });

    testWidgets('handles queue completion status', (WidgetTester tester) async {
      // Arrange
      final completedEntry = testQueueEntry.copyWith(
        status: QueueEntryStatus.completed,
        position: 0,
        estimatedWaitMinutes: 0,
      );

      when(mockQueueService.getQueueStatus('entry123'))
          .thenAnswer((_) async => completedEntry);

      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: AnonymousQueueStatusScreen(
            queueEntry: testQueueEntry,
            salon: testSalon,
            queueService: mockQueueService,
          ),
        ),
      );
      await tester.pumpAndSettle();

      // Assert
      expect(find.text('COMPLETED'), findsOneWidget);
      expect(find.text('Your service is complete'), findsOneWidget);
    });

    testWidgets('handles being called status', (WidgetTester tester) async {
      // Arrange
      final calledEntry = testQueueEntry.copyWith(
        status: QueueEntryStatus.called,
        position: 1,
        estimatedWaitMinutes: 0,
      );

      when(mockQueueService.getQueueStatus('entry123'))
          .thenAnswer((_) async => calledEntry);

      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: AnonymousQueueStatusScreen(
            queueEntry: testQueueEntry,
            salon: testSalon,
            queueService: mockQueueService,
          ),
        ),
      );
      await tester.pumpAndSettle();

      // Assert
      expect(find.text('YOU\'RE UP!'), findsOneWidget);
      expect(find.text('Please proceed to the salon'), findsOneWidget);
    });

    testWidgets('allows leaving the queue', (WidgetTester tester) async {
      // Arrange
      when(mockQueueService.getQueueStatus('entry123'))
          .thenAnswer((_) async => testQueueEntry);
      when(mockQueueService.leaveQueue('entry123'))
          .thenAnswer((_) async {});

      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: AnonymousQueueStatusScreen(
            queueEntry: testQueueEntry,
            salon: testSalon,
            queueService: mockQueueService,
          ),
        ),
      );
      await tester.pumpAndSettle();

      // Find and tap leave queue button
      await tester.tap(find.text('Leave Queue'));
      await tester.pumpAndSettle();

      // Confirm in dialog
      await tester.tap(find.text('Leave'));
      await tester.pumpAndSettle();

      // Assert
      verify(mockQueueService.leaveQueue('entry123')).called(1);
    });

    testWidgets('handles network errors gracefully', (WidgetTester tester) async {
      // Arrange
      when(mockQueueService.getQueueStatus('entry123'))
          .thenThrow(Exception('Network error'));

      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: AnonymousQueueStatusScreen(
            queueEntry: testQueueEntry,
            salon: testSalon,
            queueService: mockQueueService,
          ),
        ),
      );
      await tester.pumpAndSettle();

      // Assert - should show cached data and error indicator
      expect(find.text('15 min'), findsOneWidget); // cached data
      expect(find.text('Connection error'), findsOneWidget);
    });

    testWidgets('provides refresh functionality', (WidgetTester tester) async {
      // Arrange
      when(mockQueueService.getQueueStatus('entry123'))
          .thenAnswer((_) async => testQueueEntry);

      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: AnonymousQueueStatusScreen(
            queueEntry: testQueueEntry,
            salon: testSalon,
            queueService: mockQueueService,
          ),
        ),
      );
      await tester.pumpAndSettle();

      // Trigger pull-to-refresh
      await tester.fling(find.byType(RefreshIndicator), const Offset(0, 300), 1000);
      await tester.pumpAndSettle();

      // Assert - service called again
      verify(mockQueueService.getQueueStatus('entry123')).called(2);
    });

    testWidgets('shows real-time updates with timer', (WidgetTester tester) async {
      // Arrange
      when(mockQueueService.getQueueStatus('entry123'))
          .thenAnswer((_) async => testQueueEntry);

      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: AnonymousQueueStatusScreen(
            queueEntry: testQueueEntry,
            salon: testSalon,
            queueService: mockQueueService,
            updateInterval: const Duration(milliseconds: 100), // Fast for testing
          ),
        ),
      );
      await tester.pumpAndSettle();

      // Wait for timer to trigger
      await tester.pump(const Duration(milliseconds: 150));

      // Assert - service called multiple times due to timer
      verify(mockQueueService.getQueueStatus('entry123')).called(greaterThan(1));
    });
  });
} 