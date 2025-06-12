import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

class QueueStatusScreen extends StatelessWidget {
  const QueueStatusScreen({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    // Mock data
    final String userName = "Rommel B";
    final String salonName = "Market at Mirada";
    final String salonAddress = "30921 Mirada Blvd, San Antonio, FL";
    final int waitTime = 34;
    final int position = 6;
    final String closingTime = "18:00";
    final String phone = "(352) 668-4089";
    final double distance = 10.9;
    final bool isOpen = true;

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
          Icon(
            Icons.notifications_outlined,
            color: theme.colorScheme.onPrimary,
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
                _buildWaitCard(theme, waitTime, position),
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
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: const [
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
          ? Icon(Icons.check, size: 14, color: AppTheme.primaryColor)
          : null,
    );
  }

  Widget _buildWaitCard(ThemeData theme, int waitTime, int position) {
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
              SizedBox(
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
                  text: '${position}º',
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
                side: BorderSide(color: AppTheme.primaryColor, width: 2),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(24),
                ),
                textStyle: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold),
                padding: const EdgeInsets.symmetric(vertical: 12),
              ),
              onPressed: () {},
              child: const Text('Ver lista de espera'),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSalonCard(ThemeData theme, String name, String address, bool isOpen, String closingTime, double distance, String phone, BuildContext context) {
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
          _buildSalonAction(theme, Icons.navigation, 'Como chegar', onTap: () {}),
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
                builder: (context) => const HaircutReminderSheet(),
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
        color: theme.colorScheme.surfaceVariant,
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
            icon: Icon(Icons.home, color: AppTheme.primaryColor),
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
  const HaircutReminderSheet({Key? key}) : super(key: key);

  @override
  State<HaircutReminderSheet> createState() => _HaircutReminderSheetState();
}

class _HaircutReminderSheetState extends State<HaircutReminderSheet> {
  int selectedWeek = 3;
  final List<int> weeks = List.generate(8, (i) => i + 1);

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
                onPressed: () {
                  Navigator.of(context).pop();
                },
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