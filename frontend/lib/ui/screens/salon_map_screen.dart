import 'dart:math';
import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:flutter_map_cancellable_tile_provider/flutter_map_cancellable_tile_provider.dart';
import '../../models/salon.dart';
import 'salon_details_screen.dart';
import '../../models/salon_service.dart';
import '../../models/salon_contact.dart';
import '../../models/salon_hours.dart';
import '../../models/salon_review.dart';
import '../widgets/bottom_nav_bar.dart';
import '../../state/check_in_state.dart';

SalonColors _randomSalonColors(int seed) {
  final palettes = [
    SalonColors(primary: Colors.redAccent, secondary: Colors.orange, background: Colors.red.shade50),
    SalonColors(primary: Colors.blueAccent, secondary: Colors.cyan, background: Colors.blue.shade50),
    SalonColors(primary: Colors.green, secondary: Colors.teal, background: Colors.green.shade50),
    SalonColors(primary: Colors.purple, secondary: Colors.pinkAccent, background: Colors.purple.shade50),
    SalonColors(primary: Colors.amber, secondary: Colors.deepOrange, background: Colors.amber.shade50),
    SalonColors(primary: Colors.indigo, secondary: Colors.lime, background: Colors.indigo.shade50),
  ];
  return palettes[seed % palettes.length];
}

Color waitTimeToColor(int waitTime) {
  // Clamp waitTime between 0 and 180
  final clamped = waitTime.clamp(0, 180);
  if (clamped <= 90) {
    // 0-90 min: green to yellow
    final t = clamped / 90.0;
    return Color.lerp(Colors.greenAccent.shade400, Colors.yellow.shade700, t)!;
  } else {
    // 90-180 min: yellow to red
    final t = (clamped - 90) / 90.0;
    return Color.lerp(Colors.yellow.shade700, Colors.red.shade900, t)!;
  }
}

class SalonMapScreen extends StatefulWidget {
  const SalonMapScreen({super.key});

  @override
  State<SalonMapScreen> createState() => _SalonMapScreenState();
}

class _SalonMapScreenState extends State<SalonMapScreen> {
  final MapController _mapController = MapController();
  Salon? _selectedSalon;
  
  // Mock salon locations
  final List<SalonLocation> _salonLocations = [
    SalonLocation(
      salon: Salon(
        name: 'Market at Mirada',
        address: '30921 Mirada Blvd, San Antonio, FL',
        waitTime: 24,
        distance: 10.9,
        isOpen: true,
        closingTime: '6 PM',
        isFavorite: true,
        queueLength: 5,
        colors: _randomSalonColors(0),
      ),
      position: const LatLng(28.3372, -82.2637),
    ),
    SalonLocation(
      salon: Salon(
        name: 'Cortez Commons',
        address: '123 Cortez Ave, San Antonio, FL',
        waitTime: 8,
        distance: 5.2,
        isOpen: true,
        closingTime: '8 PM',
        isFavorite: true,
        queueLength: 2,
        colors: _randomSalonColors(1),
      ),
      position: const LatLng(28.3422, -82.2587),
    ),
    SalonLocation(
      salon: Salon(
        name: 'Westshore Plaza',
        address: '456 Westshore Blvd, San Antonio, FL',
        waitTime: 15,
        distance: 7.8,
        isOpen: true,
        closingTime: '9 PM',
        isFavorite: false,
        queueLength: 3,
        colors: _randomSalonColors(2),
      ),
      position: const LatLng(28.3322, -82.2687),
    ),
    SalonLocation(
      salon: Salon(
        name: 'Tampa Bay Center',
        address: '789 Bay Center Dr, San Antonio, FL',
        waitTime: 32,
        distance: 12.1,
        isOpen: false,
        closingTime: '7 PM',
        isFavorite: false,
        queueLength: 8,
        colors: _randomSalonColors(3),
      ),
      position: const LatLng(28.3272, -82.2737),
    ),
  ];

  // Filter state
  bool _filterOpenNow = false;
  bool _filterShortLine = false;
  bool _filterShortWait = false;

  List<SalonLocation> get _filteredSalons {
    return _salonLocations.where((location) {
      final salon = location.salon;
      if (_filterOpenNow && !salon.isOpen) return false;
      if (_filterShortLine && salon.queueLength > 3) return false;
      if (_filterShortWait && salon.waitTime > 15) return false;
      return true;
    }).toList();
  }

  void _showFilterModal() async {
    final result = await showModalBottomSheet<Map<String, bool>>(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setModalState) {
            return Padding(
              padding: const EdgeInsets.fromLTRB(24, 24, 24, 24),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text('Filtros', style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold)),
                      IconButton(
                        icon: const Icon(Icons.close),
                        onPressed: () => Navigator.of(context).pop(),
                      ),
                    ],
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'Refine sua busca de salões próximos:',
                    style: TextStyle(color: Theme.of(context).colorScheme.onSurfaceVariant),
                  ),
                  const SizedBox(height: 16),
                  SwitchListTile(
                    value: _filterOpenNow,
                    onChanged: (val) => setModalState(() => _filterOpenNow = val),
                    title: const Text('Aberto agora'),
                    activeColor: Theme.of(context).colorScheme.primary,
                  ),
                  SwitchListTile(
                    value: _filterShortLine,
                    onChanged: (val) => setModalState(() => _filterShortLine = val),
                    title: const Text('Fila curta (≤ 3 pessoas)'),
                    activeColor: Theme.of(context).colorScheme.primary,
                  ),
                  SwitchListTile(
                    value: _filterShortWait,
                    onChanged: (val) => setModalState(() => _filterShortWait = val),
                    title: const Text('Espera rápida (≤ 15 min)'),
                    activeColor: Theme.of(context).colorScheme.primary,
                  ),
                  const SizedBox(height: 12),
                  Row(
                    children: [
                      Expanded(
                        child: OutlinedButton(
                          onPressed: () {
                            setModalState(() {
                              _filterOpenNow = false;
                              _filterShortLine = false;
                              _filterShortWait = false;
                            });
                          },
                          child: const Text('Limpar filtros'),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: ElevatedButton(
                          onPressed: () {
                            Navigator.of(context).pop({
                              'openNow': _filterOpenNow,
                              'shortLine': _filterShortLine,
                              'shortWait': _filterShortWait,
                            });
                          },
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Theme.of(context).colorScheme.primary,
                            foregroundColor: Colors.white,
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(12),
                            ),
                          ),
                          child: const Text('Aplicar filtros'),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            );
          },
        );
      },
    );
    if (result != null) {
      setState(() {
        _filterOpenNow = result['openNow'] ?? false;
        _filterShortLine = result['shortLine'] ?? false;
        _filterShortWait = result['shortWait'] ?? false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final salonPalette = CheckInState.checkedInSalon?.colors;
    return Scaffold(
      backgroundColor: salonPalette?.primary ?? theme.colorScheme.primary,
      body: Stack(
          children: [
          FlutterMap(
                mapController: _mapController,
                options: MapOptions(
              initialCenter: const LatLng(28.3372, -82.2637),
                  initialZoom: 12.0,
                  minZoom: 10.0,
                  maxZoom: 18.0,
                  onTap: (_, __) {
                    setState(() {
                      _selectedSalon = null;
                    });
                  },
                  interactionOptions: const InteractionOptions(
                    flags: InteractiveFlag.all,
                  ),
                ),
                children: [
                  TileLayer(
                    urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                    userAgentPackageName: 'com.eutonafila.frontend',
                tileProvider: CancellableNetworkTileProvider(),
                  ),
                  MarkerLayer(
                    markers: _filteredSalons.map((location) {
                      return Marker(
                        point: location.position,
                    width: 60,
                    height: 60,
                    child: GestureDetector(
                          onTap: () {
                            setState(() {
                              _selectedSalon = location.salon;
                            });
                          },
                      child: _buildMarker(location.salon, theme),
                        ),
                      );
                    }).toList(),
                  ),
                ],
              ),
          // Minimal floating AppBar
          Positioned(
            top: 32,
            left: 16,
            right: 16,
            child: SafeArea(
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                    decoration: BoxDecoration(
                  color: (salonPalette?.background ?? theme.colorScheme.surface).withOpacity(0.85),
                  borderRadius: BorderRadius.circular(16),
                      boxShadow: [
                        BoxShadow(
                      color: Colors.black.withOpacity(0.08),
                      blurRadius: 8,
                      offset: const Offset(0, 2),
                        ),
                      ],
                    ),
                child: Row(
                      children: [
                    IconButton(
                      icon: Icon(Icons.arrow_back, color: salonPalette?.primary ?? theme.colorScheme.primary),
                      onPressed: () => Navigator.of(context).pop(),
                    ),
                    const SizedBox(width: 8),
                    Text(
                      'Salões',
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                        color: salonPalette?.primary ?? theme.colorScheme.primary,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
          // Simple bottom sheet for selected salon
          if (_selectedSalon != null)
            Align(
              alignment: Alignment.bottomCenter,
              child: Builder(
                builder: (context) {
                  final isMobile = MediaQuery.of(context).size.width < 600;
                  return Padding(
                    padding: EdgeInsets.symmetric(horizontal: isMobile ? 8 : 16, vertical: 16),
                    child: Center(
                      child: ConstrainedBox(
                        constraints: BoxConstraints(
                          maxWidth: isMobile ? double.infinity : 420,
                        ),
                        child: Material(
                          elevation: 8,
                          borderRadius: BorderRadius.circular(20),
                          color: theme.colorScheme.surface,
                          child: Container(
                            width: double.infinity,
                          padding: const EdgeInsets.all(20),
                            decoration: BoxDecoration(
                              borderRadius: BorderRadius.circular(20),
                            ),
                            child: Column(
                              mainAxisSize: MainAxisSize.min,
                              crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                  _selectedSalon!.name,
                                style: theme.textTheme.titleLarge?.copyWith(
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                                const SizedBox(height: 8),
                                Text(
                                  _selectedSalon!.address,
                                  style: theme.textTheme.bodyMedium,
                                ),
                                const SizedBox(height: 16),
                                Row(
                                  children: [
                                    ElevatedButton(
                                      onPressed: () {
                                  Navigator.of(context).push(
                                    MaterialPageRoute(
                                      builder: (_) => SalonDetailsScreen(
                                              salon: _selectedSalon!,
                                        services: [
                                          SalonService(
                                            id: '1',
                                            name: 'Corte Feminino',
                                            description: 'Corte e finalização',
                                            price: 80.0,
                                            durationMinutes: 60,
                                            categories: ['Corte'],
                                          ),
                                          SalonService(
                                            id: '2',
                                            name: 'Coloração',
                                            description: 'Coloração completa',
                                            price: 150.0,
                                            durationMinutes: 120,
                                            categories: ['Coloração'],
                                          ),
                                        ],
                                        contact: SalonContact(
                                          phone: '(555) 123-4567',
                                          email: 'contato@salon.com',
                                          website: 'www.salon.com',
                                          instagram: '@salon',
                                          facebook: 'Salon',
                                        ),
                                        businessHours: [
                                          SalonHours(
                                            day: 'Segunda - Sexta',
                                            isOpen: true,
                                            openTime: '9:00',
                                            closeTime: '18:00',
                                          ),
                                          SalonHours(
                                            day: 'Sábado',
                                            isOpen: true,
                                            openTime: '9:00',
                                            closeTime: '14:00',
                                          ),
                                          SalonHours(
                                            day: 'Domingo',
                                            isOpen: false,
                                          ),
                                        ],
                                        reviews: [
                                          SalonReview(
                                            id: '1',
                                            userName: 'Maria Silva',
                                            rating: 5,
                                            comment: 'Excelente atendimento!',
                                            date: '2024-03-15',
                                          ),
                                          SalonReview(
                                            id: '2',
                                            userName: 'João Santos',
                                            rating: 4,
                                            comment: 'Muito bom serviço',
                                            date: '2024-03-14',
                                          ),
                                        ],
                                        additionalInfo: {
                                          'Estacionamento': 'Gratuito',
                                          'Formas de Pagamento': 'Dinheiro, Cartão, PIX',
                                          'Acessibilidade': 'Rampa de acesso',
                                        },
                                      ),
                                    ),
                                  );
                                },
                                      style: ElevatedButton.styleFrom(
                                        backgroundColor: theme.colorScheme.primary,
                                        foregroundColor: Colors.white,
                                        shape: RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(12),
                                        ),
                                      ),
                                      child: const Text('Ver detalhes'),
                                    ),
                                    const Spacer(),
                                    IconButton(
                                      icon: const Icon(Icons.close),
                                      onPressed: () {
                                                    setState(() {
                                          _selectedSalon = null;
                                                    });
                                                  },
                                    ),
                                  ],
                                                ),
                                              ],
                                            ),
                          ),
                        ),
                      ),
                    ),
                  );
                },
              ),
            ),
          // Floating action buttons (filter, list, location)
          Positioned(
            bottom: 32,
            right: 24,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                FloatingActionButton.extended(
                  heroTag: 'filter_fab',
                  onPressed: _showFilterModal,
                  icon: const Icon(Icons.filter_list),
                  label: const Text('Filtros'),
                  backgroundColor: salonPalette?.primary ?? theme.colorScheme.primary,
                  foregroundColor: Colors.white,
                  elevation: 4,
                ),
                const SizedBox(height: 16),
                FloatingActionButton(
                  heroTag: 'list_fab',
                  mini: true,
                  backgroundColor: salonPalette?.background ?? theme.colorScheme.surface,
                  foregroundColor: salonPalette?.primary ?? theme.colorScheme.primary,
                  onPressed: () async {
                    final selected = await showModalBottomSheet<int>(
                      context: context,
                      isScrollControlled: true,
                                                shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
                      ),
                      builder: (context) {
                        return _SalonListModal(
                          salons: _salonLocations,
                          theme: theme,
                        );
                      },
                    );
                    if (selected != null) {
                      final loc = _salonLocations[selected];
                      _mapController.move(loc.position, 15.0);
                      setState(() {
                        _selectedSalon = loc.salon;
                      });
                    }
                  },
                  child: const Icon(Icons.list),
                ),
                const SizedBox(height: 16),
                FloatingActionButton(
                  heroTag: 'location_fab',
                  onPressed: () {
                    _mapController.move(const LatLng(28.3372, -82.2637), 14.0);
                  },
                  backgroundColor: salonPalette?.primary ?? theme.colorScheme.primary,
                  foregroundColor: Colors.white,
                  child: const Icon(Icons.my_location),
                ),
              ],
            ),
          ),
      ),
          ],
      ),
    );
  }

  Widget _buildMarker(Salon salon, ThemeData theme) {
    return Container(
      width: 50,
      height: 50,
      decoration: BoxDecoration(
        color: waitTimeToColor(salon.waitTime),
        shape: BoxShape.circle,
        border: Border.all(color: Colors.white, width: 3),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.2),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
            child: Icon(
        Icons.content_cut,
        color: Colors.white,
        size: 24,
      ),
    );
  }
}

class SalonLocation {
  final Salon salon;
  final LatLng position;

  SalonLocation({
    required this.salon,
    required this.position,
  });
}

class _SalonListModal extends StatefulWidget {
  final List<SalonLocation> salons;
  final ThemeData theme;
  const _SalonListModal({required this.salons, required this.theme});

  @override
  State<_SalonListModal> createState() => _SalonListModalState();
}

class _SalonListModalState extends State<_SalonListModal> {
  String _search = '';

  @override
  Widget build(BuildContext context) {
    final filtered = widget.salons.where((loc) {
      final q = _search.toLowerCase();
      return loc.salon.name.toLowerCase().contains(q) ||
             loc.salon.address.toLowerCase().contains(q);
    }).toList();
    return DraggableScrollableSheet(
      expand: false,
      initialChildSize: 0.95,
      minChildSize: 0.5,
      maxChildSize: 0.98,
      builder: (context, scrollController) {
        return Container(
          decoration: BoxDecoration(
            color: widget.theme.colorScheme.surface,
            borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withOpacity(0.08),
                blurRadius: 8,
                offset: const Offset(0, -2),
              ),
            ],
          ),
          child: Column(
            children: [
              Container(
                width: 40,
                height: 5,
                margin: const EdgeInsets.only(top: 12, bottom: 8),
              decoration: BoxDecoration(
                  color: widget.theme.colorScheme.outline.withOpacity(0.2),
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              Padding(
                padding: const EdgeInsets.fromLTRB(24, 0, 24, 8),
                child: TextField(
                  autofocus: true,
                  decoration: InputDecoration(
                    hintText: 'Buscar salão...',
                    prefixIcon: Icon(Icons.search, color: widget.theme.colorScheme.primary),
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(16),
                      borderSide: BorderSide.none,
                    ),
                    filled: true,
                    fillColor: widget.theme.colorScheme.surfaceVariant.withOpacity(0.5),
                  ),
                  onChanged: (value) => setState(() => _search = value),
                ),
              ),
              Expanded(
                child: filtered.isEmpty
                    ? Center(
                        child: Text('Nenhum salão encontrado', style: widget.theme.textTheme.bodyLarge),
                      )
                    : ListView.separated(
                        controller: scrollController,
                        padding: const EdgeInsets.fromLTRB(24, 0, 24, 24),
                        itemCount: filtered.length,
                        separatorBuilder: (_, __) => const SizedBox(height: 16),
                        itemBuilder: (context, index) {
                          final location = filtered[index];
                          final salon = location.salon;
                          return Material(
                            color: Colors.transparent,
                            child: InkWell(
                              borderRadius: BorderRadius.circular(16),
                              onTap: () => Navigator.of(context).pop(widget.salons.indexOf(location)),
      child: Container(
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
                                  color: widget.theme.colorScheme.surface,
          borderRadius: BorderRadius.circular(16),
          boxShadow: [
            BoxShadow(
                                      color: Colors.black.withOpacity(0.03),
                                      blurRadius: 2,
                                      offset: const Offset(0, 1),
            ),
          ],
        ),
                                child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Container(
                                      width: 48,
                                      height: 48,
                  decoration: BoxDecoration(
                                        color: salon.isOpen ? salon.colors.primary.withOpacity(0.1) : Colors.grey.withOpacity(0.1),
                                        shape: BoxShape.circle,
                  ),
                  child: Icon(
                    Icons.store,
                                        color: salon.isOpen ? salon.colors.primary : Colors.grey,
                                        size: 28,
                  ),
                ),
                                    const SizedBox(width: 16),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                                          Text(
                                            salon.name,
                                            style: widget.theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
                                            maxLines: 1,
                                            overflow: TextOverflow.ellipsis,
                      ),
                      const SizedBox(height: 4),
                      Text(
                                            salon.address,
                                            style: widget.theme.textTheme.bodySmall,
                                            maxLines: 1,
                                            overflow: TextOverflow.ellipsis,
                                          ),
                                          const SizedBox(height: 8),
                                          Row(
                    children: [
                                              _buildChip(
                                                context,
                                                salon.isOpen ? 'Aberto' : 'Fechado',
                                                salon.isOpen ? Colors.green : Colors.red,
                                                icon: salon.isOpen ? Icons.check_circle : Icons.cancel,
                                              ),
                                              const SizedBox(width: 6),
                                              _buildChip(
                                                context,
                                                '${salon.waitTime} min',
                                                widget.theme.colorScheme.primary,
                                                icon: Icons.timer,
                                              ),
                                              const SizedBox(width: 6),
                                              _buildChip(
                                                context,
                                                '${salon.queueLength} na fila',
                                                widget.theme.colorScheme.secondary,
                                                icon: Icons.people_outline,
                                              ),
                                              const SizedBox(width: 6),
                                              _buildChip(
                                                context,
                                                '${salon.distance} km',
                                                widget.theme.colorScheme.primary,
                                                icon: Icons.location_on,
                                              ),
                                            ],
                                          ),
                                        ],
                  ),
                ),
                const SizedBox(width: 8),
                                    Icon(Icons.chevron_right, color: widget.theme.colorScheme.primary),
                                  ],
                                ),
                              ),
                            ),
                          );
                        },
              ),
            ),
          ],
          ),
        );
      },
    );
  }

  Widget _buildChip(BuildContext context, String label, Color color, {IconData? icon}) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
        decoration: BoxDecoration(
        color: color.withOpacity(0.1),
              borderRadius: BorderRadius.circular(8),
                ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
              children: [
          if (icon != null) ...[
            Icon(icon, size: 14, color: color),
            const SizedBox(width: 2),
          ],
          Text(
            label,
            style: widget.theme.textTheme.labelMedium?.copyWith(
              color: color,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }
} 