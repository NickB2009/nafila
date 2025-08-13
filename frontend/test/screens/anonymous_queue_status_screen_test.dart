import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/ui/screens/anonymous_queue_status_screen.dart';
import 'package:eutonafila_frontend/models/anonymous_user.dart';
import 'package:eutonafila_frontend/models/public_salon.dart';

void main() {
  group('AnonymousQueueStatusScreen', () {
    late AnonymousQueueEntry testQueueEntry;
    late PublicSalon testSalon;

    setUp(() {
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
        distanceKm: 0.5,
        services: ['Haircut', 'Beard Trim'],
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

    testWidgets('displays queue data correctly', (WidgetTester tester) async {
      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: AnonymousQueueStatusScreen(
            queueEntry: testQueueEntry,
            salon: testSalon,
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

    testWidgets('handles queue completion status', (WidgetTester tester) async {
      // Arrange
      final completedEntry = testQueueEntry.copyWith(
        status: QueueEntryStatus.completed,
        position: 0,
        estimatedWaitMinutes: 0,
      );

      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: AnonymousQueueStatusScreen(
            queueEntry: completedEntry,
            salon: testSalon,
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

      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: AnonymousQueueStatusScreen(
            queueEntry: calledEntry,
            salon: testSalon,
          ),
        ),
      );
      await tester.pumpAndSettle();

      // Assert
      expect(find.text('YOU\'RE UP!'), findsOneWidget);
      expect(find.text('Please proceed to the salon'), findsOneWidget);
    });

    testWidgets('shows leave queue button', (WidgetTester tester) async {
      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: AnonymousQueueStatusScreen(
            queueEntry: testQueueEntry,
            salon: testSalon,
          ),
        ),
      );
      await tester.pumpAndSettle();

      // Assert
      expect(find.text('Leave Queue'), findsOneWidget);
    });

    testWidgets('displays salon information correctly', (WidgetTester tester) async {
      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: AnonymousQueueStatusScreen(
            queueEntry: testQueueEntry,
            salon: testSalon,
          ),
        ),
      );
      await tester.pumpAndSettle();

      // Assert
      expect(find.text('Test Salon'), findsAtLeastNWidgets(1));
      expect(find.text('123 Test St'), findsOneWidget);
      expect(find.text('Haircut'), findsOneWidget);
    });
  });
} 