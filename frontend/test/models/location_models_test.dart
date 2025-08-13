import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/models/location_models.dart';

void main() {
  group('Location Models Tests', () {
    group('LocationAddressRequest', () {
      test('should create with all fields', () {
        const address = LocationAddressRequest(
          street: '123 Main St',
          city: 'Anytown',
          state: 'CA',
          postalCode: '12345',
          country: 'USA',
        );

        expect(address.street, equals('123 Main St'));
        expect(address.city, equals('Anytown'));
        expect(address.state, equals('CA'));
        expect(address.postalCode, equals('12345'));
        expect(address.country, equals('USA'));
      });

      test('should create with optional fields as null', () {
        const address = LocationAddressRequest(
          street: '456 Oak Ave',
          city: 'Small Town',
        );

        expect(address.street, equals('456 Oak Ave'));
        expect(address.city, equals('Small Town'));
        expect(address.state, isNull);
        expect(address.postalCode, isNull);
        expect(address.country, isNull);
      });

      test('should serialize to JSON correctly', () {
        const address = LocationAddressRequest(
          street: '789 Pine Rd',
          city: 'Big City',
          state: 'NY',
          postalCode: '67890',
          country: 'United States',
        );

        final json = address.toJson();

        expect(json['street'], equals('789 Pine Rd'));
        expect(json['city'], equals('Big City'));
        expect(json['state'], equals('NY'));
        expect(json['postalCode'], equals('67890'));
        expect(json['country'], equals('United States'));
      });

      test('should deserialize from JSON correctly', () {
        final json = {
          'street': '321 Elm St',
          'city': 'Test City',
          'state': 'TX',
          'postalCode': '54321',
          'country': 'US',
        };

        final address = LocationAddressRequest.fromJson(json);

        expect(address.street, equals('321 Elm St'));
        expect(address.city, equals('Test City'));
        expect(address.state, equals('TX'));
        expect(address.postalCode, equals('54321'));
        expect(address.country, equals('US'));
      });
    });

    group('CreateLocationRequest', () {
      test('should create with required fields', () {
        const address = LocationAddressRequest(
          street: '100 Salon St',
          city: 'Beauty Town',
          state: 'CA',
          postalCode: '90210',
        );

        const request = CreateLocationRequest(
          businessName: 'Premium Cuts',
          contactEmail: 'info@premiumcuts.com',
          contactPhone: '+1-555-0123',
          address: address,
          maxQueueCapacity: 50,
        );

        expect(request.businessName, equals('Premium Cuts'));
        expect(request.contactEmail, equals('info@premiumcuts.com'));
        expect(request.contactPhone, equals('+1-555-0123'));
        expect(request.address, equals(address));
        expect(request.maxQueueCapacity, equals(50));
        expect(request.businessHours, isNull);
        expect(request.description, isNull);
        expect(request.website, isNull);
      });

      test('should create with all fields', () {
        const address = LocationAddressRequest(
          street: '200 Style Ave',
          city: 'Fashion City',
          state: 'NY',
          postalCode: '10001',
          country: 'USA',
        );

        var businessHours = {
          'Monday': '9:00 AM - 6:00 PM',
          'Tuesday': '9:00 AM - 6:00 PM',
          'Wednesday': '9:00 AM - 6:00 PM',
          'Thursday': '9:00 AM - 8:00 PM',
          'Friday': '9:00 AM - 8:00 PM',
          'Saturday': '8:00 AM - 5:00 PM',
          'Sunday': 'Closed',
        };

        var request = CreateLocationRequest(
          businessName: 'Luxury Salon & Spa',
          contactEmail: 'contact@luxurysalon.com',
          contactPhone: '+1-555-0456',
          address: address,
          businessHours: businessHours,
          maxQueueCapacity: 75,
          description: 'Full-service luxury salon and spa',
          website: 'https://luxurysalon.com',
        );

        expect(request.businessName, equals('Luxury Salon & Spa'));
        expect(request.contactEmail, equals('contact@luxurysalon.com'));
        expect(request.contactPhone, equals('+1-555-0456'));
        expect(request.address, equals(address));
        expect(request.businessHours, equals(businessHours));
        expect(request.maxQueueCapacity, equals(75));
        expect(request.description, equals('Full-service luxury salon and spa'));
        expect(request.website, equals('https://luxurysalon.com'));
      });

      test('should serialize to JSON correctly', () {
        const address = LocationAddressRequest(
          street: '300 Beauty Blvd',
          city: 'Glamour City',
          state: 'CA',
          postalCode: '90211',
        );

        var businessHours = {
          'Monday': '10:00 AM - 7:00 PM',
          'Sunday': 'Closed',
        };

        var request = CreateLocationRequest(
          businessName: 'JSON Salon',
          contactEmail: 'test@jsonsalon.com',
          contactPhone: '+1-555-JSON',
          address: address,
          businessHours: businessHours,
          maxQueueCapacity: 30,
          description: 'Test salon for JSON',
        );

        final json = request.toJson();

        expect(json['businessName'], equals('JSON Salon'));
        expect(json['contactEmail'], equals('test@jsonsalon.com'));
        expect(json['contactPhone'], equals('+1-555-JSON'));
        expect(json['address'], equals(address.toJson()));
        expect(json['businessHours'], equals(businessHours));
        expect(json['maxQueueCapacity'], equals(30));
        expect(json['description'], equals('Test salon for JSON'));
      });

      test('should deserialize from JSON correctly', () {
        final json = {
          'businessName': 'Deserialized Salon',
          'contactEmail': 'deser@salon.com',
          'contactPhone': '+1-555-DESER',
          'address': {
            'street': '400 Parse Ave',
            'city': 'JSON City',
            'state': 'FL',
            'postalCode': '33101',
          },
          'maxQueueCapacity': 40,
          'description': 'Deserialized from JSON',
          'website': 'https://deserialized.com',
        };

        final request = CreateLocationRequest.fromJson(json);

        expect(request.businessName, equals('Deserialized Salon'));
        expect(request.contactEmail, equals('deser@salon.com'));
        expect(request.contactPhone, equals('+1-555-DESER'));
        expect(request.address?.street, equals('400 Parse Ave'));
        expect(request.address?.city, equals('JSON City'));
        expect(request.address?.state, equals('FL'));
        expect(request.address?.postalCode, equals('33101'));
        expect(request.maxQueueCapacity, equals(40));
        expect(request.description, equals('Deserialized from JSON'));
        expect(request.website, equals('https://deserialized.com'));
      });
    });

    group('UpdateLocationRequest', () {
      test('should create with basic fields', () {
        const address = LocationAddressRequest(
          street: '500 Update St',
          city: 'Change City',
        );

        const request = UpdateLocationRequest(
          businessName: 'Updated Salon',
          address: address,
          maxQueueCapacity: 60,
        );

        expect(request.businessName, equals('Updated Salon'));
        expect(request.address, equals(address));
        expect(request.maxQueueCapacity, equals(60));
        expect(request.businessHours, isNull);
        expect(request.description, isNull);
      });

      test('should create with all fields', () {
        const address = LocationAddressRequest(
          street: '600 Full Update Ave',
          city: 'Complete City',
          state: 'TX',
          postalCode: '75001',
          country: 'United States',
        );

        var businessHours = {
          'Monday': '8:00 AM - 9:00 PM',
          'Tuesday': '8:00 AM - 9:00 PM',
          'Sunday': '10:00 AM - 6:00 PM',
        };

        var request = UpdateLocationRequest(
          businessName: 'Fully Updated Salon',
          address: address,
          businessHours: businessHours,
          maxQueueCapacity: 100,
          description: 'Now with extended hours and more capacity',
        );

        expect(request.businessName, equals('Fully Updated Salon'));
        expect(request.address, equals(address));
        expect(request.businessHours, equals(businessHours));
        expect(request.maxQueueCapacity, equals(100));
        expect(request.description, equals('Now with extended hours and more capacity'));
      });

      test('should serialize to JSON correctly', () {
        const address = LocationAddressRequest(
          street: '700 JSON Update Rd',
          city: 'Serialize City',
          state: 'NV',
        );

        const request = UpdateLocationRequest(
          businessName: 'JSON Updated Salon',
          address: address,
          maxQueueCapacity: 80,
          description: 'Updated via JSON serialization',
        );

        final json = request.toJson();

        expect(json['businessName'], equals('JSON Updated Salon'));
        expect(json['address'], equals(address.toJson()));
        expect(json['maxQueueCapacity'], equals(80));
        expect(json['description'], equals('Updated via JSON serialization'));
      });

      test('should deserialize from JSON correctly', () {
        final json = {
          'businessName': 'JSON Parsed Salon',
          'address': {
            'street': '800 Deserialize Blvd',
            'city': 'Parse Town',
            'state': 'OR',
            'postalCode': '97001',
          },
          'businessHours': {
            'Monday': '9:00 AM - 5:00 PM',
            'Friday': '9:00 AM - 8:00 PM',
          },
          'maxQueueCapacity': 90,
          'description': 'Parsed from JSON successfully',
        };

        final request = UpdateLocationRequest.fromJson(json);

        expect(request.businessName, equals('JSON Parsed Salon'));
        expect(request.address?.street, equals('800 Deserialize Blvd'));
        expect(request.address?.city, equals('Parse Town'));
        expect(request.businessHours!['Monday'], equals('9:00 AM - 5:00 PM'));
        expect(request.businessHours!['Friday'], equals('9:00 AM - 8:00 PM'));
        expect(request.maxQueueCapacity, equals(90));
        expect(request.description, equals('Parsed from JSON successfully'));
      });
    });

    group('ToggleQueueRequest', () {
      test('should create with location ID and enable flag', () {
        const request = ToggleQueueRequest(
          locationId: 'loc-123',
          enableQueue: true,
        );

        expect(request.locationId, equals('loc-123'));
        expect(request.enableQueue, isTrue);
      });

      test('should create with disable flag', () {
        const request = ToggleQueueRequest(
          locationId: 'loc-456',
          enableQueue: false,
        );

        expect(request.locationId, equals('loc-456'));
        expect(request.enableQueue, isFalse);
      });

      test('should serialize to JSON correctly', () {
        const request = ToggleQueueRequest(
          locationId: 'loc-789',
          enableQueue: true,
        );

        final json = request.toJson();

        expect(json['locationId'], equals('loc-789'));
        expect(json['enableQueue'], isTrue);
      });

      test('should deserialize from JSON correctly', () {
        final json = {
          'locationId': 'loc-json-test',
          'enableQueue': false,
        };

        final request = ToggleQueueRequest.fromJson(json);

        expect(request.locationId, equals('loc-json-test'));
        expect(request.enableQueue, isFalse);
      });
    });
  });
}