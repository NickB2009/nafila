import 'package:json_annotation/json_annotation.dart';

part 'services_models.g.dart';

/// Request model for adding a new service offered
@JsonSerializable()
class AddServiceOfferedRequest {
  final String? locationId;
  final String? name;
  final String? description;
  final int estimatedDurationMinutes;
  final double price;
  final String? imageUrl;

  const AddServiceOfferedRequest({
    this.locationId,
    this.name,
    this.description,
    required this.estimatedDurationMinutes,
    required this.price,
    this.imageUrl,
  });

  factory AddServiceOfferedRequest.fromJson(Map<String, dynamic> json) =>
      _$AddServiceOfferedRequestFromJson(json);

  Map<String, dynamic> toJson() => _$AddServiceOfferedRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is AddServiceOfferedRequest &&
          runtimeType == other.runtimeType &&
          locationId == other.locationId &&
          name == other.name &&
          description == other.description &&
          estimatedDurationMinutes == other.estimatedDurationMinutes &&
          price == other.price &&
          imageUrl == other.imageUrl;

  @override
  int get hashCode => Object.hash(
    locationId,
    name,
    description,
    estimatedDurationMinutes,
    price,
    imageUrl,
  );
}

/// Request model for updating a service offered
@JsonSerializable()
class UpdateServiceOfferedRequest {
  final String? name;
  final String? description;
  final String? locationId;
  final int? estimatedDurationMinutes;
  final double? price;
  final String? imageUrl;
  final bool isActive;

  const UpdateServiceOfferedRequest({
    this.name,
    this.description,
    this.locationId,
    this.estimatedDurationMinutes,
    this.price,
    this.imageUrl,
    this.isActive = true,
  });

  factory UpdateServiceOfferedRequest.fromJson(Map<String, dynamic> json) =>
      _$UpdateServiceOfferedRequestFromJson(json);

  Map<String, dynamic> toJson() => _$UpdateServiceOfferedRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is UpdateServiceOfferedRequest &&
          runtimeType == other.runtimeType &&
          name == other.name &&
          description == other.description &&
          locationId == other.locationId &&
          estimatedDurationMinutes == other.estimatedDurationMinutes &&
          price == other.price &&
          imageUrl == other.imageUrl &&
          isActive == other.isActive;

  @override
  int get hashCode => Object.hash(
    name,
    description,
    locationId,
    estimatedDurationMinutes,
    price,
    imageUrl,
    isActive,
  );
}

/// DTO for service offered information
@JsonSerializable()
class ServiceOfferedDto {
  final String id;
  final String locationId;
  final String name;
  final String? description;
  final int estimatedDurationMinutes;
  final double price;
  final String? imageUrl;
  final bool isActive;
  final DateTime createdAt;
  final DateTime? updatedAt;

  const ServiceOfferedDto({
    required this.id,
    required this.locationId,
    required this.name,
    this.description,
    required this.estimatedDurationMinutes,
    required this.price,
    this.imageUrl,
    required this.isActive,
    required this.createdAt,
    this.updatedAt,
  });

  factory ServiceOfferedDto.fromJson(Map<String, dynamic> json) =>
      _$ServiceOfferedDtoFromJson(json);

  Map<String, dynamic> toJson() => _$ServiceOfferedDtoToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is ServiceOfferedDto &&
          runtimeType == other.runtimeType &&
          id == other.id &&
          locationId == other.locationId &&
          name == other.name &&
          description == other.description &&
          estimatedDurationMinutes == other.estimatedDurationMinutes &&
          price == other.price &&
          imageUrl == other.imageUrl &&
          isActive == other.isActive &&
          createdAt == other.createdAt &&
          updatedAt == other.updatedAt;

  @override
  int get hashCode => Object.hash(
    id,
    locationId,
    name,
    description,
    estimatedDurationMinutes,
    price,
    imageUrl,
    isActive,
    createdAt,
    updatedAt,
  );
}

/// Result model for service operations
@JsonSerializable()
class ServiceOperationResult {
  final bool success;
  final String? serviceId;
  final Map<String, String>? fieldErrors;
  final List<String>? errors;

  const ServiceOperationResult({
    required this.success,
    this.serviceId,
    this.fieldErrors,
    this.errors,
  });

  factory ServiceOperationResult.fromJson(Map<String, dynamic> json) =>
      _$ServiceOperationResultFromJson(json);

  Map<String, dynamic> toJson() => _$ServiceOperationResultToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is ServiceOperationResult &&
          runtimeType == other.runtimeType &&
          success == other.success &&
          serviceId == other.serviceId &&
          fieldErrors == other.fieldErrors &&
          errors == other.errors;

  @override
  int get hashCode => Object.hash(success, serviceId, fieldErrors, errors);
}