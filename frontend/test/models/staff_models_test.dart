import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/models/staff_models.dart';

void main() {
  group('AddBarberRequest', () {
    test('should create AddBarberRequest with required fields', () {
      final request = AddBarberRequest(
        firstName: 'João',
        lastName: 'Silva',
        email: 'joao@example.com',
        phoneNumber: '+5511999999999',
        username: 'joao.silva',
        locationId: 'location-123',
        serviceTypeIds: ['service-1', 'service-2'],
        deactivateOnCreation: false,
        address: 'Rua das Flores, 123',
        notes: 'Experienced barber',
      );

      expect(request.firstName, equals('João'));
      expect(request.lastName, equals('Silva'));
      expect(request.email, equals('joao@example.com'));
      expect(request.phoneNumber, equals('+5511999999999'));
      expect(request.username, equals('joao.silva'));
      expect(request.locationId, equals('location-123'));
      expect(request.serviceTypeIds, equals(['service-1', 'service-2']));
      expect(request.deactivateOnCreation, isFalse);
      expect(request.address, equals('Rua das Flores, 123'));
      expect(request.notes, equals('Experienced barber'));
    });

    test('should serialize to JSON correctly', () {
      final request = AddBarberRequest(
        firstName: 'João',
        lastName: 'Silva',
        email: 'joao@example.com',
        phoneNumber: '+5511999999999',
        username: 'joao.silva',
        locationId: 'location-123',
        serviceTypeIds: ['service-1', 'service-2'],
      );

      final json = request.toJson();
      expect(json['firstName'], equals('João'));
      expect(json['lastName'], equals('Silva'));
      expect(json['email'], equals('joao@example.com'));
      expect(json['phoneNumber'], equals('+5511999999999'));
      expect(json['username'], equals('joao.silva'));
      expect(json['locationId'], equals('location-123'));
      expect(json['serviceTypeIds'], equals(['service-1', 'service-2']));
    });
  });

  group('AddBarberResult', () {
    test('should create successful result', () {
      final result = AddBarberResult(
        success: true,
        barberId: 'barber-123',
        status: 'Created',
        fieldErrors: {},
        errors: [],
      );

      expect(result.success, isTrue);
      expect(result.barberId, equals('barber-123'));
      expect(result.status, equals('Created'));
      expect(result.fieldErrors, isEmpty);
      expect(result.errors, isEmpty);
    });

    test('should create failed result with errors', () {
      final result = AddBarberResult(
        success: false,
        barberId: null,
        status: 'ValidationFailed',
        fieldErrors: {'email': 'Email already exists'},
        errors: ['Username is required'],
      );

      expect(result.success, isFalse);
      expect(result.barberId, isNull);
      expect(result.status, equals('ValidationFailed'));
      expect(result.fieldErrors!['email'], equals('Email already exists'));
      expect(result.errors!.first, equals('Username is required'));
    });
  });

  group('EditBarberRequest', () {
    test('should create EditBarberRequest with all fields', () {
      final request = EditBarberRequest(
        staffMemberId: 'staff-123',
        name: 'João Silva',
        email: 'joao.new@example.com',
        phoneNumber: '+5511888888888',
        profilePictureUrl: 'https://example.com/photo.jpg',
        role: 'Senior Barber',
      );

      expect(request.staffMemberId, equals('staff-123'));
      expect(request.name, equals('João Silva'));
      expect(request.email, equals('joao.new@example.com'));
      expect(request.phoneNumber, equals('+5511888888888'));
      expect(request.profilePictureUrl, equals('https://example.com/photo.jpg'));
      expect(request.role, equals('Senior Barber'));
    });
  });

  group('UpdateStaffStatusRequest', () {
    test('should create status update request', () {
      final request = UpdateStaffStatusRequest(
        staffMemberId: 'staff-123',
        newStatus: 'Available',
        notes: 'Ready for next customer',
      );

      expect(request.staffMemberId, equals('staff-123'));
      expect(request.newStatus, equals('Available'));
      expect(request.notes, equals('Ready for next customer'));
    });
  });

  group('StartBreakRequest', () {
    test('should create break request with required fields', () {
      final request = StartBreakRequest(
        staffMemberId: 'staff-123',
        durationMinutes: 15,
        reason: 'Lunch break',
      );

      expect(request.staffMemberId, equals('staff-123'));
      expect(request.durationMinutes, equals(15));
      expect(request.reason, equals('Lunch break'));
    });
  });

  group('EndBreakRequest', () {
    test('should create end break request', () {
      final request = EndBreakRequest(
        staffMemberId: 'staff-123',
        breakId: 'break-123',
      );

      expect(request.staffMemberId, equals('staff-123'));
      expect(request.breakId, equals('break-123'));
    });
  });

  group('StaffActivityDto', () {
    test('should create StaffActivityDto with all fields', () {
      final activity = StaffActivityDto(
        staffId: 'staff-123',
        staffName: 'João Silva',
        status: 'Available',
        lastActivity: DateTime(2024, 1, 15, 10, 30),
        customersServedToday: 5,
        averageServiceTimeMinutes: 25.5,
        isOnBreak: false,
        breakStartTime: null,
      );

      expect(activity.staffId, equals('staff-123'));
      expect(activity.staffName, equals('João Silva'));
      expect(activity.status, equals('Available'));
      expect(activity.lastActivity, equals(DateTime(2024, 1, 15, 10, 30)));
      expect(activity.customersServedToday, equals(5));
      expect(activity.averageServiceTimeMinutes, equals(25.5));
      expect(activity.isOnBreak, isFalse);
      expect(activity.breakStartTime, isNull);
    });

    test('should handle staff on break', () {
      final breakTime = DateTime(2024, 1, 15, 12, 0);
      final activity = StaffActivityDto(
        staffId: 'staff-123',
        staffName: 'João Silva',
        status: 'OnBreak',
        lastActivity: DateTime(2024, 1, 15, 11, 45),
        customersServedToday: 3,
        averageServiceTimeMinutes: 30.0,
        isOnBreak: true,
        breakStartTime: breakTime,
      );

      expect(activity.isOnBreak, isTrue);
      expect(activity.breakStartTime, equals(breakTime));
      expect(activity.status, equals('OnBreak'));
    });
  });
}