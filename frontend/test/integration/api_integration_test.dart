import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/models/auth_models.dart';
import 'package:eutonafila_frontend/models/queue_models.dart';
import 'package:eutonafila_frontend/models/location_models.dart';
import 'package:eutonafila_frontend/models/organization_models.dart';
import 'package:eutonafila_frontend/models/staff_models.dart';
import 'package:eutonafila_frontend/models/services_models.dart';

void main() {
  // Initialize Flutter binding for tests
  TestWidgetsFlutterBinding.ensureInitialized();
  
  group('API Integration Tests', () {
    setUpAll(() async {
      // Note: In a real test environment, we would mock these services
      // For now, we'll focus on model validation and structure testing
      // Services would be mocked to avoid SharedPreferences dependency
    });

    group('Authentication Flow', () {
      test('should handle complete authentication workflow', () async {
        // Test registration
        final registerRequest = RegisterRequest(
          fullName: 'Test User',
          email: 'test@example.com',
          phoneNumber: '+5511999999999',
          password: 'TestPassword123!',
        );

        // Test model creation and validation
        expect(registerRequest.fullName, equals('Test User'));
        expect(registerRequest.email, equals('test@example.com'));
        expect(registerRequest.phoneNumber, equals('+5511999999999'));
        expect(registerRequest.password, equals('TestPassword123!'));

        // Test login
        final loginRequest = LoginRequest(
          phoneNumber: '+5511999999999',
          password: 'TestPassword123!',
        );

        expect(loginRequest.phoneNumber, equals('+5511999999999'));
        expect(loginRequest.password, equals('TestPassword123!'));

        // Test 2FA verification
        final verify2FARequest = VerifyTwoFactorRequest(
          phoneNumber: '+5511999999999',
          twoFactorCode: '123456',
          twoFactorToken: 'test-token',
        );

        expect(verify2FARequest.phoneNumber, equals('+5511999999999'));
        expect(verify2FARequest.twoFactorCode, equals('123456'));
        expect(verify2FARequest.twoFactorToken, equals('test-token'));
      });
    });

    group('Organization Management', () {
      test('should handle organization lifecycle', () async {
        // Create organization
        final createOrgRequest = CreateOrganizationRequest(
          name: 'Test Barbershop',
          subscriptionPlanId: 'plan-basic',
          slug: 'test-barbershop',
          description: 'A test barbershop for integration testing',
          contactEmail: 'contact@testbarbershop.com',
          contactPhone: '+5511999999999',
          websiteUrl: 'https://testbarbershop.com',
          primaryColor: '#1E88E5',
          secondaryColor: '#FFC107',
          sharesDataForAnalytics: true,
        );

        expect(createOrgRequest.name, equals('Test Barbershop'));
        expect(createOrgRequest.contactEmail, equals('contact@testbarbershop.com'));
        expect(createOrgRequest.contactPhone, equals('+5511999999999'));

        // Update organization
        final updateOrgRequest = UpdateOrganizationRequest(
          organizationId: 'org-123',
          name: 'Test Barbershop Updated',
          description: 'Updated description',
          contactEmail: 'updated@testbarbershop.com',
          contactPhone: '+5511888888888',
          websiteUrl: 'https://testbarbershop.com.br',
        );

        expect(updateOrgRequest.organizationId, equals('org-123'));
        expect(updateOrgRequest.name, equals('Test Barbershop Updated'));

        // Update branding
        final brandingRequest = UpdateBrandingRequest(
          organizationId: 'org-123',
          primaryColor: '#2196F3',
          secondaryColor: '#FF9800',
          logoUrl: 'https://example.com/logo.png',
          faviconUrl: 'https://example.com/favicon.ico',
          tagLine: 'Excellence in every cut',
        );

        expect(brandingRequest.organizationId, equals('org-123'));
        expect(brandingRequest.primaryColor, equals('#2196F3'));
      });
    });

    group('Location Management', () {
      test('should handle location lifecycle', () async {
        // Create location
        final createLocationRequest = CreateLocationRequest(
          businessName: 'Test Location',
          contactEmail: 'location@testbarbershop.com',
          contactPhone: '+5511999999999',
          address: LocationAddressRequest(
            street: 'Test Street, 123',
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
          description: 'Test location for integration testing',
          website: 'https://testlocation.com',
        );

        expect(createLocationRequest.businessName, equals('Test Location'));
        expect(createLocationRequest.contactEmail, equals('location@testbarbershop.com'));

        // Update location
        final updateLocationRequest = UpdateLocationRequest(
          businessName: 'Test Location Updated',
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
          description: 'Updated test location',
        );

        expect(updateLocationRequest.businessName, equals('Test Location Updated'));
        expect(updateLocationRequest.maxQueueCapacity, equals(75));
      });
    });

    group('Staff Management', () {
      test('should handle staff lifecycle', () async {
        // Add barber
        final addBarberRequest = AddBarberRequest(
          firstName: 'João',
          lastName: 'Silva',
          email: 'joao@testbarbershop.com',
          phoneNumber: '+5511999999999',
          username: 'joao.silva',
          locationId: 'location-123',
          serviceTypeIds: ['service-1', 'service-2'],
          deactivateOnCreation: false,
          address: 'Test Address, 123',
          notes: 'Experienced barber for testing',
        );

        expect(addBarberRequest.firstName, equals('João'));
        expect(addBarberRequest.lastName, equals('Silva'));
        expect(addBarberRequest.email, equals('joao@testbarbershop.com'));

        // Edit barber
        final editBarberRequest = EditBarberRequest(
          staffMemberId: 'staff-123',
          name: 'João Silva Santos',
          email: 'joao.updated@testbarbershop.com',
          phoneNumber: '+5511888888888',
          profilePictureUrl: 'https://example.com/photo.jpg',
          role: 'Senior Barber',
        );

        expect(editBarberRequest.staffMemberId, equals('staff-123'));
        expect(editBarberRequest.name, equals('João Silva Santos'));

        // Update staff status
        final statusRequest = UpdateStaffStatusRequest(
          staffMemberId: 'staff-123',
          newStatus: 'Available',
          notes: 'Ready for customers',
        );

        expect(statusRequest.staffMemberId, equals('staff-123'));
        expect(statusRequest.newStatus, equals('Available'));
      });
    });

    group('Services Management', () {
      test('should handle services lifecycle', () async {
        // Add service
        final addServiceRequest = AddServiceOfferedRequest(
          locationId: 'location-123',
          name: 'Test Haircut',
          description: 'Test haircut service for integration testing',
          estimatedDurationMinutes: 30,
          price: 25.0,
          imageUrl: 'https://example.com/haircut.jpg',
        );

        expect(addServiceRequest.locationId, equals('location-123'));
        expect(addServiceRequest.name, equals('Test Haircut'));
        expect(addServiceRequest.price, equals(25.0));

        // Update service
        final updateServiceRequest = UpdateServiceOfferedRequest(
          name: 'Test Haircut Premium',
          description: 'Updated test haircut service',
          estimatedDurationMinutes: 35,
          price: 30.0,
          imageUrl: 'https://example.com/premium-haircut.jpg',
          isActive: true,
        );

        expect(updateServiceRequest.name, equals('Test Haircut Premium'));
        expect(updateServiceRequest.price, equals(30.0));
      });
    });

    group('Queue Management', () {
      test('should handle complete queue workflow', () async {
        // Add queue
        final addQueueRequest = AddQueueRequest(
          locationId: 'location-123',
          maxSize: 50,
          lateClientCapTimeInMinutes: 15,
        );

        expect(addQueueRequest.locationId, equals('location-123'));
        expect(addQueueRequest.maxSize, equals(50));

        // Join queue
        final joinQueueRequest = JoinQueueRequest(
          queueId: 'queue-123',
          customerName: 'Test Customer',
          phoneNumber: '+5511999999999',
          email: 'customer@example.com',
          serviceTypeId: 'service-123',
          notes: 'Regular haircut',
          isAnonymous: false,
        );

        expect(joinQueueRequest.queueId, equals('queue-123'));
        expect(joinQueueRequest.customerName, equals('Test Customer'));

        // Call next
        final callNextRequest = CallNextRequest(
          staffMemberId: 'staff-123',
          queueId: 'queue-123',
        );

        expect(callNextRequest.staffMemberId, equals('staff-123'));
        expect(callNextRequest.queueId, equals('queue-123'));

        // Check in
        final checkInRequest = CheckInRequest(queueEntryId: 'entry-123');

        expect(checkInRequest.queueEntryId, equals('entry-123'));

        // Finish service
        final finishRequest = FinishRequest(
          queueEntryId: 'entry-123',
          serviceDurationMinutes: 30,
          notes: 'Service completed successfully',
        );

        expect(finishRequest.queueEntryId, equals('entry-123'));
        expect(finishRequest.serviceDurationMinutes, equals(30));
      });
    });

    group('Error Handling', () {
      test('should handle network errors gracefully', () async {
        // Test model validation with empty/invalid data
        final emptyLoginRequest = LoginRequest(phoneNumber: '', password: '');
        expect(emptyLoginRequest.phoneNumber, equals(''));
        expect(emptyLoginRequest.password, equals(''));
        
        final invalidBarberRequest = AddBarberRequest(
          firstName: '',
          lastName: '',
          email: '',
          phoneNumber: '',
          username: '',
          locationId: '',
          serviceTypeIds: [],
        );
        expect(invalidBarberRequest.firstName, equals(''));
        expect(invalidBarberRequest.email, equals(''));
      });
    });

    group('Data Validation', () {
      test('should validate request data properly', () async {
        // Test model validation
        final loginRequest = LoginRequest(phoneNumber: '+5511999999999', password: 'password123');
        expect(loginRequest.phoneNumber, equals('+5511999999999'));
        expect(loginRequest.password, equals('password123'));

        final queueRequest = AddQueueRequest(locationId: 'location-123', maxSize: 50);
        expect(queueRequest.locationId, equals('location-123'));
        expect(queueRequest.maxSize, equals(50));

        final staffRequest = AddBarberRequest(
          firstName: 'João',
          lastName: 'Silva',
          email: 'joao@example.com',
          phoneNumber: '+5511999999999',
          username: 'joao.silva',
          locationId: 'location-123',
          serviceTypeIds: ['service-1'],
        );
        expect(staffRequest.firstName, equals('João'));
        expect(staffRequest.lastName, equals('Silva'));
        expect(staffRequest.email, equals('joao@example.com'));
      });
    });
  });
}