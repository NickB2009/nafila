import 'package:flutter/material.dart';
import '../../models/analytics_models.dart';

/// Widget for displaying queue analytics data in a card format
class AnalyticsCard extends StatelessWidget {
  final String title;
  final Widget child;
  final Color? color;
  final VoidCallback? onTap;

  const AnalyticsCard({
    super.key,
    required this.title,
    required this.child,
    this.color,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    return Card(
      elevation: 2,
      color: color ?? theme.cardColor,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                title,
                style: theme.textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 12),
              child,
            ],
          ),
        ),
      ),
    );
  }
}

/// Widget for displaying wait time estimate
class WaitTimeCard extends StatelessWidget {
  final WaitTimeEstimate waitTime;
  final VoidCallback? onTap;

  const WaitTimeCard({
    super.key,
    required this.waitTime,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    return AnalyticsCard(
      title: 'Estimated Wait Time',
      onTap: onTap,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(
                Icons.access_time,
                color: theme.colorScheme.primary,
                size: 24,
              ),
              const SizedBox(width: 8),
              Text(
                waitTime.formattedWaitTime,
                style: theme.textTheme.headlineSmall?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: theme.colorScheme.primary,
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            'Service: ${waitTime.serviceType}',
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
          ),
          const SizedBox(height: 4),
          Row(
            children: [
              Icon(
                Icons.verified,
                size: 16,
                color: _getConfidenceColor(waitTime.confidence),
              ),
              const SizedBox(width: 4),
              Text(
                '${waitTime.confidence} confidence',
                style: theme.textTheme.bodySmall?.copyWith(
                  color: _getConfidenceColor(waitTime.confidence),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Color _getConfidenceColor(String confidence) {
    switch (confidence.toLowerCase()) {
      case 'high':
        return Colors.green;
      case 'medium':
        return Colors.orange;
      case 'low':
        return Colors.red;
      default:
        return Colors.grey;
    }
  }
}

/// Widget for displaying queue health status
class QueueHealthCard extends StatelessWidget {
  final QueueHealthStatus health;
  final VoidCallback? onTap;

  const QueueHealthCard({
    super.key,
    required this.health,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final statusColor = Color(int.parse(health.statusColor.replaceFirst('#', '0xFF')));
    
    return AnalyticsCard(
      title: 'Queue Health',
      onTap: onTap,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 12,
                height: 12,
                decoration: BoxDecoration(
                  color: statusColor,
                  shape: BoxShape.circle,
                ),
              ),
              const SizedBox(width: 8),
              Text(
                health.status,
                style: theme.textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: statusColor,
                ),
              ),
              const Spacer(),
              Text(
                '${health.healthScore.toInt()}%',
                style: theme.textTheme.titleLarge?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: statusColor,
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Row(
            children: [
              Expanded(
                child: _buildMetric(
                  context,
                  'Queue Length',
                  health.currentQueueLength.toString(),
                  Icons.people,
                ),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: _buildMetric(
                  context,
                  'Avg Wait',
                  '${health.currentWaitMinutes} min',
                  Icons.schedule,
                ),
              ),
            ],
          ),
          if (health.hasIssues) ...[
            const SizedBox(height: 12),
            Container(
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: Colors.red.withOpacity(0.1),
                borderRadius: BorderRadius.circular(8),
                border: Border.all(color: Colors.red.withOpacity(0.3)),
              ),
              child: Row(
                children: [
                  Icon(Icons.warning, color: Colors.red, size: 16),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      '${health.issues.length} issue${health.issues.length > 1 ? 's' : ''} detected',
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: Colors.red,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildMetric(BuildContext context, String label, String value, IconData icon) {
    final theme = Theme.of(context);
    
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Icon(icon, size: 16, color: theme.colorScheme.onSurfaceVariant),
            const SizedBox(width: 4),
            Text(
              label,
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
            ),
          ],
        ),
        const SizedBox(height: 4),
        Text(
          value,
          style: theme.textTheme.titleMedium?.copyWith(
            fontWeight: FontWeight.bold,
          ),
        ),
      ],
    );
  }
}

/// Widget for displaying performance metrics
class PerformanceMetricsCard extends StatelessWidget {
  final QueuePerformanceMetrics metrics;
  final VoidCallback? onTap;

  const PerformanceMetricsCard({
    super.key,
    required this.metrics,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    return AnalyticsCard(
      title: 'Performance Metrics',
      onTap: onTap,
      child: Column(
        children: [
          Row(
            children: [
              Expanded(
                child: _buildMetric(
                  context,
                  'Total Customers',
                  metrics.totalCustomers.toString(),
                  Icons.people_outline,
                ),
              ),
              Expanded(
                child: _buildMetric(
                  context,
                  'Completed',
                  metrics.completedServices.toString(),
                  Icons.check_circle_outline,
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              Expanded(
                child: _buildMetric(
                  context,
                  'Avg Wait',
                  '${metrics.averageWaitTime.inMinutes} min',
                  Icons.schedule,
                ),
              ),
              Expanded(
                child: _buildMetric(
                  context,
                  'Efficiency',
                  '${metrics.efficiencyPercentage.toInt()}%',
                  Icons.trending_up,
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              Expanded(
                child: _buildMetric(
                  context,
                  'Satisfaction',
                  '${metrics.customerSatisfaction.toStringAsFixed(1)}/5',
                  Icons.star_outline,
                ),
              ),
              Expanded(
                child: _buildMetric(
                  context,
                  'Peak Hour',
                  metrics.formattedPeakHour ?? 'N/A',
                  Icons.access_time,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildMetric(BuildContext context, String label, String value, IconData icon) {
    final theme = Theme.of(context);
    
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Icon(icon, size: 16, color: theme.colorScheme.onSurfaceVariant),
            const SizedBox(width: 4),
            Text(
              label,
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
            ),
          ],
        ),
        const SizedBox(height: 4),
        Text(
          value,
          style: theme.textTheme.titleMedium?.copyWith(
            fontWeight: FontWeight.bold,
          ),
        ),
      ],
    );
  }
}

/// Widget for displaying recommendations
class RecommendationsCard extends StatelessWidget {
  final QueueRecommendations recommendations;
  final VoidCallback? onTap;

  const RecommendationsCard({
    super.key,
    required this.recommendations,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final priorityColor = Color(int.parse(recommendations.priorityColor.replaceFirst('#', '0xFF')));
    
    return AnalyticsCard(
      title: 'Recommendations',
      onTap: onTap,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: BoxDecoration(
                  color: priorityColor.withOpacity(0.1),
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: priorityColor.withOpacity(0.3)),
                ),
                child: Text(
                  '${recommendations.priority} Priority',
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: priorityColor,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ),
              const Spacer(),
              Text(
                '${recommendations.confidencePercentage.toInt()}% confidence',
                style: theme.textTheme.bodySmall?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          if (recommendations.hasRecommendations) ...[
            ...recommendations.recommendations.take(3).map((rec) => Padding(
              padding: const EdgeInsets.only(bottom: 8),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Icon(
                    Icons.lightbulb_outline,
                    size: 16,
                    color: theme.colorScheme.primary,
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      rec,
                      style: theme.textTheme.bodyMedium,
                    ),
                  ),
                ],
              ),
            )),
            if (recommendations.recommendations.length > 3)
              Text(
                '+${recommendations.recommendations.length - 3} more recommendations',
                style: theme.textTheme.bodySmall?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                  fontStyle: FontStyle.italic,
                ),
              ),
          ] else
            Row(
              children: [
                Icon(
                  Icons.check_circle,
                  color: Colors.green,
                  size: 20,
                ),
                const SizedBox(width: 8),
                Text(
                  'All systems operating optimally',
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: Colors.green,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ],
            ),
        ],
      ),
    );
  }
}

/// Widget for displaying customer satisfaction feedback form
class SatisfactionFeedbackCard extends StatefulWidget {
  final String queueEntryId;
  final Function(int rating, String? feedback) onSubmit;

  const SatisfactionFeedbackCard({
    super.key,
    required this.queueEntryId,
    required this.onSubmit,
  });

  @override
  State<SatisfactionFeedbackCard> createState() => _SatisfactionFeedbackCardState();
}

class _SatisfactionFeedbackCardState extends State<SatisfactionFeedbackCard> {
  int _rating = 0;
  final _feedbackController = TextEditingController();
  bool _isSubmitting = false;

  @override
  void dispose() {
    _feedbackController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    return AnalyticsCard(
      title: 'Rate Your Experience',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'How was your queue experience?',
            style: theme.textTheme.bodyMedium,
          ),
          const SizedBox(height: 16),
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: List.generate(5, (index) {
              final starIndex = index + 1;
              return GestureDetector(
                onTap: () {
                  setState(() {
                    _rating = starIndex;
                  });
                },
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 4),
                  child: Icon(
                    starIndex <= _rating ? Icons.star : Icons.star_border,
                    color: starIndex <= _rating ? Colors.amber : Colors.grey,
                    size: 32,
                  ),
                ),
              );
            }),
          ),
          const SizedBox(height: 16),
          TextField(
            controller: _feedbackController,
            decoration: const InputDecoration(
              hintText: 'Optional feedback...',
              border: OutlineInputBorder(),
            ),
            maxLines: 3,
          ),
          const SizedBox(height: 16),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: _rating > 0 && !_isSubmitting ? _submitFeedback : null,
              child: _isSubmitting
                  ? const SizedBox(
                      height: 20,
                      width: 20,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Text('Submit Feedback'),
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _submitFeedback() async {
    if (_rating == 0) return;

    setState(() {
      _isSubmitting = true;
    });

    try {
      await widget.onSubmit(_rating, _feedbackController.text.trim().isEmpty 
          ? null : _feedbackController.text.trim());
      
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Thank you for your feedback!'),
            backgroundColor: Colors.green,
          ),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Failed to submit feedback: $e'),
            backgroundColor: Colors.red,
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isSubmitting = false;
        });
      }
    }
  }
}
