import 'package:flutter/material.dart';
import '../../models/salon.dart';
import '../theme/app_theme.dart';
import '../screens/check_in_screen.dart';

/// Card widget displaying salon information with wait time and check-in option
class SalonCard extends StatelessWidget {
  final Salon salon;
  final bool isSelected;
  final VoidCallback? onTap;

  const SalonCard({
    super.key,
    required this.salon,
    this.isSelected = false,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    return Card(
      elevation: isSelected ? 4 : 2,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
        side: BorderSide(
          color: isSelected ? AppTheme.primaryColor : Colors.transparent,
          width: 2,
        ),
      ),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
              Row(
                children: [
                  CircleAvatar(
                    radius: 24,
                    backgroundColor: AppTheme.primaryColor.withOpacity(0.1),
                    child: Icon(
                      Icons.store,
                      color: AppTheme.primaryColor,
                      size: 24,
                    ),
            ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          salon.name,
                          style: theme.textTheme.titleLarge,
              ),
                        const SizedBox(height: 4),
          Text(
                          salon.address,
                          style: theme.textTheme.bodyMedium?.copyWith(
                            color: theme.colorScheme.onSurfaceVariant,
                          ),
                ),
            ],
          ),
                  ),
                ],
            ),
              const SizedBox(height: 16),
          Row(
            children: [
                  _buildInfoChip(
                    context,
                    Icons.access_time,
                    '${salon.waitTime} min',
                  ),
                  const SizedBox(width: 8),
                  _buildInfoChip(
                    context,
                    Icons.people,
                    '${salon.queueLength} in queue',
                  ),
                ],
              ),
              const SizedBox(height: 16),
              // Check In button
              SizedBox(
                width: double.infinity,
                child: ElevatedButton.icon(
                  icon: Icon(Icons.check_circle, color: theme.colorScheme.onPrimary),
                  label: const Text('Check In'),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppTheme.primaryColor,
                    foregroundColor: theme.colorScheme.onPrimary,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(24),
                ),
                    textStyle: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold),
                    padding: const EdgeInsets.symmetric(vertical: 12),
              ),
                  onPressed: () {
                    Navigator.of(context).push(
                      MaterialPageRoute(
                        builder: (_) => CheckInScreen(
                          salon: salon,
                        ),
                      ),
                    );
                  },
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildInfoChip(BuildContext context, IconData icon, String label) {
    final theme = Theme.of(context);
    
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: AppTheme.primaryColor.withOpacity(0.1),
        borderRadius: BorderRadius.circular(20),
              ),
              child: Row(
        mainAxisSize: MainAxisSize.min,
                children: [
          Icon(
            icon,
            size: 16,
            color: AppTheme.primaryColor,
                  ),
          const SizedBox(width: 4),
          Text(
            label,
            style: theme.textTheme.labelMedium?.copyWith(
              color: AppTheme.primaryColor,
                      fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }

  void _showCheckInDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: Text('Check in to ${salon.name}'),
          content: Text(
            'Are you ready to join the queue at ${salon.name}?\n\n'
            'Estimated wait time: ${salon.waitTime} minutes',
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('Cancel'),
            ),
            ElevatedButton(
              onPressed: () {
                Navigator.of(context).pop();
                _handleCheckIn(context);
              },
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF1DB584),
                foregroundColor: Colors.white,
              ),
              child: const Text('Check In'),
            ),
          ],
        );
      },
    );
  }

  void _handleCheckIn(BuildContext context) {
    // TODO: Implement actual check-in logic
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Successfully checked in to ${salon.name}!'),
        backgroundColor: const Color(0xFF1DB584),
        behavior: SnackBarBehavior.floating,
      ),
    );
  }
}
