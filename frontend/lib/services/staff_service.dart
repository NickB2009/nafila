import '../models/staff_models.dart';
import '../config/api_config.dart';
import 'api_client.dart';

/// Service for handling staff-related operations
class StaffService {
  final ApiClient _apiClient;

  const StaffService({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  /// Creates StaffService with default dependencies
  static Future<StaffService> create() async {
    final apiClient = ApiClient();
    return StaffService(apiClient: apiClient);
  }

  /// Adds a new barber to the system
  Future<AddBarberResult> addBarber(AddBarberRequest request) async {
    try {
      final response = await _apiClient.post(
        '${ApiConfig.staffEndpoint}/barbers',
        data: request.toJson(),
      );
      return AddBarberResult.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }

  /// Edits an existing barber's details
  Future<EditBarberResult> editBarber(String staffMemberId, EditBarberRequest request) async {
    try {
      final response = await _apiClient.put(
        '${ApiConfig.staffEndpoint}/barbers/$staffMemberId',
        data: request.toJson(),
      );
      return EditBarberResult.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }

  /// Updates a staff member's status
  Future<UpdateStaffStatusResult> updateStaffStatus(String staffMemberId, UpdateStaffStatusRequest request) async {
    try {
      final response = await _apiClient.put(
        '${ApiConfig.staffEndpoint}/barbers/$staffMemberId/status',
        data: request.toJson(),
      );
      return UpdateStaffStatusResult.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }

  /// Starts a break for a staff member
  Future<StartBreakResult> startBreak(String staffMemberId, StartBreakRequest request) async {
    try {
      final response = await _apiClient.put(
        '${ApiConfig.staffEndpoint}/barbers/$staffMemberId/start-break',
        data: request.toJson(),
      );
      return StartBreakResult.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }

  /// Ends a break for a staff member
  Future<EndBreakResult> endBreak(String staffMemberId, EndBreakRequest request) async {
    try {
      final response = await _apiClient.put(
        '${ApiConfig.staffEndpoint}/barbers/$staffMemberId/end-break',
        data: request.toJson(),
      );
      return EndBreakResult.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }
}