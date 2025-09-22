import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../controllers/queue_controller.dart';
import '../widgets/queue_card.dart';
import '../../models/queue_entry.dart';
import '../theme/app_theme.dart';
import '../widgets/bottom_nav_bar.dart';

/// Main screen displaying the queue management interface
class QueueScreen extends StatelessWidget {
  const QueueScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    
    return Scaffold(
      appBar: AppBar(
        backgroundColor: colors?.primary ?? theme.colorScheme.primary,
        title: Text(
          'Gerenciamento de Fila',
          style: theme.textTheme.titleLarge?.copyWith(
            color: colors?.onSurface ?? theme.colorScheme.onPrimary,
          ),
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            color: colors?.onSurface ?? theme.colorScheme.onPrimary,
            onPressed: () {
              // TODO: Implement refresh functionality with real queue service
            },
            tooltip: 'Atualizar',
          ),
        ],
      ),
      backgroundColor: colors?.background ?? theme.colorScheme.surface,
      body: SafeArea(
        child: LayoutBuilder(
          builder: (context, constraints) {
            return Consumer<QueueController>(
              builder: (context, queueController, child) {
                if (queueController.isLoading) {
                  return Center(
                    child: CircularProgressIndicator(
                      color: theme.colorScheme.primary,
                    ),
                  );
                }

                if (queueController.queueEntries.isEmpty) {
                  return _buildEmptyState(context);
                }

                return Column(
                  children: [
                    // Queue stats
                    _buildQueueStats(
                        context, queueController, constraints.maxWidth),

                    // Queue list
                    Expanded(
                      child: _buildQueueList(
                          context, queueController, constraints.maxWidth),
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
      BuildContext context, QueueController controller, double width) {
    final theme = Theme.of(context);
    final isCompact = width < 400;
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);

    return Container(
      margin: EdgeInsets.all(isCompact ? 12 : 16),
      padding: EdgeInsets.all(isCompact ? 12 : 16),
      decoration: BoxDecoration(
        color: (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.08),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.2),
        ),
      ),
      child: Row(
        children: [
          Expanded(
            child: _buildStatItem(
              context,
              'Aguardando',
              '0', // TODO: Calculate from real queue data
              Icons.schedule,
              // fallback: use secondary or primary from palette
              colors?.secondary ?? theme.colorScheme.tertiary,
              isCompact,
            ),
          ),
          SizedBox(width: isCompact ? 16 : 24),
          Expanded(
            child: _buildStatItem(
              context,
              'Em Atendimento',
              '0', // TODO: Calculate from real queue data
              Icons.person_outline,
              colors?.secondary ?? theme.colorScheme.secondary,
              isCompact,
            ),
          ),
          SizedBox(width: isCompact ? 16 : 24),
          Expanded(
            child: _buildStatItem(
              context,
              'Total',
              controller.queueEntries.length.toString(),
              Icons.people,
              colors?.primary ?? theme.colorScheme.primary,
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
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);

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
            color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
            fontSize: isCompact ? 11 : 12,
          ),
        ),
      ],
    );
  }

  Widget _buildQueueList(
      BuildContext context, QueueController controller, double width) {
    // Sort entries: in-service first, then waiting by position, then completed
    // TODO: Convert controller.queueEntries to QueueEntry objects
    final sortedEntries = <QueueEntry>[];
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
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);

    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.people_outline,
            size: 64,
            color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
          ),
          const SizedBox(height: 16),
          Text(
            'Ninguém na fila',
            style: theme.textTheme.headlineSmall?.copyWith(
              color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            'Toque no botão + para adicionar alguém',
            style: theme.textTheme.bodyMedium?.copyWith(
              color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
            ),
          ),
        ],
      ),
    );
  }

  void _showAddPersonDialog(BuildContext context) {
    final nameController = TextEditingController();
    final theme = Theme.of(context);
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);

    showDialog(
      context: context,
      builder: (dialogContext) => Theme(
        data: theme.copyWith(
          dialogTheme: DialogThemeData(
            backgroundColor: colors?.background ?? theme.colorScheme.surface,
            surfaceTintColor: Colors.transparent,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(16),
            ),
          ),
        ),
        child: AlertDialog(
          title: Text(
            'Adicionar Pessoa à Fila',
            style: theme.textTheme.titleLarge?.copyWith(
              color: theme.colorScheme.onSurface,
            ),
          ),
          content: TextField(
            controller: nameController,
            decoration: InputDecoration(
              labelText: 'Nome',
              labelStyle: TextStyle(color: theme.colorScheme.onSurfaceVariant),
              hintText: 'Digite o nome da pessoa',
              hintStyle: TextStyle(color: theme.colorScheme.onSurfaceVariant),
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
              ),
              focusedBorder: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
                borderSide: BorderSide(color: theme.colorScheme.primary, width: 2),
              ),
            ),
            style: TextStyle(color: theme.colorScheme.onSurface),
            autofocus: true,
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(dialogContext).pop(),
              child: Text(
                'Cancelar',
                style: theme.textTheme.labelLarge?.copyWith(
                  color: colors?.primary ?? theme.colorScheme.primary,
                ),
              ),
            ),
            FilledButton(
              onPressed: () {
                if (nameController.text.trim().isNotEmpty) {
                  // TODO: Implement add to queue with real service
                  Navigator.of(dialogContext).pop();
                }
              },
              child: Text(
                'Adicionar',
                style: theme.textTheme.labelLarge?.copyWith(
                  color: theme.colorScheme.onPrimary,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _showEntryActions(BuildContext context, QueueEntry entry) {
    final theme = Theme.of(context);
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    
    showModalBottomSheet(
      context: context,
      backgroundColor: colors?.background ?? theme.colorScheme.surface,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      builder: (bottomSheetContext) => Container(
        decoration: BoxDecoration(
          color: colors?.background ?? theme.colorScheme.surface,
          borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
        ),
        padding: const EdgeInsets.all(16),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 40,
              height: 4,
              margin: const EdgeInsets.only(bottom: 16),
              decoration: BoxDecoration(
                color: theme.colorScheme.outline.withOpacity(0.3),
                borderRadius: BorderRadius.circular(2),
              ),
            ),
            Text(
              entry.name,
              style: theme.textTheme.titleLarge?.copyWith(
                color: theme.colorScheme.onSurface,
              ),
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
                  style: theme.textTheme.bodyLarge?.copyWith(
                    color: theme.colorScheme.onSurface,
                  ),
                ),
                onTap: () {
                  // TODO: Implement update status with real service
                  Navigator.of(bottomSheetContext).pop();
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
                  style: theme.textTheme.bodyLarge?.copyWith(
                    color: theme.colorScheme.onSurface,
                  ),
                ),
                onTap: () {
                  // TODO: Implement update status with real service
                  Navigator.of(bottomSheetContext).pop();
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
                // TODO: Implement remove from queue with real service
                Navigator.of(bottomSheetContext).pop();
              },
            ),
            const SizedBox(height: 16),
          ],
        ),
      ),
    );
  }
}
