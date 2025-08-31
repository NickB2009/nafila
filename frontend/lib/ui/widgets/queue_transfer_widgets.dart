import 'package:flutter/material.dart';
import '../../models/queue_transfer_models.dart';

/// Widget for displaying transfer suggestions
class TransferSuggestionsCard extends StatelessWidget {
  final TransferSuggestionsResponse suggestions;
  final Function(QueueTransferSuggestion) onSuggestionTap;
  final VoidCallback? onRefresh;

  const TransferSuggestionsCard({
    super.key,
    required this.suggestions,
    required this.onSuggestionTap,
    this.onRefresh,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    if (!suggestions.hasSuggestions) {
      return _buildNoSuggestions(context, theme);
    }

    return Card(
      elevation: 2,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _buildHeader(context, theme),
            const SizedBox(height: 16),
            _buildCurrentStatus(context, theme),
            const SizedBox(height: 16),
            _buildSuggestionsList(context, theme),
          ],
        ),
      ),
    );
  }

  Widget _buildHeader(BuildContext context, ThemeData theme) {
    return Row(
      children: [
        Icon(
          Icons.swap_horiz,
          color: theme.colorScheme.primary,
          size: 24,
        ),
        const SizedBox(width: 8),
        Expanded(
          child: Text(
            'Better Options Available',
            style: theme.textTheme.titleLarge?.copyWith(
              fontWeight: FontWeight.bold,
              color: theme.colorScheme.primary,
            ),
          ),
        ),
        if (onRefresh != null)
          IconButton(
            onPressed: onRefresh,
            icon: Icon(
              Icons.refresh,
              color: theme.colorScheme.onSurfaceVariant,
            ),
            tooltip: 'Refresh suggestions',
          ),
      ],
    );
  }

  Widget _buildCurrentStatus(BuildContext context, ThemeData theme) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: theme.colorScheme.surfaceContainerHighest.withOpacity(0.5),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        children: [
          Icon(
            Icons.schedule,
            size: 16,
            color: theme.colorScheme.onSurfaceVariant,
          ),
          const SizedBox(width: 8),
          Text(
            'Current: ${suggestions.currentSalonName}',
            style: theme.textTheme.bodyMedium?.copyWith(
              fontWeight: FontWeight.w500,
            ),
          ),
          const Spacer(),
          Text(
            'Position ${suggestions.currentPosition} • ${suggestions.currentFormattedWaitTime}',
            style: theme.textTheme.bodySmall?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSuggestionsList(BuildContext context, ThemeData theme) {
    final validSuggestions = suggestions.validSuggestions.take(3).toList();
    
    return Column(
      children: validSuggestions.map((suggestion) => 
        _buildSuggestionItem(context, theme, suggestion)
      ).toList(),
    );
  }

  Widget _buildSuggestionItem(BuildContext context, ThemeData theme, QueueTransferSuggestion suggestion) {
    final priorityColor = Color(int.parse(suggestion.priorityColor.replaceFirst('#', '0xFF')));
    
    return Container(
      margin: const EdgeInsets.only(bottom: 8),
      child: InkWell(
        onTap: () => onSuggestionTap(suggestion),
        borderRadius: BorderRadius.circular(8),
        child: Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            border: Border.all(color: priorityColor.withOpacity(0.3)),
            borderRadius: BorderRadius.circular(8),
            color: priorityColor.withOpacity(0.05),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                    decoration: BoxDecoration(
                      color: priorityColor,
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Text(
                      suggestion.priority.toUpperCase(),
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: Colors.white,
                        fontWeight: FontWeight.bold,
                        fontSize: 10,
                      ),
                    ),
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      suggestion.title,
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                    decoration: BoxDecoration(
                      color: Colors.green.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Text(
                      suggestion.improvementText,
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: Colors.green,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 8),
              Text(
                suggestion.description,
                style: theme.textTheme.bodyMedium,
              ),
              const SizedBox(height: 8),
              Row(
                children: [
                  _buildInfoChip(
                    context,
                    Icons.location_on,
                    suggestion.formattedDistance,
                    theme.colorScheme.secondary,
                  ),
                  const SizedBox(width: 8),
                  _buildInfoChip(
                    context,
                    Icons.schedule,
                    suggestion.formattedWaitTime,
                    theme.colorScheme.primary,
                  ),
                  const Spacer(),
                  Icon(
                    Icons.arrow_forward_ios,
                    size: 16,
                    color: theme.colorScheme.onSurfaceVariant,
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
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 14, color: color),
          const SizedBox(width: 4),
          Text(
            label,
            style: theme.textTheme.bodySmall?.copyWith(
              color: color,
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildNoSuggestions(BuildContext context, ThemeData theme) {
    return Card(
      elevation: 2,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            Icon(
              Icons.check_circle,
              size: 48,
              color: Colors.green,
            ),
            const SizedBox(height: 12),
            Text(
              'You\'re in the best queue!',
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
                color: Colors.green,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              'No better options available right now.',
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }
}

/// Widget for transfer confirmation dialog
class TransferConfirmationDialog extends StatefulWidget {
  final QueueTransferSuggestion suggestion;
  final String currentSalonName;
  final int currentPosition;
  final int currentWaitMinutes;
  final Function(String? reason) onConfirm;
  final VoidCallback onCancel;

  const TransferConfirmationDialog({
    super.key,
    required this.suggestion,
    required this.currentSalonName,
    required this.currentPosition,
    required this.currentWaitMinutes,
    required this.onConfirm,
    required this.onCancel,
  });

  @override
  State<TransferConfirmationDialog> createState() => _TransferConfirmationDialogState();
}

class _TransferConfirmationDialogState extends State<TransferConfirmationDialog> {
  final _reasonController = TextEditingController();
  bool _isLoading = false;

  @override
  void dispose() {
    _reasonController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    return AlertDialog(
      title: Row(
        children: [
          Icon(
            Icons.swap_horiz,
            color: theme.colorScheme.primary,
          ),
          const SizedBox(width: 8),
          const Expanded(child: Text('Confirm Transfer')),
        ],
      ),
      content: SingleChildScrollView(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Transfer to ${widget.suggestion.targetSalonName}?',
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 16),
            _buildComparisonTable(context, theme),
            const SizedBox(height: 16),
            Text(
              'Reason (optional):',
              style: theme.textTheme.labelLarge,
            ),
            const SizedBox(height: 8),
            TextField(
              controller: _reasonController,
              decoration: const InputDecoration(
                hintText: 'Why are you transferring?',
                border: OutlineInputBorder(),
              ),
              maxLines: 2,
            ),
            const SizedBox(height: 16),
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.orange.withOpacity(0.1),
                borderRadius: BorderRadius.circular(8),
                border: Border.all(color: Colors.orange.withOpacity(0.3)),
              ),
              child: Row(
                children: [
                  Icon(Icons.info, color: Colors.orange, size: 20),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      'You will lose your current position and join the new queue.',
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: Colors.orange.shade700,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: _isLoading ? null : widget.onCancel,
          child: const Text('Cancel'),
        ),
        ElevatedButton(
          onPressed: _isLoading ? null : _handleConfirm,
          child: _isLoading
              ? const SizedBox(
                  width: 20,
                  height: 20,
                  child: CircularProgressIndicator(strokeWidth: 2),
                )
              : const Text('Transfer'),
        ),
      ],
    );
  }

  Widget _buildComparisonTable(BuildContext context, ThemeData theme) {
    return Container(
      decoration: BoxDecoration(
        border: Border.all(color: theme.colorScheme.outline.withOpacity(0.3)),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Column(
        children: [
          _buildComparisonRow(
            context,
            theme,
            'Location',
            widget.currentSalonName,
            widget.suggestion.targetSalonName,
            isHeader: true,
          ),
          _buildComparisonRow(
            context,
            theme,
            'Position',
            '${widget.currentPosition}',
            'End of queue',
          ),
          _buildComparisonRow(
            context,
            theme,
            'Wait Time',
            '${widget.currentWaitMinutes} min',
            widget.suggestion.formattedWaitTime,
            isImprovement: widget.suggestion.timeImprovement > 0,
          ),
          _buildComparisonRow(
            context,
            theme,
            'Distance',
            'Current location',
            widget.suggestion.formattedDistance,
          ),
        ],
      ),
    );
  }

  Widget _buildComparisonRow(
    BuildContext context,
    ThemeData theme,
    String label,
    String current,
    String target, {
    bool isHeader = false,
    bool isImprovement = false,
  }) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      decoration: BoxDecoration(
        color: isHeader ? theme.colorScheme.surfaceContainerHighest.withOpacity(0.5) : null,
        border: Border(
          bottom: BorderSide(
            color: theme.colorScheme.outline.withOpacity(0.2),
            width: 0.5,
          ),
        ),
      ),
      child: Row(
        children: [
          SizedBox(
            width: 80,
            child: Text(
              label,
              style: theme.textTheme.bodyMedium?.copyWith(
                fontWeight: isHeader ? FontWeight.bold : FontWeight.w500,
              ),
            ),
          ),
          Expanded(
            child: Text(
              current,
              style: theme.textTheme.bodyMedium,
              textAlign: TextAlign.center,
            ),
          ),
          Icon(
            Icons.arrow_forward,
            size: 16,
            color: theme.colorScheme.onSurfaceVariant,
          ),
          Expanded(
            child: Text(
              target,
              style: theme.textTheme.bodyMedium?.copyWith(
                color: isImprovement ? Colors.green : null,
                fontWeight: isImprovement ? FontWeight.bold : null,
              ),
              textAlign: TextAlign.center,
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _handleConfirm() async {
    setState(() {
      _isLoading = true;
    });

    try {
      final reason = _reasonController.text.trim();
      widget.onConfirm(reason.isEmpty ? null : reason);
    } finally {
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
      }
    }
  }
}

/// Widget for displaying transfer history
class TransferHistoryCard extends StatelessWidget {
  final List<QueueTransferAnalytics> transfers;
  final VoidCallback? onViewAll;

  const TransferHistoryCard({
    super.key,
    required this.transfers,
    this.onViewAll,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    if (transfers.isEmpty) {
      return Card(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            children: [
              Icon(
                Icons.history,
                size: 48,
                color: theme.colorScheme.onSurfaceVariant,
              ),
              const SizedBox(height: 12),
              Text(
                'No transfer history',
                style: theme.textTheme.titleMedium,
              ),
              const SizedBox(height: 8),
              Text(
                'Your queue transfers will appear here.',
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                ),
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
      );
    }

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(
                  Icons.history,
                  color: theme.colorScheme.primary,
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    'Transfer History',
                    style: theme.textTheme.titleLarge?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
                if (onViewAll != null)
                  TextButton(
                    onPressed: onViewAll,
                    child: const Text('View All'),
                  ),
              ],
            ),
            const SizedBox(height: 16),
            ...transfers.take(3).map((transfer) => 
              _buildTransferItem(context, theme, transfer)
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildTransferItem(BuildContext context, ThemeData theme, QueueTransferAnalytics transfer) {
    final isSuccessful = transfer.wasSuccessful;
    final statusColor = isSuccessful ? Colors.green : Colors.red;
    
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        border: Border.all(color: theme.colorScheme.outline.withOpacity(0.2)),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(
                isSuccessful ? Icons.check_circle : Icons.error,
                color: statusColor,
                size: 20,
              ),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  transfer.transferType.toUpperCase(),
                  style: theme.textTheme.titleSmall?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: statusColor,
                  ),
                ),
              ),
              Text(
                _formatDate(transfer.transferredAt),
                style: theme.textTheme.bodySmall?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          if (isSuccessful)
            Text(
              transfer.improvementSummary,
              style: theme.textTheme.bodyMedium,
            ),
          if (transfer.reason != null) ...[
            const SizedBox(height: 4),
            Text(
              'Reason: ${transfer.reason}',
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
                fontStyle: FontStyle.italic,
              ),
            ),
          ],
        ],
      ),
    );
  }

  String _formatDate(DateTime date) {
    final now = DateTime.now();
    final difference = now.difference(date);
    
    if (difference.inDays > 0) {
      return '${difference.inDays}d ago';
    } else if (difference.inHours > 0) {
      return '${difference.inHours}h ago';
    } else if (difference.inMinutes > 0) {
      return '${difference.inMinutes}m ago';
    } else {
      return 'Just now';
    }
  }
}
