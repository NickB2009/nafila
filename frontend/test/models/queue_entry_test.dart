import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/models/queue_entry.dart';

void main() {
  group('QueueEntry Tests', () {
    test('creates entry with correct properties', () {
      final entry = QueueEntry(
        id: '1',
        name: 'John Doe',
        status: QueueStatus.waiting,
        joinTime: DateTime(2024, 1, 1, 10, 0),
        position: 1,
      );

      expect(entry.id, equals('1'));
      expect(entry.name, equals('John Doe'));
      expect(entry.status, equals(QueueStatus.waiting));
      expect(entry.joinTime, equals(DateTime(2024, 1, 1, 10, 0)));
      expect(entry.position, equals(1));
    });

    test('copyWith creates new instance with updated fields', () {
      final original = QueueEntry(
        id: '1',
        name: 'John Doe',
        status: QueueStatus.waiting,
        joinTime: DateTime(2024, 1, 1, 10, 0),
        position: 1,
      );

      final updated = original.copyWith(
        status: QueueStatus.inService,
        position: 0,
      );

      expect(updated.id, equals(original.id));
      expect(updated.name, equals(original.name));
      expect(updated.status, equals(QueueStatus.inService));
      expect(updated.joinTime, equals(original.joinTime));
      expect(updated.position, equals(0));
    });

    test('copyWith returns same instance when no changes', () {
      final original = QueueEntry(
        id: '1',
        name: 'John Doe',
        status: QueueStatus.waiting,
        joinTime: DateTime(2024, 1, 1, 10, 0),
        position: 1,
      );

      final copied = original.copyWith();

      expect(copied.id, equals(original.id));
      expect(copied.name, equals(original.name));
      expect(copied.status, equals(original.status));
      expect(copied.joinTime, equals(original.joinTime));
      expect(copied.position, equals(original.position));
    });

    test('toString returns meaningful representation', () {
      final entry = QueueEntry(
        id: '1',
        name: 'John Doe',
        status: QueueStatus.waiting,
        joinTime: DateTime(2024, 1, 1, 10, 0),
        position: 1,
      );

      final string = entry.toString();
      expect(string, contains('John Doe'));
      expect(string, contains('waiting'));
      expect(string, contains('1'));
    });

    test('equality works correctly', () {
      final entry1 = QueueEntry(
        id: '1',
        name: 'John Doe',
        status: QueueStatus.waiting,
        joinTime: DateTime(2024, 1, 1, 10, 0),
        position: 1,
      );

      final entry2 = QueueEntry(
        id: '1',
        name: 'John Doe',
        status: QueueStatus.waiting,
        joinTime: DateTime(2024, 1, 1, 10, 0),
        position: 1,
      );

      final entry3 = QueueEntry(
        id: '2',
        name: 'Jane Smith',
        status: QueueStatus.inService,
        joinTime: DateTime(2024, 1, 1, 11, 0),
        position: 0,
      );

      expect(entry1, equals(entry2));
      expect(entry1, isNot(equals(entry3)));
    });

    test('hashCode is consistent with equality', () {
      final entry1 = QueueEntry(
        id: '1',
        name: 'John Doe',
        status: QueueStatus.waiting,
        joinTime: DateTime(2024, 1, 1, 10, 0),
        position: 1,
      );

      final entry2 = QueueEntry(
        id: '1',
        name: 'John Doe',
        status: QueueStatus.waiting,
        joinTime: DateTime(2024, 1, 1, 10, 0),
        position: 1,
      );

      expect(entry1.hashCode, equals(entry2.hashCode));
    });
  });

  group('QueueStatus Tests', () {
    test('all status values have display names', () {
      expect(QueueStatus.waiting.displayName, equals('Waiting'));
      expect(QueueStatus.inService.displayName, equals('In Service'));
      expect(QueueStatus.completed.displayName, equals('Completed'));
    });

    test('status values are distinct', () {
      expect(QueueStatus.waiting, isNot(equals(QueueStatus.inService)));
      expect(QueueStatus.waiting, isNot(equals(QueueStatus.completed)));
      expect(QueueStatus.inService, isNot(equals(QueueStatus.completed)));
    });
  });
} 