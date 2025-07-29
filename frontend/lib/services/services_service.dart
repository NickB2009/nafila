import '../models/services_models.dart';
import '../config/api_config.dart';
import 'api_client.dart';

/// Service for handling services-related operations
class ServicesService {
  final ApiClient _apiClient;

  const ServicesService({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  /// Creates ServicesService with default dependencies
  static Future<ServicesService> create() async {
    final apiClient = ApiClient();
    return ServicesService(apiClient: apiClient);
  }

  /// Adds a new service to the system
  Future<Map<String, dynamic>> addService(AddServiceOfferedRequest request) async {
    try {
      final response = await _apiClient.post(
        ApiConfig.servicesOfferedEndpoint,
        data: request.toJson(),
      );
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Gets all services
  Future<List<dynamic>> getServices() async {
    try {
      final response = await _apiClient.get(ApiConfig.servicesOfferedEndpoint);
      return response.data as List<dynamic>;
    } catch (e) {
      rethrow;
    }
  }

  /// Gets service details by ID
  Future<Map<String, dynamic>> getService(String serviceId) async {
    try {
      final response = await _apiClient.get('${ApiConfig.servicesOfferedEndpoint}/$serviceId');
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Updates an existing service
  Future<Map<String, dynamic>> updateService(String serviceId, UpdateServiceOfferedRequest request) async {
    try {
      final response = await _apiClient.put(
        '${ApiConfig.servicesOfferedEndpoint}/$serviceId',
        data: request.toJson(),
      );
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Deletes a service
  Future<Map<String, dynamic>> deleteService(String serviceId) async {
    try {
      final response = await _apiClient.delete('${ApiConfig.servicesOfferedEndpoint}/$serviceId');
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Activates a service
  Future<Map<String, dynamic>> activateService(String serviceId) async {
    try {
      final response = await _apiClient.put('${ApiConfig.servicesOfferedEndpoint}/$serviceId/activate');
      return response.data;
    } catch (e) {
      rethrow;
    }
  }
}