import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../controllers/app_controller.dart';
import '../widgets/nearby_salons_widget.dart';
import '../widgets/bottom_nav_bar.dart';

/// Home screen that shows different content based on authentication state
class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Consumer<AppController>(
      builder: (context, appController, child) {
        return Scaffold(
          appBar: AppBar(
            title: const Text('EutoNaFila'),
            backgroundColor: Theme.of(context).colorScheme.surface,
            elevation: 0,
            actions: [
              if (appController.isAnonymousMode) ...[
                TextButton(
                  onPressed: () {
                    Navigator.pushNamed(context, '/login');
                  },
                  child: const Text('Entrar'),
                ),
                IconButton(
                  onPressed: () {
                    Navigator.pushNamed(context, '/register');
                  },
                  icon: const Icon(Icons.person_add),
                ),
              ] else ...[
                IconButton(
                  onPressed: () {
                    Navigator.pushNamed(context, '/profile');
                  },
                  icon: const Icon(Icons.account_circle),
                ),
                IconButton(
                  onPressed: () {
                    appController.auth.logout();
                  },
                  icon: const Icon(Icons.logout),
                ),
              ],
            ],
          ),
          body: SafeArea(
            child: RefreshIndicator(
              onRefresh: () async {
                if (appController.isAnonymousMode) {
                  await appController.anonymous.refresh();
                } else {
                  // Refresh authenticated user data
                  await appController.queue.refresh();
                }
              },
              child: SingleChildScrollView(
                physics: const AlwaysScrollableScrollPhysics(),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    if (appController.isAnonymousMode) ...[
                      // Anonymous user content
                      const SizedBox(height: 16),
                      const NearbySalonsWidget(),
                      const SizedBox(height: 32),
                      _buildAnonymousFooter(context),
                    ] else ...[
                      // Authenticated user content
                      _buildAuthenticatedContent(context, appController),
                    ],
                  ],
                ),
              ),
            ),
          ),
          bottomNavigationBar: const BottomNavBar(currentIndex: 0),
        );
      },
    );
  }

  Widget _buildAnonymousFooter(BuildContext context) {
    final theme = Theme.of(context);
    
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        children: [
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              color: theme.primaryColor.withOpacity(0.1),
              borderRadius: BorderRadius.circular(16),
              border: Border.all(
                color: theme.primaryColor.withOpacity(0.2),
              ),
            ),
            child: Column(
              children: [
                Icon(
                  Icons.account_circle,
                  size: 48,
                  color: theme.primaryColor,
                ),
                const SizedBox(height: 12),
                Text(
                  'Crie sua conta para mais funcionalidades',
                  style: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 8),
                Text(
                  'Salve seus salões favoritos, acompanhe seu histórico e receba notificações.',
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: Colors.grey[600],
                  ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 16),
                Row(
                  children: [
                    Expanded(
                      child: OutlinedButton(
                        onPressed: () {
                          Navigator.pushNamed(context, '/login');
                        },
                        child: const Text('Entrar'),
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: ElevatedButton(
                        onPressed: () {
                          Navigator.pushNamed(context, '/register');
                        },
                        child: const Text('Cadastrar'),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildAuthenticatedContent(BuildContext context, AppController appController) {
    // Content for authenticated users
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Bem-vindo de volta!',
            style: Theme.of(context).textTheme.headlineMedium?.copyWith(
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            'Gerencie suas filas e agendamentos',
            style: Theme.of(context).textTheme.bodyLarge?.copyWith(
              color: Colors.grey[600],
            ),
          ),
          const SizedBox(height: 24),
          
          // User's current queue status, favorites, etc.
          // This would be implemented with the authenticated controllers
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Suas filas ativas',
                    style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 16),
                  Text(
                    'Você não está em nenhuma fila no momento.',
                    style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                      color: Colors.grey[600],
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}