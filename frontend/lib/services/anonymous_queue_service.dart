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
      // Primary endpoint - try the expected path
      final primaryUrl = ApiConfig.getUrl('${ApiConfig.publicEndpoint}/queue/join');
      print('🔗 Attempting queue join at: $primaryUrl');
      print('📦 Request data: ${request.toJson()}');
      
      final response = await _dio.post(
        primaryUrl,
        data: request.toJson(),
        options: Options(
          headers: ApiConfig.defaultHeaders,
        ),
      );

      // Parse response using the correct API response model
      final joinResult = AnonymousJoinResult.fromJson(response.data);
      
      if (!joinResult.success) {
        final errorMessage = joinResult.errors?.join(', ') ?? 
                           joinResult.fieldErrors?.values.join(', ') ?? 
                           'Failed to join queue';
        throw Exception(errorMessage);
      }
      
      if (joinResult.id == null) {
        throw Exception('Queue entry ID not returned from server');
      }
      
      // Create queue entry from API response
      final queueEntry = AnonymousQueueEntry(
        id: joinResult.id!,
        anonymousUserId: user.id,
        salonId: salon.id,
        salonName: salon.name,
        position: joinResult.position,
        estimatedWaitMinutes: joinResult.estimatedWaitMinutes,
        joinedAt: joinResult.joinedAt ?? DateTime.now(),
        lastUpdated: DateTime.now(),
        status: _parseQueueStatus(joinResult.status ?? 'waiting'),
        serviceRequested: serviceRequested,
      );

      // Save to local storage
      await _userService.addQueueEntry(queueEntry);

      return queueEntry;
    } catch (e) {
      print('❌ Primary endpoint failed: $e');
      
      // If 404, try alternative endpoint paths
      if (e is DioException && e.response?.statusCode == 404) {
        print('🔄 Trying alternative endpoint paths...');
        
        // Alternative paths to try
        final alternativeUrls = [
          '${ApiConfig.currentBaseUrl}/Public/queue/join', // Without /api prefix
          '${ApiConfig.currentBaseUrl}/api/Public/queue/join', // Direct path
          '${ApiConfig.currentBaseUrl}/queue/join', // Simplified path
        ];
        
        for (final altUrl in alternativeUrls) {
          try {
            print('🔗 Trying alternative: $altUrl');
            final response = await _dio.post(
              altUrl,
              data: request.toJson(),
              options: Options(
                headers: ApiConfig.defaultHeaders,
              ),
            );
            
            print('✅ Success with alternative URL!');
            
            // Parse response using the correct API response model
            final joinResult = AnonymousJoinResult.fromJson(response.data);
            
            if (!joinResult.success) {
              final errorMessage = joinResult.errors?.join(', ') ?? 
                                 joinResult.fieldErrors?.values.join(', ') ?? 
                                 'Failed to join queue';
              throw Exception(errorMessage);
            }
            
            if (joinResult.id == null) {
              throw Exception('Queue entry ID not returned from server');
            }
            
            // Create queue entry from API response
            final queueEntry = AnonymousQueueEntry(
              id: joinResult.id!,
              anonymousUserId: user.id,
              salonId: salon.id,
              salonName: salon.name,
              position: joinResult.position,
              estimatedWaitMinutes: joinResult.estimatedWaitMinutes,
              joinedAt: joinResult.joinedAt ?? DateTime.now(),
              lastUpdated: DateTime.now(),
              status: _parseQueueStatus(joinResult.status ?? 'waiting'),
              serviceRequested: serviceRequested,
            );

            // Save to local storage
            await _userService.addQueueEntry(queueEntry);

            return queueEntry;
          } catch (altError) {
            print('❌ Alternative URL failed: $altError');
            continue; // Try next URL
          }
        }
      }
      
      // Handle API errors with specific messages
      if (e is DioException) {
        print('🔍 DioException details:');
        print('   Status: ${e.response?.statusCode}');
        print('   Message: ${e.message}');
        print('   Response: ${e.response?.data}');
        print('   URL: ${e.requestOptions.uri}');
        
        // Check for CORS errors (common in web development)
        if (e.type == DioExceptionType.connectionError || 
            e.message?.toLowerCase().contains('cors') == true ||
            e.message?.toLowerCase().contains('cross-origin') == true ||
            e.message?.toLowerCase().contains('blocked') == true ||
            e.message?.toLowerCase().contains('preflight') == true) {
          throw Exception(
            'CORS Error: Cannot connect to API from web browser.\n\n'
            'Solutions:\n'
            '1. Run Chrome with CORS disabled:\n'
            '   chrome.exe --user-data-dir="C:/chrome-dev-session" --disable-web-security\n'
            '2. Use Flutter mobile/desktop app instead\n'
            '3. Wait for production deployment with proper CORS setup'
          );
        }
        
        if (e.response?.statusCode == 404) {
          throw Exception('Queue service endpoint not found. The API might be under maintenance. Please try again later.');
        } else if (e.response?.statusCode == 400) {
          final responseData = e.response?.data;
          if (responseData is Map && responseData.containsKey('errors')) {
            final errors = responseData['errors'];
            if (errors is Map) {
              // Handle field-specific errors
              final fieldErrors = <String>[];
              errors.forEach((field, messages) {
                if (messages is List) {
                  for (final message in messages) {
                    fieldErrors.add('$field: $message');
                  }
                } else {
                  fieldErrors.add('$field: $messages');
                }
              });
              if (fieldErrors.isNotEmpty) {
                throw Exception('Erro de validação:\n${fieldErrors.join('\n')}');
              }
            }
            throw Exception('Dados inválidos: $errors');
          }
          throw Exception('Dados da solicitação inválidos. Verifique suas informações e tente novamente.');
        } else if (e.response?.statusCode == 500) {
          throw Exception('Erro do servidor. Parece haver um problema de configuração no backend. Tente novamente mais tarde.');
        } else {
          throw Exception('Failed to join queue: ${e.message ?? 'Network error'}');
        }
      }
      throw Exception('Failed to join queue: ${e.toString()}');
    }
  }

  /// Leave a queue
  Future<void> leaveQueue(String entryId) async {
    try {
      // Call backend API
      await _dio.post(
        ApiConfig.getUrl('${ApiConfig.publicEndpoint}/queue/leave/$entryId'),
        options: Options(
          headers: ApiConfig.defaultHeaders,
        ),
      );

      // Remove from local storage
      await _userService.removeQueueEntry(entryId);
    } catch (e) {
      // Handle API errors with specific messages
      if (e is DioException) {
        if (e.response?.statusCode == 404) {
          throw Exception('Queue entry not found. It may have already been removed.');
        } else if (e.response?.statusCode == 500) {
          throw Exception('Server error while leaving queue. Please try again.');
        } else if (e.response?.statusCode == 400) {
          throw Exception('Invalid request to leave queue.');
        } else {
          throw Exception('Failed to leave queue: ${e.message}');
        }
      }
      throw Exception('Failed to leave queue: ${e.toString()}');
    }
  }

  /// Get current queue status for an entry
  Future<AnonymousQueueEntry?> getQueueStatus(String entryId) async {
    try {
      final response = await _dio.get(
        ApiConfig.getUrl('${ApiConfig.publicEndpoint}/queue/entry-status/$entryId'),
        options: Options(
          headers: ApiConfig.defaultHeaders,
        ),
      );

      final data = response.data;
      final user = await _userService.getAnonymousUser();
      if (user == null) return null;

      final updatedEntry = AnonymousQueueEntry(
        id: entryId,
        anonymousUserId: user.id,
        salonId: data['salonId'] ?? data['SalonId'] ?? 'unknown',
        salonName: data['salonName'] ?? data['SalonName'] ?? 'Unknown Salon',
        position: data['position'] ?? data['Position'] ?? 1,
        estimatedWaitMinutes: data['estimatedWaitMinutes'] ?? data['EstimatedWaitMinutes'] ?? 10,
        joinedAt: DateTime.parse(data['joinedAt'] ?? data['JoinedAt'] ?? DateTime.now().toIso8601String()),
        lastUpdated: DateTime.now(),
        status: _parseQueueStatus(data['status'] ?? data['Status'] ?? 'waiting'),
        serviceRequested: data['serviceRequested'] ?? data['ServiceRequested'],
      );

      // Update local storage
      await _userService.updateQueueEntry(entryId, updatedEntry);

      return updatedEntry;
    } catch (e) {
      throw Exception('Failed to get queue status: ${e.toString()}');
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
        ApiConfig.getUrl('${ApiConfig.publicEndpoint}/queue/update/$entryId'),
        data: {
          if (name != null) 'name': name,
          if (email != null) 'email': email,
          if (emailNotifications != null) 'emailNotifications': emailNotifications,
          if (browserNotifications != null) 'browserNotifications': browserNotifications,
        },
        options: Options(
          headers: ApiConfig.defaultHeaders,
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
      throw Exception('Failed to update contact info: ${e.toString()}');
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