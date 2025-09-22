import 'package:json_annotation/json_annotation.dart';

part 'anonymous_user.g.dart';

@JsonSerializable()
class AnonymousUser {
  final String id; // Local UUID
  final String name;
  final String email;
  final DateTime createdAt;
  final AnonymousUserPreferences preferences;
  final List<AnonymousQueueEntry> activeQueues;
  final List<AnonymousQueueEntry> queueHistory;

  const AnonymousUser({
    required this.id,
    required this.name,
    required this.email,
    required this.createdAt,
    required this.preferences,
    required this.activeQueues,
    required this.queueHistory,
  });

  factory AnonymousUser.fromJson(Map<String, dynamic> json) =>
      _$AnonymousUserFromJson(json);

  Map<String, dynamic> toJson() => _$AnonymousUserToJson(this);

  AnonymousUser copyWith({
    String? name,
    String? email,
    AnonymousUserPreferences? preferences,
    List<AnonymousQueueEntry>? activeQueues,
    List<AnonymousQueueEntry>? queueHistory,
  }) {
    return AnonymousUser(
      id: id,
      name: name ?? this.name,
      email: email ?? this.email,
      createdAt: createdAt,
      preferences: preferences ?? this.preferences,
      activeQueues: activeQueues ?? this.activeQueues,
      queueHistory: queueHistory ?? this.queueHistory,
    );
  }
}

@JsonSerializable()
class AnonymousUserPreferences {
  final bool emailNotifications;
  final bool browserNotifications;
  final bool saveQueueHistory;

  const AnonymousUserPreferences({
    required this.emailNotifications,
    required this.browserNotifications,
    required this.saveQueueHistory,
  });

  factory AnonymousUserPreferences.fromJson(Map<String, dynamic> json) =>
      _$AnonymousUserPreferencesFromJson(json);

  Map<String, dynamic> toJson() => _$AnonymousUserPreferencesToJson(this);

  factory AnonymousUserPreferences.defaultPreferences() {
    return const AnonymousUserPreferences(
      emailNotifications: true,
      browserNotifications: true,
      saveQueueHistory: true,
    );
  }
}

@JsonSerializable()
class AnonymousQueueEntry {
  final String id; // Backend queue entry ID
  final String anonymousUserId; // Local user ID
  final String salonId;
  final String salonName;
  final int position;
  final int estimatedWaitMinutes;
  final DateTime joinedAt;
  final DateTime? lastUpdated;
  final QueueEntryStatus status;
  final String? serviceRequested;

  const AnonymousQueueEntry({
    required this.id,
    required this.anonymousUserId,
    required this.salonId,
    required this.salonName,
    required this.position,
    required this.estimatedWaitMinutes,
    required this.joinedAt,
    this.lastUpdated,
    required this.status,
    this.serviceRequested,
  });

  factory AnonymousQueueEntry.fromJson(Map<String, dynamic> json) =>
      _$AnonymousQueueEntryFromJson(json);

  Map<String, dynamic> toJson() => _$AnonymousQueueEntryToJson(this);

  bool get isActive => status == QueueEntryStatus.waiting || status == QueueEntryStatus.called;

  AnonymousQueueEntry copyWith({
    int? position,
    int? estimatedWaitMinutes,
    DateTime? lastUpdated,
    QueueEntryStatus? status,
  }) {
    return AnonymousQueueEntry(
      id: id,
      anonymousUserId: anonymousUserId,
      salonId: salonId,
      salonName: salonName,
      position: position ?? this.position,
      estimatedWaitMinutes: estimatedWaitMinutes ?? this.estimatedWaitMinutes,
      joinedAt: joinedAt,
      lastUpdated: lastUpdated ?? this.lastUpdated,
      status: status ?? this.status,
      serviceRequested: serviceRequested,
    );
  }
}

enum QueueEntryStatus {
  waiting,
  called,
  completed,
  cancelled,
  expired,
}

@JsonSerializable()
class JoinQueueRequest {
  final String salonId;
  final String name;
  final String email;
  final String anonymousUserId;
  final String? serviceRequested;
  final bool emailNotifications;
  final bool browserNotifications;

  const JoinQueueRequest({
    required this.salonId,
    required this.name,
    required this.email,
    required this.anonymousUserId,
    this.serviceRequested,
    required this.emailNotifications,
    required this.browserNotifications,
  });

  factory JoinQueueRequest.fromJson(Map<String, dynamic> json) =>
      _$JoinQueueRequestFromJson(json);

  Map<String, dynamic> toJson() => _$JoinQueueRequestToJson(this);
}

/// Response model for anonymous queue join API
@JsonSerializable()
class AnonymousJoinResult {
  final bool success;
  final String? id;
  final int position;
  final int estimatedWaitMinutes;
  final DateTime? joinedAt;
  final String? status;
  final Map<String, String>? fieldErrors;
  final List<String>? errors;

  const AnonymousJoinResult({
    required this.success,
    this.id,
    required this.position,
    required this.estimatedWaitMinutes,
    this.joinedAt,
    this.status,
    this.fieldErrors,
    this.errors,
  });

  factory AnonymousJoinResult.fromJson(Map<String, dynamic> json) =>
      _$AnonymousJoinResultFromJson(json);

  Map<String, dynamic> toJson() => _$AnonymousJoinResultToJson(this);
} 