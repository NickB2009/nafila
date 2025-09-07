import 'package:flutter/material.dart';
import '../../utils/brazilian_names_generator.dart';
import '../theme/app_theme.dart';
import 'dart:io' show Platform;
import 'package:url_launcher/url_launcher.dart' as launcher;
import 'package:flutter/foundation.dart' show kIsWeb;
import '../widgets/bottom_nav_bar.dart';
import 'package:flutter/services.dart';
import '../../models/salon.dart';

class QueueStatusScreen extends StatelessWidget {
  final Salon salon;
  const QueueStatusScreen({super.key, required this.salon});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    final isSmallScreen = MediaQuery.of(context).size.width < 600;
    
    return WillPopScope(
      onWillPop: () async {
        // Only prevent going back if user is checked in
        return !CheckInState.isCheckedIn;
      },
      child: Scaffold(
      backgroundColor: colors?.background ?? theme.colorScheme.surface,
      body: SafeArea(
        child: SingleChildScrollView(
          padding: EdgeInsets.all(isSmallScreen ? 16 : 20),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Header
              Row(
                children: [
                  Expanded(
                    child: Text(
                      'Status da Fila',
                      style: theme.textTheme.headlineSmall?.copyWith(
                        fontWeight: FontWeight.bold,
                        fontSize: isSmallScreen ? 18 : null,
                      ),
                    ),
                  ),
                ],
              ),
              SizedBox(height: isSmallScreen ? 20 : 24),
              
              // Progress Steps
              Container(
                padding: EdgeInsets.all(isSmallScreen ? 12 : 16),
                decoration: BoxDecoration(
                  color: (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.1),
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(
                    color: (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.3),
                    width: 1,
                  ),
                ),
                child: _buildProgressSteps(theme, colors, context),
              ),
              SizedBox(height: isSmallScreen ? 16 : 20),
              
              // Wait Time Card
              _buildWaitCard(theme, colors!, 15, 3, context),
              SizedBox(height: isSmallScreen ? 16 : 20),
              
              // Salon Info Card
              _buildSalonCard(theme, colors, 'Barbearia do João', 'Rua das Flores, 123 - Centro', true, '20:00', 0.5, '(11) 91234-5678', context),
              SizedBox(height: isSmallScreen ? 16 : 20),
              
              // Reminder Card
              _buildReminderCard(theme, colors, context),
            ],
          ),
        ),
      ),
      bottomNavigationBar: const BottomNavBar(currentIndex: 0),
    ),
    );
  }

  Widget _buildProgressSteps(ThemeData theme, SalonColors? colors, BuildContext context) {
    final isSmallScreen = MediaQuery.of(context).size.width < 600;
    return Column(
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            _buildStepCircle(theme, true, colors!),
            Expanded(
              child: Divider(
                color: theme.colorScheme.onPrimary.withOpacity(0.5),
                thickness: 2,
              ),
            ),
            _buildStepCircle(theme, false, colors),
            Expanded(
              child: Divider(
                color: theme.colorScheme.onPrimary.withOpacity(0.5),
                thickness: 2,
              ),
            ),
            _buildStepCircle(theme, false, colors),
          ],
        ),
        SizedBox(height: isSmallScreen ? 6 : 8),
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Flexible(
              child: Text(
                'Na fila', 
                style: TextStyle(
                  color: Colors.white, 
                  fontWeight: FontWeight.bold,
                  fontSize: isSmallScreen ? 12 : 14,
                ),
                textAlign: TextAlign.center,
              ),
            ),
            Flexible(
              child: Text(
                'Chegada', 
                style: TextStyle(
                  color: Colors.white,
                  fontSize: isSmallScreen ? 12 : 14,
                ),
                textAlign: TextAlign.center,
              ),
            ),
            Flexible(
              child: Text(
                'Corte', 
                style: TextStyle(
                  color: Colors.white,
                  fontSize: isSmallScreen ? 12 : 14,
                ),
                textAlign: TextAlign.center,
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildStepCircle(ThemeData theme, bool active, SalonColors colors) {
    return Container(
      width: 20,
      height: 20,
      decoration: BoxDecoration(
        color: active ? colors.background : Colors.transparent,
        border: Border.all(
          color: colors.background,
          width: 2,
        ),
        shape: BoxShape.circle,
      ),
      child: active
          ? Icon(Icons.check, size: 14, color: colors.primary)
          : null,
    );
  }

  Widget _buildWaitCard(ThemeData theme, SalonColors colors, int waitTime, int position, BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: colors.background,
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
              SizedBox(
                width: 16,
                height: 16,
                child: CircularProgressIndicator(
                  strokeWidth: 2,
                  valueColor: AlwaysStoppedAnimation(colors.primary),
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            '$waitTime min',
            style: theme.textTheme.displayMedium?.copyWith(
              color: colors.primary,
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
                foregroundColor: colors.primary,
                side: BorderSide(color: colors.primary, width: 2),
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

  Widget _buildSalonCard(ThemeData theme, SalonColors colors, String name, String address, bool isOpen, String closingTime, double distance, String phone, BuildContext context) {
    // Mock coordinates for the salon
    const double lat = -22.9068;
    const double lng = -43.1729;
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: colors.background,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
            decoration: BoxDecoration(
              color: colors.primary.withOpacity(0.1),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Text(
              'CHECK-IN REALIZADO',
              style: theme.textTheme.labelSmall?.copyWith(
                color: colors.primary,
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
              Flexible(
                child: Text(
                  '• Fecha às $closingTime', 
                  style: theme.textTheme.bodyMedium,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
              const SizedBox(width: 8),
              Flexible(
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(Icons.directions_car, size: 16, color: theme.colorScheme.onSurfaceVariant),
                    const SizedBox(width: 2),
                    Text('${distance.toStringAsFixed(1)} km', style: theme.textTheme.bodyMedium),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          _buildSalonAction(theme, Icons.navigation, 'Como chegar', onTap: () async {
            const googleMapsUrl = 'https://www.google.com/maps/dir/?api=1&destination=$lat,$lng';
            const appleMapsUrl = 'http://maps.apple.com/?daddr=$lat,$lng';
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
          _buildSalonAction(theme, Icons.phone, phone, onTap: () async {
            // Show options to call or copy
            showModalBottomSheet(
              context: context,
              shape: const RoundedRectangleBorder(
                borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
              ),
              builder: (modalContext) => Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  ListTile(
                    leading: const Icon(Icons.phone),
                    title: const Text('Ligar'),
                    onTap: () async {
                      final phoneUri = Uri(scheme: 'tel', path: phone);
                      if (await launcher.canLaunchUrl(phoneUri)) {
                        await launcher.launchUrl(phoneUri);
                      }
                      Navigator.of(modalContext).pop();
                    },
                  ),
                  ListTile(
                    leading: const Icon(Icons.copy),
                    title: const Text('Copiar número'),
                    onTap: () {
                      Clipboard.setData(ClipboardData(text: phone));
                      ScaffoldMessenger.of(context).showSnackBar(
                        SnackBar(
                          content: const Text('Número copiado!'),
                          backgroundColor: theme.colorScheme.primary,
                          behavior: SnackBarBehavior.floating,
                          duration: const Duration(seconds: 1),
                        ),
                      );
                      Navigator.of(modalContext).pop();
                    },
                  ),
                ],
              ),
            );
          }),
          Divider(color: theme.dividerColor),
          _buildSalonAction(theme, Icons.delete_outline, 'Cancelar check-in', color: theme.colorScheme.error, onTap: () async {
            final confirmed = await showDialog<bool>(
              context: context,
              builder: (dialogContext) => AlertDialog(
                title: const Text('Cancelar check-in'),
                content: const Text('Tem certeza que deseja cancelar seu check-in?'),
                actions: [
                  TextButton(
                    onPressed: () => Navigator.of(dialogContext).pop(false),
                    child: const Text('Voltar'),
                  ),
                  TextButton(
                    onPressed: () => Navigator.of(dialogContext).pop(true),
                    style: TextButton.styleFrom(foregroundColor: theme.colorScheme.error),
                    child: const Text('Cancelar'),
                  ),
                ],
              ),
            );
            if (confirmed == true) {
              CheckInState.isCheckedIn = false;
              CheckInState.checkedInSalon = null;
              // Navigate back to the root and then to the salon finder
              Navigator.of(context).pushNamedAndRemoveUntil('/', (route) => false);
            }
          }),
        ],
      ),
    );
  }

  Widget _buildSalonAction(ThemeData theme, IconData icon, String label, {Widget? trailing, Color? color, VoidCallback? onTap}) {
    final colors = salon.colors;
    return ListTile(
      contentPadding: EdgeInsets.zero,
      leading: Icon(icon, color: color ?? colors.primary),
      title: Text(
        label,
        style: theme.textTheme.bodyLarge?.copyWith(
          color: color ?? colors.primary,
          fontWeight: FontWeight.w500,
        ),
      ),
      trailing: trailing,
      onTap: onTap,
    );
  }

  Widget _buildBetaChip(ThemeData theme, SalonColors colors) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
      decoration: BoxDecoration(
        color: colors.secondary.withOpacity(0.2),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Text(
        'BETA',
        style: theme.textTheme.labelSmall?.copyWith(
          color: colors.primary,
          fontWeight: FontWeight.bold,
        ),
      ),
    );
  }

  Widget _buildReminderCard(ThemeData theme, SalonColors colors, BuildContext context) {
    final isSmallScreen = MediaQuery.of(context).size.width < 600;
    return Container(
      width: double.infinity,
      padding: EdgeInsets.all(isSmallScreen ? 16 : 20),
      decoration: BoxDecoration(
        color: colors.background,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'LEMBRETE',
            style: theme.textTheme.labelMedium?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
              fontWeight: FontWeight.bold,
              letterSpacing: 1.2,
            ),
          ),
          SizedBox(height: isSmallScreen ? 12 : 16),
          _buildActionButton(theme, Icons.card_giftcard, 'Lembrar próximo corte', colors, () {
            showModalBottomSheet(
              context: context,
              isScrollControlled: true,
              backgroundColor: theme.colorScheme.surface,
              shape: const RoundedRectangleBorder(
                borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
              ),
              builder: (modalContext) => HaircutReminderSheet(parentContext: context),
            );
          }),
        ],
      ),
    );
  }

  Widget _buildActionButton(ThemeData theme, IconData icon, String label, SalonColors colors, VoidCallback onTap, {bool isDestructive = false}) {
    return Builder(
      builder: (context) {
        final isSmallScreen = MediaQuery.of(context).size.width < 600;
        return InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(12),
          child: Padding(
            padding: EdgeInsets.all(isSmallScreen ? 12 : 16),
            child: Row(
              children: [
                Icon(
                  icon,
                  color: isDestructive ? theme.colorScheme.error : colors.primary,
                  size: isSmallScreen ? 20 : 24,
                ),
                SizedBox(width: isSmallScreen ? 12 : 16),
                Expanded(
                  child: Text(
                    label,
                    style: theme.textTheme.bodyLarge?.copyWith(
                      color: isDestructive ? theme.colorScheme.error : colors.primary,
                      fontWeight: FontWeight.w500,
                      fontSize: isSmallScreen ? 14 : null,
                    ),
                  ),
                ),
                Icon(
                  Icons.arrow_forward_ios,
                  color: isDestructive ? theme.colorScheme.error : colors.primary,
                  size: isSmallScreen ? 16 : 20,
                ),
              ],
            ),
          ),
        );
      },
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
      final theme = Theme.of(widget.parentContext);
      ScaffoldMessenger.of(widget.parentContext).showSnackBar(
        SnackBar(
          content: const Text('Lembrete agendado com sucesso!'),
          backgroundColor: theme.colorScheme.primary,
          behavior: SnackBarBehavior.floating,
          duration: const Duration(seconds: 2),
        ),
      );
    });
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    
    return Container(
      decoration: BoxDecoration(
        color: colors?.background ?? theme.colorScheme.surface,
        borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
      ),
      child: Padding(
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
                  Expanded(
                    child: Text(
                      'Agendar meu próximo corte para...',
                      style: theme.textTheme.titleLarge?.copyWith(
                        fontWeight: FontWeight.bold,
                        color: colors?.onSurface ?? theme.colorScheme.onSurface,
                      ),
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  IconButton(
                    icon: Icon(Icons.close, color: colors?.onSurface ?? theme.colorScheme.onSurface),
                    onPressed: () => Navigator.of(context).pop(),
                  ),
                ],
              ),
              const SizedBox(height: 8),
              Text(
                'Avise quando quiser cortar o cabelo de novo e enviaremos um lembrete.',
                style: theme.textTheme.bodyMedium?.copyWith(color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant),
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
                            color: isSelected ? (colors?.onSurface ?? theme.colorScheme.onSurface) : (colors?.secondary ?? theme.colorScheme.onSurfaceVariant),
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
                    backgroundColor: colors?.primary ?? theme.colorScheme.primary,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(24),
                    ),
                  ),
                  onPressed: _confirmReminder,
                  child: Text(
                    'Agendar lembrete único',
                    style: theme.textTheme.titleMedium?.copyWith(
                      color: colors?.background ?? theme.colorScheme.onPrimary,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ),
            ],
          ),
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
      {"name": BrazilianNamesGenerator.generateNameWithInitial(), "inSalon": false, "isUser": true},
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
                        color: theme.colorScheme.primary.withOpacity(0.07),
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