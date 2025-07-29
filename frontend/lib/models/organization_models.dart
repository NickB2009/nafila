import 'package:json_annotation/json_annotation.dart';

part 'organization_models.g.dart';

/// Request model for creating a new organization
@JsonSerializable()
class CreateOrganizationRequest {
  final String name;
  final String subscriptionPlanId;
  final String? slug;
  final String? description;
  final String? contactEmail;
  final String? contactPhone;
  final String? websiteUrl;
  final String? primaryColor;
  final String? secondaryColor;
  final String? logoUrl;
  final String? faviconUrl;
  final String? tagLine;
  final bool sharesDataForAnalytics;

  const CreateOrganizationRequest({
    required this.name,
    required this.subscriptionPlanId,
    this.slug,
    this.description,
    this.contactEmail,
    this.contactPhone,
    this.websiteUrl,
    this.primaryColor,
    this.secondaryColor,
    this.logoUrl,
    this.faviconUrl,
    this.tagLine,
    this.sharesDataForAnalytics = false,
  });

  factory CreateOrganizationRequest.fromJson(Map<String, dynamic> json) =>
      _$CreateOrganizationRequestFromJson(json);

  Map<String, dynamic> toJson() => _$CreateOrganizationRequestToJson(this);
}

/// Response model for organization creation results
@JsonSerializable()
class CreateOrganizationResult {
  final bool success;
  final String? organizationId;
  final String status;
  final String? slug;
  final Map<String, String> fieldErrors;
  final List<String> errors;

  const CreateOrganizationResult({
    required this.success,
    required this.organizationId,
    required this.status,
    required this.slug,
    required this.fieldErrors,
    required this.errors,
  });

  factory CreateOrganizationResult.fromJson(Map<String, dynamic> json) =>
      _$CreateOrganizationResultFromJson(json);

  Map<String, dynamic> toJson() => _$CreateOrganizationResultToJson(this);
}

/// Request model for updating an organization
@JsonSerializable()
class UpdateOrganizationRequest {
  final String organizationId;
  final String name;
  final String? description;
  final String? contactEmail;
  final String? contactPhone;
  final String? websiteUrl;

  const UpdateOrganizationRequest({
    required this.organizationId,
    required this.name,
    this.description,
    this.contactEmail,
    this.contactPhone,
    this.websiteUrl,
  });

  factory UpdateOrganizationRequest.fromJson(Map<String, dynamic> json) =>
      _$UpdateOrganizationRequestFromJson(json);

  Map<String, dynamic> toJson() => _$UpdateOrganizationRequestToJson(this);
}

/// Request model for updating organization branding
@JsonSerializable()
class UpdateBrandingRequest {
  final String organizationId;
  final String primaryColor;
  final String secondaryColor;
  final String? logoUrl;
  final String? faviconUrl;
  final String? tagLine;

  const UpdateBrandingRequest({
    required this.organizationId,
    required this.primaryColor,
    required this.secondaryColor,
    this.logoUrl,
    this.faviconUrl,
    this.tagLine,
  });

  factory UpdateBrandingRequest.fromJson(Map<String, dynamic> json) =>
      _$UpdateBrandingRequestFromJson(json);

  Map<String, dynamic> toJson() => _$UpdateBrandingRequestToJson(this);
}

/// Response model for organization operation results
@JsonSerializable()
class OrganizationOperationResult {
  final bool success;
  final Map<String, String> fieldErrors;
  final List<String> errors;

  const OrganizationOperationResult({
    required this.success,
    required this.fieldErrors,
    required this.errors,
  });

  factory OrganizationOperationResult.fromJson(Map<String, dynamic> json) =>
      _$OrganizationOperationResultFromJson(json);

  Map<String, dynamic> toJson() => _$OrganizationOperationResultToJson(this);
}

/// Model representing live activity data for organizations
@JsonSerializable()
class TrackLiveActivityResult {
  final bool success;
  final LiveActivityDto? liveActivity;
  final List<String> errors;
  final Map<String, String> fieldErrors;

  const TrackLiveActivityResult({
    required this.success,
    this.liveActivity,
    required this.errors,
    required this.fieldErrors,
  });

  factory TrackLiveActivityResult.fromJson(Map<String, dynamic> json) =>
      _$TrackLiveActivityResultFromJson(json);

  Map<String, dynamic> toJson() => _$TrackLiveActivityResultToJson(this);
}

/// Model for live activity data
@JsonSerializable()
class LiveActivityDto {
  final String? organizationId;
  final String? organizationName;
  final DateTime lastUpdated;
  final OrganizationSummaryDto? summary;
  final List<LocationActivityDto>? locations;

  const LiveActivityDto({
    this.organizationId,
    this.organizationName,
    required this.lastUpdated,
    this.summary,
    this.locations,
  });

  factory LiveActivityDto.fromJson(Map<String, dynamic> json) =>
      _$LiveActivityDtoFromJson(json);

  Map<String, dynamic> toJson() => _$LiveActivityDtoToJson(this);
}

/// Model for organization summary statistics
@JsonSerializable()
class OrganizationSummaryDto {
  final int totalLocations;
  final int totalActiveQueues;
  final int totalCustomersWaiting;
  final int totalStaffMembers;
  final int totalAvailableStaff;
  final int totalBusyStaff;
  final double averageWaitTimeMinutes;

  const OrganizationSummaryDto({
    required this.totalLocations,
    required this.totalActiveQueues,
    required this.totalCustomersWaiting,
    required this.totalStaffMembers,
    required this.totalAvailableStaff,
    required this.totalBusyStaff,
    required this.averageWaitTimeMinutes,
  });

  factory OrganizationSummaryDto.fromJson(Map<String, dynamic> json) =>
      _$OrganizationSummaryDtoFromJson(json);

  Map<String, dynamic> toJson() => _$OrganizationSummaryDtoToJson(this);
}

/// Model for location activity data
@JsonSerializable()
class LocationActivityDto {
  final String? locationId;
  final String? locationName;
  final bool isOpen;
  final int totalCustomersWaiting;
  final int totalStaffMembers;
  final int availableStaffMembers;
  final int busyStaffMembers;
  final double averageWaitTimeMinutes;
  final List<QueueActivityDto>? queues;
  final List<StaffActivityDto>? staff;

  const LocationActivityDto({
    this.locationId,
    this.locationName,
    required this.isOpen,
    required this.totalCustomersWaiting,
    required this.totalStaffMembers,
    required this.availableStaffMembers,
    required this.busyStaffMembers,
    required this.averageWaitTimeMinutes,
    this.queues,
    this.staff,
  });

  factory LocationActivityDto.fromJson(Map<String, dynamic> json) =>
      _$LocationActivityDtoFromJson(json);

  Map<String, dynamic> toJson() => _$LocationActivityDtoToJson(this);
}

/// Model for queue activity data
@JsonSerializable()
class QueueActivityDto {
  final String? queueId;
  final DateTime queueDate;
  final bool isActive;
  final int customersWaiting;
  final int customersBeingServed;
  final int totalCustomersToday;
  final double averageWaitTimeMinutes;
  final int maxSize;

  const QueueActivityDto({
    this.queueId,
    required this.queueDate,
    required this.isActive,
    required this.customersWaiting,
    required this.customersBeingServed,
    required this.totalCustomersToday,
    required this.averageWaitTimeMinutes,
    required this.maxSize,
  });

  factory QueueActivityDto.fromJson(Map<String, dynamic> json) =>
      _$QueueActivityDtoFromJson(json);

  Map<String, dynamic> toJson() => _$QueueActivityDtoToJson(this);
}

/// Model for staff activity data
@JsonSerializable()
class StaffActivityDto {
  final String? staffId;
  final String? staffName;
  final String? status;
  final DateTime? lastActivity;
  final int customersServedToday;
  final double averageServiceTimeMinutes;
  final bool isOnBreak;
  final DateTime? breakStartTime;

  const StaffActivityDto({
    this.staffId,
    this.staffName,
    this.status,
    this.lastActivity,
    required this.customersServedToday,
    required this.averageServiceTimeMinutes,
    required this.isOnBreak,
    this.breakStartTime,
  });

  factory StaffActivityDto.fromJson(Map<String, dynamic> json) =>
      _$StaffActivityDtoFromJson(json);

  Map<String, dynamic> toJson() => _$StaffActivityDtoToJson(this);
}