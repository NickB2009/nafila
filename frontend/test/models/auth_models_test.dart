import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/models/auth_models.dart';

void main() {
  group('Authentication Models Tests', () {
    group('LoginRequest', () {
      test('should create LoginRequest with required fields', () {
        const loginRequest = LoginRequest(
          phoneNumber: '+5511999999999',
          password: 'testpass123',
        );

        expect(loginRequest.phoneNumber, equals('+5511999999999'));
        expect(loginRequest.password, equals('testpass123'));
      });

      test('should serialize to JSON correctly', () {
        const loginRequest = LoginRequest(
          phoneNumber: '+5511999999999',
          password: 'testpass123',
        );

        final json = loginRequest.toJson();

        expect(json['PhoneNumber'], equals('+5511999999999'));
        expect(json['Password'], equals('testpass123'));
      });

      test('should deserialize from JSON correctly', () {
        final json = {
          'phoneNumber': '+5511999999999',
          'password': 'testpass123',
        };

        final loginRequest = LoginRequest.fromJson(json);

        expect(loginRequest.phoneNumber, equals('+5511999999999'));
        expect(loginRequest.password, equals('testpass123'));
      });
    });

    group('LoginResult', () {
      test('should create successful LoginResult', () {
        const loginResult = LoginResult(
          success: true,
          token: 'jwt-token-123',
          fullName: 'Test User',
          phoneNumber: '+5511999999999',
          email: 'test@example.com',
          role: 'Client',
          permissions: ['read'],
          requiresTwoFactor: false,
        );

        expect(loginResult.success, isTrue);
        expect(loginResult.token, equals('jwt-token-123'));
        expect(loginResult.fullName, equals('Test User'));
        expect(loginResult.phoneNumber, equals('+5511999999999'));
        expect(loginResult.email, equals('test@example.com'));
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
          phoneNumber: '+5511999999999',
        );

        expect(loginResult.success, isFalse);
        expect(loginResult.requiresTwoFactor, isTrue);
        expect(loginResult.twoFactorToken, equals('2fa-token-123'));
        expect(loginResult.phoneNumber, equals('+5511999999999'));
      });

      test('should serialize to JSON correctly', () {
        const loginResult = LoginResult(
          success: true,
          token: 'jwt-token-123',
          fullName: 'Test User',
          phoneNumber: '+5511999999999',
          email: 'test@example.com',
          role: 'Client',
          permissions: ['read', 'write'],
          requiresTwoFactor: false,
        );

        final json = loginResult.toJson();

        expect(json['success'], isTrue);
        expect(json['token'], equals('jwt-token-123'));
        expect(json['fullName'], equals('Test User'));
        expect(json['phoneNumber'], equals('+5511999999999'));
        expect(json['email'], equals('test@example.com'));
        expect(json['role'], equals('Client'));
        expect(json['permissions'], equals(['read', 'write']));
        expect(json['requiresTwoFactor'], isFalse);
      });

      test('should deserialize from JSON correctly', () {
        final json = {
          'success': true,
          'token': 'jwt-token-123',
          'fullName': 'Test User',
          'phoneNumber': '+5511999999999',
          'email': 'test@example.com',
          'role': 'Client',
          'permissions': ['read', 'write'],
          'requiresTwoFactor': false,
        };

        final loginResult = LoginResult.fromJson(json);

        expect(loginResult.success, isTrue);
        expect(loginResult.token, equals('jwt-token-123'));
        expect(loginResult.fullName, equals('Test User'));
        expect(loginResult.phoneNumber, equals('+5511999999999'));
        expect(loginResult.email, equals('test@example.com'));
        expect(loginResult.role, equals('Client'));
        expect(loginResult.permissions, equals(['read', 'write']));
        expect(loginResult.requiresTwoFactor, isFalse);
      });
    });

    group('RegisterRequest', () {
      test('should create RegisterRequest with all fields', () {
        const registerRequest = RegisterRequest(
          fullName: 'New User',
          email: 'test@example.com',
          phoneNumber: '+5511999999999',
          password: 'Password123!',
        );

        expect(registerRequest.fullName, equals('New User'));
        expect(registerRequest.email, equals('test@example.com'));
        expect(registerRequest.phoneNumber, equals('+5511999999999'));
        expect(registerRequest.password, equals('Password123!'));
      });

      test('should serialize to JSON correctly', () {
        const registerRequest = RegisterRequest(
          fullName: 'New User',
          email: 'test@example.com',
          phoneNumber: '+5511999999999',
          password: 'Password123!',
        );

        final json = registerRequest.toJson();

        expect(json['FullName'], equals('New User'));
        expect(json['Email'], equals('test@example.com'));
        expect(json['PhoneNumber'], equals('+5511999999999'));
        expect(json['Password'], equals('Password123!'));
      });

      test('should deserialize from JSON correctly', () {
        final json = {
          'fullName': 'New User',
          'email': 'test@example.com',
          'phoneNumber': '+5511999999999',
          'password': 'Password123!',
        };

        final registerRequest = RegisterRequest.fromJson(json);

        expect(registerRequest.fullName, equals('New User'));
        expect(registerRequest.email, equals('test@example.com'));
        expect(registerRequest.phoneNumber, equals('+5511999999999'));
        expect(registerRequest.password, equals('Password123!'));
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

    group('User', () {
      test('should create User with all fields', () {
        const user = User(
          id: 'user-123',
          fullName: 'Test User',
          email: 'test@example.com',
          phoneNumber: '+5511999999999',
          role: 'Customer',
          permissions: ['read'],
        );

        expect(user.id, equals('user-123'));
        expect(user.fullName, equals('Test User'));
        expect(user.email, equals('test@example.com'));
        expect(user.phoneNumber, equals('+5511999999999'));
        expect(user.role, equals('Customer'));
        expect(user.permissions, equals(['read']));
      });

      test('should serialize to JSON correctly', () {
        const user = User(
          id: 'user-123',
          fullName: 'Test User',
          email: 'test@example.com',
          phoneNumber: '+5511999999999',
          role: 'Customer',
          permissions: ['read', 'write'],
        );

        final json = user.toJson();

        expect(json['UserId'], equals('user-123'));
        expect(json['FullName'], equals('Test User'));
        expect(json['Email'], equals('test@example.com'));
        expect(json['PhoneNumber'], equals('+5511999999999'));
        expect(json['Role'], equals('Customer'));
        expect(json['Permissions'], equals(['read', 'write']));
      });

      test('should deserialize from JSON correctly', () {
        final json = {
          'UserId': 'user-123',
          'FullName': 'Test User',
          'Email': 'test@example.com',
          'PhoneNumber': '+5511999999999',
          'Role': 'Customer',
          'Permissions': ['read', 'write'],
        };

        final user = User.fromJson(json);

        expect(user.id, equals('user-123'));
        expect(user.fullName, equals('Test User'));
        expect(user.email, equals('test@example.com'));
        expect(user.phoneNumber, equals('+5511999999999'));
        expect(user.role, equals('Customer'));
        expect(user.permissions, equals(['read', 'write']));
      });

      test('should handle alternative JSON field names', () {
        final json = {
          'id': 'user-123',
          'fullName': 'Test User',
          'email': 'test@example.com',
          'phoneNumber': '+5511999999999',
          'role': 'Customer',
          'permissions': ['read'],
        };

        final user = User.fromJson(json);

        expect(user.id, equals('user-123'));
        expect(user.fullName, equals('Test User'));
        expect(user.email, equals('test@example.com'));
        expect(user.phoneNumber, equals('+5511999999999'));
        expect(user.role, equals('Customer'));
        expect(user.permissions, equals(['read']));
      });
    });
  });
}