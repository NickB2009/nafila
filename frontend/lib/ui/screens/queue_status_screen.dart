import 'package:flutter/material.dart';
import '../theme/app_theme.dart';
import 'notifications_screen.dart';
import 'dart:io' show Platform;
import 'package:url_launcher/url_launcher.dart' as launcher;
import 'package:flutter/foundation.dart' show kIsWeb;

class QueueStatusScreen extends StatelessWidget {
  const QueueStatusScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    // Mock data
    const String userName = "Rommel B";
    const String salonName = "Market at Mirada";
    const String salonAddress = "30921 Mirada Blvd, San Antonio, FL";
    const int waitTime = 34;
    const int position = 6;
    const String closingTime = "18:00";
    const String phone = "(352) 668-4089";
    const double distance = 10.9;
    const bool isOpen = true;

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
                // Stepper
                _buildStepper(theme),
                const SizedBox(height: 20),
                // Estimated Wait Card
                _buildWaitCard(theme, waitTime, position, context),
                const SizedBox(height: 20),
                // Checked-in Salon Card
                _buildSalonCard(theme, salonName, salonAddress, isOpen, closingTime, distance, phone, context),
              ],
            ),
          ),
        ),
      ),
      bottomNavigationBar: _buildBottomNavigation(context),
    );
  }

  Widget _buildStepper(ThemeData theme) {
    return Column(
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            _buildStepCircle(theme, true),
            Expanded(
              child: Divider(
                color: theme.colorScheme.onPrimary.withOpacity(0.5),
                thickness: 2,
              ),
            ),
            _buildStepCircle(theme, false),
            Expanded(
              child: Divider(
                color: theme.colorScheme.onPrimary.withOpacity(0.5),
                thickness: 2,
              ),
            ),
            _buildStepCircle(theme, false),
          ],
        ),
        const SizedBox(height: 8),
        const Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text('Na fila', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
            Text('Chegada', style: TextStyle(color: Colors.white)),
            Text('Corte', style: TextStyle(color: Colors.white)),
          ],
        ),
      ],
    );
  }

  Widget _buildStepCircle(ThemeData theme, bool active) {
    return Container(
      width: 20,
      height: 20,
      decoration: BoxDecoration(
        color: active ? theme.colorScheme.onPrimary : Colors.transparent,
        border: Border.all(
          color: theme.colorScheme.onPrimary,
          width: 2,
        ),
        shape: BoxShape.circle,
      ),
      child: active
          ? const Icon(Icons.check, size: 14, color: AppTheme.primaryColor)
          : null,
    );
  }

  Widget _buildWaitCard(ThemeData theme, int waitTime, int position, BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: theme.colorScheme.surface,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Text(
                'SEU TEMPO DE ESPERA',
                style: theme.textTheme.labelMedium?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                  fontWeight: FontWeight.bold,
                  letterSpacing: 1.2,
                ),
              ),
              const SizedBox(width: 8),
              const SizedBox(
                width: 16,
                height: 16,
                child: CircularProgressIndicator(
                  strokeWidth: 2,
                  valueColor: AlwaysStoppedAnimation(AppTheme.primaryColor),
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            '$waitTime min',
            style: theme.textTheme.displayMedium?.copyWith(
              color: AppTheme.primaryColor,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 16),
          Divider(color: theme.dividerColor),
          const SizedBox(height: 8),
          Text(
            'Você está na fila',
            style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 4),
          RichText(
            text: TextSpan(
              style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant),
              children: [
                const TextSpan(text: 'Você é o '),
                TextSpan(
                  text: '$positionº',
                  style: const TextStyle(fontWeight: FontWeight.bold),
                ),
                const TextSpan(text: ' da fila'),
              ],
            ),
          ),
          const SizedBox(height: 16),
          SizedBox(
            width: double.infinity,
            child: OutlinedButton(
              style: OutlinedButton.styleFrom(
                foregroundColor: AppTheme.primaryColor,
                side: const BorderSide(color: AppTheme.primaryColor, width: 2),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(24),
                ),
                textStyle: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold),
                padding: const EdgeInsets.symmetric(vertical: 12),
              ),
              onPressed: () {
                showModalBottomSheet(
                  context: context,
                  isScrollControlled: true,
                  backgroundColor: theme.colorScheme.surface,
                  shape: const RoundedRectangleBorder(
                    borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
                  ),
                  builder: (modalContext) => const WaitlistSheet(),
                );
              },
              child: const Text('Ver lista de espera'),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSalonCard(ThemeData theme, String name, String address, bool isOpen, String closingTime, double distance, String phone, BuildContext context) {
    // Mock coordinates for the salon
    const double lat = -22.9068;
    const double lng = -43.1729;
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: theme.colorScheme.surface,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
            decoration: BoxDecoration(
              color: AppTheme.primaryColor.withOpacity(0.1),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Text(
              'CHECK-IN REALIZADO',
              style: theme.textTheme.labelSmall?.copyWith(
                color: AppTheme.primaryColor,
                fontWeight: FontWeight.bold,
                letterSpacing: 1.1,
              ),
            ),
          ),
          const SizedBox(height: 8),
          Text(
            name,
            style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 4),
          Text(
            address,
            style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant),
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              Text(
                isOpen ? 'Aberto' : 'Fechado',
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: isOpen ? Colors.green : theme.colorScheme.error,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(width: 8),
              Text('• Fecha às $closingTime', style: theme.textTheme.bodyMedium),
              const SizedBox(width: 8),
              Icon(Icons.directions_car, size: 16, color: theme.colorScheme.onSurfaceVariant),
              const SizedBox(width: 2),
              Text('${distance.toStringAsFixed(1)} km', style: theme.textTheme.bodyMedium),
            ],
          ),
          const SizedBox(height: 16),
          _buildSalonAction(theme, Icons.navigation, 'Como chegar', onTap: () async {
            final googleMapsUrl = 'https://www.google.com/maps/dir/?api=1&destination=$lat,$lng';
            final appleMapsUrl = 'http://maps.apple.com/?daddr=$lat,$lng';
            if (kIsWeb) {
              await launcher.launchUrl(
                Uri.parse(googleMapsUrl),
                mode: launcher.LaunchMode.externalApplication,
              );
            } else if (Platform.isIOS || Platform.isMacOS) {
              showModalBottomSheet(
                context: context,
                shape: const RoundedRectangleBorder(
                  borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
                ),
                builder: (modalContext) => Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    ListTile(
                      leading: const Icon(Icons.map),
                      title: const Text('Apple Maps'),
                      onTap: () async {
                        await launcher.launchUrl(
                          Uri.parse(appleMapsUrl),
                          mode: launcher.LaunchMode.externalApplication,
                        );
                        Navigator.of(modalContext).pop();
                      },
                    ),
                    ListTile(
                      leading: const Icon(Icons.map),
                      title: const Text('Google Maps'),
                      onTap: () async {
                        await launcher.launchUrl(
                          Uri.parse(googleMapsUrl),
                          mode: launcher.LaunchMode.externalApplication,
                        );
                        Navigator.of(modalContext).pop();
                      },
                    ),
                  ],
                ),
              );
            } else {
              await launcher.launchUrl(
                Uri.parse(googleMapsUrl),
                mode: launcher.LaunchMode.externalApplication,
              );
            }
          }),
          Divider(color: theme.dividerColor),
          _buildSalonAction(
            theme,
            Icons.card_giftcard,
            'Lembrar próximo corte',
            trailing: _buildBetaChip(theme),
            onTap: () {
              showModalBottomSheet(
                context: context,
                isScrollControlled: true,
                backgroundColor: theme.colorScheme.surface,
                shape: const RoundedRectangleBorder(
                  borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
                ),
                builder: (modalContext) => HaircutReminderSheet(parentContext: context),
              );
            },
          ),
          Divider(color: theme.dividerColor),
          _buildSalonAction(theme, Icons.phone, phone, onTap: () {}),
          Divider(color: theme.dividerColor),
          _buildSalonAction(theme, Icons.delete_outline, 'Cancelar check-in', color: theme.colorScheme.error, onTap: () {}),
        ],
      ),
    );
  }

  Widget _buildSalonAction(ThemeData theme, IconData icon, String label, {Widget? trailing, Color? color, VoidCallback? onTap}) {
    return ListTile(
      contentPadding: EdgeInsets.zero,
      leading: Icon(icon, color: color ?? AppTheme.primaryColor),
      title: Text(
        label,
        style: theme.textTheme.bodyLarge?.copyWith(
          color: color ?? theme.colorScheme.onSurface,
          fontWeight: FontWeight.w500,
        ),
      ),
      trailing: trailing,
      onTap: onTap,
    );
  }

  Widget _buildBetaChip(ThemeData theme) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
      decoration: BoxDecoration(
        color: theme.colorScheme.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(8),
      ),
      child: Text(
        'BETA',
        style: theme.textTheme.labelSmall?.copyWith(
          color: theme.colorScheme.onSurfaceVariant,
          fontWeight: FontWeight.bold,
        ),
      ),
    );
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
          IconButton(
            icon: const Icon(Icons.home, color: AppTheme.primaryColor),
            onPressed: () {},
          ),
          IconButton(
            icon: Icon(Icons.search, color: theme.colorScheme.onSurfaceVariant),
            onPressed: () {},
          ),
          IconButton(
            icon: Icon(Icons.person_outline, color: theme.colorScheme.onSurfaceVariant),
            onPressed: () {},
          ),
        ],
      ),
    );
  }
}

class HaircutReminderSheet extends StatefulWidget {
  final BuildContext parentContext;
  const HaircutReminderSheet({super.key, required this.parentContext});

  @override
  State<HaircutReminderSheet> createState() => _HaircutReminderSheetState();
}

class _HaircutReminderSheetState extends State<HaircutReminderSheet> {
  int selectedWeek = 3;
  final List<int> weeks = List.generate(8, (i) => i + 1);

  void _confirmReminder() {
    Navigator.of(context).pop();
    // Show SnackBar after closing the modal
    Future.delayed(const Duration(milliseconds: 300), () {
      ScaffoldMessenger.of(widget.parentContext).showSnackBar(
        const SnackBar(
          content: Text('Lembrete agendado com sucesso!'),
          backgroundColor: AppTheme.primaryColor,
          behavior: SnackBarBehavior.floating,
          duration: Duration(seconds: 2),
        ),
      );
    });
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Padding(
      padding: MediaQuery.of(context).viewInsets,
      child: Container(
        padding: const EdgeInsets.fromLTRB(24, 24, 24, 24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Agendar meu próximo corte para...',
                  style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
                ),
                IconButton(
                  icon: const Icon(Icons.close),
                  onPressed: () => Navigator.of(context).pop(),
                ),
              ],
            ),
            const SizedBox(height: 8),
            Text(
              'Avise quando quiser cortar o cabelo de novo e enviaremos um lembrete.',
              style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant),
            ),
            const SizedBox(height: 24),
            SizedBox(
              height: 120,
              child: ListWheelScrollView.useDelegate(
                itemExtent: 40,
                diameterRatio: 1.2,
                physics: const FixedExtentScrollPhysics(),
                onSelectedItemChanged: (index) {
                  setState(() {
                    selectedWeek = weeks[index];
                  });
                },
                childDelegate: ListWheelChildBuilderDelegate(
                  builder: (context, index) {
                    if (index < 0 || index >= weeks.length) return null;
                    final week = weeks[index];
                    final isSelected = week == selectedWeek;
                    return Center(
                      child: Text(
                        week == 1 ? '1 semana' : '$week semanas',
                        style: theme.textTheme.titleMedium?.copyWith(
                          color: isSelected ? theme.colorScheme.onSurface : theme.colorScheme.onSurfaceVariant,
                          fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
                          fontSize: isSelected ? 22 : 18,
                        ),
                      ),
                    );
                  },
                  childCount: weeks.length,
                ),
              ),
            ),
            const SizedBox(height: 24),
            SizedBox(
              width: double.infinity,
              height: 48,
              child: ElevatedButton(
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppTheme.primaryColor,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(24),
                  ),
                ),
                onPressed: _confirmReminder,
                child: Text(
                  'Agendar lembrete único',
                  style: theme.textTheme.titleMedium?.copyWith(
                    color: theme.colorScheme.onPrimary,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class WaitlistSheet extends StatelessWidget {
  const WaitlistSheet({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    // Mock waitlist data
    final List<Map<String, dynamic>> waitlist = [
      {"name": "DM", "inSalon": false},
      {"name": "JC", "inSalon": false},
      {"name": "MP", "inSalon": true},
      {"name": "AP", "inSalon": true},
      {"name": "CM", "inSalon": false},
      {"name": "Rommel B", "inSalon": false, "isUser": true},
    ];
    final int userPosition = waitlist.indexWhere((g) => g["isUser"] == true) + 1;

    return Padding(
      padding: MediaQuery.of(context).viewInsets,
      child: Container(
        padding: const EdgeInsets.fromLTRB(24, 24, 24, 24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Você é o $userPositionº da fila',
                  style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
                ),
                IconButton(
                  icon: const Icon(Icons.close),
                  onPressed: () => Navigator.of(context).pop(),
                ),
              ],
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                Expanded(
                  flex: 1,
                  child: Text('Nº.', style: theme.textTheme.labelMedium?.copyWith(fontWeight: FontWeight.bold)),
                ),
                Expanded(
                  flex: 3,
                  child: Text('CONVIDADO', style: theme.textTheme.labelMedium?.copyWith(fontWeight: FontWeight.bold)),
                ),
                Expanded(
                  flex: 2,
                  child: Text('NO SALÃO', style: theme.textTheme.labelMedium?.copyWith(fontWeight: FontWeight.bold), textAlign: TextAlign.end),
                ),
              ],
            ),
            const SizedBox(height: 8),
            ...waitlist.asMap().entries.map((entry) {
              final idx = entry.key;
              final guest = entry.value;
              final bool isUser = guest["isUser"] == true;
              return Container(
                margin: const EdgeInsets.symmetric(vertical: 2),
                decoration: isUser
                    ? BoxDecoration(
                        color: AppTheme.primaryColor.withOpacity(0.07),
                        borderRadius: BorderRadius.circular(8),
                      )
                    : null,
                child: Row(
                  children: [
                    Expanded(
                      flex: 1,
                      child: Text(
                        "${idx + 1}.",
                        style: theme.textTheme.bodyLarge?.copyWith(
                          fontWeight: isUser ? FontWeight.bold : FontWeight.normal,
                        ),
                      ),
                    ),
                    Expanded(
                      flex: 3,
                      child: Text(
                        guest["name"],
                        style: theme.textTheme.bodyLarge?.copyWith(
                          fontWeight: isUser ? FontWeight.bold : FontWeight.normal,
                        ),
                      ),
                    ),
                    Expanded(
                      flex: 2,
                      child: guest["inSalon"] == true
                          ? const Align(
                              alignment: Alignment.centerRight,
                              child: Icon(Icons.check, color: AppTheme.primaryColor, size: 22),
                            )
                          : const SizedBox.shrink(),
                    ),
                  ],
                ),
              );
            }),
          ],
        ),
      ),
    );
  }
} 