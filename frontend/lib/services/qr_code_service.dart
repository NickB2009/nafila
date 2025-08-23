import 'package:dio/dio.dart';
import 'package:permission_handler/permission_handler.dart';
import '../config/api_config.dart';
import '../models/qr_code_models.dart';
import 'api_client.dart';

/// Service for handling QR code operations
class QrCodeService {
  final ApiClient _apiClient;

  const QrCodeService({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  /// Creates QrCodeService with default dependencies
  static Future<QrCodeService> create() async {
    final apiClient = ApiClient();
    return QrCodeService(apiClient: apiClient);
  }

  /// Generate QR code for queue joining (staff only)
  Future<QrGenerationResponse> generateQrCode(QrGenerationRequest request) async {
    try {
      final response = await _apiClient.post(
        '${ApiConfig.qrCodeEndpoint}/generate',
        data: request.toJson(),
      );
      
      return QrGenerationResponse.fromJson(response.data);
    } on DioException catch (e) {
      if (e.response?.statusCode == 401) {
        throw Exception('Unauthorized: Staff access required');
      } else if (e.response?.statusCode == 400) {
        final errorData = e.response?.data;
        if (errorData is Map<String, dynamic> && errorData['errors'] != null) {
          final errors = (errorData['errors'] as List).cast<String>();
          return QrGenerationResponse(
            success: false,
            errors: errors,
          );
        }
        throw Exception('Invalid request parameters');
      }
      throw Exception('Failed to generate QR code: ${e.message}');
    } catch (e) {
      throw Exception('Failed to generate QR code: $e');
    }
  }

  /// Check camera permission for QR scanning
  Future<bool> checkCameraPermission() async {
    try {
      final status = await Permission.camera.status;
      return status.isGranted;
    } catch (e) {
      return false;
    }
  }

  /// Request camera permission for QR scanning
  Future<bool> requestCameraPermission() async {
    try {
      final status = await Permission.camera.request();
      return status.isGranted;
    } catch (e) {
      return false;
    }
  }

  /// Parse QR code data from scanned result
  QrScanResult parseQrCode(String rawData) {
    return QrScanResult.fromRawData(rawData);
  }

  /// Validate QR join data
  bool validateQrJoinData(QrJoinData joinData) {
    // Check if expired
    if (joinData.isExpired) {
      return false;
    }

    // Check if location ID is valid (basic validation)
    if (joinData.locationId.isEmpty) {
      return false;
    }

    // Additional validation can be added here
    return true;
  }

  /// Get QR code expiry status
  QrExpiryStatus getExpiryStatus(QrJoinData joinData) {
    if (joinData.expiresAt == null) {
      return QrExpiryStatus.noExpiry;
    }

    final now = DateTime.now();
    final expiresAt = joinData.expiresAt!;

    if (now.isAfter(expiresAt)) {
      return QrExpiryStatus.expired;
    }

    final timeRemaining = expiresAt.difference(now);
    if (timeRemaining.inMinutes <= 5) {
      return QrExpiryStatus.expiringSoon;
    }

    return QrExpiryStatus.valid;
  }

  /// Create join URL for manual entry (fallback)
  String createJoinUrl({
    required String locationId,
    String? serviceTypeId,
    DateTime? expiresAt,
  }) {
    final baseUrl = 'https://app.eutonafila.com';
    var joinUrl = '$baseUrl/join?locationId=$locationId';
    
    if (serviceTypeId != null && serviceTypeId.isNotEmpty) {
      joinUrl += '&serviceTypeId=$serviceTypeId';
    }

    if (expiresAt != null) {
      final expiryUnixTime = expiresAt.millisecondsSinceEpoch ~/ 1000;
      joinUrl += '&expires=$expiryUnixTime';
    }

    return joinUrl;
  }

  /// Extract location info from QR data for display
  Future<Map<String, dynamic>?> getLocationInfo(String locationId) async {
    try {
      // This would typically call a public endpoint to get location info
      // For now, return null to indicate we need to implement this
      return null;
    } catch (e) {
      return null;
    }
  }

  /// Validate QR code format without parsing
  bool isValidQrFormat(String data) {
    try {
      final uri = Uri.parse(data);
      return uri.path.contains('/join') && 
             uri.queryParameters.containsKey('locationId');
    } catch (e) {
      return false;
    }
  }

  /// Get user-friendly error message for QR scan issues
  String getErrorMessage(QrScanResult result) {
    if (result.errorMessage != null) {
      return result.errorMessage!;
    }

    if (result.joinData?.isExpired == true) {
      return 'This QR code has expired. Please ask staff for a new one.';
    }

    if (!isValidQrFormat(result.rawData)) {
      return 'This is not a valid queue QR code.';
    }

    return 'Unable to process QR code. Please try again.';
  }
}

/// QR code expiry status
enum QrExpiryStatus {
  valid,
  expiringSoon,
  expired,
  noExpiry,
}

/// Extension for QrExpiryStatus
extension QrExpiryStatusExtension on QrExpiryStatus {
  String get displayText {
    switch (this) {
      case QrExpiryStatus.valid:
        return 'Valid';
      case QrExpiryStatus.expiringSoon:
        return 'Expires soon';
      case QrExpiryStatus.expired:
        return 'Expired';
      case QrExpiryStatus.noExpiry:
        return 'No expiry';
    }
  }

  String get color {
    switch (this) {
      case QrExpiryStatus.valid:
        return '#4CAF50'; // Green
      case QrExpiryStatus.expiringSoon:
        return '#FF9800'; // Orange
      case QrExpiryStatus.expired:
        return '#F44336'; // Red
      case QrExpiryStatus.noExpiry:
        return '#2196F3'; // Blue
    }
  }
}
