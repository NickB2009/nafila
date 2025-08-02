import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import 'package:url_launcher/url_launcher.dart' as launcher;
import 'dart:async';
import '../../models/anonymous_user.dart';
import '../../models/public_salon.dart';
import '../../services/anonymous_queue_service.dart';
import '../../controllers/app_controller.dart';
import '../widgets/bottom_nav_bar.dart';
import '../theme/app_theme.dart';

/// Screen for displaying real-time anonymous queue status with backend data
class AnonymousQueueStatusScreen extends StatefulWidget {
  final AnonymousQueueEntry queueEntry;
  final PublicSalon salon;
  final AnonymousQueueService? queueService;
  final Duration updateInterval;

  const AnonymousQueueStatusScreen({
    super.key,
    required this.queueEntry,
    required this.salon,
    this.queueService,
    this.updateInterval = const Duration(seconds: 30),
  });

  @override
  State<AnonymousQueueStatusScreen> createState() => AnonymousQueueStatusScreenState();
}

class AnonymousQueueStatusScreenState extends State<AnonymousQueueStatusScreen> {
  late AnonymousQueueService _queueService;
  late AnonymousQueueEntry _currentEntry;
  Timer? _updateTimer;
  bool _isLoading = false;
  bool _hasError = false;
  String? _errorMessage;

  @override
  void initState() {
    super.initState();
    _queueService = widget.queueService ?? AnonymousQueueService.create();
    _currentEntry = widget.queueEntry;
    _startPeriodicUpdates();
    _refreshQueueStatus();
  }

  @override
  void dispose() {
    _updateTimer?.cancel();
    super.dispose();
  }

  void _startPeriodicUpdates() {
    _updateTimer = Timer.periodic(widget.updateInterval, (timer) {
      if (mounted && _currentEntry.isActive) {
        _refreshQueueStatus();
      }
    });
  }

  Future<void> _refreshQueueStatus() async {
    await refreshQueueStatus();
  }

  /// Public method for testing
  Future<void> refreshQueueStatus() async {
    if (!mounted) return;

    setState(() {
      _isLoading = true;
      _hasError = false;
      _errorMessage = null;
    });

    try {
      final updatedEntry = await _queueService.getQueueStatus(_currentEntry.id);
      if (updatedEntry != null && mounted) {
        setState(() {
          _currentEntry = updatedEntry;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _isLoading = false;
          _hasError = true;
          _errorMessage = 'Connection error';
        });
      }
    }
  }

  Future<void> _leaveQueue() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Leave Queue'),
        content: const Text('Are you sure you want to leave the queue?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: const Text('Cancel'),
          ),
          TextButton(
            onPressed: () => Navigator.of(context).pop(true),
            style: TextButton.styleFrom(foregroundColor: Colors.red),
            child: const Text('Leave'),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      try {
        await _queueService.leaveQueue(_currentEntry.id);
        if (mounted) {
          // Refresh salon data before going back to show updated queue length
          final appController = Provider.of<AppController>(context, listen: false);
          await appController.anonymous.loadPublicSalons();
          
          Navigator.of(context).pop(); // Go back to salon list
        }
      } catch (e) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Failed to leave queue: ${e.toString()}'),
              backgroundColor: Colors.red,
            ),
          );
        }
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isSmallScreen = MediaQuery.of(context).size.width < 600;

    return Scaffold(
      backgroundColor: theme.colorScheme.surface,
      body: RefreshIndicator(
        onRefresh: _refreshQueueStatus,
        child: SafeArea(
          child: SingleChildScrollView(
            physics: const AlwaysScrollableScrollPhysics(),
            padding: EdgeInsets.all(isSmallScreen ? 16 : 20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _buildHeader(theme, isSmallScreen),
                SizedBox(height: isSmallScreen ? 20 : 24),
                _buildProgressSteps(theme, isSmallScreen),
                SizedBox(height: isSmallScreen ? 16 : 20),
                _buildWaitCard(theme, isSmallScreen),
                SizedBox(height: isSmallScreen ? 16 : 20),
                _buildSalonCard(theme, isSmallScreen),
                SizedBox(height: isSmallScreen ? 16 : 20),
                _buildActionsCard(theme, isSmallScreen),
              ],
            ),
          ),
        ),
      ),
      bottomNavigationBar: const BottomNavBar(currentIndex: 0),
    );
  }

  Widget _buildHeader(ThemeData theme, bool isSmallScreen) {
    return Row(
      children: [
        IconButton(
          onPressed: () async {
            // Refresh salon data before going back to show updated queue length
            final appController = Provider.of<AppController>(context, listen: false);
            await appController.anonymous.loadPublicSalons();
            
            Navigator.of(context).pop();
          },
          icon: const Icon(Icons.arrow_back),
        ),
        Expanded(
          child: Text(
            'Queue Status',
            style: theme.textTheme.headlineSmall?.copyWith(
              fontWeight: FontWeight.bold,
              fontSize: isSmallScreen ? 18 : null,
            ),
          ),
        ),
        if (_hasError)
          Icon(
            Icons.warning,
            color: Colors.orange,
            size: isSmallScreen ? 20 : 24,
          ),
      ],
    );
  }

  Widget _buildProgressSteps(ThemeData theme, bool isSmallScreen) {
    final isWaiting = _currentEntry.status == QueueEntryStatus.waiting;
    final isCalled = _currentEntry.status == QueueEntryStatus.called;
    final isCompleted = _currentEntry.status == QueueEntryStatus.completed;

    return Container(
      padding: EdgeInsets.all(isSmallScreen ? 16 : 20),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            AppTheme.primaryColor,
            AppTheme.primaryColor.withOpacity(0.8),
          ],
        ),
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              _buildStepCircle(theme, true), // Always checked (in queue)
              Expanded(
                child: Divider(
                  color: Colors.white.withOpacity(0.5),
                  thickness: 2,
                ),
              ),
              _buildStepCircle(theme, isCalled || isCompleted),
              Expanded(
                child: Divider(
                  color: Colors.white.withOpacity(0.5),
                  thickness: 2,
                ),
              ),
              _buildStepCircle(theme, isCompleted),
            ],
          ),
          SizedBox(height: isSmallScreen ? 6 : 8),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Flexible(
                child: Text(
                  'In Queue',
                  style: TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.bold,
                    fontSize: isSmallScreen ? 12 : 14,
                  ),
                  textAlign: TextAlign.center,
                ),
              ),
              Flexible(
                child: Text(
                  'Called',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: isSmallScreen ? 12 : 14,
                  ),
                  textAlign: TextAlign.center,
                ),
              ),
              Flexible(
                child: Text(
                  'Service',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: isSmallScreen ? 12 : 14,
                  ),
                  textAlign: TextAlign.center,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildStepCircle(ThemeData theme, bool active) {
    return Container(
      width: 20,
      height: 20,
      decoration: BoxDecoration(
        color: active ? Colors.white : Colors.transparent,
        border: Border.all(color: Colors.white, width: 2),
        shape: BoxShape.circle,
      ),
      child: active
          ? Icon(Icons.check, size: 14, color: AppTheme.primaryColor)
          : null,
    );
  }

  Widget _buildWaitCard(ThemeData theme, bool isSmallScreen) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: theme.colorScheme.background,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: theme.colorScheme.outline.withOpacity(0.2),
        ),
      ),
      child: _buildWaitContent(theme),
    );
  }

  Widget _buildWaitContent(ThemeData theme) {
    switch (_currentEntry.status) {
      case QueueEntryStatus.called:
        return _buildCalledStatus(theme);
      case QueueEntryStatus.completed:
        return _buildCompletedStatus(theme);
      case QueueEntryStatus.cancelled:
      case QueueEntryStatus.expired:
        return _buildInactiveStatus(theme);
      case QueueEntryStatus.waiting:
      default:
        return _buildWaitingStatus(theme);
    }
  }

  Widget _buildWaitingStatus(ThemeData theme) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Text(
              'YOUR WAIT TIME',
              style: theme.textTheme.labelMedium?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
                fontWeight: FontWeight.bold,
                letterSpacing: 1.2,
              ),
            ),
            const SizedBox(width: 8),
            if (_isLoading)
              const SizedBox(
                width: 16,
                height: 16,
                child: CircularProgressIndicator(strokeWidth: 2),
              ),
          ],
        ),
        const SizedBox(height: 8),
        Text(
          '${_currentEntry.estimatedWaitMinutes} min',
          style: theme.textTheme.displayMedium?.copyWith(
            color: AppTheme.primaryColor,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 16),
        Divider(color: theme.dividerColor),
        const SizedBox(height: 8),
        Text(
          'You are in the queue',
          style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
        ),
        const SizedBox(height: 4),
        RichText(
          text: TextSpan(
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
            children: [
              const TextSpan(text: 'You are '),
              TextSpan(
                text: '${_currentEntry.position}º',
                style: const TextStyle(fontWeight: FontWeight.bold),
              ),
              const TextSpan(text: ' in line'),
            ],
          ),
        ),
        if (_hasError) ...[
          const SizedBox(height: 8),
          Text(
            _errorMessage ?? 'Connection error',
            style: theme.textTheme.bodySmall?.copyWith(
              color: Colors.orange,
            ),
          ),
        ],
      ],
    );
  }

  Widget _buildCalledStatus(ThemeData theme) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'YOU\'RE UP!',
          style: theme.textTheme.headlineSmall?.copyWith(
            color: Colors.green,
            fontWeight: FontWeight.bold,
            letterSpacing: 1.2,
          ),
        ),
        const SizedBox(height: 8),
        Text(
          'Please proceed to the salon',
          style: theme.textTheme.bodyLarge?.copyWith(
            fontWeight: FontWeight.w500,
          ),
        ),
        const SizedBox(height: 16),
        Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            color: Colors.green.withOpacity(0.1),
            borderRadius: BorderRadius.circular(8),
          ),
          child: Row(
            children: [
              const Icon(Icons.notifications_active, color: Colors.green),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  'Your turn has arrived! Head to the salon now.',
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: Colors.green.shade800,
                  ),
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildCompletedStatus(ThemeData theme) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'COMPLETED',
          style: theme.textTheme.headlineSmall?.copyWith(
            color: Colors.blue,
            fontWeight: FontWeight.bold,
            letterSpacing: 1.2,
          ),
        ),
        const SizedBox(height: 8),
        Text(
          'Your service is complete',
          style: theme.textTheme.bodyLarge?.copyWith(
            fontWeight: FontWeight.w500,
          ),
        ),
        const SizedBox(height: 16),
        Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            color: Colors.blue.withOpacity(0.1),
            borderRadius: BorderRadius.circular(8),
          ),
          child: Row(
            children: [
              const Icon(Icons.check_circle, color: Colors.blue),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  'Thank you for visiting ${widget.salon.name}!',
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: Colors.blue.shade800,
                  ),
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildInactiveStatus(ThemeData theme) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          _currentEntry.status == QueueEntryStatus.cancelled ? 'CANCELLED' : 'EXPIRED',
          style: theme.textTheme.headlineSmall?.copyWith(
            color: Colors.red,
            fontWeight: FontWeight.bold,
            letterSpacing: 1.2,
          ),
        ),
        const SizedBox(height: 8),
        Text(
          'Your queue entry is no longer active',
          style: theme.textTheme.bodyLarge?.copyWith(
            fontWeight: FontWeight.w500,
          ),
        ),
      ],
    );
  }

  Widget _buildSalonCard(ThemeData theme, bool isSmallScreen) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: theme.colorScheme.background,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: theme.colorScheme.outline.withOpacity(0.2),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
            decoration: BoxDecoration(
              color: AppTheme.primaryColor.withOpacity(0.1),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Text(
              'JOINED QUEUE',
              style: theme.textTheme.labelSmall?.copyWith(
                color: AppTheme.primaryColor,
                fontWeight: FontWeight.bold,
                letterSpacing: 1.1,
              ),
            ),
          ),
          const SizedBox(height: 8),
          Text(
            widget.salon.name,
            style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 4),
          Text(
            widget.salon.address,
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              Text(
                widget.salon.isOpen ? 'Open' : 'Closed',
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: widget.salon.isOpen ? Colors.green : theme.colorScheme.error,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(width: 16),
              Icon(Icons.people, size: 16, color: theme.colorScheme.onSurfaceVariant),
              const SizedBox(width: 4),
              Text('${widget.salon.queueLength} in queue'),
            ],
          ),
          if (_currentEntry.serviceRequested != null) ...[
            const SizedBox(height: 8),
            Row(
              children: [
                Icon(Icons.content_cut, size: 16, color: theme.colorScheme.onSurfaceVariant),
                const SizedBox(width: 4),
                Text('Service: ${_currentEntry.serviceRequested}'),
              ],
            ),
          ],
          const SizedBox(height: 16),
          _buildSalonActions(theme),
        ],
      ),
    );
  }

  Widget _buildSalonActions(ThemeData theme) {
    return Column(
      children: [
        _buildSalonAction(
          theme,
          Icons.navigation,
          'Get Directions',
          onTap: () => _openMaps(),
        ),
        Divider(color: theme.dividerColor),
      ],
    );
  }

  Widget _buildSalonAction(
    ThemeData theme,
    IconData icon,
    String label, {
    VoidCallback? onTap,
  }) {
    return ListTile(
      contentPadding: EdgeInsets.zero,
      leading: Icon(icon, color: AppTheme.primaryColor),
      title: Text(
        label,
        style: theme.textTheme.bodyLarge?.copyWith(
          color: AppTheme.primaryColor,
          fontWeight: FontWeight.w500,
        ),
      ),
      onTap: onTap,
    );
  }

  Widget _buildActionsCard(ThemeData theme, bool isSmallScreen) {
    if (!_currentEntry.isActive) {
      return const SizedBox.shrink();
    }

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: theme.colorScheme.background,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: theme.colorScheme.outline.withOpacity(0.2),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'ACTIONS',
            style: theme.textTheme.labelMedium?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
              fontWeight: FontWeight.bold,
              letterSpacing: 1.2,
            ),
          ),
          const SizedBox(height: 16),
          SizedBox(
            width: double.infinity,
            child: OutlinedButton(
              style: OutlinedButton.styleFrom(
                foregroundColor: Colors.red,
                side: const BorderSide(color: Colors.red, width: 2),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(24),
                ),
                padding: const EdgeInsets.symmetric(vertical: 12),
              ),
              onPressed: _leaveQueue,
              child: const Text(
                'Leave Queue',
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _openMaps() async {
    final lat = widget.salon.latitude;
    final lng = widget.salon.longitude;
    final url = 'https://www.google.com/maps/dir/?api=1&destination=$lat,$lng';

    try {
      await launcher.launchUrl(
        Uri.parse(url),
        mode: launcher.LaunchMode.externalApplication,
      );
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Could not open maps')),
        );
      }
    }
  }


} 