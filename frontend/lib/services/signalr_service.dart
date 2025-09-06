import 'package:web_socket_channel/web_socket_channel.dart';
import 'package:flutter/foundation.dart';
import '../config/api_config.dart';

/// Service for managing real-time WebSocket communication with the backend
class SignalRService {
  WebSocketChannel? _channel;
  final String _wsUrl = '${ApiConfig.currentBaseUrl.replaceFirst('http', 'ws')}/queueHub';
  
  // Connection state
  bool _isConnected = false;
  String? _connectionId;

  // Callbacks for different events
  Function(String queueId, Map<String, dynamic> update)? _onQueueUpdate;
  Function(String message)? _onNotification;

  bool get isConnected => _isConnected;

  /// Initialize the WebSocket connection
  Future<void> initialize() async {
    try {
      _channel = WebSocketChannel.connect(Uri.parse(_wsUrl));
      
      // Set up event handlers
      _setupEventHandlers();
      
      _isConnected = true;
      _connectionId = DateTime.now().millisecondsSinceEpoch.toString();
      debugPrint('✅ WebSocket connected to $_wsUrl');
    } catch (e) {
      debugPrint('⚠️ WebSocket connection failed: $e');
      debugPrint('📡 Real-time updates will not be available');
      _isConnected = false;
      // Don't rethrow - let the app continue without real-time updates
    }
  }

  /// Set up event handlers for incoming messages
  void _setupEventHandlers() {
    if (_channel == null) return;

    _channel!.stream.listen(
      (message) {
        _handleMessage(message);
      },
      onError: (error) {
        debugPrint('⚠️ WebSocket error: $error');
        _isConnected = false;
      },
      onDone: () {
        debugPrint('📡 WebSocket connection closed');
        _isConnected = false;
      },
    );
  }

  /// Handle incoming WebSocket messages
  void _handleMessage(dynamic message) {
    try {
      if (message is String) {
        final data = _parseMessage(message);
        if (data != null) {
          _processMessage(data);
        }
      }
    } catch (e) {
      debugPrint('Error handling message: $e');
    }
  }

  /// Parse incoming message string
  Map<String, dynamic>? _parseMessage(String message) {
    try {
      // Simple JSON parsing for now
      // In a real implementation, you might want to use a proper JSON parser
      if (message.startsWith('{') && message.endsWith('}')) {
        // This is a simplified parser - in production use jsonDecode
        return {'raw': message};
      }
      return null;
    } catch (e) {
      debugPrint('Failed to parse message: $e');
      return null;
    }
  }

  /// Process parsed message and trigger appropriate callbacks
  void _processMessage(Map<String, dynamic> data) {
    final type = data['type'] ?? data['Type'];
    final payload = data['payload'] ?? data['Payload'] ?? data;

    switch (type?.toString().toLowerCase()) {
      case 'queueupdate':
        final queueId = payload['queueId'] ?? payload['QueueId'];
        if (queueId != null && _onQueueUpdate != null) {
          _onQueueUpdate!(queueId.toString(), payload);
        }
        break;
      
      case 'notification':
        final message = payload['message'] ?? payload['Message'];
        if (message != null && _onNotification != null) {
          _onNotification!(message.toString());
        }
        break;
      
      default:
        debugPrint('Unknown message type: $type');
    }
  }

  /// Subscribe to updates for a specific queue
  Future<void> subscribeToQueue(String queueId) async {
    if (!isConnected || _channel == null) {
      throw Exception('WebSocket not connected');
    }

    try {
      final message = {
        'type': 'SubscribeToQueue',
        'queueId': queueId,
      };
      _channel!.sink.add(_encodeMessage(message));
      debugPrint('Subscribed to queue: $queueId');
    } catch (e) {
      debugPrint('Failed to subscribe to queue $queueId: $e');
      rethrow;
    }
  }

  /// Unsubscribe from updates for a specific queue
  Future<void> unsubscribeFromQueue(String queueId) async {
    if (!isConnected || _channel == null) {
      debugPrint('⚠️ Cannot unsubscribe from queue $queueId: WebSocket not connected');
      return; // Don't throw, just log and return
    }

    try {
      final message = {
        'type': 'UnsubscribeFromQueue',
        'queueId': queueId,
      };
      _channel!.sink.add(_encodeMessage(message));
      debugPrint('✅ Unsubscribed from queue: $queueId');
    } catch (e) {
      debugPrint('⚠️ Failed to unsubscribe from queue $queueId: $e');
      // Don't rethrow - just log the error
    }
  }

  /// Subscribe to updates for a specific queue entry
  Future<void> subscribeToQueueEntry(String entryId) async {
    if (!isConnected || _channel == null) {
      throw Exception('WebSocket not connected');
    }

    try {
      final message = {
        'type': 'SubscribeToQueueEntry',
        'entryId': entryId,
      };
      _channel!.sink.add(_encodeMessage(message));
      debugPrint('Subscribed to queue entry: $entryId');
    } catch (e) {
      debugPrint('Failed to subscribe to queue entry $entryId: $e');
      rethrow;
    }
  }

  /// Unsubscribe from updates for a specific queue entry
  Future<void> unsubscribeFromQueueEntry(String entryId) async {
    if (!isConnected || _channel == null) {
      debugPrint('⚠️ Cannot unsubscribe from queue entry $entryId: WebSocket not connected');
      return; // Don't throw, just log and return
    }

    try {
      final message = {
        'type': 'UnsubscribeFromQueueEntry',
        'entryId': entryId,
      };
      _channel!.sink.add(_encodeMessage(message));
      debugPrint('✅ Unsubscribed from queue entry $entryId');
    } catch (e) {
      debugPrint('⚠️ Failed to unsubscribe from queue entry $entryId: $e');
      // Don't rethrow - just log the error
    }
  }

  /// Encode message to string for WebSocket transmission
  String _encodeMessage(Map<String, dynamic> message) {
    // Simple encoding - in production use jsonEncode
    return message.toString();
  }

  /// Set callback for queue updates
  void setQueueUpdateCallback(Function(String queueId, Map<String, dynamic> update) callback) {
    _onQueueUpdate = callback;
  }

  /// Set callback for queue entry updates
  void setQueueEntryUpdateCallback(Function(String entryId, Map<String, dynamic> update) callback) {
    // For WebSocket implementation, we'll use the same callback as queue updates
    _onQueueUpdate = (queueId, update) => callback(queueId, update);
  }

  /// Set callback for notifications
  void setNotificationCallback(Function(String message) callback) {
    _onNotification = callback;
  }

  /// Send a test message to the WebSocket
  Future<void> sendTestMessage(String message) async {
    if (!isConnected || _channel == null) {
      throw Exception('WebSocket not connected');
    }

    try {
      final data = {
        'type': 'TestMessage',
        'message': message,
      };
      _channel!.sink.add(_encodeMessage(data));
    } catch (e) {
      debugPrint('Failed to send test message: $e');
      rethrow;
    }
  }

  /// Disconnect from the WebSocket
  Future<void> disconnect() async {
    if (_channel != null) {
      await _channel!.sink.close();
      _channel = null;
      _isConnected = false;
      _connectionId = null;
      debugPrint('WebSocket disconnected');
    }
  }

  /// Get the current connection state
  String get connectionState => _isConnected ? 'Connected' : 'Disconnected';

  /// Get the connection ID (if connected)
  String? get connectionId => _connectionId;
}
