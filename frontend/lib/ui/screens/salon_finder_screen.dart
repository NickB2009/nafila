import 'package:flutter/material.dart';
import '../widgets/salon_card.dart';
import '../widgets/bottom_nav_bar.dart';
import '../../models/salon.dart';
import '../theme/app_theme.dart';
import 'notifications_screen.dart';
import 'salon_map_screen.dart';
import 'account_screen.dart';
import 'check_in_screen.dart';

/// Salon finder screen for mobile web interface
class SalonFinderScreen extends StatelessWidget {
  const SalonFinderScreen({super.key});

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
                // Header text
                Text(
                  "Olá, Rommel!",
                  style: theme.textTheme.titleLarge?.copyWith(
                    color: theme.colorScheme.onPrimary,
                  ),
                ),
                const SizedBox(height: 8),
                Text(
                  "Faça cada dia\num ótimo dia para o cabelo.",
                  style: theme.textTheme.headlineLarge?.copyWith(
                    color: theme.colorScheme.onPrimary,
                    height: 1.2,
                  ),
                ),
                const SizedBox(height: 20),

                // Chair icon
                Align(
                  alignment: Alignment.centerRight,
                  child: SizedBox(
                    width: size.width > 600 ? 160 : 120,
                    height: size.width > 600 ? 160 : 120,
                    child: CustomPaint(
                      painter: ChairPainter(
                        color: theme.colorScheme.onPrimary.withOpacity(0.3),
                      ),
                    ),
                  ),
                ),

                const SizedBox(height: 20),

                // Find salon card
                _buildFindSalonCard(context, theme),

                const SizedBox(height: 20),

                // Salon listings
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
      ),
      child: Row(
        children: [
          Container(
            width: 50,
            height: 50,
            decoration: BoxDecoration(
              color: theme.colorScheme.primary.withOpacity(0.1),
              borderRadius: BorderRadius.circular(25),
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
                  child: Text(
                    'Ver mapa →',
                    style: theme.textTheme.labelLarge?.copyWith(
                      color: theme.colorScheme.primary,
                    ),
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

    return salons
        .map((salon) => Padding(
              padding: const EdgeInsets.only(bottom: 16),
              child: SalonCard(
                salon: salon,
                onCheckIn: salon.isOpen ? () {
                  Navigator.of(context).push(
                    MaterialPageRoute(
                      builder: (_) => CheckInScreen(salon: salon),
                    ),
                  );
                } : null,
              ),
            ))
        .toList();
  }
}

/// Custom painter for the salon chair icon
class ChairPainter extends CustomPainter {
  final Color color;

  ChairPainter({required this.color});

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = color
      ..style = PaintingStyle.stroke
      ..strokeWidth = 2;

    final path = Path()
      ..moveTo(size.width * 0.2, size.height * 0.8)
      ..lineTo(size.width * 0.8, size.height * 0.8)
      ..lineTo(size.width * 0.9, size.height * 0.6)
      ..lineTo(size.width * 0.7, size.height * 0.4)
      ..lineTo(size.width * 0.3, size.height * 0.4)
      ..lineTo(size.width * 0.1, size.height * 0.6)
      ..close();

    canvas.drawPath(path, paint);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}
