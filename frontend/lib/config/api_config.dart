/// API configuration for the application
class ApiConfig {
  // Base configuration
  static const String baseUrl = 'https://localhost:7126/api';
  static const Duration defaultTimeout = Duration(seconds: 30);

  // Timeout configurations (for compatibility)
  static const Duration connectTimeout = Duration(seconds: 30);
  static const Duration receiveTimeout = Duration(seconds: 30);
  static const Duration sendTimeout = Duration(seconds: 30);

  // Environment detection and base URL
  static String get currentBaseUrl => baseUrl;

  // Authentication endpoints
  static const String authBaseEndpoint = '/Auth';
  static const String loginEndpoint = '$authBaseEndpoint/login';
  static const String registerEndpoint = '$authBaseEndpoint/register';
  static const String verify2FAEndpoint = '$authBaseEndpoint/verify-two-factor';
  static const String profileEndpoint = '$authBaseEndpoint/profile';
  static const String logoutEndpoint = '$authBaseEndpoint/logout';
  
  // Legacy endpoint getters (for compatibility)
  static String get authLoginEndpoint => loginEndpoint;
  static String get authRegisterEndpoint => registerEndpoint;
  static String get authVerify2FAEndpoint => verify2FAEndpoint;
  static String get authProfileEndpoint => profileEndpoint;

  // Organization endpoints
  static const String organizationsEndpoint = '/Organizations';

  // Location endpoints
  static const String locationsEndpoint = '/Locations';

  // Queue endpoints
  static const String queuesEndpoint = '/Queues';

  // Staff endpoints
  static const String staffEndpoint = '/Staff';

  // Services endpoints
  static const String servicesOfferedEndpoint = '/ServicesOffered';

  // Analytics endpoints
  static const String analyticsEndpoint = '/Analytics';
  static const String calculateWaitEndpoint = '$analyticsEndpoint/calculate-wait';
  static const String crossBarbershopEndpoint = '$analyticsEndpoint/cross-barbershop';
  static const String organizationAnalyticsEndpoint = '$analyticsEndpoint/organization';
  static const String topOrganizationsEndpoint = '$analyticsEndpoint/top-organizations';

  // Public endpoints (no authentication required)
  static const String publicEndpoint = '/Public';
  static const String publicSalonsEndpoint = '$publicEndpoint/salons';
  static const String publicQueueStatusEndpoint = '$publicEndpoint/queue-status';
  static const String publicLocationSearchEndpoint = '$publicEndpoint/salons/nearby';

  // Default headers
  static const Map<String, String> defaultHeaders = {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  };

  /// Gets headers with authentication token
  static Map<String, String> getAuthHeaders(String? token) {
    final headers = Map<String, String>.from(defaultHeaders);
    if (token != null && token.isNotEmpty) {
      headers['Authorization'] = 'Bearer $token';
    }
    return headers;
  }

  /// Gets complete URL for an endpoint
  static String getUrl(String endpoint) {
    if (endpoint.startsWith('/')) {
      return '$baseUrl$endpoint';
    }
    return '$baseUrl/$endpoint';
  }
}