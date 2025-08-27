/// Data models for queue analytics and metrics
library;

/// Wait time estimate for a queue entry
class WaitTimeEstimate {
  final String salonId;
  final String serviceType;
  final Duration estimatedWaitTime;
  final DateTime calculatedAt;
  final String confidence;

  const WaitTimeEstimate({
    required this.salonId,
    required this.serviceType,
    required this.estimatedWaitTime,
    required this.calculatedAt,
    required this.confidence,
  });

  factory WaitTimeEstimate.fromJson(Map<String, dynamic> json) {
    return WaitTimeEstimate(
      salonId: json['salonId'] ?? '',
      serviceType: json['serviceType'] ?? '',
      estimatedWaitTime: _parseDuration(json['estimatedWaitTime']),
      calculatedAt: DateTime.parse(json['calculatedAt'] ?? DateTime.now().toIso8601String()),
      confidence: json['confidence'] ?? 'Medium',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'salonId': salonId,
      'serviceType': serviceType,
      'estimatedWaitTime': estimatedWaitTime.toString(),
      'calculatedAt': calculatedAt.toIso8601String(),
      'confidence': confidence,
    };
  }

  /// Get wait time in minutes for display
  int get waitTimeMinutes => estimatedWaitTime.inMinutes;

  /// Get formatted wait time string
  String get formattedWaitTime {
    if (estimatedWaitTime.inMinutes < 60) {
      return '${estimatedWaitTime.inMinutes} min';
    } else {
      final hours = estimatedWaitTime.inHours;
      final minutes = estimatedWaitTime.inMinutes % 60;
      return '${hours}h ${minutes}min';
    }
  }
}

/// Queue performance metrics for a salon
class QueuePerformanceMetrics {
  final String salonId;
  final DateTime periodStart;
  final DateTime periodEnd;
  final int totalCustomers;
  final Duration averageWaitTime;
  final Duration averageServiceTime;
  final Duration? peakHour;
  final double customerSatisfaction;
  final double queueEfficiency;
  final Duration peakWaitTime;
  final int peakHourCustomers;
  final int completedServices;
  final int cancelledServices;

  const QueuePerformanceMetrics({
    required this.salonId,
    required this.periodStart,
    required this.periodEnd,
    required this.totalCustomers,
    required this.averageWaitTime,
    required this.averageServiceTime,
    this.peakHour,
    required this.customerSatisfaction,
    required this.queueEfficiency,
    required this.peakWaitTime,
    required this.peakHourCustomers,
    required this.completedServices,
    required this.cancelledServices,
  });

  factory QueuePerformanceMetrics.fromJson(Map<String, dynamic> json) {
    return QueuePerformanceMetrics(
      salonId: json['salonId'] ?? '',
      periodStart: DateTime.parse(json['periodStart'] ?? DateTime.now().toIso8601String()),
      periodEnd: DateTime.parse(json['periodEnd'] ?? DateTime.now().toIso8601String()),
      totalCustomers: json['totalCustomers'] ?? 0,
      averageWaitTime: _parseDuration(json['averageWaitTime']),
      averageServiceTime: _parseDuration(json['averageServiceTime']),
      peakHour: json['peakHour'] != null ? _parseDuration(json['peakHour']) : null,
      customerSatisfaction: (json['customerSatisfaction'] ?? 0.0).toDouble(),
      queueEfficiency: (json['queueEfficiency'] ?? 0.0).toDouble(),
      peakWaitTime: _parseDuration(json['peakWaitTime']),
      peakHourCustomers: json['peakHourCustomers'] ?? 0,
      completedServices: json['completedServices'] ?? 0,
      cancelledServices: json['cancelledServices'] ?? 0,
    );
  }

  /// Get cancellation rate as percentage
  double get cancellationRate {
    if (totalCustomers == 0) return 0.0;
    return (cancelledServices / totalCustomers) * 100;
  }

  /// Get completion rate as percentage
  double get completionRate {
    if (totalCustomers == 0) return 0.0;
    return (completedServices / totalCustomers) * 100;
  }

  /// Get efficiency percentage for display
  double get efficiencyPercentage => queueEfficiency * 100;

  /// Get formatted peak hour string
  String? get formattedPeakHour {
    if (peakHour == null) return null;
    final hour = peakHour!.inHours;
    return '${hour.toString().padLeft(2, '0')}:00';
  }
}

/// Wait time trend data for a specific date
class WaitTimeTrend {
  final DateTime date;
  final Duration averageWaitTime;
  final Duration peakWaitTime;
  final int totalCustomers;
  final Duration? busiestHour;
  final int busiestHourCustomers;

  const WaitTimeTrend({
    required this.date,
    required this.averageWaitTime,
    required this.peakWaitTime,
    required this.totalCustomers,
    this.busiestHour,
    required this.busiestHourCustomers,
  });

  factory WaitTimeTrend.fromJson(Map<String, dynamic> json) {
    return WaitTimeTrend(
      date: DateTime.parse(json['date'] ?? DateTime.now().toIso8601String()),
      averageWaitTime: _parseDuration(json['averageWaitTime']),
      peakWaitTime: _parseDuration(json['peakWaitTime']),
      totalCustomers: json['totalCustomers'] ?? 0,
      busiestHour: json['busiestHour'] != null ? _parseDuration(json['busiestHour']) : null,
      busiestHourCustomers: json['busiestHourCustomers'] ?? 0,
    );
  }

  /// Get formatted date string
  String get formattedDate {
    return '${date.day}/${date.month}';
  }

  /// Get average wait time in minutes
  int get averageWaitMinutes => averageWaitTime.inMinutes;

  /// Get peak wait time in minutes
  int get peakWaitMinutes => peakWaitTime.inMinutes;
}

/// Queue recommendations for a salon
class QueueRecommendations {
  final String salonId;
  final DateTime generatedAt;
  final List<String> recommendations;
  final String priority;
  final double confidenceScore;

  const QueueRecommendations({
    required this.salonId,
    required this.generatedAt,
    required this.recommendations,
    required this.priority,
    required this.confidenceScore,
  });

  factory QueueRecommendations.fromJson(Map<String, dynamic> json) {
    final recommendationsList = json['recommendations'] as List<dynamic>? ?? [];
    
    return QueueRecommendations(
      salonId: json['salonId'] ?? '',
      generatedAt: DateTime.parse(json['generatedAt'] ?? DateTime.now().toIso8601String()),
      recommendations: recommendationsList.map((r) => r.toString()).toList(),
      priority: json['priority'] ?? 'Medium',
      confidenceScore: (json['confidenceScore'] ?? 0.8).toDouble(),
    );
  }

  /// Check if there are any recommendations
  bool get hasRecommendations => recommendations.isNotEmpty;

  /// Get priority color for UI
  String get priorityColor {
    switch (priority.toLowerCase()) {
      case 'high':
        return '#F44336'; // Red
      case 'medium':
        return '#FF9800'; // Orange
      case 'low':
        return '#4CAF50'; // Green
      default:
        return '#2196F3'; // Blue
    }
  }

  /// Get confidence percentage for display
  double get confidencePercentage => confidenceScore * 100;
}

/// Customer satisfaction response
class CustomerSatisfactionResponse {
  final bool success;
  final String message;
  final DateTime recordedAt;

  const CustomerSatisfactionResponse({
    required this.success,
    required this.message,
    required this.recordedAt,
  });

  factory CustomerSatisfactionResponse.fromJson(Map<String, dynamic> json) {
    return CustomerSatisfactionResponse(
      success: json['success'] ?? false,
      message: json['message'] ?? '',
      recordedAt: DateTime.parse(json['recordedAt'] ?? DateTime.now().toIso8601String()),
    );
  }
}

/// Queue health status for analytics
class QueueHealthStatus {
  final String salonId;
  final String status;
  final DateTime lastUpdated;
  final int currentQueueLength;
  final Duration currentAverageWaitTime;
  final List<String> issues;
  final List<String> recommendations;
  final double healthScore;

  const QueueHealthStatus({
    required this.salonId,
    required this.status,
    required this.lastUpdated,
    required this.currentQueueLength,
    required this.currentAverageWaitTime,
    required this.issues,
    required this.recommendations,
    required this.healthScore,
  });

  factory QueueHealthStatus.fromJson(Map<String, dynamic> json) {
    final issuesList = json['issues'] as List<dynamic>? ?? [];
    final recommendationsList = json['recommendations'] as List<dynamic>? ?? [];
    
    return QueueHealthStatus(
      salonId: json['salonId'] ?? '',
      status: json['status'] ?? 'Unknown',
      lastUpdated: DateTime.parse(json['lastUpdated'] ?? DateTime.now().toIso8601String()),
      currentQueueLength: json['currentQueueLength'] ?? 0,
      currentAverageWaitTime: _parseDuration(json['currentAverageWaitTime']),
      issues: issuesList.map((i) => i.toString()).toList(),
      recommendations: recommendationsList.map((r) => r.toString()).toList(),
      healthScore: (json['healthScore'] ?? 0.0).toDouble(),
    );
  }

  /// Check if the queue is healthy
  bool get isHealthy => status.toLowerCase() == 'healthy';

  /// Check if there are any issues
  bool get hasIssues => issues.isNotEmpty;

  /// Get status color for UI
  String get statusColor {
    switch (status.toLowerCase()) {
      case 'healthy':
        return '#4CAF50'; // Green
      case 'warning':
        return '#FF9800'; // Orange
      case 'critical':
        return '#F44336'; // Red
      default:
        return '#9E9E9E'; // Grey
    }
  }

  /// Get current wait time in minutes
  int get currentWaitMinutes => currentAverageWaitTime.inMinutes;
}

/// Comprehensive salon analytics summary
class SalonAnalyticsSummary {
  final String salonId;
  final QueuePerformanceMetrics performance;
  final QueueHealthStatus health;
  final QueueRecommendations recommendations;
  final List<WaitTimeTrend> trends;
  final DateTime lastUpdated;

  const SalonAnalyticsSummary({
    required this.salonId,
    required this.performance,
    required this.health,
    required this.recommendations,
    required this.trends,
    required this.lastUpdated,
  });

  /// Get overall salon score based on multiple metrics
  double get overallScore {
    final healthWeight = 0.4;
    final efficiencyWeight = 0.3;
    final satisfactionWeight = 0.3;

    final healthScore = health.healthScore / 100;
    final efficiencyScore = performance.queueEfficiency;
    final satisfactionScore = performance.customerSatisfaction / 5.0;

    return (healthScore * healthWeight + 
            efficiencyScore * efficiencyWeight + 
            satisfactionScore * satisfactionWeight) * 100;
  }

  /// Get trend direction (improving, declining, stable)
  String get trendDirection {
    if (trends.length < 2) return 'stable';
    
    final recent = trends.take(3).map((t) => t.averageWaitMinutes).toList();
    final older = trends.skip(3).take(3).map((t) => t.averageWaitMinutes).toList();
    
    if (recent.isEmpty || older.isEmpty) return 'stable';
    
    final recentAvg = recent.reduce((a, b) => a + b) / recent.length;
    final olderAvg = older.reduce((a, b) => a + b) / older.length;
    
    if (recentAvg < olderAvg * 0.9) return 'improving';
    if (recentAvg > olderAvg * 1.1) return 'declining';
    return 'stable';
  }
}

/// Helper function to parse duration from various formats
Duration _parseDuration(dynamic value) {
  if (value == null) return Duration.zero;
  
  if (value is String) {
    // Try to parse ISO 8601 duration format (PT15M30S)
    if (value.startsWith('PT')) {
      final regex = RegExp(r'PT(?:(\d+)H)?(?:(\d+)M)?(?:(\d+(?:\.\d+)?)S)?');
      final match = regex.firstMatch(value);
      if (match != null) {
        final hours = int.tryParse(match.group(1) ?? '0') ?? 0;
        final minutes = int.tryParse(match.group(2) ?? '0') ?? 0;
        final seconds = double.tryParse(match.group(3) ?? '0') ?? 0;
        return Duration(
          hours: hours,
          minutes: minutes,
          seconds: seconds.round(),
        );
      }
    }
    
    // Try to parse simple formats like "00:15:30"
    final parts = value.split(':');
    if (parts.length == 3) {
      final hours = int.tryParse(parts[0]) ?? 0;
      final minutes = int.tryParse(parts[1]) ?? 0;
      final seconds = int.tryParse(parts[2]) ?? 0;
      return Duration(hours: hours, minutes: minutes, seconds: seconds);
    }
    
    // Try to parse as minutes
    final minutes = int.tryParse(value);
    if (minutes != null) {
      return Duration(minutes: minutes);
    }
  }
  
  if (value is int) {
    return Duration(minutes: value);
  }
  
  if (value is double) {
    return Duration(minutes: value.round());
  }
  
  return Duration.zero;
}
