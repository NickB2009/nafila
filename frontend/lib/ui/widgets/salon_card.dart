import 'package:flutter/material.dart';
import '../../models/salon.dart';

/// Card widget displaying salon information with wait time and check-in option
class SalonCard extends StatelessWidget {
  final Salon salon;

  const SalonCard({
    super.key,
    required this.salon,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Wait time header
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
            decoration: BoxDecoration(
              color: Colors.grey[100],
              borderRadius: BorderRadius.circular(8),
            ),
            child: const Text(
              'ESTIMATED WAIT',
              style: TextStyle(
                fontSize: 12,
                fontWeight: FontWeight.w500,
                color: Colors.grey,
                letterSpacing: 0.5,
              ),
            ),
          ),

          const SizedBox(height: 12),

          // Wait time display
          Text(
            '${salon.waitTime} min',
            style: const TextStyle(
              fontSize: 48,
              fontWeight: FontWeight.bold,
              color: Color(0xFF1DB584),
              height: 1.0,
            ),
          ),

          const SizedBox(height: 16),

          // Salon name and favorite icon
          Row(
            children: [
              Expanded(
                child: Text(
                  salon.name,
                  style: const TextStyle(
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                    color: Colors.black87,
                  ),
                ),
              ),
              if (salon.isFavorite)
                const Icon(
                  Icons.star,
                  color: Color(0xFF1DB584),
                  size: 20,
                ),
            ],
          ),

          const SizedBox(height: 4),

          // Address
          Text(
            salon.address,
            style: const TextStyle(
              fontSize: 14,
              color: Colors.grey,
            ),
          ),

          const SizedBox(height: 8),

          // Status and distance
          Row(
            children: [
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                decoration: BoxDecoration(
                  color: salon.isOpen ? Colors.green : Colors.red,
                  borderRadius: BorderRadius.circular(4),
                ),
                child: Text(
                  salon.isOpen ? 'Open' : 'Closed',
                  style: const TextStyle(
                    fontSize: 12,
                    color: Colors.white,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ),
              const SizedBox(width: 8),
              Text(
                '• Closes ${salon.closingTime}',
                style: const TextStyle(
                  fontSize: 12,
                  color: Colors.grey,
                ),
              ),
              const SizedBox(width: 8),
              Text(
                '• 🚗 ${salon.distance} mi',
                style: const TextStyle(
                  fontSize: 12,
                  color: Colors.grey,
                ),
              ),
            ],
          ),

          const SizedBox(height: 16),

          // Check in button
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: salon.isOpen
                  ? () {
                      _showCheckInDialog(context);
                    }
                  : null,
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF1DB584),
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(vertical: 16),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
                elevation: 0,
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(
                    Icons.check_circle_outline,
                    size: 20,
                  ),
                  const SizedBox(width: 8),
                  const Text(
                    'Check In',
                    style: TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.w600,
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

  void _showCheckInDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: Text('Check in to ${salon.name}'),
          content: Text(
            'Are you ready to join the queue at ${salon.name}?\n\n'
            'Estimated wait time: ${salon.waitTime} minutes',
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('Cancel'),
            ),
            ElevatedButton(
              onPressed: () {
                Navigator.of(context).pop();
                _handleCheckIn(context);
              },
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF1DB584),
                foregroundColor: Colors.white,
              ),
              child: const Text('Check In'),
            ),
          ],
        );
      },
    );
  }

  void _handleCheckIn(BuildContext context) {
    // TODO: Implement actual check-in logic
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Successfully checked in to ${salon.name}!'),
        backgroundColor: const Color(0xFF1DB584),
        behavior: SnackBarBehavior.floating,
      ),
    );
  }
}
