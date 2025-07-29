import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:dio/dio.dart';
import 'package:eutonafila_frontend/services/staff_service.dart';
import 'package:eutonafila_frontend/services/api_client.dart';
import 'package:eutonafila_frontend/models/staff_models.dart';

import 'staff_service_test.mocks.dart';

@GenerateMocks([ApiClient])
void main() {
  late StaffService staffService;
  late MockApiClient mockApiClient;

  setUp(() {
    mockApiClient = MockApiClient();
    staffService = StaffService(apiClient: mockApiClient);
  });

  group('StaffService', () {
    group('addBarber', () {
      test('should successfully add a new barber', () async {
        // Arrange
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
          notes: 'Barbeiro experiente',
        );

        final expectedResult = AddBarberResult(
          success: true,
          barberId: 'barber-123',
          status: 'Created',
          fieldErrors: {},
          errors: [],
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Staff/barbers'),
          statusCode: 200,
          data: expectedResult.toJson(),
        );

        when(mockApiClient.post('/Staff/barbers', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await staffService.addBarber(request);

        // Assert
        expect(result.success, isTrue);
        expect(result.barberId, equals('barber-123'));
        expect(result.status, equals('Created'));
        verify(mockApiClient.post('/Staff/barbers', data: request.toJson())).called(1);
      });

      test('should handle validation errors when adding barber', () async {
        // Arrange
        final request = AddBarberRequest(
          firstName: 'João',
          lastName: 'Silva',
          email: 'invalid-email',
          phoneNumber: '+5511999999999',
          username: 'joao.silva',
          locationId: 'location-123',
          serviceTypeIds: ['service-1'],
        );

        final expectedResult = AddBarberResult(
          success: false,
          barberId: null,
          status: 'ValidationFailed',
          fieldErrors: {'email': 'Invalid email format'},
          errors: ['Username already exists'],
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Staff/barbers'),
          statusCode: 400,
          data: expectedResult.toJson(),
        );

        when(mockApiClient.post('/Staff/barbers', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await staffService.addBarber(request);

        // Assert
        expect(result.success, isFalse);
        expect(result.barberId, isNull);
        expect(result.fieldErrors!['email'], equals('Invalid email format'));
        expect(result.errors!.first, equals('Username already exists'));
      });
    });

    group('editBarber', () {
      test('should successfully edit barber details', () async {
        // Arrange
        final request = EditBarberRequest(
          staffMemberId: 'staff-123',
          name: 'João Silva Santos',
          email: 'joao.novo@example.com',
          phoneNumber: '+5511888888888',
          profilePictureUrl: 'https://example.com/photo.jpg',
          role: 'Senior Barber',
        );

        final expectedResult = EditBarberResult(
          success: true,
          staffMemberId: 'staff-123',
          name: 'João Silva Santos',
          email: 'joao.novo@example.com',
          phoneNumber: '+5511888888888',
          profilePictureUrl: 'https://example.com/photo.jpg',
          role: 'Senior Barber',
          errors: [],
          fieldErrors: {},
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Staff/barbers/staff-123'),
          statusCode: 200,
          data: expectedResult.toJson(),
        );

        when(mockApiClient.put('/Staff/barbers/staff-123', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await staffService.editBarber('staff-123', request);

        // Assert
        expect(result.success, isTrue);
        expect(result.staffMemberId, equals('staff-123'));
        expect(result.name, equals('João Silva Santos'));
        expect(result.email, equals('joao.novo@example.com'));
        verify(mockApiClient.put('/Staff/barbers/staff-123', data: request.toJson())).called(1);
      });
    });

    group('updateStaffStatus', () {
      test('should successfully update staff status', () async {
        // Arrange
        final request = UpdateStaffStatusRequest(
          staffMemberId: 'staff-123',
          newStatus: 'Available',
          notes: 'Ready for next customer',
        );

        final expectedResult = UpdateStaffStatusResult(
          success: true,
          staffMemberId: 'staff-123',
          newStatus: 'Available',
          previousStatus: 'Busy',
          fieldErrors: {},
          errors: [],
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Staff/barbers/staff-123/status'),
          statusCode: 200,
          data: expectedResult.toJson(),
        );

        when(mockApiClient.put('/Staff/barbers/staff-123/status', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await staffService.updateStaffStatus('staff-123', request);

        // Assert
        expect(result.success, isTrue);
        expect(result.staffMemberId, equals('staff-123'));
        expect(result.newStatus, equals('Available'));
        expect(result.previousStatus, equals('Busy'));
        verify(mockApiClient.put('/Staff/barbers/staff-123/status', data: request.toJson())).called(1);
      });
    });

    group('startBreak', () {
      test('should successfully start a break', () async {
        // Arrange
        final request = StartBreakRequest(
          staffMemberId: 'staff-123',
          durationMinutes: 15,
          reason: 'Lunch break',
        );

        final expectedResult = StartBreakResult(
          success: true,
          staffMemberId: 'staff-123',
          breakId: 'break-123',
          newStatus: 'OnBreak',
          fieldErrors: {},
          errors: [],
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Staff/barbers/staff-123/start-break'),
          statusCode: 200,
          data: expectedResult.toJson(),
        );

        when(mockApiClient.put('/Staff/barbers/staff-123/start-break', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await staffService.startBreak('staff-123', request);

        // Assert
        expect(result.success, isTrue);
        expect(result.staffMemberId, equals('staff-123'));
        expect(result.breakId, equals('break-123'));
        expect(result.newStatus, equals('OnBreak'));
        verify(mockApiClient.put('/Staff/barbers/staff-123/start-break', data: request.toJson())).called(1);
      });
    });

    group('endBreak', () {
      test('should successfully end a break', () async {
        // Arrange
        final request = EndBreakRequest(
          staffMemberId: 'staff-123',
          breakId: 'break-123',
        );

        final expectedResult = EndBreakResult(
          success: true,
          staffMemberId: 'staff-123',
          breakId: 'break-123',
          newStatus: 'Available',
          fieldErrors: {},
          errors: [],
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Staff/barbers/staff-123/end-break'),
          statusCode: 200,
          data: expectedResult.toJson(),
        );

        when(mockApiClient.put('/Staff/barbers/staff-123/end-break', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await staffService.endBreak('staff-123', request);

        // Assert
        expect(result.success, isTrue);
        expect(result.staffMemberId, equals('staff-123'));
        expect(result.breakId, equals('break-123'));
        expect(result.newStatus, equals('Available'));
        verify(mockApiClient.put('/Staff/barbers/staff-123/end-break', data: request.toJson())).called(1);
      });
    });
  });
}