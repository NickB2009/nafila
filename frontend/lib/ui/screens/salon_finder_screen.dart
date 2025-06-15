import 'package:flutter/material.dart';
import '../widgets/salon_card.dart';
import '../../models/salon.dart';
import '../theme/app_theme.dart';
import 'notifications_screen.dart';
import 'salon_map_screen.dart';
import 'account_screen.dart';

/// Salon finder screen for mobile web interface
class SalonFinderScreen extends StatelessWidget {
  const SalonFinderScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    return Scaffold(
      backgroundColor: AppTheme.primaryColor,
      appBar: AppBar(
        backgroundColor: AppTheme.primaryColor,
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
            onPressed: () {
              Navigator.of(context).pushNamed('/tv-dashboard');
            },
          ),
          IconButton(
            icon: Icon(Icons.notifications_outlined, color: theme.colorScheme.onPrimary),
            onPressed: () {
              Navigator.of(context).push(
                MaterialPageRoute(builder: (_) => const NotificationsScreen()),
              );
            },
          ),
          const SizedBox(width: 16),
        ],
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          child: Padding(
            padding: const EdgeInsets.all(20.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Header text
                Text(
                  "Lookin' good, Rommel!",
                  style: theme.textTheme.titleLarge?.copyWith(
                    color: theme.colorScheme.onPrimary,
                  ),
                ),
                const SizedBox(height: 8),
                Text(
                  "Make every day\na great hair day.",
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
                    width: 120,
                    height: 120,
                    child: CustomPaint(
                      painter: ChairPainter(
                        color: theme.colorScheme.onPrimary.withOpacity(0.3),
                      ),
                    ),
                  ),
                ),

                const SizedBox(height: 20),

                // Find salon card
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(20),
                  decoration: BoxDecoration(
                    color: theme.colorScheme.surface,
                    borderRadius: BorderRadius.circular(16),
                  ),
                  child: Row(
                    children: [
                      Container(
                        width: 50,
                        height: 50,
                        decoration: BoxDecoration(
                          color: AppTheme.primaryColor.withOpacity(0.1),
                          borderRadius: BorderRadius.circular(25),
                        ),
                        child: const Icon(
                          Icons.location_on,
                          color: AppTheme.primaryColor,
                          size: 24,
                        ),
                      ),
                      const SizedBox(width: 16),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Find a salon near you',
                              style: theme.textTheme.titleLarge,
                            ),
                            const SizedBox(height: 4),
                            GestureDetector(
                              onTap: () {
                                Navigator.of(context).push(
                                  MaterialPageRoute(
                                    builder: (_) => const SalonMapScreen(),
                                  ),
                                );
                              },
                              child: Text(
                                'View map →',
                                style: theme.textTheme.labelLarge?.copyWith(
                                  color: AppTheme.primaryColor,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),

                const SizedBox(height: 20),

                // Salon listings
                ..._buildSalonCards(),
              ],
            ),
          ),
        ),
      ),
      bottomNavigationBar: _buildBottomNavigation(context),
    );
  }

  List<Widget> _buildSalonCards() {
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
              child: SalonCard(salon: salon),
            ))
        .toList();
  }

  Widget _buildBottomNavigation(BuildContext context) {
    final theme = Theme.of(context);
    
    return Container(
      height: 80,
      decoration: BoxDecoration(
        color: theme.colorScheme.surface,
        borderRadius: const BorderRadius.only(
          topLeft: Radius.circular(20),
          topRight: Radius.circular(20),
        ),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        children: [
          _buildNavItem(context, Icons.home, true),
          _buildNavItem(context, Icons.search, false),
          _buildNavItem(context, Icons.person_outline, false),
        ],
      ),
    );
  }

  Widget _buildNavItem(BuildContext context, IconData icon, bool isSelected) {
    final theme = Theme.of(context);
    
    return GestureDetector(
      onTap: () {
        if (icon == Icons.person_outline && !isSelected) {
          Navigator.of(context).push(
            MaterialPageRoute(builder: (_) => const AccountScreen()),
          );
        }
      },
      child: Container(
        padding: const EdgeInsets.all(12),
        child: Icon(
          icon,
          color: isSelected ? AppTheme.primaryColor : theme.colorScheme.onSurfaceVariant,
          size: 28,
        ),
      ),
    );
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
      ..strokeWidth = 2.0;

    // Chair outline - simplified representation
    final path = Path();

    // Seat
    path.addRRect(RRect.fromRectAndRadius(
      Rect.fromLTWH(size.width * 0.2, size.height * 0.4, size.width * 0.6,
          size.height * 0.3),
      const Radius.circular(8),
    ));

    // Backrest
    path.addRRect(RRect.fromRectAndRadius(
      Rect.fromLTWH(size.width * 0.25, size.height * 0.1, size.width * 0.5,
          size.height * 0.35),
      const Radius.circular(8),
    ));

    // Armrests
    path.addRRect(RRect.fromRectAndRadius(
      Rect.fromLTWH(size.width * 0.05, size.height * 0.35, size.width * 0.15,
          size.height * 0.25),
      const Radius.circular(4),
    ));

    path.addRRect(RRect.fromRectAndRadius(
      Rect.fromLTWH(size.width * 0.8, size.height * 0.35, size.width * 0.15,
          size.height * 0.25),
      const Radius.circular(4),
    ));

    // Base
    path.addRRect(RRect.fromRectAndRadius(
      Rect.fromLTWH(size.width * 0.35, size.height * 0.7, size.width * 0.3,
          size.height * 0.25),
      const Radius.circular(4),
    ));

    canvas.drawPath(path, paint);
  }

  @override
  bool shouldRepaint(CustomPainter oldDelegate) => false;
}
