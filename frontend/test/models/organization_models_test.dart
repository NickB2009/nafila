import 'package:flutter_test/flutter_test.dart';
import '../../lib/models/organization_models.dart';

void main() {
  group('Organization Models Tests', () {
    group('CreateOrganizationRequest', () {
      test('should create with required fields', () {
        const request = CreateOrganizationRequest(
          name: 'Test Salon Chain',
          subscriptionPlanId: 'plan-123',
        );

        expect(request.name, equals('Test Salon Chain'));
        expect(request.subscriptionPlanId, equals('plan-123'));
        expect(request.slug, isNull);
        expect(request.description, isNull);
      });

      test('should create with all optional fields', () {
        const request = CreateOrganizationRequest(
          name: 'Premium Salon Chain',
          subscriptionPlanId: 'premium-plan-456',
          slug: 'premium-salon-chain',
          description: 'High-end salon chain',
          contactEmail: 'contact@premiumsalon.com',
          contactPhone: '+1234567890',
          websiteUrl: 'https://premiumsalon.com',
          primaryColor: '#FF5722',
          secondaryColor: '#FFC107',
          logoUrl: 'https://example.com/logo.png',
          faviconUrl: 'https://example.com/favicon.ico',
          tagLine: 'Premium cuts for premium people',
          sharesDataForAnalytics: true,
        );

        expect(request.name, equals('Premium Salon Chain'));
        expect(request.subscriptionPlanId, equals('premium-plan-456'));
        expect(request.slug, equals('premium-salon-chain'));
        expect(request.description, equals('High-end salon chain'));
        expect(request.contactEmail, equals('contact@premiumsalon.com'));
        expect(request.contactPhone, equals('+1234567890'));
        expect(request.websiteUrl, equals('https://premiumsalon.com'));
        expect(request.primaryColor, equals('#FF5722'));
        expect(request.secondaryColor, equals('#FFC107'));
        expect(request.logoUrl, equals('https://example.com/logo.png'));
        expect(request.faviconUrl, equals('https://example.com/favicon.ico'));
        expect(request.tagLine, equals('Premium cuts for premium people'));
        expect(request.sharesDataForAnalytics, isTrue);
      });

      test('should serialize to JSON correctly', () {
        const request = CreateOrganizationRequest(
          name: 'Test Salon',
          subscriptionPlanId: 'plan-123',
          slug: 'test-salon',
          description: 'A test salon',
          sharesDataForAnalytics: false,
        );

        final json = request.toJson();

        expect(json['name'], equals('Test Salon'));
        expect(json['subscriptionPlanId'], equals('plan-123'));
        expect(json['slug'], equals('test-salon'));
        expect(json['description'], equals('A test salon'));
        expect(json['sharesDataForAnalytics'], isFalse);
      });

      test('should deserialize from JSON correctly', () {
        final json = {
          'name': 'JSON Salon',
          'subscriptionPlanId': 'json-plan-789',
          'slug': 'json-salon',
          'primaryColor': '#2196F3',
          'sharesDataForAnalytics': true,
        };

        final request = CreateOrganizationRequest.fromJson(json);

        expect(request.name, equals('JSON Salon'));
        expect(request.subscriptionPlanId, equals('json-plan-789'));
        expect(request.slug, equals('json-salon'));
        expect(request.primaryColor, equals('#2196F3'));
        expect(request.sharesDataForAnalytics, isTrue);
      });
    });

    group('CreateOrganizationResult', () {
      test('should create successful result', () {
        const result = CreateOrganizationResult(
          success: true,
          organizationId: 'org-123',
          status: 'Created',
          slug: 'test-organization',
          fieldErrors: {},
          errors: [],
        );

        expect(result.success, isTrue);
        expect(result.organizationId, equals('org-123'));
        expect(result.status, equals('Created'));
        expect(result.slug, equals('test-organization'));
        expect(result.fieldErrors, isEmpty);
        expect(result.errors, isEmpty);
      });

      test('should create failed result with errors', () {
        const result = CreateOrganizationResult(
          success: false,
          organizationId: null,
          status: 'Failed',
          slug: null,
          fieldErrors: {'name': 'Name is required'},
          errors: ['Organization creation failed'],
        );

        expect(result.success, isFalse);
        expect(result.organizationId, isNull);
        expect(result.status, equals('Failed'));
        expect(result.slug, isNull);
        expect(result.fieldErrors, equals({'name': 'Name is required'}));
        expect(result.errors, equals(['Organization creation failed']));
      });

      test('should serialize to JSON correctly', () {
        const result = CreateOrganizationResult(
          success: true,
          organizationId: 'org-456',
          status: 'Active',
          slug: 'active-org',
          fieldErrors: {},
          errors: [],
        );

        final json = result.toJson();

        expect(json['success'], isTrue);
        expect(json['organizationId'], equals('org-456'));
        expect(json['status'], equals('Active'));
        expect(json['slug'], equals('active-org'));
        expect(json['fieldErrors'], isEmpty);
        expect(json['errors'], isEmpty);
      });

      test('should deserialize from JSON correctly', () {
        final json = {
          'success': false,
          'organizationId': null,
          'status': 'Error',
          'slug': null,
          'fieldErrors': {'email': 'Invalid email format'},
          'errors': ['Validation failed', 'Database error'],
        };

        final result = CreateOrganizationResult.fromJson(json);

        expect(result.success, isFalse);
        expect(result.organizationId, isNull);
        expect(result.status, equals('Error'));
        expect(result.slug, isNull);
        expect(result.fieldErrors, equals({'email': 'Invalid email format'}));
        expect(result.errors, equals(['Validation failed', 'Database error']));
      });
    });

    group('UpdateOrganizationRequest', () {
      test('should create with required fields', () {
        const request = UpdateOrganizationRequest(
          organizationId: 'org-123',
          name: 'Updated Salon Name',
        );

        expect(request.organizationId, equals('org-123'));
        expect(request.name, equals('Updated Salon Name'));
        expect(request.description, isNull);
        expect(request.contactEmail, isNull);
        expect(request.contactPhone, isNull);
        expect(request.websiteUrl, isNull);
      });

      test('should create with all fields', () {
        const request = UpdateOrganizationRequest(
          organizationId: 'org-456',
          name: 'Complete Update Salon',
          description: 'Updated description',
          contactEmail: 'updated@salon.com',
          contactPhone: '+9876543210',
          websiteUrl: 'https://updated-salon.com',
        );

        expect(request.organizationId, equals('org-456'));
        expect(request.name, equals('Complete Update Salon'));
        expect(request.description, equals('Updated description'));
        expect(request.contactEmail, equals('updated@salon.com'));
        expect(request.contactPhone, equals('+9876543210'));
        expect(request.websiteUrl, equals('https://updated-salon.com'));
      });

      test('should serialize to JSON correctly', () {
        const request = UpdateOrganizationRequest(
          organizationId: 'org-789',
          name: 'JSON Update Salon',
          description: 'Updated via JSON',
          contactEmail: 'json@update.com',
        );

        final json = request.toJson();

        expect(json['organizationId'], equals('org-789'));
        expect(json['name'], equals('JSON Update Salon'));
        expect(json['description'], equals('Updated via JSON'));
        expect(json['contactEmail'], equals('json@update.com'));
      });
    });

    group('UpdateBrandingRequest', () {
      test('should create with required fields', () {
        const request = UpdateBrandingRequest(
          organizationId: 'org-123',
          primaryColor: '#FF5722',
          secondaryColor: '#FFC107',
        );

        expect(request.organizationId, equals('org-123'));
        expect(request.primaryColor, equals('#FF5722'));
        expect(request.secondaryColor, equals('#FFC107'));
        expect(request.logoUrl, isNull);
        expect(request.faviconUrl, isNull);
        expect(request.tagLine, isNull);
      });

      test('should create with all fields', () {
        const request = UpdateBrandingRequest(
          organizationId: 'org-456',
          primaryColor: '#2196F3',
          secondaryColor: '#FF9800',
          logoUrl: 'https://example.com/new-logo.png',
          faviconUrl: 'https://example.com/new-favicon.ico',
          tagLine: 'New and improved',
        );

        expect(request.organizationId, equals('org-456'));
        expect(request.primaryColor, equals('#2196F3'));
        expect(request.secondaryColor, equals('#FF9800'));
        expect(request.logoUrl, equals('https://example.com/new-logo.png'));
        expect(request.faviconUrl, equals('https://example.com/new-favicon.ico'));
        expect(request.tagLine, equals('New and improved'));
      });

      test('should serialize to JSON correctly', () {
        const request = UpdateBrandingRequest(
          organizationId: 'org-789',
          primaryColor: '#9C27B0',
          secondaryColor: '#4CAF50',
          tagLine: 'Fresh new look',
        );

        final json = request.toJson();

        expect(json['organizationId'], equals('org-789'));
        expect(json['primaryColor'], equals('#9C27B0'));
        expect(json['secondaryColor'], equals('#4CAF50'));
        expect(json['tagLine'], equals('Fresh new look'));
      });
    });

    group('OrganizationOperationResult', () {
      test('should create successful operation result', () {
        const result = OrganizationOperationResult(
          success: true,
          fieldErrors: {},
          errors: [],
        );

        expect(result.success, isTrue);
        expect(result.fieldErrors, isEmpty);
        expect(result.errors, isEmpty);
      });

      test('should create failed operation result', () {
        const result = OrganizationOperationResult(
          success: false,
          fieldErrors: {'primaryColor': 'Invalid color format'},
          errors: ['Update failed', 'Invalid data'],
        );

        expect(result.success, isFalse);
        expect(result.fieldErrors, equals({'primaryColor': 'Invalid color format'}));
        expect(result.errors, equals(['Update failed', 'Invalid data']));
      });

      test('should serialize to JSON correctly', () {
        const result = OrganizationOperationResult(
          success: false,
          fieldErrors: {'name': 'Name too long'},
          errors: ['Validation error'],
        );

        final json = result.toJson();

        expect(json['success'], isFalse);
        expect(json['fieldErrors'], equals({'name': 'Name too long'}));
        expect(json['errors'], equals(['Validation error']));
      });

      test('should deserialize from JSON correctly', () {
        final json = {
          'success': true,
          'fieldErrors': <String, String>{},
          'errors': <String>[],
        };

        final result = OrganizationOperationResult.fromJson(json);

        expect(result.success, isTrue);
        expect(result.fieldErrors, isEmpty);
        expect(result.errors, isEmpty);
      });
    });
  });
}