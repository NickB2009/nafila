import 'package:flutter_test/flutter_test.dart';
import 'package:dio/dio.dart';
import '../../lib/config/api_config.dart';
import '../../lib/services/public_salon_service.dart';

void main() {
  group('Production API Integration Tests', () {
    late Dio dio;
    
    setUp(() {
      dio = Dio();
      // Configure for production API
      ApiConfig.initialize(apiUrl: 'https://api.eutonafila.com.br');
    });

    test('should connect to production API base URL', () async {
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
        fail('Cannot connect to production API: $e');
      }
    });

    test('should reach production swagger endpoint', () async {
      try {
        print('Testing Swagger endpoint: https://api.eutonafila.com.br/swagger/index.html');
        
        final response = await dio.get(
          'https://api.eutonafila.com.br/swagger/index.html',
          options: Options(
            validateStatus: (status) => status! < 500,
          ),
        );
        
        print('✅ Swagger endpoint reachable: ${response.statusCode}');
        expect(response.statusCode, equals(200));
      } catch (e) {
        print('❌ Swagger endpoint failed: $e');
        fail('Cannot reach Swagger documentation: $e');
      }
    });

    test('should reach production public salons endpoint', () async {
      try {
        final url = ApiConfig.getUrl(ApiConfig.publicSalonsEndpoint);
        print('Testing production salons endpoint: $url');
        
        final response = await dio.get(
          url,
          options: Options(
            headers: ApiConfig.defaultHeaders,
            validateStatus: (status) => status! < 500,
          ),
        );
        
        print('✅ Production salons endpoint: ${response.statusCode}');
        
        if (response.statusCode == 200) {
          print('✅ Got salon data: ${response.data}');
          expect(response.data, isA<List>());
          
          final List salons = response.data;
          print('Number of salons: ${salons.length}');
          
          if (salons.isNotEmpty) {
            print('First salon structure: ${salons.first}');
          }
        } else if (response.statusCode == 404) {
          print('⚠️ Endpoint not found - check API documentation');
        } else {
          print('⚠️ Unexpected status: ${response.statusCode}');
        }
        
        expect(response.statusCode, lessThan(500));
      } catch (e) {
        print('❌ Production salons endpoint failed: $e');
        if (e is DioException) {
          print('Error type: ${e.type}');
          print('Error message: ${e.message}');
          print('Request URL: ${e.requestOptions.uri}');
          if (e.response != null) {
            print('Response status: ${e.response!.statusCode}');
            print('Response headers: ${e.response!.headers}');
            print('Response data: ${e.response!.data}');
          }
        }
        fail('Cannot reach production salons endpoint: $e');
      }
    });

    test('should test PublicSalonService with production API', () async {
      try {
        final service = PublicSalonService.create();
        print('Testing PublicSalonService with production API...');
        
        final salons = await service.getPublicSalons();
        print('✅ Got ${salons.length} salons from production service');
        
        expect(salons, isA<List>());
        if (salons.isNotEmpty) {
          final firstSalon = salons.first;
          print('First salon name: ${firstSalon.name}');
          print('First salon address: ${firstSalon.address}');
          print('First salon queue length: ${firstSalon.queueLength}');
          print('First salon wait time: ${firstSalon.currentWaitTimeMinutes}');
        }
      } catch (e) {
        print('❌ Production PublicSalonService failed: $e');
        if (e is DioException && e.response != null) {
          print('Response status: ${e.response!.statusCode}');
          print('Response data: ${e.response!.data}');
        }
        fail('Production PublicSalonService integration failed: $e');
      }
    });

    test('should test different API endpoints', () async {
      final endpoints = [
        '/Public/salons',
        '/api/Public/salons', 
        '/swagger/index.html',
      ];

      for (final endpoint in endpoints) {
        try {
          print('Testing endpoint: https://api.eutonafila.com.br$endpoint');
          
          final response = await dio.get(
            'https://api.eutonafila.com.br$endpoint',
            options: Options(
              headers: ApiConfig.defaultHeaders,
              validateStatus: (status) => status! < 500,
            ),
          );
          
          print('✅ SUCCESS with $endpoint: ${response.statusCode}');
          
          if (endpoint.contains('salons') && response.statusCode == 200) {
            final List data = response.data;
            print('   Salons count: ${data.length}');
          }
        } catch (e) {
          print('❌ Failed with $endpoint: ${e.toString().substring(0, 100)}...');
        }
      }
    });
  });
}