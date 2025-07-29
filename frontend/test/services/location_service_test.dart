import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:dio/dio.dart';
import 'package:eutonafila_frontend/services/location_service.dart';
import 'package:eutonafila_frontend/services/api_client.dart';
import 'package:eutonafila_frontend/models/location_models.dart';

import 'location_service_test.mocks.dart';

@GenerateMocks([ApiClient])
void main() {
  late LocationService locationService;
  late MockApiClient mockApiClient;

  setUp(() {
    mockApiClient = MockApiClient();
    locationService = LocationService(apiClient: mockApiClient);
  });

  group('LocationService', () {
    group('createLocation', () {
      test('should successfully create a new location', () async {
        // Arrange
        final request = CreateLocationRequest(
          businessName: 'Barbearia do João',
          contactEmail: 'contato@barbeariadojoao.com',
          contactPhone: '+5511999999999',
          address: LocationAddressRequest(
            street: 'Rua das Flores, 123',
            city: 'São Paulo',
            state: 'SP',
            postalCode: '01234-567',
            country: 'Brasil',
          ),
          businessHours: {
            'monday': '09:00-18:00',
            'tuesday': '09:00-18:00',
            'wednesday': '09:00-18:00',
            'thursday': '09:00-18:00',
            'friday': '09:00-19:00',
            'saturday': '08:00-17:00',
            'sunday': 'closed',
          },
          maxQueueCapacity: 50,
          description: 'Barbearia tradicional no centro da cidade',
          website: 'https://barbeariadojoao.com',
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Locations'),
          statusCode: 200,
          data: {
            'locationId': 'location-123',
            'success': true,
            'message': 'Location created successfully',
          },
        );

        when(mockApiClient.post('/Locations', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await locationService.createLocation(request);

        // Assert
        expect(result['locationId'], equals('location-123'));
        expect(result['success'], isTrue);
        expect(result['message'], equals('Location created successfully'));
        verify(mockApiClient.post('/Locations', data: request.toJson())).called(1);
      });

      test('should handle API errors when creating location', () async {
        // Arrange
        final request = CreateLocationRequest(
          businessName: 'Barbearia do João',
          contactEmail: 'contato@barbeariadojoao.com',
          contactPhone: '+5511999999999',
        );

        when(mockApiClient.post('/Locations', data: anyNamed('data')))
            .thenThrow(DioException(
              requestOptions: RequestOptions(path: '/Locations'),
              message: 'Validation error',
            ));

        // Act & Assert
        expect(() => locationService.createLocation(request), throwsA(isA<DioException>()));
      });
    });

    group('getLocation', () {
      test('should successfully get location details', () async {
        // Arrange
        final response = Response(
          requestOptions: RequestOptions(path: '/Locations/location-123'),
          statusCode: 200,
          data: {
            'locationId': 'location-123',
            'businessName': 'Barbearia do João',
            'contactEmail': 'contato@barbeariadojoao.com',
            'contactPhone': '+5511999999999',
            'address': {
              'street': 'Rua das Flores, 123',
              'city': 'São Paulo',
              'state': 'SP',
              'postalCode': '01234-567',
              'country': 'Brasil',
            },
            'businessHours': {
              'monday': '09:00-18:00',
              'tuesday': '09:00-18:00',
              'wednesday': '09:00-18:00',
              'thursday': '09:00-18:00',
              'friday': '09:00-19:00',
              'saturday': '08:00-17:00',
              'sunday': 'closed',
            },
            'maxQueueCapacity': 50,
            'isActive': true,
          },
        );

        when(mockApiClient.get('/Locations/location-123'))
            .thenAnswer((_) async => response);

        // Act
        final result = await locationService.getLocation('location-123');

        // Assert
        expect(result['locationId'], equals('location-123'));
        expect(result['businessName'], equals('Barbearia do João'));
        expect(result['contactEmail'], equals('contato@barbeariadojoao.com'));
        expect(result['isActive'], isTrue);
        verify(mockApiClient.get('/Locations/location-123')).called(1);
      });
    });

    group('updateLocation', () {
      test('should successfully update location', () async {
        // Arrange
        final request = UpdateLocationRequest(
          businessName: 'Barbearia do João - Unidade Centro',
          address: LocationAddressRequest(
            street: 'Rua das Flores, 456',
            city: 'São Paulo',
            state: 'SP',
            postalCode: '01234-890',
            country: 'Brasil',
          ),
          businessHours: {
            'monday': '08:00-19:00',
            'tuesday': '08:00-19:00',
            'wednesday': '08:00-19:00',
            'thursday': '08:00-19:00',
            'friday': '08:00-20:00',
            'saturday': '07:00-18:00',
            'sunday': '08:00-15:00',
          },
          maxQueueCapacity: 75,
          description: 'Barbearia tradicional no centro da cidade - Agora com horário estendido',
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Locations/location-123'),
          statusCode: 200,
          data: {
            'success': true,
            'message': 'Location updated successfully',
          },
        );

        when(mockApiClient.put('/Locations/location-123', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await locationService.updateLocation('location-123', request);

        // Assert
        expect(result['success'], isTrue);
        expect(result['message'], equals('Location updated successfully'));
        verify(mockApiClient.put('/Locations/location-123', data: request.toJson())).called(1);
      });
    });

    group('deleteLocation', () {
      test('should successfully delete location', () async {
        // Arrange
        final response = Response(
          requestOptions: RequestOptions(path: '/Locations/location-123'),
          statusCode: 200,
          data: {
            'success': true,
            'message': 'Location deleted successfully',
          },
        );

        when(mockApiClient.delete('/Locations/location-123'))
            .thenAnswer((_) async => response);

        // Act
        final result = await locationService.deleteLocation('location-123');

        // Assert
        expect(result['success'], isTrue);
        expect(result['message'], equals('Location deleted successfully'));
        verify(mockApiClient.delete('/Locations/location-123')).called(1);
      });
    });

    group('toggleQueueStatus', () {
      test('should successfully toggle queue status', () async {
        // Arrange
        final request = ToggleQueueRequest(
          locationId: 'location-123',
          enableQueue: true,
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Locations/location-123/queue-status'),
          statusCode: 200,
          data: {
            'success': true,
            'queueEnabled': true,
            'message': 'Queue status updated successfully',
          },
        );

        when(mockApiClient.put('/Locations/location-123/queue-status', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await locationService.toggleQueueStatus('location-123', request);

        // Assert
        expect(result['success'], isTrue);
        expect(result['queueEnabled'], isTrue);
        expect(result['message'], equals('Queue status updated successfully'));
        verify(mockApiClient.put('/Locations/location-123/queue-status', data: request.toJson())).called(1);
      });
    });
  });
}