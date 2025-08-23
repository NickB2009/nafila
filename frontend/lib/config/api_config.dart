/// API configuration for the application with environment-based URL detection
class ApiConfig {
  // Environment-based configuration
  static const String _localUrl = 'https://localhost:7126/api';
  static const String _developmentUrl = 'https://localhost:7126/api';
  static const String _productionUrl = 'https://api.eutonafila.com.br/api';
  
  // Custom API URL for manual override (set this for your specific setup)
  static String? _customApiUrl;
  
  static const Duration defaultTimeout = Duration(seconds: 30);

  // Timeout configurations (for compatibility)
  static const Duration connectTimeout = Duration(seconds: 30);
  static const Duration receiveTimeout = Duration(seconds: 30);
  static const Duration sendTimeout = Duration(seconds: 30);

  /// Set a custom API URL for development/testing
  static void setCustomApiUrl(String? url) {
    _customApiUrl = url;
  }

  /// Get the current environment
  static ApiEnvironment get currentEnvironment {
    if (_customApiUrl != null) return ApiEnvironment.custom;
    
    // You can add more sophisticated environment detection here
    // For now, defaulting to development
    return ApiEnvironment.development;
  }

  // Environment detection and base URL
  static String get currentBaseUrl {
    if (_customApiUrl != null) {
      // Ensure custom URL has /api suffix
      final url = _customApiUrl!;
      return url.endsWith('/api') ? url : '$url/api';
    }
    
    switch (currentEnvironment) {
      case ApiEnvironment.local:
        return _localUrl;
      case ApiEnvironment.development:
        return _developmentUrl;
      case ApiEnvironment.production:
        return _productionUrl;
      case ApiEnvironment.custom:
        return _customApiUrl!;
    }
  }

  /// Initialize API configuration with optional custom URL
  /// Call this in main() to set your API URL
  /// 
  /// Examples:
  /// - ApiConfig.initialize(); // Uses default localhost:7126
  /// - ApiConfig.initialize(apiUrl: 'https://your-api.com'); // Custom API
  /// - ApiConfig.initialize(apiUrl: 'http://192.168.1.100:7126'); // Local network
  static void initialize({String? apiUrl}) {
    if (apiUrl != null) {
      setCustomApiUrl(apiUrl);
      print('🔗 API configured for: $currentBaseUrl');
    } else {
      print('🔗 API configured for: $currentBaseUrl (default)');
    }
  }

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

  // Queue Analytics endpoints
  static const String queueAnalyticsEndpoint = '/QueueAnalytics';
  static const String waitTimeEndpoint = '$queueAnalyticsEndpoint/wait-time';
  static const String performanceEndpoint = '$queueAnalyticsEndpoint/performance';
  static const String trendsEndpoint = '$queueAnalyticsEndpoint/trends';
  static const String satisfactionEndpoint = '$queueAnalyticsEndpoint/satisfaction';
  static const String recommendationsEndpoint = '$queueAnalyticsEndpoint/recommendations';
  static const String healthEndpoint = '$queueAnalyticsEndpoint/health';

  // QR Code endpoints
  static const String qrCodeEndpoint = '/QrCode';
  static const String generateQrEndpoint = '$qrCodeEndpoint/generate';

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
      return '$currentBaseUrl$endpoint';
    }
    return '$currentBaseUrl/$endpoint';
  }
}

/// API Environment enumeration
enum ApiEnvironment {
  local,
  development,
  production,
  custom,
}