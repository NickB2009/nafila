import 'package:flutter/material.dart';
import '../theme/app_theme.dart';
import 'notifications_screen.dart';

class AccountScreen extends StatelessWidget {
  const AccountScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    return Scaffold(
      backgroundColor: AppTheme.primaryColor,
      body: SafeArea(
        child: Column(
          children: [
            // Header Section
            _buildHeader(theme),
            
            // Content Section
            Expanded(
              child: Container(
                decoration: BoxDecoration(
                  color: theme.colorScheme.background,
                  borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
                ),
                child: SingleChildScrollView(
                  padding: const EdgeInsets.all(20),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Account Info Card
                      _buildAccountInfoCard(theme),
                      
                      const SizedBox(height: 20),
                      
                      // Haircut Reminder Card
                      _buildHaircutReminderCard(theme),
                      
                      const SizedBox(height: 30),
                      
                      // Preferences Section
                      _buildSectionHeader(theme, 'PREFERÊNCIAS'),
                      const SizedBox(height: 16),
                      _buildMenuItem(theme, Icons.notifications_outlined, 'Configurações de Comunicação', onTap: () {}),
                      _buildMenuItem(theme, Icons.wb_sunny_outlined, 'Display', onTap: () {}),
                      
                      const SizedBox(height: 30),
                      
                      // Help & Policies Section
                      _buildSectionHeader(theme, 'AJUDA E POLÍTICAS'),
                      const SizedBox(height: 16),
                      _buildMenuItem(theme, Icons.help_outline, 'Atendimento ao Cliente', hasExternalIcon: true, onTap: () {}),
                      _buildMenuItem(theme, Icons.accessibility_outlined, 'Aviso de Acessibilidade', hasExternalIcon: true, onTap: () {}),
                      _buildMenuItem(theme, Icons.description_outlined, 'Legal e Privacidade', onTap: () {}),
                      
                      const SizedBox(height: 30),
                      
                      // Developer Section (for testing)
                      _buildSectionHeader(theme, 'DESENVOLVEDOR'),
                      const SizedBox(height: 16),
                      _buildMenuItem(theme, Icons.tv, 'Painel TV do Salão', onTap: () {
                        Navigator.of(context).pushNamed('/tv-dashboard');
                      }),
                      
                      const SizedBox(height: 20),
                    ],
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
      bottomNavigationBar: _buildBottomNavigation(context),
    );
  }

  Widget _buildHeader(ThemeData theme) {
    return Padding(
      padding: const EdgeInsets.all(20),
      child: Row(
        children: [
          // Profile Avatar
          Container(
            width: 80,
            height: 80,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              border: Border.all(
                color: theme.colorScheme.onPrimary.withOpacity(0.3),
                width: 2,
              ),
            ),
            child: Icon(
              Icons.person_outline,
              size: 40,
              color: theme.colorScheme.onPrimary.withOpacity(0.8),
            ),
          ),
          
          const SizedBox(width: 16),
          
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
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  'Rommel B',
                  style: theme.textTheme.titleLarge?.copyWith(
                    color: theme.colorScheme.onPrimary.withOpacity(0.9),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildAccountInfoCard(ThemeData theme) {
    return Container(
      padding: const EdgeInsets.all(20),
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
            onTap: () {},
          ),
          Divider(color: theme.dividerColor),
          _buildMenuItem(
            theme, 
            Icons.star_outline, 
            'Favoritos',
            showDivider: false,
            onTap: () {},
          ),
        ],
      ),
    );
  }

  Widget _buildHaircutReminderCard(ThemeData theme) {
    return Container(
      padding: const EdgeInsets.all(20),
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
                  ),
                ),
                const SizedBox(height: 8),
                Text(
                  '14 de Junho.',
                  style: theme.textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
          ),
          Container(
            width: 60,
            height: 60,
            decoration: BoxDecoration(
              color: AppTheme.primaryColor.withOpacity(0.1),
              shape: BoxShape.circle,
            ),
            child: Icon(
              Icons.content_cut,
              color: AppTheme.primaryColor,
              size: 28,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSectionHeader(ThemeData theme, String title) {
    return Text(
      title,
      style: theme.textTheme.labelLarge?.copyWith(
        color: theme.colorScheme.onSurfaceVariant,
        fontWeight: FontWeight.bold,
        letterSpacing: 0.5,
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
  }) {
    return Column(
      children: [
        ListTile(
          contentPadding: EdgeInsets.zero,
          leading: Icon(
            icon,
            color: AppTheme.primaryColor,
            size: 24,
          ),
          title: Text(
            title,
            style: theme.textTheme.bodyLarge?.copyWith(
              fontWeight: FontWeight.w500,
            ),
          ),
          trailing: Icon(
            hasExternalIcon ? Icons.open_in_new : Icons.chevron_right,
            color: theme.colorScheme.onSurfaceVariant,
            size: hasExternalIcon ? 20 : 24,
          ),
          onTap: onTap,
        ),
        if (showDivider) Divider(color: theme.dividerColor),
      ],
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
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 8,
            offset: const Offset(0, -2),
          ),
        ],
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        children: [
          _buildNavItem(context, Icons.home_outlined, false, onTap: () {
            Navigator.of(context).popUntil((route) => route.isFirst);
          }),
          _buildNavItem(context, Icons.search_outlined, false, onTap: () {
            // Navigate to search/map
          }),
          _buildNavItem(context, Icons.person, true, onTap: () {
            // Already on account screen
          }),
        ],
      ),
    );
  }

  Widget _buildNavItem(BuildContext context, IconData icon, bool isSelected, {VoidCallback? onTap}) {
    final theme = Theme.of(context);
    
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.all(12),
        child: Icon(
          icon,
          color: isSelected ? AppTheme.primaryColor : theme.colorScheme.onSurfaceVariant,
          size: 28,
        ),
      ),
    );
  }
} 