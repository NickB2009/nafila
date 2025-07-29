import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../controllers/anonymous_controller.dart';
import '../../controllers/app_controller.dart';
import '../../models/public_salon.dart';

/// Widget showing all public salons for anonymous users (before login)
class PublicSalonsWidget extends StatelessWidget {
  const PublicSalonsWidget({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    return Consumer<AppController>(
      builder: (context, appController, child) {
        final controller = appController.anonymous;
        
        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header
            _buildHeader(context),
            const SizedBox(height: 16),
            
            // Salons list
            if (controller.isLoading)
              const Center(
                child: Padding(
                  padding: EdgeInsets.all(32.0),
                  child: CircularProgressIndicator(),
                ),
              )
            else if (controller.error != null)
              Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  children: [
                    Text(
                      controller.error!,
                      style: TextStyle(color: theme.colorScheme.error),
                    ),
                    const SizedBox(height: 8),
                    ElevatedButton(
                      onPressed: () async {
                        await controller.loadPublicSalons();
                      },
                      child: const Text('Tentar novamente'),
                    ),
                  ],
                ),
              )
            else if (controller.nearbySalons.isEmpty)
              const Padding(
                padding: EdgeInsets.all(32.0),
                child: Center(
                  child: Text('Nenhum salão encontrado'),
                ),
              )
            else
              Column(
                children: controller.nearbySalons.map((salon) => 
                  _buildSalonCard(context, salon, appController)
                ).toList(),
              ),
          ],
        );
      },
    );
  }

  Widget _buildHeader(BuildContext context) {
    final theme = Theme.of(context);
    
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16.0),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Salões disponíveis',
                  style: theme.textTheme.headlineSmall?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Row(
                  children: [
                    Container(
                      width: 8,
                      height: 8,
                      decoration: const BoxDecoration(
                        color: Colors.green,
                        shape: BoxShape.circle,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Text(
                      'Tempo real • Sem login necessário',
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: Colors.grey[600],
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          // Refresh button
          IconButton(
            onPressed: () async {
              final appController = Provider.of<AppController>(context, listen: false);
              await appController.anonymous.loadPublicSalons();
            },
            icon: const Icon(Icons.refresh),
            tooltip: 'Atualizar lista',
          ),
        ],
      ),
    );
  }

  Widget _buildSalonCard(BuildContext context, PublicSalon salon, AppController appController) {
    final theme = Theme.of(context);
    
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 8.0),
      decoration: BoxDecoration(
        color: theme.colorScheme.surface,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header with name and favorite
            Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Expanded(
                            child: Text(
                              salon.name,
                              style: theme.textTheme.titleLarge?.copyWith(
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                          ),
                          if (salon.isFast) ...[
                            Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 8,
                                vertical: 4,
                              ),
                              decoration: BoxDecoration(
                                color: Colors.amber.withOpacity(0.2),
                                borderRadius: BorderRadius.circular(8),
                              ),
                              child: Text(
                                'RÁPIDO',
                                style: TextStyle(
                                  color: Colors.amber[700],
                                  fontSize: 10,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                            const SizedBox(width: 8),
                          ],
                        ],
                      ),
                      const SizedBox(height: 4),
                      Text(
                        salon.address,
                        style: theme.textTheme.bodyMedium?.copyWith(
                          color: Colors.grey[600],
                        ),
                      ),
                    ],
                  ),
                ),
                IconButton(
                  onPressed: () {
                    // Toggle favorite (would require login)
                    _showLoginRequired(context, 'favoritar salão');
                  },
                  icon: const Icon(Icons.favorite_border),
                  style: IconButton.styleFrom(
                    foregroundColor: Colors.grey[400],
                  ),
                ),
              ],
            ),
            
            const SizedBox(height: 16),
            
            // Stats row
            Row(
              children: [
                _buildStatChip(
                  icon: Icons.access_time,
                  label: salon.currentWaitTimeMinutes != null 
                    ? '${salon.currentWaitTimeMinutes} min'
                    : 'N/A',
                  color: salon.isFast ? Colors.green : Colors.orange,
                ),
                const SizedBox(width: 12),
                _buildStatChip(
                  icon: Icons.people,
                  label: '${salon.queueLength} fila',
                  color: Colors.blue,
                ),
                const SizedBox(width: 12),
                if (salon.distanceKm != null)
                  _buildStatChip(
                    icon: Icons.location_on,
                    label: '${salon.distanceKm!.toStringAsFixed(1)} km',
                    color: Colors.purple,
                  ),
              ],
            ),
            
            const SizedBox(height: 16),
            
            // Check-in button
            Container(
              width: double.infinity,
              height: 56,
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  colors: [
                    Colors.amber[600]!,
                    Colors.amber[500]!,
                  ],
                ),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Material(
                color: Colors.transparent,
                child: InkWell(
                  onTap: () => _handleCheckInAttempt(context, salon, appController),
                  borderRadius: BorderRadius.circular(12),
                  child: Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 16),
                    child: Row(
                      children: [
                        Expanded(
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                'Check-in rápido!',
                                style: theme.textTheme.titleMedium?.copyWith(
                                  color: Colors.white,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                              Text(
                                salon.currentWaitTimeMinutes != null && salon.currentWaitTimeMinutes! <= 5 
                                  ? 'Sem espera' 
                                  : 'Pouca espera',
                                style: theme.textTheme.bodySmall?.copyWith(
                                  color: Colors.white.withOpacity(0.9),
                                ),
                              ),
                            ],
                          ),
                        ),
                        Container(
                          padding: const EdgeInsets.all(8),
                          decoration: BoxDecoration(
                            color: Colors.white.withOpacity(0.2),
                            borderRadius: BorderRadius.circular(8),
                          ),
                          child: const Icon(
                            Icons.login,
                            color: Colors.white,
                            size: 20,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatChip({
    required IconData icon,
    required String label,
    required Color color,
  }) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            icon,
            size: 16,
            color: color,
          ),
          const SizedBox(width: 4),
          Text(
            label,
            style: TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w500,
              color: color,
            ),
          ),
        ],
      ),
    );
  }

  void _handleCheckInAttempt(BuildContext context, PublicSalon salon, AppController appController) {
    // Show login dialog since user is not authenticated
    _showLoginRequired(context, 'fazer check-in');
  }

  void _showLoginRequired(BuildContext context, String action) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('Login necessário'),
          content: Text('Você precisa fazer login para $action.'),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('Cancelar'),
            ),
            ElevatedButton(
              onPressed: () {
                Navigator.of(context).pop();
                // Navigate to login screen
                Navigator.pushNamed(context, '/login');
              },
              child: const Text('Fazer Login'),
            ),
          ],
        );
      },
    );
  }
}