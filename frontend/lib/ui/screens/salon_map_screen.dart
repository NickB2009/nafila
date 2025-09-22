import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:flutter_map_cancellable_tile_provider/flutter_map_cancellable_tile_provider.dart';
import 'package:provider/provider.dart';
import 'package:geolocator/geolocator.dart';
import '../../models/salon.dart';
import '../../models/public_salon.dart';
import 'salon_details_screen.dart';
import '../../models/salon_service.dart';
import '../../models/salon_contact.dart';
import '../widgets/bottom_nav_bar.dart';
import '../../utils/palette_utils.dart';
import '../../controllers/app_controller.dart';
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
  PublicSalon? _selectedSalon;
  List<SalonLocation> _salonLocations = [];
  bool _isLoading = false;
  Position? _userPosition;
  
  // Default location (São Paulo, Brazil)
  static const LatLng _defaultCenter = LatLng(-23.5505, -46.6333);

  // Filter state
  bool _filterOpenNow = false;
  bool _filterShortLine = false;
  bool _filterShortWait = false;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadSalons();
      _getCurrentLocation();
    });
  }

  /// Load salons from the backend API
  Future<void> _loadSalons() async {
    if (!mounted) return;
    
    setState(() {
      _isLoading = true;
    });

    try {
      final appController = Provider.of<AppController>(context, listen: false);
      
      // Check if app controller is initialized
      if (!appController.isInitialized) {
        print('⚠️ AppController not yet initialized, using fallback data');
        _generateFallbackSalons();
        return;
      }
      
      // Load public salons through the anonymous controller
      await appController.anonymous.loadPublicSalons();
      
      // Get the loaded salons
      final loadedSalons = appController.anonymous.nearbySalons;
      
      if (loadedSalons.isNotEmpty && mounted) {
        final salonLocations = <SalonLocation>[];
        
        for (int i = 0; i < loadedSalons.length; i++) {
          final salon = loadedSalons[i];
          
          // Use salon coordinates if available, otherwise use default locations around São Paulo
          final position = _getSalonPosition(salon, i);
          
          salonLocations.add(SalonLocation(
            publicSalon: salon,
            salon: _convertToSalon(salon, i),
            position: position,
          ));
        }
        
        setState(() {
          _salonLocations = salonLocations;
        });
        print('✅ Loaded ${salonLocations.length} salons for map');
      } else {
        print('⚠️ No salons returned from API, using fallback');
        _generateFallbackSalons();
      }
    } catch (e) {
      print('❌ Error loading salons from API: $e');
      _generateFallbackSalons();
      
      
      // Show snackbar with error
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Erro ao carregar salões. Tente novamente mais tarde.'),
            backgroundColor: Colors.orange,
            duration: Duration(seconds: 3),
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
      }
    }
  }

  /// Get salon position from coordinates or generate default position
  LatLng _getSalonPosition(PublicSalon salon, int index) {
    if (salon.latitude != null && salon.longitude != null) {
      return LatLng(salon.latitude!, salon.longitude!);
    }
    
    // Generate positions around São Paulo if coordinates are not available
    final baseLatitude = -23.5505;
    final baseLongitude = -46.6333;
    final random = index * 0.01; // Use index for consistent positioning
    
    return LatLng(
      baseLatitude + (random - 0.005),
      baseLongitude + (random - 0.005),
    );
  }

  /// Convert PublicSalon to Salon for compatibility
  Salon _convertToSalon(PublicSalon publicSalon, int index) {
    return Salon(
      name: publicSalon.name,
      address: publicSalon.address,
      waitTime: publicSalon.currentWaitTimeMinutes ?? 15,
      distance: publicSalon.distanceKm ?? 1.0,
      isOpen: publicSalon.isOpen,
      closingTime: '19:00', // Default closing time
      isFavorite: false,
      queueLength: publicSalon.queueLength ?? 0,
      colors: _randomSalonColors(index),
    );
  }

  /// Fallback method when API is unavailable - show empty state
  void _generateFallbackSalons() {
    _salonLocations = [];
    
    if (mounted) {
      setState(() {});
    }
  }

  /// Get user's current location
  Future<void> _getCurrentLocation() async {
    try {
      bool serviceEnabled = await Geolocator.isLocationServiceEnabled();
      if (!serviceEnabled) {
        print('⚠️ Location services are disabled');
        return;
      }

      LocationPermission permission = await Geolocator.checkPermission();
      if (permission == LocationPermission.denied) {
        permission = await Geolocator.requestPermission();
        if (permission == LocationPermission.denied) {
          print('⚠️ Location permissions are denied');
          return;
        }
      }

      if (permission == LocationPermission.deniedForever) {
        print('⚠️ Location permissions are permanently denied');
        return;
      }

      final position = await Geolocator.getCurrentPosition();
      setState(() {
        _userPosition = position;
      });
      
      print('✅ Got user location: ${position.latitude}, ${position.longitude}');
    } catch (e) {
      print('❌ Error getting location: $e');
    }
  }

  /// Move map to user's location
  void _goToUserLocation() {
    if (_userPosition != null) {
      _mapController.move(
        LatLng(_userPosition!.latitude, _userPosition!.longitude),
        15.0,
      );
    } else {
      // Try to get location again
      _getCurrentLocation();
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Obtendo sua localização...'),
          duration: Duration(seconds: 2),
        ),
      );
    }
  }

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
              initialCenter: _defaultCenter,
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
              if (!_isLoading)
                MarkerLayer(
                  markers: [
                    // Salon markers
                    ..._filteredSalons.map((location) {
                      return Marker(
                        point: location.position,
                        width: 60,
                        height: 60,
                        child: GestureDetector(
                          onTap: () {
                            setState(() {
                              _selectedSalon = location.publicSalon;
                            });
                          },
                          child: _buildMarker(location.salon, theme),
                        ),
                      );
                    }),
                    // User location marker
                    if (_userPosition != null)
                      Marker(
                        point: LatLng(_userPosition!.latitude, _userPosition!.longitude),
                        width: 40,
                        height: 40,
                        child: Container(
                          decoration: BoxDecoration(
                            color: Colors.blue,
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
                          child: const Icon(
                            Icons.person,
                            color: Colors.white,
                            size: 20,
                          ),
                        ),
                      ),
                  ],
                ),
            ],
          ),
          
          // Loading indicator
          if (_isLoading)
            Container(
              color: Colors.black.withOpacity(0.3),
              child: const Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    CircularProgressIndicator(color: Colors.white),
                    SizedBox(height: 16),
                    Text(
                      'Carregando salões...',
                      style: TextStyle(color: Colors.white, fontSize: 16),
                    ),
                  ],
                ),
              ),
            ),
          
          // Zoom buttons for desktop/laptop only
          if (showZoomButtons)
            Positioned(
              top: 120,
              right: 24,
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
                        'Mapa de Salões',
                        style: theme.textTheme.titleMedium?.copyWith(
                          fontWeight: FontWeight.bold,
                          color: salonPalette?.primary ?? theme.colorScheme.primary,
                          fontSize: isSmallScreen ? 16 : null,
                        ),
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),
                    if (_isLoading)
                      SizedBox(
                        width: 20,
                        height: 20,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          valueColor: AlwaysStoppedAnimation<Color>(
                            salonPalette?.primary ?? theme.colorScheme.primary
                          ),
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
                                  maxLines: 2,
                                ),
                                SizedBox(height: isMobile ? 8 : 12),
                                Row(
                                  children: [
                                    _buildInfoChip(
                                      _selectedSalon!.isOpen ? 'Aberto' : 'Fechado',
                                      _selectedSalon!.isOpen ? Colors.green : Colors.red,
                                      Icons.access_time,
                                    ),
                                    const SizedBox(width: 8),
                                    _buildInfoChip(
                                      '${_selectedSalon!.currentWaitTimeMinutes ?? 0} min',
                                      theme.colorScheme.primary,
                                      Icons.timer,
                                    ),
                                    const SizedBox(width: 8),
                                    _buildInfoChip(
                                      '${_selectedSalon!.queueLength ?? 0} fila',
                                      theme.colorScheme.secondary,
                                      Icons.people,
                                    ),
                                  ],
                                ),
                                SizedBox(height: isMobile ? 12 : 16),
                                Row(
                                  children: [
                                    Expanded(
                                      child: ElevatedButton(
                                        onPressed: () {
                                          // Navigate to salon details with real data
                                          Navigator.of(context).push(
                                            MaterialPageRoute(
                                              builder: (_) => SalonDetailsScreen(
                                                salon: _convertToSalon(_selectedSalon!, 0),
                                                services: _convertToSalonServices(_selectedSalon!),
                                                contact: const SalonContact(phone: '', email: ''),
                                                businessHours: const [],
                                                reviews: const [],
                                                additionalInfo: const {},
                                                publicSalon: _selectedSalon,
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
                      shape: const RoundedRectangleBorder(
                        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
                      ),
                      builder: (context) {
                        return _SalonListModal(
                          salons: _salonLocations,
                          theme: theme,
                        );
                      },
                    );
                    if (selected != null && selected < _salonLocations.length) {
                      setState(() {
                        _selectedSalon = _salonLocations[selected].publicSalon;
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
                  onPressed: _goToUserLocation,
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

  Widget _buildInfoChip(String label, Color color, IconData icon) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: color.withOpacity(0.3)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 12, color: color),
          const SizedBox(width: 4),
          Text(
            label,
            style: TextStyle(
              color: color,
              fontSize: 11,
              fontWeight: FontWeight.w600,
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
      child: const Icon(
        Icons.content_cut,
        color: Colors.white,
        size: 24,
      ),
    );
  }

  // Convert PublicSalon services to SalonService list
  List<SalonService> _convertToSalonServices(PublicSalon publicSalon) {
    final services = publicSalon.services ?? ['Haircut', 'Beard Trim'];
    return services.map((service) => SalonService(
      id: service.toLowerCase().replaceAll(' ', '_'),
      name: service,
      description: 'Professional $service service',
      price: 25.0, // Default price
      durationMinutes: 30, // Default duration in minutes
    )).toList();
  }
}

class SalonLocation {
  final PublicSalon publicSalon;
  final Salon salon;
  final LatLng position;

  SalonLocation({
    required this.publicSalon,
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
                                                '${salon.distance.toStringAsFixed(1)} km',
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