import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

class PersonalInfoScreen extends StatefulWidget {
  const PersonalInfoScreen({super.key});

  @override
  State<PersonalInfoScreen> createState() => _PersonalInfoScreenState();
}

class _PersonalInfoScreenState extends State<PersonalInfoScreen> with SingleTickerProviderStateMixin {
  late TabController _tabController;
  String? modelPhotoUrl; // Mock: could be a network or asset image
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
  }

  @override
  void dispose() {
    _tabController.dispose();
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
      appBar: AppBar(
        backgroundColor: theme.colorScheme.primary,
        elevation: 0,
        iconTheme: IconThemeData(color: theme.colorScheme.onPrimary),
        title: Text(
          'Perfil',
          style: theme.textTheme.titleLarge?.copyWith(
            color: theme.colorScheme.onPrimary,
            fontWeight: FontWeight.bold,
          ),
        ),
        bottom: TabBar(
          controller: _tabController,
          indicatorColor: theme.colorScheme.onPrimary,
          labelColor: theme.colorScheme.onPrimary,
          unselectedLabelColor: theme.colorScheme.onPrimary.withOpacity(0.6),
          tabs: const [
            Tab(text: 'Informações'),
            Tab(text: 'Cut Notes'),
          ],
        ),
      ),
      backgroundColor: theme.colorScheme.primary,
      body: SafeArea(
        child: Container(
          decoration: BoxDecoration(
            color: theme.colorScheme.surface,
            borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
          ),
          child: TabBarView(
            controller: _tabController,
            children: [
              // Tab 1: Informações Pessoais
              SingleChildScrollView(
                padding: const EdgeInsets.all(24),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        CircleAvatar(
                          radius: 28,
                          backgroundColor: theme.colorScheme.primary.withOpacity(0.13),
                          child: Icon(Icons.person, size: 32, color: theme.colorScheme.primary),
                        ),
                        const SizedBox(width: 16),
                        Text(
                          'Seus Dados',
                          style: theme.textTheme.titleLarge?.copyWith(
                            fontWeight: FontWeight.bold,
                            color: theme.colorScheme.onSurface,
                          ),
                        ),
                        const Spacer(),
                        ElevatedButton.icon(
                          onPressed: () {
                            ScaffoldMessenger.of(context).showSnackBar(
                              const SnackBar(content: Text('Edição em breve!'), duration: Duration(seconds: 1)),
                            );
                          },
                          icon: const Icon(Icons.edit, size: 18),
                          label: const Text('Editar'),
                          style: ElevatedButton.styleFrom(
                            backgroundColor: theme.colorScheme.primary,
                            foregroundColor: theme.colorScheme.onPrimary,
                            elevation: 0,
                            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                            textStyle: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold),
                            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 24),
                    Card(
                      elevation: 2,
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
                      color: theme.colorScheme.surfaceContainerHighest,
                      child: Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
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
                  ],
                ),
              ),
              // Tab 2: Cut Notes
              SingleChildScrollView(
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
                    const SizedBox(height: 18),
                    // Modelos de Corte
                    Text(
                      'Modelos de Corte',
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                        color: theme.colorScheme.onSurface,
                      ),
                    ),
                    const SizedBox(height: 10),
                    SizedBox(
                      height: 140,
                      child: ListView(
                        scrollDirection: Axis.horizontal,
                        children: _buildCutTemplates(theme),
                      ),
                    ),
                    const SizedBox(height: 24),
                    const Divider(height: 32),
                    // Foto de Referência
                    Text(
                      'Foto de Referência',
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                        color: theme.colorScheme.onSurface,
                      ),
                    ),
                    const SizedBox(height: 12),
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
                                border: Border.all(color: theme.colorScheme.outline.withOpacity(0.3), width: 1.5),
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
                                      Icon(Icons.add_a_photo, color: theme.colorScheme.onSurfaceVariant, size: 36),
                                      const SizedBox(height: 10),
                                      Text('Adicionar foto', style: theme.textTheme.labelMedium?.copyWith(fontWeight: FontWeight.w500)),
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
                              child: const Text('Remover'),
                            ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 24),
                    const Divider(height: 32),
                    // Preferências Personalizadas
                    Text(
                      'Preferências Personalizadas',
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                        color: theme.colorScheme.onSurface,
                      ),
                    ),
                    const SizedBox(height: 18),
                    _buildChipsSection(theme, 'Laterais', sides, sidesOptions, (v) => _onChipChanged('sides', v)),
                    const SizedBox(height: 16),
                    _buildChipsSection(theme, 'Fade', fade, fadeOptions, (v) => _onChipChanged('fade', v)),
                    const SizedBox(height: 16),
                    _buildChipsSection(theme, 'Topo', top, topOptions, (v) => _onChipChanged('top', v)),
                    const SizedBox(height: 16),
                    _buildChipsSection(theme, 'Franja', franja, franjaOptions, (v) => _onChipChanged('franja', v)),
                    const SizedBox(height: 32),
                    // Resumo
                    Center(
                      child: Card(
                        elevation: 1,
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                        color: theme.colorScheme.surfaceContainerHighest,
                        child: Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 16),
                          child: Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              Icon(Icons.content_cut, color: theme.colorScheme.primary, size: 22),
                              const SizedBox(width: 10),
                              Text(
                                'Laterais: $sides  •  Fade: $fade  •  Topo: $top  •  Franja: $franja',
                                style: theme.textTheme.bodyLarge?.copyWith(fontWeight: FontWeight.w600),
                              ),
                              IconButton(
                                icon: const Icon(Icons.copy_rounded, size: 20),
                                color: theme.colorScheme.onSurfaceVariant,
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
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildInfoRow(ThemeData theme, IconData icon, String label, String value) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, color: theme.colorScheme.primary, size: 24),
        const SizedBox(width: 14),
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
              const SizedBox(height: 2),
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
        const SizedBox(height: 8),
        Wrap(
          spacing: 10,
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
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              elevation: isSelected ? 2 : 0,
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
      'img': null, // Will use an icon
      'isCustom': true,
    },
    {
      'name': 'Degradê',
      'img': 'https://images.unsplash.com/photo-1517841905240-472988babdf9?auto=format&fit=facearea&w=120&h=120',
      'sides': 'Máquina 2',
      'fade': 'Médio',
      'top': 'Médio',
      'franja': 'Não',
    },
    {
      'name': 'Social',
      'img': 'https://images.unsplash.com/photo-1529626455594-4ff0802cfb7e?auto=format&fit=facearea&w=120&h=120',
      'sides': 'Tesoura',
      'fade': 'Sem fade',
      'top': 'Médio',
      'franja': 'Não',
    },
    {
      'name': 'Militar',
      'img': 'https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?auto=format&fit=facearea&w=120&h=120',
      'sides': 'Máquina 1',
      'fade': 'Baixo',
      'top': 'Curto',
      'franja': 'Não',
    },
    {
      'name': 'Topete',
      'img': 'https://images.unsplash.com/photo-1511367461989-f85a21fda167?auto=format&fit=facearea&w=120&h=120',
      'sides': 'Máquina 2',
      'fade': 'Alto',
      'top': 'Longo',
      'franja': 'Sim',
    },
    {
      'name': 'Corte Americano',
      'img': 'https://images.unsplash.com/photo-1464983953574-0892a716854b?auto=format&fit=facearea&w=120&h=120',
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
            color: isSelected ? theme.colorScheme.primary.withOpacity(0.13) : theme.colorScheme.surface,
            borderRadius: BorderRadius.circular(16),
            border: Border.all(
              color: isSelected ? theme.colorScheme.primary : theme.colorScheme.outline.withOpacity(0.18),
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
                ClipRRect(
                  borderRadius: BorderRadius.circular(12),
                  child: Image.network(
                    t['img'],
                    width: 70,
                    height: 70,
                    fit: BoxFit.cover,
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

  // --- Avatar Hair Logic ---
  Color _getSidesColor(String sides) {
    switch (sides) {
      case 'Máquina 1': return Colors.brown[700]!;
      case 'Máquina 2': return Colors.brown[500]!;
      case 'Máquina 3': return Colors.brown[300]!;
      case 'Tesoura': return Colors.brown[400]!;
      default: return Colors.brown[200]!;
    }
  }

  Gradient _getFadeGradient(String fade) {
    switch (fade) {
      case 'Baixo':
        return LinearGradient(colors: [Colors.brown[700]!, Colors.brown[200]!], begin: Alignment.bottomCenter, end: Alignment.topCenter);
      case 'Médio':
        return LinearGradient(colors: [Colors.brown[700]!, Colors.brown[100]!], begin: Alignment.bottomCenter, end: Alignment.topCenter);
      case 'Alto':
        return LinearGradient(colors: [Colors.brown[700]!, Colors.white], begin: Alignment.bottomCenter, end: Alignment.topCenter);
      default:
        return const LinearGradient(colors: [Colors.transparent, Colors.transparent]);
    }
  }

  double _getTopHeight(String top) {
    switch (top) {
      case 'Curto': return 28;
      case 'Médio': return 38;
      case 'Longo': return 54;
      case 'Tesoura': return 44;
      case 'Máquina': return 24;
      default: return 36;
    }
  }

  Color _getTopColor(String top) {
    switch (top) {
      case 'Curto': return Colors.brown[600]!;
      case 'Médio': return Colors.brown[500]!;
      case 'Longo': return Colors.brown[400]!;
      case 'Tesoura': return Colors.brown[300]!;
      case 'Máquina': return Colors.brown[700]!;
      default: return Colors.brown[400]!;
    }
  }

  Color _getFranjaColor(String franja) {
    switch (franja) {
      case 'Sim': return Colors.brown[300]!;
      case 'Deixar crescer': return Colors.brown[200]!;
      default: return Colors.transparent;
    }
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