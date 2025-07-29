import '../models/organization_models.dart';
import '../config/api_config.dart';
import 'api_client.dart';

/// Service for handling organization-related operations
class OrganizationService {
  final ApiClient _apiClient;

  const OrganizationService({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  /// Creates OrganizationService with default dependencies
  static Future<OrganizationService> create() async {
    final apiClient = ApiClient();
    return OrganizationService(apiClient: apiClient);
  }

  /// Creates a new organization
  Future<CreateOrganizationResult> createOrganization(CreateOrganizationRequest request) async {
    try {
      final response = await _apiClient.post(
        ApiConfig.organizationsEndpoint,
        data: request.toJson(),
      );
      return CreateOrganizationResult.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }

  /// Gets all organizations
  Future<List<dynamic>> getOrganizations() async {
    try {
      final response = await _apiClient.get(ApiConfig.organizationsEndpoint);
      return response.data as List<dynamic>;
    } catch (e) {
      rethrow;
    }
  }

  /// Gets organization details by ID
  Future<Map<String, dynamic>> getOrganization(String organizationId) async {
    try {
      final response = await _apiClient.get('${ApiConfig.organizationsEndpoint}/$organizationId');
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Updates an existing organization
  Future<OrganizationOperationResult> updateOrganization(String organizationId, UpdateOrganizationRequest request) async {
    try {
      final response = await _apiClient.put(
        '${ApiConfig.organizationsEndpoint}/$organizationId',
        data: request.toJson(),
      );
      return OrganizationOperationResult.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }

  /// Updates organization branding
  Future<OrganizationOperationResult> updateBranding(String organizationId, UpdateBrandingRequest request) async {
    try {
      final response = await _apiClient.put(
        '${ApiConfig.organizationsEndpoint}/$organizationId/branding',
        data: request.toJson(),
      );
      return OrganizationOperationResult.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }

  /// Updates organization subscription plan
  Future<OrganizationOperationResult> updateSubscriptionPlan(String organizationId, String subscriptionPlanId) async {
    try {
      final response = await _apiClient.put(
        '${ApiConfig.organizationsEndpoint}/$organizationId/subscription/$subscriptionPlanId',
      );
      return OrganizationOperationResult.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }

  /// Updates analytics sharing setting
  Future<OrganizationOperationResult> updateAnalyticsSharing(String organizationId, bool sharesDataForAnalytics) async {
    try {
      final response = await _apiClient.put(
        '${ApiConfig.organizationsEndpoint}/$organizationId/analytics-sharing',
        data: sharesDataForAnalytics,
      );
      return OrganizationOperationResult.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }

  /// Activates an organization
  Future<OrganizationOperationResult> activateOrganization(String organizationId) async {
    try {
      final response = await _apiClient.put(
        '${ApiConfig.organizationsEndpoint}/$organizationId/activate',
      );
      return OrganizationOperationResult.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }

  /// Deactivates an organization
  Future<OrganizationOperationResult> deactivateOrganization(String organizationId) async {
    try {
      final response = await _apiClient.put(
        '${ApiConfig.organizationsEndpoint}/$organizationId/deactivate',
      );
      return OrganizationOperationResult.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }

  /// Gets organization by slug
  Future<Map<String, dynamic>> getOrganizationBySlug(String slug) async {
    try {
      final response = await _apiClient.get('${ApiConfig.organizationsEndpoint}/by-slug/$slug');
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Gets live activity for an organization
  Future<TrackLiveActivityResult> getLiveActivity(String organizationId) async {
    try {
      final response = await _apiClient.get(
        '${ApiConfig.organizationsEndpoint}/$organizationId/live-activity',
      );
      return TrackLiveActivityResult.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }
}