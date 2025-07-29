import 'package:json_annotation/json_annotation.dart';

part 'location_models.g.dart';

/// Request model for location address
@JsonSerializable()
class LocationAddressRequest {
  final String? street;
  final String? city;
  final String? state;
  final String? postalCode;
  final String? country;

  const LocationAddressRequest({
    this.street,
    this.city,
    this.state,
    this.postalCode,
    this.country,
  });

  factory LocationAddressRequest.fromJson(Map<String, dynamic> json) =>
      _$LocationAddressRequestFromJson(json);

  Map<String, dynamic> toJson() => _$LocationAddressRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is LocationAddressRequest &&
          runtimeType == other.runtimeType &&
          street == other.street &&
          city == other.city &&
          state == other.state &&
          postalCode == other.postalCode &&
          country == other.country;

  @override
  int get hashCode => Object.hash(street, city, state, postalCode, country);
}

/// Request model for creating a new location
@JsonSerializable(explicitToJson: true)
class CreateLocationRequest {
  final String? businessName;
  final String? contactEmail;
  final String? contactPhone;
  final LocationAddressRequest? address;
  final Map<String, String>? businessHours;
  final int? maxQueueCapacity;
  final String? description;
  final String? website;

  const CreateLocationRequest({
    this.businessName,
    this.contactEmail,
    this.contactPhone,
    this.address,
    this.businessHours,
    this.maxQueueCapacity,
    this.description,
    this.website,
  });

  factory CreateLocationRequest.fromJson(Map<String, dynamic> json) =>
      _$CreateLocationRequestFromJson(json);

  Map<String, dynamic> toJson() => _$CreateLocationRequestToJson(this);
}

/// Request model for updating a location
@JsonSerializable(explicitToJson: true)
class UpdateLocationRequest {
  final String? businessName;
  final LocationAddressRequest? address;
  final Map<String, String>? businessHours;
  final int? maxQueueCapacity;
  final String? description;

  const UpdateLocationRequest({
    this.businessName,
    this.address,
    this.businessHours,
    this.maxQueueCapacity,
    this.description,
  });

  factory UpdateLocationRequest.fromJson(Map<String, dynamic> json) =>
      _$UpdateLocationRequestFromJson(json);

  Map<String, dynamic> toJson() => _$UpdateLocationRequestToJson(this);
}

/// Request model for toggling queue status
@JsonSerializable()
class ToggleQueueRequest {
  final String? locationId;
  final bool enableQueue;

  const ToggleQueueRequest({
    this.locationId,
    required this.enableQueue,
  });

  factory ToggleQueueRequest.fromJson(Map<String, dynamic> json) =>
      _$ToggleQueueRequestFromJson(json);

  Map<String, dynamic> toJson() => _$ToggleQueueRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is ToggleQueueRequest &&
          runtimeType == other.runtimeType &&
          locationId == other.locationId &&
          enableQueue == other.enableQueue;

  @override
  int get hashCode => Object.hash(locationId, enableQueue);
}

/// Model representing a complete location with all details
@JsonSerializable(explicitToJson: true)
class Location {
  final String id;
  final String organizationId;
  final String businessName;
  final String? contactEmail;
  final String? contactPhone;
  final LocationAddress? address;
  final Map<String, String>? businessHours;
  final int maxQueueCapacity;
  final String? description;
  final String? website;
  final bool isActive;
  final DateTime createdAt;
  final DateTime updatedAt;

  const Location({
    required this.id,
    required this.organizationId,
    required this.businessName,
    this.contactEmail,
    this.contactPhone,
    this.address,
    this.businessHours,
    required this.maxQueueCapacity,
    this.description,
    this.website,
    required this.isActive,
    required this.createdAt,
    required this.updatedAt,
  });

  factory Location.fromJson(Map<String, dynamic> json) =>
      _$LocationFromJson(json);

  Map<String, dynamic> toJson() => _$LocationToJson(this);

  Location copyWith({
    String? id,
    String? organizationId,
    String? businessName,
    String? contactEmail,
    String? contactPhone,
    LocationAddress? address,
    Map<String, String>? businessHours,
    int? maxQueueCapacity,
    String? description,
    String? website,
    bool? isActive,
    DateTime? createdAt,
    DateTime? updatedAt,
  }) {
    return Location(
      id: id ?? this.id,
      organizationId: organizationId ?? this.organizationId,
      businessName: businessName ?? this.businessName,
      contactEmail: contactEmail ?? this.contactEmail,
      contactPhone: contactPhone ?? this.contactPhone,
      address: address ?? this.address,
      businessHours: businessHours ?? this.businessHours,
      maxQueueCapacity: maxQueueCapacity ?? this.maxQueueCapacity,
      description: description ?? this.description,
      website: website ?? this.website,
      isActive: isActive ?? this.isActive,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
    );
  }
}

/// Model representing a location address (response model)
@JsonSerializable()
class LocationAddress {
  final String? street;
  final String? city;
  final String? state;
  final String? postalCode;
  final String? country;

  const LocationAddress({
    this.street,
    this.city,
    this.state,
    this.postalCode,
    this.country,
  });

  factory LocationAddress.fromJson(Map<String, dynamic> json) =>
      _$LocationAddressFromJson(json);

  Map<String, dynamic> toJson() => _$LocationAddressToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is LocationAddress &&
          runtimeType == other.runtimeType &&
          street == other.street &&
          city == other.city &&
          state == other.state &&
          postalCode == other.postalCode &&
          country == other.country;

  @override
  int get hashCode => Object.hash(street, city, state, postalCode, country);

  @override
  String toString() {
    final parts = <String>[];
    if (street != null) parts.add(street!);
    if (city != null) parts.add(city!);
    if (state != null) parts.add(state!);
    if (postalCode != null) parts.add(postalCode!);
    if (country != null) parts.add(country!);
    return parts.join(', ');
  }
}

/// Model representing an organization
@JsonSerializable(explicitToJson: true)
class Organization {
  final String id;
  final String name;
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
  final String subscriptionPlanId;
  final bool sharesDataForAnalytics;
  final bool isActive;
  final DateTime createdAt;
  final DateTime updatedAt;

  const Organization({
    required this.id,
    required this.name,
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
    required this.subscriptionPlanId,
    required this.sharesDataForAnalytics,
    required this.isActive,
    required this.createdAt,
    required this.updatedAt,
  });

  factory Organization.fromJson(Map<String, dynamic> json) =>
      _$OrganizationFromJson(json);

  Map<String, dynamic> toJson() => _$OrganizationToJson(this);

  Organization copyWith({
    String? id,
    String? name,
    String? slug,
    String? description,
    String? contactEmail,
    String? contactPhone,
    String? websiteUrl,
    String? primaryColor,
    String? secondaryColor,
    String? logoUrl,
    String? faviconUrl,
    String? tagLine,
    String? subscriptionPlanId,
    bool? sharesDataForAnalytics,
    bool? isActive,
    DateTime? createdAt,
    DateTime? updatedAt,
  }) {
    return Organization(
      id: id ?? this.id,
      name: name ?? this.name,
      slug: slug ?? this.slug,
      description: description ?? this.description,
      contactEmail: contactEmail ?? this.contactEmail,
      contactPhone: contactPhone ?? this.contactPhone,
      websiteUrl: websiteUrl ?? this.websiteUrl,
      primaryColor: primaryColor ?? this.primaryColor,
      secondaryColor: secondaryColor ?? this.secondaryColor,
      logoUrl: logoUrl ?? this.logoUrl,
      faviconUrl: faviconUrl ?? this.faviconUrl,
      tagLine: tagLine ?? this.tagLine,
      subscriptionPlanId: subscriptionPlanId ?? this.subscriptionPlanId,
      sharesDataForAnalytics:
          sharesDataForAnalytics ?? this.sharesDataForAnalytics,
      isActive: isActive ?? this.isActive,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
    );
  }
}