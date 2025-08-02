import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../models/public_salon.dart';
import '../../services/anonymous_queue_service.dart';
import '../../controllers/app_controller.dart';
import '../theme/app_theme.dart';
import 'anonymous_queue_status_screen.dart';

/// Screen for anonymous users to join a queue without creating an account
class AnonymousJoinQueueScreen extends StatefulWidget {
  final PublicSalon salon;
  final AnonymousQueueService? queueService;

  const AnonymousJoinQueueScreen({
    super.key,
    required this.salon,
    this.queueService,
  });

  @override
  State<AnonymousJoinQueueScreen> createState() => _AnonymousJoinQueueScreenState();
}

class _AnonymousJoinQueueScreenState extends State<AnonymousJoinQueueScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  late AnonymousQueueService _queueService;
  
  bool _emailNotifications = true;
  bool _browserNotifications = true;
  bool _isLoading = false;
  String? _errorMessage;
  List<String> _selectedServices = [];

  @override
  void initState() {
    super.initState();
    _queueService = widget.queueService ?? AnonymousQueueService.create();
    
    // Set default service to "Haircut" if available in salon's services
    final availableServices = widget.salon.services?.isNotEmpty == true 
        ? widget.salon.services!
        : [
            'Haircut',
            'Beard Trim',
            'Hair Wash',
            'Styling',
            'Hair Treatment',
            'Mustache Trim',
            'Eyebrow Trim',
          ];
    
    // Default to "Haircut" if it's in the available services (most common barbershop service)
    if (availableServices.contains('Haircut')) {
      _selectedServices = ['Haircut'];
    }
  }

  @override
  void dispose() {
    _nameController.dispose();
    _emailController.dispose();
    super.dispose();
  }

  Future<void> _joinQueue() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final queueEntry = await _queueService.joinQueue(
        salon: widget.salon,
        name: _nameController.text.trim(),
        email: _emailController.text.trim(),
        serviceRequested: _selectedServices.isNotEmpty ? _selectedServices.join(', ') : null,
        emailNotifications: _emailNotifications,
        browserNotifications: _browserNotifications,
      );

      if (mounted) {
        // Navigate to queue status screen with REAL data from backend
        Navigator.of(context).pushReplacement(
          MaterialPageRoute(
            builder: (context) => AnonymousQueueStatusScreen(
              queueEntry: queueEntry,
              salon: widget.salon,
            ),
          ),
        );
        
        // Refresh salon data on main page to show updated queue length
        // This ensures the queue length is updated when user goes back
        WidgetsBinding.instance.addPostFrameCallback((_) {
          final appController = Provider.of<AppController>(context, listen: false);
          appController.anonymous.loadPublicSalons();
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _errorMessage = e.toString().replaceFirst('Exception: ', '');
          _isLoading = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isSmallScreen = MediaQuery.of(context).size.width < 600;

    return Scaffold(
      backgroundColor: theme.colorScheme.surface,
      body: SafeArea(
        child: SingleChildScrollView(
          padding: EdgeInsets.all(isSmallScreen ? 16 : 20),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _buildHeader(theme, isSmallScreen),
                SizedBox(height: isSmallScreen ? 20 : 24),
                _buildSalonInfo(theme, isSmallScreen),
                SizedBox(height: isSmallScreen ? 20 : 24),
                _buildAnonymousNotice(theme, isSmallScreen),
                SizedBox(height: isSmallScreen ? 20 : 24),
                _buildForm(theme, isSmallScreen),
                SizedBox(height: isSmallScreen ? 20 : 24),
                _buildNotificationPreferences(theme, isSmallScreen),
                SizedBox(height: isSmallScreen ? 24 : 32),
                _buildJoinButton(theme, isSmallScreen),
                if (_errorMessage != null) ...[
                  const SizedBox(height: 16),
                  _buildErrorMessage(theme),
                ],
                const SizedBox(height: 16),
                _buildDisclaimer(theme, isSmallScreen),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildHeader(ThemeData theme, bool isSmallScreen) {
    return Row(
      children: [
        IconButton(
          onPressed: () => Navigator.of(context).pop(),
          icon: const Icon(Icons.arrow_back),
        ),
        Expanded(
          child: Text(
            'Join Queue',
            style: theme.textTheme.headlineSmall?.copyWith(
              fontWeight: FontWeight.bold,
              fontSize: isSmallScreen ? 18 : null,
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildSalonInfo(ThemeData theme, bool isSmallScreen) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: theme.colorScheme.surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: theme.colorScheme.outline.withOpacity(0.2),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            widget.salon.name,
            style: theme.textTheme.titleLarge?.copyWith(
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            widget.salon.address,
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
          ),
          const SizedBox(height: 12),
          Row(
            children: [
              Icon(
                widget.salon.isOpen ? Icons.access_time : Icons.access_time_filled,
                size: 16,
                color: widget.salon.isOpen ? Colors.green : Colors.red,
              ),
              const SizedBox(width: 4),
              Text(
                widget.salon.isOpen ? 'Open' : 'Closed',
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: widget.salon.isOpen ? Colors.green : Colors.red,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(width: 16),
              Icon(
                Icons.people,
                size: 16,
                color: theme.colorScheme.onSurfaceVariant,
              ),
              const SizedBox(width: 4),
              Text(
                '${widget.salon.queueLength ?? 0} in queue',
                style: theme.textTheme.bodyMedium,
              ),
              if (widget.salon.currentWaitTimeMinutes != null) ...[
                const SizedBox(width: 16),
                Icon(
                  Icons.timer,
                  size: 16,
                  color: theme.colorScheme.onSurfaceVariant,
                ),
                const SizedBox(width: 4),
                Text(
                  '~${widget.salon.currentWaitTimeMinutes} min wait',
                  style: theme.textTheme.bodyMedium,
                ),
              ],
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildAnonymousNotice(ThemeData theme, bool isSmallScreen) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.orange.withOpacity(0.1),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: Colors.orange.withOpacity(0.3),
        ),
      ),
      child: Row(
        children: [
          Icon(
            Icons.info_outline,
            color: Colors.orange,
            size: isSmallScreen ? 20 : 24,
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Anonymous Queue Entry',
                  style: theme.textTheme.titleSmall?.copyWith(
                    color: Colors.orange.shade800,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  'You can join the queue without creating an account. Your information will be saved locally on your device.',
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: Colors.orange.shade700,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildForm(ThemeData theme, bool isSmallScreen) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Your Information',
          style: theme.textTheme.titleMedium?.copyWith(
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 16),
        TextFormField(
          controller: _nameController,
          decoration: const InputDecoration(
            labelText: 'Your Name *',
            hintText: 'Enter your full name',
            prefixIcon: Icon(Icons.person),
          ),
          validator: (value) {
            if (value == null || value.trim().isEmpty) {
              return 'Please enter your name';
            }
            if (value.trim().length < 2) {
              return 'Name must be at least 2 characters';
            }
            return null;
          },
          textInputAction: TextInputAction.next,
        ),
        const SizedBox(height: 16),
        TextFormField(
          controller: _emailController,
          decoration: const InputDecoration(
            labelText: 'Email Address *',
            hintText: 'Enter your email',
            prefixIcon: Icon(Icons.email),
          ),
          keyboardType: TextInputType.emailAddress,
          validator: (value) {
            if (value == null || value.trim().isEmpty) {
              return 'Please enter your email';
            }
            final emailRegex = RegExp(r'^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$');
            if (!emailRegex.hasMatch(value.trim())) {
              return 'Please enter a valid email address';
            }
            return null;
          },
          textInputAction: TextInputAction.next,
        ),
        const SizedBox(height: 16),
        _buildServiceSelection(theme),
      ],
    );
  }

  Widget _buildNotificationPreferences(ThemeData theme, bool isSmallScreen) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Notification Preferences',
          style: theme.textTheme.titleMedium?.copyWith(
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 12),
        CheckboxListTile(
          title: const Text('Email notifications'),
          subtitle: const Text('Receive queue updates via email'),
          value: _emailNotifications,
          onChanged: (value) {
            setState(() {
              _emailNotifications = value ?? false;
            });
          },
          contentPadding: EdgeInsets.zero,
        ),
        CheckboxListTile(
          title: const Text('Browser notifications'),
          subtitle: const Text('Show notifications when this page is open'),
          value: _browserNotifications,
          onChanged: (value) {
            setState(() {
              _browserNotifications = value ?? false;
            });
          },
          contentPadding: EdgeInsets.zero,
        ),
      ],
    );
  }

  Widget _buildJoinButton(ThemeData theme, bool isSmallScreen) {
    return SizedBox(
      width: double.infinity,
      height: 48,
      child: ElevatedButton(
        style: ElevatedButton.styleFrom(
          backgroundColor: AppTheme.primaryColor,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(24),
          ),
        ),
        onPressed: _isLoading ? null : _joinQueue,
        child: _isLoading
            ? const SizedBox(
                width: 24,
                height: 24,
                child: CircularProgressIndicator(
                  strokeWidth: 2,
                  valueColor: AlwaysStoppedAnimation(Colors.white),
                ),
              )
            : Text(
                'Join Queue',
                style: theme.textTheme.titleMedium?.copyWith(
                  color: Colors.white,
                  fontWeight: FontWeight.bold,
                ),
              ),
      ),
    );
  }

  Widget _buildErrorMessage(ThemeData theme) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.red.withOpacity(0.1),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: Colors.red.withOpacity(0.3),
        ),
      ),
      child: Row(
        children: [
          const Icon(
            Icons.error_outline,
            color: Colors.red,
            size: 20,
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              _errorMessage!,
              style: theme.textTheme.bodyMedium?.copyWith(
                color: Colors.red.shade700,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildServiceSelection(ThemeData theme) {
    // Get available services for this salon, with fallback to common services
    final List<String> availableServices = widget.salon.services?.isNotEmpty == true 
        ? widget.salon.services!
        : [
            'Haircut',
            'Beard Trim',
            'Hair Wash',
            'Styling',
            'Hair Treatment',
            'Mustache Trim',
            'Eyebrow Trim',
          ];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            const Icon(Icons.content_cut, size: 20),
            const SizedBox(width: 8),
            Text(
              'Services Requested (Optional)',
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
        const SizedBox(height: 8),
        Text(
          'Select all services you need - you can choose multiple',
          style: theme.textTheme.bodySmall?.copyWith(
            color: theme.colorScheme.onSurfaceVariant,
          ),
        ),
        const SizedBox(height: 12),
        
        // Services as selectable chips
        Wrap(
          spacing: 8,
          runSpacing: 4,
          children: availableServices.map((service) {
            final isSelected = _selectedServices.contains(service);
            return FilterChip(
              label: Text(service),
              selected: isSelected,
              onSelected: (bool selected) {
                setState(() {
                  if (selected) {
                    _selectedServices.add(service);
                  } else {
                    _selectedServices.remove(service);
                  }
                });
              },
              selectedColor: AppTheme.primaryColor.withOpacity(0.2),
              checkmarkColor: AppTheme.primaryColor,
              side: BorderSide(
                color: isSelected 
                    ? AppTheme.primaryColor 
                    : theme.colorScheme.outline.withOpacity(0.5),
              ),
            );
          }).toList(),
        ),
        
        // Show selected services summary
        if (_selectedServices.isNotEmpty) ...[
          const SizedBox(height: 12),
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: AppTheme.primaryColor.withOpacity(0.1),
              borderRadius: BorderRadius.circular(8),
              border: Border.all(
                color: AppTheme.primaryColor.withOpacity(0.3),
              ),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Selected Services:',
                  style: theme.textTheme.labelMedium?.copyWith(
                    color: AppTheme.primaryColor,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  _selectedServices.join(' + '),
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: AppTheme.primaryColor,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ],
            ),
          ),
        ],
      ],
    );
  }

  Widget _buildDisclaimer(ThemeData theme, bool isSmallScreen) {
    return Text(
      'Your information is stored locally on your device. We only use your email to send queue updates if enabled.',
      style: theme.textTheme.bodySmall?.copyWith(
        color: theme.colorScheme.onSurfaceVariant,
        fontSize: isSmallScreen ? 12 : null,
      ),
      textAlign: TextAlign.center,
    );
  }
} 