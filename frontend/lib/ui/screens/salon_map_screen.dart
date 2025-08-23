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
import '../../utils/palette_utils.dart';
import 'package:flutter/foundation.dart' show defaultTargetPlatform, TargetPlatform;

SalonColors _randomSalonColors(int seed) {
  final palettes = [
    SalonColors(primary: Colors.redAccent, secondary: Colors.orange, background: Colors.red.shade50, onSurface: Colors.red.shade900),
    SalonColors(primary: Colors.blueAccent, secondary: Colors.cyan, background: Colors.blue.shade50, onSurface: Colors.blue.shade900),
    SalonColors(primary: Colors.green, secondary: Colors.teal, background: Colors.green.shade50, onSurface: Colors.green.shade900),
    SalonColors(primary: Colors.purple, secondary: Colors.pinkAccent, background: Colors.purple.shade50, onSurface: Colors.purple.shade900),
    SalonColors(primary: Colors.amber, secondary: Colors.deepOrange, background: Colors.amber.shade50, onSurface: Colors.amber.shade900),
    SalonColors(primary: Colors.indigo, secondary: Colors.lime, background: Colors.indigo.shade50, onSurface: Colors.indigo.shade900),
  ];
  final light = palettes[seed % palettes.length];
  return SalonColors(
    primary: light.primary,
    secondary: light.secondary,
    background: light.background,
    onSurface: light.onSurface,
    dark: generateDarkPalette(light),
  );
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
    final theme = Theme.of(context);
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    
    final result = await showModalBottomSheet<Map<String, bool>>(
      context: context,
      backgroundColor: colors?.background ?? theme.colorScheme.surface,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setModalState) {
            return Container(
              decoration: BoxDecoration(
                color: colors?.background ?? theme.colorScheme.surface,
                borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
              ),
              child: Padding(
                padding: const EdgeInsets.fromLTRB(24, 24, 24, 24),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          'Filtros', 
                          style: theme.textTheme.titleLarge?.copyWith(
                            fontWeight: FontWeight.bold,
                            color: colors?.onSurface ?? theme.colorScheme.onSurface,
                          ),
                        ),
                        IconButton(
                          icon: Icon(Icons.close, color: colors?.onSurface ?? theme.colorScheme.onSurface),
                          onPressed: () => Navigator.of(context).pop(),
                        ),
                      ],
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Refine sua busca de salões próximos:',
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
                      ),
                    ),
                    const SizedBox(height: 16),
                    SwitchListTile(
                      value: _filterOpenNow,
                      onChanged: (val) => setModalState(() => _filterOpenNow = val),
                      title: Text(
                        'Aberto agora',
                        style: theme.textTheme.bodyLarge?.copyWith(
                          color: colors?.onSurface ?? theme.colorScheme.onSurface,
                        ),
                      ),
                      activeColor: colors?.primary ?? theme.colorScheme.primary,
                    ),
                    SwitchListTile(
                      value: _filterShortLine,
                      onChanged: (val) => setModalState(() => _filterShortLine = val),
                      title: Text(
                        'Fila curta (≤ 3 pessoas)',
                        style: theme.textTheme.bodyLarge?.copyWith(
                          color: colors?.onSurface ?? theme.colorScheme.onSurface,
                        ),
                      ),
                      activeColor: colors?.primary ?? theme.colorScheme.primary,
                    ),
                    SwitchListTile(
                      value: _filterShortWait,
                      onChanged: (val) => setModalState(() => _filterShortWait = val),
                      title: Text(
                        'Espera rápida (≤ 15 min)',
                        style: theme.textTheme.bodyLarge?.copyWith(
                          color: colors?.onSurface ?? theme.colorScheme.onSurface,
                        ),
                      ),
                      activeColor: colors?.primary ?? theme.colorScheme.primary,
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
                            style: OutlinedButton.styleFrom(
                              foregroundColor: colors?.primary ?? theme.colorScheme.primary,
                              side: BorderSide(color: colors?.primary ?? theme.colorScheme.primary),
                            ),
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
                              backgroundColor: colors?.primary ?? theme.colorScheme.primary,
                              foregroundColor: colors?.background ?? theme.colorScheme.onPrimary,
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
    // Platform detection for desktop/laptop
    final isDesktop =
        defaultTargetPlatform == TargetPlatform.macOS ||
        defaultTargetPlatform == TargetPlatform.windows ||
        defaultTargetPlatform == TargetPlatform.linux ||
        (defaultTargetPlatform == TargetPlatform.fuchsia) ||
        (defaultTargetPlatform == TargetPlatform.android && MediaQuery.of(context).size.width > 900) ||
        (defaultTargetPlatform == TargetPlatform.iOS && MediaQuery.of(context).size.width > 900) ||
        (Theme.of(context).platform == TargetPlatform.macOS ||
         Theme.of(context).platform == TargetPlatform.windows ||
         Theme.of(context).platform == TargetPlatform.linux) ||
        (MediaQuery.of(context).size.width > 900 && Theme.of(context).platform == TargetPlatform.android);
    // For web, treat as desktop if width is large
    final isWebDesktop = MediaQuery.of(context).size.width > 900 && (defaultTargetPlatform != TargetPlatform.android && defaultTargetPlatform != TargetPlatform.iOS);
    final showZoomButtons = isDesktop || isWebDesktop;
    final isSmallScreen = MediaQuery.of(context).size.width < 600;
    return Scaffold(
      backgroundColor: salonPalette?.primary ?? theme.colorScheme.primary,
      body: Stack(
          children: [
          FlutterMap(
                mapController: _mapController,
                options: MapOptions(
              initialCenter: const LatLng(28.3372, -82.2637),
                  initialZoom: 12.0,
                  minZoom: 2.0,
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
          // Zoom buttons for desktop/laptop only
          if (showZoomButtons)
            Positioned(
              top: 120, // move higher up
              right: 24, // move to the extreme right
              child: Column(
                children: [
                  FloatingActionButton(
                    heroTag: 'zoom_in_fab',
                    mini: true,
                    backgroundColor: salonPalette?.primary ?? theme.colorScheme.primary,
                    foregroundColor: Colors.white,
                    onPressed: () {
                      final center = _mapController.camera.center;
                      final zoom = _mapController.camera.zoom + 1;
                      _mapController.move(center, zoom.clamp(2.0, 18.0));
                    },
                    child: const Icon(Icons.add),
                  ),
                  const SizedBox(height: 8),
                  FloatingActionButton(
                    heroTag: 'zoom_out_fab',
                    mini: true,
                    backgroundColor: salonPalette?.primary ?? theme.colorScheme.primary,
                    foregroundColor: Colors.white,
                    onPressed: () {
                      final center = _mapController.camera.center;
                      final zoom = _mapController.camera.zoom - 1;
                      _mapController.move(center, zoom.clamp(2.0, 18.0));
                    },
                    child: const Icon(Icons.remove),
                  ),
                ],
              ),
            ),
          // Minimal floating AppBar
          Positioned(
            top: isSmallScreen ? 16 : 32,
            left: isSmallScreen ? 8 : 16,
            right: isSmallScreen ? 8 : 16,
            child: SafeArea(
              child: Container(
                padding: EdgeInsets.symmetric(
                  horizontal: isSmallScreen ? 8 : 12, 
                  vertical: isSmallScreen ? 6 : 8
                ),
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
                      icon: Icon(
                        Icons.arrow_back, 
                        color: salonPalette?.primary ?? theme.colorScheme.primary,
                        size: isSmallScreen ? 20 : 24,
                      ),
                      onPressed: () => Navigator.of(context).pop(),
                      padding: EdgeInsets.zero,
                      constraints: BoxConstraints(
                        minWidth: isSmallScreen ? 36 : 48,
                        minHeight: isSmallScreen ? 36 : 48,
                      ),
                    ),
                    SizedBox(width: isSmallScreen ? 6 : 8),
                    Expanded(
                      child: Text(
                        'Salões',
                        style: theme.textTheme.titleMedium?.copyWith(
                          fontWeight: FontWeight.bold,
                          color: salonPalette?.primary ?? theme.colorScheme.primary,
                          fontSize: isSmallScreen ? 16 : null,
                        ),
                        overflow: TextOverflow.ellipsis,
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
                  final theme = Theme.of(context);
                  return Padding(
                    padding: EdgeInsets.symmetric(horizontal: isMobile ? 8 : 16, vertical: isMobile ? 12 : 16),
                    child: Center(
                      child: ConstrainedBox(
                        constraints: BoxConstraints(
                          maxWidth: isMobile ? double.infinity : 420,
                        ),
                        child: Material(
                          elevation: 8,
                          borderRadius: BorderRadius.circular(20),
                          color: theme.colorScheme.surface,
                          shadowColor: Colors.black.withOpacity(0.2),
                          child: Container(
                            width: double.infinity,
                            padding: EdgeInsets.all(isMobile ? 16 : 20),
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
                                    fontSize: isMobile ? 18 : null,
                                  ),
                                  overflow: TextOverflow.ellipsis,
                                  maxLines: 1,
                                ),
                                SizedBox(height: isMobile ? 6 : 8),
                                Text(
                                  _selectedSalon!.address,
                                  style: theme.textTheme.bodyMedium?.copyWith(
                                    fontSize: isMobile ? 13 : null,
                                  ),
                                  overflow: TextOverflow.ellipsis,
                                  maxLines: 1,
                                ),
                                SizedBox(height: isMobile ? 12 : 16),
                                Row(
                                  children: [
                                    Expanded(
                                      child: ElevatedButton(
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
                                          padding: EdgeInsets.symmetric(
                                            vertical: isMobile ? 10 : 12,
                                          ),
                                        ),
                                        child: Text(
                                          'Ver detalhes',
                                          style: TextStyle(fontSize: isMobile ? 13 : 14),
                                        ),
                                      ),
                                    ),
                                    SizedBox(width: isMobile ? 8 : 12),
                                    IconButton(
                                      icon: Icon(
                                        Icons.close,
                                        size: isMobile ? 20 : 24,
                                      ),
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
            bottom: isSmallScreen ? 16 : 32,
            right: isSmallScreen ? 16 : 24,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                FloatingActionButton.extended(
                  heroTag: 'filter_fab',
                  onPressed: _showFilterModal,
                  icon: Icon(
                    Icons.filter_list,
                    size: isSmallScreen ? 20 : 24,
                  ),
                  label: Text(
                    'Filtros',
                    style: TextStyle(fontSize: isSmallScreen ? 12 : 14),
                  ),
                  backgroundColor: salonPalette?.primary ?? theme.colorScheme.primary,
                  foregroundColor: theme.colorScheme.onPrimary,
                  elevation: 4,
                ),
                SizedBox(height: isSmallScreen ? 12 : 16),
                FloatingActionButton(
                  heroTag: 'list_fab',
                  mini: isSmallScreen,
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
                      setState(() {
                        _selectedSalon = _salonLocations[selected].salon;
                      });
                      _mapController.move(
                        _salonLocations[selected].position,
                        15.0,
                      );
                    }
                  },
                  child: Icon(
                    Icons.list,
                    size: isSmallScreen ? 20 : 24,
                  ),
                ),
                SizedBox(height: isSmallScreen ? 12 : 16),
                FloatingActionButton(
                  heroTag: 'location_fab',
                  mini: isSmallScreen,
                  backgroundColor: salonPalette?.background ?? theme.colorScheme.surface,
                  foregroundColor: salonPalette?.primary ?? theme.colorScheme.primary,
                  onPressed: () {
                    // TODO: Implement location functionality
                  },
                  child: Icon(
                    Icons.my_location,
                    size: isSmallScreen ? 20 : 24,
                  ),
                ),
              ],
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
        border: Border.all(
          color: Colors.white, 
          width: 3
        ),
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
    final isSmallScreen = MediaQuery.of(context).size.width < 600;
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    
    return DraggableScrollableSheet(
      expand: false,
      initialChildSize: 0.95,
      minChildSize: 0.5,
      maxChildSize: 0.98,
      builder: (context, scrollController) {
        return Container(
          decoration: BoxDecoration(
            color: colors?.background ?? widget.theme.colorScheme.surface,
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
                margin: EdgeInsets.only(top: isSmallScreen ? 8 : 12, bottom: isSmallScreen ? 6 : 8),
                decoration: BoxDecoration(
                  color: colors?.secondary ?? widget.theme.colorScheme.outline.withOpacity(0.2),
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              Padding(
                padding: EdgeInsets.fromLTRB(
                  isSmallScreen ? 16 : 24, 
                  0, 
                  isSmallScreen ? 16 : 24, 
                  isSmallScreen ? 6 : 8
                ),
                child: TextField(
                  autofocus: true,
                  decoration: InputDecoration(
                    hintText: 'Buscar salão...',
                    prefixIcon: Icon(
                      Icons.search, 
                      color: colors?.primary ?? widget.theme.colorScheme.primary,
                      size: isSmallScreen ? 20 : 24,
                    ),
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(16),
                      borderSide: BorderSide.none,
                    ),
                    filled: true,
                    fillColor: colors?.background != null 
                      ? colors!.background.withOpacity(0.1)
                      : widget.theme.colorScheme.surfaceContainerHighest.withOpacity(0.5),
                    contentPadding: EdgeInsets.symmetric(
                      horizontal: isSmallScreen ? 12 : 16,
                      vertical: isSmallScreen ? 12 : 16,
                    ),
                  ),
                  onChanged: (value) => setState(() => _search = value),
                ),
              ),
              Expanded(
                child: filtered.isEmpty
                    ? Center(
                        child: Text(
                          'Nenhum salão encontrado', 
                          style: widget.theme.textTheme.bodyLarge?.copyWith(
                            fontSize: isSmallScreen ? 14 : null,
                            color: colors?.secondary ?? widget.theme.colorScheme.onSurfaceVariant,
                          ),
                        ),
                      )
                    : ListView.separated(
                        controller: scrollController,
                        padding: EdgeInsets.fromLTRB(
                          isSmallScreen ? 16 : 24, 
                          0, 
                          isSmallScreen ? 16 : 24, 
                          isSmallScreen ? 16 : 24
                        ),
                        itemCount: filtered.length,
                        separatorBuilder: (_, __) => SizedBox(height: isSmallScreen ? 12 : 16),
                        itemBuilder: (context, index) {
                          final location = filtered[index];
                          final salon = location.salon;
                          final isSmallScreen = MediaQuery.of(context).size.width < 600;
                          return Material(
                            color: Colors.transparent,
                            child: InkWell(
                              borderRadius: BorderRadius.circular(16),
                              onTap: () => Navigator.of(context).pop(widget.salons.indexOf(location)),
                              child: Container(
                                padding: EdgeInsets.all(isSmallScreen ? 16 : 20),
                                decoration: BoxDecoration(
                                  color: colors?.background ?? widget.theme.colorScheme.surface,
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
                                      width: isSmallScreen ? 40 : 48,
                                      height: isSmallScreen ? 40 : 48,
                                      decoration: BoxDecoration(
                                        color: salon.isOpen ? salon.colors.primary.withOpacity(0.1) : Colors.grey.withOpacity(0.1),
                                        shape: BoxShape.circle,
                                      ),
                                      child: Icon(
                                        Icons.store,
                                        color: salon.isOpen ? salon.colors.primary : Colors.grey,
                                        size: isSmallScreen ? 24 : 28,
                                      ),
                                    ),
                                    SizedBox(width: isSmallScreen ? 12 : 16),
                                    Expanded(
                                      child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: [
                                          Text(
                                            salon.name,
                                            style: widget.theme.textTheme.titleMedium?.copyWith(
                                              fontWeight: FontWeight.bold,
                                              fontSize: isSmallScreen ? 16 : null,
                                            ),
                                            maxLines: 1,
                                            overflow: TextOverflow.ellipsis,
                                          ),
                                          SizedBox(height: isSmallScreen ? 2 : 4),
                                          Text(
                                            salon.address,
                                            style: widget.theme.textTheme.bodySmall?.copyWith(
                                              fontSize: isSmallScreen ? 12 : null,
                                            ),
                                            maxLines: 1,
                                            overflow: TextOverflow.ellipsis,
                                          ),
                                          SizedBox(height: isSmallScreen ? 6 : 8),
                                          // Always use Wrap to prevent overflow
                                          Wrap(
                                            spacing: isSmallScreen ? 4 : 6,
                                            runSpacing: isSmallScreen ? 4 : 6,
                                            children: [
                                              _buildChip(
                                                context,
                                                salon.isOpen ? 'Aberto' : 'Fechado',
                                                salon.isOpen ? Colors.green : Colors.red,
                                                icon: salon.isOpen ? Icons.check_circle : Icons.cancel,
                                                isSmallScreen: isSmallScreen,
                                              ),
                                              _buildChip(
                                                context,
                                                '${salon.waitTime} min',
                                                widget.theme.colorScheme.primary,
                                                icon: Icons.timer,
                                                isSmallScreen: isSmallScreen,
                                              ),
                                              _buildChip(
                                                context,
                                                '${salon.queueLength} na fila',
                                                widget.theme.colorScheme.secondary,
                                                icon: Icons.people_outline,
                                                isSmallScreen: isSmallScreen,
                                              ),
                                              _buildChip(
                                                context,
                                                '${salon.distance} km',
                                                widget.theme.colorScheme.primary,
                                                icon: Icons.location_on,
                                                isSmallScreen: isSmallScreen,
                                              ),
                                            ],
                                          ),
                                        ],
                                      ),
                                    ),
                                    SizedBox(width: isSmallScreen ? 4 : 8),
                                    Icon(
                                      Icons.chevron_right, 
                                      color: widget.theme.colorScheme.primary,
                                      size: isSmallScreen ? 20 : 24,
                                    ),
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

  Widget _buildChip(BuildContext context, String label, Color color, {IconData? icon, bool isSmallScreen = false}) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;
    final chipColor = isDark ? color.withOpacity(0.8) : color;
    final backgroundColor = isDark ? chipColor.withOpacity(0.2) : chipColor.withOpacity(0.1);
    
    return Container(
      padding: EdgeInsets.symmetric(
        horizontal: isSmallScreen ? 4 : 6, 
        vertical: isSmallScreen ? 2 : 3
      ),
      decoration: BoxDecoration(
        color: backgroundColor,
        borderRadius: BorderRadius.circular(6),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          if (icon != null) ...[
            Icon(
              icon, 
              size: isSmallScreen ? 10 : 12, 
              color: chipColor
            ),
            SizedBox(width: isSmallScreen ? 1 : 2),
          ],
          Text(
            label,
            style: widget.theme.textTheme.labelSmall?.copyWith(
              color: chipColor,
              fontWeight: FontWeight.w600,
              fontSize: isSmallScreen ? 10 : 11,
            ),
          ),
        ],
      ),
    );
  }
} 