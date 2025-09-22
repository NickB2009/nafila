import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../screens/salon_finder_screen.dart';
import '../screens/salon_map_screen.dart';
import '../screens/account_screen.dart';
import '../../models/salon.dart';
import '../../controllers/app_controller.dart';

/// Global check-in state (replace with Provider or real state management)
class CheckInState {
  static bool isCheckedIn = false;
  static Salon? checkedInSalon;
}

class BottomNavBar extends StatelessWidget {
  final int currentIndex;

  const BottomNavBar({
    super.key,
    required this.currentIndex,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final appController = Provider.of<AppController>(context);
    final isAnonymous = appController.isAnonymousMode;
    final brightness = theme.brightness;
    final salonColors = CheckInState.isCheckedIn && CheckInState.checkedInSalon != null
        ? CheckInState.checkedInSalon!.colors.forBrightness(brightness)
        : null;
    final bottomPadding = MediaQuery.of(context).padding.bottom;
    
    return Material(
      color: Colors.transparent,
      child: Container(
        height: 80 + bottomPadding,
        decoration: BoxDecoration(
          color: salonColors?.background ?? theme.colorScheme.surface,
          borderRadius: const BorderRadius.only(
            topLeft: Radius.circular(20),
            topRight: Radius.circular(20),
          ),
          boxShadow: [
            BoxShadow(
              color: (salonColors?.primary ?? theme.shadowColor).withOpacity(0.1),
              blurRadius: 10,
              offset: const Offset(0, -4),
            ),
          ],
          border: Border(
            top: BorderSide(
              color: (salonColors?.secondary ?? theme.dividerColor).withOpacity(0.15),
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
              () {
                if (CheckInState.isCheckedIn) {
                  _showCheckInNavigationDialog(context);
                  return;
                }
                Navigator.of(context).pushReplacement(
                  MaterialPageRoute(builder: (_) => const SalonFinderScreen()),
                );
              },
              salonColors: salonColors,
            ),
            _buildNavItem(
              context,
              Icons.search,
              currentIndex == 1,
              'Buscar',
              () {
                if (CheckInState.isCheckedIn) {
                  _showCheckInNavigationDialog(context);
                  return;
                }
                Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => const SalonMapScreen()),
                );
              },
              salonColors: salonColors,
            ),
            if (!isAnonymous)
              _buildNavItem(
                context,
                Icons.person_outline,
                currentIndex == 2,
                'Conta',
                () {
                  if (CheckInState.isCheckedIn) {
                    _showCheckInNavigationDialog(context);
                    return;
                  }
                  Navigator.of(context).push(
                    MaterialPageRoute(builder: (_) => const AccountScreen()),
                  );
                },
                salonColors: salonColors,
              ),
          ],
        ),
      ),
    );
  }

  void _showCheckInNavigationDialog(BuildContext context) {
    final theme = Theme.of(context);
    final brightness = Theme.of(context).brightness;
    final salonColors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    
    showDialog(
      context: context,
      builder: (dialogContext) => AlertDialog(
        backgroundColor: salonColors?.background ?? theme.colorScheme.surface,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: Row(
          children: [
            Icon(
              Icons.info_outline,
              color: salonColors?.primary ?? theme.colorScheme.primary,
            ),
            const SizedBox(width: 8),
            Expanded(
              child: Text(
                'Você está na fila',
                style: theme.textTheme.titleLarge?.copyWith(
                  color: salonColors?.onSurface ?? theme.colorScheme.onSurface,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ],
        ),
        content: Text(
          'Você está atualmente na fila de um salão. Para navegar para outras telas, primeiro cancele seu check-in.',
          style: theme.textTheme.bodyMedium?.copyWith(
            color: salonColors?.secondary ?? theme.colorScheme.onSurfaceVariant,
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(),
            style: TextButton.styleFrom(
              foregroundColor: salonColors?.primary ?? theme.colorScheme.primary,
            ),
            child: const Text('Entendi'),
          ),
        ],
      ),
    );
  }

  Widget _buildNavItem(
    BuildContext context,
    IconData icon,
    bool isSelected,
    String label,
    VoidCallback onTap,
    {SalonColors? salonColors}
  ) {
    final theme = Theme.of(context);
    final selectedColor = salonColors?.primary ?? theme.colorScheme.primary;
    final unselectedColor = salonColors?.onSurface.withOpacity(0.7) ?? theme.colorScheme.onSurface.withOpacity(0.6);
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
                  ? selectedColor
                  : unselectedColor,
                size: 28,
              ),
              const SizedBox(height: 6),
              Text(
                label,
                style: theme.textTheme.labelSmall?.copyWith(
                  color: isSelected 
                    ? selectedColor
                    : unselectedColor,
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