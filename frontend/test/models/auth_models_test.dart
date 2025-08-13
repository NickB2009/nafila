import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/models/auth_models.dart';

void main() {
  group('Authentication Models Tests', () {
    group('LoginRequest', () {
      test('should create LoginRequest with required fields', () {
        const loginRequest = LoginRequest(
          username: 'testuser',
          password: 'testpass123',
        );

        expect(loginRequest.username, equals('testuser'));
        expect(loginRequest.password, equals('testpass123'));
      });

      test('should serialize to JSON correctly', () {
        const loginRequest = LoginRequest(
          username: 'testuser',
          password: 'testpass123',
        );

        final json = loginRequest.toJson();

        expect(json['username'], equals('testuser'));
        expect(json['password'], equals('testpass123'));
      });

      test('should deserialize from JSON correctly', () {
        final json = {
          'username': 'testuser',
          'password': 'testpass123',
        };

        final loginRequest = LoginRequest.fromJson(json);

        expect(loginRequest.username, equals('testuser'));
        expect(loginRequest.password, equals('testpass123'));
      });
    });

    group('LoginResult', () {
      test('should create successful LoginResult', () {
        const loginResult = LoginResult(
          success: true,
          token: 'jwt-token-123',
          username: 'testuser',
          role: 'Client',
          permissions: ['read'],
          requiresTwoFactor: false,
        );

        expect(loginResult.success, isTrue);
        expect(loginResult.token, equals('jwt-token-123'));
        expect(loginResult.username, equals('testuser'));
        expect(loginResult.role, equals('Client'));
        expect(loginResult.permissions, equals(['read']));
        expect(loginResult.requiresTwoFactor, isFalse);
        expect(loginResult.error, isNull);
      });

      test('should create failed LoginResult with error', () {
        const loginResult = LoginResult(
          success: false,
          error: 'Invalid credentials',
        );

        expect(loginResult.success, isFalse);
        expect(loginResult.error, equals('Invalid credentials'));
        expect(loginResult.token, isNull);
      });

      test('should create LoginResult requiring 2FA', () {
        const loginResult = LoginResult(
          success: false,
          requiresTwoFactor: true,
          twoFactorToken: '2fa-token-123',
          username: 'testuser',
        );

        expect(loginResult.success, isFalse);
        expect(loginResult.requiresTwoFactor, isTrue);
        expect(loginResult.twoFactorToken, equals('2fa-token-123'));
        expect(loginResult.username, equals('testuser'));
      });

      test('should serialize to JSON correctly', () {
        const loginResult = LoginResult(
          success: true,
          token: 'jwt-token-123',
          username: 'testuser',
          role: 'Client',
          permissions: ['read', 'write'],
          requiresTwoFactor: false,
        );

        final json = loginResult.toJson();

        expect(json['success'], isTrue);
        expect(json['token'], equals('jwt-token-123'));
        expect(json['username'], equals('testuser'));
        expect(json['role'], equals('Client'));
        expect(json['permissions'], equals(['read', 'write']));
        expect(json['requiresTwoFactor'], isFalse);
      });

      test('should deserialize from JSON correctly', () {
        final json = {
          'success': true,
          'token': 'jwt-token-123',
          'username': 'testuser',
          'role': 'Client',
          'permissions': ['read', 'write'],
          'requiresTwoFactor': false,
        };

        final loginResult = LoginResult.fromJson(json);

        expect(loginResult.success, isTrue);
        expect(loginResult.token, equals('jwt-token-123'));
        expect(loginResult.username, equals('testuser'));
        expect(loginResult.role, equals('Client'));
        expect(loginResult.permissions, equals(['read', 'write']));
        expect(loginResult.requiresTwoFactor, isFalse);
      });
    });

    group('RegisterRequest', () {
      test('should create RegisterRequest with all fields', () {
        const registerRequest = RegisterRequest(
          username: 'newuser',
          email: 'test@example.com',
          password: 'password123',
          confirmPassword: 'password123',
        );

        expect(registerRequest.username, equals('newuser'));
        expect(registerRequest.email, equals('test@example.com'));
        expect(registerRequest.password, equals('password123'));
        expect(registerRequest.confirmPassword, equals('password123'));
      });

      test('should serialize to JSON correctly', () {
        const registerRequest = RegisterRequest(
          username: 'newuser',
          email: 'test@example.com',
          password: 'password123',
          confirmPassword: 'password123',
        );

        final json = registerRequest.toJson();

        expect(json['username'], equals('newuser'));
        expect(json['email'], equals('test@example.com'));
        expect(json['password'], equals('password123'));
        expect(json['confirmPassword'], equals('password123'));
      });

      test('should deserialize from JSON correctly', () {
        final json = {
          'username': 'newuser',
          'email': 'test@example.com',
          'password': 'password123',
          'confirmPassword': 'password123',
        };

        final registerRequest = RegisterRequest.fromJson(json);

        expect(registerRequest.username, equals('newuser'));
        expect(registerRequest.email, equals('test@example.com'));
        expect(registerRequest.password, equals('password123'));
        expect(registerRequest.confirmPassword, equals('password123'));
      });
    });

    group('RegisterResult', () {
      test('should create successful RegisterResult', () {
        const registerResult = RegisterResult(
          success: true,
        );

        expect(registerResult.success, isTrue);
        expect(registerResult.error, isNull);
        expect(registerResult.fieldErrors, isNull);
      });

      test('should create failed RegisterResult with error', () {
        const registerResult = RegisterResult(
          success: false,
          error: 'Registration failed',
          fieldErrors: {'email': 'Email already exists'},
        );

        expect(registerResult.success, isFalse);
        expect(registerResult.error, equals('Registration failed'));
        expect(registerResult.fieldErrors, equals({'email': 'Email already exists'}));
      });

      test('should serialize to JSON correctly', () {
        const registerResult = RegisterResult(
          success: false,
          error: 'Registration failed',
          fieldErrors: {'email': 'Email already exists'},
        );

        final json = registerResult.toJson();

        expect(json['success'], isFalse);
        expect(json['error'], equals('Registration failed'));
        expect(json['fieldErrors'], equals({'email': 'Email already exists'}));
      });
    });
  });
}