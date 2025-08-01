import 'dart:convert';
import 'dart:html' as html;
import 'package:uuid/uuid.dart';
import '../models/anonymous_user.dart';

/// Service for managing anonymous user data in browser localStorage
class AnonymousUserService {
  static const String _storageKey = 'eutonafila_anonymous_user';
  static const String _sessionKey = 'eutonafila_session_backup';
  static const Uuid _uuid = Uuid();

  /// Get the current anonymous user from localStorage
  Future<AnonymousUser?> getAnonymousUser() async {
    try {
      final storedData = html.window.localStorage[_storageKey];
      if (storedData == null || storedData.isEmpty) {
        return null;
      }

      final json = jsonDecode(storedData);
      return AnonymousUser.fromJson(json);
    } catch (e) {
      print('Error loading anonymous user: $e');
      return null;
    }
  }

  /// Create a new anonymous user
  Future<AnonymousUser> createAnonymousUser({
    required String name,
    required String email,
    AnonymousUserPreferences? preferences,
  }) async {
    final user = AnonymousUser(
      id: _uuid.v4(),
      name: name,
      email: email,
      createdAt: DateTime.now(),
      preferences: preferences ?? AnonymousUserPreferences.defaultPreferences(),
      activeQueues: [],
      queueHistory: [],
    );

    await saveAnonymousUser(user);
    return user;
  }

  /// Save anonymous user to localStorage
  Future<void> saveAnonymousUser(AnonymousUser user) async {
    try {
      final jsonString = jsonEncode(user.toJson());
      html.window.localStorage[_storageKey] = jsonString;
      
      // Also save to sessionStorage as backup
      html.window.sessionStorage[_sessionKey] = jsonString;
    } catch (e) {
      print('Error saving anonymous user: $e');
      rethrow;
    }
  }

  /// Update anonymous user profile
  Future<AnonymousUser> updateProfile({
    String? name,
    String? email,
    AnonymousUserPreferences? preferences,
  }) async {
    final currentUser = await getAnonymousUser();
    if (currentUser == null) {
      throw Exception('No anonymous user found');
    }

    final updatedUser = currentUser.copyWith(
      name: name,
      email: email,
      preferences: preferences,
    );

    await saveAnonymousUser(updatedUser);
    return updatedUser;
  }

  /// Add a queue entry to the anonymous user
  Future<AnonymousUser> addQueueEntry(AnonymousQueueEntry entry) async {
    final currentUser = await getAnonymousUser();
    if (currentUser == null) {
      throw Exception('No anonymous user found');
    }

    final updatedQueues = List<AnonymousQueueEntry>.from(currentUser.activeQueues)
      ..add(entry);

    final updatedUser = currentUser.copyWith(activeQueues: updatedQueues);
    await saveAnonymousUser(updatedUser);
    return updatedUser;
  }

  /// Update a queue entry
  Future<AnonymousUser> updateQueueEntry(String entryId, AnonymousQueueEntry updatedEntry) async {
    final currentUser = await getAnonymousUser();
    if (currentUser == null) {
      throw Exception('No anonymous user found');
    }

    final updatedQueues = currentUser.activeQueues
        .map((entry) => entry.id == entryId ? updatedEntry : entry)
        .toList();

    final updatedUser = currentUser.copyWith(activeQueues: updatedQueues);
    await saveAnonymousUser(updatedUser);
    return updatedUser;
  }

  /// Move a queue entry from active to history
  Future<AnonymousUser> moveToHistory(String entryId) async {
    final currentUser = await getAnonymousUser();
    if (currentUser == null) {
      throw Exception('No anonymous user found');
    }

    final entryToMove = currentUser.activeQueues.firstWhere(
      (entry) => entry.id == entryId,
      orElse: () => throw Exception('Queue entry not found'),
    );

    final updatedActiveQueues = currentUser.activeQueues
        .where((entry) => entry.id != entryId)
        .toList();

    final updatedHistory = List<AnonymousQueueEntry>.from(currentUser.queueHistory)
      ..add(entryToMove);

    final updatedUser = currentUser.copyWith(
      activeQueues: updatedActiveQueues,
      queueHistory: updatedHistory,
    );

    await saveAnonymousUser(updatedUser);
    return updatedUser;
  }

  /// Remove a queue entry completely
  Future<AnonymousUser> removeQueueEntry(String entryId) async {
    final currentUser = await getAnonymousUser();
    if (currentUser == null) {
      throw Exception('No anonymous user found');
    }

    final updatedQueues = currentUser.activeQueues
        .where((entry) => entry.id != entryId)
        .toList();

    final updatedUser = currentUser.copyWith(activeQueues: updatedQueues);
    await saveAnonymousUser(updatedUser);
    return updatedUser;
  }

  /// Get all active queue entries
  Future<List<AnonymousQueueEntry>> getActiveQueueEntries() async {
    final user = await getAnonymousUser();
    return user?.activeQueues ?? [];
  }

  /// Get queue history
  Future<List<AnonymousQueueEntry>> getQueueHistory() async {
    final user = await getAnonymousUser();
    return user?.queueHistory ?? [];
  }

  /// Check if user exists
  Future<bool> hasAnonymousUser() async {
    final user = await getAnonymousUser();
    return user != null;
  }

  /// Clear all anonymous user data
  Future<void> clearAnonymousUser() async {
    html.window.localStorage.remove(_storageKey);
    html.window.sessionStorage.remove(_sessionKey);
  }

  /// Generate unique anonymous ID
  String generateAnonymousId() {
    return _uuid.v4();
  }

  /// Export user data (for backup or migration)
  Future<String> exportUserData() async {
    final user = await getAnonymousUser();
    if (user == null) {
      throw Exception('No anonymous user found');
    }
    return jsonEncode(user.toJson());
  }

  /// Import user data (for recovery)
  Future<AnonymousUser> importUserData(String jsonData) async {
    try {
      final json = jsonDecode(jsonData);
      final user = AnonymousUser.fromJson(json);
      await saveAnonymousUser(user);
      return user;
    } catch (e) {
      throw Exception('Invalid user data format: $e');
    }
  }

  /// Check if localStorage is available
  bool get isStorageAvailable {
    try {
      const testKey = '__test_storage__';
      html.window.localStorage[testKey] = 'test';
      html.window.localStorage.remove(testKey);
      return true;
    } catch (e) {
      return false;
    }
  }

  /// Get storage usage estimate (in bytes)
  int getStorageUsage() {
    try {
      final data = html.window.localStorage[_storageKey];
      return data?.length ?? 0;
    } catch (e) {
      return 0;
    }
  }

  /// Clean up expired entries (call periodically)
  Future<AnonymousUser?> cleanupExpiredEntries() async {
    final user = await getAnonymousUser();
    if (user == null) return null;

    final now = DateTime.now();
    const maxAge = Duration(days: 7); // Clean up completed entries older than 7 days

    final activeQueues = user.activeQueues
        .where((entry) => entry.isActive)
        .toList();

    final recentHistory = user.queueHistory
        .where((entry) => now.difference(entry.joinedAt) <= maxAge)
        .toList();

    if (activeQueues.length != user.activeQueues.length ||
        recentHistory.length != user.queueHistory.length) {
      final cleanedUser = user.copyWith(
        activeQueues: activeQueues,
        queueHistory: recentHistory,
      );
      await saveAnonymousUser(cleanedUser);
      return cleanedUser;
    }

    return user;
  }
} 