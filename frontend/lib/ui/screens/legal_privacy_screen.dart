import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import '../widgets/bottom_nav_bar.dart';

class LegalPrivacyScreen extends StatelessWidget {
  const LegalPrivacyScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    
    return Scaffold(
      backgroundColor: colors?.primary ?? theme.colorScheme.primary,
      appBar: AppBar(
        backgroundColor: colors?.primary ?? theme.colorScheme.primary,
        elevation: 0,
        iconTheme: IconThemeData(color: colors?.onSurface ?? theme.colorScheme.onPrimary),
        title: Text(
          'Legal e Privacidade',
          style: theme.textTheme.titleLarge?.copyWith(
            color: colors?.onSurface ?? theme.colorScheme.onPrimary,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      body: SafeArea(
        child: Container(
          decoration: BoxDecoration(
            color: colors?.background ?? theme.colorScheme.surface,
            borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
          ),
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _buildHeaderSection(context, theme),
                const SizedBox(height: 32),
                _buildPrivacyPolicySection(context, theme),
                const SizedBox(height: 32),
                _buildTermsOfServiceSection(context, theme),
                const SizedBox(height: 32),
                _buildDataProtectionSection(context, theme),
                const SizedBox(height: 32),
                _buildContactSection(context, theme),
                const SizedBox(height: 32),
                _buildVersionInfo(context, theme),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildHeaderSection(BuildContext context, ThemeData theme) {
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Icon(
              Icons.gavel,
              color: colors?.primary ?? theme.colorScheme.primary,
              size: 32,
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Text(
                'Informações Legais',
                style: theme.textTheme.headlineSmall?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: colors?.onSurface ?? theme.colorScheme.onSurface,
                ),
              ),
            ),
          ],
        ),
        const SizedBox(height: 16),
        Text(
          'Esta página contém informações importantes sobre nossos termos de uso, política de privacidade e proteção de dados.',
          style: theme.textTheme.bodyLarge?.copyWith(
            color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
            height: 1.5,
          ),
        ),
      ],
    );
  }

  Widget _buildPrivacyPolicySection(BuildContext context, ThemeData theme) {
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      color: colors?.background ?? theme.colorScheme.surface,
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(Icons.privacy_tip, color: colors?.primary ?? theme.colorScheme.primary),
                const SizedBox(width: 12),
                Text(
                  'Política de Privacidade',
                  style: theme.textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: colors?.onSurface ?? theme.colorScheme.onSurface,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            _buildPrivacyItem(
              context,
              theme,
              icon: Icons.data_usage,
              title: 'Coleta de Dados',
              description: 'Coletamos apenas dados necessários para o funcionamento do aplicativo, incluindo informações de localização e preferências do usuário.',
            ),
            _buildPrivacyItem(
              context,
              theme,
              icon: Icons.security,
              title: 'Segurança',
              description: 'Seus dados são protegidos com criptografia de ponta a ponta e armazenados de forma segura.',
            ),
            _buildPrivacyItem(
              context,
              theme,
              icon: Icons.share,
              title: 'Compartilhamento',
              description: 'Não compartilhamos seus dados pessoais com terceiros sem seu consentimento explícito.',
            ),
            const SizedBox(height: 16),
            SizedBox(
              width: double.infinity,
              child: OutlinedButton.icon(
                onPressed: () => _launchURL('https://nafila.com/privacy'),
                style: OutlinedButton.styleFrom(
                  foregroundColor: colors?.primary ?? theme.colorScheme.primary,
                  side: BorderSide(color: colors?.primary ?? theme.colorScheme.primary),
                ),
                icon: const Icon(Icons.open_in_new),
                label: const Text('Ler Política Completa'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildTermsOfServiceSection(BuildContext context, ThemeData theme) {
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      color: colors?.background ?? theme.colorScheme.surface,
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(Icons.description, color: colors?.primary ?? theme.colorScheme.primary),
                const SizedBox(width: 12),
                Text(
                  'Termos de Uso',
                  style: theme.textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: colors?.onSurface ?? theme.colorScheme.onSurface,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            _buildTermsItem(
              context,
              theme,
              icon: Icons.check_circle,
              title: 'Uso Aceitável',
              description: 'O aplicativo deve ser usado apenas para agendamento de serviços em salões de beleza.',
            ),
            _buildTermsItem(
              context,
              theme,
              icon: Icons.block,
              title: 'Uso Proibido',
              description: 'É proibido usar o aplicativo para atividades ilegais ou que violem direitos de terceiros.',
            ),
            _buildTermsItem(
              context,
              theme,
              icon: Icons.cancel,
              title: 'Responsabilidades',
              description: 'O usuário é responsável pela veracidade das informações fornecidas e pelo uso adequado do serviço.',
            ),
            const SizedBox(height: 16),
            SizedBox(
              width: double.infinity,
              child: OutlinedButton.icon(
                onPressed: () => _launchURL('https://nafila.com/terms'),
                style: OutlinedButton.styleFrom(
                  foregroundColor: colors?.primary ?? theme.colorScheme.primary,
                  side: BorderSide(color: colors?.primary ?? theme.colorScheme.primary),
                ),
                icon: const Icon(Icons.open_in_new),
                label: const Text('Ler Termos Completos'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildDataProtectionSection(BuildContext context, ThemeData theme) {
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      color: colors?.background ?? theme.colorScheme.surface,
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(Icons.shield, color: colors?.primary ?? theme.colorScheme.primary),
                const SizedBox(width: 12),
                Text(
                  'Proteção de Dados (LGPD)',
                  style: theme.textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: colors?.onSurface ?? theme.colorScheme.onSurface,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            _buildLGPDItem(
              context,
              theme,
              icon: Icons.person,
              title: 'Seus Direitos',
              description: 'Você tem direito de acessar, corrigir, excluir e portar seus dados pessoais.',
            ),
            _buildLGPDItem(
              context,
              theme,
              icon: Icons.check_circle_outline,
              title: 'Consentimento',
              description: 'Seu consentimento é necessário para o processamento de dados pessoais.',
            ),
            _buildLGPDItem(
              context,
              theme,
              icon: Icons.delete_forever,
              title: 'Exclusão',
              description: 'Você pode solicitar a exclusão de seus dados a qualquer momento.',
            ),
            const SizedBox(height: 16),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () => _showDataRequestDialog(context),
                style: ElevatedButton.styleFrom(
                  backgroundColor: colors?.primary ?? theme.colorScheme.primary,
                  foregroundColor: colors?.background ?? theme.colorScheme.onPrimary,
                ),
                icon: const Icon(Icons.data_usage),
                label: const Text('Solicitar Meus Dados'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildContactSection(BuildContext context, ThemeData theme) {
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      color: colors?.background ?? theme.colorScheme.surface,
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(Icons.contact_support, color: colors?.primary ?? theme.colorScheme.primary),
                const SizedBox(width: 12),
                Text(
                  'Contato',
                  style: theme.textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: colors?.onSurface ?? theme.colorScheme.onSurface,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            _buildContactItem(
              context,
              theme,
              icon: Icons.email,
              title: 'Email',
              description: 'legal@nafila.com',
              onTap: () => _launchEmail('legal@nafila.com'),
            ),
            _buildContactItem(
              context,
              theme,
              icon: Icons.phone,
              title: 'Telefone',
              description: '+55 (11) 99999-9999',
              onTap: () => _launchPhone('+5511999999999'),
            ),
            _buildContactItem(
              context,
              theme,
              icon: Icons.location_on,
              title: 'Endereço',
              description: 'São Paulo, SP - Brasil',
              onTap: null,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildVersionInfo(BuildContext context, ThemeData theme) {
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    return Card(
      elevation: 1,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      color: colors?.background ?? theme.colorScheme.surface,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Row(
          children: [
            Icon(Icons.info_outline, color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Versão do Aplicativo',
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: colors?.onSurface ?? theme.colorScheme.onSurface,
                    ),
                  ),
                  Text(
                    '1.0.0',
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
                    ),
                  ),
                ],
              ),
            ),
            Text(
              'Última atualização: ${DateTime.now().day}/${DateTime.now().month}/${DateTime.now().year}',
              style: theme.textTheme.bodySmall?.copyWith(
                color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildPrivacyItem(
    BuildContext context, ThemeData theme, {
    required IconData icon,
    required String title,
    required String description,
  }) {
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, color: colors?.primary ?? theme.colorScheme.primary, size: 20),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: colors?.onSurface ?? theme.colorScheme.onSurface,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  description,
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTermsItem(
    BuildContext context, ThemeData theme, {
    required IconData icon,
    required String title,
    required String description,
  }) {
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, color: colors?.primary ?? theme.colorScheme.primary, size: 20),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: colors?.onSurface ?? theme.colorScheme.onSurface,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  description,
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildLGPDItem(
    BuildContext context, ThemeData theme, {
    required IconData icon,
    required String title,
    required String description,
  }) {
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, color: colors?.primary ?? theme.colorScheme.primary, size: 20),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: colors?.onSurface ?? theme.colorScheme.onSurface,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  description,
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildContactItem(
    BuildContext context, ThemeData theme, {
    required IconData icon,
    required String title,
    required String description,
    VoidCallback? onTap,
  }) {
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(8),
        child: Padding(
          padding: const EdgeInsets.all(8),
          child: Row(
            children: [
              Icon(icon, color: colors?.primary ?? theme.colorScheme.primary, size: 20),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      title,
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                        color: colors?.onSurface ?? theme.colorScheme.onSurface,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      description,
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
                      ),
                    ),
                  ],
                ),
              ),
              if (onTap != null)
                Icon(
                  Icons.arrow_forward_ios,
                  color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
                  size: 16,
                ),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _launchURL(String url) async {
    final Uri uri = Uri.parse(url);
    if (await canLaunchUrl(uri)) {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    }
  }

  Future<void> _launchEmail(String email) async {
    final Uri emailUri = Uri(
      scheme: 'mailto',
      path: email,
      query: 'subject=Dúvida sobre Legal e Privacidade - Nafila',
    );
    if (await canLaunchUrl(emailUri)) {
      await launchUrl(emailUri);
    }
  }

  Future<void> _launchPhone(String phone) async {
    final Uri phoneUri = Uri(scheme: 'tel', path: phone);
    if (await canLaunchUrl(phoneUri)) {
      await launchUrl(phoneUri);
    }
  }

  void _showDataRequestDialog(BuildContext context) {
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          backgroundColor: colors?.background ?? Theme.of(context).colorScheme.surface,
          title: Text(
            'Solicitar Dados',
            style: Theme.of(context).textTheme.titleLarge?.copyWith(
              color: colors?.onSurface ?? Theme.of(context).colorScheme.onSurface,
            ),
          ),
          content: Text(
            'Para solicitar seus dados pessoais, entre em contato conosco através do email legal@nafila.com ou pelo telefone +55 (11) 99999-9999.',
            style: Theme.of(context).textTheme.bodyLarge?.copyWith(
              color: colors?.secondary ?? Theme.of(context).colorScheme.onSurfaceVariant,
            ),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: Text(
                'Fechar',
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  color: colors?.secondary ?? Theme.of(context).colorScheme.onSurfaceVariant,
                ),
              ),
            ),
            ElevatedButton(
              style: ElevatedButton.styleFrom(
                backgroundColor: colors?.primary ?? Theme.of(context).colorScheme.primary,
                foregroundColor: colors?.background ?? Theme.of(context).colorScheme.onPrimary,
              ),
              onPressed: () {
                Navigator.of(context).pop();
                _launchEmail('legal@nafila.com');
              },
              child: Text(
                'Enviar Email',
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  color: colors?.background ?? Theme.of(context).colorScheme.onPrimary,
                ),
              ),
            ),
          ],
        );
      },
    );
  }
} 