import 'package:flutter/foundation.dart';
import '../services/auth_service.dart';
import '../services/queue_service.dart';
import '../services/location_service.dart';
import '../services/organization_service.dart';
import '../services/staff_service.dart';
import '../services/services_service.dart';
import '../services/public_salon_service.dart';
import 'auth_controller.dart';
import 'queue_controller.dart';
import 'anonymous_controller.dart';

/// Main application controller that coordinates all other controllers
class AppController extends ChangeNotifier {
  // Services
  late final AuthService _authService;
  late final QueueService _queueService;
  late final LocationService _locationService;
  late final OrganizationService _organizationService;
  late final StaffService _staffService;
  late final ServicesService _servicesService;
  late final PublicSalonService _publicSalonService;

  // Controllers
  late final AuthController _authController;
  late final QueueController _queueController;
  late final AnonymousController _anonymousController;

  // State
  bool _isInitialized = false;
  String? _error;
  bool _isAnonymousMode = true; // Start in anonymous mode

  // Getters
  bool get isInitialized => _isInitialized;
  String? get error => _error;
  bool get isAnonymousMode => _isAnonymousMode;
  AuthController get auth => _authController;
  QueueController get queue => _queueController;
  AnonymousController get anonymous => _anonymousController;

  // Service getters for direct access if needed
  LocationService get locationService => _locationService;
  OrganizationService get organizationService => _organizationService;
  StaffService get staffService => _staffService;
  ServicesService get servicesService => _servicesService;
  PublicSalonService get publicSalonService => _publicSalonService;

  /// Initializes all services and controllers
  Future<void> initialize() async {
    try {
      // Initialize services
      _authService = await AuthService.create();
      _queueService = await QueueService.create();
      _locationService = await LocationService.create();
      _organizationService = await OrganizationService.create();
      _staffService = await StaffService.create();
      _servicesService = await ServicesService.create();
      _publicSalonService = PublicSalonService.create();

      // Initialize controllers
      _authController = AuthController(authService: _authService);
      _queueController = QueueController(queueService: _queueService);
      _anonymousController = AnonymousController(publicSalonService: _publicSalonService);

      // Set up listeners
      _setupListeners();

      // Load stored authentication
      await _authController.loadStoredAuth();

      // If user is authenticated, switch to authenticated mode
      if (_authController.isAuthenticated) {
        _isAnonymousMode = false;
      }

      // Load initial anonymous data
      if (_isAnonymousMode) {
        await _anonymousController.loadPublicSalons();
      }

      _isInitialized = true;
      _error = null;
      notifyListeners();
    } catch (e) {
      _error = 'Failed to initialize app: ${e.toString()}';
      notifyListeners();
      rethrow;
    }
  }

  /// Sets up listeners between controllers
  void _setupListeners() {
    // Listen to auth controller changes
    _authController.addListener(() {
      // When auth state changes, update anonymous mode
      final wasAnonymous = _isAnonymousMode;
      _isAnonymousMode = !_authController.isAuthenticated;

      if (!_authController.isAuthenticated) {
        _queueController.clear();
        // Switch to anonymous mode and load public data
        if (!wasAnonymous) {
          _anonymousController.loadPublicSalons();
        }
      } else {
        // User just logged in, clear anonymous data
        _anonymousController.clear();
      }
      notifyListeners();
    });

    // Listen to queue controller changes
    _queueController.addListener(() {
      notifyListeners();
    });

    // Listen to anonymous controller changes
    _anonymousController.addListener(() {
      notifyListeners();
    });
  }

  /// Switches to authenticated mode after login
  Future<void> switchToAuthenticatedMode() async {
    _isAnonymousMode = false;
    _anonymousController.clear();
    notifyListeners();
  }

  /// Handles when user attempts an action requiring authentication
  Future<bool> requireAuthentication(String action) async {
    if (_authController.isAuthenticated) {
      return true;
    }

    // User needs to authenticate for this action
    // Return false to indicate authentication is required
    _error = 'Please log in to $action';
    notifyListeners();
    return false;
  }

  /// Gets locations for the current organization (authenticated users only)
  Future<List<dynamic>> getLocations() async {
    try {
      if (_isAnonymousMode) {
        // For anonymous users, return public salon data
        return _anonymousController.nearbySalons;
      }

      if (!_authController.isAuthenticated) {
        throw Exception('User not authenticated');
      }

      final locations = await _locationService.getLocations();
      return locations;
    } catch (e) {
      _error = 'Failed to load locations: ${e.toString()}';
      notifyListeners();
      rethrow;
    }
  }

  /// Gets organization details by ID
  Future<Map<String, dynamic>> getOrganization(String organizationId) async {
    try {
      if (!_authController.isAuthenticated) {
        throw Exception('User not authenticated');
      }

      final organization = await _organizationService.getOrganization(organizationId);
      return organization;
    } catch (e) {
      _error = 'Failed to load organization: ${e.toString()}';
      notifyListeners();
      rethrow;
    }
  }

  /// Gets staff members for a location
  Future<List<dynamic>> getStaffMembers(String locationId) async {
    try {
      if (!_authController.isAuthenticated) {
        throw Exception('User not authenticated');
      }

      // This would be implemented when we add the getStaffMembers method to StaffService
      // For now, return empty list
      return [];
    } catch (e) {
      _error = 'Failed to load staff members: ${e.toString()}';
      notifyListeners();
      rethrow;
    }
  }

  /// Gets services offered at a location
  Future<List<dynamic>> getServices() async {
    try {
      if (!_authController.isAuthenticated) {
        throw Exception('User not authenticated');
      }

      final services = await _servicesService.getServices();
      return services;
    } catch (e) {
      _error = 'Failed to load services: ${e.toString()}';
      notifyListeners();
      rethrow;
    }
  }

  /// Clears any error state
  void clearError() {
    _error = null;
    notifyListeners();
  }

  /// Disposes all controllers and clears state
  @override
  void dispose() {
    _authController.dispose();
    _queueController.dispose();
    super.dispose();
  }
}