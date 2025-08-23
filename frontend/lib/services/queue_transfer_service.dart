import 'package:dio/dio.dart';
import '../config/api_config.dart';
import '../models/queue_transfer_models.dart';

/// Service for handling queue transfer operations
class QueueTransferService {
  final Dio _dio;

  QueueTransferService({Dio? dio}) : _dio = dio ?? Dio();

  /// Creates QueueTransferService with default dependencies
  static Future<QueueTransferService> create() async {
    return QueueTransferService();
  }

  /// Transfer a queue entry to another salon, service, or time slot
  Future<QueueTransferResponse> transferQueueEntry(QueueTransferRequest request) async {
    try {
      final response = await _dio.post(
        ApiConfig.getUrl('${ApiConfig.queueTransferEndpoint}/transfer'),
        data: request.toJson(),
        options: Options(headers: ApiConfig.defaultHeaders),
      );
      
      return QueueTransferResponse.fromJson(response.data);
    } on DioException catch (e) {
      if (e.response?.statusCode == 400) {
        final errorData = e.response?.data;
        if (errorData is Map<String, dynamic>) {
          return QueueTransferResponse.fromJson(errorData);
        }
      }
      throw Exception('Failed to transfer queue entry: ${e.message}');
    } catch (e) {
      throw Exception('Failed to transfer queue entry: $e');
    }
  }

  /// Get transfer suggestions for a queue entry
  Future<TransferSuggestionsResponse> getTransferSuggestions(TransferSuggestionsRequest request) async {
    try {
      final response = await _dio.post(
        ApiConfig.getUrl('${ApiConfig.queueTransferEndpoint}/suggestions'),
        data: request.toJson(),
        options: Options(headers: ApiConfig.defaultHeaders),
      );
      
      return TransferSuggestionsResponse.fromJson(response.data);
    } on DioException catch (e) {
      if (e.response?.statusCode == 400) {
        final errorData = e.response?.data;
        if (errorData is Map<String, dynamic>) {
          return TransferSuggestionsResponse.fromJson(errorData);
        }
      }
      throw Exception('Failed to get transfer suggestions: ${e.message}');
    } catch (e) {
      throw Exception('Failed to get transfer suggestions: $e');
    }
  }

  /// Check if a queue entry is eligible for transfer
  Future<TransferEligibilityResponse> checkTransferEligibility(TransferEligibilityRequest request) async {
    try {
      final response = await _dio.post(
        ApiConfig.getUrl('${ApiConfig.queueTransferEndpoint}/eligibility'),
        data: request.toJson(),
        options: Options(headers: ApiConfig.defaultHeaders),
      );
      
      return TransferEligibilityResponse.fromJson(response.data);
    } catch (e) {
      throw Exception('Failed to check transfer eligibility: $e');
    }
  }

  /// Get transfer history for a customer
  Future<List<QueueTransferAnalytics>> getCustomerTransferHistory(String customerId) async {
    try {
      final response = await _dio.get(
        ApiConfig.getUrl('${ApiConfig.queueTransferEndpoint}/history/$customerId'),
        options: Options(headers: ApiConfig.defaultHeaders),
      );
      
      return (response.data as List)
          .map((item) => QueueTransferAnalytics.fromJson(item))
          .toList();
    } catch (e) {
      throw Exception('Failed to get transfer history: $e');
    }
  }

  /// Cancel a pending transfer
  Future<bool> cancelTransfer(String transferId) async {
    try {
      final response = await _dio.post(
        ApiConfig.getUrl('${ApiConfig.queueTransferEndpoint}/cancel/$transferId'),
        options: Options(headers: ApiConfig.defaultHeaders),
      );
      
      return response.data['success'] ?? false;
    } catch (e) {
      throw Exception('Failed to cancel transfer: $e');
    }
  }

  /// Get nearby salons for transfer suggestions
  Future<List<Map<String, dynamic>>> getNearbySalonsForTransfer(
    String salonId, {
    double maxDistanceKm = 5.0,
    int maxResults = 10,
  }) async {
    try {
      final response = await _dio.get(
        ApiConfig.getUrl('${ApiConfig.queueTransferEndpoint}/nearby-salons/$salonId'),
        queryParameters: {
          'maxDistanceKm': maxDistanceKm,
          'maxResults': maxResults,
        },
        options: Options(headers: ApiConfig.defaultHeaders),
      );
      
      return List<Map<String, dynamic>>.from(response.data['salons'] ?? []);
    } catch (e) {
      throw Exception('Failed to get nearby salons: $e');
    }
  }

  /// Quick transfer to a specific salon (simplified API)
  Future<QueueTransferResponse> quickTransferToSalon({
    required String queueEntryId,
    required String targetSalonId,
    String? reason,
  }) async {
    final request = QueueTransferRequest(
      queueEntryId: queueEntryId,
      transferType: 'salon',
      targetSalonId: targetSalonId,
      reason: reason,
      maintainPosition: false,
    );
    
    return transferQueueEntry(request);
  }

  /// Quick transfer to a different service (simplified API)
  Future<QueueTransferResponse> quickTransferToService({
    required String queueEntryId,
    required String targetServiceType,
    String? reason,
  }) async {
    final request = QueueTransferRequest(
      queueEntryId: queueEntryId,
      transferType: 'service',
      targetServiceType: targetServiceType,
      reason: reason,
      maintainPosition: true, // Usually maintain position for service changes
    );
    
    return transferQueueEntry(request);
  }

  /// Get smart transfer suggestions with filtering
  Future<TransferSuggestionsResponse> getSmartSuggestions({
    required String queueEntryId,
    double? maxDistanceKm = 5.0,
    int? maxWaitTimeMinutes,
    List<String>? preferredServices,
    bool prioritizeShorterWait = true,
    bool prioritizeCloserDistance = false,
  }) async {
    final request = TransferSuggestionsRequest(
      queueEntryId: queueEntryId,
      maxDistanceKm: maxDistanceKm,
      maxWaitTimeMinutes: maxWaitTimeMinutes,
      preferredServiceTypes: preferredServices,
      includeSalonTransfers: true,
      includeServiceTransfers: preferredServices != null,
      includeTimeTransfers: false, // Future feature
    );
    
    final response = await getTransferSuggestions(request);
    
    if (response.success && response.hasSuggestions) {
      // Apply client-side filtering and sorting
      var suggestions = response.validSuggestions;
      
      if (prioritizeShorterWait) {
        suggestions.sort((a, b) => a.estimatedWaitMinutes.compareTo(b.estimatedWaitMinutes));
      } else if (prioritizeCloserDistance) {
        suggestions.sort((a, b) => a.distanceKm.compareTo(b.distanceKm));
      }
      
      return TransferSuggestionsResponse(
        success: true,
        suggestions: suggestions,
        currentSalonId: response.currentSalonId,
        currentSalonName: response.currentSalonName,
        currentPosition: response.currentPosition,
        currentEstimatedWaitMinutes: response.currentEstimatedWaitMinutes,
        generatedAt: response.generatedAt,
        errors: response.errors,
      );
    }
    
    return response;
  }

  /// Check if transfer would be beneficial
  Future<bool> isTransferBeneficial({
    required String queueEntryId,
    required String targetSalonId,
    int minTimeSavings = 10, // Minimum minutes to save
    int minPositionImprovement = 2, // Minimum positions to improve
  }) async {
    try {
      final eligibility = await checkTransferEligibility(
        TransferEligibilityRequest(
          queueEntryId: queueEntryId,
          transferType: 'salon',
          targetSalonId: targetSalonId,
        ),
      );
      
      if (!eligibility.isEligible) return false;
      
      // Get current suggestions to compare
      final suggestions = await getTransferSuggestions(
        TransferSuggestionsRequest(queueEntryId: queueEntryId),
      );
      
      if (!suggestions.success) return false;
      
      // Find suggestion for this specific salon
      final targetSuggestion = suggestions.suggestions.firstWhere(
        (s) => s.targetSalonId == targetSalonId,
        orElse: () => throw Exception('Target salon not found in suggestions'),
      );
      
      return targetSuggestion.timeImprovement >= minTimeSavings ||
             targetSuggestion.positionImprovement >= minPositionImprovement;
    } catch (e) {
      return false;
    }
  }

  /// Get transfer statistics for analytics
  Future<Map<String, dynamic>> getTransferStats() async {
    // This would typically be called with user authentication
    // For now, return mock data
    return {
      'totalTransfers': 0,
      'successfulTransfers': 0,
      'averageTimeSaved': 0,
      'averagePositionsImproved': 0,
      'mostPopularTargetSalons': <String>[],
    };
  }
}
