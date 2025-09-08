// Manual JSON serialization - no code generation needed

/// Request model for user login
class LoginRequest {
  final String phoneNumber;
  final String password;

  const LoginRequest({
    required this.phoneNumber,
    required this.password,
  });

  factory LoginRequest.fromJson(Map<String, dynamic> json) {
    return LoginRequest(
      phoneNumber: json['phoneNumber'] ?? json['PhoneNumber'] ?? '',
      password: json['password'] ?? json['Password'] ?? '',
    );
  }

  Map<String, dynamic> toJson() => {
    'PhoneNumber': phoneNumber,
    'Password': password,
  };
}

/// Response model for login results
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

  factory LoginResult.fromJson(Map<String, dynamic> json) {
    return LoginResult(
      success: json['success'] ?? false,
      token: json['token'],
      error: json['error'],
      fullName: json['fullName'],
      phoneNumber: json['phoneNumber'],
      email: json['email'],
      role: json['role'],
      permissions: json['permissions'] != null 
          ? List<String>.from(json['permissions']) 
          : null,
      requiresTwoFactor: json['requiresTwoFactor'] ?? false,
      twoFactorToken: json['twoFactorToken'],
    );
  }

  Map<String, dynamic> toJson() => {
    'success': success,
    'token': token,
    'error': error,
    'fullName': fullName,
    'phoneNumber': phoneNumber,
    'email': email,
    'role': role,
    'permissions': permissions,
    'requiresTwoFactor': requiresTwoFactor,
    'twoFactorToken': twoFactorToken,
  };
}

/// Request model for user registration
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

  factory RegisterRequest.fromJson(Map<String, dynamic> json) {
    return RegisterRequest(
      fullName: json['fullName'] ?? json['FullName'] ?? '',
      email: json['email'] ?? json['Email'] ?? '',
      phoneNumber: json['phoneNumber'] ?? json['PhoneNumber'] ?? '',
      password: json['password'] ?? json['Password'] ?? '',
    );
  }

  Map<String, dynamic> toJson() => {
    'FullName': fullName,
    'Email': email,
    'PhoneNumber': phoneNumber,
    'Password': password,
  };
}

/// Response model for registration results
class RegisterResult {
  final bool success;
  final String? error;
  final Map<String, String>? fieldErrors;

  const RegisterResult({
    required this.success,
    this.error,
    this.fieldErrors,
  });

  factory RegisterResult.fromJson(Map<String, dynamic> json) {
    return RegisterResult(
      success: json['success'] ?? false,
      error: json['error'],
      fieldErrors: json['fieldErrors'] != null 
          ? Map<String, String>.from(json['fieldErrors']) 
          : null,
    );
  }

  Map<String, dynamic> toJson() => {
    'success': success,
    'error': error,
    'fieldErrors': fieldErrors,
  };
}

/// Request model for two-factor authentication verification
class VerifyTwoFactorRequest {
  final String phoneNumber;
  final String twoFactorCode;
  final String twoFactorToken;

  const VerifyTwoFactorRequest({
    required this.phoneNumber,
    required this.twoFactorCode,
    required this.twoFactorToken,
  });

  factory VerifyTwoFactorRequest.fromJson(Map<String, dynamic> json) {
    return VerifyTwoFactorRequest(
      phoneNumber: json['phoneNumber'] ?? json['PhoneNumber'] ?? '',
      twoFactorCode: json['twoFactorCode'] ?? json['TwoFactorCode'] ?? '',
      twoFactorToken: json['twoFactorToken'] ?? json['TwoFactorToken'] ?? '',
    );
  }

  Map<String, dynamic> toJson() => {
    'PhoneNumber': phoneNumber,
    'TwoFactorCode': twoFactorCode,
    'TwoFactorToken': twoFactorToken,
  };
}

/// Request model for admin verification
class AdminVerificationRequest {
  final String phoneNumber;
  final String twoFactorCode;
  final String twoFactorToken;

  const AdminVerificationRequest({
    required this.phoneNumber,
    required this.twoFactorCode,
    required this.twoFactorToken,
  });

  factory AdminVerificationRequest.fromJson(Map<String, dynamic> json) {
    return AdminVerificationRequest(
      phoneNumber: json['phoneNumber'] ?? json['PhoneNumber'] ?? '',
      twoFactorCode: json['twoFactorCode'] ?? json['TwoFactorCode'] ?? '',
      twoFactorToken: json['twoFactorToken'] ?? json['TwoFactorToken'] ?? '',
    );
  }

  Map<String, dynamic> toJson() => {
    'PhoneNumber': phoneNumber,
    'TwoFactorCode': twoFactorCode,
    'TwoFactorToken': twoFactorToken,
  };
}

/// Response model for admin verification results
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

  factory AdminVerificationResult.fromJson(Map<String, dynamic> json) {
    return AdminVerificationResult(
      success: json['success'] ?? false,
      error: json['error'],
      token: json['token'],
      permissions: json['permissions'] != null 
          ? List<String>.from(json['permissions']) 
          : null,
    );
  }

  Map<String, dynamic> toJson() => {
    'success': success,
    'error': error,
    'token': token,
    'permissions': permissions,
  };
}

/// User model for profile information
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

  Map<String, dynamic> toJson() => {
    'UserId': id,
    'FullName': fullName,
    'Email': email,
    'PhoneNumber': phoneNumber,
    'Role': role,
    'Permissions': permissions,
  };
}