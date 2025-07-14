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
  late List<Map<String, dynamic>> haircutHistory;

  @override
  void initState() {
    super.initState();
    haircutHistory = [
      {
        'date': DateTime.now().subtract(const Duration(days: 15)),
        'salon': 'Market at Mirada',
        'style': 'Corte Clássico',
        'technique': 'Tesoura + Máquina',
        'price': 'R\$ 45,00',
        'barber': 'Carlos Silva',
        'description': {
          'sides': 'Número 2',
          'fade': 'Fade Médio',
          'top': 'Tesoura 4cm',
          'fringe': 'Lateral Direita',
          'neckline': 'Arredondada',
          'beard': 'Aparada',
          'notes': 'Cliente prefere deixar um pouco mais comprido na frente. Usar gel para finalizar.',
        },
        'favorite': false,
      },
      {
        'date': DateTime.now().subtract(const Duration(days: 45)),
        'salon': 'Cortez Commons',
        'style': 'Fade Americano',
        'technique': 'Degradê',
        'price': 'R\$ 60,00',
        'barber': 'João Santos',
        'description': {
          'sides': 'Número 1',
          'fade': 'Fade Alto',
          'top': 'Tesoura 6cm',
          'fringe': 'Para Cima',
          'neckline': 'Quadrada',
          'beard': 'Não',
          'notes': 'Degradê bem marcado nas laterais. Cliente gosta do estilo mais moderno.',
        },
        'favorite': true,
      },
      {
        'date': DateTime.now().subtract(const Duration(days: 78)),
        'salon': 'Barbearia Central',
        'style': 'Corte Social',
        'technique': 'Tesoura',
        'price': 'R\$ 35,00',
        'barber': 'Pedro Lima',
        'description': {
          'sides': 'Tesoura Curta',
          'fade': 'Sem Fade',
          'top': 'Tesoura 5cm',
          'fringe': 'Lateral Esquerda',
          'neckline': 'Arredondada',
          'beard': 'Aparada',
          'notes': 'Corte tradicional, conservador. Cliente trabalha em escritório.',
        },
        'favorite': false,
      },
    ];
    _sortHaircuts();
  }

  void _favoriteHaircut(int index) {
    setState(() {
      final item = haircutHistory.removeAt(index);
      item['favorite'] = true;
      haircutHistory.insert(0, item);
      _sortHaircuts();
    });
  }

  void _sortHaircuts() {
    haircutHistory.sort((a, b) {
      if (a['favorite'] == b['favorite']) {
        return b['date'].compareTo(a['date']);
      }
      return (b['favorite'] ? 1 : 0) - (a['favorite'] ? 1 : 0);
    });
  }

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
                            // Haircut History Card
                            _buildHaircutHistoryCard(theme, cardPadding, isSmallScreen, titleFontSize),
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

  Widget _buildHaircutHistoryCard(ThemeData theme, double cardPadding, bool isSmallScreen, double titleFontSize) {
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
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Histórico de Cortes',
                style: theme.textTheme.titleLarge?.copyWith(
                  fontWeight: FontWeight.bold,
                  fontSize: titleFontSize * 0.9,
                ),
              ),
              TextButton(
                onPressed: () {
                  Navigator.of(context).push(
                    MaterialPageRoute(
                      builder: (_) => FullHaircutHistoryPage(haircutHistory: haircutHistory),
                    ),
                  );
                },
                child: Text(
                  'Ver todos',
                  style: TextStyle(
                    color: theme.colorScheme.primary,
                    fontSize: isSmallScreen ? 12 : 14,
                  ),
                ),
              ),
            ],
          ),
          SizedBox(height: isSmallScreen ? 8 : 12),
          ...haircutHistory.map((haircut) => _buildHaircutHistoryItem(context, theme, isSmallScreen, haircut)),
        ],
      ),
    );
  }

  Widget _buildHaircutHistoryItem(BuildContext context, ThemeData theme, bool isSmallScreen, Map<String, dynamic> haircut) {
    return InkWell(
      onTap: () {
        // Show haircut details
        showDialog(
          context: context,
          builder: (context) => AlertDialog(
            title: Text('Detalhes do Corte'),
            content: SingleChildScrollView(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Basic info
                  buildDetailRow('Data', _formatDate(haircut['date']), theme),
                  buildDetailRow('Salão', haircut['salon'], theme),
                  buildDetailRow('Estilo', haircut['style'], theme),
                  buildDetailRow('Técnica', haircut['technique'], theme),
                  buildDetailRow('Barbeiro', haircut['barber'], theme),
                  buildDetailRow('Preço', haircut['price'], theme),
                  
                  if (haircut['description'] != null) ...[
                    const SizedBox(height: 16),
                    Text(
                      'Descrição Detalhada',
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                        color: theme.colorScheme.primary,
                      ),
                    ),
                    const SizedBox(height: 8),
                    buildDetailRow('Laterais', haircut['description']['sides'], theme),
                    buildDetailRow('Fade', haircut['description']['fade'], theme),
                    buildDetailRow('Topo', haircut['description']['top'], theme),
                    buildDetailRow('Franja', haircut['description']['fringe'], theme),
                    buildDetailRow('Nuca', haircut['description']['neckline'], theme),
                    buildDetailRow('Barba', haircut['description']['beard'], theme),
                    
                    if (haircut['description']['notes'] != null && haircut['description']['notes']!.isNotEmpty) ...[
                      const SizedBox(height: 12),
                      Container(
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: theme.colorScheme.surfaceVariant.withOpacity(0.3),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Observações do Barbeiro',
                              style: theme.textTheme.bodyMedium?.copyWith(
                                fontWeight: FontWeight.w600,
                                color: theme.colorScheme.primary,
                              ),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              haircut['description']['notes'],
                              style: theme.textTheme.bodyMedium?.copyWith(
                                fontStyle: FontStyle.italic,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ],
                  const SizedBox(height: 16),
                  ElevatedButton.icon(
                    icon: const Icon(Icons.sync_alt),
                    label: const Text('Transferir para Preferências'),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: theme.colorScheme.primary,
                      foregroundColor: Colors.white,
                    ),
                    onPressed: () {
                      Navigator.of(context).pop();
                      _transferCutToPreferences(context, haircut['description']);
                    },
                  ),
                ],
              ),
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.pop(context),
                child: const Text('Fechar'),
              ),
              TextButton(
                onPressed: () {
                  Navigator.pop(context);
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Corte favoritado!')),
                  );
                },
                child: Text(
                  'Favoritar',
                  style: TextStyle(color: theme.colorScheme.primary),
                ),
              ),
            ],
          ),
        );
      },
      borderRadius: BorderRadius.circular(12),
      child: Padding(
        padding: EdgeInsets.symmetric(vertical: isSmallScreen ? 8 : 12, horizontal: 8),
        child: Row(
          children: [
            Container(
              width: isSmallScreen ? 40 : 48,
              height: isSmallScreen ? 40 : 48,
              decoration: BoxDecoration(
                color: theme.colorScheme.primary.withOpacity(0.1),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Icon(
                Icons.content_cut,
                color: theme.colorScheme.primary,
                size: isSmallScreen ? 20 : 24,
              ),
            ),
            SizedBox(width: isSmallScreen ? 12 : 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '${_formatDate(haircut['date'])}',
                    style: theme.textTheme.bodyLarge?.copyWith(
                      fontWeight: FontWeight.w600,
                      fontSize: isSmallScreen ? 14 : 16,
                    ),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    '${haircut['salon']} • ${haircut['style']}',
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: theme.colorScheme.onSurfaceVariant,
                      fontSize: isSmallScreen ? 12 : 14,
                    ),
                  ),
                ],
              ),
            ),
            Text(
              haircut['price'],
              style: theme.textTheme.bodyMedium?.copyWith(
                fontWeight: FontWeight.w600,
                color: theme.colorScheme.primary,
                fontSize: isSmallScreen ? 12 : 14,
              ),
            ),
            SizedBox(width: isSmallScreen ? 8 : 12),
            Icon(
              Icons.chevron_right,
              color: theme.colorScheme.onSurfaceVariant,
              size: isSmallScreen ? 20 : 24,
            ),
            IconButton(
              icon: const Icon(Icons.sync_alt),
              tooltip: 'Transferir para Preferências',
              color: theme.colorScheme.primary,
              onPressed: () {
                _transferCutToPreferences(context, haircut['description']);
              },
            ),
          ],
        ),
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
                    Flexible(
                      child: Text(
                        _formatDate(_reminderDate),
                        style: theme.textTheme.titleLarge?.copyWith(
                          fontWeight: FontWeight.bold,
                          fontSize: titleFontSize,
                        ),
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Flexible(
                      child: TextButton.icon(
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

// Global function so it can be used by any widget in this file
Widget buildDetailRow(String label, String value, ThemeData theme) {
  return Row(
    crossAxisAlignment: CrossAxisAlignment.start,
    children: [
      Text(
        '$label: ',
        style: theme.textTheme.bodyMedium?.copyWith(
          fontWeight: FontWeight.w600,
          color: theme.colorScheme.onSurfaceVariant,
        ),
      ),
      Expanded(
        child: Text(
          value,
          style: theme.textTheme.bodyMedium?.copyWith(
            fontWeight: FontWeight.w500,
          ),
        ),
      ),
    ],
  );
}

// Global function for date formatting
String _formatDate(DateTime date) {
  // Ex: 14 de Junho de 2024
  final months = [
    '', 'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
    'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
  ];
  return '${date.day} de ${months[date.month]} de ${date.year}';
}

// --- Full Haircut History Page ---
class FullHaircutHistoryPage extends StatefulWidget {
  final List<Map<String, dynamic>> haircutHistory;
  const FullHaircutHistoryPage({super.key, required this.haircutHistory});

  @override
  State<FullHaircutHistoryPage> createState() => _FullHaircutHistoryPageState();
}

class _FullHaircutHistoryPageState extends State<FullHaircutHistoryPage> {
  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final salonPalette = CheckInState.checkedInSalon?.colors;
    return Scaffold(
      appBar: AppBar(
        title: const Text('Histórico Completo de Cortes'),
        backgroundColor: salonPalette?.primary ?? theme.colorScheme.primary,
        foregroundColor: salonPalette != null ? salonPalette.background : theme.colorScheme.onPrimary,
      ),
      backgroundColor: salonPalette?.background ?? theme.colorScheme.surface,
      body: ListView.separated(
        padding: const EdgeInsets.all(20),
        itemCount: widget.haircutHistory.length,
        separatorBuilder: (_, __) => const SizedBox(height: 16),
        itemBuilder: (context, i) {
          final haircut = widget.haircutHistory[i];
          return InkWell(
            onTap: () {
              showDialog(
                context: context,
                builder: (context) => AlertDialog(
                  title: Text('Detalhes do Corte'),
                  content: SingleChildScrollView(
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        buildDetailRow('Data', _formatDate(haircut['date']), theme),
                        buildDetailRow('Salão', haircut['salon'], theme),
                        buildDetailRow('Estilo', haircut['style'], theme),
                        buildDetailRow('Técnica', haircut['technique'], theme),
                        buildDetailRow('Barbeiro', haircut['barber'], theme),
                        buildDetailRow('Preço', haircut['price'], theme),
                        if (haircut['description'] != null) ...[
                          const SizedBox(height: 16),
                          Text(
                            'Descrição Detalhada',
                            style: theme.textTheme.titleMedium?.copyWith(
                              fontWeight: FontWeight.bold,
                              color: salonPalette?.primary ?? theme.colorScheme.primary,
                            ),
                          ),
                          const SizedBox(height: 8),
                          buildDetailRow('Laterais', haircut['description']['sides'], theme),
                          buildDetailRow('Fade', haircut['description']['fade'], theme),
                          buildDetailRow('Topo', haircut['description']['top'], theme),
                          buildDetailRow('Franja', haircut['description']['fringe'], theme),
                          buildDetailRow('Nuca', haircut['description']['neckline'], theme),
                          buildDetailRow('Barba', haircut['description']['beard'], theme),
                          if (haircut['description']['notes'] != null && haircut['description']['notes']!.isNotEmpty) ...[
                            const SizedBox(height: 12),
                            Container(
                              padding: const EdgeInsets.all(12),
                              decoration: BoxDecoration(
                                color: (salonPalette?.primary ?? theme.colorScheme.primary).withOpacity(0.08),
                                borderRadius: BorderRadius.circular(8),
                              ),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    'Observações do Barbeiro',
                                    style: theme.textTheme.bodyMedium?.copyWith(
                                      fontWeight: FontWeight.w600,
                                      color: salonPalette?.primary ?? theme.colorScheme.primary,
                                    ),
                                  ),
                                  const SizedBox(height: 4),
                                  Text(
                                    haircut['description']['notes'],
                                    style: theme.textTheme.bodyMedium?.copyWith(
                                      fontStyle: FontStyle.italic,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ],
                        const SizedBox(height: 16),
                        ElevatedButton.icon(
                          icon: const Icon(Icons.sync_alt),
                          label: const Text('Transferir para Preferências'),
                          style: ElevatedButton.styleFrom(
                            backgroundColor: salonPalette?.primary ?? theme.colorScheme.primary,
                            foregroundColor: Colors.white,
                          ),
                          onPressed: () {
                            Navigator.of(context).pop();
                            _transferCutToPreferences(context, haircut['description']);
                          },
                        ),
                      ],
                    ),
                  ),
                  actions: [
                    TextButton(
                      onPressed: () => Navigator.pop(context),
                      child: const Text('Fechar'),
                    ),
                    TextButton(
                      onPressed: () {
                        Navigator.pop(context);
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(content: Text('Corte favoritado!')),
                        );
                      },
                      child: Text(
                        'Favoritar',
                        style: TextStyle(color: salonPalette?.primary ?? theme.colorScheme.primary),
                      ),
                    ),
                  ],
                ),
              );
            },
            borderRadius: BorderRadius.circular(12),
            child: Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: salonPalette?.background ?? theme.colorScheme.surface,
                borderRadius: BorderRadius.circular(12),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.04),
                    blurRadius: 6,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: Row(
                children: [
                  Container(
                    width: 44,
                    height: 44,
                    decoration: BoxDecoration(
                      color: (salonPalette?.primary ?? theme.colorScheme.primary).withOpacity(0.12),
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Icon(
                      Icons.content_cut,
                      color: salonPalette?.primary ?? theme.colorScheme.primary,
                      size: 22,
                    ),
                  ),
                  const SizedBox(width: 16),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          _formatDate(haircut['date']),
                          style: theme.textTheme.bodyLarge?.copyWith(
                            fontWeight: FontWeight.w600,
                            color: salonPalette?.primary ?? theme.colorScheme.primary,
                          ),
                        ),
                        const SizedBox(height: 2),
                        Text(
                          '${haircut['salon']} • ${haircut['style']}',
                          style: theme.textTheme.bodyMedium?.copyWith(
                            color: theme.colorScheme.onSurfaceVariant,
                          ),
                        ),
                      ],
                    ),
                  ),
                  Text(
                    haircut['price'],
                    style: theme.textTheme.bodyMedium?.copyWith(
                      fontWeight: FontWeight.w600,
                      color: salonPalette?.primary ?? theme.colorScheme.primary,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Icon(
                    Icons.chevron_right,
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
} 

// Add the transfer logic
void _transferCutToPreferences(BuildContext context, Map<String, dynamic> desc) {
  Navigator.of(context).push(
    MaterialPageRoute(
      builder: (_) => PersonalInfoScreen(
        initialPreferences: {
          'sides': desc['sides'] ?? '',
          'fade': desc['fade'] ?? '',
          'top': desc['top'] ?? '',
          'franja': desc['fringe'] ?? '',
          'neckline': desc['neckline'] ?? '',
          'beard': desc['beard'] ?? '',
          'notes': desc['notes'] ?? '',
        },
      ),
    ),
  );
} 