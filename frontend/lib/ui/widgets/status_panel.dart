import 'package:flutter/material.dart';
import '../../models/queue_entry.dart';

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
          Text(
            status.displayName,
            style: theme.textTheme.labelSmall?.copyWith(
              color: statusInfo.color,
              fontWeight: FontWeight.w600,
              fontSize: compact ? 11 : 12,
            ),
          ),
        ],
      ),
    );
  }

  _StatusInfo _getStatusInfo(QueueStatus status) {
    switch (status) {
      case QueueStatus.waiting:
        return _StatusInfo(
          icon: Icons.schedule,
          color: const Color(0xFFFF9800), // Orange
        );
      case QueueStatus.inService:
        return _StatusInfo(
          icon: Icons.person_outline,
          color: const Color(0xFF4CAF50), // Green
        );
      case QueueStatus.completed:
        return _StatusInfo(
          icon: Icons.check_circle_outline,
          color: const Color(0xFF9E9E9E), // Grey
        );
    }
  }
}

class _StatusInfo {
  final IconData icon;
  final Color color;

  _StatusInfo({
    required this.icon,
    required this.color,
  });
}
