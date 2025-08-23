import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:dio/dio.dart';
import 'package:eutonafila_frontend/services/queue_analytics_service.dart';
import 'package:eutonafila_frontend/services/api_client.dart';
import 'package:eutonafila_frontend/models/analytics_models.dart';

import 'queue_analytics_service_test.mocks.dart';

@GenerateMocks([ApiClient])
void main() {
  late QueueAnalyticsService analyticsService;
  late MockApiClient mockApiClient;

  setUp(() {
    mockApiClient = MockApiClient();
    analyticsService = QueueAnalyticsService(apiClient: mockApiClient);
  });

  group('QueueAnalyticsService', () {
    group('getEstimatedWaitTime', () {
      test('should successfully get estimated wait time', () async {
        // Arrange
        const salonId = 'salon-123';
        const serviceType = 'haircut';
        
        final response = Response(
          requestOptions: RequestOptions(path: '/QueueAnalytics/wait-time/$salonId'),
          statusCode: 200,
          data: {
            'salonId': salonId,
            'serviceType': serviceType,
            'estimatedWaitTime': 'PT25M',
            'calculatedAt': '2024-01-15T10:30:00Z',
            'confidence': 'High',
          },
        );

        when(mockApiClient.get('/QueueAnalytics/wait-time/$salonId', queryParameters: {'serviceType': serviceType}))
            .thenAnswer((_) async => response);

        // Act
        final result = await analyticsService.getEstimatedWaitTime(salonId, serviceType: serviceType);

        // Assert
        expect(result, isA<WaitTimeEstimate>());
        expect(result.waitTimeMinutes, equals(25));
        expect(result.salonId, equals(salonId));
        expect(result.serviceType, equals(serviceType));
        expect(result.confidence, equals('High'));
        verify(mockApiClient.get('/QueueAnalytics/wait-time/$salonId', queryParameters: {'serviceType': serviceType})).called(1);
      });
    });

    group('getQueuePerformance', () {
      test('should successfully get performance metrics', () async {
        // Arrange
        const salonId = 'salon-123';
        
        final response = Response(
          requestOptions: RequestOptions(path: '/QueueAnalytics/performance/$salonId'),
          statusCode: 200,
          data: {
            'salonId': salonId,
            'periodStart': '2024-01-01T00:00:00Z',
            'periodEnd': '2024-01-15T00:00:00Z',
            'totalCustomers': 45,
            'averageWaitTime': 'PT18M',
            'averageServiceTime': 'PT22M',
            'customerSatisfaction': 4.2,
            'queueEfficiency': 0.87,
            'peakWaitTime': 'PT35M',
            'peakHourCustomers': 12,
            'completedServices': 42,
            'cancelledServices': 3,
          },
        );

        when(mockApiClient.get('/QueueAnalytics/performance/$salonId', queryParameters: {}))
            .thenAnswer((_) async => response);

        // Act
        final result = await analyticsService.getQueuePerformance(salonId);

        // Assert
        expect(result, isA<QueuePerformanceMetrics>());
        expect(result.totalCustomers, equals(45));
        expect(result.averageWaitTime.inMinutes, equals(18));
        expect(result.customerSatisfaction, equals(4.2));
        verify(mockApiClient.get('/QueueAnalytics/performance/$salonId', queryParameters: {})).called(1);
      });
    });
  });
}
