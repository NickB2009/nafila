import 'package:dio/dio.dart';
import '../models/anonymous_user.dart';
import '../models/public_salon.dart';
import '../config/api_config.dart';
import 'anonymous_user_service.dart';

/// Service for managing anonymous queue operations with backend API
class AnonymousQueueService {
  final AnonymousUserService _userService;
  final Dio _dio;

  AnonymousQueueService({
    required AnonymousUserService userService,
    Dio? dio,
  }) : _userService = userService,
       _dio = dio ?? Dio();

  /// Join a queue anonymously
  Future<AnonymousQueueEntry> joinQueue({
    required PublicSalon salon,
    required String name,
    required String email,
    String? serviceRequested,
    bool emailNotifications = true,
    bool browserNotifications = true,
  }) async {
    // Get or create anonymous user
    AnonymousUser? user = await _userService.getAnonymousUser();
    
    if (user == null) {
      user = await _userService.createAnonymousUser(
        name: name,
        email: email,
      );
    } else if (user.name != name || user.email != email) {
      // Update user info if different
      user = await _userService.updateProfile(
        name: name,
        email: email,
      );
    }

    // Check if already in queue for this salon
    final existingEntry = user.activeQueues
        .where((entry) => entry.salonId == salon.id && entry.isActive)
        .firstOrNull;
    
    if (existingEntry != null) {
      throw Exception('You are already in the queue for ${salon.name}');
    }

    // Prepare request
    final request = JoinQueueRequest(
      salonId: salon.id,
      name: name,
      email: email,
      anonymousUserId: user.id,
      serviceRequested: serviceRequested,
      emailNotifications: emailNotifications,
      browserNotifications: browserNotifications,
    );

    try {
      // Call backend API
      final response = await _dio.post(
        'https://localhost:7126/api/Public/queue/join',
        data: request.toJson(),
        options: Options(
          headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
          },
        ),
      );

      // Parse response and create queue entry
      final queueData = response.data;
      final queueEntry = AnonymousQueueEntry(
        id: queueData['id'],
        anonymousUserId: user.id,
        salonId: salon.id,
        salonName: salon.name,
        position: queueData['position'],
        estimatedWaitMinutes: queueData['estimatedWaitMinutes'],
        joinedAt: DateTime.parse(queueData['joinedAt']),
        lastUpdated: DateTime.now(),
        status: QueueEntryStatus.waiting,
        serviceRequested: serviceRequested,
      );

      // Save to local storage
      await _userService.addQueueEntry(queueEntry);

      return queueEntry;
    } catch (e) {
      // For development: create mock queue entry if API not available
      if (e is DioException && e.response?.statusCode == 404) {
        return _createMockQueueEntry(user, salon, serviceRequested);
      }
      rethrow;
    }
  }

  /// Leave a queue
  Future<void> leaveQueue(String entryId) async {
    try {
      // Call backend API
      await _dio.post(
        'https://localhost:7126/api/Public/queue/leave/$entryId',
        options: Options(
          headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
          },
        ),
      );
    } catch (e) {
      // Continue with local removal even if API call fails
      print('Failed to notify backend of queue leave: $e');
    }

    // Remove from local storage
    await _userService.removeQueueEntry(entryId);
  }

  /// Get current queue status for an entry
  Future<AnonymousQueueEntry?> getQueueStatus(String entryId) async {
    try {
      final response = await _dio.get(
        'https://localhost:7126/api/Public/queue/status/$entryId',
        options: Options(
          headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
          },
        ),
      );

      final data = response.data;
      final user = await _userService.getAnonymousUser();
      if (user == null) return null;

      final updatedEntry = AnonymousQueueEntry(
        id: entryId,
        anonymousUserId: user.id,
        salonId: data['salonId'],
        salonName: data['salonName'],
        position: data['position'],
        estimatedWaitMinutes: data['estimatedWaitMinutes'],
        joinedAt: DateTime.parse(data['joinedAt']),
        lastUpdated: DateTime.now(),
        status: _parseQueueStatus(data['status']),
        serviceRequested: data['serviceRequested'],
      );

      // Update local storage
      await _userService.updateQueueEntry(entryId, updatedEntry);

      return updatedEntry;
    } catch (e) {
      // Return local data if API call fails
      final user = await _userService.getAnonymousUser();
      return user?.activeQueues
          .where((entry) => entry.id == entryId)
          .firstOrNull;
    }
  }

  /// Update contact information for a queue entry
  Future<void> updateContactInfo({
    required String entryId,
    String? name,
    String? email,
    bool? emailNotifications,
    bool? browserNotifications,
  }) async {
    try {
      await _dio.put(
        'https://localhost:7126/api/Public/queue/update/$entryId',
        data: {
          if (name != null) 'name': name,
          if (email != null) 'email': email,
          if (emailNotifications != null) 'emailNotifications': emailNotifications,
          if (browserNotifications != null) 'browserNotifications': browserNotifications,
        },
        options: Options(
          headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
          },
        ),
      );

      // Update local user profile if name/email changed
      if (name != null || email != null) {
        await _userService.updateProfile(
          name: name,
          email: email,
        );
      }
    } catch (e) {
      // Update local data even if API call fails
      if (name != null || email != null) {
        await _userService.updateProfile(
          name: name,
          email: email,
        );
      }
      rethrow;
    }
  }

  /// Get all active queue entries for the current anonymous user
  Future<List<AnonymousQueueEntry>> getActiveQueues() async {
    return await _userService.getActiveQueueEntries();
  }

  /// Get queue history for the current anonymous user
  Future<List<AnonymousQueueEntry>> getQueueHistory() async {
    return await _userService.getQueueHistory();
  }

  /// Check if user can join queue (not already in queue for same salon)
  Future<bool> canJoinQueue(String salonId) async {
    final activeQueues = await getActiveQueues();
    return !activeQueues.any((entry) => 
        entry.salonId == salonId && entry.isActive);
  }

  /// Sync local queue data with backend (call periodically)
  Future<void> syncWithBackend() async {
    final activeQueues = await getActiveQueues();
    
    for (final entry in activeQueues) {
      try {
        await getQueueStatus(entry.id);
      } catch (e) {
        print('Failed to sync queue entry ${entry.id}: $e');
      }
    }
  }

  /// Create a mock queue entry for development when backend is not available
  AnonymousQueueEntry _createMockQueueEntry(
    AnonymousUser user, 
    PublicSalon salon, 
    String? serviceRequested,
  ) {
    return AnonymousQueueEntry(
      id: _userService.generateAnonymousId(),
      anonymousUserId: user.id,
      salonId: salon.id,
      salonName: salon.name,
      position: 3, // Mock position
      estimatedWaitMinutes: 25, // Mock wait time
      joinedAt: DateTime.now(),
      lastUpdated: DateTime.now(),
      status: QueueEntryStatus.waiting,
      serviceRequested: serviceRequested,
    );
  }

  /// Parse queue status from API response
  QueueEntryStatus _parseQueueStatus(String status) {
    switch (status.toLowerCase()) {
      case 'waiting':
        return QueueEntryStatus.waiting;
      case 'called':
        return QueueEntryStatus.called;
      case 'completed':
        return QueueEntryStatus.completed;
      case 'cancelled':
        return QueueEntryStatus.cancelled;
      case 'expired':
        return QueueEntryStatus.expired;
      default:
        return QueueEntryStatus.waiting;
    }
  }

  /// Factory method to create service with dependencies
  static AnonymousQueueService create() {
    return AnonymousQueueService(
      userService: AnonymousUserService(),
    );
  }
} 