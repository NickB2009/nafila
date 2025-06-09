import 'package:flutter/material.dart';
import '../../models/queue_entry.dart';
import 'status_panel.dart';

/// A card widget that displays individual queue entry information
class QueueCard extends StatelessWidget {
  final QueueEntry entry;
  final double width;
  final VoidCallback? onTap;

  const QueueCard({
    super.key,
    required this.entry,
    required this.width,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isCompact = width < 400;

    return Card(
      margin: EdgeInsets.symmetric(
        horizontal: isCompact ? 12 : 16,
        vertical: 6,
      ),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Container(
          constraints: const BoxConstraints(
            minHeight: 80,
            maxHeight: 88,
          ),
          padding: EdgeInsets.all(isCompact ? 12 : 16),
          child: Row(
            children: [
              // Avatar
              CircleAvatar(
                radius: isCompact ? 20 : 24,
                backgroundColor: theme.colorScheme.primary.withOpacity(0.1),
                child: Text(
                  _getInitials(entry.name),
                  style: theme.textTheme.titleMedium?.copyWith(
                    color: theme.colorScheme.primary,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),

              SizedBox(width: isCompact ? 12 : 16),

              // Name and details
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text(
                      entry.name,
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.w600,
                      ),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 4),
                    Row(
                      children: [
                        if (entry.status == QueueStatus.waiting) ...[
                          Icon(
                            Icons.numbers,
                            size: 14,
                            color: theme.colorScheme.onSurfaceVariant,
                          ),
                          const SizedBox(width: 4),
                          Text(
                            'Position ${entry.position}',
                            style: theme.textTheme.bodySmall?.copyWith(
                              color: theme.colorScheme.onSurfaceVariant,
                            ),
                          ),
                        ] else ...[
                          Icon(
                            Icons.access_time,
                            size: 14,
                            color: theme.colorScheme.onSurfaceVariant,
                          ),
                          const SizedBox(width: 4),
                          Text(
                            _getTimeAgo(entry.joinTime),
                            style: theme.textTheme.bodySmall?.copyWith(
                              color: theme.colorScheme.onSurfaceVariant,
                            ),
                          ),
                        ]
                      ],
                    ),
                  ],
                ),
              ),

              // Status panel
              StatusPanel(
                status: entry.status,
                compact: isCompact,
              ),
            ],
          ),
        ),
      ),
    );
  }

  String _getInitials(String name) {
    final words = name.trim().split(' ');
    if (words.length >= 2) {
      return '${words[0][0]}${words[1][0]}'.toUpperCase();
    } else if (words.isNotEmpty) {
      return words[0][0].toUpperCase();
    }
    return '?';
  }

  String _getTimeAgo(DateTime dateTime) {
    final now = DateTime.now();
    final difference = now.difference(dateTime);

    if (difference.inMinutes < 1) {
      return 'Just now';
    } else if (difference.inMinutes < 60) {
      return '${difference.inMinutes}m ago';
    } else if (difference.inHours < 24) {
      return '${difference.inHours}h ago';
    } else {
      return '${difference.inDays}d ago';
    }
  }
}
