import 'package:shared_preferences/shared_preferences.dart';

/// Utility class for caching phone numbers locally
class PhoneCache {
  static const String _phoneNumberKey = 'cached_phone_number';
  static const String _recentPhonesKey = 'recent_phone_numbers';
  static const int _maxRecentPhones = 5;

  /// Save the most recent phone number
  static Future<void> savePhoneNumber(String phoneNumber) async {
    if (phoneNumber.trim().isEmpty) return;
    
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_phoneNumberKey, phoneNumber.trim());
    
    // Also add to recent phones list
    await _addToRecentPhones(phoneNumber.trim());
  }

  /// Get the most recent phone number
  static Future<String?> getLastPhoneNumber() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_phoneNumberKey);
  }

  /// Add phone number to recent phones list
  static Future<void> _addToRecentPhones(String phoneNumber) async {
    final prefs = await SharedPreferences.getInstance();
    List<String> recentPhones = prefs.getStringList(_recentPhonesKey) ?? [];
    
    // Remove if already exists to avoid duplicates
    recentPhones.remove(phoneNumber);
    
    // Add to beginning of list
    recentPhones.insert(0, phoneNumber);
    
    // Keep only the most recent phones
    if (recentPhones.length > _maxRecentPhones) {
      recentPhones = recentPhones.take(_maxRecentPhones).toList();
    }
    
    await prefs.setStringList(_recentPhonesKey, recentPhones);
  }

  /// Get list of recent phone numbers
  static Future<List<String>> getRecentPhoneNumbers() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getStringList(_recentPhonesKey) ?? [];
  }

  /// Clear all cached phone numbers
  static Future<void> clearCache() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_phoneNumberKey);
    await prefs.remove(_recentPhonesKey);
  }

  /// Check if phone number exists in recent list
  static Future<bool> isRecentPhone(String phoneNumber) async {
    final recentPhones = await getRecentPhoneNumbers();
    return recentPhones.contains(phoneNumber.trim());
  }
}
