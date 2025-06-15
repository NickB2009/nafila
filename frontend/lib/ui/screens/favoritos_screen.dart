import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

class FavoritosScreen extends StatefulWidget {
  const FavoritosScreen({super.key});

  @override
  State<FavoritosScreen> createState() => _FavoritosScreenState();
}

class _FavoritosScreenState extends State<FavoritosScreen> {
  List<Map<String, dynamic>> favoritos = [
    {
      'name': 'Market at Mirada',
      'address': '30921 Mirada Blvd, San Antonio, FL',
      'waitTime': 24,
      'distance': 10.9,
      'isOpen': true,
      'closingTime': '18:00',
    },
    {
      'name': 'Cortez Commons',
      'address': '123 Cortez Ave, San Antonio, FL',
      'waitTime': 8,
      'distance': 5.2,
      'isOpen': true,
      'closingTime': '20:00',
    },
  ];

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Scaffold(
      appBar: AppBar(
        backgroundColor: AppTheme.primaryColor,
        elevation: 0,
        iconTheme: IconThemeData(color: theme.colorScheme.onPrimary),
        title: Text(
          'Favoritos',
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
          child: favoritos.isEmpty
              ? _buildEmptyState(theme)
              : ListView.separated(
                  padding: const EdgeInsets.all(24),
                  itemCount: favoritos.length,
                  separatorBuilder: (_, __) => const SizedBox(height: 18),
                  itemBuilder: (context, i) => _buildSalonCard(theme, favoritos[i], i),
                ),
        ),
      ),
    );
  }

  Widget _buildEmptyState(ThemeData theme) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 80),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.star_border, size: 60, color: AppTheme.primaryColor),
            const SizedBox(height: 24),
            Text(
              'Nenhum salão favorito ainda',
              style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 10),
            Text(
              'Toque na estrela em um salão para adicioná-lo aos seus favoritos.',
              style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSalonCard(ThemeData theme, Map<String, dynamic> salon, int index) {
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      color: theme.colorScheme.surfaceVariant,
      child: Padding(
        padding: const EdgeInsets.all(18),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(Icons.store, color: AppTheme.primaryColor, size: 28),
                const SizedBox(width: 10),
                Expanded(
                  child: Text(
                    salon['name'],
                    style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
                  ),
                ),
                Icon(Icons.star, color: AppTheme.primaryColor, size: 22),
              ],
            ),
            const SizedBox(height: 6),
            Text(
              salon['address'],
              style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant),
            ),
            const SizedBox(height: 10),
            Row(
              children: [
                Text(
                  salon['isOpen'] ? 'Aberto' : 'Fechado',
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: salon['isOpen'] ? Colors.green : theme.colorScheme.error,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(width: 8),
                Text('• Fecha às ${salon['closingTime']}', style: theme.textTheme.bodyMedium),
                const SizedBox(width: 8),
                Icon(Icons.directions_car, size: 16, color: theme.colorScheme.onSurfaceVariant),
                const SizedBox(width: 2),
                Text('${salon['distance'].toStringAsFixed(1)} km', style: theme.textTheme.bodyMedium),
              ],
            ),
            const SizedBox(height: 14),
            Row(
              children: [
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: AppTheme.primaryColor.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Row(
                    children: [
                      Icon(Icons.timer, size: 16, color: AppTheme.primaryColor),
                      const SizedBox(width: 4),
                      Text('${salon['waitTime']} min', style: theme.textTheme.labelMedium?.copyWith(color: AppTheme.primaryColor)),
                    ],
                  ),
                ),
                const Spacer(),
                TextButton.icon(
                  onPressed: () {
                    setState(() {
                      favoritos.removeAt(index);
                    });
                  },
                  icon: const Icon(Icons.delete_outline),
                  label: const Text('Remover dos favoritos'),
                  style: TextButton.styleFrom(
                    foregroundColor: theme.colorScheme.error,
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
} 