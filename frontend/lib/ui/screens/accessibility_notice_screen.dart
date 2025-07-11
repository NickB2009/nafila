import 'package:flutter/material.dart';
import 'atendimento_screen.dart';

class AccessibilityNoticeScreen extends StatefulWidget {
  const AccessibilityNoticeScreen({super.key});

  @override
  State<AccessibilityNoticeScreen> createState() => _AccessibilityNoticeScreenState();
}

class _AccessibilityNoticeScreenState extends State<AccessibilityNoticeScreen>
    with TickerProviderStateMixin {
  late AnimationController _animationController;
  late List<Animation<double>> _slideAnimations;
  late List<Animation<double>> _fadeAnimations;

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 1200),
      vsync: this,
    );

    // Create staggered animations for each section
    _slideAnimations = List.generate(5, (index) {
      return Tween<double>(
        begin: -50.0,
        end: 0.0,
      ).animate(CurvedAnimation(
        parent: _animationController,
        curve: Interval(
          index * 0.12,
          (index * 0.12) + 0.5,
          curve: Curves.easeOutCubic,
        ),
      ));
    });

    _fadeAnimations = List.generate(5, (index) {
      return Tween<double>(
        begin: 0.0,
        end: 1.0,
      ).animate(CurvedAnimation(
        parent: _animationController,
        curve: Interval(
          index * 0.12,
          (index * 0.12) + 0.5,
          curve: Curves.easeOut,
        ),
      ));
    });

    // Start the animation
    _animationController.forward();
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    return Scaffold(
      backgroundColor: theme.colorScheme.primary,
      appBar: AppBar(
        backgroundColor: theme.colorScheme.primary,
        elevation: 0,
        iconTheme: IconThemeData(color: theme.colorScheme.onPrimary),
        title: Text(
          'Aviso de Acessibilidade',
          style: theme.textTheme.titleLarge?.copyWith(
            color: theme.colorScheme.onPrimary,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      body: SafeArea(
        child: Container(
          decoration: BoxDecoration(
            color: theme.colorScheme.surface,
            borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
          ),
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: AnimatedBuilder(
              animation: _animationController,
              builder: (context, child) {
                return Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Transform.translate(
                      offset: Offset(0, _slideAnimations[0].value),
                      child: Opacity(
                        opacity: _fadeAnimations[0].value,
                        child: _buildHeaderSection(theme),
                      ),
                    ),
                    const SizedBox(height: 32),
                    Transform.translate(
                      offset: Offset(0, _slideAnimations[1].value),
                      child: Opacity(
                        opacity: _fadeAnimations[1].value,
                        child: _buildFeatureGrid(theme),
                      ),
                    ),
                    const SizedBox(height: 32),
                    Transform.translate(
                      offset: Offset(0, _slideAnimations[2].value),
                      child: Opacity(
                        opacity: _fadeAnimations[2].value,
                        child: _buildInstructionsCard(theme),
                      ),
                    ),
                    const SizedBox(height: 32),
                    Transform.translate(
                      offset: Offset(0, _slideAnimations[3].value),
                      child: Opacity(
                        opacity: _fadeAnimations[3].value,
                        child: _buildComplianceSection(theme),
                      ),
                    ),
                    const SizedBox(height: 32),
                    Transform.translate(
                      offset: Offset(0, _slideAnimations[4].value),
                      child: Opacity(
                        opacity: _fadeAnimations[4].value,
                        child: _buildHelpCard(context, theme),
                      ),
                    ),
                  ],
                );
              },
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildHeaderSection(ThemeData theme) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Icon(
              Icons.accessibility_new,
              color: theme.colorScheme.primary,
              size: 32,
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Text(
                'Acessibilidade para Todos',
                style: theme.textTheme.headlineSmall?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: theme.colorScheme.onSurface,
                ),
              ),
            ),
          ],
        ),
        const SizedBox(height: 16),
        Text(
          'O Nafila está comprometido em fornecer uma experiência digital inclusiva e acessível para todos os usuários.',
          style: theme.textTheme.bodyLarge?.copyWith(
            color: theme.colorScheme.onSurfaceVariant,
            height: 1.5,
          ),
        ),
      ],
    );
  }

  Widget _buildFeatureGrid(ThemeData theme) {
    final features = [
      {'icon': Icons.visibility, 'title': 'Alto Contraste', 'description': 'Modo de alto contraste para melhor visibilidade'},
      {'icon': Icons.text_fields, 'title': 'Texto Ajustável', 'description': 'Controle o tamanho do texto conforme sua necessidade'},
      {'icon': Icons.record_voice_over, 'title': 'Leitor de Tela', 'description': 'Compatível com VoiceOver e TalkBack'},
      {'icon': Icons.keyboard, 'title': 'Navegação', 'description': 'Suporte completo a navegação por teclado'},
    ].map((f) => Map<String, dynamic>.from(f)).toList();

    return GridView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 2,
        crossAxisSpacing: 16,
        mainAxisSpacing: 16,
        childAspectRatio: 1.1,
      ),
      itemCount: features.length,
      itemBuilder: (context, index) {
        final feature = features[index];
        return Card(
          elevation: 2,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(
                  feature['icon'] as IconData,
                  color: theme.colorScheme.primary,
                  size: 32,
                ),
                const SizedBox(height: 12),
                Text(
                  feature['title'] as String,
                  style: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 4),
                Text(
                  feature['description'] as String,
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                  textAlign: TextAlign.center,
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _buildInstructionsCard(ThemeData theme) {
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(Icons.help_outline, color: theme.colorScheme.primary),
                const SizedBox(width: 12),
                Text(
                  'Como Usar',
                  style: theme.textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            _buildInstructionStep(
              theme,
              icon: Icons.text_fields,
              title: 'Ajuste de Fonte',
              description: 'Acesse "Display" nas configurações',
            ),
            _buildInstructionStep(
              theme,
              icon: Icons.contrast,
              title: 'Alto Contraste',
              description: 'Ative em "Display" nas configurações',
            ),
            _buildInstructionStep(
              theme,
              icon: Icons.record_voice_over,
              title: 'Leitor de Tela',
              description: 'Ative o VoiceOver (iOS) ou TalkBack (Android)',
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildInstructionStep(
    ThemeData theme, {
    required IconData icon,
    required String title,
    required String description,
  }) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: theme.colorScheme.primary.withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Icon(icon, color: theme.colorScheme.primary, size: 20),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                Text(
                  description,
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildComplianceSection(ThemeData theme) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: theme.colorScheme.primary.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(16),
      ),
      child: Row(
        children: [
          Icon(
            Icons.verified,
            color: theme.colorScheme.primary,
            size: 32,
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Conformidade WCAG 2.1',
                  style: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  'Seguimos as diretrizes de acessibilidade nível AA',
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildHelpCard(BuildContext context, ThemeData theme) {
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Precisa de Ajuda?',
              style: theme.textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              'Entre em contato com nossa equipe de suporte para assistência adicional.',
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
            ),
            const SizedBox(height: 16),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () {
                  print('Button pressed! Testing navigation...');
                  // First, let's test if the button works by showing a dialog
                  showDialog(
                    context: context,
                    builder: (context) => AlertDialog(
                      title: const Text('Teste de Botão'),
                      content: const Text('O botão está funcionando! Tentando navegar...'),
                      actions: [
                        TextButton(
                          onPressed: () => Navigator.of(context).pop(),
                          child: const Text('Cancelar'),
                        ),
                        ElevatedButton(
                          onPressed: () {
                            Navigator.of(context).pop(); // Close dialog
                            // Now try to navigate
                            try {
                              Navigator.of(context).push(
                                MaterialPageRoute(
                                  builder: (context) => const AtendimentoScreen(),
                                ),
                              );
                            } catch (e) {
                              print('Navigation error: $e');
                              showDialog(
                                context: context,
                                builder: (context) => AlertDialog(
                                  title: const Text('Erro de Navegação'),
                                  content: Text('Erro ao abrir tela de atendimento: $e'),
                                  actions: [
                                    TextButton(
                                      onPressed: () => Navigator.of(context).pop(),
                                      child: const Text('OK'),
                                    ),
                                  ],
                                ),
                              );
                            }
                          },
                          child: const Text('Tentar Navegar'),
                        ),
                      ],
                    ),
                  );
                },
                icon: const Icon(Icons.support_agent),
                label: const Text('Falar com Suporte'),
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
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