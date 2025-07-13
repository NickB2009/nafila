import 'package:flutter/material.dart';
import '../widgets/bottom_nav_bar.dart';
import '../../models/salon.dart';
import '../../models/salon_service.dart';
import '../../models/salon_contact.dart';
import '../../models/salon_hours.dart';
import '../../models/salon_review.dart';
import 'salon_details_screen.dart';
import 'check_in_screen.dart';

SalonColors _randomSalonColors(int seed) {
  final palettes = [
    SalonColors(primary: Colors.redAccent, secondary: Colors.orange, background: Colors.red.shade50, onSurface: Colors.red.shade900),
    SalonColors(primary: Colors.blueAccent, secondary: Colors.cyan, background: Colors.blue.shade50, onSurface: Colors.blue.shade900),
    SalonColors(primary: Colors.green, secondary: Colors.teal, background: Colors.green.shade50, onSurface: Colors.green.shade900),
    SalonColors(primary: Colors.purple, secondary: Colors.pinkAccent, background: Colors.purple.shade50, onSurface: Colors.purple.shade900),
    SalonColors(primary: Colors.amber, secondary: Colors.deepOrange, background: Colors.amber.shade50, onSurface: Colors.amber.shade900),
    SalonColors(primary: Colors.indigo, secondary: Colors.lime, background: Colors.indigo.shade50, onSurface: Colors.indigo.shade900),
  ];
  return palettes[seed % palettes.length];
}

class FavoritosScreen extends StatefulWidget {
  const FavoritosScreen({super.key});

  @override
  State<FavoritosScreen> createState() => _FavoritosScreenState();
}

class _FavoritosScreenState extends State<FavoritosScreen> with SingleTickerProviderStateMixin {
  late AnimationController _animationController;
  late Animation<double> _fadeAnimation;
  late Animation<Offset> _slideAnimation;

  final List<Salon> favoritos = [
    Salon(
      name: 'Market at Mirada',
      address: '30921 Mirada Blvd, San Antonio, FL',
      waitTime: 24,
      distance: 10.9,
      isOpen: true,
      closingTime: '18:00',
      isFavorite: true,
      queueLength: 5,
      colors: _randomSalonColors(0),
    ),
    Salon(
      name: 'Cortez Commons',
      address: '123 Cortez Ave, San Antonio, FL',
      waitTime: 8,
      distance: 5.2,
      isOpen: true,
      closingTime: '20:00',
      isFavorite: true,
      queueLength: 2,
      colors: _randomSalonColors(1),
    ),
  ];

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 800),
      vsync: this,
    );

    _fadeAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(parent: _animationController, curve: Curves.easeOut),
    );

    _slideAnimation = Tween<Offset>(
      begin: const Offset(0, 0.2),
      end: Offset.zero,
    ).animate(
      CurvedAnimation(parent: _animationController, curve: Curves.easeOut),
    );

    _animationController.forward();
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Scaffold(
      backgroundColor: theme.colorScheme.primary,
      body: SafeArea(
        child: Column(
          children: [
            // Header
            Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  IconButton(
                    icon: Container(
                      padding: const EdgeInsets.all(8),
                      decoration: BoxDecoration(
                        color: theme.colorScheme.onPrimary.withOpacity(0.2),
                        shape: BoxShape.circle,
                      ),
                      child: Icon(
                        Icons.arrow_back,
                        color: theme.colorScheme.onPrimary,
                      ),
                    ),
                    onPressed: () => Navigator.of(context).pop(),
                  ),
                  const SizedBox(width: 16),
                  Text(
          'Favoritos',
                    style: theme.textTheme.headlineMedium?.copyWith(
            color: theme.colorScheme.onPrimary,
            fontWeight: FontWeight.bold,
          ),
                  ),
                ],
        ),
      ),
            // Content
            Expanded(
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
                        itemBuilder: (context, index) => _buildSalonCard(theme, favoritos[index], index),
                      ),
              ),
                ),
          ],
        ),
      ),
      bottomNavigationBar: const BottomNavBar(currentIndex: 2),
    );
  }

  Widget _buildEmptyState(ThemeData theme) {
    return FadeTransition(
      opacity: _fadeAnimation,
      child: SlideTransition(
        position: _slideAnimation,
        child: Center(
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 80),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
                Icon(
                  Icons.favorite_border,
                  size: 64,
                  color: theme.colorScheme.primary.withOpacity(0.5),
                ),
            const SizedBox(height: 24),
            Text(
              'Nenhum salão favorito ainda',
                  style: theme.textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 10),
            Text(
              'Toque na estrela em um salão para adicioná-lo aos seus favoritos.',
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
              textAlign: TextAlign.center,
            ),
          ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildSalonCard(ThemeData theme, Salon salon, int index) {
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0.0, end: 1.0),
      duration: Duration(milliseconds: 600 + (index * 200)),
      builder: (context, value, child) {
        return Transform.translate(
          offset: Offset(0, 20 * (1 - value)),
          child: Opacity(
            opacity: value,
            child: child,
          ),
        );
      },
      child: Card(
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(16),
          side: BorderSide(
            color: theme.colorScheme.outline.withOpacity(0.2),
          ),
        ),
        child: InkWell(
          onTap: () {
            Navigator.of(context).push(
              MaterialPageRoute(
                builder: (_) => SalonDetailsScreen(
                  salon: salon,
                  services: [
                    SalonService(
                      id: '1',
                      name: 'Corte Feminino',
                      description: 'Corte e finalização',
                      price: 80.0,
                      durationMinutes: 60,
                      categories: ['Corte'],
                    ),
                    SalonService(
                      id: '2',
                      name: 'Coloração',
                      description: 'Coloração completa',
                      price: 150.0,
                      durationMinutes: 120,
                      categories: ['Coloração'],
                    ),
                  ],
                  contact: SalonContact(
                    phone: '(555) 123-4567',
                    email: 'contato@salon.com',
                    website: 'www.salon.com',
                    instagram: '@salon',
                    facebook: 'Salon',
                  ),
                  businessHours: [
                    SalonHours(
                      day: 'Segunda - Sexta',
                      isOpen: true,
                      openTime: '9:00',
                      closeTime: '18:00',
                    ),
                    SalonHours(
                      day: 'Sábado',
                      isOpen: true,
                      openTime: '9:00',
                      closeTime: '14:00',
                    ),
                    SalonHours(
                      day: 'Domingo',
                      isOpen: false,
                    ),
                  ],
                  reviews: [
                    SalonReview(
                      id: '1',
                      userName: 'Maria Silva',
                      rating: 5,
                      comment: 'Excelente atendimento!',
                      date: '2024-03-15',
                    ),
                    SalonReview(
                      id: '2',
                      userName: 'João Santos',
                      rating: 4,
                      comment: 'Muito bom serviço',
                      date: '2024-03-14',
                    ),
                  ],
                  additionalInfo: {
                    'Estacionamento': 'Gratuito',
                    'Formas de Pagamento': 'Dinheiro, Cartão, PIX',
                    'Acessibilidade': 'Rampa de acesso',
                  },
                ),
              ),
            );
          },
          borderRadius: BorderRadius.circular(16),
      child: Padding(
            padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                    Container(
                      width: 50,
                      height: 50,
                      decoration: BoxDecoration(
                        color: salon.colors.primary,
                        borderRadius: BorderRadius.circular(25),
                      ),
                      child: Icon(
                        Icons.store,
                        color: salon.colors.primary,
                        size: 24,
                      ),
                    ),
                    const SizedBox(width: 16),
                Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            salon.name,
                            style: theme.textTheme.titleLarge?.copyWith(
                              fontWeight: FontWeight.bold,
                  ),
            ),
                          const SizedBox(height: 4),
            Text(
                            salon.address,
                            style: theme.textTheme.bodyMedium?.copyWith(
                              color: theme.colorScheme.onSurfaceVariant,
                            ),
            ),
                        ],
                      ),
                    ),
                    IconButton(
                      icon: Icon(
                        Icons.favorite,
                        color: theme.colorScheme.primary,
                      ),
                      onPressed: () {
                        setState(() {
                          favoritos.removeAt(index);
                        });
                      },
                    ),
                  ],
                ),
                const SizedBox(height: 16),
            Row(
              children: [
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                      decoration: BoxDecoration(
                        color: salon.isOpen 
                            ? Colors.green.withOpacity(0.1)
                            : theme.colorScheme.error.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(
                            salon.isOpen ? Icons.circle : Icons.circle_outlined,
                            size: 12,
                            color: salon.isOpen ? Colors.green : theme.colorScheme.error,
                          ),
                          const SizedBox(width: 4),
                Text(
                            salon.isOpen ? 'Aberto' : 'Fechado',
                            style: theme.textTheme.labelMedium?.copyWith(
                              color: salon.isOpen ? Colors.green : theme.colorScheme.error,
                    fontWeight: FontWeight.bold,
                            ),
                          ),
                        ],
                  ),
                ),
                const SizedBox(width: 8),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                      decoration: BoxDecoration(
                        color: theme.colorScheme.primary.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(
                            Icons.access_time,
                            size: 16,
                            color: theme.colorScheme.primary,
                          ),
                          const SizedBox(width: 4),
                          Text(
                            '${salon.waitTime} min',
                            style: theme.textTheme.labelMedium?.copyWith(
                              color: theme.colorScheme.primary,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
              ],
                      ),
            ),
                    const SizedBox(width: 8),
                Container(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                  decoration: BoxDecoration(
                        color: theme.colorScheme.secondary.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(20),
                  ),
                  child: Row(
                        mainAxisSize: MainAxisSize.min,
                    children: [
                          Icon(
                            Icons.people_outline,
                            size: 16,
                            color: theme.colorScheme.secondary,
                          ),
                      const SizedBox(width: 4),
                          Text(
                            '${salon.queueLength} na fila',
                            style: theme.textTheme.labelMedium?.copyWith(
                              color: theme.colorScheme.secondary,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                    ],
                  ),
                    ),
                  ],
                ),
                const SizedBox(height: 16),
                Row(
                  children: [
                    Expanded(
                      child: ElevatedButton.icon(
                        onPressed: salon.isOpen ? () {
                          Navigator.of(context).push(
                            MaterialPageRoute(
                              builder: (_) => CheckInScreen(salon: salon),
                            ),
                          );
                        } : null,
                        icon: const Icon(Icons.check_circle),
                        label: const Text('Check In'),
                        style: ElevatedButton.styleFrom(
                          backgroundColor: theme.colorScheme.primary,
                          foregroundColor: theme.colorScheme.onPrimary,
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                          padding: const EdgeInsets.symmetric(vertical: 12),
                        ),
                      ),
                    ),
                    const SizedBox(width: 8),
                    IconButton(
                  onPressed: () {
                        // TODO: Implement share functionality
                  },
                      icon: const Icon(Icons.share),
                      style: IconButton.styleFrom(
                        backgroundColor: theme.colorScheme.primary.withOpacity(0.1),
                        padding: const EdgeInsets.all(12),
                  ),
                ),
              ],
            ),
          ],
            ),
          ),
        ),
      ),
    );
  }
} 