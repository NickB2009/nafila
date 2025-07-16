import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../theme/app_theme.dart';
import '../widgets/bottom_nav_bar.dart';

class DisplayScreen extends StatefulWidget {
  const DisplayScreen({super.key});

  @override
  State<DisplayScreen> createState() => _DisplayScreenState();
}

class _DisplayScreenState extends State<DisplayScreen> {
  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    final themeProvider = Provider.of<ThemeProvider>(context);
    
    return Scaffold(
      appBar: AppBar(
        backgroundColor: colors?.primary ?? theme.colorScheme.primary,
        elevation: 0,
        iconTheme: IconThemeData(color: colors?.onSurface ?? theme.colorScheme.onPrimary),
        title: Text(
          'Display',
          style: theme.textTheme.titleLarge?.copyWith(
            color: colors?.onSurface ?? theme.colorScheme.onPrimary,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      backgroundColor: colors?.primary ?? theme.colorScheme.primary,
      body: SafeArea(
        child: Container(
          decoration: BoxDecoration(
            color: colors?.background ?? theme.colorScheme.surface,
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
                color: colors?.background ?? theme.colorScheme.surface,
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 18),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('Tema', style: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold)),
                      const SizedBox(height: 10),
                      Row(
                        children: [
                          _buildThemeChip(theme, ThemeMode.light, 'Claro', Icons.light_mode, themeProvider, colors),
                          const SizedBox(width: 8),
                          _buildThemeChip(theme, ThemeMode.dark, 'Escuro', Icons.dark_mode, themeProvider, colors),
                          const SizedBox(width: 8),
                          _buildThemeChip(theme, ThemeMode.system, 'Sistema', Icons.phone_android, themeProvider, colors),
                        ],
                      ),
                      const SizedBox(height: 22),
                      Text('Tamanho da Fonte', style: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold)),
                      const SizedBox(height: 10),
                      Row(
                        children: [
                          _buildFontChip(theme, 0.9, 'Pequeno', themeProvider, colors),
                          const SizedBox(width: 8),
                          _buildFontChip(theme, 1.0, 'Médio', themeProvider, colors),
                          const SizedBox(width: 8),
                          _buildFontChip(theme, 1.2, 'Grande', themeProvider, colors),
                        ],
                      ),
                      const SizedBox(height: 22),
                      SwitchListTile.adaptive(
                        value: themeProvider.highContrast,
                        onChanged: (v) => themeProvider.setHighContrast(v),
                        title: Text('Modo alto contraste', style: theme.textTheme.bodyLarge),
                        secondary: Icon(Icons.contrast, color: colors?.primary ?? theme.colorScheme.primary),
                        contentPadding: EdgeInsets.zero,
                      ),
                      const Divider(height: 1),
                      SwitchListTile.adaptive(
                        value: themeProvider.animations,
                        onChanged: (v) => themeProvider.setAnimations(v),
                        title: Text('Animações', style: theme.textTheme.bodyLarge),
                        secondary: Icon(Icons.animation, color: colors?.primary ?? theme.colorScheme.primary),
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
              _buildPreviewCard(theme, themeProvider, colors),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildThemeChip(ThemeData theme, ThemeMode mode, String label, IconData icon, ThemeProvider provider, dynamic colors) {
    final isSelected = provider.themeMode == mode;
    return ChoiceChip(
      label: Row(
        mainAxisSize: MainAxisSize.min,
        children: [Icon(icon, size: 18), const SizedBox(width: 4), Text(label)],
      ),
      selected: isSelected,
      onSelected: (_) => provider.setThemeMode(mode),
      selectedColor: colors?.primary ?? theme.colorScheme.primary,
      labelStyle: theme.textTheme.bodyMedium?.copyWith(
        color: isSelected ? (colors?.onSurface ?? theme.colorScheme.onPrimary) : (colors?.onSurface ?? theme.colorScheme.onSurface),
        fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
      ),
      backgroundColor: colors?.background ?? theme.colorScheme.surface,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
      elevation: isSelected ? 2 : 0,
    );
  }

  Widget _buildFontChip(ThemeData theme, double size, String label, ThemeProvider provider, dynamic colors) {
    final isSelected = provider.fontSize == size;
    return ChoiceChip(
      label: Text(label),
      selected: isSelected,
      onSelected: (_) => provider.setFontSize(size),
      selectedColor: colors?.primary ?? theme.colorScheme.primary,
      labelStyle: theme.textTheme.bodyMedium?.copyWith(
        color: isSelected ? (colors?.onSurface ?? theme.colorScheme.onPrimary) : (colors?.onSurface ?? theme.colorScheme.onSurface),
        fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
      ),
      backgroundColor: colors?.background ?? theme.colorScheme.surface,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
      elevation: isSelected ? 2 : 0,
    );
  }

  Widget _buildPreviewCard(ThemeData theme, ThemeProvider provider, dynamic colors) {
    Color previewBg = provider.highContrast ? Colors.black : (colors?.background ?? theme.colorScheme.surface);
    Color previewText = provider.highContrast ? Colors.yellow : (colors?.onSurface ?? theme.colorScheme.onSurface);
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
                fontSize: 18 * provider.fontSize,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              'Veja como suas preferências afetam a aparência do app.',
              style: theme.textTheme.bodyLarge?.copyWith(
                color: previewText,
                fontSize: 15 * provider.fontSize,
              ),
            ),
          ],
        ),
      ),
    );
  }
} 