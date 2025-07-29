import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:dio/dio.dart';
import 'package:eutonafila_frontend/services/services_service.dart';
import 'package:eutonafila_frontend/services/api_client.dart';
import 'package:eutonafila_frontend/models/services_models.dart';

import 'services_service_test.mocks.dart';

@GenerateMocks([ApiClient])
void main() {
  late ServicesService servicesService;
  late MockApiClient mockApiClient;

  setUp(() {
    mockApiClient = MockApiClient();
    servicesService = ServicesService(apiClient: mockApiClient);
  });

  group('ServicesService', () {
    group('addService', () {
      test('should successfully add a new service', () async {
        // Arrange
        final request = AddServiceOfferedRequest(
          locationId: 'location-123',
          name: 'Corte Masculino',
          description: 'Corte de cabelo tradicional masculino',
          estimatedDurationMinutes: 30,
          price: 25.0,
          imageUrl: 'https://example.com/haircut.jpg',
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/ServicesOffered'),
          statusCode: 200,
          data: {
            'success': true,
            'serviceId': 'service-123',
            'message': 'Service created successfully',
          },
        );

        when(mockApiClient.post('/ServicesOffered', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await servicesService.addService(request);

        // Assert
        expect(result['success'], isTrue);
        expect(result['serviceId'], equals('service-123'));
        expect(result['message'], equals('Service created successfully'));
        verify(mockApiClient.post('/ServicesOffered', data: request.toJson())).called(1);
      });
    });

    group('getServices', () {
      test('should successfully get all services', () async {
        // Arrange
        final response = Response(
          requestOptions: RequestOptions(path: '/ServicesOffered'),
          statusCode: 200,
          data: [
            {
              'id': 'service-123',
              'name': 'Corte Masculino',
              'description': 'Corte de cabelo tradicional masculino',
              'price': 25.0,
              'estimatedDurationMinutes': 30,
              'isActive': true,
            },
            {
              'id': 'service-456',
              'name': 'Corte e Barba',
              'description': 'Corte de cabelo e barba completa',
              'price': 35.0,
              'estimatedDurationMinutes': 45,
              'isActive': true,
            },
          ],
        );

        when(mockApiClient.get('/ServicesOffered'))
            .thenAnswer((_) async => response);

        // Act
        final result = await servicesService.getServices();

        // Assert
        expect(result, isA<List>());
        expect(result.length, equals(2));
        expect(result[0]['name'], equals('Corte Masculino'));
        expect(result[1]['name'], equals('Corte e Barba'));
        verify(mockApiClient.get('/ServicesOffered')).called(1);
      });
    });

    group('getService', () {
      test('should successfully get service details', () async {
        // Arrange
        final response = Response(
          requestOptions: RequestOptions(path: '/ServicesOffered/service-123'),
          statusCode: 200,
          data: {
            'id': 'service-123',
            'locationId': 'location-123',
            'name': 'Corte Masculino',
            'description': 'Corte de cabelo tradicional masculino',
            'estimatedDurationMinutes': 30,
            'price': 25.0,
            'imageUrl': 'https://example.com/haircut.jpg',
            'isActive': true,
          },
        );

        when(mockApiClient.get('/ServicesOffered/service-123'))
            .thenAnswer((_) async => response);

        // Act
        final result = await servicesService.getService('service-123');

        // Assert
        expect(result['id'], equals('service-123'));
        expect(result['name'], equals('Corte Masculino'));
        expect(result['price'], equals(25.0));
        expect(result['isActive'], isTrue);
        verify(mockApiClient.get('/ServicesOffered/service-123')).called(1);
      });
    });

    group('updateService', () {
      test('should successfully update service', () async {
        // Arrange
        final request = UpdateServiceOfferedRequest(
          name: 'Corte Masculino Premium',
          description: 'Corte de cabelo masculino com acabamento premium',
          estimatedDurationMinutes: 35,
          price: 30.0,
          imageUrl: 'https://example.com/premium-haircut.jpg',
          isActive: true,
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/ServicesOffered/service-123'),
          statusCode: 200,
          data: {
            'success': true,
            'message': 'Service updated successfully',
          },
        );

        when(mockApiClient.put('/ServicesOffered/service-123', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await servicesService.updateService('service-123', request);

        // Assert
        expect(result['success'], isTrue);
        expect(result['message'], equals('Service updated successfully'));
        verify(mockApiClient.put('/ServicesOffered/service-123', data: request.toJson())).called(1);
      });
    });

    group('deleteService', () {
      test('should successfully delete service', () async {
        // Arrange
        final response = Response(
          requestOptions: RequestOptions(path: '/ServicesOffered/service-123'),
          statusCode: 200,
          data: {
            'success': true,
            'message': 'Service deleted successfully',
          },
        );

        when(mockApiClient.delete('/ServicesOffered/service-123'))
            .thenAnswer((_) async => response);

        // Act
        final result = await servicesService.deleteService('service-123');

        // Assert
        expect(result['success'], isTrue);
        expect(result['message'], equals('Service deleted successfully'));
        verify(mockApiClient.delete('/ServicesOffered/service-123')).called(1);
      });
    });

    group('activateService', () {
      test('should successfully activate service', () async {
        // Arrange
        final response = Response(
          requestOptions: RequestOptions(path: '/ServicesOffered/service-123/activate'),
          statusCode: 200,
          data: {
            'success': true,
            'message': 'Service activated successfully',
          },
        );

        when(mockApiClient.put('/ServicesOffered/service-123/activate'))
            .thenAnswer((_) async => response);

        // Act
        final result = await servicesService.activateService('service-123');

        // Assert
        expect(result['success'], isTrue);
        expect(result['message'], equals('Service activated successfully'));
        verify(mockApiClient.put('/ServicesOffered/service-123/activate')).called(1);
      });
    });
  });
}