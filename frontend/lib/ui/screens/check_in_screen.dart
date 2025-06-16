import 'package:flutter/material.dart';
import '../theme/app_theme.dart';
import '../../models/salon.dart';
import 'check_in_success_screen.dart';

class CheckInScreen extends StatefulWidget {
  final Salon salon;
  const CheckInScreen({super.key, required this.salon});

  @override
  State<CheckInScreen> createState() => _CheckInScreenState();
}

class _CheckInScreenState extends State<CheckInScreen> {
  // Mock user data and state
  final List<String> peopleOptions = ["1 pessoa", "2 pessoas", "3 pessoas"];
  String selectedPeople = "1 pessoa";
  final TextEditingController nameController = TextEditingController();
  final TextEditingController phoneController = TextEditingController();
  bool smsOptIn = true;

  @override
  void dispose() {
    nameController.dispose();
    phoneController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final salon = widget.salon;
    return Scaffold(
      backgroundColor: theme.colorScheme.surface,
      body: SafeArea(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Top bar with close button
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                IconButton(
                  icon: const Icon(Icons.close),
                  onPressed: () => Navigator.of(context).pop(),
                  color: theme.colorScheme.onSurface,
                ),
              ],
            ),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 24.0),
              child: Text(
                "Check-in",
                style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
              ),
            ),
            const SizedBox(height: 16),
            // Salon Info Card
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 24.0),
              child: Card(
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(16),
                ),
                elevation: 0,
                color: theme.colorScheme.surface,
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Icon(Icons.check_circle, color: theme.colorScheme.primary, size: 36),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              salon.name,
                              style: theme.textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.bold),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              salon.address,
                              style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
            const SizedBox(height: 16),
            Divider(indent: 24, endIndent: 24, color: theme.dividerColor),
            const SizedBox(height: 8),
            // User Info Section
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 24.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text("Nome completo", style: theme.textTheme.labelMedium),
                  const SizedBox(height: 2),
                  TextField(
                    controller: nameController,
                    decoration: InputDecoration(
                      hintText: "Digite seu nome",
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                        borderSide: BorderSide(color: theme.colorScheme.primary),
                      ),
                      contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                    ),
                    style: theme.textTheme.bodyLarge,
                  ),
                  const SizedBox(height: 16),
                  Text("Número de pessoas cortando o cabelo", style: theme.textTheme.labelMedium),
                  DropdownButton<String>(
                    value: selectedPeople,
                    isExpanded: true,
                    icon: const Icon(Icons.keyboard_arrow_down),
                    items: peopleOptions.map((String value) {
                      return DropdownMenuItem<String>(
                        value: value,
                        child: Text(value, style: theme.textTheme.bodyLarge),
                      );
                    }).toList(),
                    onChanged: (String? newValue) {
                      setState(() {
                        selectedPeople = newValue!;
                      });
                    },
                  ),
                  const SizedBox(height: 16),
                  Text("Telefone", style: theme.textTheme.labelMedium),
                  const SizedBox(height: 2),
                  TextField(
                    controller: phoneController,
                    keyboardType: TextInputType.phone,
                    decoration: InputDecoration(
                      hintText: "Digite seu telefone",
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                        borderSide: BorderSide(color: theme.colorScheme.primary),
                      ),
                      contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                    ),
                    style: theme.textTheme.bodyLarge,
                  ),
                ],
              ),
            ),
            const SizedBox(height: 16),
            // SMS Opt-In Section
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 24.0),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Checkbox(
                    value: smsOptIn,
                    onChanged: (bool? value) {
                      setState(() {
                        smsOptIn = value ?? false;
                      });
                    },
                    activeColor: theme.colorScheme.primary,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(6)),
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          "Receba uma mensagem avisando quando for a hora de ir ao salão",
                          style: theme.textTheme.bodyLarge,
                        ),
                        const SizedBox(height: 4),
                        RichText(
                          text: TextSpan(
                            style: theme.textTheme.bodySmall?.copyWith(color: theme.colorScheme.onSurfaceVariant),
                            children: [
                              const TextSpan(
                                text: "Ao fornecer seu número de celular e marcar esta caixa, você concorda em receber até duas mensagens de texto automáticas da Great Clips informando o tempo de espera para esta visita ao salão, sujeito à nossa ",
                              ),
                              TextSpan(
                                text: "Política de Privacidade.",
                                style: theme.textTheme.bodySmall?.copyWith(
                                  color: theme.colorScheme.primary,
                                  decoration: TextDecoration.underline,
                                ),
                              ),
                              const TextSpan(
                                text: " Seu consentimento não é condição para compra. Tarifas de mensagem e dados podem ser aplicadas.",
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
            const Spacer(),
            // Footer
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 8.0),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text("Powered by ICS", style: theme.textTheme.labelSmall),
                  const SizedBox(width: 8),
                  Icon(Icons.credit_card, size: 18, color: theme.colorScheme.onSurfaceVariant), // Placeholder for Net Check In logo
                  const SizedBox(width: 8),
                  Text("Net Check In™", style: theme.textTheme.labelSmall),
                ],
              ),
            ),
            // Check In Button
            Padding(
              padding: const EdgeInsets.fromLTRB(24, 0, 24, 24),
              child: SizedBox(
                width: double.infinity,
                height: 48,
                child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: theme.colorScheme.primary,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(24),
                    ),
                  ),
                  onPressed: () {
                    Navigator.of(context).push(
                      MaterialPageRoute(
                        builder: (_) => const CheckInSuccessScreen(),
                      ),
                    );
                  },
                  child: Text(
                    "Check-in",
                    style: theme.textTheme.titleMedium?.copyWith(
                      color: theme.colorScheme.onPrimary,
                      fontWeight: FontWeight.bold,
                    ),
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