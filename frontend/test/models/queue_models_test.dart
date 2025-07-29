import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/models/queue_models.dart';

void main() {
  group('AddQueueRequest', () {
    test('should create AddQueueRequest with required fields', () {
      final request = AddQueueRequest(
        locationId: 'location-123',
        maxSize: 50,
        lateClientCapTimeInMinutes: 15,
      );

      expect(request.locationId, equals('location-123'));
      expect(request.maxSize, equals(50));
      expect(request.lateClientCapTimeInMinutes, equals(15));
    });

    test('should serialize to JSON correctly', () {
      final request = AddQueueRequest(
        locationId: 'location-123',
        maxSize: 50,
        lateClientCapTimeInMinutes: 15,
      );

      final json = request.toJson();
      expect(json['locationId'], equals('location-123'));
      expect(json['maxSize'], equals(50));
      expect(json['lateClientCapTimeInMinutes'], equals(15));
    });

    test('should deserialize from JSON correctly', () {
      final json = {
        'locationId': 'location-123',
        'maxSize': 50,
        'lateClientCapTimeInMinutes': 15,
      };

      final request = AddQueueRequest.fromJson(json);
      expect(request.locationId, equals('location-123'));
      expect(request.maxSize, equals(50));
      expect(request.lateClientCapTimeInMinutes, equals(15));
    });
  });

  group('JoinQueueRequest', () {
    test('should create JoinQueueRequest with required fields', () {
      final request = JoinQueueRequest(
        queueId: 'queue-123',
        customerName: 'John Doe',
        phoneNumber: '+5511999999999',
        email: 'john@example.com',
        isAnonymous: false,
        notes: 'Regular haircut',
        serviceTypeId: 'service-123',
      );

      expect(request.queueId, equals('queue-123'));
      expect(request.customerName, equals('John Doe'));
      expect(request.phoneNumber, equals('+5511999999999'));
      expect(request.email, equals('john@example.com'));
      expect(request.isAnonymous, isFalse);
      expect(request.notes, equals('Regular haircut'));
      expect(request.serviceTypeId, equals('service-123'));
    });

    test('should support anonymous users', () {
      final request = JoinQueueRequest(
        queueId: 'queue-123',
        customerName: 'Anonymous',
        isAnonymous: true,
      );

      expect(request.isAnonymous, isTrue);
      expect(request.phoneNumber, isNull);
      expect(request.email, isNull);
    });

    test('should serialize to JSON correctly', () {
      final request = JoinQueueRequest(
        queueId: 'queue-123',
        customerName: 'John Doe',
        phoneNumber: '+5511999999999',
        email: 'john@example.com',
        isAnonymous: false,
      );

      final json = request.toJson();
      expect(json['queueId'], equals('queue-123'));
      expect(json['customerName'], equals('John Doe'));
      expect(json['phoneNumber'], equals('+5511999999999'));
      expect(json['email'], equals('john@example.com'));
      expect(json['isAnonymous'], isFalse);
    });
  });

  group('BarberAddRequest', () {
    test('should create BarberAddRequest with required fields', () {
      final request = BarberAddRequest(
        queueId: 'queue-123',
        staffMemberId: 'staff-123',
        customerName: 'John Doe',
        phoneNumber: '+5511999999999',
        email: 'john@example.com',
        serviceTypeId: 'service-123',
        notes: 'Walk-in customer',
      );

      expect(request.queueId, equals('queue-123'));
      expect(request.staffMemberId, equals('staff-123'));
      expect(request.customerName, equals('John Doe'));
      expect(request.phoneNumber, equals('+5511999999999'));
      expect(request.email, equals('john@example.com'));
      expect(request.serviceTypeId, equals('service-123'));
      expect(request.notes, equals('Walk-in customer'));
    });
  });

  group('QueueActivityDto', () {
    test('should create QueueActivityDto with all fields', () {
      final activity = QueueActivityDto(
        queueId: 'queue-123',
        queueDate: DateTime(2024, 1, 15),
        isActive: true,
        customersWaiting: 5,
        customersBeingServed: 2,
        totalCustomersToday: 25,
        averageWaitTimeMinutes: 15.5,
        maxSize: 50,
      );

      expect(activity.queueId, equals('queue-123'));
      expect(activity.queueDate, equals(DateTime(2024, 1, 15)));
      expect(activity.isActive, isTrue);
      expect(activity.customersWaiting, equals(5));
      expect(activity.customersBeingServed, equals(2));
      expect(activity.totalCustomersToday, equals(25));
      expect(activity.averageWaitTimeMinutes, equals(15.5));
      expect(activity.maxSize, equals(50));
    });

    test('should serialize to JSON correctly', () {
      final activity = QueueActivityDto(
        queueId: 'queue-123',
        queueDate: DateTime(2024, 1, 15),
        isActive: true,
        customersWaiting: 5,
        customersBeingServed: 2,
        totalCustomersToday: 25,
        averageWaitTimeMinutes: 15.5,
        maxSize: 50,
      );

      final json = activity.toJson();
      expect(json['queueId'], equals('queue-123'));
      expect(json['isActive'], isTrue);
      expect(json['customersWaiting'], equals(5));
      expect(json['customersBeingServed'], equals(2));
      expect(json['totalCustomersToday'], equals(25));
      expect(json['averageWaitTimeMinutes'], equals(15.5));
      expect(json['maxSize'], equals(50));
    });
  });

  group('CallNextRequest', () {
    test('should create CallNextRequest with required fields', () {
      final request = CallNextRequest(
        staffMemberId: 'staff-123',
        queueId: 'queue-123',
      );

      expect(request.staffMemberId, equals('staff-123'));
      expect(request.queueId, equals('queue-123'));
    });
  });

  group('CheckInRequest', () {
    test('should create CheckInRequest with required fields', () {
      final request = CheckInRequest(
        queueEntryId: 'entry-123',
      );

      expect(request.queueEntryId, equals('entry-123'));
    });
  });

  group('FinishRequest', () {
    test('should create FinishRequest with required fields', () {
      final request = FinishRequest(
        queueEntryId: 'entry-123',
        serviceDurationMinutes: 30,
        notes: 'Service completed successfully',
      );

      expect(request.queueEntryId, equals('entry-123'));
      expect(request.serviceDurationMinutes, equals(30));
      expect(request.notes, equals('Service completed successfully'));
    });
  });

  group('CancelQueueRequest', () {
    test('should create CancelQueueRequest with required fields', () {
      final request = CancelQueueRequest(
        queueEntryId: 'entry-123',
      );

      expect(request.queueEntryId, equals('entry-123'));
    });
  });
}