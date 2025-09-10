import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:dio/dio.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:eutonafila_frontend/services/auth_service.dart';
import 'package:eutonafila_frontend/services/api_client.dart';
import 'package:eutonafila_frontend/models/auth_models.dart';

// Generate mocks
@GenerateNiceMocks([
  MockSpec<ApiClient>(),
  MockSpec<SharedPreferences>(),
])
import 'auth_service_test.mocks.dart';

void main() {
  group('AuthService Tests', () {
    late AuthService authService;
    late MockApiClient mockApiClient;
    late MockSharedPreferences mockSharedPreferences;

    setUp(() {
      mockApiClient = MockApiClient();
      mockSharedPreferences = MockSharedPreferences();
      authService = AuthService(
        apiClient: mockApiClient,
        sharedPreferences: mockSharedPreferences,
      );
    });

    group('Login', () {
      test('should login successfully and store token', () async {
        // Arrange
        const loginRequest = LoginRequest(
          phoneNumber: '+5511999999999',
          password: 'StrongPass123!',
        );
        
        const loginResult = LoginResult(
          success: true,
          token: 'jwt-token-123',
          fullName: 'Test User',
          phoneNumber: '+5511999999999',
          email: 'test@example.com',
          role: 'Customer',
          permissions: ['read'],
          requiresTwoFactor: false,
        );

        final mockResponse = Response(
          data: loginResult.toJson(),
          statusCode: 200,
          requestOptions: RequestOptions(path: '/auth/login'),
        );

        when(mockApiClient.post(any, data: anyNamed('data')))
            .thenAnswer((_) async => mockResponse);

        when(mockSharedPreferences.setString(any, any))
            .thenAnswer((_) async => true);

        // Act
        final result = await authService.login(loginRequest);

        // Assert
        expect(result.success, isTrue);
        expect(result.token, equals('jwt-token-123'));
        expect(result.fullName, equals('Test User'));
        expect(result.phoneNumber, equals('+5511999999999'));
        expect(result.email, equals('test@example.com'));
        expect(result.role, equals('Customer'));
        expect(result.permissions, equals(['read']));
        expect(result.requiresTwoFactor, isFalse);

        // Verify API call
        verify(mockApiClient.post('/auth/login', data: loginRequest.toJson()));
        
        // Verify token storage
        verify(mockSharedPreferences.setString('auth_token', 'jwt-token-123'));
        verify(mockSharedPreferences.setString('phone_number', '+5511999999999'));
        verify(mockSharedPreferences.setString('user_role', 'Customer'));
      });

      test('should handle login failure', () async {
        // Arrange
        const loginRequest = LoginRequest(
          phoneNumber: '+5511999999999',
          password: 'wrongpassword',
        );
        
        const loginResult = LoginResult(
          success: false,
          error: 'Invalid phone number or password',
        );

        final mockResponse = Response(
          data: loginResult.toJson(),
          statusCode: 400,
          requestOptions: RequestOptions(path: '/auth/login'),
        );

        when(mockApiClient.post(any, data: anyNamed('data')))
            .thenAnswer((_) async => mockResponse);

        // Act
        final result = await authService.login(loginRequest);

        // Assert
        expect(result.success, isFalse);
        expect(result.error, equals('Invalid phone number or password'));
        expect(result.token, isNull);
      });

      test('should handle two-factor authentication requirement', () async {
        // Arrange
        const loginRequest = LoginRequest(
          phoneNumber: '+5511999999999',
          password: 'StrongPass123!',
        );
        
        const loginResult = LoginResult(
          success: false,
          requiresTwoFactor: true,
          twoFactorToken: '2fa-token-123',
          phoneNumber: '+5511999999999',
        );

        final mockResponse = Response(
          data: loginResult.toJson(),
          statusCode: 200,
          requestOptions: RequestOptions(path: '/auth/login'),
        );

        when(mockApiClient.post(any, data: anyNamed('data')))
            .thenAnswer((_) async => mockResponse);

        // Act
        final result = await authService.login(loginRequest);

        // Assert
        expect(result.success, isFalse);
        expect(result.requiresTwoFactor, isTrue);
        expect(result.twoFactorToken, equals('2fa-token-123'));
        expect(result.phoneNumber, equals('+5511999999999'));
      });

      test('should handle network error', () async {
        // Arrange
        const loginRequest = LoginRequest(
          phoneNumber: '+5511999999999',
          password: 'StrongPass123!',
        );

        when(mockApiClient.post(any, data: anyNamed('data')))
            .thenThrow(DioException(
              requestOptions: RequestOptions(path: '/auth/login'),
              type: DioExceptionType.connectionTimeout,
            ));

        // Act
        final result = await authService.login(loginRequest);

        // Assert
        expect(result.success, isFalse);
        expect(result.error, equals('Erro ao efetuar login'));
      });
    });

    group('Registration', () {
      test('should register successfully', () async {
        // Arrange
        const registerRequest = RegisterRequest(
          fullName: 'New User',
          email: 'newuser@example.com',
          phoneNumber: '+5511999999999',
          password: 'StrongPass123!',
        );
        
        const registerResult = RegisterResult(
          success: true,
        );

        final mockResponse = Response(
          data: registerResult.toJson(),
          statusCode: 200,
          requestOptions: RequestOptions(path: '/auth/register'),
        );

        when(mockApiClient.post(any, data: anyNamed('data')))
            .thenAnswer((_) async => mockResponse);

        // Act
        final result = await authService.register(registerRequest);

        // Assert
        expect(result.success, isTrue);
        expect(result.error, isNull);

        // Verify API call
        verify(mockApiClient.post('/auth/register', data: registerRequest.toJson()));
      });

      test('should handle registration failure with field errors', () async {
        // Arrange
        const registerRequest = RegisterRequest(
          fullName: 'Existing User',
          email: 'existing@example.com',
          phoneNumber: '+5511999999998',
          password: 'StrongPass123!',
        );
        
        const registerResult = RegisterResult(
          success: false,
          error: 'Registration failed',
          fieldErrors: {'phoneNumber': 'Phone number already registered'},
        );

        final mockResponse = Response(
          data: registerResult.toJson(),
          statusCode: 400,
          requestOptions: RequestOptions(path: '/auth/register'),
        );

        when(mockApiClient.post(any, data: anyNamed('data')))
            .thenAnswer((_) async => mockResponse);

        // Act
        final result = await authService.register(registerRequest);

        // Assert
        expect(result.success, isFalse);
        expect(result.error, equals('Registration failed'));
        expect(result.fieldErrors, equals({'phoneNumber': 'Phone number already registered'}));
      });
    });

    group('Profile', () {
      test('should get user profile successfully', () async {
        // Arrange
        final mockProfileData = {
          'UserId': 'user-123',
          'FullName': 'Test User',
          'Email': 'test@example.com',
          'PhoneNumber': '+5511999999999',
          'Role': 'Customer',
          'Permissions': ['read', 'write'],
        };

        final mockResponse = Response(
          data: mockProfileData,
          statusCode: 200,
          requestOptions: RequestOptions(path: '/auth/profile'),
        );

        when(mockSharedPreferences.getString('auth_token'))
            .thenReturn('jwt-token-123');
        when(mockApiClient.get(any))
            .thenAnswer((_) async => mockResponse);

        // Act
        final result = await authService.getProfile();

        // Assert
        expect(result.id, equals('user-123'));
        expect(result.fullName, equals('Test User'));
        expect(result.email, equals('test@example.com'));
        expect(result.phoneNumber, equals('+5511999999999'));
        expect(result.role, equals('Customer'));
        expect(result.permissions, equals(['read', 'write']));

        // Verify API call
        verify(mockApiClient.get('/auth/profile'));
      });

      test('should handle profile request when not authenticated', () async {
        // Arrange
        when(mockSharedPreferences.getString('auth_token'))
            .thenReturn(null);

        // Act & Assert
        expect(
          () async => await authService.getProfile(),
          throwsA(isA<Exception>().having(
            (e) => e.toString(),
            'message',
            contains('User not authenticated'),
          )),
        );
      });
    });

    group('Two-Factor Authentication', () {
      test('should verify two-factor code successfully', () async {
        // Arrange
        const verifyRequest = VerifyTwoFactorRequest(
          phoneNumber: '+5511999999999',
          twoFactorCode: '123456',
          twoFactorToken: '2fa-token-123',
        );
        
        const loginResult = LoginResult(
          success: true,
          token: 'jwt-token-456',
          fullName: 'Test User',
          phoneNumber: '+5511999999999',
          email: 'test@example.com',
          role: 'Customer',
          permissions: ['read'],
          requiresTwoFactor: false,
        );

        final mockResponse = Response(
          data: loginResult.toJson(),
          statusCode: 200,
          requestOptions: RequestOptions(path: '/auth/verify-2fa'),
        );

        when(mockApiClient.post(any, data: anyNamed('data')))
            .thenAnswer((_) async => mockResponse);

        when(mockSharedPreferences.setString(any, any))
            .thenAnswer((_) async => true);

        // Act
        final result = await authService.verifyTwoFactor(verifyRequest);

        // Assert
        expect(result.success, isTrue);
        expect(result.token, equals('jwt-token-456'));
        expect(result.requiresTwoFactor, isFalse);

        // Verify token storage
        verify(mockSharedPreferences.setString('auth_token', 'jwt-token-456'));
        verify(mockSharedPreferences.setString('phone_number', '+5511999999999'));
      });
    });

    group('Authentication State', () {
      test('should load stored authentication data', () async {
        // Arrange
        when(mockSharedPreferences.getString('auth_token'))
            .thenReturn('stored-token-123');
        when(mockSharedPreferences.getString('phone_number'))
            .thenReturn('+5511999999999');
        when(mockSharedPreferences.getString('user_role'))
            .thenReturn('Customer');

        // Mock profile response for token validation
        final mockProfileData = {
          'UserId': 'user-123',
          'FullName': 'Test User',
          'Email': 'test@example.com',
          'PhoneNumber': '+5511999999999',
          'Role': 'Customer',
          'Permissions': ['read'],
        };

        final mockResponse = Response(
          data: mockProfileData,
          statusCode: 200,
          requestOptions: RequestOptions(path: '/auth/profile'),
        );

        when(mockApiClient.get(any))
            .thenAnswer((_) async => mockResponse);

        // Act
        await authService.loadStoredAuth();

        // Assert
        expect(authService.isAuthenticated, isTrue);
        expect(authService.currentPhoneNumber, equals('+5511999999999'));
        expect(authService.currentUser?.fullName, equals('Test User'));
        expect(authService.currentRole, equals('Customer'));

        verify(mockApiClient.setAuthToken('stored-token-123'));
      });

      test('should clear authentication data on logout', () async {
        // Arrange
        when(mockSharedPreferences.remove(any))
            .thenAnswer((_) async => true);

        // Act
        await authService.logout();

        // Assert
        expect(authService.isAuthenticated, isFalse);
        expect(authService.currentPhoneNumber, isNull);
        expect(authService.currentUser, isNull);
        expect(authService.currentRole, isNull);

        // Verify storage cleanup
        verify(mockSharedPreferences.remove('auth_token'));
        verify(mockSharedPreferences.remove('phone_number'));
        verify(mockSharedPreferences.remove('user_role'));
        verify(mockApiClient.clearAuthToken());
      });

      test('should handle demo login', () async {
        // Arrange
        when(mockSharedPreferences.setString(any, any))
            .thenAnswer((_) async => true);

        // Act
        await authService.demoLogin(
          phoneNumber: '+5511999999999',
          role: 'Customer',
        );

        // Assert
        expect(authService.isAuthenticated, isTrue);
        expect(authService.currentPhoneNumber, equals('+5511999999999'));
        expect(authService.currentUser?.fullName, equals('Usuário Demo'));
        expect(authService.currentUser?.role, equals('Customer'));
        expect(authService.currentRole, equals('Customer'));

        // Verify token storage
        verify(mockSharedPreferences.setString('auth_token', any));
        verify(mockSharedPreferences.setString('phone_number', '+5511999999999'));
        verify(mockSharedPreferences.setString('user_role', 'Customer'));
      });
    });
  });
}