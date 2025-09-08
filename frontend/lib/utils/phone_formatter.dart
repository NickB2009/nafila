import 'package:flutter/services.dart';

/// Utility class for formatting Brazilian phone numbers
class PhoneFormatter {
  /// Formats a Brazilian phone number to (XX) XXXXX-XXXX format
  static String format(String phoneNumber) {
    // Remove all non-digit characters
    final digitsOnly = phoneNumber.replaceAll(RegExp(r'[^0-9]'), '');
    
    if (digitsOnly.isEmpty) return '';
    
    // Format based on length
    if (digitsOnly.length <= 2) {
      return '($digitsOnly';
    } else if (digitsOnly.length <= 7) {
      return '(${digitsOnly.substring(0, 2)}) ${digitsOnly.substring(2)}';
    } else if (digitsOnly.length <= 11) {
      final areaCode = digitsOnly.substring(0, 2);
      final firstPart = digitsOnly.substring(2, digitsOnly.length > 7 ? 7 : digitsOnly.length);
      final secondPart = digitsOnly.length > 7 ? digitsOnly.substring(7) : '';
      
      if (secondPart.isNotEmpty) {
        return '($areaCode) $firstPart-$secondPart';
      } else {
        return '($areaCode) $firstPart';
      }
    } else {
      // Limit to 11 digits for Brazilian phone numbers
      final truncated = digitsOnly.substring(0, 11);
      return format(truncated);
    }
  }
  
  /// Removes formatting from phone number, keeping only digits
  static String unformat(String formattedPhone) {
    return formattedPhone.replaceAll(RegExp(r'[^0-9]'), '');
  }
  
  /// Validates if the phone number has the correct format for Brazil
  static bool isValid(String phoneNumber) {
    final digitsOnly = unformat(phoneNumber);
    return digitsOnly.length >= 10 && digitsOnly.length <= 11;
  }
}

/// Text input formatter for Brazilian phone numbers
class BrazilianPhoneInputFormatter extends TextInputFormatter {
  @override
  TextEditingValue formatEditUpdate(
    TextEditingValue oldValue,
    TextEditingValue newValue,
  ) {
    // Only allow digits
    final digitsOnly = newValue.text.replaceAll(RegExp(r'[^0-9]'), '');
    
    // Limit to 11 digits
    final limitedDigits = digitsOnly.length > 11 ? digitsOnly.substring(0, 11) : digitsOnly;
    
    // Format the phone number
    final formattedText = PhoneFormatter.format(limitedDigits);
    
    // Calculate new cursor position
    int newCursorPosition = formattedText.length;
    
    // Try to maintain cursor position relative to digits
    if (oldValue.selection.baseOffset <= oldValue.text.length) {
      final oldDigitsBeforeCursor = oldValue.text
          .substring(0, oldValue.selection.baseOffset)
          .replaceAll(RegExp(r'[^0-9]'), '');
      
      int digitCount = 0;
      for (int i = 0; i < formattedText.length; i++) {
        if (RegExp(r'[0-9]').hasMatch(formattedText[i])) {
          digitCount++;
          if (digitCount == oldDigitsBeforeCursor.length) {
            newCursorPosition = i + 1;
            break;
          }
        }
      }
    }
    
    return TextEditingValue(
      text: formattedText,
      selection: TextSelection.collapsed(offset: newCursorPosition),
    );
  }
}
