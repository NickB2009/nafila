import 'package:flutter/foundation.dart';
import '../services/auth_service.dart';
import '../services/queue_service.dart';
import '../services/location_service.dart';
import '../services/organization_service.dart';
import '../services/staff_service.dart';
import '../services/services_service.dart';
import '../services/public_salon_service.dart';
import '../services/signalr_service.dart';
import '../services/queue_analytics_service.dart';
import '../services/queue_transfer_service.dart';
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
  late final SignalRService _signalRService;
  late final QueueAnalyticsService _queueAnalyticsService;
  late final QueueTransferService _queueTransferService;

  // Controllers
  late final AuthController _authController;
  late final QueueController _queueController;
  late final AnonymousController _anonymousController;

  // State
  bool _isInitialized = false;
  bool _isInitializing = false;
  String? _error;
  bool _isAnonymousMode = true; // Start in anonymous mode

  // Getters
  bool get isInitialized => _isInitialized;
  bool get isInitializing => _isInitializing;
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
  SignalRService get signalRService => _signalRService;
  QueueAnalyticsService get queueAnalyticsService => _queueAnalyticsService;

  /// Check if real-time features are available
  bool get isRealTimeAvailable => _signalRService.isConnected;
  QueueTransferService get queueTransferService => _queueTransferService;

  /// Initializes all services and controllers
  Future<void> initialize() async {
    // Prevent multiple initializations
    if (_isInitialized || _isInitializing) {
      print('🔄 AppController already initialized or initializing, skipping...');
      return;
    }
    
    _isInitializing = true;
    print('🚀 Initializing AppController...');
    
    try {
      // Initialize services
      _authService = await AuthService.create();
      _queueService = await QueueService.create();
      _locationService = await LocationService.create();
      _organizationService = await OrganizationService.create();
      _staffService = await StaffService.create();
      _servicesService = await ServicesService.create();
      _publicSalonService = PublicSalonService.create();
      _signalRService = SignalRService();
      _queueAnalyticsService = await QueueAnalyticsService.create();
      _queueTransferService = await QueueTransferService.create();

      // Initialize controllers
      _authController = AuthController(authService: _authService);
      _queueController = QueueController(queueService: _queueService);
      _anonymousController = AnonymousController(publicSalonService: _publicSalonService);

      // Load stored authentication
      await _authController.loadStoredAuth();

      // If user is authenticated, switch to authenticated mode
      if (_authController.isAuthenticated) {
        _isAnonymousMode = false;
      }

      // Initialize SignalR service
      try {
        await _signalRService.initialize();
        print('✅ SignalR service initialized');
      } catch (e) {
        print('⚠️ SignalR service initialization failed: $e');
        print('📱 App will continue without real-time updates');
        // Don't block initialization if SignalR fails
      }

      // Load initial anonymous data (don't block init if it fails)
      if (_isAnonymousMode) {
        try {
          await _anonymousController.loadPublicSalons();
        } catch (_) {}
      }

      // Set up listeners AFTER all initialization is complete
      _setupListeners();

      _isInitialized = true;
      _isInitializing = false;
      _error = null;
      print('✅ AppController initialization complete');
      
      // Only notify once at the end of initialization
      notifyListeners();
    } catch (e) {
      _error = 'Failed to initialize app: ${e.toString()}';
      _isInitializing = false;
      notifyListeners();
      rethrow;
    }
  }

  /// Sets up listeners between controllers
  void _setupListeners() {
    // Listen to auth controller changes
    _authController.addListener(() {
      // Skip notifications during initialization
      if (_isInitializing) return;
      
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
      // Skip notifications during initialization
      if (_isInitializing) return;
      notifyListeners();
    });

    // Listen to anonymous controller changes
    _anonymousController.addListener(() {
      // Skip notifications during initialization
      if (_isInitializing) return;
      notifyListeners();
    });
  }

  /// Switches to authenticated mode after login
  Future<void> switchToAuthenticatedMode() async {
    _isAnonymousMode = false;
    _anonymousController.clear();

    // Reconnect SignalR if needed
    if (!_signalRService.isConnected) {
      try {
        await _signalRService.initialize();
        print('✅ SignalR reconnected after authentication');
      } catch (e) {
        print('⚠️ SignalR reconnection failed: $e');
      }
    }

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
    _anonymousController.dispose();
    super.dispose();
  }

  /// Resets the controller state for re-initialization
  void reset() {
    _isInitialized = false;
    _error = null;
    _isAnonymousMode = true;
  }
}