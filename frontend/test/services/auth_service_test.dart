import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:dio/dio.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../lib/services/auth_service.dart';
import '../../lib/services/api_client.dart';
import '../../lib/models/auth_models.dart';

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
          username: 'testuser',
          password: 'password123',
        );
        
        const loginResult = LoginResult(
          success: true,
          token: 'jwt-token-123',
          username: 'testuser',
          role: 'Client',
          permissions: ['read'],
          requiresTwoFactor: false,
        );

        final mockResponse = Response(
          data: loginResult.toJson(),
          statusCode: 200,
          requestOptions: RequestOptions(path: '/Auth/login'),
        );

        when(mockApiClient.post('/Auth/login', data: loginRequest.toJson()))
            .thenAnswer((_) async => mockResponse);
        when(mockSharedPreferences.setString('auth_token', 'jwt-token-123'))
            .thenAnswer((_) async => true);
        when(mockSharedPreferences.setString('user_role', 'Client'))
            .thenAnswer((_) async => true);
        when(mockSharedPreferences.setString('username', 'testuser'))
            .thenAnswer((_) async => true);

        // Act
        final result = await authService.login(loginRequest);

        // Assert
        expect(result.success, isTrue);
        expect(result.token, equals('jwt-token-123'));
        expect(result.username, equals('testuser'));
        expect(result.role, equals('Client'));

        // Verify API call
        verify(mockApiClient.post('/Auth/login', data: loginRequest.toJson())).called(1);
        
        // Verify token storage
        verify(mockSharedPreferences.setString('auth_token', 'jwt-token-123')).called(1);
        verify(mockSharedPreferences.setString('user_role', 'Client')).called(1);
        verify(mockSharedPreferences.setString('username', 'testuser')).called(1);
        
        // Verify token set on API client
        verify(mockApiClient.setAuthToken('jwt-token-123')).called(1);
      });

      test('should handle login failure correctly', () async {
        // Arrange
        const loginRequest = LoginRequest(
          username: 'testuser',
          password: 'wrongpassword',
        );
        
        const loginResult = LoginResult(
          success: false,
          error: 'Invalid credentials',
        );

        final mockResponse = Response(
          data: loginResult.toJson(),
          statusCode: 400,
          requestOptions: RequestOptions(path: '/Auth/login'),
        );

        when(mockApiClient.post('/Auth/login', data: loginRequest.toJson()))
            .thenAnswer((_) async => mockResponse);

        // Act
        final result = await authService.login(loginRequest);

        // Assert
        expect(result.success, isFalse);
        expect(result.error, equals('Invalid credentials'));
        expect(result.token, isNull);

        // Verify no token storage
        verifyNever(mockSharedPreferences.setString(any, any));
        verifyNever(mockApiClient.setAuthToken(any));
      });

      test('should handle login requiring 2FA', () async {
        // Arrange
        const loginRequest = LoginRequest(
          username: 'testuser',
          password: 'password123',
        );
        
        const loginResult = LoginResult(
          success: false,
          requiresTwoFactor: true,
          twoFactorToken: '2fa-token-123',
          username: 'testuser',
        );

        final mockResponse = Response(
          data: loginResult.toJson(),
          statusCode: 200,
          requestOptions: RequestOptions(path: '/Auth/login'),
        );

        when(mockApiClient.post('/Auth/login', data: loginRequest.toJson()))
            .thenAnswer((_) async => mockResponse);

        // Act
        final result = await authService.login(loginRequest);

        // Assert
        expect(result.success, isFalse);
        expect(result.requiresTwoFactor, isTrue);
        expect(result.twoFactorToken, equals('2fa-token-123'));
        expect(result.username, equals('testuser'));

        // Verify no token storage yet
        verifyNever(mockSharedPreferences.setString('auth_token', any));
      });

      test('should handle API errors during login', () async {
        // Arrange
        const loginRequest = LoginRequest(
          username: 'testuser',
          password: 'password123',
        );

        final dioError = DioException(
          requestOptions: RequestOptions(path: '/Auth/login'),
          response: Response(
            statusCode: 500,
            requestOptions: RequestOptions(path: '/Auth/login'),
          ),
          type: DioExceptionType.badResponse,
        );

        when(mockApiClient.post('/Auth/login', data: loginRequest.toJson()))
            .thenThrow(dioError);

        // Act & Assert
        expect(
          () async => await authService.login(loginRequest),
          throwsA(isA<DioException>()),
        );
      });
    });

    group('Register', () {
      test('should register successfully', () async {
        // Arrange
        const registerRequest = RegisterRequest(
          username: 'newuser',
          email: 'test@example.com',
          password: 'password123',
          confirmPassword: 'password123',
        );
        
        const registerResult = RegisterResult(success: true);

        final mockResponse = Response(
          data: registerResult.toJson(),
          statusCode: 200,
          requestOptions: RequestOptions(path: '/Auth/register'),
        );

        when(mockApiClient.post('/Auth/register', data: registerRequest.toJson()))
            .thenAnswer((_) async => mockResponse);

        // Act
        final result = await authService.register(registerRequest);

        // Assert
        expect(result.success, isTrue);
        expect(result.error, isNull);

        // Verify API call
        verify(mockApiClient.post('/Auth/register', data: registerRequest.toJson())).called(1);
      });

      test('should handle registration failure with field errors', () async {
        // Arrange
        const registerRequest = RegisterRequest(
          username: 'existinguser',
          email: 'test@example.com',
          password: 'password123',
          confirmPassword: 'password123',
        );
        
        const registerResult = RegisterResult(
          success: false,
          error: 'Registration failed',
          fieldErrors: {'username': 'Username already exists'},
        );

        final mockResponse = Response(
          data: registerResult.toJson(),
          statusCode: 400,
          requestOptions: RequestOptions(path: '/Auth/register'),
        );

        when(mockApiClient.post('/Auth/register', data: registerRequest.toJson()))
            .thenAnswer((_) async => mockResponse);

        // Act
        final result = await authService.register(registerRequest);

        // Assert
        expect(result.success, isFalse);
        expect(result.error, equals('Registration failed'));
        expect(result.fieldErrors, equals({'username': 'Username already exists'}));
      });
    });

    group('Token Management', () {
      test('should load stored token on initialization', () async {
        // Arrange
        when(mockSharedPreferences.getString('auth_token'))
            .thenReturn('stored-token-123');
        when(mockSharedPreferences.getString('user_role'))
            .thenReturn('Client');
        when(mockSharedPreferences.getString('username'))
            .thenReturn('testuser');

        // Act
        await authService.loadStoredAuth();

        // Assert
        expect(authService.isAuthenticated, isTrue);
        expect(authService.currentToken, equals('stored-token-123'));
        expect(authService.currentRole, equals('Client'));
        expect(authService.currentUsername, equals('testuser'));

        // Verify token set on API client
        verify(mockApiClient.setAuthToken('stored-token-123')).called(1);
      });

      test('should handle missing stored token', () async {
        // Arrange
        when(mockSharedPreferences.getString('auth_token')).thenReturn(null);

        // Act
        await authService.loadStoredAuth();

        // Assert
        expect(authService.isAuthenticated, isFalse);
        expect(authService.currentToken, isNull);
        expect(authService.currentRole, isNull);
        expect(authService.currentUsername, isNull);

        // Verify no token set on API client
        verifyNever(mockApiClient.setAuthToken(any));
      });

      test('should logout and clear stored data', () async {
        // Arrange
        when(mockSharedPreferences.remove('auth_token')).thenAnswer((_) async => true);
        when(mockSharedPreferences.remove('user_role')).thenAnswer((_) async => true);
        when(mockSharedPreferences.remove('username')).thenAnswer((_) async => true);

        // Act
        await authService.logout();

        // Assert
        expect(authService.isAuthenticated, isFalse);
        expect(authService.currentToken, isNull);
        expect(authService.currentRole, isNull);
        expect(authService.currentUsername, isNull);

        // Verify storage clearing
        verify(mockSharedPreferences.remove('auth_token')).called(1);
        verify(mockSharedPreferences.remove('user_role')).called(1);
        verify(mockSharedPreferences.remove('username')).called(1);
        
        // Verify token cleared from API client
        verify(mockApiClient.clearAuthToken()).called(1);
      });
    });

    group('Two-Factor Authentication', () {
      test('should verify 2FA successfully', () async {
        // Arrange
        const verifyRequest = VerifyTwoFactorRequest(
          username: 'testuser',
          twoFactorCode: '123456',
          twoFactorToken: '2fa-token-123',
        );
        
        const loginResult = LoginResult(
          success: true,
          token: 'jwt-token-456',
          username: 'testuser',
          role: 'Client',
          permissions: ['read'],
          requiresTwoFactor: false,
        );

        final mockResponse = Response(
          data: loginResult.toJson(),
          statusCode: 200,
          requestOptions: RequestOptions(path: '/Auth/verify-2fa'),
        );

        when(mockApiClient.post('/Auth/verify-2fa', data: verifyRequest.toJson()))
            .thenAnswer((_) async => mockResponse);
        when(mockSharedPreferences.setString('auth_token', 'jwt-token-456'))
            .thenAnswer((_) async => true);
        when(mockSharedPreferences.setString('user_role', 'Client'))
            .thenAnswer((_) async => true);
        when(mockSharedPreferences.setString('username', 'testuser'))
            .thenAnswer((_) async => true);

        // Act
        final result = await authService.verifyTwoFactor(verifyRequest);

        // Assert
        expect(result.success, isTrue);
        expect(result.token, equals('jwt-token-456'));
        expect(result.requiresTwoFactor, isFalse);

        // Verify token storage
        verify(mockSharedPreferences.setString('auth_token', 'jwt-token-456')).called(1);
        verify(mockApiClient.setAuthToken('jwt-token-456')).called(1);
      });
    });
  });
}