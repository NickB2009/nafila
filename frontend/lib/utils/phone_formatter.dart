import 'package:flutter/services.dart';

/// Phone number formatter that handles international formats
class PhoneNumberFormatter extends TextInputFormatter {
  static const Map<String, String> _countryCodes = {
    'BR': '+55',
    'US': '+1',
    'UK': '+44',
    'DE': '+49',
    'FR': '+33',
    'IT': '+39',
    'ES': '+34',
    'PT': '+351',
    'AR': '+54',
    'MX': '+52',
  };

  @override
  TextEditingValue formatEditUpdate(
    TextEditingValue oldValue,
    TextEditingValue newValue,
  ) {
    final text = newValue.text;
    
    // Don't format if user is deleting
    if (text.length < oldValue.text.length) {
      return newValue;
    }
    
    // Remove all non-digit characters except +
    String cleaned = text.replaceAll(RegExp(r'[^\d+]'), '');
    
    // If it doesn't start with +, add +55 (Brazil default)
    if (!cleaned.startsWith('+')) {
      cleaned = '+55$cleaned';
    }
    
    // Format based on country code
    String formatted = _formatByCountryCode(cleaned);
    
    return TextEditingValue(
      text: formatted,
      selection: TextSelection.collapsed(offset: formatted.length),
    );
  }

  String _formatByCountryCode(String phone) {
    if (phone.startsWith('+55')) {
      // Brazilian format: +55 (11) 99999-9999
      final digits = phone.substring(3);
      if (digits.length >= 2) {
        String formatted = '+55';
        if (digits.length >= 2) {
          formatted += ' (${digits.substring(0, 2)}';
          if (digits.length > 2) {
            formatted += ')';
            if (digits.length >= 7) {
              formatted += ' ${digits.substring(2, 7)}';
              if (digits.length > 7) {
                formatted += '-${digits.substring(7, digits.length > 11 ? 11 : digits.length)}';
              }
            } else {
              formatted += ' ${digits.substring(2)}';
            }
          }
        }
        return formatted;
      }
    } else if (phone.startsWith('+1')) {
      // US format: +1 (555) 123-4567
      final digits = phone.substring(2);
      if (digits.length >= 3) {
        String formatted = '+1';
        if (digits.length >= 3) {
          formatted += ' (${digits.substring(0, 3)}';
          if (digits.length > 3) {
            formatted += ')';
            if (digits.length >= 6) {
              formatted += ' ${digits.substring(3, 6)}';
              if (digits.length > 6) {
                formatted += '-${digits.substring(6, digits.length > 10 ? 10 : digits.length)}';
              }
            } else {
              formatted += ' ${digits.substring(3)}';
            }
          }
        }
        return formatted;
      }
    }
    
    // Default: just return with country code
    return phone;
  }

  /// Extract clean phone number for API calls
  static String getCleanPhoneNumber(String formattedPhone) {
    return formattedPhone.replaceAll(RegExp(r'[^\d+]'), '');
  }

  /// Get display format for phone number
  static String getDisplayFormat(String cleanPhone) {
    final formatter = PhoneNumberFormatter();
    return formatter._formatByCountryCode(cleanPhone);
  }

  /// Check if phone number is valid
  static bool isValidPhoneNumber(String phone) {
    final clean = getCleanPhoneNumber(phone);
    
    // Must start with + and have at least 7 digits after country code
    if (!clean.startsWith('+')) return false;
    
    // Brazilian numbers: +55 + 2 digit area code + 8-9 digits
    if (clean.startsWith('+55')) {
      final digits = clean.substring(3);
      return digits.length >= 10 && digits.length <= 11;
    }
    
    // US numbers: +1 + 3 digit area code + 7 digits  
    if (clean.startsWith('+1')) {
      final digits = clean.substring(2);
      return digits.length == 10;
    }
    
    // Other international numbers: at least 7 digits after country code
    final countryCodeMatch = RegExp(r'^\+(\d{1,4})').firstMatch(clean);
    if (countryCodeMatch != null) {
      final countryCode = countryCodeMatch.group(1)!;
      final digits = clean.substring(countryCode.length + 1);
      return digits.length >= 7 && digits.length <= 15;
    }
    
    return false;
  }

  /// Get country code from phone number
  static String? getCountryCode(String phone) {
    final clean = getCleanPhoneNumber(phone);
    final match = RegExp(r'^\+(\d{1,4})').firstMatch(clean);
    return match?.group(1);
  }

  /// Get supported country codes
  static Map<String, String> get supportedCountries => _countryCodes;
}