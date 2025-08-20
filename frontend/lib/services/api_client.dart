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
      // Treat all <500 responses as valid to avoid throwing for expected 4xx
      validateStatus: (status) => status != null && status < 500,
    );
  }

  /// Sets up interceptors for logging and error handling
  void _setupInterceptors() {
    // Sanitized logging + error handling interceptor
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) {
          // Build a sanitized snapshot for logging (do not modify real request body)
          dynamic bodyPreview;
          final data = options.data;
          if (data is Map) {
            final cloned = Map<String, dynamic>.from(data);
            for (final key in ['password', 'confirmPassword', 'token', 'refreshToken']) {
              if (cloned.containsKey(key)) cloned[key] = '***';
            }
            bodyPreview = cloned;
          } else {
            bodyPreview = data is String && data.length > 200 ? '${data.substring(0, 200)}…' : data;
          }

          // Sanitize headers
          final sanitizedHeaders = Map<String, dynamic>.from(options.headers);
          for (final key in ['Authorization', 'authorization']) {
            if (sanitizedHeaders.containsKey(key)) sanitizedHeaders[key] = 'Bearer ***';
          }

          _logger.d('HTTP → ${options.method} ${options.uri}\nheaders: $sanitizedHeaders\nconnectTimeout: ${_dio.options.connectTimeout}\nreceiveTimeout: ${_dio.options.receiveTimeout}\nsendTimeout: ${_dio.options.sendTimeout}\nbody: $bodyPreview');
          handler.next(options);
        },
        onError: (error, handler) {
          final status = error.response?.statusCode;
          // Convert expected client errors into normal responses
          if (status != null && status < 500 && error.response != null) {
            handler.resolve(error.response!);
            return;
          }
          _logger.e('API Error: ${error.message}', error: error);
          handler.next(error);
        },
        onResponse: (response, handler) {
          // Log concise response info
          final isBinary = response.data is List<int>;
          final bodyPreview = isBinary
              ? '<binary ${ (response.data as List<int>).length } bytes>'
              : response.data;
          _logger.d('HTTP ← ${response.statusCode} ${response.requestOptions.method} ${response.requestOptions.uri}\nbody: $bodyPreview');
          handler.next(response);
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