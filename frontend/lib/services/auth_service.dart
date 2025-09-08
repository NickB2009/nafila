import 'package:shared_preferences/shared_preferences.dart';
import 'package:dio/dio.dart';
import '../models/auth_models.dart';
import '../config/api_config.dart';
import 'api_client.dart';

/// Service for handling authentication operations
class AuthService {
  final dynamic _apiClient; // Use dynamic to allow mocking
  final dynamic _sharedPreferences; // Use dynamic to allow mocking

  // Authentication state
  String? _currentToken;
  String? _currentPhoneNumber;
  String? _currentRole;
  User? _currentUser;

  AuthService({
    required dynamic apiClient,
    required dynamic sharedPreferences,
  }) : _apiClient = apiClient,
       _sharedPreferences = sharedPreferences;

  /// Creates AuthService with default dependencies
  static Future<AuthService> create() async {
    final apiClient = ApiClient();
    final sharedPreferences = await SharedPreferences.getInstance();
    final authService = AuthService(
      apiClient: apiClient,
      sharedPreferences: sharedPreferences,
    );
    await authService.loadStoredAuth();
    return authService;
  }

  // Getters for authentication state
  bool get isAuthenticated => _currentToken != null;
  String? get currentToken => _currentToken;
  String? get currentPhoneNumber => _currentPhoneNumber;
  String? get currentRole => _currentRole;
  User? get currentUser => _currentUser;

  /// Loads stored authentication data from SharedPreferences
  Future<void> loadStoredAuth() async {
    _currentToken = _sharedPreferences.getString('auth_token');
    _currentPhoneNumber = _sharedPreferences.getString('phone_number');
    _currentRole = _sharedPreferences.getString('user_role');

    if (_currentToken != null) {
      _apiClient.setAuthToken(_currentToken!);
      // Try to load user profile if token exists
      try {
        _currentUser = await getProfile();
      } catch (e) {
        // If profile loading fails, token might be expired
        print('Failed to load user profile: $e');
        await logout(); // Clear invalid auth data
      }
    }
  }

  /// Authenticates user with username and password
  Future<LoginResult> login(LoginRequest request) async {
    try {
      print('🔑 Attempting login for phone: ${request.phoneNumber}');
      print('🔗 Login endpoint: ${ApiConfig.getUrl(ApiConfig.authLoginEndpoint)}');
      
      final response = await _apiClient.post(
        ApiConfig.authLoginEndpoint,
        data: request.toJson(),
      );

      print('✅ Login response status: ${response.statusCode}');
      print('📝 Login response data: ${response.data}');

      final loginResult = LoginResult.fromJson(response.data);

      // Store authentication data if login was successful
      if (loginResult.success && loginResult.token != null) {
        print('✅ Login successful, storing auth data');
        await _storeAuthData(
          token: loginResult.token!,
          phoneNumber: loginResult.phoneNumber,
          role: loginResult.role,
        );
        
        // Get full user profile after successful login
        try {
          _currentUser = await getProfile();
        } catch (e) {
          print('Failed to load user profile after login: $e');
        }
        print('✅ Auth data stored successfully');
      } else {
        print('❌ Login failed: ${loginResult.error}');
        if (loginResult.requiresTwoFactor) {
          print('🔐 Two-factor authentication required');
        }
      }

      return loginResult;
    } catch (e) {
      print('❌ Login exception: $e');
      
      // Parse structured errors returned with 4xx
      if (e is DioException && e.response?.data != null) {
        print('🔍 DioException details:');
        print('   Status: ${e.response?.statusCode}');
        print('   Response: ${e.response?.data}');
        print('   URL: ${e.requestOptions.uri}');
        
        try {
          final data = e.response!.data;
          final result = LoginResult.fromJson(data);
          print('🔍 Parsed error result: success=${result.success}, error=${result.error}');
          return result;
        } catch (parseError) {
          print('❌ Failed to parse login error response: $parseError');
        }
      }
      return const LoginResult(success: false, error: 'Erro ao efetuar login');
    }
  }

  /// Registers a new user account
  Future<RegisterResult> register(RegisterRequest request) async {
    try {
      print('🔐 Attempting registration for user: ${request.fullName}');
      print('🔗 Registration endpoint: ${ApiConfig.getUrl(ApiConfig.authRegisterEndpoint)}');
      
      final requestData = request.toJson();
      print('📤 Request data being sent: $requestData');
      
      final response = await _apiClient.post(
        ApiConfig.authRegisterEndpoint,
        data: requestData,
      );

      print('✅ Registration response status: ${response.statusCode}');
      print('📝 Registration response data: ${response.data}');
      
      final result = RegisterResult.fromJson(response.data);
      
      if (result.success) {
        print('✅ Registration successful for user: ${request.fullName}');
      } else {
        print('❌ Registration failed: ${result.error}');
        if (result.fieldErrors != null) {
          print('🔍 Field errors: ${result.fieldErrors}');
        }
      }
      
      return result;
    } catch (e) {
      print('❌ Registration exception: $e');
      
      if (e is DioException && e.response?.data != null) {
        print('🔍 DioException details:');
        print('   Status: ${e.response?.statusCode}');
        print('   Response: ${e.response?.data}');
        print('   URL: ${e.requestOptions.uri}');
        
        try {
          final responseData = e.response!.data;
          
          // Handle the new backend error format
          if (responseData is Map && responseData.containsKey('error')) {
            final result = RegisterResult(
              success: false,
              error: responseData['message'] ?? responseData['error'] ?? 'Registration failed',
              fieldErrors: responseData['details'] is List 
                ? {'general': (responseData['details'] as List).join(', ')}
                : null,
            );
            print('🔍 Parsed server error: ${result.error}');
            return result;
          } else {
            // Try to parse as RegisterResult for backward compatibility
            final result = RegisterResult.fromJson(responseData);
            print('🔍 Parsed error result: success=${result.success}, error=${result.error}');
            return result;
          }
        } catch (parseError) {
          print('❌ Failed to parse error response: $parseError');
          
          // Return a generic error with the HTTP status
          return RegisterResult(
            success: false,
            error: 'Server error (${e.response?.statusCode}): ${e.response?.statusMessage ?? 'Unknown error'}',
          );
        }
      }
      return const RegisterResult(success: false, error: 'Erro ao criar conta');
    }
  }

  /// Verifies two-factor authentication code
  Future<LoginResult> verifyTwoFactor(VerifyTwoFactorRequest request) async {
    try {
      final response = await _apiClient.post(
        ApiConfig.authVerify2FAEndpoint,
        data: request.toJson(),
      );

      final loginResult = LoginResult.fromJson(response.data);

      // Store authentication data if verification was successful
      if (loginResult.success && loginResult.token != null) {
        await _storeAuthData(
          token: loginResult.token!,
          phoneNumber: loginResult.phoneNumber,
          role: loginResult.role,
        );
        
        // Get full user profile after successful 2FA verification
        try {
          _currentUser = await getProfile();
        } catch (e) {
          print('Failed to load user profile after 2FA: $e');
        }
      }

      return loginResult;
    } catch (e) {
      rethrow;
    }
  }

  /// Gets user profile information
  Future<User> getProfile() async {
    if (!isAuthenticated) {
      throw Exception('User not authenticated');
    }

    try {
      final response = await _apiClient.get(ApiConfig.authProfileEndpoint);
      final user = User.fromJson(response.data);
      _currentUser = user;
      return user;
    } catch (e) {
      rethrow;
    }
  }

  /// Logs out the current user and clears stored data
  Future<void> logout() async {
    // Clear local state
    _currentToken = null;
    _currentPhoneNumber = null;
    _currentRole = null;
    _currentUser = null;

    // Clear stored data
    await _sharedPreferences.remove('auth_token');
    await _sharedPreferences.remove('phone_number');
    await _sharedPreferences.remove('user_role');

    // Clear API client token
    _apiClient.clearAuthToken();
  }

  /// Clears all cached user data including test/demo data
  Future<void> clearAllCachedData() async {
    // Clear all authentication-related data
    await logout();
    
    // Clear any additional cached data that might contain test users
    await _sharedPreferences.clear();
    
    print('🧹 All cached data cleared');
  }

  /// Stores authentication data locally and updates API client
  Future<void> _storeAuthData({
    required String token,
    String? phoneNumber,
    String? role,
  }) async {
    // Update local state
    _currentToken = token;
    _currentPhoneNumber = phoneNumber;
    _currentRole = role;

    // Store in SharedPreferences
    await _sharedPreferences.setString('auth_token', token);
    if (phoneNumber != null) {
      await _sharedPreferences.setString('phone_number', phoneNumber);
    }
    if (role != null) {
      await _sharedPreferences.setString('user_role', role);
    }

    // Set token on API client
    _apiClient.setAuthToken(token);
  }

  /// Checks if the current user has a specific permission
  bool hasPermission(String permission) {
    // This would typically check against stored permissions
    // For now, we'll implement basic role-based access
    switch (_currentRole) {
      case 'PlatformAdmin':
        return true; // Platform admins have all permissions
      case 'Admin':
        return ['read', 'write', 'admin'].contains(permission);
      case 'Barber':
        return ['read', 'write'].contains(permission);
      case 'Client':
        return ['read'].contains(permission);
      default:
        return false;
    }
  }

  /// Checks if the current user has a specific role
  bool hasRole(String role) {
    return _currentRole == role;
  }

  /// Demo-only login (local token without hitting the server)
  Future<void> demoLogin({String phoneNumber = '+5511999999999', String role = 'Customer'}) async {
    // Generate a pseudo token and store
    final token = 'demo-token-${DateTime.now().millisecondsSinceEpoch}';
    await _storeAuthData(token: token, phoneNumber: phoneNumber, role: role);
    
    // Create a demo user
    _currentUser = User(
      id: 'demo-user-id',
      fullName: 'Usuário Demo',
      email: 'demo@example.com',
      phoneNumber: phoneNumber,
      role: role,
      permissions: ['read'],
    );
  }
}