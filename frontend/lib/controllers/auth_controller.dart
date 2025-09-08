import 'package:flutter/foundation.dart';
import '../services/auth_service.dart';
import '../models/auth_models.dart';

/// Controller for managing authentication state and operations
class AuthController extends ChangeNotifier {
  final AuthService _authService;

  AuthController({required AuthService authService}) : _authService = authService;

  // State management
  bool _isLoading = false;
  String? _error;
  bool _requiresTwoFactor = false;
  String? _twoFactorToken;
  Map<String, String>? _fieldErrors;

  // Getters
  bool get isLoading => _isLoading;
  String? get error => _error;
  bool get isAuthenticated => _authService.isAuthenticated;
  String? get currentToken => _authService.currentToken;
  String? get currentPhoneNumber => _authService.currentPhoneNumber;
  User? get currentUser => _authService.currentUser;
  String? get currentRole => _authService.currentRole;
  bool get requiresTwoFactor => _requiresTwoFactor;
  String? get twoFactorToken => _twoFactorToken;
  Map<String, String>? get fieldErrors => _fieldErrors;

  /// Sets loading state and notifies listeners
  void _setLoading(bool loading) {
    _isLoading = loading;
    notifyListeners();
  }

  /// Sets error state and notifies listeners
  void _setError(String? error) {
    _error = error;
    notifyListeners();
  }

  /// Clears error state
  void clearError() {
    _setError(null);
    _fieldErrors = null;
  }

  /// Sets two-factor authentication state
  void _setTwoFactorState(bool requiresTwoFactor, String? token) {
    _requiresTwoFactor = requiresTwoFactor;
    _twoFactorToken = token;
    notifyListeners();
  }

  /// Logs in a user
  Future<bool> login(LoginRequest request) async {
    try {
      _setLoading(true);
      _setError(null);
      _fieldErrors = null;
      _setTwoFactorState(false, null);

      final result = await _authService.login(request);
      
      if (result.success) {
        if (result.requiresTwoFactor) {
          _setTwoFactorState(true, result.twoFactorToken);
          return false; // Not fully authenticated yet
        } else {
          _setTwoFactorState(false, null);
          return true; // Fully authenticated
        }
      } else {
        _setError(result.error ?? 'Login failed');
        return false;
      }
    } catch (e) {
      _setError('Error during login: ${e.toString()}');
      return false;
    } finally {
      _setLoading(false);
    }
  }

  /// Registers a new user
  Future<bool> register(RegisterRequest request) async {
    try {
      _setLoading(true);
      _setError(null);
      _fieldErrors = null;

      final result = await _authService.register(request);
      
      if (result.success) {
        return true;
      } else {
        _fieldErrors = result.fieldErrors;
        _setError(result.error ?? 'Registration failed');
        return false;
      }
    } catch (e) {
      _setError('Error during registration: ${e.toString()}');
      return false;
    } finally {
      _setLoading(false);
    }
  }

  /// Verifies two-factor authentication
  Future<bool> verifyTwoFactor(String twoFactorCode) async {
    if (!_requiresTwoFactor || _twoFactorToken == null || currentPhoneNumber == null) {
      _setError('Two-factor verification not available');
      return false;
    }

    try {
      _setLoading(true);
      _setError(null);

      final request = VerifyTwoFactorRequest(
        phoneNumber: currentPhoneNumber!,
        twoFactorCode: twoFactorCode,
        twoFactorToken: _twoFactorToken!,
      );

      final result = await _authService.verifyTwoFactor(request);
      
      if (result.success) {
        _setTwoFactorState(false, null);
        return true;
      } else {
        _setError(result.error ?? 'Two-factor verification failed');
        return false;
      }
    } catch (e) {
      _setError('Error during two-factor verification: ${e.toString()}');
      return false;
    } finally {
      _setLoading(false);
    }
  }

  /// Gets user profile information
  Future<User?> getProfile() async {
    if (!isAuthenticated) {
      _setError('User not authenticated');
      return null;
    }

    try {
      _setLoading(true);
      _setError(null);

      final result = await _authService.getProfile();
      return result;
    } catch (e) {
      _setError('Error loading profile: ${e.toString()}');
      return null;
    } finally {
      _setLoading(false);
    }
  }

  /// Logs out the current user
  Future<void> logout() async {
    try {
      _setLoading(true);
      _setError(null);

      await _authService.logout();
      _setTwoFactorState(false, null);
    } catch (e) {
      _setError('Error during logout: ${e.toString()}');
    } finally {
      _setLoading(false);
    }
  }

  /// Clears all cached data including test/demo users
  Future<void> clearAllCachedData() async {
    try {
      _setLoading(true);
      _setError(null);

      await _authService.clearAllCachedData();
      _setTwoFactorState(false, null);
      notifyListeners(); // Force UI refresh
    } catch (e) {
      _setError('Error clearing cached data: ${e.toString()}');
    } finally {
      _setLoading(false);
    }
  }

  /// Checks if the current user has a specific permission
  bool hasPermission(String permission) {
    return _authService.hasPermission(permission);
  }

  /// Checks if the current user has a specific role
  bool hasRole(String role) {
    return _authService.hasRole(role);
  }

  /// Loads stored authentication data
  Future<void> loadStoredAuth() async {
    try {
      await _authService.loadStoredAuth();
      notifyListeners();
    } catch (e) {
      _setError('Error loading stored authentication: ${e.toString()}');
    }
  }

  /// Clears all authentication state
  void clear() {
    _error = null;
    _isLoading = false;
    _requiresTwoFactor = false;
    _twoFactorToken = null;
    notifyListeners();
  }

  /// Demo login (local token without backend) for development
  Future<void> demoLogin({String phoneNumber = '+5511999999999', String role = 'Customer'}) async {
    await _authService.demoLogin(phoneNumber: phoneNumber, role: role);
    notifyListeners();
  }

  /// Verifies token by calling profile; returns true on success
  Future<bool> verifyProfile() async {
    try {
      final profile = await _authService.getProfile();
      return profile.id.isNotEmpty;
    } catch (_) {
      return false;
    }
  }
}