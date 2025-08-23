import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:dio/dio.dart';
import 'package:eutonafila_frontend/services/api_client.dart';
import 'package:eutonafila_frontend/config/api_config.dart';

// Generate mocks
@GenerateNiceMocks([MockSpec<Dio>(), MockSpec<BaseOptions>()])
import 'api_client_test.mocks.dart';

void main() {
  group('ApiClient Tests', () {
    late ApiClient apiClient;
    late MockDio mockDio;
    late MockBaseOptions mockOptions;
    late Map<String, dynamic> mockHeaders;

    setUp(() {
      mockDio = MockDio();
      mockOptions = MockBaseOptions();
      mockHeaders = <String, dynamic>{};
      
      // Setup the mock chain
      when(mockDio.options).thenReturn(mockOptions);
      when(mockOptions.headers).thenReturn(mockHeaders);
      
      apiClient = ApiClient.withDio(mockDio);
    });

    test('should create ApiClient with correct base configuration', () {
      final client = ApiClient();
      expect(client.dio.options.baseUrl, equals(ApiConfig.currentBaseUrl));
      expect(client.dio.options.connectTimeout, equals(const Duration(seconds: 30)));
      expect(client.dio.options.receiveTimeout, equals(const Duration(seconds: 30)));
    });

    test('should set authorization header when token is provided', () {
      const token = 'test-jwt-token';
      apiClient.setAuthToken(token);
      
      expect(mockHeaders['Authorization'], equals('Bearer $token'));
    });

    test('should clear authorization header when token is removed', () {
      mockHeaders['Authorization'] = 'Bearer test-token';
      apiClient.clearAuthToken();
      
      expect(mockHeaders.containsKey('Authorization'), isFalse);
    });

    test('should handle HTTP GET requests successfully', () async {
      const endpoint = '/test-endpoint';
      final mockResponse = Response(
        data: {'message': 'success'},
        statusCode: 200,
        requestOptions: RequestOptions(path: endpoint),
      );

      when(mockDio.get(endpoint)).thenAnswer((_) async => mockResponse);

      final response = await apiClient.get(endpoint);
      
      expect(response.statusCode, equals(200));
      expect(response.data['message'], equals('success'));
      verify(mockDio.get(endpoint)).called(1);
    });

    test('should handle HTTP POST requests with data', () async {
      const endpoint = '/test-endpoint';
      final requestData = {'name': 'test'};
      final mockResponse = Response(
        data: {'id': '123'},
        statusCode: 201,
        requestOptions: RequestOptions(path: endpoint),
      );

      when(mockDio.post(endpoint, data: requestData))
          .thenAnswer((_) async => mockResponse);

      final response = await apiClient.post(endpoint, data: requestData);
      
      expect(response.statusCode, equals(201));
      expect(response.data['id'], equals('123'));
      verify(mockDio.post(endpoint, data: requestData)).called(1);
    });

    test('should handle HTTP errors correctly', () async {
      const endpoint = '/error-endpoint';
      final dioError = DioException(
        requestOptions: RequestOptions(path: endpoint),
        response: Response(
          statusCode: 404,
          requestOptions: RequestOptions(path: endpoint),
        ),
        type: DioExceptionType.badResponse,
      );

      when(mockDio.get(endpoint)).thenThrow(dioError);

      expect(
        () async => await apiClient.get(endpoint),
        throwsA(isA<DioException>()),
      );
    });

    test('should add request interceptor for logging', () {
      final client = ApiClient();
      expect(client.dio.interceptors.length, greaterThan(0));
    });
  });
}