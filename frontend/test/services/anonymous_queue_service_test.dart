import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:dio/dio.dart';
import '../../lib/services/anonymous_queue_service.dart';
import '../../lib/services/anonymous_user_service.dart';
import '../../lib/models/anonymous_user.dart';
import '../../lib/models/public_salon.dart';

@GenerateMocks([Dio, AnonymousUserService])
import 'anonymous_queue_service_test.mocks.dart';

void main() {
  group('AnonymousQueueService - Real Data Only', () {
    late AnonymousQueueService service;
    late MockDio mockDio;
    late MockAnonymousUserService mockUserService;
    late PublicSalon testSalon;
    late AnonymousUser testUser;

    setUp(() {
      mockDio = MockDio();
      mockUserService = MockAnonymousUserService();
      service = AnonymousQueueService(
        userService: mockUserService,
        dio: mockDio,
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

      testUser = AnonymousUser(
        id: 'user123',
        name: 'Test User',
        email: 'test@example.com',
        createdAt: DateTime.now(),
        preferences: AnonymousUserPreferences.defaultPreferences(),
        activeQueues: [],
        queueHistory: [],
      );
    });

    group('joinQueue', () {
      test('should throw exception when API returns 404 (no mock data fallback)', () async {
        // Arrange
        when(mockUserService.getAnonymousUser()).thenAnswer((_) async => testUser);
        when(mockDio.post(any, data: anyNamed('data'), options: anyNamed('options')))
            .thenThrow(DioException(
          requestOptions: RequestOptions(path: '/test'),
          response: Response(
            requestOptions: RequestOptions(path: '/test'),
            statusCode: 404,
          ),
        ));

        // Act & Assert
        expect(
          () => service.joinQueue(
            salon: testSalon,
            name: 'Test User',
            email: 'test@example.com',
          ),
          throwsA(isA<Exception>().having(
            (e) => e.toString(),
            'message',
            contains('Queue service is not available'),
          )),
        );

        // Verify no mock data was created
        verifyNever(mockUserService.addQueueEntry(any));
      });

      test('should throw exception when API returns 500', () async {
        // Arrange
        when(mockUserService.getAnonymousUser()).thenAnswer((_) async => testUser);
        when(mockDio.post(any, data: anyNamed('data'), options: anyNamed('options')))
            .thenThrow(DioException(
          requestOptions: RequestOptions(path: '/test'),
          response: Response(
            requestOptions: RequestOptions(path: '/test'),
            statusCode: 500,
          ),
        ));

        // Act & Assert
        expect(
          () => service.joinQueue(
            salon: testSalon,
            name: 'Test User',
            email: 'test@example.com',
          ),
          throwsA(isA<Exception>().having(
            (e) => e.toString(),
            'message',
            contains('Server error'),
          )),
        );
      });

      test('should return real queue entry when API call succeeds', () async {
        // Arrange
        when(mockUserService.getAnonymousUser()).thenAnswer((_) async => testUser);
        when(mockUserService.addQueueEntry(any)).thenAnswer((_) async {});
        
        final realApiResponse = Response(
          requestOptions: RequestOptions(path: '/test'),
          statusCode: 200,
          data: {
            'id': 'real_queue_123',
            'position': 2,
            'estimatedWaitMinutes': 10,
            'joinedAt': DateTime.now().toIso8601String(),
          },
        );
        
        when(mockDio.post(any, data: anyNamed('data'), options: anyNamed('options')))
            .thenAnswer((_) async => realApiResponse);

        // Act
        final result = await service.joinQueue(
          salon: testSalon,
          name: 'Test User',
          email: 'test@example.com',
        );

        // Assert - verify it's real data from API, not mock data
        expect(result.id, equals('real_queue_123')); // Real ID from API
        expect(result.position, equals(2)); // Real position from API
        expect(result.estimatedWaitMinutes, equals(10)); // Real wait time from API
        expect(result.salonId, equals(testSalon.id));
        expect(result.salonName, equals(testSalon.name));
        
        // Verify it was saved to local storage
        verify(mockUserService.addQueueEntry(any)).called(1);
      });
    });

    group('getQueueStatus', () {
      test('should return updated queue entry from real API data', () async {
        // Arrange
        when(mockUserService.getAnonymousUser()).thenAnswer((_) async => testUser);
        when(mockUserService.updateQueueEntry(any, any)).thenAnswer((_) async {});
        
        final realApiResponse = Response(
          requestOptions: RequestOptions(path: '/test'),
          statusCode: 200,
          data: {
            'salonId': testSalon.id,
            'salonName': testSalon.name,
            'position': 1,
            'estimatedWaitMinutes': 5,
            'joinedAt': DateTime.now().subtract(const Duration(minutes: 10)).toIso8601String(),
            'status': 'waiting',
            'serviceRequested': 'Haircut',
          },
        );
        
        when(mockDio.get(any, options: anyNamed('options')))
            .thenAnswer((_) async => realApiResponse);

        // Act
        final result = await service.getQueueStatus('queue123');

        // Assert - verify real-time data from API
        expect(result, isNotNull);
        expect(result!.position, equals(1)); // Updated position from API
        expect(result.estimatedWaitMinutes, equals(5)); // Updated wait time from API
        expect(result.status, equals(QueueEntryStatus.waiting));
        expect(result.lastUpdated, isNotNull); // Should be updated
        
        // Verify local storage was updated with real data
        verify(mockUserService.updateQueueEntry(any, any)).called(1);
      });
    });
  });

  group('AnonymousQueueService - Database Integration', () {
    late MockDio mockDio;
    late MockAnonymousUserService mockUserService;
    late AnonymousQueueService queueService;
    late AnonymousUser testUser;
    late PublicSalon testSalon;

    setUp(() {
      mockDio = MockDio();
      mockUserService = MockAnonymousUserService();
      queueService = AnonymousQueueService(
        userService: mockUserService,
        dio: mockDio,
      );

      testUser = AnonymousUser(
        id: 'user123',
        name: 'Test User',
        email: 'test@example.com',
        createdAt: DateTime.now(),
        preferences: AnonymousUserPreferences.defaultPreferences(),
        activeQueues: [],
        queueHistory: [],
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
        services: ['Haircut', 'Beard Trim'],
      );
    });

    test('should successfully insert queue entry into database', () async {
      // Arrange
      when(mockUserService.getAnonymousUser()).thenAnswer((_) async => testUser);
      
      final mockResponse = Response(
        data: {
          'id': 'queue123',
          'position': 6,
          'estimatedWaitMinutes': 20,
          'joinedAt': '2024-01-01T10:00:00Z',
        },
        statusCode: 200,
        requestOptions: RequestOptions(path: ''),
      );
      
      when(mockDio.post(
        any,
        data: anyNamed('data'),
        options: anyNamed('options'),
      )).thenAnswer((_) async => mockResponse);

      when(mockUserService.addQueueEntry(any)).thenAnswer((_) async {});

      // Act
      final result = await queueService.joinQueue(
        salon: testSalon,
        name: 'Test User',
        email: 'test@example.com',
        serviceRequested: 'Haircut',
      );

      // Assert
      expect(result.id, 'queue123');
      expect(result.position, 6);
      expect(result.estimatedWaitMinutes, 20);
      expect(result.salonId, 'salon123');
      expect(result.serviceRequested, 'Haircut');

      // Verify API was called with correct data
      verify(mockDio.post(
        any,
        data: argThat(isA<Map<String, dynamic>>()),
        options: anyNamed('options'),
      )).called(1);

      // Verify local storage was updated
      verify(mockUserService.addQueueEntry(any)).called(1);
    });

    test('should handle API errors gracefully without creating mock data', () async {
      // Arrange
      when(mockUserService.getAnonymousUser()).thenAnswer((_) async => testUser);
      
      when(mockDio.post(
        any,
        data: anyNamed('data'),
        options: anyNamed('options'),
      )).thenThrow(DioException(
        requestOptions: RequestOptions(path: ''),
        response: Response(
          statusCode: 500,
          requestOptions: RequestOptions(path: ''),
        ),
      ));

      // Act & Assert
      expect(
        () => queueService.joinQueue(
          salon: testSalon,
          name: 'Test User',
          email: 'test@example.com',
        ),
        throwsA(isA<Exception>()),
      );

      // Verify no mock data was created
      verifyNever(mockUserService.addQueueEntry(any));
    });
  });
} 