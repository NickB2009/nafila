import 'package:flutter/material.dart';
import '../widgets/bottom_nav_bar.dart';

class ComunicacoesScreen extends StatefulWidget {
  const ComunicacoesScreen({super.key});

  @override
  State<ComunicacoesScreen> createState() => _ComunicacoesScreenState();
}

class _ComunicacoesScreenState extends State<ComunicacoesScreen> {
  bool emailOn = true;
  bool smsOn = true;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    return Scaffold(
      appBar: AppBar(
        backgroundColor: colors?.primary ?? theme.colorScheme.primary,
        elevation: 0,
        iconTheme: IconThemeData(color: colors?.background ?? theme.colorScheme.onPrimary),
        title: Text(
          'Configurações de Comunicação',
          style: theme.textTheme.titleLarge?.copyWith(
            color: colors?.background ?? theme.colorScheme.onPrimary,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      backgroundColor: colors?.primary ?? theme.colorScheme.primary,
      body: SafeArea(
        child: Container(
          decoration: BoxDecoration(
            color: colors?.background ?? theme.colorScheme.surface,
            borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
          ),
          child: ListView(
            padding: const EdgeInsets.all(24),
            children: [
              Text(
                'Como você gostaria de receber notificações e atualizações?',
                style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold, color: colors?.onSurface ?? theme.colorScheme.onSurface),
              ),
              const SizedBox(height: 24),
              Card(
                elevation: 2,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                color: colors?.background ?? theme.colorScheme.surface,
                child: Column(
                  children: [
                    SwitchListTile.adaptive(
                      value: emailOn,
                      onChanged: (v) => setState(() => emailOn = v),
                      title: Text('Receber notificações por e-mail', style: theme.textTheme.bodyLarge?.copyWith(color: colors?.onSurface ?? theme.colorScheme.onSurface)),
                      secondary: Icon(Icons.email_outlined, color: colors?.primary ?? theme.colorScheme.primary),
                    ),
                    const Divider(height: 1),
                    SwitchListTile.adaptive(
                      value: smsOn,
                      onChanged: (v) => setState(() => smsOn = v),
                      title: Text('Receber notificações por SMS', style: theme.textTheme.bodyLarge?.copyWith(color: colors?.onSurface ?? theme.colorScheme.onSurface)),
                      secondary: Icon(Icons.sms_outlined, color: colors?.primary ?? theme.colorScheme.primary),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 32),
              Text(
                'Você pode alterar essas preferências a qualquer momento.',
                style: theme.textTheme.bodyMedium?.copyWith(color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant),
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
      ),
    );
  }
} 