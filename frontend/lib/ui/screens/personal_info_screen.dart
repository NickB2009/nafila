import 'package:flutter/material.dart';
import '../../utils/brazilian_names_generator.dart';
import 'package:flutter/services.dart';
import '../widgets/bottom_nav_bar.dart';
import '../../models/salon.dart';

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
    final salon = CheckInState.checkedInSalon;
    final colors = salon?.colors;
    // Mock user data
    final String name = BrazilianNamesGenerator.generateNameWithInitial();
    const String phone = '(11) 91234-5678';
    const String city = 'São Paulo';
    final String email = BrazilianNamesGenerator.generateEmail();

    return Scaffold(
      backgroundColor: colors?.background ?? theme.colorScheme.surface,
      body: SafeArea(
        child: Column(
          children: [
            // Clean Header
            Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                  colors: [
                    colors?.primary ?? theme.colorScheme.primary,
                    (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.8),
                  ],
                ),
              ),
              child: Column(
                children: [
                  // Back button and title
                  Row(
                    children: [
                      Container(
                        decoration: BoxDecoration(
                          color: Colors.white.withOpacity(0.2),
                          borderRadius: BorderRadius.circular(12),
                        ),
                        child: IconButton(
                          icon: const Icon(Icons.arrow_back, color: Colors.white),
                          onPressed: () => Navigator.of(context).pop(),
                        ),
                      ),
                      const SizedBox(width: 16),
                      Expanded(
                        child: Text(
                          'Meu Perfil',
                          style: theme.textTheme.headlineSmall?.copyWith(
                            color: Colors.white,
                            fontWeight: FontWeight.w600,
                          ),
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 24),
                  // Profile avatar and name
                  Center(
                    child: Column(
                      children: [
                        Container(
                          width: 80,
                          height: 80,
                          decoration: BoxDecoration(
                            shape: BoxShape.circle,
                            color: Colors.white.withOpacity(0.2),
                            border: Border.all(color: Colors.white.withOpacity(0.3), width: 2),
                          ),
                          child: Icon(
                            Icons.person,
                            size: 36,
                            color: Colors.white,
                          ),
                        ),
                        const SizedBox(height: 12),
                        Text(
                          name,
                          style: theme.textTheme.titleLarge?.copyWith(
                            color: Colors.white,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                        Text(
                          'Membro desde 2024',
                          style: theme.textTheme.bodyMedium?.copyWith(
                            color: Colors.white.withOpacity(0.8),
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
            // Minimal Tab Bar
            Container(
              margin: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: colors?.background ?? theme.colorScheme.surface,
                borderRadius: BorderRadius.circular(16),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.05),
                    blurRadius: 10,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: TabBar(
                controller: _tabController,
                indicator: BoxDecoration(
                  color: colors?.primary ?? theme.colorScheme.primary,
                  borderRadius: BorderRadius.circular(16),
                ),
                labelColor: Colors.white,
                unselectedLabelColor: (colors?.secondary ?? theme.colorScheme.onSurfaceVariant),
                labelStyle: theme.textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.w600,
                ),
                unselectedLabelStyle: theme.textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.w500,
                ),
                indicatorSize: TabBarIndicatorSize.tab,
                dividerColor: Colors.transparent,
                tabs: const [
                  Tab(text: 'Informações'),
                  Tab(text: 'Preferências'),
                ],
              ),
            ),
            // Content
            Expanded(
              child: TabBarView(
                controller: _tabController,
                children: [
                  // Tab 1: Informações Pessoais
                  FadeTransition(
                    opacity: _fadeAnimation,
                    child: SlideTransition(
                      position: _slideAnimation,
                      child: SingleChildScrollView(
                        padding: const EdgeInsets.symmetric(horizontal: 20),
                        child: Column(
                          children: [
                            // Info Cards
                            _buildInfoCard(
                              theme,
                              'Informações Pessoais',
                              [
                                _buildInfoRow(theme, Icons.person_outline, 'Nome completo', name),
                                _buildInfoRow(theme, Icons.phone, 'Telefone', phone),
                                _buildInfoRow(theme, Icons.location_on_outlined, 'Cidade', city),
                                _buildInfoRow(theme, Icons.email_outlined, 'Email', email),
                              ],
                            ),
                            const SizedBox(height: 20),
                            // Edit Button
                            SizedBox(
                              width: double.infinity,
                              child: ElevatedButton.icon(
                                onPressed: () async {
                                  final result = await showModalBottomSheet<Map<String, String>>(
                                    context: context,
                                    isScrollControlled: true,
                                    shape: const RoundedRectangleBorder(
                                      borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
                                    ),
                                    builder: (context) {
                                      final nameController = TextEditingController(text: name);
                                      final phoneController = TextEditingController(text: phone);
                                      final cityController = TextEditingController(text: city);
                                      final emailController = TextEditingController(text: email);
                                      return Padding(
                                        padding: EdgeInsets.only(
                                          left: 24, right: 24,
                                          top: 24,
                                          bottom: MediaQuery.of(context).viewInsets.bottom + 24,
                                        ),
                                        child: Column(
                                          mainAxisSize: MainAxisSize.min,
                                          crossAxisAlignment: CrossAxisAlignment.start,
                                          children: [
                                            Row(
                                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                              children: [
                                                Text('Editar Informações', style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
                                                IconButton(
                                                  icon: const Icon(Icons.close),
                                                  onPressed: () => Navigator.of(context).pop(),
                                                ),
                                              ],
                                            ),
                                            const SizedBox(height: 16),
                                            TextField(
                                              controller: nameController,
                                              decoration: const InputDecoration(labelText: 'Nome completo'),
                                            ),
                                            const SizedBox(height: 12),
                                            TextField(
                                              controller: phoneController,
                                              decoration: const InputDecoration(labelText: 'Telefone'),
                                              keyboardType: TextInputType.phone,
                                            ),
                                            const SizedBox(height: 12),
                                            TextField(
                                              controller: cityController,
                                              decoration: const InputDecoration(labelText: 'Cidade'),
                                            ),
                                            const SizedBox(height: 12),
                                            TextField(
                                              controller: emailController,
                                              decoration: const InputDecoration(labelText: 'Email'),
                                              keyboardType: TextInputType.emailAddress,
                                            ),
                                            const SizedBox(height: 24),
                                            SizedBox(
                                              width: double.infinity,
                                              child: ElevatedButton(
                                                style: ElevatedButton.styleFrom(
                                                  backgroundColor: theme.colorScheme.primary,
                                                  foregroundColor: Colors.white,
                                                  shape: RoundedRectangleBorder(
                                                    borderRadius: BorderRadius.circular(12),
                                                  ),
                                                ),
                                                onPressed: () {
                                                  Navigator.of(context).pop({
                                                    'name': nameController.text,
                                                    'phone': phoneController.text,
                                                    'city': cityController.text,
                                                    'email': emailController.text,
                                                  });
                                                },
                                                child: const Text('Salvar'),
                                              ),
                                            ),
                                          ],
                                        ),
                                      );
                                    },
                                  );
                                  if (result != null) {
                                    ScaffoldMessenger.of(context).showSnackBar(
                                      const SnackBar(
                                        content: Text('Informações atualizadas!'),
                                        duration: Duration(seconds: 2),
                                      ),
                                    );
                                    // In a real app, update state/provider here
                                  }
                                },
                                icon: const Icon(Icons.edit_outlined, size: 18),
                                label: const Text('Editar Perfil'),
                                style: ElevatedButton.styleFrom(
                                  backgroundColor: theme.colorScheme.primary,
                                  foregroundColor: Colors.white,
                                  elevation: 0,
                                  padding: const EdgeInsets.symmetric(vertical: 16),
                                  shape: RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(16),
                                  ),
                                ),
                              ),
                            ),
                            const SizedBox(height: 20),
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
                        padding: const EdgeInsets.symmetric(horizontal: 20),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            // Cut Templates
                            Text(
                              'Modelos de Corte',
                              style: theme.textTheme.titleLarge?.copyWith(
                                fontWeight: FontWeight.w600,
                                color: theme.colorScheme.onSurface,
                              ),
                            ),
                            const SizedBox(height: 16),
                            SizedBox(
                              height: 120,
                              child: ListView(
                                scrollDirection: Axis.horizontal,
                                children: _buildCutTemplates(theme),
                              ),
                            ),
                            const SizedBox(height: 32),
                            // Reference Photo
                            Text(
                              'Foto de Referência',
                              style: theme.textTheme.titleLarge?.copyWith(
                                fontWeight: FontWeight.w600,
                                color: theme.colorScheme.onSurface,
                              ),
                            ),
                            const SizedBox(height: 16),
                            Center(
                              child: GestureDetector(
                                onTap: () {
                                  setState(() {
                                    modelPhotoUrl = modelPhotoUrl == null
                                      ? 'https://images.unsplash.com/photo-1511367461989-f85a21fda167?auto=format&fit=facearea&w=400&h=400'
                                      : null;
                                  });
                                },
                                child: Container(
                                  width: 140,
                                  height: 140,
                                  decoration: BoxDecoration(
                                    color: theme.colorScheme.surface,
                                    borderRadius: BorderRadius.circular(20),
                                    border: Border.all(
                                      color: theme.colorScheme.outline.withOpacity(0.2),
                                      width: 1,
                                    ),
                                    boxShadow: [
                                      BoxShadow(
                                        color: Colors.black.withOpacity(0.05),
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
                                            Icons.add_a_photo_outlined,
                                            color: theme.colorScheme.primary,
                                            size: 32,
                                          ),
                                          const SizedBox(height: 8),
                                          Text(
                                            'Adicionar foto',
                                            style: theme.textTheme.bodySmall?.copyWith(
                                              color: theme.colorScheme.primary,
                                              fontWeight: FontWeight.w500,
                                            ),
                                          ),
                                        ],
                                      )
                                    : ClipRRect(
                                        borderRadius: BorderRadius.circular(19),
                                        child: Image.network(
                                          modelPhotoUrl!,
                                          width: 140,
                                          height: 140,
                                          fit: BoxFit.cover,
                                        ),
                                      ),
                                ),
                              ),
                            ),
                            if (modelPhotoUrl != null) ...[
                              const SizedBox(height: 12),
                              Center(
                                child: TextButton.icon(
                                  onPressed: () {
                                    setState(() {
                                      modelPhotoUrl = null;
                                    });
                                  },
                                  icon: const Icon(Icons.delete_outline, size: 16),
                                  label: const Text('Remover'),
                                  style: TextButton.styleFrom(
                                    foregroundColor: theme.colorScheme.error,
                                  ),
                                ),
                              ),
                            ],
                            const SizedBox(height: 32),
                            // Preferences
                            Text(
                              'Preferências de Corte',
                              style: theme.textTheme.titleLarge?.copyWith(
                                fontWeight: FontWeight.w600,
                                color: theme.colorScheme.onSurface,
                              ),
                            ),
                            const SizedBox(height: 20),
                            _buildPreferencesSection(theme),
                            const SizedBox(height: 32),
                            // Summary Card
                            _buildSummaryCard(theme),
                            const SizedBox(height: 20),
                          ],
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildInfoCard(ThemeData theme, String title, List<Widget> children) {
    final colors = CheckInState.checkedInSalon?.colors;
    return Container(
      width: double.infinity,
      decoration: BoxDecoration(
        color: colors?.background ?? theme.colorScheme.surface,
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              title,
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.w600,
                color: (colors?.primary ?? theme.colorScheme.onSurface),
              ),
            ),
            const SizedBox(height: 16),
            ...children,
          ],
        ),
      ),
    );
  }

  Widget _buildInfoRow(ThemeData theme, IconData icon, String label, String value) {
    final colors = CheckInState.checkedInSalon?.colors;
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.1),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Icon(
              icon,
              color: (colors?.primary ?? theme.colorScheme.primary),
              size: 18,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  label,
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: (colors?.secondary ?? theme.colorScheme.onSurfaceVariant),
                    fontWeight: FontWeight.w500,
                  ),
                ),
                Text(
                  value,
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: (colors?.primary ?? theme.colorScheme.onSurface),
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildPreferencesSection(ThemeData theme) {
    final colors = CheckInState.checkedInSalon?.colors;
    return Column(
      children: [
        _buildPreferenceRow(theme, 'Laterais', sides, sidesOptions, (v) => _onChipChanged('sides', v)),
        const SizedBox(height: 20),
        _buildPreferenceRow(theme, 'Fade', fade, fadeOptions, (v) => _onChipChanged('fade', v)),
        const SizedBox(height: 20),
        _buildPreferenceRow(theme, 'Topo', top, topOptions, (v) => _onChipChanged('top', v)),
        const SizedBox(height: 20),
        _buildPreferenceRow(theme, 'Franja', franja, franjaOptions, (v) => _onChipChanged('franja', v)),
      ],
    );
  }

  Widget _buildPreferenceRow(ThemeData theme, String label, String selected, List<String> options, ValueChanged<String> onChanged) {
    final colors = CheckInState.checkedInSalon?.colors;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          label,
          style: theme.textTheme.titleSmall?.copyWith(
            color: (colors?.secondary ?? theme.colorScheme.onSurfaceVariant),
            fontWeight: FontWeight.w600,
          ),
        ),
        const SizedBox(height: 12),
        Wrap(
          spacing: 8,
          runSpacing: 8,
          children: options.map((option) {
            final isSelected = selected == option;
            return GestureDetector(
              onTap: () => onChanged(option),
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                decoration: BoxDecoration(
                  color: isSelected ? (colors?.primary ?? theme.colorScheme.primary) : (colors?.background ?? theme.colorScheme.surface),
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(
                    color: isSelected ? (colors?.primary ?? theme.colorScheme.primary) : (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.2),
                    width: 1,
                  ),
                ),
                child: Text(
                  option,
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: isSelected ? Colors.white : (colors?.onSurface ?? theme.colorScheme.onSurface),
                    fontWeight: isSelected ? FontWeight.w600 : FontWeight.w500,
                  ),
                ),
              ),
            );
          }).toList(),
        ),
      ],
    );
  }

  Widget _buildSummaryCard(ThemeData theme) {
    final colors = CheckInState.checkedInSalon?.colors;
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.05),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.1),
          width: 1,
        ),
      ),
      child: Row(
        children: [
          Icon(
            Icons.content_cut,
            color: (colors?.primary ?? theme.colorScheme.primary),
            size: 24,
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              'Laterais: $sides  •  Fade: $fade  •  Topo: $top  •  Franja: $franja',
              style: theme.textTheme.bodyMedium?.copyWith(
                fontWeight: FontWeight.w600,
                color: (colors?.primary ?? theme.colorScheme.onSurface),
              ),
            ),
          ),
          IconButton(
            icon: const Icon(Icons.copy_outlined, size: 20),
            color: (colors?.primary ?? theme.colorScheme.primary),
            onPressed: () {
              final summary = 'Laterais: $sides  •  Fade: $fade  •  Topo: $top  •  Franja: $franja';
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                  content: Text('Copiado!'),
                  duration: Duration(seconds: 1),
                ),
              );
            },
          ),
        ],
      ),
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
    final colors = CheckInState.checkedInSalon?.colors;
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
          width: 100,
          margin: const EdgeInsets.only(right: 12),
          decoration: BoxDecoration(
            color: isSelected ? (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.1) : (colors?.background ?? theme.colorScheme.surface),
            borderRadius: BorderRadius.circular(16),
            border: Border.all(
              color: isSelected
                  ? (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.2)
                  : theme.colorScheme.outline.withOpacity(0.2),
              width: isSelected ? 2 : 1,
            ),
            boxShadow: [
              if (isSelected)
                BoxShadow(
                  color: (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.1),
                  blurRadius: 8,
                  offset: const Offset(0, 2),
                ),
            ],
          ),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              if (t['isCustom'] == true)
                Icon(Icons.edit_outlined, size: 32, color: (colors?.primary ?? theme.colorScheme.primary))
              else
                Container(
                  width: 50,
                  height: 50,
                  decoration: BoxDecoration(
                    color: (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.1),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Icon(
                    Icons.content_cut,
                    color: (colors?.primary ?? theme.colorScheme.primary),
                    size: 24,
                  ),
                ),
              const SizedBox(height: 8),
              Text(
                t['name'],
                style: theme.textTheme.bodySmall?.copyWith(
                  fontWeight: FontWeight.w600,
                  color: isSelected ? (colors?.primary ?? theme.colorScheme.primary) : theme.colorScheme.onSurface,
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