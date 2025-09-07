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
  String? _currentUsername;
  String? _currentRole;

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
  String? get currentUsername => _currentUsername;
  String? get currentRole => _currentRole;

  /// Loads stored authentication data from SharedPreferences
  Future<void> loadStoredAuth() async {
    _currentToken = _sharedPreferences.getString('auth_token');
    _currentUsername = _sharedPreferences.getString('username');
    _currentRole = _sharedPreferences.getString('user_role');

    if (_currentToken != null) {
      _apiClient.setAuthToken(_currentToken!);
    }
  }

  /// Authenticates user with username and password
  Future<LoginResult> login(LoginRequest request) async {
    try {
      print('🔑 Attempting login for user: ${request.username}');
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
          username: loginResult.username,
          role: loginResult.role,
        );
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
      print('🔐 Attempting registration for user: ${request.username}');
      print('🔗 Registration endpoint: ${ApiConfig.getUrl(ApiConfig.authRegisterEndpoint)}');
      
      final response = await _apiClient.post(
        ApiConfig.authRegisterEndpoint,
        data: request.toJson(),
      );

      print('✅ Registration response status: ${response.statusCode}');
      print('📝 Registration response data: ${response.data}');
      
      final result = RegisterResult.fromJson(response.data);
      
      if (result.success) {
        print('✅ Registration successful for user: ${request.username}');
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
          final result = RegisterResult.fromJson(e.response!.data);
          print('🔍 Parsed error result: success=${result.success}, error=${result.error}');
          return result;
        } catch (parseError) {
          print('❌ Failed to parse error response: $parseError');
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
          username: loginResult.username,
          role: loginResult.role,
        );
      }

      return loginResult;
    } catch (e) {
      rethrow;
    }
  }

  /// Gets user profile information
  Future<Map<String, dynamic>> getProfile() async {
    if (!isAuthenticated) {
      throw Exception('User not authenticated');
    }

    try {
      final response = await _apiClient.get(ApiConfig.authProfileEndpoint);
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Logs out the current user and clears stored data
  Future<void> logout() async {
    // Clear local state
    _currentToken = null;
    _currentUsername = null;
    _currentRole = null;

    // Clear stored data
    await _sharedPreferences.remove('auth_token');
    await _sharedPreferences.remove('username');
    await _sharedPreferences.remove('user_role');

    // Clear API client token
    _apiClient.clearAuthToken();
  }

  /// Stores authentication data locally and updates API client
  Future<void> _storeAuthData({
    required String token,
    String? username,
    String? role,
  }) async {
    // Update local state
    _currentToken = token;
    _currentUsername = username;
    _currentRole = role;

    // Store in SharedPreferences
    await _sharedPreferences.setString('auth_token', token);
    if (username != null) {
      await _sharedPreferences.setString('username', username);
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
  Future<void> demoLogin({String username = 'demo', String role = 'Customer'}) async {
    // Generate a pseudo token and store
    final token = 'demo-token-${DateTime.now().millisecondsSinceEpoch}';
    await _storeAuthData(token: token, username: username, role: role);
  }
}