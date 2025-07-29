import 'package:json_annotation/json_annotation.dart';

part 'public_salon.g.dart';

/// Public salon information available without authentication
@JsonSerializable()
class PublicSalon {
  final String id;
  final String name;
  final String address;
  final double? latitude;
  final double? longitude;
  final double? distanceKm;
  final bool isOpen;
  final int? currentWaitTimeMinutes;
  final int? queueLength;
  final bool isFast; // "RÁPIDO" badge
  final bool isPopular; // "POPULAR" badge
  final String? imageUrl;
  final Map<String, String>? businessHours;
  final List<String>? services;
  final double? rating;
  final int? reviewCount;

  const PublicSalon({
    required this.id,
    required this.name,
    required this.address,
    this.latitude,
    this.longitude,
    this.distanceKm,
    required this.isOpen,
    this.currentWaitTimeMinutes,
    this.queueLength,
    this.isFast = false,
    this.isPopular = false,
    this.imageUrl,
    this.businessHours,
    this.services,
    this.rating,
    this.reviewCount,
  });

  factory PublicSalon.fromJson(Map<String, dynamic> json) =>
      _$PublicSalonFromJson(json);

  Map<String, dynamic> toJson() => _$PublicSalonToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is PublicSalon &&
          runtimeType == other.runtimeType &&
          id == other.id &&
          name == other.name &&
          address == other.address;

  @override
  int get hashCode => Object.hash(id, name, address);
}

/// Request for nearby salons search
@JsonSerializable()
class NearbySearchRequest {
  final double? latitude;
  final double? longitude;
  final double? radiusKm;
  final int? limit;
  final bool? openOnly;
  final bool? fastOnly;

  const NearbySearchRequest({
    this.latitude,
    this.longitude,
    this.radiusKm = 10.0,
    this.limit = 20,
    this.openOnly = false,
    this.fastOnly = false,
  });

  factory NearbySearchRequest.fromJson(Map<String, dynamic> json) =>
      _$NearbySearchRequestFromJson(json);

  Map<String, dynamic> toJson() => _$NearbySearchRequestToJson(this);
}

/// Public queue status information
@JsonSerializable()
class PublicQueueStatus {
  final String salonId;
  final String salonName;
  final int queueLength;
  final int? estimatedWaitTimeMinutes;
  final bool isAcceptingCustomers;
  final DateTime lastUpdated;
  final List<String>? availableServices;

  const PublicQueueStatus({
    required this.salonId,
    required this.salonName,
    required this.queueLength,
    this.estimatedWaitTimeMinutes,
    required this.isAcceptingCustomers,
    required this.lastUpdated,
    this.availableServices,
  });

  factory PublicQueueStatus.fromJson(Map<String, dynamic> json) =>
      _$PublicQueueStatusFromJson(json);

  Map<String, dynamic> toJson() => _$PublicQueueStatusToJson(this);
}