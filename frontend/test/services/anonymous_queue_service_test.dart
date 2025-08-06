import 'package:flutter_test/flutter_test.dart';
import 'package:dio/dio.dart';
import '../../lib/services/anonymous_queue_service.dart';
import '../../lib/services/anonymous_user_service.dart';
import '../../lib/models/anonymous_user.dart';
import '../../lib/models/public_salon.dart';

void main() {
  group('AnonymousQueueService - Basic Tests', () {
    late AnonymousQueueService service;
    late AnonymousUserService userService;
    late PublicSalon testSalon;

    setUp(() {
      userService = AnonymousUserService();
      service = AnonymousQueueService(
        userService: userService,
        dio: Dio(),
      );

      testSalon = const PublicSalon(
        id: 'salon123',
        name: 'Test Salon',
        address: 'Test Address',
        latitude: -23.5505,
        longitude: -46.6333,
        isOpen: true,
        queueLength: 5,
        currentWaitTimeMinutes: 15,
      );
    });

    test('should create service instance', () {
      expect(service, isNotNull);
      expect(service, isA<AnonymousQueueService>());
    });

    test('should have correct salon data', () {
      expect(testSalon.id, equals('salon123'));
      expect(testSalon.name, equals('Test Salon'));
      expect(testSalon.address, equals('Test Address'));
      expect(testSalon.isOpen, isTrue);
      expect(testSalon.queueLength, equals(5));
      expect(testSalon.currentWaitTimeMinutes, equals(15));
    });

    test('should handle null dio parameter', () {
      final serviceWithNullDio = AnonymousQueueService(
        userService: userService,
      );
      expect(serviceWithNullDio, isNotNull);
    });
  });

  group('AnonymousQueueService - Model Tests', () {
    test('should create valid AnonymousUser', () {
      final user = AnonymousUser(
        id: 'user123',
        name: 'Test User',
        email: 'test@example.com',
        createdAt: DateTime.now(),
        preferences: AnonymousUserPreferences.defaultPreferences(),
        activeQueues: [],
        queueHistory: [],
      );

      expect(user.id, equals('user123'));
      expect(user.name, equals('Test User'));
      expect(user.email, equals('test@example.com'));
      expect(user.activeQueues, isEmpty);
      expect(user.queueHistory, isEmpty);
    });

    test('should create valid AnonymousQueueEntry', () {
      final entry = AnonymousQueueEntry(
        id: 'entry123',
        anonymousUserId: 'user123',
        salonId: 'salon123',
        salonName: 'Test Salon',
        position: 3,
        estimatedWaitMinutes: 15,
        joinedAt: DateTime.now(),
        lastUpdated: DateTime.now(),
        status: QueueEntryStatus.waiting,
        serviceRequested: 'Haircut',
      );

      expect(entry.id, equals('entry123'));
      expect(entry.salonId, equals('salon123'));
      expect(entry.position, equals(3));
      expect(entry.estimatedWaitMinutes, equals(15));
      expect(entry.status, equals(QueueEntryStatus.waiting));
      expect(entry.serviceRequested, equals('Haircut'));
    });

    test('should create valid PublicSalon', () {
      final salon = const PublicSalon(
        id: 'salon123',
        name: 'Test Salon',
        address: 'Test Address',
        latitude: -23.5505,
        longitude: -46.6333,
        isOpen: true,
        queueLength: 5,
        currentWaitTimeMinutes: 15,
        services: ['Haircut', 'Beard Trim'],
      );

      expect(salon.id, equals('salon123'));
      expect(salon.name, equals('Test Salon'));
      expect(salon.address, equals('Test Address'));
      expect(salon.isOpen, isTrue);
      expect(salon.queueLength, equals(5));
      expect(salon.currentWaitTimeMinutes, equals(15));
      expect(salon.services, containsAll(['Haircut', 'Beard Trim']));
    });
  });
} 