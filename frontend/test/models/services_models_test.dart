import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/models/services_models.dart';

void main() {
  group('AddServiceOfferedRequest', () {
    test('should create AddServiceOfferedRequest with all fields', () {
      final request = AddServiceOfferedRequest(
        locationId: 'location-123',
        name: 'Corte Masculino',
        description: 'Corte de cabelo tradicional masculino',
        estimatedDurationMinutes: 30,
        price: 25.0,
        imageUrl: 'https://example.com/haircut.jpg',
      );

      expect(request.locationId, equals('location-123'));
      expect(request.name, equals('Corte Masculino'));
      expect(request.description, equals('Corte de cabelo tradicional masculino'));
      expect(request.estimatedDurationMinutes, equals(30));
      expect(request.price, equals(25.0));
      expect(request.imageUrl, equals('https://example.com/haircut.jpg'));
    });

    test('should serialize to JSON correctly', () {
      final request = AddServiceOfferedRequest(
        locationId: 'location-123',
        name: 'Corte Masculino',
        description: 'Corte de cabelo tradicional masculino',
        estimatedDurationMinutes: 30,
        price: 25.0,
      );

      final json = request.toJson();
      expect(json['locationId'], equals('location-123'));
      expect(json['name'], equals('Corte Masculino'));
      expect(json['description'], equals('Corte de cabelo tradicional masculino'));
      expect(json['estimatedDurationMinutes'], equals(30));
      expect(json['price'], equals(25.0));
    });
  });

  group('UpdateServiceOfferedRequest', () {
    test('should create UpdateServiceOfferedRequest with all fields', () {
      final request = UpdateServiceOfferedRequest(
        name: 'Corte e Barba',
        description: 'Corte de cabelo e barba completa',
        locationId: 'location-123',
        estimatedDurationMinutes: 45,
        price: 35.0,
        imageUrl: 'https://example.com/haircut-beard.jpg',
        isActive: true,
      );

      expect(request.name, equals('Corte e Barba'));
      expect(request.description, equals('Corte de cabelo e barba completa'));
      expect(request.locationId, equals('location-123'));
      expect(request.estimatedDurationMinutes, equals(45));
      expect(request.price, equals(35.0));
      expect(request.imageUrl, equals('https://example.com/haircut-beard.jpg'));
      expect(request.isActive, isTrue);
    });

    test('should support partial updates', () {
      final request = UpdateServiceOfferedRequest(
        name: 'Corte Premium',
        price: 40.0,
        isActive: true,
      );

      expect(request.name, equals('Corte Premium'));
      expect(request.price, equals(40.0));
      expect(request.isActive, isTrue);
      expect(request.description, isNull);
      expect(request.imageUrl, isNull);
    });
  });

  group('ServiceOfferedDto', () {
    test('should create ServiceOfferedDto with all fields', () {
      final service = ServiceOfferedDto(
        id: 'service-123',
        locationId: 'location-123',
        name: 'Corte Masculino',
        description: 'Corte de cabelo tradicional masculino',
        estimatedDurationMinutes: 30,
        price: 25.0,
        imageUrl: 'https://example.com/haircut.jpg',
        isActive: true,
        createdAt: DateTime(2024, 1, 15),
        updatedAt: DateTime(2024, 1, 20),
      );

      expect(service.id, equals('service-123'));
      expect(service.locationId, equals('location-123'));
      expect(service.name, equals('Corte Masculino'));
      expect(service.description, equals('Corte de cabelo tradicional masculino'));
      expect(service.estimatedDurationMinutes, equals(30));
      expect(service.price, equals(25.0));
      expect(service.imageUrl, equals('https://example.com/haircut.jpg'));
      expect(service.isActive, isTrue);
      expect(service.createdAt, equals(DateTime(2024, 1, 15)));
      expect(service.updatedAt, equals(DateTime(2024, 1, 20)));
    });

    test('should serialize to JSON correctly', () {
      final service = ServiceOfferedDto(
        id: 'service-123',
        locationId: 'location-123',
        name: 'Corte Masculino',
        description: 'Corte de cabelo tradicional masculino',
        estimatedDurationMinutes: 30,
        price: 25.0,
        isActive: true,
        createdAt: DateTime(2024, 1, 15),
        updatedAt: DateTime(2024, 1, 20),
      );

      final json = service.toJson();
      expect(json['id'], equals('service-123'));
      expect(json['locationId'], equals('location-123'));
      expect(json['name'], equals('Corte Masculino'));
      expect(json['estimatedDurationMinutes'], equals(30));
      expect(json['price'], equals(25.0));
      expect(json['isActive'], isTrue);
    });
  });

  group('ServiceOperationResult', () {
    test('should create successful result', () {
      final result = ServiceOperationResult(
        success: true,
        serviceId: 'service-123',
        fieldErrors: {},
        errors: [],
      );

      expect(result.success, isTrue);
      expect(result.serviceId, equals('service-123'));
      expect(result.fieldErrors, isEmpty);
      expect(result.errors, isEmpty);
    });

    test('should create failed result with errors', () {
      final result = ServiceOperationResult(
        success: false,
        serviceId: null,
        fieldErrors: {'name': 'Service name is required'},
        errors: ['Price must be greater than 0'],
      );

      expect(result.success, isFalse);
      expect(result.serviceId, isNull);
      expect(result.fieldErrors!['name'], equals('Service name is required'));
      expect(result.errors!.first, equals('Price must be greater than 0'));
    });
  });
}