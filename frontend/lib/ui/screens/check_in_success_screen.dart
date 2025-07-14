import 'package:flutter/material.dart';
import 'queue_status_screen.dart';
import '../widgets/bottom_nav_bar.dart';
import '../../models/salon.dart';

class CheckInSuccessScreen extends StatefulWidget {
  final bool delayNavigation;
  final Salon salon;
  const CheckInSuccessScreen({super.key, this.delayNavigation = true, required this.salon});

  @override
  State<CheckInSuccessScreen> createState() => _CheckInSuccessScreenState();
}

class _CheckInSuccessScreenState extends State<CheckInSuccessScreen> {
  @override
  void initState() {
    super.initState();
    CheckInState.isCheckedIn = true;
    CheckInState.checkedInSalon = widget.salon;
    if (widget.delayNavigation) {
      Future.delayed(const Duration(seconds: 2), () {
        if (mounted) {
          Navigator.of(context).pushReplacement(
            MaterialPageRoute(builder: (_) => QueueStatusScreen(salon: widget.salon)),
          );
        }
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = CheckInState.checkedInSalon?.colors;
    return Scaffold(
      backgroundColor: colors?.primary ?? theme.colorScheme.primary,
      body: SafeArea(
        child: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Container(
                width: 72,
                height: 72,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  border: Border.all(
                    color: colors?.onSurface ?? theme.colorScheme.onPrimary,
                    width: 4,
                  ),
                ),
                child: Center(
                  child: Icon(
                    Icons.check,
                    size: 40,
                    color: colors?.onSurface ?? theme.colorScheme.onPrimary,
                  ),
                ),
              ),
              const SizedBox(height: 24),
              Text(
                'Check-in realizado com sucesso.',
                style: theme.textTheme.titleLarge?.copyWith(
                  color: colors?.onSurface ?? theme.colorScheme.onPrimary,
                  fontWeight: FontWeight.w500,
                ),
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
      ),
    );
  }
} 