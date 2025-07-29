import '../models/location_models.dart';
import '../config/api_config.dart';
import 'api_client.dart';

/// Service for handling location-related operations
class LocationService {
  final ApiClient _apiClient;

  const LocationService({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  /// Creates LocationService with default dependencies
  static Future<LocationService> create() async {
    final apiClient = ApiClient();
    return LocationService(apiClient: apiClient);
  }

  /// Gets all locations
  Future<List<dynamic>> getLocations() async {
    try {
      final response = await _apiClient.get(ApiConfig.locationsEndpoint);
      return response.data as List<dynamic>;
    } catch (e) {
      rethrow;
    }
  }

  /// Creates a new location
  Future<Map<String, dynamic>> createLocation(CreateLocationRequest request) async {
    try {
      final response = await _apiClient.post(
        ApiConfig.locationsEndpoint,
        data: request.toJson(),
      );
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Gets location details by ID
  Future<Map<String, dynamic>> getLocation(String locationId) async {
    try {
      final response = await _apiClient.get('${ApiConfig.locationsEndpoint}/$locationId');
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Updates an existing location
  Future<Map<String, dynamic>> updateLocation(String locationId, UpdateLocationRequest request) async {
    try {
      final response = await _apiClient.put(
        '${ApiConfig.locationsEndpoint}/$locationId',
        data: request.toJson(),
      );
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Deletes a location
  Future<Map<String, dynamic>> deleteLocation(String locationId) async {
    try {
      final response = await _apiClient.delete('${ApiConfig.locationsEndpoint}/$locationId');
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Toggles queue status for a location
  Future<Map<String, dynamic>> toggleQueueStatus(String locationId, ToggleQueueRequest request) async {
    try {
      final response = await _apiClient.put(
        '${ApiConfig.locationsEndpoint}/$locationId/queue-status',
        data: request.toJson(),
      );
      return response.data;
    } catch (e) {
      rethrow;
    }
  }
}