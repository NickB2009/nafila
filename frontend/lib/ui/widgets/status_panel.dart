import 'package:flutter/material.dart';
import '../../models/queue_entry.dart';
import '../theme/app_theme.dart';

/// A widget that displays queue status with appropriate color and icon
class StatusPanel extends StatelessWidget {
  final QueueStatus status;
  final bool compact;

  const StatusPanel({
    super.key,
    required this.status,
    this.compact = false,
  });

  @override
  Widget build(BuildContext context) {
    final statusInfo = _getStatusInfo(status);
    final theme = Theme.of(context);

    return Container(
      padding: compact
          ? const EdgeInsets.symmetric(horizontal: 8, vertical: 4)
          : const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      decoration: BoxDecoration(
        color: statusInfo.color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(
          color: statusInfo.color.withOpacity(0.3),
          width: 1,
        ),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            statusInfo.icon,
            size: compact ? 14 : 16,
            color: statusInfo.color,
          ),
          const SizedBox(width: 4),
          Flexible(
            child: Text(
              status.displayName,
              style: theme.textTheme.labelSmall?.copyWith(
                color: statusInfo.color,
                fontWeight: FontWeight.w600,
                fontSize: compact ? 11 : 12,
              ),
              overflow: TextOverflow.ellipsis,
              maxLines: 1,
            ),
          ),
        ],
      ),
    );
  }

  StatusInfo _getStatusInfo(QueueStatus status) {
    switch (status) {
      case QueueStatus.waiting:
        return StatusInfo(
          icon: Icons.access_time,
          color: AppTheme.statusColors['waiting']!,
        );
      case QueueStatus.inService:
        return StatusInfo(
          icon: Icons.person_outline,
          color: AppTheme.statusColors['inService']!,
        );
      case QueueStatus.completed:
        return StatusInfo(
          icon: Icons.check_circle_outline,
          color: AppTheme.statusColors['completed']!,
        );
    }
  }
}

class StatusInfo {
  final IconData icon;
  final Color color;

  const StatusInfo({
    required this.icon,
    required this.color,
  });
}
