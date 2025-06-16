import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../view_models/mock_queue_notifier.dart';
import '../widgets/queue_card.dart';
import '../../models/queue_entry.dart';
import '../theme/app_theme.dart';

/// Main screen displaying the queue management interface
class QueueScreen extends StatelessWidget {
  const QueueScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    return Scaffold(
      appBar: AppBar(
        title: Text(
          'Gerenciamento de Fila',
          style: theme.textTheme.titleLarge?.copyWith(
            color: theme.colorScheme.onPrimary,
          ),
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: () {
              context.read<MockQueueNotifier>().refresh();
            },
            tooltip: 'Atualizar',
          ),
        ],
      ),
      body: SafeArea(
        child: LayoutBuilder(
          builder: (context, constraints) {
            return Consumer<MockQueueNotifier>(
              builder: (context, queueNotifier, child) {
                if (queueNotifier.isLoading) {
                  return Center(
                    child: CircularProgressIndicator(
                      color: theme.colorScheme.primary,
                    ),
                  );
                }

                if (queueNotifier.entries.isEmpty) {
                  return _buildEmptyState(context);
                }

                return Column(
                  children: [
                    // Queue stats
                    _buildQueueStats(
                        context, queueNotifier, constraints.maxWidth),

                    // Queue list
                    Expanded(
                      child: _buildQueueList(
                          context, queueNotifier, constraints.maxWidth),
                    ),
                  ],
                );
              },
            );
          },
        ),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _showAddPersonDialog(context),
        tooltip: 'Adicionar Pessoa',
        child: const Icon(Icons.add),
      ),
    );
  }

  Widget _buildQueueStats(
      BuildContext context, MockQueueNotifier notifier, double width) {
    final theme = Theme.of(context);
    final isCompact = width < 400;

    return Container(
      margin: EdgeInsets.all(isCompact ? 12 : 16),
      padding: EdgeInsets.all(isCompact ? 12 : 16),
      decoration: BoxDecoration(
        color: theme.colorScheme.primaryContainer.withOpacity(0.3),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: theme.colorScheme.primary.withOpacity(0.2),
        ),
      ),
      child: Row(
        children: [
          Expanded(
            child: _buildStatItem(
              context,
              'Aguardando',
              notifier.waitingCount.toString(),
              Icons.schedule,
              theme.colorScheme.tertiary,
              isCompact,
            ),
          ),
          SizedBox(width: isCompact ? 16 : 24),
          Expanded(
            child: _buildStatItem(
              context,
              'Em Atendimento',
              notifier.inServiceCount.toString(),
              Icons.person_outline,
              theme.colorScheme.secondary,
              isCompact,
            ),
          ),
          SizedBox(width: isCompact ? 16 : 24),
          Expanded(
            child: _buildStatItem(
              context,
              'Total',
              notifier.entries.length.toString(),
              Icons.people,
              theme.colorScheme.primary,
              isCompact,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatItem(BuildContext context, String label, String value,
      IconData icon, Color color, bool isCompact) {
    final theme = Theme.of(context);

    return Column(
      children: [
        Icon(
          icon,
          size: isCompact ? 20 : 24,
          color: color,
        ),
        const SizedBox(height: 4),
        Text(
          value,
          style: theme.textTheme.headlineSmall?.copyWith(
            fontWeight: FontWeight.bold,
            color: color,
            fontSize: isCompact ? 18 : 24,
          ),
        ),
        Text(
          label,
          style: theme.textTheme.bodySmall?.copyWith(
            color: theme.colorScheme.onSurfaceVariant,
            fontSize: isCompact ? 11 : 12,
          ),
        ),
      ],
    );
  }

  Widget _buildQueueList(
      BuildContext context, MockQueueNotifier notifier, double width) {
    // Sort entries: in-service first, then waiting by position, then completed
    final sortedEntries = List<QueueEntry>.from(notifier.entries);
    sortedEntries.sort((a, b) {
      if (a.status == QueueStatus.inService &&
          b.status != QueueStatus.inService) {
        return -1;
      }
      if (b.status == QueueStatus.inService &&
          a.status != QueueStatus.inService) {
        return 1;
      }
      if (a.status == QueueStatus.waiting && b.status == QueueStatus.waiting) {
        return a.position.compareTo(b.position);
      }
      if (a.status == QueueStatus.waiting &&
          b.status == QueueStatus.completed) {
        return -1;
      }
      if (a.status == QueueStatus.completed &&
          b.status == QueueStatus.waiting) {
        return 1;
      }
      return a.joinTime.compareTo(b.joinTime);
    });

    return ListView.builder(
      padding: const EdgeInsets.only(bottom: 80), // Space for FAB
      itemCount: sortedEntries.length,
      itemBuilder: (context, index) {
        final entry = sortedEntries[index];
        return QueueCard(
          entry: entry,
          width: width,
          onTap: () => _showEntryActions(context, entry),
        );
      },
    );
  }

  Widget _buildEmptyState(BuildContext context) {
    final theme = Theme.of(context);

    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.people_outline,
            size: 64,
            color: theme.colorScheme.onSurfaceVariant,
          ),
          const SizedBox(height: 16),
          Text(
            'Ninguém na fila',
            style: theme.textTheme.headlineSmall?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            'Toque no botão + para adicionar alguém',
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
          ),
        ],
      ),
    );
  }

  void _showAddPersonDialog(BuildContext context) {
    final nameController = TextEditingController();
    final theme = Theme.of(context);

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(
          'Adicionar Pessoa à Fila',
          style: theme.textTheme.titleLarge,
        ),
        content: TextField(
          controller: nameController,
          decoration: InputDecoration(
            labelText: 'Nome',
            hintText: 'Digite o nome da pessoa',
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
            ),
          ),
          autofocus: true,
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: Text(
              'Cancelar',
              style: theme.textTheme.labelLarge?.copyWith(
                color: theme.colorScheme.primary,
              ),
            ),
          ),
          FilledButton(
            onPressed: () {
              if (nameController.text.trim().isNotEmpty) {
                context
                    .read<MockQueueNotifier>()
                    .addToQueue(nameController.text.trim());
                Navigator.of(context).pop();
              }
            },
            child: Text(
              'Adicionar',
              style: theme.textTheme.labelLarge,
            ),
          ),
        ],
      ),
    );
  }

  void _showEntryActions(BuildContext context, QueueEntry entry) {
    final theme = Theme.of(context);
    
    showModalBottomSheet(
      context: context,
      builder: (context) => Container(
        padding: const EdgeInsets.all(16),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(
              entry.name,
              style: theme.textTheme.titleLarge,
            ),
            const SizedBox(height: 16),
            if (entry.status == QueueStatus.waiting) ...[
              ListTile(
                leading: Icon(
                  Icons.person_outline,
                  color: AppTheme.statusColors['inService'],
                ),
                title: Text(
                  'Iniciar Atendimento',
                  style: theme.textTheme.bodyLarge,
                ),
                onTap: () {
                  context
                      .read<MockQueueNotifier>()
                      .updateStatus(entry.id, QueueStatus.inService);
                  Navigator.of(context).pop();
                },
              ),
            ],
            if (entry.status == QueueStatus.inService) ...[
              ListTile(
                leading: Icon(
                  Icons.check_circle_outline,
                  color: AppTheme.statusColors['completed'],
                ),
                title: Text(
                  'Concluir Atendimento',
                  style: theme.textTheme.bodyLarge,
                ),
                onTap: () {
                  context
                      .read<MockQueueNotifier>()
                      .updateStatus(entry.id, QueueStatus.completed);
                  Navigator.of(context).pop();
                },
              ),
            ],
            ListTile(
              leading: Icon(
                Icons.delete_outline,
                color: theme.colorScheme.error,
              ),
              title: Text(
                'Remover da Fila',
                style: theme.textTheme.bodyLarge?.copyWith(
                  color: theme.colorScheme.error,
                ),
              ),
              onTap: () {
                context.read<MockQueueNotifier>().removeFromQueue(entry.id);
                Navigator.of(context).pop();
              },
            ),
            const SizedBox(height: 16),
          ],
        ),
      ),
    );
  }
}
