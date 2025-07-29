import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/services/public_salon_service.dart';
import 'package:eutonafila_frontend/controllers/anonymous_controller.dart';
import 'package:eutonafila_frontend/models/public_salon.dart';

void main() {
  group('Anonymous Salon Browsing Integration Tests', () {
    late PublicSalonService publicSalonService;
    late AnonymousController anonymousController;

    setUp(() {
      publicSalonService = PublicSalonService.create();
      anonymousController = AnonymousController(publicSalonService: publicSalonService);
    });

    group('Public Salon Service Integration', () {
      test('should get public salons (fallback to mock)', () async {
        final salons = await publicSalonService.getPublicSalons();
        
        expect(salons, isA<List<PublicSalon>>());
        expect(salons.isNotEmpty, isTrue);
        
        // Verify mock data structure
        final firstSalon = salons.first;
        expect(firstSalon.id, isNotEmpty);
        expect(firstSalon.name, isNotEmpty);
        expect(firstSalon.address, isNotEmpty);
        expect(firstSalon.isOpen, isA<bool>());
      });

      test('should get nearby salons with parameters', () async {
        final salons = await publicSalonService.getNearbySlons(
          latitude: -23.5505,
          longitude: -46.6333,
          radiusKm: 5.0,
          openOnly: true,
        );
        
        expect(salons, isA<List<PublicSalon>>());
        expect(salons.isNotEmpty, isTrue);
      });

      test('should get queue status for salon', () async {
        const salonId = 'salon-1';
        final queueStatus = await publicSalonService.getQueueStatus(salonId);
        
        expect(queueStatus, isA<PublicQueueStatus>());
        expect(queueStatus, isNotNull);
        expect(queueStatus!.salonId, equals(salonId));
        expect(queueStatus.queueLength, isA<int>());
        expect(queueStatus.isAcceptingCustomers, isA<bool>());
      });

      test('should get salon details', () async {
        const salonId = 'salon-1';
        final salon = await publicSalonService.getSalonDetails(salonId);
        
        expect(salon, isA<PublicSalon>());
        expect(salon!.id, equals(salonId));
        expect(salon.name, isNotEmpty);
        expect(salon.address, isNotEmpty);
      });
    });

    group('Anonymous Controller Integration', () {
      test('should load public salons through controller', () async {
        await anonymousController.loadPublicSalons();
        
        expect(anonymousController.isLoading, isFalse);
        expect(anonymousController.error, isNull);
        expect(anonymousController.nearbySalons.isNotEmpty, isTrue);
        
        // Verify controller state
        final salons = anonymousController.nearbySalons;
        expect(salons.length, greaterThan(0));
        expect(salons.first.name, isNotEmpty);
      });

      test('should load nearby salons with location', () async {
        await anonymousController.loadNearbySalons(
          latitude: -23.5505,
          longitude: -46.6333,
          radiusKm: 10.0,
        );
        
        expect(anonymousController.isLoading, isFalse);
        expect(anonymousController.error, isNull);
        expect(anonymousController.nearbySalons.isNotEmpty, isTrue);
      });

      test('should get queue status through controller', () async {
        const salonId = 'salon-1';
        await anonymousController.getQueueStatus(salonId);
        
        expect(anonymousController.isLoading, isFalse);
        expect(anonymousController.error, isNull);
        expect(anonymousController.selectedQueueStatus, isNotNull);
        expect(anonymousController.selectedQueueStatus!.salonId, equals(salonId));
      });

      test('should get salon details through controller', () async {
        const salonId = 'salon-1';
        await anonymousController.getSalonDetails(salonId);
        
        expect(anonymousController.isLoading, isFalse);
        expect(anonymousController.error, isNull);
        expect(anonymousController.selectedSalon, isNotNull);
        expect(anonymousController.selectedSalon!.id, equals(salonId));
      });

      test('should filter salons by various criteria', () async {
        await anonymousController.loadPublicSalons();
        
        // Test filtering methods
        final fastSalons = anonymousController.fastSalons;
        final popularSalons = anonymousController.popularSalons;
        final openSalons = anonymousController.openSalons;
        final salonsByDistance = anonymousController.salonsByDistance;
        final salonsByWaitTime = anonymousController.salonsByWaitTime;
        
        expect(fastSalons, isA<List<PublicSalon>>());
        expect(popularSalons, isA<List<PublicSalon>>());
        expect(openSalons, isA<List<PublicSalon>>());
        expect(salonsByDistance, isA<List<PublicSalon>>());
        expect(salonsByWaitTime, isA<List<PublicSalon>>());
        
        // Verify filtering logic
        for (final salon in fastSalons) {
          expect(salon.isFast, isTrue);
        }
        
        for (final salon in popularSalons) {
          expect(salon.isPopular, isTrue);
        }
        
        for (final salon in openSalons) {
          expect(salon.isOpen, isTrue);
        }
      });

      test('should handle check-in attempts correctly', () async {
        const salonId = 'salon-1';
        final canCheckIn = await anonymousController.attemptCheckIn(salonId);
        
        // Should return false for anonymous users
        expect(canCheckIn, isFalse);
      });

      test('should refresh data correctly', () async {
        // Load initial data
        await anonymousController.loadPublicSalons();
        final initialCount = anonymousController.nearbySalons.length;
        
        // Refresh
        await anonymousController.refresh();
        
        expect(anonymousController.isLoading, isFalse);
        expect(anonymousController.error, isNull);
        expect(anonymousController.nearbySalons.length, equals(initialCount));
      });

      test('should clear state correctly', () async {
        // Load some data
        await anonymousController.loadPublicSalons();
        await anonymousController.getQueueStatus('salon-1');
        await anonymousController.getSalonDetails('salon-1');
        
        // Verify data is loaded
        expect(anonymousController.nearbySalons.isNotEmpty, isTrue);
        expect(anonymousController.selectedQueueStatus, isNotNull);
        expect(anonymousController.selectedSalon, isNotNull);
        
        // Clear
        anonymousController.clear();
        
        // Verify cleared state
        expect(anonymousController.nearbySalons.isEmpty, isTrue);
        expect(anonymousController.selectedQueueStatus, isNull);
        expect(anonymousController.selectedSalon, isNull);
        expect(anonymousController.error, isNull);
        expect(anonymousController.isLoading, isFalse);
      });

      test('should handle error states correctly', () async {
        // Clear error
        anonymousController.clearError();
        expect(anonymousController.error, isNull);
      });
    });

    group('Data Validation', () {
      test('should have valid mock data structure', () async {
        final salons = await publicSalonService.getPublicSalons();
        
        for (final salon in salons) {
          // Required fields
          expect(salon.id, isNotEmpty);
          expect(salon.name, isNotEmpty);
          expect(salon.address, isNotEmpty);
          expect(salon.isOpen, isA<bool>());
          
          // Optional but typed fields
          if (salon.currentWaitTimeMinutes != null) {
            expect(salon.currentWaitTimeMinutes, greaterThanOrEqualTo(0));
          }
          
          if (salon.queueLength != null) {
            expect(salon.queueLength, greaterThanOrEqualTo(0));
          }
          
          if (salon.distanceKm != null) {
            expect(salon.distanceKm, greaterThanOrEqualTo(0));
          }
          
          if (salon.rating != null) {
            expect(salon.rating, greaterThanOrEqualTo(0));
            expect(salon.rating, lessThanOrEqualTo(5));
          }
          
          if (salon.reviewCount != null) {
            expect(salon.reviewCount, greaterThanOrEqualTo(0));
          }
          
          // Boolean fields
          expect(salon.isFast, isA<bool>());
          expect(salon.isPopular, isA<bool>());
        }
      });
    });
  });
}