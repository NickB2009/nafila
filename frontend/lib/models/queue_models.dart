import 'package:json_annotation/json_annotation.dart';

part 'queue_models.g.dart';

/// Request model for adding a new queue
@JsonSerializable()
class AddQueueRequest {
  final String locationId;
  final int? maxSize;
  final int? lateClientCapTimeInMinutes;

  const AddQueueRequest({
    required this.locationId,
    this.maxSize,
    this.lateClientCapTimeInMinutes,
  });

  factory AddQueueRequest.fromJson(Map<String, dynamic> json) =>
      _$AddQueueRequestFromJson(json);

  Map<String, dynamic> toJson() => _$AddQueueRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is AddQueueRequest &&
          runtimeType == other.runtimeType &&
          locationId == other.locationId &&
          maxSize == other.maxSize &&
          lateClientCapTimeInMinutes == other.lateClientCapTimeInMinutes;

  @override
  int get hashCode => Object.hash(locationId, maxSize, lateClientCapTimeInMinutes);
}

/// Request model for joining a queue
@JsonSerializable()
class JoinQueueRequest {
  final String? queueId;
  final String? customerName;
  final String? phoneNumber;
  final String? email;
  final bool isAnonymous;
  final String? notes;
  final String? serviceTypeId;

  const JoinQueueRequest({
    this.queueId,
    this.customerName,
    this.phoneNumber,
    this.email,
    this.isAnonymous = false,
    this.notes,
    this.serviceTypeId,
  });

  factory JoinQueueRequest.fromJson(Map<String, dynamic> json) =>
      _$JoinQueueRequestFromJson(json);

  Map<String, dynamic> toJson() => _$JoinQueueRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is JoinQueueRequest &&
          runtimeType == other.runtimeType &&
          queueId == other.queueId &&
          customerName == other.customerName &&
          phoneNumber == other.phoneNumber &&
          email == other.email &&
          isAnonymous == other.isAnonymous &&
          notes == other.notes &&
          serviceTypeId == other.serviceTypeId;

  @override
  int get hashCode => Object.hash(queueId, customerName, phoneNumber, email, isAnonymous, notes, serviceTypeId);
}

/// Request model for barber adding a customer to queue
@JsonSerializable()
class BarberAddRequest {
  final String? queueId;
  final String? staffMemberId;
  final String? customerName;
  final String? phoneNumber;
  final String? email;
  final String? serviceTypeId;
  final String? notes;

  const BarberAddRequest({
    this.queueId,
    this.staffMemberId,
    this.customerName,
    this.phoneNumber,
    this.email,
    this.serviceTypeId,
    this.notes,
  });

  factory BarberAddRequest.fromJson(Map<String, dynamic> json) =>
      _$BarberAddRequestFromJson(json);

  Map<String, dynamic> toJson() => _$BarberAddRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is BarberAddRequest &&
          runtimeType == other.runtimeType &&
          queueId == other.queueId &&
          staffMemberId == other.staffMemberId &&
          customerName == other.customerName &&
          phoneNumber == other.phoneNumber &&
          email == other.email &&
          serviceTypeId == other.serviceTypeId &&
          notes == other.notes;

  @override
  int get hashCode => Object.hash(queueId, staffMemberId, customerName, phoneNumber, email, serviceTypeId, notes);
}

/// Request model for calling next customer
@JsonSerializable()
class CallNextRequest {
  final String? staffMemberId;
  final String? queueId;

  const CallNextRequest({
    this.staffMemberId,
    this.queueId,
  });

  factory CallNextRequest.fromJson(Map<String, dynamic> json) =>
      _$CallNextRequestFromJson(json);

  Map<String, dynamic> toJson() => _$CallNextRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is CallNextRequest &&
          runtimeType == other.runtimeType &&
          staffMemberId == other.staffMemberId &&
          queueId == other.queueId;

  @override
  int get hashCode => Object.hash(staffMemberId, queueId);
}

/// Request model for checking in a customer
@JsonSerializable()
class CheckInRequest {
  final String? queueEntryId;

  const CheckInRequest({
    this.queueEntryId,
  });

  factory CheckInRequest.fromJson(Map<String, dynamic> json) =>
      _$CheckInRequestFromJson(json);

  Map<String, dynamic> toJson() => _$CheckInRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is CheckInRequest &&
          runtimeType == other.runtimeType &&
          queueEntryId == other.queueEntryId;

  @override
  int get hashCode => queueEntryId.hashCode;
}

/// Request model for finishing a service
@JsonSerializable()
class FinishRequest {
  final String? queueEntryId;
  final int? serviceDurationMinutes;
  final String? notes;

  const FinishRequest({
    this.queueEntryId,
    this.serviceDurationMinutes,
    this.notes,
  });

  factory FinishRequest.fromJson(Map<String, dynamic> json) =>
      _$FinishRequestFromJson(json);

  Map<String, dynamic> toJson() => _$FinishRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is FinishRequest &&
          runtimeType == other.runtimeType &&
          queueEntryId == other.queueEntryId &&
          serviceDurationMinutes == other.serviceDurationMinutes &&
          notes == other.notes;

  @override
  int get hashCode => Object.hash(queueEntryId, serviceDurationMinutes, notes);
}

/// Request model for canceling a queue entry
@JsonSerializable()
class CancelQueueRequest {
  final String queueEntryId;

  const CancelQueueRequest({
    required this.queueEntryId,
  });

  factory CancelQueueRequest.fromJson(Map<String, dynamic> json) =>
      _$CancelQueueRequestFromJson(json);

  Map<String, dynamic> toJson() => _$CancelQueueRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is CancelQueueRequest &&
          runtimeType == other.runtimeType &&
          queueEntryId == other.queueEntryId;

  @override
  int get hashCode => queueEntryId.hashCode;
}

/// DTO for queue activity information
@JsonSerializable()
class QueueActivityDto {
  final String? queueId;
  final DateTime queueDate;
  final bool isActive;
  final int customersWaiting;
  final int customersBeingServed;
  final int totalCustomersToday;
  final double averageWaitTimeMinutes;
  final int? maxSize;

  const QueueActivityDto({
    this.queueId,
    required this.queueDate,
    required this.isActive,
    required this.customersWaiting,
    required this.customersBeingServed,
    required this.totalCustomersToday,
    required this.averageWaitTimeMinutes,
    this.maxSize,
  });

  factory QueueActivityDto.fromJson(Map<String, dynamic> json) =>
      _$QueueActivityDtoFromJson(json);

  Map<String, dynamic> toJson() => _$QueueActivityDtoToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is QueueActivityDto &&
          runtimeType == other.runtimeType &&
          queueId == other.queueId &&
          queueDate == other.queueDate &&
          isActive == other.isActive &&
          customersWaiting == other.customersWaiting &&
          customersBeingServed == other.customersBeingServed &&
          totalCustomersToday == other.totalCustomersToday &&
          averageWaitTimeMinutes == other.averageWaitTimeMinutes &&
          maxSize == other.maxSize;

  @override
  int get hashCode => Object.hash(
    queueId,
    queueDate,
    isActive,
    customersWaiting,
    customersBeingServed,
    totalCustomersToday,
    averageWaitTimeMinutes,
    maxSize,
  );
}

/// Request model for saving haircut details
@JsonSerializable()
class SaveHaircutDetailsRequest {
  final String? queueEntryId;
  final String? haircutDetails;
  final String? additionalNotes;
  final String? photoUrl;

  const SaveHaircutDetailsRequest({
    this.queueEntryId,
    this.haircutDetails,
    this.additionalNotes,
    this.photoUrl,
  });

  factory SaveHaircutDetailsRequest.fromJson(Map<String, dynamic> json) =>
      _$SaveHaircutDetailsRequestFromJson(json);

  Map<String, dynamic> toJson() => _$SaveHaircutDetailsRequestToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is SaveHaircutDetailsRequest &&
          runtimeType == other.runtimeType &&
          queueEntryId == other.queueEntryId &&
          haircutDetails == other.haircutDetails &&
          additionalNotes == other.additionalNotes &&
          photoUrl == other.photoUrl;

  @override
  int get hashCode => Object.hash(queueEntryId, haircutDetails, additionalNotes, photoUrl);
}

/// Result model for saving haircut details
@JsonSerializable()
class SaveHaircutDetailsResult {
  final bool success;
  final String? serviceHistoryId;
  final String? customerId;
  final List<String>? errors;
  final Map<String, String>? fieldErrors;

  const SaveHaircutDetailsResult({
    required this.success,
    this.serviceHistoryId,
    this.customerId,
    this.errors,
    this.fieldErrors,
  });

  factory SaveHaircutDetailsResult.fromJson(Map<String, dynamic> json) =>
      _$SaveHaircutDetailsResultFromJson(json);

  Map<String, dynamic> toJson() => _$SaveHaircutDetailsResultToJson(this);

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is SaveHaircutDetailsResult &&
          runtimeType == other.runtimeType &&
          success == other.success &&
          serviceHistoryId == other.serviceHistoryId &&
          customerId == other.customerId &&
          errors == other.errors &&
          fieldErrors == other.fieldErrors;

  @override
  int get hashCode => Object.hash(success, serviceHistoryId, customerId, errors, fieldErrors);
}