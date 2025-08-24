import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:dio/dio.dart';
import 'package:eutonafila_frontend/services/organization_service.dart';
import 'package:eutonafila_frontend/services/api_client.dart';
import 'package:eutonafila_frontend/models/organization_models.dart';

import 'organization_service_test.mocks.dart';

@GenerateMocks([ApiClient])
void main() {
  late OrganizationService organizationService;
  late MockApiClient mockApiClient;

  setUp(() {
    mockApiClient = MockApiClient();
    organizationService = OrganizationService(apiClient: mockApiClient);
  });

  group('OrganizationService', () {
    group('createOrganization', () {
      test('should successfully create a new organization', () async {
        // Arrange
        final request = CreateOrganizationRequest(
          name: 'Barbearias Premium',
          subscriptionPlanId: 'plan-123',
          slug: 'barbearias-premium',
          description: 'Rede de barbearias premium',
          contactEmail: 'contato@barbeariasspremium.com',
          contactPhone: '+5511999999999',
          websiteUrl: 'https://barbeariasspremium.com',
          primaryColor: '#1E88E5',
          secondaryColor: '#FFC107',
          logoUrl: 'https://example.com/logo.png',
          faviconUrl: 'https://example.com/favicon.ico',
          tagLine: 'Excelência em cada corte',
          sharesDataForAnalytics: true,
        );

        final expectedResult = CreateOrganizationResult(
          success: true,
          organizationId: 'org-123',
          status: 'Created',
          slug: 'barbearias-premium',
          fieldErrors: {},
          errors: [],
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Organizations'),
          statusCode: 200,
          data: expectedResult.toJson(),
        );

        when(mockApiClient.post('/Organizations', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await organizationService.createOrganization(request);

        // Assert
        expect(result.success, isTrue);
        expect(result.organizationId, equals('org-123'));
        expect(result.status, equals('Created'));
        expect(result.slug, equals('barbearias-premium'));
        verify(mockApiClient.post('/Organizations', data: request.toJson())).called(1);
      });
    });

    group('getOrganizations', () {
      test('should successfully get all organizations', () async {
        // Arrange
        final response = Response(
          requestOptions: RequestOptions(path: '/Organizations'),
          statusCode: 200,
          data: [
            {
              'id': 'org-123',
              'name': 'Barbearias Premium',
              'slug': 'barbearias-premium',
              'isActive': true,
            },
            {
              'id': 'org-456',
              'name': 'Salão Elegante',
              'slug': 'salao-elegante',
              'isActive': true,
            },
          ],
        );

        when(mockApiClient.get('/Organizations'))
            .thenAnswer((_) async => response);

        // Act
        final result = await organizationService.getOrganizations();

        // Assert
        expect(result, isA<List>());
        expect(result.length, equals(2));
        expect(result[0]['name'], equals('Barbearias Premium'));
        expect(result[1]['name'], equals('Salão Elegante'));
        verify(mockApiClient.get('/Organizations')).called(1);
      });
    });

    group('getOrganization', () {
      test('should successfully get organization details', () async {
        // Arrange
        final response = Response(
          requestOptions: RequestOptions(path: '/Organizations/org-123'),
          statusCode: 200,
          data: {
            'id': 'org-123',
            'name': 'Barbearias Premium',
            'slug': 'barbearias-premium',
            'description': 'Rede de barbearias premium',
            'contactEmail': 'contato@barbeariasspremium.com',
            'contactPhone': '+5511999999999',
            'websiteUrl': 'https://barbeariasspremium.com',
            'primaryColor': '#1E88E5',
            'secondaryColor': '#FFC107',
            'isActive': true,
          },
        );

        when(mockApiClient.get('/Organizations/org-123'))
            .thenAnswer((_) async => response);

        // Act
        final result = await organizationService.getOrganization('org-123');

        // Assert
        expect(result['id'], equals('org-123'));
        expect(result['name'], equals('Barbearias Premium'));
        expect(result['slug'], equals('barbearias-premium'));
        expect(result['isActive'], isTrue);
        verify(mockApiClient.get('/Organizations/org-123')).called(1);
      });
    });

    group('updateOrganization', () {
      test('should successfully update organization', () async {
        // Arrange
        final request = UpdateOrganizationRequest(
          organizationId: 'org-123',
          name: 'Barbearias Premium - Atualizada',
          description: 'Rede de barbearias premium com novos serviços',
          contactEmail: 'novo.contato@barbeariasspremium.com',
          contactPhone: '+5511888888888',
          websiteUrl: 'https://barbeariasspremium.com.br',
        );

        final expectedResult = OrganizationOperationResult(
          success: true,
          fieldErrors: {},
          errors: [],
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Organizations/org-123'),
          statusCode: 200,
          data: expectedResult.toJson(),
        );

        when(mockApiClient.put('/Organizations/org-123', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await organizationService.updateOrganization('org-123', request);

        // Assert
        expect(result.success, isTrue);
        expect(result.fieldErrors, isEmpty);
        expect(result.errors, isEmpty);
        verify(mockApiClient.put('/Organizations/org-123', data: request.toJson())).called(1);
      });
    });

    group('updateBranding', () {
      test('should successfully update organization branding', () async {
        // Arrange
        final request = UpdateBrandingRequest(
          organizationId: 'org-123',
          primaryColor: '#2196F3',
          secondaryColor: '#FF9800',
          logoUrl: 'https://example.com/new-logo.png',
          faviconUrl: 'https://example.com/new-favicon.ico',
          tagLine: 'Nova tagline inspiradora',
        );

        final expectedResult = OrganizationOperationResult(
          success: true,
          fieldErrors: {},
          errors: [],
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Organizations/org-123/branding'),
          statusCode: 200,
          data: expectedResult.toJson(),
        );

        when(mockApiClient.put('/Organizations/org-123/branding', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await organizationService.updateBranding('org-123', request);

        // Assert
        expect(result.success, isTrue);
        expect(result.fieldErrors, isEmpty);
        expect(result.errors, isEmpty);
        verify(mockApiClient.put('/Organizations/org-123/branding', data: request.toJson())).called(1);
      });
    });

    group('updateSubscriptionPlan', () {
      test('should successfully update subscription plan', () async {
        // Arrange
        final expectedResult = OrganizationOperationResult(
          success: true,
          fieldErrors: {},
          errors: [],
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Organizations/org-123/subscription/plan-456'),
          statusCode: 200,
          data: expectedResult.toJson(),
        );

        when(mockApiClient.put('/Organizations/org-123/subscription/plan-456'))
            .thenAnswer((_) async => response);

        // Act
        final result = await organizationService.updateSubscriptionPlan('org-123', 'plan-456');

        // Assert
        expect(result.success, isTrue);
        verify(mockApiClient.put('/Organizations/org-123/subscription/plan-456')).called(1);
      });
    });

    group('updateAnalyticsSharing', () {
      test('should successfully update analytics sharing setting', () async {
        // Arrange
        final expectedResult = OrganizationOperationResult(
          success: true,
          fieldErrors: {},
          errors: [],
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Organizations/org-123/analytics-sharing'),
          statusCode: 200,
          data: expectedResult.toJson(),
        );

        when(mockApiClient.put('/Organizations/org-123/analytics-sharing', data: anyNamed('data')))
            .thenAnswer((_) async => response);

        // Act
        final result = await organizationService.updateAnalyticsSharing('org-123', true);

        // Assert
        expect(result.success, isTrue);
        verify(mockApiClient.put('/Organizations/org-123/analytics-sharing', data: true)).called(1);
      });
    });

    group('activateOrganization', () {
      test('should successfully activate organization', () async {
        // Arrange
        final expectedResult = OrganizationOperationResult(
          success: true,
          fieldErrors: {},
          errors: [],
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Organizations/org-123/activate'),
          statusCode: 200,
          data: expectedResult.toJson(),
        );

        when(mockApiClient.put('/Organizations/org-123/activate'))
            .thenAnswer((_) async => response);

        // Act
        final result = await organizationService.activateOrganization('org-123');

        // Assert
        expect(result.success, isTrue);
        verify(mockApiClient.put('/Organizations/org-123/activate')).called(1);
      });
    });

    group('deactivateOrganization', () {
      test('should successfully deactivate organization', () async {
        // Arrange
        final expectedResult = OrganizationOperationResult(
          success: true,
          fieldErrors: {},
          errors: [],
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Organizations/org-123/deactivate'),
          statusCode: 200,
          data: expectedResult.toJson(),
        );

        when(mockApiClient.put('/Organizations/org-123/deactivate'))
            .thenAnswer((_) async => response);

        // Act
        final result = await organizationService.deactivateOrganization('org-123');

        // Assert
        expect(result.success, isTrue);
        verify(mockApiClient.put('/Organizations/org-123/deactivate')).called(1);
      });
    });

    group('getOrganizationBySlug', () {
      test('should successfully get organization by slug', () async {
        // Arrange
        final response = Response(
          requestOptions: RequestOptions(path: '/Organizations/by-slug/barbearias-premium'),
          statusCode: 200,
          data: {
            'id': 'org-123',
            'name': 'Barbearias Premium',
            'slug': 'barbearias-premium',
            'isActive': true,
          },
        );

        when(mockApiClient.get('/Organizations/by-slug/barbearias-premium'))
            .thenAnswer((_) async => response);

        // Act
        final result = await organizationService.getOrganizationBySlug('barbearias-premium');

        // Assert
        expect(result['id'], equals('org-123'));
        expect(result['slug'], equals('barbearias-premium'));
        verify(mockApiClient.get('/Organizations/by-slug/barbearias-premium')).called(1);
      });
    });

    group('getLiveActivity', () {
      test('should successfully get live activity', () async {
        // Arrange
        final expectedResult = TrackLiveActivityResult(
          success: true,
          liveActivity: LiveActivityDto(
            organizationId: 'org-123',
            organizationName: 'Barbearias Premium',
            lastUpdated: DateTime(2024, 1, 15, 10, 30),
            summary: OrganizationSummaryDto(
              totalLocations: 3,
              totalActiveQueues: 2,
              totalCustomersWaiting: 15,
              totalStaffMembers: 8,
              totalAvailableStaff: 5,
              totalBusyStaff: 3,
              averageWaitTimeMinutes: 22.5,
            ),
            locations: [],
          ),
          errors: [],
          fieldErrors: {},
        );

        final response = Response(
          requestOptions: RequestOptions(path: '/Organizations/org-123/live-activity'),
          statusCode: 200,
          data: {
            'success': true,
            'liveActivity': {
              'organizationId': 'org-123',
              'organizationName': 'Barbearias Premium',
              'lastUpdated': '2024-01-15T10:30:00.000',
              'summary': {
                'totalLocations': 3,
                'totalActiveQueues': 2,
                'totalCustomersWaiting': 15,
                'totalStaffMembers': 8,
                'totalAvailableStaff': 5,
                'totalBusyStaff': 3,
                'averageWaitTimeMinutes': 22.5,
              },
              'locations': [],
            },
            'errors': [],
            'fieldErrors': {},
          },
        );

        when(mockApiClient.get('/Organizations/org-123/live-activity'))
            .thenAnswer((_) async => response);

        // Act
        final result = await organizationService.getLiveActivity('org-123');

        // Assert
        expect(result.success, isTrue);
        expect(result.liveActivity?.organizationId, equals('org-123'));
        expect(result.liveActivity?.organizationName, equals('Barbearias Premium'));
        verify(mockApiClient.get('/Organizations/org-123/live-activity')).called(1);
      });
    });
  });
}