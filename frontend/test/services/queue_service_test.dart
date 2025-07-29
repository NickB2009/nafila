import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:dio/dio.dart';
import 'package:eutonafila_frontend/services/queue_service.dart';
import 'package:eutonafila_frontend/services/api_client.dart';
import 'package:eutonafila_frontend/models/queue_models.dart';

import 'queue_service_test.mocks.dart';

@GenerateMocks([ApiClient])
void main() {
  late QueueService queueService;
  late MockApiClient mockApiClient;

  setUp(() {
    mockApiClient = MockApiClient();
    queueService = QueueService(apiClient: mockApiClient);
  });

  group('QueueService', () {
    group('addQueue', () {
      test('should successfully add a new queue', () async {
        // Arrange
        final request = AddQueueRequest(
          locationId: 'location-123',
          maxSize: 50,
          lateClientCapTimeInMinutes: 15,
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Queues'),
          statusCode: 200,
          data: {'queueId': 'queue-123', 'success': true},
        );

        when(mockApiClient.post('/Queues', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await queueService.addQueue(request);

        // Assert
        expect(result['queueId'], equals('queue-123'));
        expect(result['success'], isTrue);
        verify(mockApiClient.post('/Queues', data: request.toJson())).called(1);
      });

      test('should handle API errors when adding queue', () async {
        // Arrange
        final request = AddQueueRequest(
          locationId: 'location-123',
          maxSize: 50,
        );

        when(mockApiClient.post('/Queues', data: anyNamed('data')))
            .thenThrow(DioException(
              requestOptions: RequestOptions(path: '/Queues'),
              message: 'Network error',
            ));

        // Act & Assert
        expect(() => queueService.addQueue(request), throwsA(isA<DioException>()));
      });
    });

    group('joinQueue', () {
      test('should successfully join a queue', () async {
        // Arrange
        final request = JoinQueueRequest(
          queueId: 'queue-123',
          customerName: 'João Silva',
          phoneNumber: '+5511999999999',
          email: 'joao@example.com',
          serviceTypeId: 'service-123',
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Queues/queue-123/join'),
          statusCode: 200,
          data: {
            'queueEntryId': 'entry-123',
            'position': 5,
            'estimatedWaitTime': 25,
          },
        );

        when(mockApiClient.post('/Queues/queue-123/join', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await queueService.joinQueue('queue-123', request);

        // Assert
        expect(result['queueEntryId'], equals('entry-123'));
        expect(result['position'], equals(5));
        expect(result['estimatedWaitTime'], equals(25));
        verify(mockApiClient.post('/Queues/queue-123/join', data: request.toJson())).called(1);
      });
    });

    group('callNext', () {
      test('should successfully call next customer', () async {
        // Arrange
        final request = CallNextRequest(
          staffMemberId: 'staff-123',
          queueId: 'queue-123',
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Queues/queue-123/call-next'),
          statusCode: 200,
          data: {
            'calledCustomer': 'João Silva',
            'queueEntryId': 'entry-123',
          },
        );

        when(mockApiClient.post('/Queues/queue-123/call-next', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await queueService.callNext('queue-123', request);

        // Assert
        expect(result['calledCustomer'], equals('João Silva'));
        expect(result['queueEntryId'], equals('entry-123'));
        verify(mockApiClient.post('/Queues/queue-123/call-next', data: request.toJson())).called(1);
      });
    });

    group('checkIn', () {
      test('should successfully check in customer', () async {
        // Arrange
        final request = CheckInRequest(queueEntryId: 'entry-123');

        final response = Response(
          requestOptions: RequestOptions(path: '/Queues/queue-123/check-in'),
          statusCode: 200,
          data: {
            'success': true,
            'message': 'Customer checked in successfully',
          },
        );

        when(mockApiClient.post('/Queues/queue-123/check-in', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await queueService.checkIn('queue-123', request);

        // Assert
        expect(result['success'], isTrue);
        expect(result['message'], equals('Customer checked in successfully'));
        verify(mockApiClient.post('/Queues/queue-123/check-in', data: request.toJson())).called(1);
      });
    });

    group('finishService', () {
      test('should successfully finish service', () async {
        // Arrange
        final request = FinishRequest(
          queueEntryId: 'entry-123',
          serviceDurationMinutes: 30,
          notes: 'Service completed successfully',
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Queues/queue-123/finish'),
          statusCode: 200,
          data: {
            'success': true,
            'totalServiceTime': 30,
          },
        );

        when(mockApiClient.post('/Queues/queue-123/finish', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await queueService.finishService('queue-123', request);

        // Assert
        expect(result['success'], isTrue);
        expect(result['totalServiceTime'], equals(30));
        verify(mockApiClient.post('/Queues/queue-123/finish', data: request.toJson())).called(1);
      });
    });

    group('cancelQueue', () {
      test('should successfully cancel queue entry', () async {
        // Arrange
        final request = CancelQueueRequest(queueEntryId: 'entry-123');

        final response = Response(
          requestOptions: RequestOptions(path: '/Queues/queue-123/cancel'),
          statusCode: 200,
          data: {
            'success': true,
            'message': 'Queue entry cancelled',
          },
        );

        when(mockApiClient.post('/Queues/queue-123/cancel', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await queueService.cancelQueue('queue-123', request);

        // Assert
        expect(result['success'], isTrue);
        expect(result['message'], equals('Queue entry cancelled'));
        verify(mockApiClient.post('/Queues/queue-123/cancel', data: request.toJson())).called(1);
      });
    });

    group('getQueue', () {
      test('should successfully get queue details', () async {
        // Arrange
        final response = Response(
          requestOptions: RequestOptions(path: '/Queues/queue-123'),
          statusCode: 200,
          data: {
            'queueId': 'queue-123',
            'locationId': 'location-123',
            'customersWaiting': 5,
            'isActive': true,
          },
        );

        when(mockApiClient.get('/Queues/queue-123'))
            .thenAnswer((_) async => response);

        // Act
        final result = await queueService.getQueue('queue-123');

        // Assert
        expect(result['queueId'], equals('queue-123'));
        expect(result['locationId'], equals('location-123'));
        expect(result['customersWaiting'], equals(5));
        expect(result['isActive'], isTrue);
        verify(mockApiClient.get('/Queues/queue-123')).called(1);
      });
    });

    group('getWaitTime', () {
      test('should successfully get wait time for queue entry', () async {
        // Arrange
        final response = Response(
          requestOptions: RequestOptions(path: '/Queues/queue-123/entries/entry-123/wait-time'),
          statusCode: 200,
          data: {
            'estimatedWaitTimeMinutes': 15,
            'position': 3,
            'peopleAhead': 2,
          },
        );

        when(mockApiClient.get('/Queues/queue-123/entries/entry-123/wait-time'))
            .thenAnswer((_) async => response);

        // Act
        final result = await queueService.getWaitTime('queue-123', 'entry-123');

        // Assert
        expect(result['estimatedWaitTimeMinutes'], equals(15));
        expect(result['position'], equals(3));
        expect(result['peopleAhead'], equals(2));
        verify(mockApiClient.get('/Queues/queue-123/entries/entry-123/wait-time')).called(1);
      });
    });

    group('saveHaircutDetails', () {
      test('should successfully save haircut details', () async {
        // Arrange
        final request = SaveHaircutDetailsRequest(
          queueEntryId: 'entry-123',
          haircutDetails: 'Short sides, long top',
          additionalNotes: 'Customer was satisfied',
          photoUrl: 'https://example.com/photo.jpg',
        );

        final expectedResult = SaveHaircutDetailsResult(
          success: true,
          serviceHistoryId: 'history-123',
          customerId: 'customer-123',
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Queues/entries/entry-123/haircut-details'),
          statusCode: 200,
          data: expectedResult.toJson(),
        );

        when(mockApiClient.post('/Queues/entries/entry-123/haircut-details', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await queueService.saveHaircutDetails('entry-123', request);

        // Assert
        expect(result.success, isTrue);
        expect(result.serviceHistoryId, equals('history-123'));
        expect(result.customerId, equals('customer-123'));
        verify(mockApiClient.post('/Queues/entries/entry-123/haircut-details', data: request.toJson())).called(1);
      });
    });
  });
}