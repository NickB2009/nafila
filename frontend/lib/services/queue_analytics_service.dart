import 'package:dio/dio.dart';
import '../config/api_config.dart';
import '../models/analytics_models.dart';
import 'api_client.dart';

/// Service for handling queue analytics and metrics operations
class QueueAnalyticsService {
  final ApiClient _apiClient;

  const QueueAnalyticsService({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  /// Creates QueueAnalyticsService with default dependencies
  static Future<QueueAnalyticsService> create() async {
    final apiClient = ApiClient();
    return QueueAnalyticsService(apiClient: apiClient);
  }

  /// Calculate estimated wait time for a new queue entry
  Future<WaitTimeEstimate> getEstimatedWaitTime(
    String salonId, {
    String serviceType = 'haircut',
  }) async {
    try {
      final response = await _apiClient.get(
        '${ApiConfig.queueAnalyticsEndpoint}/wait-time/$salonId',
        queryParameters: {'serviceType': serviceType},
      );
      
      return WaitTimeEstimate.fromJson(response.data);
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        throw Exception('Salon not found');
      } else if (e.response?.statusCode == 400) {
        throw Exception('Invalid salon ID');
      }
      throw Exception('Failed to get estimated wait time: ${e.message}');
    } catch (e) {
      throw Exception('Failed to get estimated wait time: $e');
    }
  }

  /// Get queue performance metrics for a salon
  Future<QueuePerformanceMetrics> getQueuePerformance(
    String salonId, {
    DateTime? fromDate,
    DateTime? toDate,
  }) async {
    try {
      final queryParams = <String, dynamic>{};
      if (fromDate != null) {
        queryParams['fromDate'] = fromDate.toIso8601String();
      }
      if (toDate != null) {
        queryParams['toDate'] = toDate.toIso8601String();
      }

      final response = await _apiClient.get(
        '${ApiConfig.queueAnalyticsEndpoint}/performance/$salonId',
        queryParameters: queryParams,
      );
      
      return QueuePerformanceMetrics.fromJson(response.data);
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        throw Exception('Salon not found');
      } else if (e.response?.statusCode == 400) {
        throw Exception('Invalid salon ID or date range');
      }
      throw Exception('Failed to get performance metrics: ${e.message}');
    } catch (e) {
      throw Exception('Failed to get performance metrics: $e');
    }
  }

  /// Get wait time trends for a salon
  Future<List<WaitTimeTrend>> getWaitTimeTrends(
    String salonId, {
    int days = 7,
  }) async {
    try {
      if (days < 1 || days > 30) {
        throw Exception('Days must be between 1 and 30');
      }

      final response = await _apiClient.get(
        '${ApiConfig.queueAnalyticsEndpoint}/trends/$salonId',
        queryParameters: {'days': days},
      );
      
      final List<dynamic> trendsData = response.data;
      return trendsData.map((trend) => WaitTimeTrend.fromJson(trend)).toList();
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        throw Exception('Salon not found');
      } else if (e.response?.statusCode == 400) {
        throw Exception('Invalid salon ID or days parameter');
      }
      throw Exception('Failed to get wait time trends: ${e.message}');
    } catch (e) {
      throw Exception('Failed to get wait time trends: $e');
    }
  }

  /// Record customer satisfaction feedback
  Future<CustomerSatisfactionResponse> recordCustomerSatisfaction(
    String queueEntryId,
    int rating, {
    String? feedback,
  }) async {
    try {
      if (rating < 1 || rating > 5) {
        throw Exception('Rating must be between 1 and 5');
      }

      final requestData = {
        'queueEntryId': queueEntryId,
        'rating': rating,
        if (feedback != null && feedback.isNotEmpty) 'feedback': feedback,
      };

      final response = await _apiClient.post(
        '${ApiConfig.queueAnalyticsEndpoint}/satisfaction',
        data: requestData,
      );
      
      return CustomerSatisfactionResponse.fromJson(response.data);
    } on DioException catch (e) {
      if (e.response?.statusCode == 400) {
        throw Exception('Invalid rating or queue entry ID');
      }
      throw Exception('Failed to record satisfaction: ${e.message}');
    } catch (e) {
      throw Exception('Failed to record satisfaction: $e');
    }
  }

  /// Get queue recommendations for a salon
  Future<QueueRecommendations> getQueueRecommendations(String salonId) async {
    try {
      final response = await _apiClient.get(
        '${ApiConfig.queueAnalyticsEndpoint}/recommendations/$salonId',
      );
      
      return QueueRecommendations.fromJson(response.data);
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        throw Exception('Salon not found');
      } else if (e.response?.statusCode == 400) {
        throw Exception('Invalid salon ID');
      }
      throw Exception('Failed to get recommendations: ${e.message}');
    } catch (e) {
      throw Exception('Failed to get recommendations: $e');
    }
  }

  /// Get queue health status for a salon
  Future<QueueHealthStatus> getQueueHealth(String salonId) async {
    try {
      final response = await _apiClient.get(
        '${ApiConfig.queueAnalyticsEndpoint}/health/$salonId',
      );
      
      return QueueHealthStatus.fromJson(response.data);
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        throw Exception('Salon not found');
      } else if (e.response?.statusCode == 400) {
        throw Exception('Invalid salon ID');
      }
      throw Exception('Failed to get health status: ${e.message}');
    } catch (e) {
      throw Exception('Failed to get health status: $e');
    }
  }

  /// Get comprehensive analytics data for a salon (combines multiple metrics)
  Future<SalonAnalyticsSummary> getSalonAnalyticsSummary(String salonId) async {
    try {
      // Fetch multiple analytics in parallel for better performance
      final futures = await Future.wait([
        getQueuePerformance(salonId),
        getQueueHealth(salonId),
        getQueueRecommendations(salonId),
        getWaitTimeTrends(salonId, days: 7),
      ]);

      final performance = futures[0] as QueuePerformanceMetrics;
      final health = futures[1] as QueueHealthStatus;
      final recommendations = futures[2] as QueueRecommendations;
      final trends = futures[3] as List<WaitTimeTrend>;

      return SalonAnalyticsSummary(
        salonId: salonId,
        performance: performance,
        health: health,
        recommendations: recommendations,
        trends: trends,
        lastUpdated: DateTime.now(),
      );
    } catch (e) {
      throw Exception('Failed to get salon analytics summary: $e');
    }
  }
}
