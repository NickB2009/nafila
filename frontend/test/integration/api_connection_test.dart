import 'package:flutter_test/flutter_test.dart';
import 'package:dio/dio.dart';
import 'package:eutonafila_frontend/config/api_config.dart';
import 'package:eutonafila_frontend/services/public_salon_service.dart';

void main() {
  group('API Connection Integration Tests', () {
    late Dio dio;
    
    setUp(() {
      dio = Dio();
      // Initialize API config for testing
      ApiConfig.initialize();
    });

    test('should connect to API base URL', () async {
      try {
        print('Testing connection to: ${ApiConfig.currentBaseUrl}');
        
        final response = await dio.get(
          ApiConfig.currentBaseUrl,
          options: Options(
            headers: ApiConfig.defaultHeaders,
            validateStatus: (status) => status! < 500, // Accept 4xx as valid connection
          ),
        );
        
        print('✅ Connection successful: ${response.statusCode}');
        expect(response.statusCode, lessThan(500));
      } catch (e) {
        print('❌ Connection failed: $e');
        if (e is DioException) {
          print('Error type: ${e.type}');
          print('Error message: ${e.message}');
          if (e.response != null) {
            print('Response status: ${e.response!.statusCode}');
            print('Response data: ${e.response!.data}');
          }
        }
        fail('Cannot connect to API: $e');
      }
    });

    test('should reach public salons endpoint', () async {
      try {
        final url = ApiConfig.getUrl(ApiConfig.publicSalonsEndpoint);
        print('Testing salons endpoint: $url');
        
        final response = await dio.get(
          url,
          options: Options(
            headers: ApiConfig.defaultHeaders,
            validateStatus: (status) => status! < 500,
          ),
        );
        
        print('✅ Salons endpoint reachable: ${response.statusCode}');
        
        if (response.statusCode == 200) {
          print('✅ Got salon data: ${response.data}');
          expect(response.data, isA<List>());
        } else {
          print('⚠️ Endpoint exists but returned: ${response.statusCode}');
        }
        
        expect(response.statusCode, lessThan(500));
      } catch (e) {
        print('❌ Salons endpoint failed: $e');
        if (e is DioException) {
          print('Error type: ${e.type}');
          print('Error message: ${e.message}');
          print('Request URL: ${e.requestOptions.uri}');
        }
        fail('Cannot reach salons endpoint: $e');
      }
    });

    test('should test PublicSalonService integration', () async {
      try {
        final service = PublicSalonService.create();
        print('Testing PublicSalonService...');
        
        final salons = await service.getPublicSalons();
        print('✅ Got ${salons.length} salons from service');
        
        expect(salons, isA<List>());
        if (salons.isNotEmpty) {
          print('First salon: ${salons.first.name}');
        }
      } catch (e) {
        print('❌ PublicSalonService failed: $e');
        fail('PublicSalonService integration failed: $e');
      }
    });

    test('should test different API URLs', () async {
      final testUrls = [
        'https://localhost:7126/api',
        'http://localhost:7126/api',
        'https://localhost:7126',
        'http://localhost:7126',
      ];

      for (final testUrl in testUrls) {
        try {
          print('Testing URL: $testUrl');
          
          final testDio = Dio();
          testDio.options.connectTimeout = const Duration(seconds: 5);
          testDio.options.receiveTimeout = const Duration(seconds: 5);
          
          final response = await testDio.get(
            '$testUrl/Public/salons',
            options: Options(
              headers: ApiConfig.defaultHeaders,
              validateStatus: (status) => status! < 500,
            ),
          );
          
          print('✅ SUCCESS with $testUrl: ${response.statusCode}');
          break; // Stop on first success
        } catch (e) {
          print('❌ Failed with $testUrl: ${e.toString().substring(0, 100)}...');
        }
      }
    });

    test('should check CORS and headers', () async {
      try {
        // Test with explicit headers
        final response = await dio.get(
          ApiConfig.getUrl(ApiConfig.publicSalonsEndpoint),
          options: Options(
            headers: {
              ...ApiConfig.defaultHeaders,
              'Origin': 'http://localhost:52288',
            },
            validateStatus: (status) => status! < 500,
          ),
        );
        
        print('✅ CORS test: ${response.statusCode}');
        print('Response headers: ${response.headers}');
      } catch (e) {
        print('❌ CORS test failed: $e');
      }
    });
  });
}