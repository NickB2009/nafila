import 'package:json_annotation/json_annotation.dart';

part 'staff_models.g.dart';

/// Request model for adding a new barber
@JsonSerializable()
class AddBarberRequest {
  final String firstName;
  final String lastName;
  final String email;
  final String phoneNumber;
  final String username;
  final String locationId;
  final List<String> serviceTypeIds;
  final bool deactivateOnCreation;
  final String? address;
  final String? notes;

  const AddBarberRequest({
    required this.firstName,
    required this.lastName,
    required this.email,
    required this.phoneNumber,
    required this.username,
    required this.locationId,
    required this.serviceTypeIds,
    this.deactivateOnCreation = false,
    this.address,
    this.notes,
  });

  factory AddBarberRequest.fromJson(Map<String, dynamic> json) =>
      _$AddBarberRequestFromJson(json);

  Map<String, dynamic> toJson() => _$AddBarberRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is AddBarberRequest &&
          runtimeType == other.runtimeType &&
          firstName == other.firstName &&
          lastName == other.lastName &&
          email == other.email &&
          phoneNumber == other.phoneNumber &&
          username == other.username &&
          locationId == other.locationId &&
          serviceTypeIds == other.serviceTypeIds &&
          deactivateOnCreation == other.deactivateOnCreation &&
          address == other.address &&
          notes == other.notes;

  @override
  int get hashCode => Object.hash(
    firstName,
    lastName,
    email,
    phoneNumber,
    username,
    locationId,
    serviceTypeIds,
    deactivateOnCreation,
    address,
    notes,
  );
}

/// Result model for adding a barber
@JsonSerializable()
class AddBarberResult {
  final bool success;
  final String? barberId;
  final String status;
  final Map<String, String>? fieldErrors;
  final List<String>? errors;

  const AddBarberResult({
    required this.success,
    this.barberId,
    required this.status,
    this.fieldErrors,
    this.errors,
  });

  factory AddBarberResult.fromJson(Map<String, dynamic> json) =>
      _$AddBarberResultFromJson(json);

  Map<String, dynamic> toJson() => _$AddBarberResultToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is AddBarberResult &&
          runtimeType == other.runtimeType &&
          success == other.success &&
          barberId == other.barberId &&
          status == other.status &&
          fieldErrors == other.fieldErrors &&
          errors == other.errors;

  @override
  int get hashCode => Object.hash(success, barberId, status, fieldErrors, errors);
}

/// Request model for editing a barber
@JsonSerializable()
class EditBarberRequest {
  final String? staffMemberId;
  final String? name;
  final String? email;
  final String? phoneNumber;
  final String? profilePictureUrl;
  final String? role;

  const EditBarberRequest({
    this.staffMemberId,
    this.name,
    this.email,
    this.phoneNumber,
    this.profilePictureUrl,
    this.role,
  });

  factory EditBarberRequest.fromJson(Map<String, dynamic> json) =>
      _$EditBarberRequestFromJson(json);

  Map<String, dynamic> toJson() => _$EditBarberRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is EditBarberRequest &&
          runtimeType == other.runtimeType &&
          staffMemberId == other.staffMemberId &&
          name == other.name &&
          email == other.email &&
          phoneNumber == other.phoneNumber &&
          profilePictureUrl == other.profilePictureUrl &&
          role == other.role;

  @override
  int get hashCode => Object.hash(staffMemberId, name, email, phoneNumber, profilePictureUrl, role);
}

/// Result model for editing a barber
@JsonSerializable()
class EditBarberResult {
  final bool success;
  final String? staffMemberId;
  final String? name;
  final String? email;
  final String? phoneNumber;
  final String? profilePictureUrl;
  final String? role;
  final List<String>? errors;
  final Map<String, String>? fieldErrors;

  const EditBarberResult({
    required this.success,
    this.staffMemberId,
    this.name,
    this.email,
    this.phoneNumber,
    this.profilePictureUrl,
    this.role,
    this.errors,
    this.fieldErrors,
  });

  factory EditBarberResult.fromJson(Map<String, dynamic> json) =>
      _$EditBarberResultFromJson(json);

  Map<String, dynamic> toJson() => _$EditBarberResultToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is EditBarberResult &&
          runtimeType == other.runtimeType &&
          success == other.success &&
          staffMemberId == other.staffMemberId &&
          name == other.name &&
          email == other.email &&
          phoneNumber == other.phoneNumber &&
          profilePictureUrl == other.profilePictureUrl &&
          role == other.role &&
          errors == other.errors &&
          fieldErrors == other.fieldErrors;

  @override
  int get hashCode => Object.hash(
    success,
    staffMemberId,
    name,
    email,
    phoneNumber,
    profilePictureUrl,
    role,
    errors,
    fieldErrors,
  );
}

/// Request model for updating staff status
@JsonSerializable()
class UpdateStaffStatusRequest {
  final String staffMemberId;
  final String newStatus;
  final String? notes;

  const UpdateStaffStatusRequest({
    required this.staffMemberId,
    required this.newStatus,
    this.notes,
  });

  factory UpdateStaffStatusRequest.fromJson(Map<String, dynamic> json) =>
      _$UpdateStaffStatusRequestFromJson(json);

  Map<String, dynamic> toJson() => _$UpdateStaffStatusRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is UpdateStaffStatusRequest &&
          runtimeType == other.runtimeType &&
          staffMemberId == other.staffMemberId &&
          newStatus == other.newStatus &&
          notes == other.notes;

  @override
  int get hashCode => Object.hash(staffMemberId, newStatus, notes);
}

/// Result model for updating staff status
@JsonSerializable()
class UpdateStaffStatusResult {
  final bool success;
  final String staffMemberId;
  final String newStatus;
  final String? previousStatus;
  final Map<String, String>? fieldErrors;
  final List<String>? errors;

  const UpdateStaffStatusResult({
    required this.success,
    required this.staffMemberId,
    required this.newStatus,
    this.previousStatus,
    this.fieldErrors,
    this.errors,
  });

  factory UpdateStaffStatusResult.fromJson(Map<String, dynamic> json) =>
      _$UpdateStaffStatusResultFromJson(json);

  Map<String, dynamic> toJson() => _$UpdateStaffStatusResultToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is UpdateStaffStatusResult &&
          runtimeType == other.runtimeType &&
          success == other.success &&
          staffMemberId == other.staffMemberId &&
          newStatus == other.newStatus &&
          previousStatus == other.previousStatus &&
          fieldErrors == other.fieldErrors &&
          errors == other.errors;

  @override
  int get hashCode => Object.hash(
    success,
    staffMemberId,
    newStatus,
    previousStatus,
    fieldErrors,
    errors,
  );
}

/// Request model for starting a break
@JsonSerializable()
class StartBreakRequest {
  final String staffMemberId;
  final int durationMinutes;
  final String? reason;

  const StartBreakRequest({
    required this.staffMemberId,
    required this.durationMinutes,
    this.reason,
  });

  factory StartBreakRequest.fromJson(Map<String, dynamic> json) =>
      _$StartBreakRequestFromJson(json);

  Map<String, dynamic> toJson() => _$StartBreakRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is StartBreakRequest &&
          runtimeType == other.runtimeType &&
          staffMemberId == other.staffMemberId &&
          durationMinutes == other.durationMinutes &&
          reason == other.reason;

  @override
  int get hashCode => Object.hash(staffMemberId, durationMinutes, reason);
}

/// Result model for starting a break
@JsonSerializable()
class StartBreakResult {
  final bool success;
  final String staffMemberId;
  final String? breakId;
  final String? newStatus;
  final Map<String, String>? fieldErrors;
  final List<String>? errors;

  const StartBreakResult({
    required this.success,
    required this.staffMemberId,
    this.breakId,
    this.newStatus,
    this.fieldErrors,
    this.errors,
  });

  factory StartBreakResult.fromJson(Map<String, dynamic> json) =>
      _$StartBreakResultFromJson(json);

  Map<String, dynamic> toJson() => _$StartBreakResultToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is StartBreakResult &&
          runtimeType == other.runtimeType &&
          success == other.success &&
          staffMemberId == other.staffMemberId &&
          breakId == other.breakId &&
          newStatus == other.newStatus &&
          fieldErrors == other.fieldErrors &&
          errors == other.errors;

  @override
  int get hashCode => Object.hash(
    success,
    staffMemberId,
    breakId,
    newStatus,
    fieldErrors,
    errors,
  );
}

/// Request model for ending a break
@JsonSerializable()
class EndBreakRequest {
  final String? staffMemberId;
  final String? breakId;

  const EndBreakRequest({
    this.staffMemberId,
    this.breakId,
  });

  factory EndBreakRequest.fromJson(Map<String, dynamic> json) =>
      _$EndBreakRequestFromJson(json);

  Map<String, dynamic> toJson() => _$EndBreakRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is EndBreakRequest &&
          runtimeType == other.runtimeType &&
          staffMemberId == other.staffMemberId &&
          breakId == other.breakId;

  @override
  int get hashCode => Object.hash(staffMemberId, breakId);
}

/// Result model for ending a break
@JsonSerializable()
class EndBreakResult {
  final bool success;
  final String staffMemberId;
  final String breakId;
  final String newStatus;
  final Map<String, String>? fieldErrors;
  final List<String>? errors;

  const EndBreakResult({
    required this.success,
    required this.staffMemberId,
    required this.breakId,
    required this.newStatus,
    this.fieldErrors,
    this.errors,
  });

  factory EndBreakResult.fromJson(Map<String, dynamic> json) =>
      _$EndBreakResultFromJson(json);

  Map<String, dynamic> toJson() => _$EndBreakResultToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is EndBreakResult &&
          runtimeType == other.runtimeType &&
          success == other.success &&
          staffMemberId == other.staffMemberId &&
          breakId == other.breakId &&
          newStatus == other.newStatus &&
          fieldErrors == other.fieldErrors &&
          errors == other.errors;

  @override
  int get hashCode => Object.hash(
    success,
    staffMemberId,
    breakId,
    newStatus,
    fieldErrors,
    errors,
  );
}

/// DTO for staff activity information
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

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is StaffActivityDto &&
          runtimeType == other.runtimeType &&
          staffId == other.staffId &&
          staffName == other.staffName &&
          status == other.status &&
          lastActivity == other.lastActivity &&
          customersServedToday == other.customersServedToday &&
          averageServiceTimeMinutes == other.averageServiceTimeMinutes &&
          isOnBreak == other.isOnBreak &&
          breakStartTime == other.breakStartTime;

  @override
  int get hashCode => Object.hash(
    staffId,
    staffName,
    status,
    lastActivity,
    customersServedToday,
    averageServiceTimeMinutes,
    isOnBreak,
    breakStartTime,
  );
}