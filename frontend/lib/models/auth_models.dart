import 'package:json_annotation/json_annotation.dart';

part 'auth_models.g.dart';

/// Request model for user login
@JsonSerializable()
class LoginRequest {
  final String phoneNumber;
  final String password;

  const LoginRequest({
    required this.phoneNumber,
    required this.password,
  });

  factory LoginRequest.fromJson(Map<String, dynamic> json) =>
      _$LoginRequestFromJson(json);

  Map<String, dynamic> toJson() => {
    'PhoneNumber': phoneNumber,
    'Password': password,
  };
}

/// Response model for login results
@JsonSerializable()
class LoginResult {
  final bool success;
  final String? token;
  final String? error;
  final String? fullName;
  final String? phoneNumber;
  final String? email;
  final String? role;
  final List<String>? permissions;
  final bool requiresTwoFactor;
  final String? twoFactorToken;

  const LoginResult({
    required this.success,
    this.token,
    this.error,
    this.fullName,
    this.phoneNumber,
    this.email,
    this.role,
    this.permissions,
    this.requiresTwoFactor = false,
    this.twoFactorToken,
  });

  factory LoginResult.fromJson(Map<String, dynamic> json) =>
      _$LoginResultFromJson(json);

  Map<String, dynamic> toJson() => _$LoginResultToJson(this);
}

/// Request model for user registration
@JsonSerializable()
class RegisterRequest {
  final String fullName;
  final String email;
  final String phoneNumber;
  final String password;

  const RegisterRequest({
    required this.fullName,
    required this.email,
    required this.phoneNumber,
    required this.password,
  });

  factory RegisterRequest.fromJson(Map<String, dynamic> json) =>
      _$RegisterRequestFromJson(json);

  Map<String, dynamic> toJson() => {
    'FullName': fullName,
    'Email': email,
    'PhoneNumber': phoneNumber,
    'Password': password,
  };
}

/// Response model for registration results
@JsonSerializable()
class RegisterResult {
  final bool success;
  final String? error;
  final Map<String, String>? fieldErrors;

  const RegisterResult({
    required this.success,
    this.error,
    this.fieldErrors,
  });

  factory RegisterResult.fromJson(Map<String, dynamic> json) =>
      _$RegisterResultFromJson(json);

  Map<String, dynamic> toJson() => _$RegisterResultToJson(this);
}

/// Request model for two-factor authentication verification
@JsonSerializable()
class VerifyTwoFactorRequest {
  final String phoneNumber;
  final String twoFactorCode;
  final String twoFactorToken;

  const VerifyTwoFactorRequest({
    required this.phoneNumber,
    required this.twoFactorCode,
    required this.twoFactorToken,
  });

  factory VerifyTwoFactorRequest.fromJson(Map<String, dynamic> json) =>
      _$VerifyTwoFactorRequestFromJson(json);

  Map<String, dynamic> toJson() => _$VerifyTwoFactorRequestToJson(this);
}

/// Request model for admin verification
@JsonSerializable()
class AdminVerificationRequest {
  final String phoneNumber;
  final String twoFactorCode;
  final String twoFactorToken;

  const AdminVerificationRequest({
    required this.phoneNumber,
    required this.twoFactorCode,
    required this.twoFactorToken,
  });

  factory AdminVerificationRequest.fromJson(Map<String, dynamic> json) =>
      _$AdminVerificationRequestFromJson(json);

  Map<String, dynamic> toJson() => _$AdminVerificationRequestToJson(this);
}

/// Response model for admin verification results
@JsonSerializable()
class AdminVerificationResult {
  final bool success;
  final String? error;
  final String? token;
  final List<String>? permissions;

  const AdminVerificationResult({
    required this.success,
    this.error,
    this.token,
    this.permissions,
  });

  factory AdminVerificationResult.fromJson(Map<String, dynamic> json) =>
      _$AdminVerificationResultFromJson(json);

  Map<String, dynamic> toJson() => _$AdminVerificationResultToJson(this);
}

/// User model for profile information
@JsonSerializable()
class User {
  final String id;
  final String fullName;
  final String email;
  final String phoneNumber;
  final String role;
  final List<String> permissions;

  const User({
    required this.id,
    required this.fullName,
    required this.email,
    required this.phoneNumber,
    required this.role,
    required this.permissions,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      id: json['UserId'] ?? json['id'] ?? '',
      fullName: json['FullName'] ?? json['fullName'] ?? '',
      email: json['Email'] ?? json['email'] ?? '',
      phoneNumber: json['PhoneNumber'] ?? json['phoneNumber'] ?? '',
      role: json['Role'] ?? json['role'] ?? '',
      permissions: List<String>.from(json['Permissions'] ?? json['permissions'] ?? []),
    );
  }

  Map<String, dynamic> toJson() => _$UserToJson(this);
}