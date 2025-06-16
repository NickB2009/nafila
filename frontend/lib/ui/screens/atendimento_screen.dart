import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import '../theme/app_theme.dart';

class AtendimentoScreen extends StatelessWidget {
  const AtendimentoScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Scaffold(
      appBar: AppBar(
        backgroundColor: theme.colorScheme.primary,
        elevation: 0,
        iconTheme: IconThemeData(color: theme.colorScheme.onPrimary),
        title: Text(
          'Atendimento ao Cliente',
          style: theme.textTheme.titleLarge?.copyWith(
            color: theme.colorScheme.onPrimary,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      backgroundColor: theme.colorScheme.primary,
      body: SafeArea(
        child: Container(
          decoration: BoxDecoration(
            color: theme.colorScheme.surface,
            borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
          ),
          child: ListView(
            padding: const EdgeInsets.all(24),
            children: [
              Row(
                children: [
                  Icon(Icons.question_answer, color: theme.colorScheme.primary, size: 28),
                  const SizedBox(width: 10),
                  Text(
                    'Perguntas Frequentes',
                    style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
                  ),
                ],
              ),
              const SizedBox(height: 18),
              ..._buildFaq(theme),
              const SizedBox(height: 36),
              Divider(height: 32),
              const SizedBox(height: 18),
              Row(
                children: [
                  Icon(Icons.support_agent, color: theme.colorScheme.primary, size: 28),
                  const SizedBox(width: 10),
                  Text(
                    'Precisa de mais ajuda?',
                    style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
                  ),
                ],
              ),
              const SizedBox(height: 10),
              Text(
                'Escolha uma opção para falar conosco:',
                style: theme.textTheme.bodyLarge?.copyWith(color: theme.colorScheme.onSurfaceVariant),
              ),
              const SizedBox(height: 18),
              Card(
                elevation: 2,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                color: theme.colorScheme.surfaceVariant,
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 20),
                  child: Column(
                    children: [
                      _buildContactButton(
                        theme,
                        icon: Icons.email_outlined,
                        label: 'Enviar e-mail',
                        onTap: () async {
                          final uri = Uri.parse('mailto:suporte@eutonafila.com');
                          if (await canLaunchUrl(uri)) {
                            await launchUrl(uri);
                          }
                        },
                      ),
                      const SizedBox(height: 14),
                      _buildContactButton(
                        theme,
                        icon: Icons.chat,
                        label: 'WhatsApp',
                        onTap: () async {
                          final uri = Uri.parse('https://wa.me/5511999999999');
                          if (await canLaunchUrl(uri)) {
                            await launchUrl(uri, mode: LaunchMode.externalApplication);
                          }
                        },
                      ),
                      const SizedBox(height: 14),
                      _buildContactButton(
                        theme,
                        icon: Icons.phone,
                        label: 'Ligar para suporte',
                        onTap: () async {
                          final uri = Uri.parse('tel:+5511999999999');
                          if (await canLaunchUrl(uri)) {
                            await launchUrl(uri);
                          }
                        },
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  List<Widget> _buildFaq(ThemeData theme) {
    final faqs = [
      {
        'q': 'Como faço para entrar na fila de um salão?',
        'a': 'Basta selecionar o salão desejado e tocar em "Check In". Você verá sua posição na fila e o tempo estimado de espera.'
      },
      {
        'q': 'Posso cancelar meu check-in?',
        'a': 'Sim! Na tela de status da fila, toque em "Cancelar check-in" para sair da fila.'
      },
      {
        'q': 'Como recebo notificações sobre minha vez?',
        'a': 'Você pode ativar notificações por SMS ou e-mail nas configurações de comunicação.'
      },
      {
        'q': 'Como adiciono um salão aos favoritos?',
        'a': 'Toque na estrela ao lado do nome do salão na lista ou no cartão do salão.'
      },
      {
        'q': 'O que é o lembrete de corte?',
        'a': 'É uma notificação para lembrar você de agendar seu próximo corte de cabelo. Você pode definir a data nas configurações da conta.'
      },
    ];
    return faqs.map((faq) => Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      color: theme.colorScheme.surfaceVariant,
      margin: const EdgeInsets.only(bottom: 14),
      child: ExpansionTile(
        tilePadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 2),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        collapsedShape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        backgroundColor: theme.colorScheme.surfaceVariant,
        collapsedBackgroundColor: theme.colorScheme.surfaceVariant,
        leading: Icon(Icons.help_outline, color: theme.colorScheme.primary),
        title: Text(faq['q']!, style: theme.textTheme.bodyLarge?.copyWith(fontWeight: FontWeight.bold)),
        children: [
          Padding(
            padding: const EdgeInsets.only(left: 8, right: 8, bottom: 16),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Icon(Icons.question_answer, color: theme.colorScheme.onSurfaceVariant, size: 20),
                const SizedBox(width: 8),
                Expanded(child: Text(faq['a']!, style: theme.textTheme.bodyMedium)),
              ],
            ),
          ),
        ],
      ),
    )).toList();
  }

  Widget _buildContactButton(ThemeData theme, {required IconData icon, required String label, required VoidCallback onTap}) {
    return SizedBox(
      width: double.infinity,
      child: ElevatedButton.icon(
        icon: Icon(icon, color: theme.colorScheme.onPrimary),
        label: Text(label, style: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold)),
        style: ElevatedButton.styleFrom(
          backgroundColor: theme.colorScheme.primary,
          foregroundColor: theme.colorScheme.onPrimary,
          padding: const EdgeInsets.symmetric(vertical: 14),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
        ),
        onPressed: onTap,
      ),
    );
  }
} 