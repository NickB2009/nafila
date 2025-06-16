import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

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
    return Scaffold(
      appBar: AppBar(
        backgroundColor: AppTheme.primaryColor,
        elevation: 0,
        iconTheme: IconThemeData(color: theme.colorScheme.onPrimary),
        title: Text(
          'Configurações de Comunicação',
          style: theme.textTheme.titleLarge?.copyWith(
            color: theme.colorScheme.onPrimary,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      backgroundColor: AppTheme.primaryColor,
      body: SafeArea(
        child: Container(
          decoration: BoxDecoration(
            color: theme.colorScheme.surface,
            borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
          ),
          child: ListView(
            padding: const EdgeInsets.all(24),
            children: [
              Text(
                'Como você gostaria de receber notificações e atualizações?',
                style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 24),
              Card(
                elevation: 2,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                color: theme.colorScheme.surfaceVariant,
                child: Column(
                  children: [
                    SwitchListTile.adaptive(
                      value: emailOn,
                      onChanged: (v) => setState(() => emailOn = v),
                      title: Text('Receber notificações por e-mail', style: theme.textTheme.bodyLarge),
                      secondary: Icon(Icons.email_outlined, color: AppTheme.primaryColor),
                    ),
                    Divider(height: 1),
                    SwitchListTile.adaptive(
                      value: smsOn,
                      onChanged: (v) => setState(() => smsOn = v),
                      title: Text('Receber notificações por SMS', style: theme.textTheme.bodyLarge),
                      secondary: Icon(Icons.sms_outlined, color: AppTheme.primaryColor),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 32),
              Text(
                'Você pode alterar essas preferências a qualquer momento.',
                style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant),
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
      ),
    );
  }
} 