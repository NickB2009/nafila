import 'package:flutter/material.dart';
import '../widgets/salon_card.dart';
import '../../models/salon.dart';

/// Salon finder screen for mobile web interface
class SalonFinderScreen extends StatelessWidget {
  const SalonFinderScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFF1DB584), // Green background from mockup
      appBar: AppBar(
        backgroundColor: const Color(0xFF1DB584),
        elevation: 0,
        leading: null,
        automaticallyImplyLeading: false,
        title: Row(
          children: [
            Container(
              width: 32,
              height: 32,
              decoration: BoxDecoration(
                color: Colors.orange,
                borderRadius: BorderRadius.circular(16),
              ),
              child: const Icon(
                Icons.person,
                color: Colors.white,
                size: 20,
              ),
            ),
            const SizedBox(width: 8),
            const Text(
              '1',
              style: TextStyle(
                color: Colors.white,
                fontSize: 18,
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
        actions: [
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
            decoration: BoxDecoration(
              color: Colors.red,
              borderRadius: BorderRadius.circular(12),
            ),
            child: const Text(
              'Q2',
              style: TextStyle(
                color: Colors.white,
                fontSize: 14,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          const SizedBox(width: 16),
          const Icon(
            Icons.notifications_outlined,
            color: Colors.white,
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
                const Text(
                  "Lookin' good, Rommel!",
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 18,
                    fontWeight: FontWeight.w400,
                  ),
                ),
                const SizedBox(height: 8),
                const Text(
                  "Make every day\na great hair day.",
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 32,
                    fontWeight: FontWeight.bold,
                    height: 1.2,
                  ),
                ),
                const SizedBox(height: 20),

                // Chair icon
                Align(
                  alignment: Alignment.centerRight,
                  child: Container(
                    width: 120,
                    height: 120,
                    child: CustomPaint(
                      painter: ChairPainter(),
                    ),
                  ),
                ),

                const SizedBox(height: 20),

                // Find salon card
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(20),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(16),
                  ),
                  child: Row(
                    children: [
                      Container(
                        width: 50,
                        height: 50,
                        decoration: BoxDecoration(
                          color: const Color(0xFF1DB584).withOpacity(0.1),
                          borderRadius: BorderRadius.circular(25),
                        ),
                        child: const Icon(
                          Icons.location_on,
                          color: Color(0xFF1DB584),
                          size: 24,
                        ),
                      ),
                      const SizedBox(width: 16),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const Text(
                              'Find a salon near you',
                              style: TextStyle(
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                                color: Colors.black87,
                              ),
                            ),
                            const SizedBox(height: 4),
                            GestureDetector(
                              onTap: () {
                                // TODO: Navigate to map
                              },
                              child: const Text(
                                'View map →',
                                style: TextStyle(
                                  fontSize: 14,
                                  color: Color(0xFF1DB584),
                                  fontWeight: FontWeight.w500,
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
      bottomNavigationBar: _buildBottomNavigation(),
    );
  }

  List<Widget> _buildSalonCards() {
    final salons = [
      Salon(
        name: 'Market at Mirada',
        address: '30921 Mirada Blvd, San Antonio, FL',
        waitTime: 24,
        distance: 10.9,
        isOpen: true,
        closingTime: '6 PM',
        isFavorite: true,
      ),
      Salon(
        name: 'Cortez Commons',
        address: '123 Cortez Ave, San Antonio, FL',
        waitTime: 8,
        distance: 5.2,
        isOpen: true,
        closingTime: '8 PM',
        isFavorite: true,
      ),
    ];

    return salons
        .map((salon) => Padding(
              padding: const EdgeInsets.only(bottom: 16),
              child: SalonCard(salon: salon),
            ))
        .toList();
  }

  Widget _buildBottomNavigation() {
    return Container(
      height: 80,
      decoration: const BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.only(
          topLeft: Radius.circular(20),
          topRight: Radius.circular(20),
        ),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        children: [
          _buildNavItem(Icons.home, true),
          _buildNavItem(Icons.search, false),
          _buildNavItem(Icons.person_outline, false),
        ],
      ),
    );
  }

  Widget _buildNavItem(IconData icon, bool isSelected) {
    return Container(
      padding: const EdgeInsets.all(12),
      child: Icon(
        icon,
        color: isSelected ? const Color(0xFF1DB584) : Colors.grey,
        size: 28,
      ),
    );
  }
}

/// Custom painter for the salon chair icon
class ChairPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = Colors.white.withOpacity(0.3)
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
