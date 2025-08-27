/// Data models for QR code functionality
library;

/// QR code join data extracted from scanned QR codes
class QrJoinData {
  final String locationId;
  final String? serviceTypeId;
  final DateTime? expiresAt;
  final String originalUrl;

  const QrJoinData({
    required this.locationId,
    this.serviceTypeId,
    this.expiresAt,
    required this.originalUrl,
  });

  /// Parse QR join data from URL
  static QrJoinData? fromUrl(String url) {
    try {
      final uri = Uri.parse(url);
      
      // Check if this is a valid join URL
      if (!uri.path.contains('/join')) {
        return null;
      }

      final locationId = uri.queryParameters['locationId'];
      if (locationId == null || locationId.isEmpty) {
        return null;
      }

      final serviceTypeId = uri.queryParameters['serviceTypeId'];
      
      DateTime? expiresAt;
      final expiresParam = uri.queryParameters['expires'];
      if (expiresParam != null) {
        final expiryUnixTime = int.tryParse(expiresParam);
        if (expiryUnixTime != null) {
          expiresAt = DateTime.fromMillisecondsSinceEpoch(expiryUnixTime * 1000);
        }
      }

      return QrJoinData(
        locationId: locationId,
        serviceTypeId: serviceTypeId,
        expiresAt: expiresAt,
        originalUrl: url,
      );
    } catch (e) {
      return null;
    }
  }

  /// Check if the QR code has expired
  bool get isExpired {
    if (expiresAt == null) return false;
    return DateTime.now().isAfter(expiresAt!);
  }

  /// Get time remaining until expiry
  Duration? get timeRemaining {
    if (expiresAt == null) return null;
    final now = DateTime.now();
    if (now.isAfter(expiresAt!)) return Duration.zero;
    return expiresAt!.difference(now);
  }

  /// Get formatted expiry time
  String? get formattedExpiryTime {
    if (expiresAt == null) return null;
    final remaining = timeRemaining;
    if (remaining == null || remaining == Duration.zero) return 'Expired';
    
    if (remaining.inHours > 0) {
      return '${remaining.inHours}h ${remaining.inMinutes % 60}m';
    } else {
      return '${remaining.inMinutes}m';
    }
  }

  /// Check if QR code is valid (not expired and has required data)
  bool get isValid => !isExpired && locationId.isNotEmpty;

  @override
  String toString() {
    return 'QrJoinData(locationId: $locationId, serviceTypeId: $serviceTypeId, expiresAt: $expiresAt)';
  }
}

/// QR code generation request for staff
class QrGenerationRequest {
  final String locationId;
  final String? serviceTypeId;
  final int? expiryMinutes;

  const QrGenerationRequest({
    required this.locationId,
    this.serviceTypeId,
    this.expiryMinutes = 60, // Default 1 hour
  });

  Map<String, dynamic> toJson() {
    return {
      'locationId': locationId,
      if (serviceTypeId != null) 'serviceTypeId': serviceTypeId,
      if (expiryMinutes != null) 'expiryMinutes': expiryMinutes.toString(),
    };
  }
}

/// QR code generation response from backend
class QrGenerationResponse {
  final bool success;
  final String? qrCodeBase64;
  final String? joinUrl;
  final DateTime? expiresAt;
  final List<String> errors;

  const QrGenerationResponse({
    required this.success,
    this.qrCodeBase64,
    this.joinUrl,
    this.expiresAt,
    this.errors = const [],
  });

  factory QrGenerationResponse.fromJson(Map<String, dynamic> json) {
    DateTime? expiresAt;
    if (json['expiresAt'] != null) {
      try {
        expiresAt = DateTime.parse(json['expiresAt']);
      } catch (e) {
        // Handle parsing error
      }
    }

    return QrGenerationResponse(
      success: json['success'] ?? false,
      qrCodeBase64: json['qrCodeBase64'],
      joinUrl: json['joinUrl'],
      expiresAt: expiresAt,
      errors: (json['errors'] as List<dynamic>?)?.cast<String>() ?? [],
    );
  }

  /// Check if response has valid QR data
  bool get hasValidQrCode => success && qrCodeBase64 != null && joinUrl != null;

  /// Get formatted expiry time
  String? get formattedExpiryTime {
    if (expiresAt == null) return null;
    final remaining = expiresAt!.difference(DateTime.now());
    if (remaining.isNegative) return 'Expired';
    
    if (remaining.inHours > 0) {
      return '${remaining.inHours}h ${remaining.inMinutes % 60}m';
    } else {
      return '${remaining.inMinutes}m';
    }
  }
}

/// QR scanner result with validation
class QrScanResult {
  final String rawData;
  final QrJoinData? joinData;
  final bool isValid;
  final String? errorMessage;

  const QrScanResult({
    required this.rawData,
    this.joinData,
    required this.isValid,
    this.errorMessage,
  });

  factory QrScanResult.fromRawData(String rawData) {
    final joinData = QrJoinData.fromUrl(rawData);
    
    if (joinData == null) {
      return QrScanResult(
        rawData: rawData,
        isValid: false,
        errorMessage: 'Invalid QR code format',
      );
    }

    if (joinData.isExpired) {
      return QrScanResult(
        rawData: rawData,
        joinData: joinData,
        isValid: false,
        errorMessage: 'QR code has expired',
      );
    }

    return QrScanResult(
      rawData: rawData,
      joinData: joinData,
      isValid: true,
    );
  }

  /// Create error result
  factory QrScanResult.error(String rawData, String errorMessage) {
    return QrScanResult(
      rawData: rawData,
      isValid: false,
      errorMessage: errorMessage,
    );
  }
}

/// QR code scanner state
enum QrScannerState {
  idle,
  scanning,
  success,
  error,
  permissionDenied,
}

/// QR code display options for staff
class QrDisplayOptions {
  final bool showUrl;
  final bool showExpiryTime;
  final bool allowDownload;
  final bool allowShare;
  final double size;

  const QrDisplayOptions({
    this.showUrl = true,
    this.showExpiryTime = true,
    this.allowDownload = true,
    this.allowShare = true,
    this.size = 200.0,
  });
}
