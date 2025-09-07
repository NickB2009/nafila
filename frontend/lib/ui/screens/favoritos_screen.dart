import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../widgets/bottom_nav_bar.dart';
import '../../models/salon.dart';
import '../../models/salon_service.dart';
import '../../models/salon_contact.dart';
import '../../models/salon_hours.dart';
import '../../models/salon_review.dart';
import '../../models/public_salon.dart';
import 'salon_details_screen.dart';
import 'check_in_screen.dart';
import '../../controllers/app_controller.dart';

// Generate salon colors for compatibility with existing screens
SalonColors _generateSalonColors(int index) {
  final colors = [
    SalonColors(
      primary: const Color(0xFFD4AF37), // Gold
      secondary: const Color(0xFF2C3E50), // Dark blue
      background: const Color(0xFFF5F5F5), // Light gray
      onSurface: const Color(0xFF2C3E50),
    ),
    SalonColors(
      primary: const Color(0xFF6B73FF), // Blue
      secondary: const Color(0xFF4CAF50), // Green
      background: const Color(0xFFF8F9FA), // Very light gray
      onSurface: const Color(0xFF2C3E50),
    ),
    SalonColors(
      primary: const Color(0xFFFF9800), // Orange
      secondary: const Color(0xFF1976D2), // Blue
      background: const Color(0xFFFFF8E1), // Light orange
      onSurface: const Color(0xFF2C3E50),
    ),
    SalonColors(
      primary: const Color(0xFF9C27B0), // Purple
      secondary: const Color(0xFFE91E63), // Pink
      background: const Color(0xFFFCE4EC), // Light pink
      onSurface: const Color(0xFF2C3E50),
    ),
    SalonColors(
      primary: const Color(0xFF00BCD4), // Cyan
      secondary: const Color(0xFF8BC34A), // Light green
      background: const Color(0xFFE0F2F1), // Light cyan
      onSurface: const Color(0xFF2C3E50),
    ),
  ];
  return colors[index % colors.length];
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

  List<Salon> _favoritos = [];
  bool _isLoading = false;
  String? _error;

  // Convert PublicSalon to Salon for compatibility with existing screens
  Salon _convertToSalon(PublicSalon publicSalon, int colorIndex) {
    return Salon(
      name: publicSalon.name,
      address: publicSalon.address,
      waitTime: publicSalon.currentWaitTimeMinutes ?? 15,
      distance: publicSalon.distanceKm ?? 1.0,
      isOpen: publicSalon.isOpen,
      closingTime: '19:00', // Default closing time
      isFavorite: true, // All salons in favorites are favorites
      queueLength: publicSalon.queueLength ?? 0,
      colors: _generateSalonColors(colorIndex),
    );
  }


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
    
    // Load favorites after the first frame is built
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadFavorites();
    });
  }

  // Load favorites from the database
  Future<void> _loadFavorites() async {
    if (!mounted) return;
    
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      // Wait a bit to ensure the widget tree is ready
      await Future.delayed(const Duration(milliseconds: 100));
      
      if (!mounted) return;
      
      final appController = Provider.of<AppController>(context, listen: false);
      
      // Load public salons from the database
      await appController.anonymous.loadPublicSalons();
      final publicSalons = appController.anonymous.nearbySalons;

      // For now, we'll show the first few salons as "favorites"
      // In a real app, this would be based on user's actual favorites
      final favoriteSalons = publicSalons.take(3).toList();
      
      final convertedFavorites = favoriteSalons.asMap().entries.map((entry) {
        final index = entry.key;
        final publicSalon = entry.value;
        return _convertToSalon(publicSalon, index);
      }).toList();

      if (!mounted) return;
      
      setState(() {
        _favoritos = convertedFavorites;
        _isLoading = false;
      });
    } catch (e) {
      if (!mounted) return;
      
      // Show error instead of mock data - let user know there's an API issue
      setState(() {
        _favoritos = [];
        _isLoading = false;
        _error = 'Erro ao carregar favoritos: $e';
      });
    }
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final brightness = Theme.of(context).brightness;
    final appPalette = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    return Scaffold(
      backgroundColor: appPalette?.primary ?? theme.colorScheme.primary,
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
                        color: appPalette?.secondary.withOpacity(0.15) ?? theme.colorScheme.onPrimary.withOpacity(0.2),
                        shape: BoxShape.circle,
                      ),
                      child: Icon(
                        Icons.arrow_back,
                        color: appPalette?.onSurface ?? theme.colorScheme.onPrimary,
                      ),
                    ),
                    onPressed: () => Navigator.of(context).pop(),
                  ),
                  const SizedBox(width: 16),
                  Text(
                    'Favoritos',
                    style: theme.textTheme.headlineMedium?.copyWith(
                      color: appPalette?.onSurface ?? theme.colorScheme.onPrimary,
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
                  color: appPalette?.background ?? theme.colorScheme.surface,
                  borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
                ),
                child: _buildContent(theme, brightness, appPalette),
              ),
            ),
          ],
        ),
      ),
      bottomNavigationBar: const BottomNavBar(currentIndex: 2),
    );
  }

  Widget _buildContent(ThemeData theme, Brightness brightness, SalonColors? appPalette) {
    if (_isLoading) {
      return _buildLoadingState(theme, brightness, appPalette);
    }
    
    if (_error != null) {
      return _buildErrorState(theme, brightness, appPalette);
    }
    
    if (_favoritos.isEmpty) {
      return _buildEmptyState(theme, brightness, appPalette);
    }
    
    return FadeTransition(
      opacity: _fadeAnimation,
      child: SlideTransition(
        position: _slideAnimation,
        child: ListView.separated(
          padding: const EdgeInsets.all(24),
          itemCount: _favoritos.length,
          separatorBuilder: (_, __) => const SizedBox(height: 18),
          itemBuilder: (context, index) => _buildSalonCard(theme, _favoritos[index], index, brightness),
        ),
      ),
    );
  }

  Widget _buildLoadingState(ThemeData theme, Brightness brightness, SalonColors? appPalette) {
    return const Center(
      child: CircularProgressIndicator(),
    );
  }

  Widget _buildErrorState(ThemeData theme, Brightness brightness, SalonColors? appPalette) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.error_outline,
              size: 64,
              color: theme.colorScheme.error,
            ),
            const SizedBox(height: 16),
            Text(
              'Error loading favorites',
              style: theme.textTheme.titleLarge?.copyWith(
                color: theme.colorScheme.onSurface,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              _error ?? 'Unknown error',
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 24),
            ElevatedButton(
              onPressed: _loadFavorites,
              child: const Text('Try Again'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildEmptyState(ThemeData theme, Brightness brightness, SalonColors? appPalette) {
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
                  color: appPalette?.secondary.withOpacity(0.5) ?? theme.colorScheme.primary.withOpacity(0.5),
                ),
                const SizedBox(height: 24),
                Text(
                  'Nenhum salão favorito ainda',
                  style: theme.textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: appPalette?.secondary ?? theme.colorScheme.onSurfaceVariant,
                  ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 10),
                Text(
                  'Toque na estrela em um salão para adicioná-lo aos seus favoritos.',
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: appPalette?.secondary ?? theme.colorScheme.onSurfaceVariant,
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

  Widget _buildSalonCard(ThemeData theme, Salon salon, int index, Brightness brightness) {
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
      child: Builder(
        builder: (context) {
          // Use individual salon colors for each favorite salon
          final colors = salon.colors.forBrightness(brightness);
          return Card(
            elevation: 2,
            color: colors.background,
            shadowColor: colors.primary.withOpacity(0.08),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(16),
              side: BorderSide(
                color: colors.secondary.withOpacity(0.18),
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
                            color: colors.primary,
                            borderRadius: BorderRadius.circular(25),
                          ),
                          child: Icon(
                            Icons.store,
                            color: colors.onSurface,
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
                                  color: colors.primary,
                                ),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                salon.address,
                                style: theme.textTheme.bodyMedium?.copyWith(
                                  color: colors.secondary,
                                ),
                              ),
                            ],
                          ),
                        ),
                        IconButton(
                          icon: Icon(
                            Icons.favorite,
                            color: colors.secondary,
                          ),
                          onPressed: () {
                            setState(() {
                              _favoritos.removeAt(index);
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
                                ? colors.secondary.withOpacity(0.12)
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
                                  color: salon.isOpen ? colors.secondary : theme.colorScheme.error,
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
                            color: colors.primary.withOpacity(0.1),
                            borderRadius: BorderRadius.circular(20),
                          ),
                          child: Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              Icon(
                                Icons.access_time,
                                size: 16,
                                color: colors.primary,
                              ),
                              const SizedBox(width: 4),
                              Text(
                                '${salon.waitTime} min',
                                style: theme.textTheme.labelMedium?.copyWith(
                                  color: colors.primary,
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
                            color: colors.secondary.withOpacity(0.1),
                            borderRadius: BorderRadius.circular(20),
                          ),
                          child: Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              Icon(
                                Icons.people_outline,
                                size: 16,
                                color: colors.secondary,
                              ),
                              const SizedBox(width: 4),
                              Text(
                                '${salon.queueLength} na fila',
                                style: theme.textTheme.labelMedium?.copyWith(
                                  color: colors.secondary,
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
                            onPressed: (salon.isOpen && !CheckInState.isCheckedIn) ? () {
                              Navigator.of(context).push(
                                MaterialPageRoute(
                                  builder: (_) => CheckInScreen(salon: salon),
                                ),
                              );
                            } : null,
                            icon: const Icon(Icons.check_circle),
                            label: const Text('Check In'),
                            style: ElevatedButton.styleFrom(
                              backgroundColor: colors.primary,
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
                            backgroundColor: colors.primary.withOpacity(0.1),
                            foregroundColor: colors.primary,
                            padding: const EdgeInsets.all(12),
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }
} 