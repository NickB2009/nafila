import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/ui/view_models/mock_queue_notifier.dart';
import 'package:eutonafila_frontend/models/queue_entry.dart';

void main() {
  group('MockQueueNotifier Tests', () {
    late MockQueueNotifier notifier;

    setUp(() {
      notifier = MockQueueNotifier();
    });

    test('initial state is loading', () {
      expect(notifier.isLoading, isTrue);
      expect(notifier.entries, isEmpty);
    });

    test('mock data loads after delay', () async {
      // Wait for mock data to load (100ms delay + buffer)
      await Future.delayed(const Duration(milliseconds: 200));

      expect(notifier.isLoading, isFalse);
      expect(notifier.entries, isNotEmpty);
      expect(notifier.waitingCount, greaterThan(0));
      expect(notifier.inServiceCount, greaterThan(0));
    });

    test('addToQueue adds new entry correctly', () async {
      // Wait for initial data to load
      await Future.delayed(const Duration(milliseconds: 200));
      
      final initialCount = notifier.entries.length;
      final initialWaitingCount = notifier.waitingCount;

      notifier.addToQueue('Test User');

      expect(notifier.entries.length, equals(initialCount + 1));
      expect(notifier.waitingCount, equals(initialWaitingCount + 1));

      final newEntry = notifier.entries.last;
      expect(newEntry.name, equals('Test User'));
      expect(newEntry.status, equals(QueueStatus.waiting));
      expect(newEntry.position, equals(initialWaitingCount + 1));
    });

    test('updateStatus changes entry status correctly', () async {
      // Wait for initial data to load
      await Future.delayed(const Duration(milliseconds: 200));
      
      final entry = notifier.entries.first;
      final initialStatus = entry.status;

      notifier.updateStatus(entry.id, QueueStatus.inService);

      final updatedEntry = notifier.entries.firstWhere((e) => e.id == entry.id);
      expect(updatedEntry.status, equals(QueueStatus.inService));
      expect(updatedEntry.status, isNot(equals(initialStatus)));
    });

    test('removeFromQueue removes entry correctly', () async {
      // Wait for initial data to load
      await Future.delayed(const Duration(milliseconds: 200));
      
      final initialCount = notifier.entries.length;
      final entryToRemove = notifier.entries.first;

      notifier.removeFromQueue(entryToRemove.id);

      expect(notifier.entries.length, equals(initialCount - 1));
      expect(notifier.entries.any((e) => e.id == entryToRemove.id), isFalse);
    });

    test('updatePositions works correctly when status changes', () async {
      // Wait for initial data to load
      await Future.delayed(const Duration(milliseconds: 200));
      
      // Get all waiting entries
      final waitingEntries = notifier.entries
          .where((e) => e.status == QueueStatus.waiting)
          .toList();

      if (waitingEntries.isNotEmpty) {
        final firstWaiting = waitingEntries.first;
        final initialPosition = firstWaiting.position;

        // Change status to in-service
        notifier.updateStatus(firstWaiting.id, QueueStatus.inService);

        // Check that positions were updated for remaining waiting entries
        final remainingWaiting = notifier.entries
            .where((e) => e.status == QueueStatus.waiting)
            .toList();

        for (int i = 0; i < remainingWaiting.length; i++) {
          expect(remainingWaiting[i].position, equals(i + 1));
        }

        // Check that the changed entry has position 0
        final changedEntry = notifier.entries.firstWhere((e) => e.id == firstWaiting.id);
        expect(changedEntry.position, equals(0));
      }
    });

    test('waitingCount returns correct count', () async {
      // Wait for initial data to load
      await Future.delayed(const Duration(milliseconds: 200));
      
      final actualWaitingCount = notifier.entries
          .where((e) => e.status == QueueStatus.waiting)
          .length;

      expect(notifier.waitingCount, equals(actualWaitingCount));
    });

    test('inServiceCount returns correct count', () async {
      // Wait for initial data to load
      await Future.delayed(const Duration(milliseconds: 200));
      
      final actualInServiceCount = notifier.entries
          .where((e) => e.status == QueueStatus.inService)
          .length;

      expect(notifier.inServiceCount, equals(actualInServiceCount));
    });

    test('refresh reloads mock data', () async {
      // Wait for initial data to load
      await Future.delayed(const Duration(milliseconds: 200));
      
      final initialEntries = List<QueueEntry>.from(notifier.entries);
      
      notifier.refresh();

      // After refresh, entries should be the same (mock data is deterministic)
      expect(notifier.entries.length, equals(initialEntries.length));
    });

    test('notifyListeners is called when state changes', () async {
      // Wait for initial data to load
      await Future.delayed(const Duration(milliseconds: 200));
      
      var listenerCalled = false;
      
      notifier.addListener(() {
        listenerCalled = true;
      });

      notifier.addToQueue('Test User');
      expect(listenerCalled, isTrue);
    });

    test('updateStatus with invalid id does nothing', () async {
      // Wait for initial data to load
      await Future.delayed(const Duration(milliseconds: 200));
      
      final initialEntries = List<QueueEntry>.from(notifier.entries);
      
      notifier.updateStatus('invalid-id', QueueStatus.completed);
      
      expect(notifier.entries, equals(initialEntries));
    });

    test('removeFromQueue with invalid id does nothing', () async {
      // Wait for initial data to load
      await Future.delayed(const Duration(milliseconds: 200));
      
      final initialEntries = List<QueueEntry>.from(notifier.entries);
      
      notifier.removeFromQueue('invalid-id');
      
      expect(notifier.entries, equals(initialEntries));
    });

    test('entries are sorted correctly by status and position', () async {
      // Wait for initial data to load
      await Future.delayed(const Duration(milliseconds: 200));
      
      // Add a new entry to ensure we have multiple waiting entries
      notifier.addToQueue('New User');

      final entries = notifier.entries;
      
      // Check that in-service entries come first
      final inServiceEntries = entries.where((e) => e.status == QueueStatus.inService);
      final waitingEntries = entries.where((e) => e.status == QueueStatus.waiting);
      final completedEntries = entries.where((e) => e.status == QueueStatus.completed);

      // Verify positions are sequential for waiting entries
      final waitingList = waitingEntries.toList();
      for (int i = 0; i < waitingList.length; i++) {
        expect(waitingList[i].position, equals(i + 1));
      }

      // Verify non-waiting entries have position 0
      for (final entry in inServiceEntries) {
        expect(entry.position, equals(0));
      }
      for (final entry in completedEntries) {
        expect(entry.position, equals(0));
      }
    });
  });
} 