import 'package:flutter/material.dart';
import '../widgets/salon_card.dart';
import '../widgets/bottom_nav_bar.dart';
import '../../models/salon.dart';
import '../../models/salon_service.dart';
import '../../models/salon_contact.dart';
import '../../models/salon_hours.dart';
import '../../models/salon_review.dart';
import 'notifications_screen.dart';
import 'salon_map_screen.dart';
import 'check_in_screen.dart';
import 'salon_details_screen.dart';

/// Salon finder screen for mobile web interface
class SalonFinderScreen extends StatefulWidget {
  const SalonFinderScreen({super.key});

  @override
  State<SalonFinderScreen> createState() => _SalonFinderScreenState();
}

class _SalonFinderScreenState extends State<SalonFinderScreen> with SingleTickerProviderStateMixin {
  final Set<String> _favoriteSalons = {};
  late AnimationController _animationController;
  late Animation<double> _fadeAnimation;
  late Animation<Offset> _slideAnimation;

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
    final size = MediaQuery.of(context).size;
    
    return Scaffold(
      backgroundColor: theme.colorScheme.primary,
      appBar: AppBar(
        backgroundColor: theme.colorScheme.primary,
        elevation: 0,
        leading: null,
        automaticallyImplyLeading: false,
        title: Row(
          children: [
            Container(
              width: 32,
              height: 32,
              decoration: BoxDecoration(
                color: theme.colorScheme.secondary,
                borderRadius: BorderRadius.circular(16),
                boxShadow: [
                  BoxShadow(
                    color: theme.colorScheme.secondary.withOpacity(0.3),
                    blurRadius: 8,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: Icon(
                Icons.person,
                color: theme.colorScheme.onSecondary,
                size: 20,
              ),
            ),
            const SizedBox(width: 8),
            Text(
              '1',
              style: theme.textTheme.titleLarge?.copyWith(
                color: theme.colorScheme.onPrimary,
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
        actions: [
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
            decoration: BoxDecoration(
              color: theme.colorScheme.error,
              borderRadius: BorderRadius.circular(12),
              boxShadow: [
                BoxShadow(
                  color: theme.colorScheme.error.withOpacity(0.3),
                  blurRadius: 8,
                  offset: const Offset(0, 2),
                ),
              ],
            ),
            child: Text(
              'Q2',
              style: theme.textTheme.labelLarge?.copyWith(
                color: theme.colorScheme.onError,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          const SizedBox(width: 16),
          IconButton(
            icon: Icon(Icons.tv, color: theme.colorScheme.onPrimary),
            onPressed: () => Navigator.of(context).pushNamed('/tv-dashboard'),
            tooltip: 'Ver TV Dashboard',
          ),
          IconButton(
            icon: Icon(Icons.notifications_outlined, color: theme.colorScheme.onPrimary),
            onPressed: () => Navigator.of(context).push(
              MaterialPageRoute(builder: (_) => const NotificationsScreen()),
            ),
            tooltip: 'Notificações',
          ),
          const SizedBox(width: 16),
        ],
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          child: Padding(
            padding: EdgeInsets.all(size.width > 600 ? 32.0 : 20.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Header text with animations
                FadeTransition(
                  opacity: _fadeAnimation,
                  child: SlideTransition(
                    position: _slideAnimation,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                Text(
                  "Olá, Rommel!",
                  style: theme.textTheme.titleLarge?.copyWith(
                            color: theme.colorScheme.onPrimary.withOpacity(0.9),
                  ),
                ),
                        const SizedBox(height: 4),
                Text(
                  "Faça cada dia\num ótimo dia para o cabelo.",
                  style: theme.textTheme.headlineLarge?.copyWith(
                            color: theme.colorScheme.onPrimary.withOpacity(0.9),
                    height: 1.2,
                            shadows: [
                              Shadow(
                                color: theme.colorScheme.onPrimary.withOpacity(0.1),
                                offset: const Offset(0, 1),
                                blurRadius: 2,
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 12),

                // Modern decorative element
                Align(
                  alignment: Alignment.centerRight,
                  child: SizedBox(
                    width: size.width > 600 ? 140 : 100,
                    height: size.width > 600 ? 140 : 100,
                    child: TweenAnimationBuilder<double>(
                      tween: Tween(begin: 0.0, end: 1.0),
                      duration: const Duration(milliseconds: 1200),
                      builder: (context, value, child) {
                        return Transform.scale(
                          scale: value,
                    child: CustomPaint(
                            painter: SalonDecorationPainter(
                              color: theme.colorScheme.onPrimary.withOpacity(0.08),
                            ),
                      ),
                        );
                      },
                    ),
                  ),
                ),

                const SizedBox(height: 12),

                // Find salon card with animation
                FadeTransition(
                  opacity: _fadeAnimation,
                  child: SlideTransition(
                    position: _slideAnimation,
                    child: _buildFindSalonCard(context, theme),
                  ),
                ),

                const SizedBox(height: 12),

                // Salon listings with staggered animations
                ..._buildSalonCards(context),
              ],
            ),
          ),
        ),
      ),
      bottomNavigationBar: const BottomNavBar(currentIndex: 0),
    );
  }

  Widget _buildFindSalonCard(BuildContext context, ThemeData theme) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: theme.colorScheme.surface,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: theme.shadowColor.withOpacity(0.1),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            theme.colorScheme.surface,
            theme.colorScheme.surface.withOpacity(0.95),
          ],
        ),
      ),
      child: Row(
        children: [
          Container(
            width: 50,
            height: 50,
            decoration: BoxDecoration(
              color: theme.colorScheme.primary.withOpacity(0.1),
              borderRadius: BorderRadius.circular(25),
              boxShadow: [
                BoxShadow(
                  color: theme.colorScheme.primary.withOpacity(0.2),
                  blurRadius: 8,
                  offset: const Offset(0, 2),
                ),
              ],
            ),
            child: Icon(
              Icons.location_on,
              color: theme.colorScheme.primary,
              size: 24,
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Encontre um salão próximo',
                  style: theme.textTheme.titleLarge,
                ),
                const SizedBox(height: 4),
                GestureDetector(
                  onTap: () => Navigator.of(context).push(
                    MaterialPageRoute(builder: (_) => const SalonMapScreen()),
                  ),
                  child: Row(
                    children: [
                      Text(
                        'Ver mapa',
                    style: theme.textTheme.labelLarge?.copyWith(
                      color: theme.colorScheme.primary,
                    ),
                      ),
                      const SizedBox(width: 4),
                      Icon(
                        Icons.arrow_forward,
                        size: 16,
                        color: theme.colorScheme.primary,
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  List<Widget> _buildSalonCards(BuildContext context) {
    final salons = [
      const Salon(
        name: 'Market at Mirada',
        address: '30921 Mirada Blvd, San Antonio, FL',
        waitTime: 24,
        distance: 10.9,
        isOpen: true,
        closingTime: '6 PM',
        isFavorite: true,
        queueLength: 5,
      ),
      const Salon(
        name: 'Cortez Commons',
        address: '123 Cortez Ave, San Antonio, FL',
        waitTime: 8,
        distance: 5.2,
        isOpen: true,
        closingTime: '8 PM',
        isFavorite: true,
        queueLength: 2,
      ),
    ];

    return List.generate(
      salons.length,
      (index) => Padding(
              padding: const EdgeInsets.only(bottom: 16),
        child: TweenAnimationBuilder<double>(
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
          child: Container(
            decoration: BoxDecoration(
              color: Theme.of(context).colorScheme.surface,
              borderRadius: BorderRadius.circular(16),
              boxShadow: [
                BoxShadow(
                  color: Theme.of(context).shadowColor.withOpacity(0.08),
                  blurRadius: 12,
                  offset: const Offset(0, 4),
                ),
              ],
            ),
            child: Material(
              color: Colors.transparent,
              child: InkWell(
                borderRadius: BorderRadius.circular(16),
                onTap: salons[index].isOpen ? () {
                  Navigator.of(context).push(
                    MaterialPageRoute(
                      builder: (_) => SalonDetailsScreen(
                        salon: salons[index],
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
                } : null,
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Row(
                                  children: [
                                    Expanded(
                                      child: Text(
                                        salons[index].name,
                                        style: Theme.of(context).textTheme.titleLarge?.copyWith(
                                          fontWeight: FontWeight.bold,
                                        ),
                                      ),
                                    ),
                                    IconButton(
                                      icon: Icon(
                                        _favoriteSalons.contains(salons[index].name)
                                            ? Icons.favorite
                                            : Icons.favorite_border,
                                        color: _favoriteSalons.contains(salons[index].name)
                                            ? Theme.of(context).colorScheme.primary
                                            : Theme.of(context).colorScheme.onSurface.withOpacity(0.5),
                                      ),
                                      onPressed: () {
                                        setState(() {
                                          if (_favoriteSalons.contains(salons[index].name)) {
                                            _favoriteSalons.remove(salons[index].name);
                                          } else {
                                            _favoriteSalons.add(salons[index].name);
                                          }
                                        });
                                      },
                                    ),
                                  ],
                                ),
                                const SizedBox(height: 4),
                                Text(
                                  salons[index].address,
                                  style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                                    color: Theme.of(context).colorScheme.onSurface.withOpacity(0.7),
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
                          _buildInfoChip(
                            context,
                            Icons.access_time,
                            '${salons[index].waitTime} min',
                            const Color(0xFF2C3E50).withOpacity(0.1),
                            const Color(0xFF2C3E50),
                          ),
                          const SizedBox(width: 8),
                          _buildInfoChip(
                            context,
                            Icons.people_outline,
                            '${salons[index].queueLength} na fila',
                            const Color(0xFF34495E).withOpacity(0.1),
                            const Color(0xFF34495E),
                          ),
                          const SizedBox(width: 8),
                          _buildInfoChip(
                            context,
                            Icons.location_on_outlined,
                            '${salons[index].distance} km',
                            const Color(0xFF2C3E50).withOpacity(0.1),
                            const Color(0xFF2C3E50),
                          ),
                        ],
                      ),
                      const SizedBox(height: 16),
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                            decoration: BoxDecoration(
                              color: salons[index].isOpen
                                  ? Theme.of(context).colorScheme.primary.withOpacity(0.1)
                                  : Theme.of(context).colorScheme.error.withOpacity(0.1),
                              borderRadius: BorderRadius.circular(8),
                            ),
                            child: Row(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                Icon(
                                  salons[index].isOpen ? Icons.circle : Icons.circle_outlined,
                                  size: 12,
                                  color: salons[index].isOpen
                                      ? Theme.of(context).colorScheme.primary
                                      : Theme.of(context).colorScheme.error,
                                ),
                                const SizedBox(width: 4),
                                Text(
                                  salons[index].isOpen ? 'Aberto' : 'Fechado',
                                  style: Theme.of(context).textTheme.labelMedium?.copyWith(
                                    color: salons[index].isOpen
                                        ? Theme.of(context).colorScheme.primary
                                        : Theme.of(context).colorScheme.error,
                                  ),
                                ),
                              ],
                            ),
                          ),
                          Row(
                            children: [
                              TextButton.icon(
                                onPressed: () {
                                  Navigator.of(context).push(
                                    MaterialPageRoute(
                                      builder: (_) => SalonDetailsScreen(
                                        salon: salons[index],
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
                                icon: const Icon(Icons.info_outline, size: 18),
                                label: const Text('Mais'),
                                style: TextButton.styleFrom(
                                  foregroundColor: const Color(0xFF2C3E50),
                                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                                ),
                              ),
                              if (salons[index].isOpen) ...[
                                const SizedBox(width: 8),
                                TextButton.icon(
                                  onPressed: () {
                                    Navigator.of(context).push(
                                      MaterialPageRoute(
                                        builder: (_) => CheckInScreen(salon: salons[index]),
                                      ),
                                    );
                                  },
                                  icon: const Icon(Icons.login, size: 18),
                                  label: const Text('Check-in'),
                                  style: TextButton.styleFrom(
                                    foregroundColor: Theme.of(context).colorScheme.primary,
                                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                                  ),
                                ),
                              ],
                            ],
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildInfoChip(BuildContext context, IconData icon, String label, Color bgColor, Color iconColor) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: bgColor,
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 16, color: iconColor),
          const SizedBox(width: 4),
          Text(
            label,
            style: Theme.of(context).textTheme.labelMedium?.copyWith(
              color: iconColor,
            ),
          ),
        ],
      ),
    );
  }
}

/// Custom painter for the salon decoration
class SalonDecorationPainter extends CustomPainter {
  final Color color;

  SalonDecorationPainter({required this.color});

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = color
      ..style = PaintingStyle.stroke
      ..strokeWidth = 2;

    // Draw a modern, abstract pattern representing hair styling
    final path = Path();
    
    // Main curve
    path.moveTo(size.width * 0.2, size.height * 0.5);
    path.quadraticBezierTo(
      size.width * 0.5,
      size.height * 0.2,
      size.width * 0.8,
      size.height * 0.5,
    );

    // Decorative elements
    for (var i = 0; i < 3; i++) {
      final offset = i * (size.width * 0.2);
      path.moveTo(size.width * 0.3 + offset, size.height * 0.6);
      path.quadraticBezierTo(
        size.width * 0.4 + offset,
        size.height * 0.4,
        size.width * 0.5 + offset,
        size.height * 0.6,
      );
    }

    // Draw the path
    canvas.drawPath(path, paint);

    // Add some dots for visual interest
    final dotPaint = Paint()
      ..color = color
      ..style = PaintingStyle.fill;

    for (var i = 0; i < 5; i++) {
      final x = size.width * (0.2 + (i * 0.15));
      final y = size.height * (0.3 + (i % 2) * 0.2);
      canvas.drawCircle(Offset(x, y), 2, dotPaint);
    }
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}
