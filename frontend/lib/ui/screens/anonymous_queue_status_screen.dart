import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:url_launcher/url_launcher.dart' as launcher;
import 'dart:async';
import '../../models/anonymous_user.dart';
import '../../models/public_salon.dart';
import '../../services/anonymous_queue_service.dart';
import '../../controllers/app_controller.dart';
import '../widgets/bottom_nav_bar.dart';

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
          _errorMessage = 'Erro de conexão';
        });
      }
    }
  }

  Future<void> _leaveQueue() async {
    final theme = Theme.of(context);
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Sair da fila'),
        content: const Text('Tem certeza que deseja sair da fila?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: const Text('Cancelar'),
          ),
          TextButton(
            onPressed: () => Navigator.of(context).pop(true),
            style: TextButton.styleFrom(foregroundColor: theme.colorScheme.error),
            child: const Text('Sair'),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      try {
        await _queueService.leaveQueue(_currentEntry.id);
        if (mounted) {
          final appController = Provider.of<AppController>(context, listen: false);
          await appController.anonymous.loadPublicSalons();
          Navigator.of(context).pop();
        }
      } catch (e) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Falha ao sair da fila: ${e.toString()}'),
              backgroundColor: theme.colorScheme.error,
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
    final colors = theme.colorScheme;
    return Row(
      children: [
        IconButton(
          onPressed: () async {
            final appController = Provider.of<AppController>(context, listen: false);
            await appController.anonymous.loadPublicSalons();
            Navigator.of(context).pop();
          },
          icon: const Icon(Icons.arrow_back),
        ),
        Expanded(
          child: Text(
            'Status da Fila',
            style: theme.textTheme.headlineSmall?.copyWith(
              fontWeight: FontWeight.bold,
              fontSize: isSmallScreen ? 18 : null,
            ),
          ),
        ),
        if (_hasError)
          Icon(
            Icons.warning,
            color: colors.tertiary,
            size: isSmallScreen ? 20 : 24,
          ),
      ],
    );
  }

  Widget _buildProgressSteps(ThemeData theme, bool isSmallScreen) {
    final cs = theme.colorScheme;
    final isCalled = _currentEntry.status == QueueEntryStatus.called;
    final isCompleted = _currentEntry.status == QueueEntryStatus.completed;

    return Container(
      padding: EdgeInsets.all(isSmallScreen ? 16 : 20),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            cs.primary,
            cs.primary.withOpacity(0.8),
          ],
        ),
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              _buildStepCircle(theme, true),
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
                  'Na fila',
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
                  'Chamado',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: isSmallScreen ? 12 : 14,
                  ),
                  textAlign: TextAlign.center,
                ),
              ),
              Flexible(
                child: Text(
                  'Atendimento',
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
    final cs = theme.colorScheme;
    return Container(
      width: 20,
      height: 20,
      decoration: BoxDecoration(
        color: active ? Colors.white : Colors.transparent,
        border: Border.all(color: Colors.white, width: 2),
        shape: BoxShape.circle,
      ),
      child: active ? Icon(Icons.check, size: 14, color: cs.primary) : null,
    );
  }

  Widget _buildWaitCard(ThemeData theme, bool isSmallScreen) {
    final cs = theme.colorScheme;
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: cs.surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: cs.outline.withOpacity(0.2),
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
        return _buildWaitingStatus(theme);
    }
  }

  Widget _buildWaitingStatus(ThemeData theme) {
    final cs = theme.colorScheme;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Text(
              'SEU TEMPO DE ESPERA',
              style: theme.textTheme.labelMedium?.copyWith(
                color: cs.onSurfaceVariant,
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
            color: cs.primary,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 16),
        Divider(color: theme.dividerColor),
        const SizedBox(height: 8),
        Text(
          'Você está na fila',
          style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
        ),
        const SizedBox(height: 4),
        RichText(
          text: TextSpan(
            style: theme.textTheme.bodyMedium?.copyWith(
              color: cs.onSurfaceVariant,
            ),
            children: [
              const TextSpan(text: 'Você é o '),
              TextSpan(
                text: '${_currentEntry.position}º',
                style: const TextStyle(fontWeight: FontWeight.bold),
              ),
              const TextSpan(text: ' da fila'),
            ],
          ),
        ),
        if (_hasError) ...[
          const SizedBox(height: 8),
          Text(
            _errorMessage ?? 'Erro de conexão',
            style: theme.textTheme.bodySmall?.copyWith(
              color: cs.tertiary,
            ),
          ),
        ],
      ],
    );
  }

  Widget _buildCalledStatus(ThemeData theme) {
    final cs = theme.colorScheme;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'SUA VEZ!',
          style: theme.textTheme.headlineSmall?.copyWith(
            color: cs.primary,
            fontWeight: FontWeight.bold,
            letterSpacing: 1.2,
          ),
        ),
        const SizedBox(height: 8),
        Text(
          'Por favor, dirija-se ao salão',
          style: theme.textTheme.bodyLarge?.copyWith(
            fontWeight: FontWeight.w500,
          ),
        ),
        const SizedBox(height: 16),
        Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            color: cs.primary.withOpacity(0.1),
            borderRadius: BorderRadius.circular(8),
          ),
          child: Row(
            children: [
              Icon(Icons.notifications_active, color: cs.primary),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  'Chegou a sua vez! Vá ao salão agora.',
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: cs.primary,
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
    final cs = theme.colorScheme;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'CONCLUÍDO',
          style: theme.textTheme.headlineSmall?.copyWith(
            color: cs.secondary,
            fontWeight: FontWeight.bold,
            letterSpacing: 1.2,
          ),
        ),
        const SizedBox(height: 8),
        Text(
          'Seu atendimento foi concluído',
          style: theme.textTheme.bodyLarge?.copyWith(
            fontWeight: FontWeight.w500,
          ),
        ),
        const SizedBox(height: 16),
        Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            color: cs.secondary.withOpacity(0.1),
            borderRadius: BorderRadius.circular(8),
          ),
          child: Row(
            children: [
              Icon(Icons.check_circle, color: cs.secondary),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  'Obrigado por visitar ${widget.salon.name}!',
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: cs.secondary,
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
    final cs = theme.colorScheme;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          _currentEntry.status == QueueEntryStatus.cancelled ? 'CANCELADO' : 'EXPIRADO',
          style: theme.textTheme.headlineSmall?.copyWith(
            color: cs.error,
            fontWeight: FontWeight.bold,
            letterSpacing: 1.2,
          ),
        ),
        const SizedBox(height: 8),
        Text(
          'Sua entrada na fila não está mais ativa',
          style: theme.textTheme.bodyLarge?.copyWith(
            fontWeight: FontWeight.w500,
          ),
        ),
      ],
    );
  }

  Widget _buildSalonCard(ThemeData theme, bool isSmallScreen) {
    final cs = theme.colorScheme;
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: cs.surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: cs.outline.withOpacity(0.2),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
            decoration: BoxDecoration(
              color: cs.primary.withOpacity(0.1),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Text(
              'NA FILA',
              style: theme.textTheme.labelSmall?.copyWith(
                color: cs.primary,
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
                widget.salon.isOpen ? 'Aberto' : 'Fechado',
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: widget.salon.isOpen ? cs.primary : cs.error,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(width: 16),
              Icon(Icons.people, size: 16, color: theme.colorScheme.onSurfaceVariant),
              const SizedBox(width: 4),
              Text('${widget.salon.queueLength} na fila'),
            ],
          ),
          if (_currentEntry.serviceRequested != null) ...[
            const SizedBox(height: 8),
            Row(
              children: [
                Icon(Icons.content_cut, size: 16, color: theme.colorScheme.onSurfaceVariant),
                const SizedBox(width: 4),
                Text('Serviço: ${_currentEntry.serviceRequested}'),
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
          'Como chegar',
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
    final cs = theme.colorScheme;
    return ListTile(
      contentPadding: EdgeInsets.zero,
      leading: Icon(icon, color: cs.primary),
      title: Text(
        label,
        style: theme.textTheme.bodyLarge?.copyWith(
          color: cs.primary,
          fontWeight: FontWeight.w500,
        ),
      ),
      onTap: onTap,
    );
  }

  Widget _buildActionsCard(ThemeData theme, bool isSmallScreen) {
    final cs = theme.colorScheme;
    if (!_currentEntry.isActive) {
      return const SizedBox.shrink();
    }

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: cs.surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: cs.outline.withOpacity(0.2),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'AÇÕES',
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
                foregroundColor: cs.error,
                side: BorderSide(color: cs.error, width: 2),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(24),
                ),
                padding: const EdgeInsets.symmetric(vertical: 12),
              ),
              onPressed: _leaveQueue,
              child: const Text(
                'Sair da fila',
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
          const SnackBar(content: Text('Não foi possível abrir o mapa')),
        );
      }
    }
  }
} 