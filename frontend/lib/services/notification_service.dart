import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:permission_handler/permission_handler.dart';
import 'dart:convert';

/// Comprehensive notification service for handling local, push, and browser notifications
class NotificationService {
  static final NotificationService _instance = NotificationService._internal();
  factory NotificationService() => _instance;
  NotificationService._internal();

  // Local notifications
  final FlutterLocalNotificationsPlugin _localNotifications = FlutterLocalNotificationsPlugin();
  
  // Firebase messaging
  FirebaseMessaging? _firebaseMessaging;
  
  // Notification settings
  bool _isInitialized = false;
  bool _localNotificationsEnabled = false;
  bool _pushNotificationsEnabled = false;
  bool _browserNotificationsEnabled = false;
  
  // Callbacks
  Function(String)? _onNotificationTapped;
  Function(Map<String, dynamic>)? _onMessageReceived;

  /// Initialize the notification service
  Future<void> initialize() async {
    if (_isInitialized) return;

    try {
      // Initialize Firebase if available
      await _initializeFirebase();
      
      // Initialize local notifications
      await _initializeLocalNotifications();
      
      // Request permissions
      await _requestPermissions();
      
      _isInitialized = true;
      debugPrint('✅ Notification service initialized successfully');
    } catch (e) {
      debugPrint('❌ Failed to initialize notification service: $e');
      // Continue without notifications if initialization fails
    }
  }

  /// Initialize Firebase Core and Messaging
  Future<void> _initializeFirebase() async {
    try {
      await Firebase.initializeApp();
      _firebaseMessaging = FirebaseMessaging.instance;
      
      // Set up Firebase message handlers
      FirebaseMessaging.onMessage.listen(_handleForegroundMessage);
      FirebaseMessaging.onMessageOpenedApp.listen(_handleBackgroundMessage);
      FirebaseMessaging.onBackgroundMessage(_firebaseMessagingBackgroundHandler);
      
      // Get FCM token
      final token = await _firebaseMessaging!.getToken();
      if (token != null) {
        debugPrint('📱 FCM Token: $token');
        // TODO: Send token to backend for user registration
      }
      
      _pushNotificationsEnabled = true;
      debugPrint('✅ Firebase messaging initialized');
    } catch (e) {
      debugPrint('⚠️ Firebase not available: $e');
      _pushNotificationsEnabled = false;
    }
  }

  /// Initialize local notifications
  Future<void> _initializeLocalNotifications() async {
    try {
      const androidSettings = AndroidInitializationSettings('@mipmap/ic_launcher');
      const iosSettings = DarwinInitializationSettings(
        requestAlertPermission: true,
        requestBadgePermission: true,
        requestSoundPermission: true,
      );
      
      const initSettings = InitializationSettings(
        android: androidSettings,
        iOS: iosSettings,
      );
      
      await _localNotifications.initialize(
        initSettings,
        onDidReceiveNotificationResponse: _handleNotificationResponse,
      );
      
      _localNotificationsEnabled = true;
      debugPrint('✅ Local notifications initialized');
    } catch (e) {
      debugPrint('❌ Failed to initialize local notifications: $e');
      _localNotificationsEnabled = false;
    }
  }

  /// Request notification permissions
  Future<void> _requestPermissions() async {
    try {
      // Request notification permissions
      final notificationStatus = await Permission.notification.request();
      _browserNotificationsEnabled = notificationStatus.isGranted;
      
      if (_browserNotificationsEnabled) {
        debugPrint('✅ Browser notifications permission granted');
      } else {
        debugPrint('⚠️ Browser notifications permission denied');
      }
    } catch (e) {
      debugPrint('❌ Failed to request permissions: $e');
      _browserNotificationsEnabled = false;
    }
  }

  /// Show a local notification
  Future<void> showLocalNotification({
    required String title,
    required String body,
    String? payload,
    NotificationImportance importance = NotificationImportance.normal,
    Duration? timeout,
  }) async {
    if (!_localNotificationsEnabled) {
      debugPrint('⚠️ Local notifications not available');
      return;
    }

    try {
      const androidDetails = AndroidNotificationDetails(
        'queuehub_channel',
        'QueueHub Notifications',
        channelDescription: 'Notifications for queue updates and reminders',
        importance: Importance.high,
        priority: Priority.high,
        showWhen: true,
        enableVibration: true,
        enableLights: true,
        color: Color(0xFF2196F3),
      );
      
      const iosDetails = DarwinNotificationDetails(
        presentAlert: true,
        presentBadge: true,
        presentSound: true,
      );
      
      const details = NotificationDetails(
        android: androidDetails,
        iOS: iosDetails,
      );
      
      await _localNotifications.show(
        DateTime.now().millisecondsSinceEpoch.remainder(100000),
        title,
        body,
        details,
        payload: payload,
      );
      
      debugPrint('📱 Local notification shown: $title');
    } catch (e) {
      debugPrint('❌ Failed to show local notification: $e');
    }
  }

  /// Show a browser notification (if supported)
  Future<void> showBrowserNotification({
    required String title,
    required String body,
    String? icon,
    String? tag,
    Map<String, dynamic>? data,
  }) async {
    if (!_browserNotificationsEnabled) {
      debugPrint('⚠️ Browser notifications not available');
      return;
    }

    try {
      // This would be implemented for web platform
      if (kIsWeb) {
        // Web notification implementation
        debugPrint('🌐 Browser notification: $title');
      }
    } catch (e) {
      debugPrint('❌ Failed to show browser notification: $e');
    }
  }

  /// Send a push notification (requires backend integration)
  Future<void> sendPushNotification({
    required String userId,
    required String title,
    required String body,
    Map<String, dynamic>? data,
  }) async {
    if (!_pushNotificationsEnabled) {
      debugPrint('⚠️ Push notifications not available');
      return;
    }

    try {
      // This would typically be done through the backend
      // For now, we'll just log the attempt
      debugPrint('📤 Push notification request: $title to user $userId');
      
      // TODO: Integrate with backend push notification service
      // await _apiService.sendPushNotification(userId, title, body, data);
    } catch (e) {
      debugPrint('❌ Failed to send push notification: $e');
    }
  }

  /// Handle foreground Firebase messages
  void _handleForegroundMessage(RemoteMessage message) {
    debugPrint('📱 Foreground message received: ${message.notification?.title}');
    
    // Show local notification for foreground messages
    showLocalNotification(
      title: message.notification?.title ?? 'New Message',
      body: message.notification?.body ?? '',
      payload: jsonEncode(message.data),
    );
    
    // Trigger callback if set
    if (_onMessageReceived != null) {
      _onMessageReceived!(message.data);
    }
  }

  /// Handle background Firebase messages
  void _handleBackgroundMessage(RemoteMessage message) {
    debugPrint('📱 Background message received: ${message.notification?.title}');
    
    // Trigger callback if set
    if (_onMessageReceived != null) {
      _onMessageReceived!(message.data);
    }
  }

  /// Handle notification tap
  void _handleNotificationResponse(NotificationResponse response) {
    debugPrint('👆 Notification tapped: ${response.payload}');
    
    if (_onNotificationTapped != null) {
      _onNotificationTapped!(response.payload ?? '');
    }
  }

  /// Set callback for notification taps
  void setNotificationTappedCallback(Function(String) callback) {
    _onNotificationTapped = callback;
  }

  /// Set callback for message received
  void setMessageReceivedCallback(Function(Map<String, dynamic>) callback) {
    _onMessageReceived = callback;
  }

  /// Subscribe to a topic for push notifications
  Future<void> subscribeToTopic(String topic) async {
    if (!_pushNotificationsEnabled || _firebaseMessaging == null) {
      debugPrint('⚠️ Push notifications not available');
      return;
    }

    try {
      await _firebaseMessaging!.subscribeToTopic(topic);
      debugPrint('✅ Subscribed to topic: $topic');
    } catch (e) {
      debugPrint('❌ Failed to subscribe to topic $topic: $e');
    }
  }

  /// Unsubscribe from a topic
  Future<void> unsubscribeFromTopic(String topic) async {
    if (!_pushNotificationsEnabled || _firebaseMessaging == null) {
      debugPrint('⚠️ Push notifications not available');
      return;
    }

    try {
      await _firebaseMessaging!.unsubscribeFromTopic(topic);
      debugPrint('✅ Unsubscribed from topic: $topic');
    } catch (e) {
      debugPrint('❌ Failed to unsubscribe from topic $topic: $e');
    }
  }

  /// Get current notification settings
  Map<String, bool> getNotificationSettings() {
    return {
      'localNotifications': _localNotificationsEnabled,
      'pushNotifications': _pushNotificationsEnabled,
      'browserNotifications': _browserNotificationsEnabled,
    };
  }

  /// Check if notifications are available
  bool get isAvailable => _isInitialized && (_localNotificationsEnabled || _pushNotificationsEnabled || _browserNotificationsEnabled);

  /// Dispose resources
  void dispose() {
    _onNotificationTapped = null;
    _onMessageReceived = null;
  }
}

/// Background message handler for Firebase
@pragma('vm:entry-point')
Future<void> _firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  // This function must be top-level and not in a class
  debugPrint('📱 Background message handler: ${message.notification?.title}');
}

/// Notification importance levels
enum NotificationImportance {
  low,
  normal,
  high,
  urgent,
}
