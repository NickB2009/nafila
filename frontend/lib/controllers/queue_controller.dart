import 'package:flutter/foundation.dart';
import '../services/queue_service.dart';
import '../models/queue_models.dart';

/// Controller for managing queue-related state and operations
class QueueController extends ChangeNotifier {
  final QueueService _queueService;

  QueueController({required QueueService queueService}) : _queueService = queueService;

  // State management
  bool _isLoading = false;
  String? _error;
  Map<String, dynamic>? _currentQueue;
  List<dynamic> _queueEntries = [];

  // Getters
  bool get isLoading => _isLoading;
  String? get error => _error;
  Map<String, dynamic>? get currentQueue => _currentQueue;
  List<dynamic> get queueEntries => _queueEntries;

  /// Sets loading state and notifies listeners
  void _setLoading(bool loading) {
    _isLoading = loading;
    notifyListeners();
  }

  /// Sets error state and notifies listeners
  void _setError(String? error) {
    _error = error;
    notifyListeners();
  }

  /// Clears error state
  void clearError() {
    _setError(null);
  }

  /// Adds a new queue
  Future<bool> addQueue(AddQueueRequest request) async {
    try {
      _setLoading(true);
      _setError(null);

      final result = await _queueService.addQueue(request);
      
      if (result['success'] == true) {
        // Optionally load the created queue
        if (result['queueId'] != null) {
          await getQueue(result['queueId']);
        }
        return true;
      } else {
        _setError(result['message'] ?? 'Failed to create queue');
        return false;
      }
    } catch (e) {
      _setError('Error creating queue: ${e.toString()}');
      return false;
    } finally {
      _setLoading(false);
    }
  }

  /// Gets queue details
  Future<bool> getQueue(String queueId) async {
    try {
      _setLoading(true);
      _setError(null);

      final result = await _queueService.getQueue(queueId);
      _currentQueue = result;
      return true;
    } catch (e) {
      _setError('Error loading queue: ${e.toString()}');
      return false;
    } finally {
      _setLoading(false);
    }
  }

  /// Joins a queue
  Future<bool> joinQueue(String queueId, JoinQueueRequest request) async {
    try {
      _setLoading(true);
      _setError(null);

      final result = await _queueService.joinQueue(queueId, request);
      
      if (result['queueEntryId'] != null) {
        // Refresh queue data
        await getQueue(queueId);
        return true;
      } else {
        _setError(result['message'] ?? 'Failed to join queue');
        return false;
      }
    } catch (e) {
      _setError('Error joining queue: ${e.toString()}');
      return false;
    } finally {
      _setLoading(false);
    }
  }

  /// Calls next customer in queue
  Future<bool> callNext(String queueId, CallNextRequest request) async {
    try {
      _setLoading(true);
      _setError(null);

      final result = await _queueService.callNext(queueId, request);
      
      if (result['calledCustomer'] != null) {
        // Refresh queue data
        await getQueue(queueId);
        return true;
      } else {
        _setError(result['message'] ?? 'Failed to call next customer');
        return false;
      }
    } catch (e) {
      _setError('Error calling next customer: ${e.toString()}');
      return false;
    } finally {
      _setLoading(false);
    }
  }

  /// Checks in a customer
  Future<bool> checkIn(String queueId, CheckInRequest request) async {
    try {
      _setLoading(true);
      _setError(null);

      final result = await _queueService.checkIn(queueId, request);
      
      if (result['success'] == true) {
        // Refresh queue data
        await getQueue(queueId);
        return true;
      } else {
        _setError(result['message'] ?? 'Failed to check in');
        return false;
      }
    } catch (e) {
      _setError('Error checking in: ${e.toString()}');
      return false;
    } finally {
      _setLoading(false);
    }
  }

  /// Finishes a service
  Future<bool> finishService(String queueId, FinishRequest request) async {
    try {
      _setLoading(true);
      _setError(null);

      final result = await _queueService.finishService(queueId, request);
      
      if (result['success'] == true) {
        // Refresh queue data
        await getQueue(queueId);
        return true;
      } else {
        _setError(result['message'] ?? 'Failed to finish service');
        return false;
      }
    } catch (e) {
      _setError('Error finishing service: ${e.toString()}');
      return false;
    } finally {
      _setLoading(false);
    }
  }

  /// Cancels a queue entry
  Future<bool> cancelQueue(String queueId, CancelQueueRequest request) async {
    try {
      _setLoading(true);
      _setError(null);

      final result = await _queueService.cancelQueue(queueId, request);
      
      if (result['success'] == true) {
        // Refresh queue data
        await getQueue(queueId);
        return true;
      } else {
        _setError(result['message'] ?? 'Failed to cancel queue entry');
        return false;
      }
    } catch (e) {
      _setError('Error canceling queue entry: ${e.toString()}');
      return false;
    } finally {
      _setLoading(false);
    }
  }

  /// Gets wait time for a queue entry
  Future<Map<String, dynamic>?> getWaitTime(String queueId, String entryId) async {
    try {
      _setLoading(true);
      _setError(null);

      final result = await _queueService.getWaitTime(queueId, entryId);
      return result;
    } catch (e) {
      _setError('Error getting wait time: ${e.toString()}');
      return null;
    } finally {
      _setLoading(false);
    }
  }

  /// Saves haircut details
  Future<bool> saveHaircutDetails(String entryId, SaveHaircutDetailsRequest request) async {
    try {
      _setLoading(true);
      _setError(null);

      final result = await _queueService.saveHaircutDetails(entryId, request);
      
      if (result.success) {
        return true;
      } else {
        _setError(result.errors?.join(', ') ?? 'Failed to save haircut details');
        return false;
      }
    } catch (e) {
      _setError('Error saving haircut details: ${e.toString()}');
      return false;
    } finally {
      _setLoading(false);
    }
  }

  /// Refreshes current queue data
  Future<void> refresh() async {
    if (_currentQueue != null && _currentQueue!['queueId'] != null) {
      await getQueue(_currentQueue!['queueId']);
    }
  }

  /// Clears all state
  void clear() {
    _currentQueue = null;
    _queueEntries = [];
    _error = null;
    _isLoading = false;
    notifyListeners();
  }
}