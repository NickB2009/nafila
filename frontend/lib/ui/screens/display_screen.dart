import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

class DisplayScreen extends StatefulWidget {
  const DisplayScreen({super.key});

  @override
  State<DisplayScreen> createState() => _DisplayScreenState();
}

class _DisplayScreenState extends State<DisplayScreen> {
  int themeMode = 2; // 0: Claro, 1: Escuro, 2: Sistema
  int fontSize = 1; // 0: Pequeno, 1: Médio, 2: Grande
  bool highContrast = false;
  bool animations = true;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Scaffold(
      appBar: AppBar(
        backgroundColor: AppTheme.primaryColor,
        elevation: 0,
        iconTheme: IconThemeData(color: theme.colorScheme.onPrimary),
        title: Text(
          'Display',
          style: theme.textTheme.titleLarge?.copyWith(
            color: theme.colorScheme.onPrimary,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      backgroundColor: AppTheme.primaryColor,
      body: SafeArea(
        child: Container(
          decoration: BoxDecoration(
            color: theme.colorScheme.surface,
            borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
          ),
          child: ListView(
            padding: const EdgeInsets.all(24),
            children: [
              Text(
                'Personalize a aparência do aplicativo conforme sua preferência.',
                style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 24),
              Card(
                elevation: 2,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                color: theme.colorScheme.surfaceVariant,
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 18),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('Tema', style: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold)),
                      const SizedBox(height: 10),
                      Row(
                        children: [
                          _buildThemeChip(theme, 0, 'Claro', Icons.light_mode),
                          const SizedBox(width: 8),
                          _buildThemeChip(theme, 1, 'Escuro', Icons.dark_mode),
                          const SizedBox(width: 8),
                          _buildThemeChip(theme, 2, 'Sistema', Icons.phone_android),
                        ],
                      ),
                      const SizedBox(height: 22),
                      Text('Tamanho da Fonte', style: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold)),
                      const SizedBox(height: 10),
                      Row(
                        children: [
                          _buildFontChip(theme, 0, 'Pequeno'),
                          const SizedBox(width: 8),
                          _buildFontChip(theme, 1, 'Médio'),
                          const SizedBox(width: 8),
                          _buildFontChip(theme, 2, 'Grande'),
                        ],
                      ),
                      const SizedBox(height: 22),
                      SwitchListTile.adaptive(
                        value: highContrast,
                        onChanged: (v) => setState(() => highContrast = v),
                        title: Text('Modo alto contraste', style: theme.textTheme.bodyLarge),
                        secondary: Icon(Icons.contrast, color: AppTheme.primaryColor),
                        contentPadding: EdgeInsets.zero,
                      ),
                      Divider(height: 1),
                      SwitchListTile.adaptive(
                        value: animations,
                        onChanged: (v) => setState(() => animations = v),
                        title: Text('Animações', style: theme.textTheme.bodyLarge),
                        secondary: Icon(Icons.animation, color: AppTheme.primaryColor),
                        contentPadding: EdgeInsets.zero,
                      ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 32),
              Text(
                'Pré-visualização',
                style: theme.textTheme.titleSmall?.copyWith(fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 12),
              _buildPreviewCard(theme),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildThemeChip(ThemeData theme, int value, String label, IconData icon) {
    final isSelected = themeMode == value;
    return ChoiceChip(
      label: Row(
        mainAxisSize: MainAxisSize.min,
        children: [Icon(icon, size: 18), const SizedBox(width: 4), Text(label)],
      ),
      selected: isSelected,
      onSelected: (_) => setState(() => themeMode = value),
      selectedColor: AppTheme.primaryColor,
      labelStyle: theme.textTheme.bodyMedium?.copyWith(
        color: isSelected ? theme.colorScheme.onPrimary : theme.colorScheme.onSurface,
        fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
      ),
      backgroundColor: theme.colorScheme.surface,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
      elevation: isSelected ? 2 : 0,
    );
  }

  Widget _buildFontChip(ThemeData theme, int value, String label) {
    final isSelected = fontSize == value;
    return ChoiceChip(
      label: Text(label),
      selected: isSelected,
      onSelected: (_) => setState(() => fontSize = value),
      selectedColor: AppTheme.primaryColor,
      labelStyle: theme.textTheme.bodyMedium?.copyWith(
        color: isSelected ? theme.colorScheme.onPrimary : theme.colorScheme.onSurface,
        fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
      ),
      backgroundColor: theme.colorScheme.surface,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
      elevation: isSelected ? 2 : 0,
    );
  }

  Widget _buildPreviewCard(ThemeData theme) {
    double fontScale = fontSize == 0 ? 0.9 : fontSize == 2 ? 1.2 : 1.0;
    Color previewBg = highContrast ? Colors.black : theme.colorScheme.surfaceVariant;
    Color previewText = highContrast ? Colors.yellow : theme.colorScheme.onSurface;
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      color: previewBg,
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Exemplo de visualização',
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
                color: previewText,
                fontSize: 18 * fontScale,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              'Veja como suas preferências afetam a aparência do app.',
              style: theme.textTheme.bodyLarge?.copyWith(
                color: previewText,
                fontSize: 15 * fontScale,
              ),
            ),
          ],
        ),
      ),
    );
  }
} 