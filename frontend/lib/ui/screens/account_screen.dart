import 'package:flutter/material.dart';
import '../../utils/brazilian_names_generator.dart';
import 'personal_info_screen.dart';
import 'favoritos_screen.dart';
import 'comunicacoes_screen.dart';
import 'display_screen.dart';
import 'atendimento_screen.dart';
import 'accessibility_notice_screen.dart';
import 'legal_privacy_screen.dart';
import '../widgets/bottom_nav_bar.dart';

class AccountScreen extends StatefulWidget {
  const AccountScreen({super.key});

  @override
  State<AccountScreen> createState() => _AccountScreenState();
}

class _AccountScreenState extends State<AccountScreen> {
  DateTime _reminderDate = DateTime.now().add(const Duration(days: 14));

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Scaffold(
      backgroundColor: theme.colorScheme.primary,
      appBar: AppBar(
        backgroundColor: theme.colorScheme.primary,
      ),
      body: SafeArea(
        child: LayoutBuilder(
          builder: (context, constraints) {
            final isSmallScreen = constraints.maxWidth < 600;
            final horizontalPadding = isSmallScreen ? 12.0 : constraints.maxWidth * 0.15;
            final sectionSpacing = isSmallScreen ? 16.0 : 32.0;
            final cardPadding = isSmallScreen ? 14.0 : 24.0;
            final avatarSize = isSmallScreen ? 60.0 : 80.0;
            final iconSize = isSmallScreen ? 28.0 : 40.0;
            final titleFontSize = isSmallScreen ? 20.0 : 26.0;
            final subtitleFontSize = isSmallScreen ? 15.0 : 18.0;
            return SingleChildScrollView(
              child: Padding(
                padding: EdgeInsets.symmetric(horizontal: horizontalPadding, vertical: sectionSpacing),
                child: Column(
                  children: [
                    // Header Section
                    _buildHeader(theme, avatarSize, iconSize, titleFontSize, subtitleFontSize),
                    SizedBox(height: sectionSpacing * 0.5),
                    // Content Section
                    Container(
                      decoration: BoxDecoration(
                        color: theme.colorScheme.surface,
                        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
                      ),
                      child: Padding(
                        padding: EdgeInsets.all(cardPadding),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            // Account Info Card
                            _buildAccountInfoCard(context, theme, cardPadding, isSmallScreen),
                            SizedBox(height: sectionSpacing * 0.5),
                            // Haircut Reminder Card
                            _buildHaircutReminderCard(theme, cardPadding, isSmallScreen, titleFontSize),
                            SizedBox(height: sectionSpacing),
                            // Preferences Section
                            _buildSectionHeader(theme, 'PREFERÊNCIAS', titleFontSize),
                            SizedBox(height: isSmallScreen ? 10 : 16),
                            _buildMenuItem(theme, Icons.notifications_outlined, 'Configurações de Comunicação', onTap: () {
                              Navigator.of(context).push(
                                MaterialPageRoute(builder: (_) => const ComunicacoesScreen()),
                              );
                            }, isSmallScreen: isSmallScreen),
                            _buildMenuItem(theme, Icons.wb_sunny_outlined, 'Display', onTap: () {
                              Navigator.of(context).push(
                                MaterialPageRoute(builder: (_) => const DisplayScreen()),
                              );
                            }, isSmallScreen: isSmallScreen),
                            SizedBox(height: sectionSpacing),
                            // Help & Policies Section
                            _buildSectionHeader(theme, 'AJUDA E POLÍTICAS', titleFontSize),
                            SizedBox(height: isSmallScreen ? 10 : 16),
                            _buildMenuItem(theme, Icons.help_outline, 'Atendimento ao Cliente', hasExternalIcon: true, onTap: () {
                              Navigator.of(context).push(
                                MaterialPageRoute(builder: (_) => const AtendimentoScreen()),
                              );
                            }, isSmallScreen: isSmallScreen),
                            _buildMenuItem(theme, Icons.accessibility_outlined, 'Aviso de Acessibilidade', hasExternalIcon: true, onTap: () {
                              Navigator.of(context).push(
                                MaterialPageRoute(builder: (_) => const AccessibilityNoticeScreen()),
                              );
                            }, isSmallScreen: isSmallScreen),
                            _buildMenuItem(theme, Icons.description_outlined, 'Legal e Privacidade', onTap: () {
                              Navigator.of(context).push(
                                MaterialPageRoute(builder: (_) => const LegalPrivacyScreen()),
                              );
                            }, isSmallScreen: isSmallScreen),
                            SizedBox(height: sectionSpacing),
                            // Developer Section (for testing)
                            _buildSectionHeader(theme, 'DESENVOLVEDOR', titleFontSize),
                            SizedBox(height: isSmallScreen ? 10 : 16),
                            _buildMenuItem(theme, Icons.tv, 'Painel TV do Salão', onTap: () {
                              Navigator.of(context).pushNamed('/tv-dashboard');
                            }, isSmallScreen: isSmallScreen),
                            SizedBox(height: isSmallScreen ? 12 : 20),
                          ],
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            );
          },
        ),
      ),
      bottomNavigationBar: const BottomNavBar(currentIndex: 2),
    );
  }

  Widget _buildHeader(ThemeData theme, double avatarSize, double iconSize, double titleFontSize, double subtitleFontSize) {
    return Padding(
      padding: const EdgeInsets.all(8),
      child: Row(
        children: [
          // Profile Avatar
          Container(
            width: avatarSize,
            height: avatarSize,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              border: Border.all(
                color: theme.colorScheme.onPrimary.withOpacity(0.3),
                width: 2,
              ),
            ),
            child: Icon(
              Icons.person_outline,
              size: iconSize,
              color: theme.colorScheme.onPrimary.withOpacity(0.8),
            ),
          ),
          SizedBox(width: avatarSize * 0.2),
          // User Info
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Conta',
                  style: theme.textTheme.headlineMedium?.copyWith(
                    color: theme.colorScheme.onPrimary,
                    fontWeight: FontWeight.bold,
                    fontSize: titleFontSize,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  BrazilianNamesGenerator.generateNameWithInitial(),
                  style: theme.textTheme.titleLarge?.copyWith(
                    color: theme.colorScheme.onPrimary.withOpacity(0.9),
                    fontSize: subtitleFontSize,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildAccountInfoCard(BuildContext context, ThemeData theme, double cardPadding, bool isSmallScreen) {
    return Container(
      padding: EdgeInsets.all(cardPadding),
      decoration: BoxDecoration(
        color: theme.colorScheme.surface,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        children: [
          _buildMenuItem(
            theme, 
            Icons.person_outline, 
            'Informações Pessoais',
            showDivider: false,
            onTap: () {
              Navigator.of(context).push(
                MaterialPageRoute(builder: (_) => const PersonalInfoScreen()),
              );
            },
            isSmallScreen: isSmallScreen,
          ),
          Divider(color: theme.dividerColor),
          _buildMenuItem(
            theme, 
            Icons.star_outline, 
            'Favoritos',
            showDivider: false,
            onTap: () {
              Navigator.of(context).push(
                MaterialPageRoute(builder: (_) => const FavoritosScreen()),
              );
            },
            isSmallScreen: isSmallScreen,
          ),
        ],
      ),
    );
  }

  Widget _buildHaircutReminderCard(ThemeData theme, double cardPadding, bool isSmallScreen, double titleFontSize) {
    return Container(
      padding: EdgeInsets.all(cardPadding),
      decoration: BoxDecoration(
        color: theme.colorScheme.surface,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Seu próximo lembrete de corte está definido para',
                  style: theme.textTheme.bodyLarge?.copyWith(
                    fontWeight: FontWeight.w500,
                    fontSize: isSmallScreen ? 13 : 16,
                  ),
                ),
                const SizedBox(height: 8),
                Row(
                  children: [
                    Text(
                      _formatDate(_reminderDate),
                      style: theme.textTheme.titleLarge?.copyWith(
                        fontWeight: FontWeight.bold,
                        fontSize: titleFontSize,
                      ),
                    ),
                    const SizedBox(width: 8),
                    TextButton.icon(
                      onPressed: () async {
                        final picked = await showDatePicker(
                          context: context,
                          initialDate: _reminderDate,
                          firstDate: DateTime.now(),
                          lastDate: DateTime.now().add(const Duration(days: 365)),
                          locale: const Locale('pt', 'BR'),
                        );
                        if (picked != null && picked != _reminderDate) {
                          setState(() {
                            _reminderDate = picked;
                          });
                          ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(
                              content: Text('Lembrete atualizado!'),
                              duration: Duration(seconds: 1),
                            ),
                          );
                        }
                      },
                      icon: Icon(Icons.edit_calendar, size: isSmallScreen ? 16 : 18),
                      label: const Text('Alterar'),
                      style: TextButton.styleFrom(
                        foregroundColor: theme.colorScheme.primary,
                        textStyle: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold, fontSize: isSmallScreen ? 12 : 14),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          Container(
            width: isSmallScreen ? 44 : 60,
            height: isSmallScreen ? 44 : 60,
            decoration: BoxDecoration(
              color: theme.colorScheme.primary.withOpacity(0.1),
              shape: BoxShape.circle,
            ),
            child: Icon(
              Icons.content_cut,
              color: theme.colorScheme.primary,
              size: isSmallScreen ? 20 : 28,
            ),
          ),
        ],
      ),
    );
  }

  String _formatDate(DateTime date) {
    // Ex: 14 de Junho de 2024
    final months = [
      '', 'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
      'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
    ];
    return '${date.day} de ${months[date.month]} de ${date.year}';
  }

  Widget _buildSectionHeader(ThemeData theme, String title, double titleFontSize) {
    return Text(
      title,
      style: theme.textTheme.labelLarge?.copyWith(
        color: theme.colorScheme.onSurfaceVariant,
        fontWeight: FontWeight.bold,
        letterSpacing: 0.5,
        fontSize: titleFontSize * 0.7,
      ),
    );
  }

  Widget _buildMenuItem(
    ThemeData theme, 
    IconData icon, 
    String title, {
    bool hasExternalIcon = false,
    bool showDivider = true,
    VoidCallback? onTap,
    bool isSmallScreen = false,
  }) {
    return Column(
      children: [
        ListTile(
          contentPadding: EdgeInsets.symmetric(vertical: isSmallScreen ? 2 : 6, horizontal: 0),
          leading: Icon(
            icon,
            color: theme.colorScheme.primary,
            size: isSmallScreen ? 20 : 24,
          ),
          title: Text(
            title,
            style: theme.textTheme.bodyLarge?.copyWith(
              fontWeight: FontWeight.w500,
              fontSize: isSmallScreen ? 14 : 16,
            ),
          ),
          trailing: Icon(
            hasExternalIcon ? Icons.open_in_new : Icons.chevron_right,
            color: theme.colorScheme.onSurfaceVariant,
            size: hasExternalIcon ? (isSmallScreen ? 16 : 20) : (isSmallScreen ? 20 : 24),
          ),
          onTap: onTap,
        ),
        if (showDivider) Divider(color: theme.dividerColor),
      ],
    );
  }
} 