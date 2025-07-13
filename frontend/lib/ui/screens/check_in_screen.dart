import 'package:flutter/material.dart';
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
    final colors = salon.colors;
    return Scaffold(
      backgroundColor: colors.background,
      body: SafeArea(
        child: LayoutBuilder(
          builder: (context, constraints) {
            final isSmallScreen = constraints.maxWidth < 600;
            final horizontalPadding = isSmallScreen ? 16.0 : constraints.maxWidth * 0.15;
            final sectionSpacing = isSmallScreen ? 16.0 : 32.0;
            final fieldSpacing = isSmallScreen ? 12.0 : 20.0;
            final titleFontSize = isSmallScreen ? 22.0 : 28.0;
            final labelFontSize = isSmallScreen ? 13.0 : 16.0;
            final inputFontSize = isSmallScreen ? 15.0 : 18.0;
            return SingleChildScrollView(
              child: Padding(
                padding: EdgeInsets.symmetric(horizontal: horizontalPadding, vertical: sectionSpacing),
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
                          color: colors.primary,
                          iconSize: isSmallScreen ? 24 : 32,
                        ),
                      ],
                    ),
                    Text(
                      "Check-in",
                      style: theme.textTheme.titleLarge?.copyWith(
                        fontWeight: FontWeight.bold,
                        fontSize: titleFontSize,
                        color: colors.primary,
                      ),
                    ),
                    SizedBox(height: sectionSpacing * 0.5),
                    // Salon Info Card
                    Card(
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(16),
                      ),
                      elevation: 0,
                      color: colors.background,
                      child: Padding(
                        padding: EdgeInsets.all(isSmallScreen ? 12.0 : 20.0),
                        child: Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Icon(Icons.check_circle, color: colors.primary, size: isSmallScreen ? 28 : 36),
                            SizedBox(width: isSmallScreen ? 8 : 16),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    salon.name,
                                    style: theme.textTheme.headlineSmall?.copyWith(
                                      fontWeight: FontWeight.bold,
                                      fontSize: isSmallScreen ? 18 : 22,
                                      color: colors.primary,
                                    ),
                                  ),
                                  SizedBox(height: 4),
                                  Text(
                                    salon.address,
                                    style: theme.textTheme.bodyMedium?.copyWith(
                                      color: colors.secondary,
                                      fontSize: isSmallScreen ? 13 : 15,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                    SizedBox(height: sectionSpacing * 0.5),
                    Divider(color: theme.dividerColor),
                    SizedBox(height: fieldSpacing),
                    // User Info Section
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text("Nome completo", style: theme.textTheme.labelMedium?.copyWith(fontSize: labelFontSize, color: colors.primary)),
                        SizedBox(height: 2),
                        TextField(
                          controller: nameController,
                          decoration: InputDecoration(
                            hintText: "Digite seu nome",
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(12),
                              borderSide: BorderSide(color: colors.primary),
                            ),
                            contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: isSmallScreen ? 10 : 14),
                          ),
                          style: theme.textTheme.bodyLarge?.copyWith(fontSize: inputFontSize, color: colors.primary),
                        ),
                        SizedBox(height: fieldSpacing),
                        Text("Número de pessoas cortando o cabelo", style: theme.textTheme.labelMedium?.copyWith(fontSize: labelFontSize, color: colors.primary)),
                        DropdownButton<String>(
                          value: selectedPeople,
                          isExpanded: true,
                          icon: const Icon(Icons.keyboard_arrow_down),
                          items: peopleOptions.map((String value) {
                            return DropdownMenuItem<String>(
                              value: value,
                              child: Text(value, style: theme.textTheme.bodyLarge?.copyWith(fontSize: inputFontSize, color: colors.primary)),
                            );
                          }).toList(),
                          onChanged: (String? newValue) {
                            setState(() {
                              selectedPeople = newValue!;
                            });
                          },
                        ),
                        SizedBox(height: fieldSpacing),
                        Text("Telefone", style: theme.textTheme.labelMedium?.copyWith(fontSize: labelFontSize, color: colors.primary)),
                        SizedBox(height: 2),
                        TextField(
                          controller: phoneController,
                          keyboardType: TextInputType.phone,
                          decoration: InputDecoration(
                            hintText: "Digite seu telefone",
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(12),
                              borderSide: BorderSide(color: colors.primary),
                            ),
                            contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: isSmallScreen ? 10 : 14),
                          ),
                          style: theme.textTheme.bodyLarge?.copyWith(fontSize: inputFontSize, color: colors.primary),
                        ),
                      ],
                    ),
                    SizedBox(height: fieldSpacing),
                    // SMS Opt-In Section
                    Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Checkbox(
                          value: smsOptIn,
                          onChanged: (bool? value) {
                            setState(() {
                              smsOptIn = value ?? false;
                            });
                          },
                          activeColor: colors.primary,
                          checkColor: colors.background,
                          fillColor: MaterialStateProperty.all(colors.primary),
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(6)),
                        ),
                        SizedBox(width: isSmallScreen ? 6 : 12),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                "Receba uma mensagem avisando quando for a hora de ir ao salão",
                                style: theme.textTheme.bodyLarge?.copyWith(fontSize: inputFontSize, color: colors.primary),
                              ),
                              SizedBox(height: 4),
                              RichText(
                                text: TextSpan(
                                  style: theme.textTheme.bodySmall?.copyWith(
                                    color: colors.secondary,
                                    fontSize: isSmallScreen ? 11 : 13,
                                  ),
                                  children: [
                                    const TextSpan(
                                      text: "Ao fornecer seu número de celular e marcar esta caixa, você concorda em receber até duas mensagens de texto automáticas da Great Clips informando o tempo de espera para esta visita ao salão, sujeito à nossa ",
                                    ),
                                    TextSpan(
                                      text: "Política de Privacidade.",
                                      style: theme.textTheme.bodySmall?.copyWith(
                                        color: colors.primary,
                                        decoration: TextDecoration.underline,
                                        fontSize: isSmallScreen ? 11 : 13,
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
                    SizedBox(height: sectionSpacing),
                    // Confirm Button
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton(
                        style: ElevatedButton.styleFrom(
                          backgroundColor: colors.primary,
                          foregroundColor: colors.background,
                          padding: EdgeInsets.symmetric(vertical: isSmallScreen ? 14 : 18),
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                          textStyle: TextStyle(fontSize: isSmallScreen ? 16 : 20, fontWeight: FontWeight.bold),
                        ),
                        onPressed: () {
                          Navigator.of(context).push(
                            MaterialPageRoute(
                              builder: (context) => CheckInSuccessScreen(salon: salon),
                            ),
                          );
                        },
                        child: const Text("Confirmar Check-in"),
                      ),
                    ),
                  ],
                ),
              ),
            );
          },
        ),
      ),
    );
  }
} 