import 'package:flutter/material.dart';
import '../screens/salon_finder_screen.dart';
import '../screens/salon_map_screen.dart';
import '../screens/account_screen.dart';

class BottomNavBar extends StatelessWidget {
  final int currentIndex;

  const BottomNavBar({
    super.key,
    required this.currentIndex,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final bottomPadding = MediaQuery.of(context).padding.bottom;
    
    return Material(
      color: Colors.transparent,
      child: Container(
        height: 80 + bottomPadding,
        decoration: BoxDecoration(
          color: theme.colorScheme.surface,
          borderRadius: const BorderRadius.only(
            topLeft: Radius.circular(20),
            topRight: Radius.circular(20),
          ),
          boxShadow: [
            BoxShadow(
              color: theme.shadowColor.withOpacity(0.1),
              blurRadius: 10,
              offset: const Offset(0, -4),
            ),
          ],
          border: Border(
            top: BorderSide(
              color: theme.dividerColor.withOpacity(0.15),
              width: 1.2,
            ),
          ),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
          children: [
            _buildNavItem(
              context,
              Icons.home,
              currentIndex == 0,
              'Início',
              () => Navigator.of(context).pushReplacement(
                MaterialPageRoute(builder: (_) => const SalonFinderScreen()),
              ),
            ),
            _buildNavItem(
              context,
              Icons.search,
              currentIndex == 1,
              'Buscar',
              () => Navigator.of(context).push(
                MaterialPageRoute(builder: (_) => const SalonMapScreen()),
              ),
            ),
            _buildNavItem(
              context,
              Icons.person_outline,
              currentIndex == 2,
              'Conta',
              () => Navigator.of(context).push(
                MaterialPageRoute(builder: (_) => const AccountScreen()),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildNavItem(
    BuildContext context,
    IconData icon,
    bool isSelected,
    String label,
    VoidCallback onTap,
  ) {
    final theme = Theme.of(context);
    return Expanded(
      child: InkWell(
        borderRadius: const BorderRadius.only(
          topLeft: Radius.circular(20),
          topRight: Radius.circular(20),
        ),
        onTap: onTap,
        child: Container(
          height: double.infinity,
          alignment: Alignment.center,
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                icon,
                color: isSelected 
                  ? theme.colorScheme.primary 
                  : theme.colorScheme.onSurface.withOpacity(0.6),
                size: 28,
              ),
              const SizedBox(height: 6),
              Text(
                label,
                style: theme.textTheme.labelSmall?.copyWith(
                  color: isSelected 
                    ? theme.colorScheme.primary 
                    : theme.colorScheme.onSurface.withOpacity(0.6),
                  fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
                  fontSize: 13,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
} 