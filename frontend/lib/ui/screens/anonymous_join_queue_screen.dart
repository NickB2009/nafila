import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../models/public_salon.dart';
import '../../models/anonymous_user.dart';
import '../../services/anonymous_queue_service.dart';
import '../../services/anonymous_user_service.dart';
import '../widgets/bottom_nav_bar.dart';

class AnonymousJoinQueueScreen extends StatefulWidget {
  final PublicSalon salon;

  const AnonymousJoinQueueScreen({
    super.key,
    required this.salon,
  });

  @override
  State<AnonymousJoinQueueScreen> createState() => _AnonymousJoinQueueScreenState();
}

class _AnonymousJoinQueueScreenState extends State<AnonymousJoinQueueScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  
  bool _emailNotifications = true;
  bool _browserNotifications = true;
  bool _isLoading = false;
  String? _selectedService;

  late AnonymousQueueService _queueService;
  late AnonymousUserService _userService;

  @override
  void initState() {
    super.initState();
    _queueService = AnonymousQueueService.create();
    _userService = AnonymousUserService();
    _loadExistingUserData();
  }

  Future<void> _loadExistingUserData() async {
    final user = await _userService.getAnonymousUser();
    if (user != null) {
      setState(() {
        _nameController.text = user.name;
        _emailController.text = user.email;
        _emailNotifications = user.preferences.emailNotifications;
        _browserNotifications = user.preferences.browserNotifications;
      });
    }
  }

  @override
  void dispose() {
    _nameController.dispose();
    _emailController.dispose();
    super.dispose();
  }

  Future<void> _joinQueue() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() {
      _isLoading = true;
    });

    try {
      // Check if user can join queue
      final canJoin = await _queueService.canJoinQueue(widget.salon.id);
      if (!canJoin) {
        _showErrorDialog('You are already in the queue for this salon');
        return;
      }

      // Join the queue
      final queueEntry = await _queueService.joinQueue(
        salon: widget.salon,
        name: _nameController.text.trim(),
        email: _emailController.text.trim(),
        serviceRequested: _selectedService,
        emailNotifications: _emailNotifications,
        browserNotifications: _browserNotifications,
      );

      // Show success and navigate to queue status
      _showSuccessDialog(queueEntry);

    } catch (e) {
      _showErrorDialog(e.toString());
    } finally {
      setState(() {
        _isLoading = false;
      });
    }
  }

  void _showSuccessDialog(AnonymousQueueEntry entry) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('✅ Queue Entry Successful'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('You\'re now in the queue for ${widget.salon.name}'),
            const SizedBox(height: 12),
            Text('👥 Position: #${entry.position}'),
            Text('⏱️ Estimated wait: ${entry.estimatedWaitMinutes} minutes'),
            const SizedBox(height: 12),
            const Text('💾 Your queue info has been saved locally.'),
            if (_emailNotifications)
              const Text('📧 You\'ll receive email updates.'),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.of(context).pop(); // Close dialog
              Navigator.of(context).pop(); // Go back to salon list
            },
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }

  void _showErrorDialog(String message) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('❌ Error'),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Join Queue'),
        backgroundColor: theme.colorScheme.surface,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Salon info card
              _buildSalonInfoCard(),
              const SizedBox(height: 24),

              // Anonymous notice
              _buildAnonymousNotice(),
              const SizedBox(height: 24),

              // Form fields
              _buildFormFields(),
              const SizedBox(height: 24),

              // Service selection
              _buildServiceSelection(),
              const SizedBox(height: 24),

              // Notification preferences
              _buildNotificationPreferences(),
              const SizedBox(height: 32),

              // Join button
              _buildJoinButton(),
              const SizedBox(height: 16),

              // Privacy notice
              _buildPrivacyNotice(),
            ],
          ),
        ),
      ),
      bottomNavigationBar: const BottomNavBar(currentIndex: 0),
    );
  }

  Widget _buildSalonInfoCard() {
    final theme = Theme.of(context);
    
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              widget.salon.name,
              style: theme.textTheme.headlineSmall?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),
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
                  widget.salon.isOpen ? Icons.schedule : Icons.schedule_outlined,
                  size: 16,
                  color: widget.salon.isOpen ? Colors.green : Colors.red,
                ),
                const SizedBox(width: 4),
                Text(
                  widget.salon.isOpen ? 'Open' : 'Closed',
                  style: TextStyle(
                    color: widget.salon.isOpen ? Colors.green : Colors.red,
                    fontWeight: FontWeight.w500,
                  ),
                ),
                const SizedBox(width: 16),
                Icon(
                  Icons.people,
                  size: 16,
                  color: theme.colorScheme.onSurfaceVariant,
                ),
                const SizedBox(width: 4),
                Text('${widget.salon.queueLength} in queue'),
                const SizedBox(width: 16),
                Icon(
                  Icons.timer,
                  size: 16,
                  color: theme.colorScheme.onSurfaceVariant,
                ),
                const SizedBox(width: 4),
                Text('~${widget.salon.currentWaitTimeMinutes} min wait'),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildAnonymousNotice() {
    final theme = Theme.of(context);
    
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: theme.colorScheme.primaryContainer,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(
                Icons.info_outline,
                color: theme.colorScheme.onPrimaryContainer,
                size: 20,
              ),
              const SizedBox(width: 8),
              Text(
                'Anonymous Queue Entry',
                style: theme.textTheme.titleSmall?.copyWith(
                  color: theme.colorScheme.onPrimaryContainer,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            'You can join the queue without creating an account. Your information will be saved locally on your device.',
            style: theme.textTheme.bodySmall?.copyWith(
              color: theme.colorScheme.onPrimaryContainer,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFormFields() {
    return Column(
      children: [
        TextFormField(
          controller: _nameController,
          decoration: const InputDecoration(
            labelText: 'Your Name *',
            hintText: 'Enter your full name',
            prefixIcon: Icon(Icons.person),
          ),
          validator: (value) {
            if (value == null || value.trim().isEmpty) {
              return 'Name is required';
            }
            if (value.trim().length < 2) {
              return 'Name must be at least 2 characters';
            }
            return null;
          },
        ),
        const SizedBox(height: 16),
        TextFormField(
          controller: _emailController,
          keyboardType: TextInputType.emailAddress,
          decoration: const InputDecoration(
            labelText: 'Email Address *',
            hintText: 'Enter your email for updates',
            prefixIcon: Icon(Icons.email),
          ),
          validator: (value) {
            if (value == null || value.trim().isEmpty) {
              return 'Email is required';
            }
            final emailRegex = RegExp(r'^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$');
            if (!emailRegex.hasMatch(value.trim())) {
              return 'Please enter a valid email';
            }
            return null;
          },
        ),
      ],
    );
  }

  Widget _buildServiceSelection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Service (Optional)',
          style: Theme.of(context).textTheme.titleMedium,
        ),
        const SizedBox(height: 8),
        DropdownButtonFormField<String>(
          value: _selectedService,
          decoration: const InputDecoration(
            hintText: 'Select a service',
            prefixIcon: Icon(Icons.content_cut),
          ),
          items: const [
            DropdownMenuItem(value: 'Haircut', child: Text('Haircut')),
            DropdownMenuItem(value: 'Beard Trim', child: Text('Beard Trim')),
            DropdownMenuItem(value: 'Shave', child: Text('Shave')),
            DropdownMenuItem(value: 'Hair Wash', child: Text('Hair Wash')),
            DropdownMenuItem(value: 'Styling', child: Text('Styling')),
          ],
          onChanged: (value) {
            setState(() {
              _selectedService = value;
            });
          },
        ),
      ],
    );
  }

  Widget _buildNotificationPreferences() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Notification Preferences',
          style: Theme.of(context).textTheme.titleMedium,
        ),
        const SizedBox(height: 8),
        CheckboxListTile(
          title: const Text('Email notifications'),
          subtitle: const Text('Receive updates about your queue position'),
          value: _emailNotifications,
          onChanged: (value) {
            setState(() {
              _emailNotifications = value ?? true;
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
              _browserNotifications = value ?? true;
            });
          },
          contentPadding: EdgeInsets.zero,
        ),
      ],
    );
  }

  Widget _buildJoinButton() {
    return SizedBox(
      width: double.infinity,
      height: 48,
      child: ElevatedButton(
        onPressed: _isLoading ? null : _joinQueue,
        style: ElevatedButton.styleFrom(
          backgroundColor: Theme.of(context).colorScheme.primary,
          foregroundColor: Theme.of(context).colorScheme.onPrimary,
        ),
        child: _isLoading
            ? const SizedBox(
                width: 20,
                height: 20,
                child: CircularProgressIndicator(strokeWidth: 2),
              )
            : const Text(
                'Join Queue',
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                ),
              ),
      ),
    );
  }

  Widget _buildPrivacyNotice() {
    final theme = Theme.of(context);
    
    return Text(
      'Your information is stored locally on your device. '
      'We only use your email to send queue updates if enabled. '
      'You can create an account later to sync across devices.',
      style: theme.textTheme.bodySmall?.copyWith(
        color: theme.colorScheme.onSurfaceVariant,
      ),
      textAlign: TextAlign.center,
    );
  }
} 