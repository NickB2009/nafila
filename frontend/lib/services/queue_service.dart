import '../models/queue_models.dart';
import '../config/api_config.dart';
import 'api_client.dart';

/// Service for handling queue-related operations
class QueueService {
  final ApiClient _apiClient;

  const QueueService({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  /// Creates QueueService with default dependencies
  static Future<QueueService> create() async {
    final apiClient = ApiClient();
    return QueueService(apiClient: apiClient);
  }

  /// Adds a new queue to the system
  Future<Map<String, dynamic>> addQueue(AddQueueRequest request) async {
    try {
      final response = await _apiClient.post(
        ApiConfig.queuesEndpoint,
        data: request.toJson(),
      );
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Gets queue details by ID
  Future<Map<String, dynamic>> getQueue(String queueId) async {
    try {
      final response = await _apiClient.get('${ApiConfig.queuesEndpoint}/$queueId');
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Joins a customer to a queue
  Future<Map<String, dynamic>> joinQueue(String queueId, JoinQueueRequest request) async {
    try {
      final response = await _apiClient.post(
        '${ApiConfig.queuesEndpoint}/$queueId/join',
        data: request.toJson(),
      );
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Adds a customer to queue (barber-initiated)
  Future<Map<String, dynamic>> barberAddToQueue(String queueId, BarberAddRequest request) async {
    try {
      final response = await _apiClient.post(
        '${ApiConfig.queuesEndpoint}/$queueId/barber-add',
        data: request.toJson(),
      );
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Calls the next customer in queue
  Future<Map<String, dynamic>> callNext(String queueId, CallNextRequest request) async {
    try {
      final response = await _apiClient.post(
        '${ApiConfig.queuesEndpoint}/$queueId/call-next',
        data: request.toJson(),
      );
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Checks in a customer
  Future<Map<String, dynamic>> checkIn(String queueId, CheckInRequest request) async {
    try {
      final response = await _apiClient.post(
        '${ApiConfig.queuesEndpoint}/$queueId/check-in',
        data: request.toJson(),
      );
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Finishes a service for a customer
  Future<Map<String, dynamic>> finishService(String queueId, FinishRequest request) async {
    try {
      final response = await _apiClient.post(
        '${ApiConfig.queuesEndpoint}/$queueId/finish',
        data: request.toJson(),
      );
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Cancels a queue entry
  Future<Map<String, dynamic>> cancelQueue(String queueId, CancelQueueRequest request) async {
    try {
      final response = await _apiClient.post(
        '${ApiConfig.queuesEndpoint}/$queueId/cancel',
        data: request.toJson(),
      );
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Gets estimated wait time for a queue entry
  Future<Map<String, dynamic>> getWaitTime(String queueId, String entryId) async {
    try {
      final response = await _apiClient.get(
        '${ApiConfig.queuesEndpoint}/$queueId/entries/$entryId/wait-time',
      );
      return response.data;
    } catch (e) {
      rethrow;
    }
  }

  /// Saves haircut details for a queue entry
  Future<SaveHaircutDetailsResult> saveHaircutDetails(String entryId, SaveHaircutDetailsRequest request) async {
    try {
      final response = await _apiClient.post(
        '${ApiConfig.queuesEndpoint}/entries/$entryId/haircut-details',
        data: request.toJson(),
      );
      return SaveHaircutDetailsResult.fromJson(response.data);
    } catch (e) {
      rethrow;
    }
  }
}