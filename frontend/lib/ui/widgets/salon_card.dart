import 'package:flutter/material.dart';
import '../../models/salon.dart';
import '../theme/app_theme.dart';
import '../screens/check_in_screen.dart';
import 'package:intl/intl.dart';

/// Card widget displaying salon information with wait time and check-in option
class SalonCard extends StatelessWidget {
  final Salon salon;
  final bool isSelected;
  final VoidCallback? onTap;
  final VoidCallback? onCheckIn;
  final bool isFavorite;
  final VoidCallback? onToggleFavorite;

  const SalonCard({
    super.key,
    required this.salon,
    this.isSelected = false,
    this.onTap,
    this.onCheckIn,
    this.isFavorite = false,
    this.onToggleFavorite,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    return Card(
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
        side: BorderSide(
          color: isSelected ? theme.colorScheme.primary : Colors.transparent,
          width: 2,
        ),
      ),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(16),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Container(
                    width: 50,
                    height: 50,
                    decoration: BoxDecoration(
                      color: theme.colorScheme.primary.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(25),
                    ),
                    child: Icon(
                      Icons.cut,
                      color: theme.colorScheme.primary,
                      size: 24,
                    ),
                  ),
                  const SizedBox(width: 16),
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
                  IconButton(
                    icon: Icon(
                      isFavorite ? Icons.favorite : Icons.favorite_border,
                      color: theme.colorScheme.primary,
                    ),
                    onPressed: onToggleFavorite,
                  ),
                ],
              ),
              const SizedBox(height: 16),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  _buildInfoChip(
                    context,
                    Icons.access_time,
                    '${salon.waitTime} min',
                    theme.colorScheme.primary,
                  ),
                  _buildInfoChip(
                    context,
                    Icons.location_on,
                    '${salon.distance} km',
                    theme.colorScheme.primary,
                  ),
                  _buildInfoChip(
                    context,
                    Icons.people,
                    '${salon.queueLength} na fila',
                    theme.colorScheme.primary,
                  ),
                ],
              ),
              const SizedBox(height: 16),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    'Fechamento: ${salon.closingTime}',
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                  ),
                  ElevatedButton(
                    onPressed: onCheckIn,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: theme.colorScheme.primary,
                      foregroundColor: theme.colorScheme.onPrimary,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(8),
                      ),
                    ),
                    child: const Text('Check-in'),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildInfoChip(BuildContext context, IconData icon, String label, Color color) {
    final theme = Theme.of(context);
    
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: theme.colorScheme.primary.withOpacity(0.1),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            icon,
            size: 16,
            color: theme.colorScheme.primary,
          ),
          const SizedBox(width: 4),
          Text(
            label,
            style: theme.textTheme.labelMedium?.copyWith(
              color: theme.colorScheme.primary,
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
