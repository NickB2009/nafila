import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

class PersonalInfoScreen extends StatefulWidget {
  const PersonalInfoScreen({super.key});

  @override
  State<PersonalInfoScreen> createState() => _PersonalInfoScreenState();
}

class _PersonalInfoScreenState extends State<PersonalInfoScreen> with TickerProviderStateMixin {
  late TabController _tabController;
  late AnimationController _animationController;
  late Animation<double> _fadeAnimation;
  late Animation<Offset> _slideAnimation;
  String? modelPhotoUrl;
  
  // Dropdown values for cut notes
  String sides = 'Máquina 2';
  String fade = 'Médio';
  String top = 'Médio';
  String franja = 'Não';
  final List<String> sidesOptions = ['Máquina 1', 'Máquina 2', 'Máquina 3', 'Tesoura', 'Outro'];
  final List<String> fadeOptions = ['Baixo', 'Médio', 'Alto', 'Sem fade'];
  final List<String> topOptions = ['Curto', 'Médio', 'Longo', 'Tesoura', 'Máquina'];
  final List<String> franjaOptions = ['Sim', 'Não', 'Deixar crescer'];

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 800),
      vsync: this,
    );

    _fadeAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(parent: _animationController, curve: Curves.easeOut),
    );

    _slideAnimation = Tween<Offset>(
      begin: const Offset(0, 0.2),
      end: Offset.zero,
    ).animate(
      CurvedAnimation(parent: _animationController, curve: Curves.easeOut),
    );

    _animationController.forward();
  }

  @override
  void dispose() {
    _tabController.dispose();
    _animationController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    // Mock user data
    const String name = 'Rommel B';
    const String phone = '(11) 91234-5678';
    const String city = 'São Paulo';
    const String email = 'rommel@email.com';

    return Scaffold(
      backgroundColor: theme.colorScheme.primary,
      body: SafeArea(
        child: Column(
          children: [
            // Header
            Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  IconButton(
                    icon: Container(
                      padding: const EdgeInsets.all(8),
                      decoration: BoxDecoration(
                        color: theme.colorScheme.onPrimary.withOpacity(0.2),
                        shape: BoxShape.circle,
                      ),
                      child: Icon(
                        Icons.arrow_back,
                        color: theme.colorScheme.onPrimary,
                      ),
                    ),
                    onPressed: () => Navigator.of(context).pop(),
                  ),
                  const SizedBox(width: 16),
                  Text(
                    'Perfil',
                    style: theme.textTheme.headlineMedium?.copyWith(
                      color: theme.colorScheme.onPrimary,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            ),
            // Tab Bar
            Container(
              margin: const EdgeInsets.symmetric(horizontal: 16),
              decoration: BoxDecoration(
                color: theme.colorScheme.onPrimary.withOpacity(0.1),
                borderRadius: BorderRadius.circular(12),
              ),
              child: TabBar(
                controller: _tabController,
                indicator: BoxDecoration(
                  color: theme.colorScheme.onPrimary,
                  borderRadius: BorderRadius.circular(12),
                ),
                labelColor: theme.colorScheme.primary,
                unselectedLabelColor: theme.colorScheme.onPrimary,
                labelStyle: theme.textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
                unselectedLabelStyle: theme.textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.w500,
                ),
                indicatorSize: TabBarIndicatorSize.tab,
                dividerColor: Colors.transparent,
                tabs: const [
                  Tab(text: 'Informações'),
                  Tab(text: 'Cut Notes'),
                ],
              ),
            ),
            // Content
            Expanded(
              child: Container(
                margin: const EdgeInsets.only(top: 16),
                decoration: BoxDecoration(
                  color: theme.colorScheme.surface,
                  borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
                ),
                child: TabBarView(
                  controller: _tabController,
                  children: [
                    // Tab 1: Informações Pessoais
                    FadeTransition(
                      opacity: _fadeAnimation,
                      child: SlideTransition(
                        position: _slideAnimation,
                        child: SingleChildScrollView(
                          padding: const EdgeInsets.all(24),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              // Profile Header
                              Center(
                                child: Column(
                                  children: [
                                    Container(
                                      width: 100,
                                      height: 100,
                                      decoration: BoxDecoration(
                                        shape: BoxShape.circle,
                                        color: theme.colorScheme.primary.withOpacity(0.1),
                                        border: Border.all(
                                          color: theme.colorScheme.primary.withOpacity(0.2),
                                          width: 2,
                                        ),
                                      ),
                                      child: Icon(
                                        Icons.person,
                                        size: 48,
                                        color: theme.colorScheme.primary,
                                      ),
                                    ),
                                    const SizedBox(height: 16),
                                    Text(
                                      name,
                                      style: theme.textTheme.headlineSmall?.copyWith(
                                        fontWeight: FontWeight.bold,
                                      ),
                                    ),
                                    const SizedBox(height: 4),
                                    Text(
                                      'Membro desde 2024',
                                      style: theme.textTheme.bodyMedium?.copyWith(
                                        color: theme.colorScheme.onSurfaceVariant,
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                              const SizedBox(height: 32),
                              // Info Card
                              Card(
                                elevation: 0,
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(16),
                                  side: BorderSide(
                                    color: theme.colorScheme.outline.withOpacity(0.2),
                                  ),
                                ),
                                child: Padding(
                                  padding: const EdgeInsets.all(20),
                                  child: Column(
                                    children: [
                                      _buildInfoRow(theme, Icons.person_outline, 'Nome completo', name),
                                      const Divider(height: 28),
                                      _buildInfoRow(theme, Icons.phone, 'Telefone', phone),
                                      const Divider(height: 28),
                                      _buildInfoRow(theme, Icons.location_on_outlined, 'Cidade', city),
                                      const Divider(height: 28),
                                      _buildInfoRow(theme, Icons.email_outlined, 'Email (opcional)', email),
                                    ],
                                  ),
                                ),
                              ),
                              const SizedBox(height: 24),
                              // Edit Button
                              SizedBox(
                                width: double.infinity,
                                child: ElevatedButton.icon(
                                  onPressed: () {
                                    ScaffoldMessenger.of(context).showSnackBar(
                                      const SnackBar(content: Text('Edição em breve!'), duration: Duration(seconds: 1)),
                                    );
                                  },
                                  icon: const Icon(Icons.edit, size: 18),
                                  label: const Text('Editar Perfil'),
                                  style: ElevatedButton.styleFrom(
                                    backgroundColor: theme.colorScheme.primary,
                                    foregroundColor: theme.colorScheme.onPrimary,
                                    elevation: 0,
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
                      ),
                    ),
                    // Tab 2: Cut Notes
                    FadeTransition(
                      opacity: _fadeAnimation,
                      child: SlideTransition(
                        position: _slideAnimation,
                        child: SingleChildScrollView(
                          padding: const EdgeInsets.all(24),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              // Main Title
                              Center(
                                child: Text(
                                  'Preferências de Corte',
                                  style: theme.textTheme.headlineSmall?.copyWith(
                                    fontWeight: FontWeight.bold,
                                    color: theme.colorScheme.onSurface,
                                  ),
                                ),
                              ),
                              const SizedBox(height: 24),
                              // Modelos de Corte
                              Text(
                                'Modelos de Corte',
                                style: theme.textTheme.titleMedium?.copyWith(
                                  fontWeight: FontWeight.bold,
                                  color: theme.colorScheme.onSurface,
                                ),
                              ),
                              const SizedBox(height: 16),
                              SizedBox(
                                height: 140,
                                child: ListView(
                                  scrollDirection: Axis.horizontal,
                                  children: _buildCutTemplates(theme),
                                ),
                              ),
                              const SizedBox(height: 32),
                              // Foto de Referência
                              Text(
                                'Foto de Referência',
                                style: theme.textTheme.titleMedium?.copyWith(
                                  fontWeight: FontWeight.bold,
                                  color: theme.colorScheme.onSurface,
                                ),
                              ),
                              const SizedBox(height: 16),
                              Center(
                                child: Column(
                                  children: [
                                    GestureDetector(
                                      onTap: () {
                                        setState(() {
                                          modelPhotoUrl = modelPhotoUrl == null
                                            ? 'https://images.unsplash.com/photo-1511367461989-f85a21fda167?auto=format&fit=facearea&w=400&h=400'
                                            : null;
                                        });
                                      },
                                      child: Container(
                                        width: 160,
                                        height: 160,
                                        decoration: BoxDecoration(
                                          color: theme.colorScheme.surface,
                                          borderRadius: BorderRadius.circular(22),
                                          border: Border.all(
                                            color: theme.colorScheme.outline.withOpacity(0.3),
                                            width: 1.5,
                                          ),
                                          boxShadow: [
                                            BoxShadow(
                                              color: Colors.black.withOpacity(0.06),
                                              blurRadius: 8,
                                              offset: const Offset(0, 2),
                                            ),
                                          ],
                                        ),
                                        child: modelPhotoUrl == null
                                          ? Column(
                                              mainAxisAlignment: MainAxisAlignment.center,
                                              children: [
                                                Icon(
                                                  Icons.add_a_photo,
                                                  color: theme.colorScheme.primary,
                                                  size: 36,
                                                ),
                                                const SizedBox(height: 10),
                                                Text(
                                                  'Adicionar foto',
                                                  style: theme.textTheme.labelMedium?.copyWith(
                                                    color: theme.colorScheme.primary,
                                                    fontWeight: FontWeight.w500,
                                                  ),
                                                ),
                                              ],
                                            )
                                          : ClipRRect(
                                              borderRadius: BorderRadius.circular(20),
                                              child: Image.network(
                                                modelPhotoUrl!,
                                                width: 160,
                                                height: 160,
                                                fit: BoxFit.cover,
                                              ),
                                            ),
                                      ),
                                    ),
                                    if (modelPhotoUrl != null)
                                      TextButton(
                                        onPressed: () {
                                          setState(() {
                                            modelPhotoUrl = null;
                                          });
                                        },
                                        child: Text(
                                          'Remover',
                                          style: TextStyle(
                                            color: theme.colorScheme.error,
                                          ),
                                        ),
                                      ),
                                  ],
                                ),
                              ),
                              const SizedBox(height: 32),
                              // Preferências Personalizadas
                              Text(
                                'Preferências Personalizadas',
                                style: theme.textTheme.titleMedium?.copyWith(
                                  fontWeight: FontWeight.bold,
                                  color: theme.colorScheme.onSurface,
                                ),
                              ),
                              const SizedBox(height: 24),
                              _buildChipsSection(theme, 'Laterais', sides, sidesOptions, (v) => _onChipChanged('sides', v)),
                              const SizedBox(height: 24),
                              _buildChipsSection(theme, 'Fade', fade, fadeOptions, (v) => _onChipChanged('fade', v)),
                              const SizedBox(height: 24),
                              _buildChipsSection(theme, 'Topo', top, topOptions, (v) => _onChipChanged('top', v)),
                              const SizedBox(height: 24),
                              _buildChipsSection(theme, 'Franja', franja, franjaOptions, (v) => _onChipChanged('franja', v)),
                              const SizedBox(height: 32),
                              // Resumo
                              Center(
                                child: Card(
                                  elevation: 0,
                                  shape: RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(16),
                                    side: BorderSide(
                                      color: theme.colorScheme.outline.withOpacity(0.2),
                                    ),
                                  ),
                                  child: Padding(
                                    padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 16),
                                    child: Row(
                                      mainAxisSize: MainAxisSize.min,
                                      children: [
                                        Icon(
                                          Icons.content_cut,
                                          color: theme.colorScheme.primary,
                                          size: 22,
                                        ),
                                        const SizedBox(width: 10),
                                        Text(
                                          'Laterais: $sides  •  Fade: $fade  •  Topo: $top  •  Franja: $franja',
                                          style: theme.textTheme.bodyLarge?.copyWith(
                                            fontWeight: FontWeight.w600,
                                          ),
                                        ),
                                        IconButton(
                                          icon: const Icon(Icons.copy_rounded, size: 20),
                                          color: theme.colorScheme.primary,
                                          onPressed: () {
                                            final summary = 'Laterais: $sides  •  Fade: $fade  •  Topo: $top  •  Franja: $franja';
                                            ScaffoldMessenger.of(context).showSnackBar(
                                              const SnackBar(content: Text('Copiado!'), duration: Duration(seconds: 1)),
                                            );
                                          },
                                        ),
                                      ],
                                    ),
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildInfoRow(ThemeData theme, IconData icon, String label, String value) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
            color: theme.colorScheme.primary.withOpacity(0.1),
            borderRadius: BorderRadius.circular(8),
          ),
          child: Icon(
            icon,
            color: theme.colorScheme.primary,
            size: 20,
          ),
        ),
        const SizedBox(width: 16),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                label,
                style: theme.textTheme.labelMedium?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                  fontWeight: FontWeight.w600,
                ),
              ),
              const SizedBox(height: 4),
              Text(
                value,
                style: theme.textTheme.bodyLarge?.copyWith(
                  color: theme.colorScheme.onSurface,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildChipsSection(ThemeData theme, String label, String selected, List<String> options, ValueChanged<String> onChanged) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          label,
          style: theme.textTheme.labelMedium?.copyWith(
            color: theme.colorScheme.onSurfaceVariant,
            fontWeight: FontWeight.w600,
          ),
        ),
        const SizedBox(height: 12),
        Wrap(
          spacing: 10,
          runSpacing: 10,
          children: options.map((option) {
            final isSelected = selected == option;
            return ChoiceChip(
              label: Text(option),
              selected: isSelected,
              onSelected: (_) => onChanged(option),
              selectedColor: theme.colorScheme.primary,
              labelStyle: theme.textTheme.bodyMedium?.copyWith(
                color: isSelected ? theme.colorScheme.onPrimary : theme.colorScheme.onSurface,
                fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
              ),
              backgroundColor: theme.colorScheme.surface,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(10),
                side: BorderSide(
                  color: isSelected
                      ? theme.colorScheme.primary
                      : theme.colorScheme.outline.withOpacity(0.2),
                ),
              ),
              elevation: 0,
            );
          }).toList(),
        ),
      ],
    );
  }

  // --- Cut Templates ---
  int selectedTemplate = 0;
  final List<Map<String, dynamic>> cutTemplates = [
    {
      'name': 'Personalizado',
      'img': null,
      'isCustom': true,
    },
    {
      'name': 'Degradê',
      'img': null,
      'sides': 'Máquina 2',
      'fade': 'Médio',
      'top': 'Médio',
      'franja': 'Não',
    },
    {
      'name': 'Social',
      'img': null,
      'sides': 'Tesoura',
      'fade': 'Sem fade',
      'top': 'Médio',
      'franja': 'Não',
    },
    {
      'name': 'Militar',
      'img': null,
      'sides': 'Máquina 1',
      'fade': 'Baixo',
      'top': 'Curto',
      'franja': 'Não',
    },
    {
      'name': 'Topete',
      'img': null,
      'sides': 'Máquina 2',
      'fade': 'Alto',
      'top': 'Longo',
      'franja': 'Sim',
    },
    {
      'name': 'Corte Americano',
      'img': null,
      'sides': 'Máquina 3',
      'fade': 'Médio',
      'top': 'Médio',
      'franja': 'Deixar crescer',
    },
  ];

  List<Widget> _buildCutTemplates(ThemeData theme) {
    return List.generate(cutTemplates.length, (i) {
      final t = cutTemplates[i];
      final isSelected = selectedTemplate == i;
      return GestureDetector(
        onTap: () {
          setState(() {
            selectedTemplate = i;
            if (t['isCustom'] == true) {
              // Do not change chips, just select custom
            } else {
              sides = t['sides'];
              fade = t['fade'];
              top = t['top'];
              franja = t['franja'];
            }
          });
        },
        child: Container(
          width: 110,
          margin: const EdgeInsets.only(right: 16),
          decoration: BoxDecoration(
            color: isSelected ? theme.colorScheme.primary.withOpacity(0.1) : theme.colorScheme.surface,
            borderRadius: BorderRadius.circular(16),
            border: Border.all(
              color: isSelected ? theme.colorScheme.primary : theme.colorScheme.outline.withOpacity(0.2),
              width: isSelected ? 2 : 1,
            ),
            boxShadow: [
              if (isSelected)
                BoxShadow(
                  color: theme.colorScheme.primary.withOpacity(0.08),
                  blurRadius: 8,
                  offset: const Offset(0, 2),
                ),
            ],
          ),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              if (t['isCustom'] == true)
                Icon(Icons.edit, size: 48, color: theme.colorScheme.primary)
              else
                Container(
                  width: 70,
                  height: 70,
                  decoration: BoxDecoration(
                    color: theme.colorScheme.primary.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Icon(
                    Icons.content_cut,
                    color: theme.colorScheme.primary,
                    size: 32,
                  ),
                ),
              const SizedBox(height: 10),
              Text(
                t['name'],
                style: theme.textTheme.bodyMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: isSelected ? theme.colorScheme.primary : theme.colorScheme.onSurface,
                ),
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
      );
    });
  }

  void _onChipChanged(String field, String value) {
    setState(() {
      if (field == 'sides') sides = value;
      if (field == 'fade') fade = value;
      if (field == 'top') top = value;
      if (field == 'franja') franja = value;
      // If any chip is changed manually, select 'Custom'
      selectedTemplate = 0;
    });
  }
} 