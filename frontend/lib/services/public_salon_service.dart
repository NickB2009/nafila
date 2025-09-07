import '../models/public_salon.dart';
import '../config/api_config.dart';
import 'api_client.dart';
import 'package:dio/dio.dart';

/// Service for public salon data (no authentication required)
class PublicSalonService {
  final ApiClient _apiClient;

  const PublicSalonService({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  /// Creates PublicSalonService with default dependencies
  static PublicSalonService create() {
    final apiClient = ApiClient();
    return PublicSalonService(apiClient: apiClient);
  }

  /// Gets nearby salons without authentication
  Future<List<PublicSalon>> getNearbySlons({
    double? latitude,
    double? longitude,
    double radiusKm = 10.0,
    int limit = 20,
    bool openOnly = false,
  }) async {
    print('🔥 getNearbySlons() called - SHOULD NOT BE USED FOR MAIN PAGE!');
    
    final request = NearbySearchRequest(
      latitude: latitude,
      longitude: longitude,
      radiusKm: radiusKm,
      limit: limit,
      openOnly: openOnly,
    );

    final response = await _apiClient.postPublic(
      ApiConfig.publicLocationSearchEndpoint,
      data: request.toJson(),
    );

    final List<dynamic> data = response.data;
    return data.map((json) => PublicSalon.fromJson(json)).toList();
  }

  /// Gets all public salons without authentication
  Future<List<PublicSalon>> getPublicSalons() async {
    try {
      // Use direct Dio call with timeouts and centralized API configuration
      final dio = Dio(
        BaseOptions(
          connectTimeout: const Duration(seconds: 8),
          receiveTimeout: const Duration(seconds: 8),
          sendTimeout: const Duration(seconds: 8),
          headers: ApiConfig.defaultHeaders,
        ),
      );
      final response = await dio.get(
        ApiConfig.getUrl(ApiConfig.publicSalonsEndpoint),
      );

      final List<dynamic> data = response.data;
      return data.map((json) => PublicSalon.fromJson(json)).toList();
    } catch (e) {
      // No mock data fallback - let the error propagate to show real API issues
      print('❌ getPublicSalons API error: $e');
      rethrow;
    }
  }

  /// Gets public queue status for a salon
  Future<PublicQueueStatus?> getQueueStatus(String salonId) async {
    try {
      final response = await _apiClient.getPublic(
        '${ApiConfig.publicQueueStatusEndpoint}/$salonId',
      );
      
      return PublicQueueStatus.fromJson(response.data);
    } catch (e) {
      // No mock data fallback - let the error propagate to show real API issues
      print('❌ getQueueStatus API error for salon $salonId: $e');
      rethrow;
    }
  }

  /// Gets salon details without authentication
  Future<PublicSalon?> getSalonDetails(String salonId) async {
    try {
      final response = await _apiClient.getPublic(
        '${ApiConfig.publicSalonsEndpoint}/$salonId',
      );
      
      return PublicSalon.fromJson(response.data);
    } catch (e) {
      // No mock data fallback - let the error propagate to show real API issues
      print('❌ getSalonDetails API error for salon $salonId: $e');
      rethrow;
    }
  }

}