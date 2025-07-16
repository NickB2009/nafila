import 'package:flutter/material.dart';
import 'atendimento_screen.dart';
import '../widgets/bottom_nav_bar.dart';

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
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    
    return Scaffold(
      backgroundColor: colors?.primary ?? theme.colorScheme.primary,
      appBar: AppBar(
        backgroundColor: colors?.primary ?? theme.colorScheme.primary,
        elevation: 0,
        iconTheme: IconThemeData(color: colors?.onSurface ?? theme.colorScheme.onPrimary),
        title: Text(
          'Aviso de Acessibilidade',
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
              color: CheckInState.checkedInSalon?.colors.primary ?? theme.colorScheme.primary,
              size: 32,
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Text(
                'Acessibilidade para Todos',
                style: theme.textTheme.headlineSmall?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: CheckInState.checkedInSalon?.colors.onSurface ?? theme.colorScheme.onSurface,
                ),
              ),
            ),
          ],
        ),
        const SizedBox(height: 16),
        Text(
          'O Nafila está comprometido em fornecer uma experiência digital inclusiva e acessível para todos os usuários.',
          style: theme.textTheme.bodyLarge?.copyWith(
            color: CheckInState.checkedInSalon?.colors.secondary ?? theme.colorScheme.onSurfaceVariant,
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
          color: CheckInState.checkedInSalon?.colors.background ?? theme.colorScheme.surface,
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Icon(feature['icon'] as IconData, color: CheckInState.checkedInSalon?.colors.primary ?? theme.colorScheme.primary, size: 28),
                const SizedBox(height: 10),
                Text(
                  feature['title'] as String,
                  style: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold, color: CheckInState.checkedInSalon?.colors.onSurface ?? theme.colorScheme.onSurface),
                ),
                const SizedBox(height: 6),
                Text(
                  feature['description'] as String,
                  style: theme.textTheme.bodyMedium?.copyWith(color: CheckInState.checkedInSalon?.colors.secondary ?? theme.colorScheme.onSurfaceVariant),
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _buildInstructionsCard(ThemeData theme) {
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      color: colors?.background ?? theme.colorScheme.surface,
      child: Padding(
        padding: const EdgeInsets.all(18),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Como usar', style: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold, color: colors?.onSurface ?? theme.colorScheme.onSurface)),
            const SizedBox(height: 10),
            Text(
              'Acesse as configurações de acessibilidade do seu dispositivo para ajustar as preferências.',
              style: theme.textTheme.bodyMedium?.copyWith(color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant),
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
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      color: colors?.background ?? theme.colorScheme.surface,
      child: Padding(
        padding: const EdgeInsets.all(18),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Conformidade', style: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold, color: colors?.onSurface ?? theme.colorScheme.onSurface)),
            const SizedBox(height: 10),
            Text(
              'O Nafila segue as diretrizes internacionais de acessibilidade digital (WCAG 2.1).',
              style: theme.textTheme.bodyMedium?.copyWith(color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildHelpCard(BuildContext context, ThemeData theme) {
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      color: colors?.background ?? theme.colorScheme.surface,
      child: Padding(
        padding: const EdgeInsets.all(18),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Precisa de Ajuda?', style: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold, color: colors?.onSurface ?? theme.colorScheme.onSurface)),
            const SizedBox(height: 10),
            Text(
              'Entre em contato com nosso suporte para dúvidas sobre acessibilidade.',
              style: theme.textTheme.bodyMedium?.copyWith(color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant),
            ),
            const SizedBox(height: 12),
            ElevatedButton.icon(
              icon: const Icon(Icons.help_outline),
              label: const Text('Falar com Suporte'),
              style: ElevatedButton.styleFrom(
                backgroundColor: colors?.primary ?? theme.colorScheme.primary,
                foregroundColor: colors?.onSurface ?? theme.colorScheme.onPrimary,
                textStyle: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              ),
              onPressed: () {
                Navigator.of(context).push(MaterialPageRoute(builder: (_) => AtendimentoScreen()));
              },
            ),
          ],
        ),
      ),
    );
  }
} 