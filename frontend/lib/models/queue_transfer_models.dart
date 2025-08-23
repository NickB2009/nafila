/// Data models for queue transfer functionality

/// Request to transfer a queue entry to another salon, service, or time slot
class QueueTransferRequest {
  final String queueEntryId;
  final String transferType; // "salon", "service", "time"
  final String? targetSalonId;
  final String? targetServiceType;
  final DateTime? preferredTime;
  final String? reason;
  final bool maintainPosition;

  const QueueTransferRequest({
    required this.queueEntryId,
    required this.transferType,
    this.targetSalonId,
    this.targetServiceType,
    this.preferredTime,
    this.reason,
    this.maintainPosition = false,
  });

  Map<String, dynamic> toJson() {
    return {
      'queueEntryId': queueEntryId,
      'transferType': transferType,
      if (targetSalonId != null) 'targetSalonId': targetSalonId,
      if (targetServiceType != null) 'targetServiceType': targetServiceType,
      if (preferredTime != null) 'preferredTime': preferredTime!.toIso8601String(),
      if (reason != null) 'reason': reason,
      'maintainPosition': maintainPosition,
    };
  }
}

/// Response for queue transfer operation
class QueueTransferResponse {
  final bool success;
  final String? newQueueEntryId;
  final String? newSalonId;
  final String? newSalonName;
  final int newPosition;
  final int estimatedWaitMinutes;
  final DateTime transferredAt;
  final String? transferType;
  final List<String> errors;
  final List<String> warnings;

  const QueueTransferResponse({
    required this.success,
    this.newQueueEntryId,
    this.newSalonId,
    this.newSalonName,
    required this.newPosition,
    required this.estimatedWaitMinutes,
    required this.transferredAt,
    this.transferType,
    this.errors = const [],
    this.warnings = const [],
  });

  factory QueueTransferResponse.fromJson(Map<String, dynamic> json) {
    return QueueTransferResponse(
      success: json['success'] ?? false,
      newQueueEntryId: json['newQueueEntryId'],
      newSalonId: json['newSalonId'],
      newSalonName: json['newSalonName'],
      newPosition: json['newPosition'] ?? 0,
      estimatedWaitMinutes: json['estimatedWaitMinutes'] ?? 0,
      transferredAt: DateTime.parse(json['transferredAt']),
      transferType: json['transferType'],
      errors: (json['errors'] as List<dynamic>?)?.cast<String>() ?? [],
      warnings: (json['warnings'] as List<dynamic>?)?.cast<String>() ?? [],
    );
  }

  String get formattedWaitTime {
    if (estimatedWaitMinutes == 0) return 'No wait';
    if (estimatedWaitMinutes < 60) return '${estimatedWaitMinutes} min';
    final hours = estimatedWaitMinutes ~/ 60;
    final minutes = estimatedWaitMinutes % 60;
    return '${hours}h ${minutes}m';
  }
}

/// Transfer suggestion for better queue experience
class QueueTransferSuggestion {
  final String suggestionId;
  final String type; // "salon", "service", "time"
  final String title;
  final String description;
  final String targetSalonId;
  final String targetSalonName;
  final int estimatedWaitMinutes;
  final int positionImprovement;
  final int timeImprovement;
  final double distanceKm;
  final String priority; // "high", "medium", "low"
  final double confidenceScore;
  final DateTime validUntil;
  final Map<String, dynamic> metadata;

  const QueueTransferSuggestion({
    required this.suggestionId,
    required this.type,
    required this.title,
    required this.description,
    required this.targetSalonId,
    required this.targetSalonName,
    required this.estimatedWaitMinutes,
    required this.positionImprovement,
    required this.timeImprovement,
    required this.distanceKm,
    required this.priority,
    required this.confidenceScore,
    required this.validUntil,
    this.metadata = const {},
  });

  factory QueueTransferSuggestion.fromJson(Map<String, dynamic> json) {
    return QueueTransferSuggestion(
      suggestionId: json['suggestionId'] ?? '',
      type: json['type'] ?? '',
      title: json['title'] ?? '',
      description: json['description'] ?? '',
      targetSalonId: json['targetSalonId'] ?? '',
      targetSalonName: json['targetSalonName'] ?? '',
      estimatedWaitMinutes: json['estimatedWaitMinutes'] ?? 0,
      positionImprovement: json['positionImprovement'] ?? 0,
      timeImprovement: json['timeImprovement'] ?? 0,
      distanceKm: (json['distanceKm'] ?? 0.0).toDouble(),
      priority: json['priority'] ?? 'medium',
      confidenceScore: (json['confidenceScore'] ?? 0.0).toDouble(),
      validUntil: DateTime.parse(json['validUntil']),
      metadata: json['metadata'] ?? {},
    );
  }

  bool get isExpired => DateTime.now().isAfter(validUntil);
  
  String get formattedDistance {
    if (distanceKm < 1.0) {
      return '${(distanceKm * 1000).round()}m';
    }
    return '${distanceKm.toStringAsFixed(1)}km';
  }

  String get formattedWaitTime {
    if (estimatedWaitMinutes == 0) return 'No wait';
    if (estimatedWaitMinutes < 60) return '${estimatedWaitMinutes} min';
    final hours = estimatedWaitMinutes ~/ 60;
    final minutes = estimatedWaitMinutes % 60;
    return '${hours}h ${minutes}m';
  }

  String get priorityColor {
    switch (priority.toLowerCase()) {
      case 'high':
        return '#4CAF50'; // Green
      case 'medium':
        return '#FF9800'; // Orange
      case 'low':
        return '#2196F3'; // Blue
      default:
        return '#9E9E9E'; // Grey
    }
  }

  String get improvementText {
    if (timeImprovement > 0) {
      return 'Save ${timeImprovement} min';
    } else if (positionImprovement > 0) {
      return '${positionImprovement} positions better';
    }
    return 'Alternative option';
  }
}

/// Request for transfer suggestions
class TransferSuggestionsRequest {
  final String queueEntryId;
  final double? maxDistanceKm;
  final int? maxWaitTimeMinutes;
  final List<String>? preferredServiceTypes;
  final bool includeSalonTransfers;
  final bool includeServiceTransfers;
  final bool includeTimeTransfers;

  const TransferSuggestionsRequest({
    required this.queueEntryId,
    this.maxDistanceKm,
    this.maxWaitTimeMinutes,
    this.preferredServiceTypes,
    this.includeSalonTransfers = true,
    this.includeServiceTransfers = true,
    this.includeTimeTransfers = false,
  });

  Map<String, dynamic> toJson() {
    return {
      'queueEntryId': queueEntryId,
      if (maxDistanceKm != null) 'maxDistanceKm': maxDistanceKm,
      if (maxWaitTimeMinutes != null) 'maxWaitTimeMinutes': maxWaitTimeMinutes,
      if (preferredServiceTypes != null) 'preferredServiceTypes': preferredServiceTypes,
      'includeSalonTransfers': includeSalonTransfers,
      'includeServiceTransfers': includeServiceTransfers,
      'includeTimeTransfers': includeTimeTransfers,
    };
  }
}

/// Response with transfer suggestions
class TransferSuggestionsResponse {
  final bool success;
  final List<QueueTransferSuggestion> suggestions;
  final String currentSalonId;
  final String currentSalonName;
  final int currentPosition;
  final int currentEstimatedWaitMinutes;
  final DateTime generatedAt;
  final List<String> errors;

  const TransferSuggestionsResponse({
    required this.success,
    required this.suggestions,
    required this.currentSalonId,
    required this.currentSalonName,
    required this.currentPosition,
    required this.currentEstimatedWaitMinutes,
    required this.generatedAt,
    this.errors = const [],
  });

  factory TransferSuggestionsResponse.fromJson(Map<String, dynamic> json) {
    return TransferSuggestionsResponse(
      success: json['success'] ?? false,
      suggestions: (json['suggestions'] as List<dynamic>?)
          ?.map((s) => QueueTransferSuggestion.fromJson(s))
          .toList() ?? [],
      currentSalonId: json['currentSalonId'] ?? '',
      currentSalonName: json['currentSalonName'] ?? '',
      currentPosition: json['currentPosition'] ?? 0,
      currentEstimatedWaitMinutes: json['currentEstimatedWaitMinutes'] ?? 0,
      generatedAt: DateTime.parse(json['generatedAt']),
      errors: (json['errors'] as List<dynamic>?)?.cast<String>() ?? [],
    );
  }

  bool get hasSuggestions => suggestions.isNotEmpty;
  
  List<QueueTransferSuggestion> get validSuggestions => 
      suggestions.where((s) => !s.isExpired).toList();
  
  List<QueueTransferSuggestion> get highPrioritySuggestions => 
      validSuggestions.where((s) => s.priority == 'high').toList();

  String get currentFormattedWaitTime {
    if (currentEstimatedWaitMinutes == 0) return 'No wait';
    if (currentEstimatedWaitMinutes < 60) return '${currentEstimatedWaitMinutes} min';
    final hours = currentEstimatedWaitMinutes ~/ 60;
    final minutes = currentEstimatedWaitMinutes % 60;
    return '${hours}h ${minutes}m';
  }
}

/// Transfer eligibility check
class TransferEligibilityRequest {
  final String queueEntryId;
  final String transferType;
  final String? targetSalonId;
  final String? targetServiceType;

  const TransferEligibilityRequest({
    required this.queueEntryId,
    required this.transferType,
    this.targetSalonId,
    this.targetServiceType,
  });

  Map<String, dynamic> toJson() {
    return {
      'queueEntryId': queueEntryId,
      'transferType': transferType,
      if (targetSalonId != null) 'targetSalonId': targetSalonId,
      if (targetServiceType != null) 'targetServiceType': targetServiceType,
    };
  }
}

/// Transfer eligibility response
class TransferEligibilityResponse {
  final bool isEligible;
  final List<String> requirements;
  final List<String> restrictions;
  final List<String> warnings;
  final int? estimatedNewPosition;
  final int? estimatedNewWaitMinutes;
  final double? transferFee;
  final DateTime? earliestTransferTime;
  final DateTime? latestTransferTime;

  const TransferEligibilityResponse({
    required this.isEligible,
    this.requirements = const [],
    this.restrictions = const [],
    this.warnings = const [],
    this.estimatedNewPosition,
    this.estimatedNewWaitMinutes,
    this.transferFee,
    this.earliestTransferTime,
    this.latestTransferTime,
  });

  factory TransferEligibilityResponse.fromJson(Map<String, dynamic> json) {
    return TransferEligibilityResponse(
      isEligible: json['isEligible'] ?? false,
      requirements: (json['requirements'] as List<dynamic>?)?.cast<String>() ?? [],
      restrictions: (json['restrictions'] as List<dynamic>?)?.cast<String>() ?? [],
      warnings: (json['warnings'] as List<dynamic>?)?.cast<String>() ?? [],
      estimatedNewPosition: json['estimatedNewPosition'],
      estimatedNewWaitMinutes: json['estimatedNewWaitMinutes'],
      transferFee: json['transferFee']?.toDouble(),
      earliestTransferTime: json['earliestTransferTime'] != null 
          ? DateTime.parse(json['earliestTransferTime']) : null,
      latestTransferTime: json['latestTransferTime'] != null 
          ? DateTime.parse(json['latestTransferTime']) : null,
    );
  }

  bool get hasRestrictions => restrictions.isNotEmpty;
  bool get hasWarnings => warnings.isNotEmpty;
  bool get hasFee => transferFee != null && transferFee! > 0;

  String? get formattedNewWaitTime {
    if (estimatedNewWaitMinutes == null) return null;
    if (estimatedNewWaitMinutes == 0) return 'No wait';
    if (estimatedNewWaitMinutes! < 60) return '${estimatedNewWaitMinutes} min';
    final hours = estimatedNewWaitMinutes! ~/ 60;
    final minutes = estimatedNewWaitMinutes! % 60;
    return '${hours}h ${minutes}m';
  }
}

/// Transfer analytics for tracking
class QueueTransferAnalytics {
  final String transferId;
  final String originalQueueEntryId;
  final String newQueueEntryId;
  final String transferType;
  final String originalSalonId;
  final String targetSalonId;
  final int originalPosition;
  final int newPosition;
  final int originalWaitMinutes;
  final int newWaitMinutes;
  final DateTime transferredAt;
  final DateTime? serviceCompletedAt;
  final bool wasSuccessful;
  final String? reason;
  final int customerSatisfactionRating;
  final Map<String, dynamic> metadata;

  const QueueTransferAnalytics({
    required this.transferId,
    required this.originalQueueEntryId,
    required this.newQueueEntryId,
    required this.transferType,
    required this.originalSalonId,
    required this.targetSalonId,
    required this.originalPosition,
    required this.newPosition,
    required this.originalWaitMinutes,
    required this.newWaitMinutes,
    required this.transferredAt,
    this.serviceCompletedAt,
    required this.wasSuccessful,
    this.reason,
    required this.customerSatisfactionRating,
    this.metadata = const {},
  });

  factory QueueTransferAnalytics.fromJson(Map<String, dynamic> json) {
    return QueueTransferAnalytics(
      transferId: json['transferId'] ?? '',
      originalQueueEntryId: json['originalQueueEntryId'] ?? '',
      newQueueEntryId: json['newQueueEntryId'] ?? '',
      transferType: json['transferType'] ?? '',
      originalSalonId: json['originalSalonId'] ?? '',
      targetSalonId: json['targetSalonId'] ?? '',
      originalPosition: json['originalPosition'] ?? 0,
      newPosition: json['newPosition'] ?? 0,
      originalWaitMinutes: json['originalWaitMinutes'] ?? 0,
      newWaitMinutes: json['newWaitMinutes'] ?? 0,
      transferredAt: DateTime.parse(json['transferredAt']),
      serviceCompletedAt: json['serviceCompletedAt'] != null 
          ? DateTime.parse(json['serviceCompletedAt']) : null,
      wasSuccessful: json['wasSuccessful'] ?? false,
      reason: json['reason'],
      customerSatisfactionRating: json['customerSatisfactionRating'] ?? 0,
      metadata: json['metadata'] ?? {},
    );
  }

  int get positionImprovement => originalPosition - newPosition;
  int get waitTimeImprovement => originalWaitMinutes - newWaitMinutes;
  
  bool get improvedPosition => positionImprovement > 0;
  bool get improvedWaitTime => waitTimeImprovement > 0;
  
  String get improvementSummary {
    if (improvedPosition && improvedWaitTime) {
      return 'Improved by $positionImprovement positions and saved $waitTimeImprovement minutes';
    } else if (improvedPosition) {
      return 'Improved by $positionImprovement positions';
    } else if (improvedWaitTime) {
      return 'Saved $waitTimeImprovement minutes';
    }
    return 'Transfer completed';
  }
}
