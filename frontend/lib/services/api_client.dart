import 'package:dio/dio.dart';
import 'package:logger/logger.dart';
import '../config/api_config.dart';

/// HTTP client wrapper for API communication
class ApiClient {
  late final Dio _dio;
  final Logger _logger = Logger();

  /// Get the underlying Dio instance
  Dio get dio => _dio;

  /// Default constructor with production configuration
  ApiClient() {
    _dio = Dio(_createBaseOptions());
    _setupInterceptors();
  }

  /// Constructor for testing with custom Dio instance
  ApiClient.withDio(this._dio);

  /// Creates base options for Dio
  BaseOptions _createBaseOptions() {
    return BaseOptions(
      baseUrl: ApiConfig.currentBaseUrl,
      connectTimeout: ApiConfig.connectTimeout,
      receiveTimeout: ApiConfig.receiveTimeout,
      sendTimeout: ApiConfig.sendTimeout,
      headers: ApiConfig.defaultHeaders,
    );
  }

  /// Sets up interceptors for logging and error handling
  void _setupInterceptors() {
    // Request/Response logging interceptor
    _dio.interceptors.add(
      LogInterceptor(
        requestHeader: true,
        requestBody: true,
        responseHeader: false,
        responseBody: true,
        error: true,
        logPrint: (object) => _logger.d(object),
      ),
    );

    // Error handling interceptor
    _dio.interceptors.add(
      InterceptorsWrapper(
        onError: (error, handler) {
          _logger.e('API Error: ${error.message}', error: error);
          handler.next(error);
        },
      ),
    );
  }

  /// Sets the authorization token for all requests
  void setAuthToken(String token) {
    _dio.options.headers['Authorization'] = 'Bearer $token';
  }

  /// Clears the authorization token
  void clearAuthToken() {
    _dio.options.headers.remove('Authorization');
  }

  /// Performs GET request without authentication (for public endpoints)
  Future<Response<T>> getPublic<T>(
    String path, {
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    // Create options that explicitly exclude Authorization header
    final publicOptions = options?.copyWith() ?? Options();
    publicOptions.headers = {
      ...?publicOptions.headers,
      'Authorization': null, // Explicitly remove auth header
    };
    
    return await _dio.get<T>(
      path,
      queryParameters: queryParameters,
      options: publicOptions,
    );
  }

  /// Performs POST request without authentication (for public endpoints)
  Future<Response<T>> postPublic<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    // Create options that explicitly exclude Authorization header
    final publicOptions = options?.copyWith() ?? Options();
    publicOptions.headers = {
      ...?publicOptions.headers,
      'Authorization': null, // Explicitly remove auth header
    };
    
    return await _dio.post<T>(
      path,
      data: data,
      queryParameters: queryParameters,
      options: publicOptions,
    );
  }

  /// Performs GET request
  Future<Response<T>> get<T>(
    String path, {
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    return await _dio.get<T>(
      path,
      queryParameters: queryParameters,
      options: options,
    );
  }

  /// Performs POST request
  Future<Response<T>> post<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    return await _dio.post<T>(
      path,
      data: data,
      queryParameters: queryParameters,
      options: options,
    );
  }

  /// Performs PUT request
  Future<Response<T>> put<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    return await _dio.put<T>(
      path,
      data: data,
      queryParameters: queryParameters,
      options: options,
    );
  }

  /// Performs DELETE request
  Future<Response<T>> delete<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    return await _dio.delete<T>(
      path,
      data: data,
      queryParameters: queryParameters,
      options: options,
    );
  }
}