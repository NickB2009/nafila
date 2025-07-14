import 'package:flutter/material.dart';
import '../widgets/bottom_nav_bar.dart';

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({super.key});

  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  bool _editMode = false;
  List<Map<String, String>> notifications = [
    {
      'title': 'Dois cortes = R\$5 de desconto*',
      'date': '14 de maio de 2025',
    },
    {
      'title': 'Quer R\$2 de desconto? Oferta exclusiva para você*!',
      'date': '8 de maio de 2025',
    },
    {
      'title': 'Fazendo você ficar incrível, esse é nosso objetivo.',
      'date': '6 de maio de 2025',
    },
  ];

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors?.forBrightness(brightness);
    return Scaffold(
      backgroundColor: colors?.background ?? theme.colorScheme.surface,
      appBar: AppBar(
        backgroundColor: colors?.background ?? theme.colorScheme.surface,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_ios_new),
          color: colors?.onSurface ?? theme.colorScheme.onSurface,
          onPressed: () => Navigator.of(context).pop(),
        ),
        title: Row(
          children: [
            Container(
              width: 32,
              height: 32,
              decoration: BoxDecoration(
                color: colors?.secondary ?? theme.colorScheme.secondary,
                borderRadius: BorderRadius.circular(16),
              ),
              child: Icon(
                Icons.person,
                color: colors?.onSurface ?? theme.colorScheme.onSecondary,
                size: 20,
              ),
            ),
            const SizedBox(width: 8),
            Text(
              '1',
              style: theme.textTheme.titleLarge?.copyWith(
                color: colors?.onSurface ?? theme.colorScheme.onSurface,
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () {
              setState(() {
                _editMode = !_editMode;
              });
            },
            child: Text(
              _editMode ? 'Concluir' : 'Editar',
              style: theme.textTheme.labelLarge?.copyWith(
                color: colors?.primary ?? theme.colorScheme.primary,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        ],
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const SizedBox(height: 8),
              Text(
                'Notificações',
                style: theme.textTheme.headlineMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: colors?.onSurface ?? theme.colorScheme.onSurface,
                ),
              ),
              const SizedBox(height: 16),
              Expanded(
                child: Container(
                  decoration: BoxDecoration(
                    color: colors?.background ?? theme.colorScheme.surface,
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: ListView.separated(
                    padding: const EdgeInsets.symmetric(vertical: 8),
                    itemCount: notifications.length,
                    separatorBuilder: (context, index) => Divider(
                      color: (colors?.primary ?? theme.dividerColor).withOpacity(0.15),
                      height: 1,
                      indent: 16,
                      endIndent: 16,
                    ),
                    itemBuilder: (context, index) {
                      final notification = notifications[index];
                      return ListTile(
                        leading: Container(
                          width: 40,
                          height: 40,
                          decoration: BoxDecoration(
                            color: (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.1),
                            borderRadius: BorderRadius.circular(20),
                          ),
                          child: Icon(
                            Icons.notifications,
                            color: colors?.primary ?? theme.colorScheme.primary,
                            size: 24,
                          ),
                        ),
                        title: Text(
                          notification['title']!,
                          style: theme.textTheme.bodyLarge?.copyWith(
                            color: colors?.onSurface ?? theme.colorScheme.onSurface,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                        subtitle: Text(
                          notification['date']!,
                          style: theme.textTheme.bodySmall?.copyWith(
                            color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
                          ),
                        ),
                        trailing: _editMode
                            ? IconButton(
                                icon: Icon(Icons.delete, color: theme.colorScheme.error),
                                onPressed: () {
                                  setState(() {
                                    notifications.removeAt(index);
                                  });
                                },
                              )
                            : Icon(
                                Icons.chevron_right,
                                color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
                              ),
                        onTap: () {},
                        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                      );
                    },
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
} 