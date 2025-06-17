import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import '../../models/salon.dart';
import 'check_in_screen.dart';
import 'notifications_screen.dart';
import '../widgets/bottom_nav_bar.dart';

class SalonMapScreen extends StatefulWidget {
  const SalonMapScreen({super.key});

  @override
  State<SalonMapScreen> createState() => _SalonMapScreenState();
}

class _SalonMapScreenState extends State<SalonMapScreen> {
  final MapController _mapController = MapController();
  Salon? _selectedSalon;
  bool _showList = true;
  String _selectedFilter = 'Aberto agora';
  bool _showSearch = false;
  final TextEditingController _searchController = TextEditingController();
  final Set<String> _favoriteSalons = {};
  
  // Mock salon locations in San Antonio, FL area
  final List<SalonLocation> _salonLocations = [
    SalonLocation(
      salon: const Salon(
        name: 'Market at Mirada',
        address: '30921 Mirada Blvd, San Antonio, FL',
        waitTime: 24,
        distance: 10.9,
        isOpen: true,
        closingTime: '6 PM',
        isFavorite: true,
        queueLength: 5,
      ),
      position: const LatLng(28.3372, -82.2637), // San Antonio, FL area
    ),
    SalonLocation(
      salon: const Salon(
        name: 'Cortez Commons',
        address: '123 Cortez Ave, San Antonio, FL',
        waitTime: 8,
        distance: 5.2,
        isOpen: true,
        closingTime: '8 PM',
        isFavorite: true,
        queueLength: 2,
      ),
      position: const LatLng(28.3422, -82.2587),
    ),
    SalonLocation(
      salon: const Salon(
        name: 'Westshore Plaza',
        address: '456 Westshore Blvd, San Antonio, FL',
        waitTime: 15,
        distance: 7.8,
        isOpen: true,
        closingTime: '9 PM',
        isFavorite: false,
        queueLength: 3,
      ),
      position: const LatLng(28.3322, -82.2687),
    ),
    SalonLocation(
      salon: const Salon(
        name: 'Tampa Bay Center',
        address: '789 Bay Center Dr, San Antonio, FL',
        waitTime: 32,
        distance: 12.1,
        isOpen: false,
        closingTime: '7 PM',
        isFavorite: false,
        queueLength: 8,
      ),
      position: const LatLng(28.3272, -82.2737),
    ),
  ];

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    return Scaffold(
      backgroundColor: theme.colorScheme.primary,
      body: SafeArea(
        child: Stack(
          children: [
            // Map
            ClipRRect(
              borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
              child: FlutterMap(
                mapController: _mapController,
                options: MapOptions(
                  initialCenter: const LatLng(28.3372, -82.2637), // San Antonio, FL
                  initialZoom: 12.0,
                  minZoom: 10.0,
                  maxZoom: 18.0,
                  onTap: (_, __) {
                    setState(() {
                      _selectedSalon = null;
                      _showSearch = false;
                    });
                  },
                ),
                children: [
                  TileLayer(
                    urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                    userAgentPackageName: 'com.eutonafila.frontend',
                  ),
                  MarkerLayer(
                    markers: _salonLocations.map((location) {
                      return Marker(
                        point: location.position,
                        width: 80,
                        height: 80,
                        child: GestureDetector(
                          onTap: () {
                            setState(() {
                              _selectedSalon = location.salon;
                              _showSearch = false;
                            });
                          },
                          child: _buildMarker(location.salon, theme),
                        ),
                      );
                    }).toList(),
                  ),
                ],
              ),
            ),
            
            // Top Header
            _buildHeader(theme),
            
            // Search Overlay
            if (_showSearch) _buildSearchOverlay(theme),
            
            // Filter Chips
            if (!_showSearch) _buildFilterChips(theme),
            
            // Zoom Controls
            _buildZoomControls(theme),
            
            // My Location Button
            _buildMyLocationButton(theme),
            
            // Selected Salon Card
            if (_selectedSalon != null && !_showList) _buildSelectedSalonCard(theme),
            
            // Bottom Sheet List
            if (_showList) _buildBottomSheetList(theme),
          ],
        ),
      ),
      bottomNavigationBar: const BottomNavBar(currentIndex: 1),
    );
  }

  Widget _buildHeader(ThemeData theme) {
    return Container(
      margin: const EdgeInsets.all(16),
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
      decoration: BoxDecoration(
        color: theme.colorScheme.primary,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: theme.shadowColor.withOpacity(0.1),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        children: [
          GestureDetector(
            onTap: () => Navigator.of(context).pop(),
            child: Icon(
              Icons.arrow_back,
              color: theme.colorScheme.onPrimary,
              size: 24,
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(
                  'Encontrar um salão',
                  style: theme.textTheme.titleLarge?.copyWith(
                    color: theme.colorScheme.onPrimary,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 2),
                Row(
                  children: [
                    Icon(
                      Icons.location_on,
                      color: theme.colorScheme.onPrimary.withOpacity(0.8),
                      size: 16,
                    ),
                    const SizedBox(width: 4),
                    Text(
                      'San Antonio, FL',
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: theme.colorScheme.onPrimary.withOpacity(0.8),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              // Notifications button (only show when not searching)
              if (!_showSearch) ...[
                GestureDetector(
                  onTap: () {
                    Navigator.of(context).push(
                      MaterialPageRoute(builder: (_) => const NotificationsScreen()),
                    );
                  },
                  child: Container(
                    padding: const EdgeInsets.all(8),
                    decoration: BoxDecoration(
                      color: theme.colorScheme.onPrimary.withOpacity(0.2),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Icon(
                      Icons.notifications_outlined,
                      color: theme.colorScheme.onPrimary,
                      size: 20,
                    ),
                  ),
                ),
                const SizedBox(width: 8),
              ],
              // Search button
              GestureDetector(
                onTap: () {
                  setState(() {
                    _showSearch = !_showSearch;
                    if (_showSearch) {
                      _selectedSalon = null;
                    }
                  });
                },
                child: Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    color: _showSearch 
                        ? theme.colorScheme.onPrimary.withOpacity(0.3)
                        : theme.colorScheme.onPrimary.withOpacity(0.2),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Icon(
                    _showSearch ? Icons.close : Icons.search,
                    color: theme.colorScheme.onPrimary,
                    size: 20,
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildFilterChips(ThemeData theme) {
    return Positioned(
      top: 100,
      left: 16,
      right: 16,
      child: SingleChildScrollView(
        scrollDirection: Axis.horizontal,
        child: Row(
          children: [
            _buildFilterChip(theme, 'Ordenar', Icons.sort, isSelected: _selectedFilter == 'Ordenar'),
            const SizedBox(width: 8),
            _buildFilterChip(theme, 'Favoritos', Icons.favorite, isSelected: _selectedFilter == 'Favoritos'),
            const SizedBox(width: 8),
            _buildFilterChip(theme, 'Visitados recentemente', Icons.history, isSelected: _selectedFilter == 'Visitados recentemente'),
            const SizedBox(width: 8),
            _buildFilterChip(theme, 'Aberto agora', Icons.access_time, isSelected: _selectedFilter == 'Aberto agora'),
          ],
        ),
      ),
    );
  }

  Widget _buildFilterChip(ThemeData theme, String label, IconData icon, {required bool isSelected}) {
    return GestureDetector(
      onTap: () {
        setState(() {
          _selectedFilter = isSelected ? '' : label;
        });
      },
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        decoration: BoxDecoration(
          color: isSelected ? theme.colorScheme.primary : theme.colorScheme.surface,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(
            color: isSelected ? theme.colorScheme.primary : theme.colorScheme.outline,
            width: 1,
          ),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.05),
              blurRadius: 4,
              offset: const Offset(0, 1),
            ),
          ],
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              icon,
              size: 16,
              color: isSelected ? theme.colorScheme.onPrimary : theme.colorScheme.onSurfaceVariant,
            ),
            const SizedBox(width: 6),
            Text(
              label,
              style: theme.textTheme.labelMedium?.copyWith(
                color: isSelected ? theme.colorScheme.onPrimary : theme.colorScheme.onSurfaceVariant,
                fontWeight: FontWeight.w500,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildMyLocationButton(ThemeData theme) {
    return Positioned(
      bottom: _selectedSalon != null || _showList ? 220 : 100,
      right: 16,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          // List toggle button
          FloatingActionButton(
            heroTag: "list",
            onPressed: () {
              setState(() {
                _showList = !_showList;
                if (_showList) _selectedSalon = null;
              });
            },
            backgroundColor: _showList ? theme.colorScheme.primary : theme.colorScheme.surface,
            foregroundColor: _showList ? Colors.white : theme.colorScheme.primary,
            elevation: 4,
            mini: true,
            child: Icon(_showList ? Icons.map : Icons.list),
          ),
          const SizedBox(height: 8),
          // My location button
          FloatingActionButton(
            heroTag: "location",
            onPressed: () {
              // Animate to user location (mocked)
              _mapController.move(const LatLng(28.3372, -82.2637), 14.0);
            },
            backgroundColor: theme.colorScheme.surface,
            foregroundColor: theme.colorScheme.primary,
            elevation: 4,
            child: const Icon(Icons.my_location),
          ),
        ],
      ),
    );
  }

  Widget _buildMarker(Salon salon, ThemeData theme) {
    final isSelected = _selectedSalon?.name == salon.name;
    
    return Stack(
      alignment: Alignment.center,
      children: [
        // Marker background
        Container(
          width: isSelected ? 60 : 50,
          height: isSelected ? 60 : 50,
          decoration: BoxDecoration(
            color: salon.isOpen ? theme.colorScheme.primary : Colors.grey,
            shape: BoxShape.circle,
            border: Border.all(
              color: Colors.white,
              width: 3,
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
            size: isSelected ? 24 : 20,
          ),
        ),
        // Wait time badge
        if (salon.isOpen)
          Positioned(
            top: 0,
            right: 0,
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
              decoration: BoxDecoration(
                color: salon.waitTime <= 10 
                    ? Colors.green 
                    : salon.waitTime <= 25 
                        ? Colors.orange 
                        : Colors.red,
                borderRadius: BorderRadius.circular(10),
                border: Border.all(color: Colors.white, width: 1),
              ),
              child: Text(
                '${salon.waitTime}m',
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 10,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ),
      ],
    );
  }

  Widget _buildSelectedSalonCard(ThemeData theme) {
    return Positioned(
      bottom: 16,
      left: 16,
      right: 16,
      child: Container(
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          color: theme.colorScheme.surface,
          borderRadius: BorderRadius.circular(16),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.1),
              blurRadius: 12,
              offset: const Offset(0, 4),
            ),
          ],
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Container(
                  width: 50,
                  height: 50,
                  decoration: BoxDecoration(
                    color: theme.colorScheme.primary.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(25),
                  ),
                  child: Icon(
                    Icons.store,
                    color: theme.colorScheme.primary,
                    size: 24,
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Expanded(
                            child: Text(
                              _selectedSalon!.name,
                              style: theme.textTheme.titleLarge?.copyWith(
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                          ),
                          const SizedBox(width: 4),
                          GestureDetector(
                            onTap: () {
                              setState(() {
                                if (_favoriteSalons.contains(_selectedSalon!.name)) {
                                  _favoriteSalons.remove(_selectedSalon!.name);
                                } else {
                                  _favoriteSalons.add(_selectedSalon!.name);
                                }
                              });
                            },
                            child: Icon(
                              _favoriteSalons.contains(_selectedSalon!.name)
                                  ? Icons.favorite
                                  : Icons.favorite_border,
                              color: theme.colorScheme.primary,
                              size: 20,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 4),
                      Text(
                        _selectedSalon!.address,
                        style: theme.textTheme.bodyMedium?.copyWith(
                          color: theme.colorScheme.onSurfaceVariant,
                        ),
                      ),
                    ],
                  ),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                  decoration: BoxDecoration(
                    color: _selectedSalon!.waitTime <= 10 
                        ? Colors.green.withOpacity(0.1)
                        : _selectedSalon!.waitTime <= 25 
                            ? Colors.orange.withOpacity(0.1)
                            : Colors.red.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Column(
                    children: [
                      Text(
                        '${_selectedSalon!.waitTime} min',
                        style: theme.textTheme.titleMedium?.copyWith(
                          color: _selectedSalon!.waitTime <= 10 
                              ? Colors.green 
                              : _selectedSalon!.waitTime <= 25 
                                  ? Colors.orange 
                                  : Colors.red,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      Text(
                        'TEMPO EST.',
                        style: theme.textTheme.labelSmall?.copyWith(
                          color: theme.colorScheme.onSurfaceVariant,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: _selectedSalon!.isOpen 
                        ? Colors.green.withOpacity(0.1) 
                        : Colors.red.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Text(
                    _selectedSalon!.isOpen ? 'Aberto' : 'Fechado',
                    style: theme.textTheme.labelSmall?.copyWith(
                      color: _selectedSalon!.isOpen ? Colors.green : Colors.red,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                Text(
                  '• Fecha às ${_selectedSalon!.closingTime}',
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
                const SizedBox(width: 8),
                Icon(
                  Icons.directions_car,
                  size: 14,
                  color: theme.colorScheme.onSurfaceVariant,
                ),
                const SizedBox(width: 2),
                Text(
                  '${_selectedSalon!.distance.toStringAsFixed(1)} km',
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                icon: Icon(Icons.check_circle, color: theme.colorScheme.onPrimary),
                label: const Text('Check In'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: theme.colorScheme.primary,
                  foregroundColor: theme.colorScheme.onPrimary,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(24),
                  ),
                  textStyle: theme.textTheme.labelLarge?.copyWith(fontWeight: FontWeight.bold),
                  padding: const EdgeInsets.symmetric(vertical: 12),
                ),
                onPressed: _selectedSalon!.isOpen ? () {
                  Navigator.of(context).push(
                    MaterialPageRoute(
                      builder: (_) => CheckInScreen(salon: _selectedSalon!),
                    ),
                  );
                } : null,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSearchOverlay(ThemeData theme) {
    return Positioned(
      top: 100,
      left: 16,
      right: 16,
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: theme.colorScheme.surface,
          borderRadius: BorderRadius.circular(16),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.1),
              blurRadius: 8,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            TextField(
              controller: _searchController,
              autofocus: true,
              decoration: InputDecoration(
                hintText: 'Buscar localização...',
                prefixIcon: Icon(Icons.search, color: theme.colorScheme.primary),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
                focusedBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: BorderSide(color: theme.colorScheme.primary, width: 2),
                ),
                contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
              ),
              onSubmitted: (value) {
                _performSearch(value, theme);
              },
            ),
            const SizedBox(height: 12),
            // Quick search suggestions
            Wrap(
              spacing: 8,
              children: [
                _buildSearchChip(theme, 'San Antonio, FL'),
                _buildSearchChip(theme, 'Tampa, FL'),
                _buildSearchChip(theme, 'Wesley Chapel, FL'),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSearchChip(ThemeData theme, String location) {
    return GestureDetector(
      onTap: () => _performSearch(location, theme),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
        decoration: BoxDecoration(
          color: theme.colorScheme.primary.withOpacity(0.1),
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: theme.colorScheme.primary.withOpacity(0.3)),
        ),
        child: Text(
          location,
          style: theme.textTheme.labelMedium?.copyWith(
            color: theme.colorScheme.primary,
            fontWeight: FontWeight.w500,
          ),
        ),
      ),
    );
  }

  void _performSearch(String query, ThemeData theme) {
    // Mock search functionality - in real app, this would geocode the address
    setState(() {
      _showSearch = false;
      _searchController.clear();
    });
    
    // Animate to a mock location based on search
    if (query.toLowerCase().contains('tampa')) {
      _mapController.move(const LatLng(27.9506, -82.4572), 12.0);
    } else if (query.toLowerCase().contains('wesley chapel')) {
      _mapController.move(const LatLng(28.2420, -82.3271), 12.0);
    } else {
      // Default to San Antonio, FL
      _mapController.move(const LatLng(28.3372, -82.2637), 12.0);
    }
    
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Buscando por: $query'),
        backgroundColor: theme.colorScheme.primary,
        behavior: SnackBarBehavior.floating,
        duration: const Duration(seconds: 2),
      ),
    );
  }

  Widget _buildZoomControls(ThemeData theme) {
    return Positioned(
      top: MediaQuery.of(context).size.height * 0.4,
      right: 16,
      child: Column(
        children: [
          Container(
            decoration: BoxDecoration(
              color: theme.colorScheme.surface,
              borderRadius: BorderRadius.circular(8),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withOpacity(0.1),
                  blurRadius: 4,
                  offset: const Offset(0, 2),
                ),
              ],
            ),
            child: Column(
              children: [
                GestureDetector(
                  onTap: () {
                    final currentZoom = _mapController.camera.zoom;
                    _mapController.move(_mapController.camera.center, currentZoom + 1);
                  },
                  child: Container(
                    width: 40,
                    height: 40,
                    decoration: BoxDecoration(
                      borderRadius: const BorderRadius.vertical(top: Radius.circular(8)),
                      border: Border(
                        bottom: BorderSide(color: theme.colorScheme.outline.withOpacity(0.2)),
                      ),
                    ),
                    child: Icon(
                      Icons.add,
                      color: theme.colorScheme.primary,
                      size: 20,
                    ),
                  ),
                ),
                GestureDetector(
                  onTap: () {
                    final currentZoom = _mapController.camera.zoom;
                    _mapController.move(_mapController.camera.center, currentZoom - 1);
                  },
                  child: Container(
                    width: 40,
                    height: 40,
                    decoration: const BoxDecoration(
                      borderRadius: BorderRadius.vertical(bottom: Radius.circular(8)),
                    ),
                    child: Icon(
                      Icons.remove,
                      color: theme.colorScheme.primary,
                      size: 20,
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildBottomSheetList(ThemeData theme) {
    return Positioned(
      bottom: 0,
      left: 0,
      right: 0,
      child: Container(
        height: MediaQuery.of(context).size.height * 0.6,
        decoration: BoxDecoration(
          color: theme.colorScheme.surface,
          borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.1),
              blurRadius: 12,
              offset: const Offset(0, -4),
            ),
          ],
        ),
        child: Column(
          children: [
            // Handle bar
            Container(
              margin: const EdgeInsets.only(top: 12),
              width: 40,
              height: 4,
              decoration: BoxDecoration(
                color: theme.colorScheme.onSurfaceVariant.withOpacity(0.3),
                borderRadius: BorderRadius.circular(2),
              ),
            ),
            // Header
            Padding(
              padding: const EdgeInsets.all(20),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    '${_salonLocations.length} salões encontrados',
                    style: theme.textTheme.titleLarge?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  GestureDetector(
                    onTap: () {
                      setState(() {
                        _showList = false;
                      });
                    },
                    child: Icon(
                      Icons.close,
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                  ),
                ],
              ),
            ),
            // List
            Expanded(
              child: ListView.builder(
                padding: const EdgeInsets.symmetric(horizontal: 20),
                itemCount: _salonLocations.length,
                itemBuilder: (context, index) {
                  final location = _salonLocations[index];
                   final isCheckedIn = location.salon.name == 'Market at Mirada';
                   return Container(
                     margin: const EdgeInsets.only(bottom: 16),
                     padding: const EdgeInsets.all(16),
                     decoration: BoxDecoration(
                       color: theme.colorScheme.surface,
                       borderRadius: BorderRadius.circular(12),
                       border: Border.all(
                         color: isCheckedIn 
                             ? theme.colorScheme.primary.withOpacity(0.3)
                             : theme.colorScheme.outline.withOpacity(0.2),
                       ),
                     ),
                    child: Row(
                      children: [
                        Container(
                          width: 50,
                          height: 50,
                          decoration: BoxDecoration(
                            color: location.salon.isOpen 
                                ? theme.colorScheme.primary.withOpacity(0.1)
                                : Colors.grey.withOpacity(0.1),
                            borderRadius: BorderRadius.circular(25),
                          ),
                          child: Icon(
                            Icons.store,
                            color: location.salon.isOpen 
                                ? theme.colorScheme.primary 
                                : Colors.grey,
                            size: 24,
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                                                     child: Column(
                             crossAxisAlignment: CrossAxisAlignment.start,
                             children: [
                               Row(
                                 children: [
                                   Expanded(
                                     child: Text(
                                       location.salon.name,
                                       style: theme.textTheme.titleMedium?.copyWith(
                                         fontWeight: FontWeight.bold,
                                       ),
                                     ),
                                   ),
                                   if (isCheckedIn)
                                     Container(
                                       padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                                       decoration: BoxDecoration(
                                         color: theme.colorScheme.primary,
                                         borderRadius: BorderRadius.circular(8),
                                       ),
                                       child: Text(
                                         'CHECKED IN',
                                         style: theme.textTheme.labelSmall?.copyWith(
                                           color: Colors.white,
                                           fontWeight: FontWeight.bold,
                                           fontSize: 10,
                                         ),
                                       ),
                                     ),
                                   const SizedBox(width: 4),
                                   GestureDetector(
                                     onTap: () {
                                       setState(() {
                                         if (_favoriteSalons.contains(location.salon.name)) {
                                           _favoriteSalons.remove(location.salon.name);
                                         } else {
                                           _favoriteSalons.add(location.salon.name);
                                         }
                                       });
                                     },
                                     child: Icon(
                                       _favoriteSalons.contains(location.salon.name)
                                           ? Icons.favorite
                                           : Icons.favorite_border,
                                       color: theme.colorScheme.primary,
                                       size: 20,
                                     ),
                                   ),
                                 ],
                               ),
                              const SizedBox(height: 4),
                              Text(
                                location.salon.address,
                                style: theme.textTheme.bodySmall?.copyWith(
                                  color: theme.colorScheme.onSurfaceVariant,
                                ),
                              ),
                              const SizedBox(height: 8),
                              Row(
                                children: [
                                  Container(
                                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                    decoration: BoxDecoration(
                                      color: location.salon.isOpen 
                                          ? Colors.green.withOpacity(0.1) 
                                          : Colors.red.withOpacity(0.1),
                                      borderRadius: BorderRadius.circular(8),
                                    ),
                                    child: Text(
                                      location.salon.isOpen ? 'Aberto' : 'Fechado',
                                      style: theme.textTheme.labelSmall?.copyWith(
                                        color: location.salon.isOpen ? Colors.green : Colors.red,
                                        fontWeight: FontWeight.bold,
                                      ),
                                    ),
                                  ),
                                  const SizedBox(width: 8),
                                  Text(
                                    '${location.salon.distance.toStringAsFixed(1)} km',
                                    style: theme.textTheme.bodySmall?.copyWith(
                                      color: theme.colorScheme.onSurfaceVariant,
                                    ),
                                  ),
                                ],
                              ),
                            ],
                          ),
                        ),
                        Column(
                          children: [
                            Container(
                              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                              decoration: BoxDecoration(
                                color: location.salon.waitTime <= 10 
                                    ? Colors.green.withOpacity(0.1)
                                    : location.salon.waitTime <= 25 
                                        ? Colors.orange.withOpacity(0.1)
                                        : Colors.red.withOpacity(0.1),
                                borderRadius: BorderRadius.circular(12),
                              ),
                              child: Column(
                                children: [
                                  Text(
                                    '${location.salon.waitTime} min',
                                    style: theme.textTheme.titleSmall?.copyWith(
                                      color: location.salon.waitTime <= 10 
                                          ? Colors.green 
                                          : location.salon.waitTime <= 25 
                                              ? Colors.orange 
                                              : Colors.red,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                  Text(
                                    'ESPERA',
                                    style: theme.textTheme.labelSmall?.copyWith(
                                      color: theme.colorScheme.onSurfaceVariant,
                                      fontWeight: FontWeight.w500,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                            const SizedBox(height: 8),
                            SizedBox(
                              width: 80,
                              child: ElevatedButton(
                                onPressed: location.salon.isOpen ? () {
                                  Navigator.of(context).push(
                                    MaterialPageRoute(
                                      builder: (_) => CheckInScreen(salon: location.salon),
                                    ),
                                  );
                                } : null,
                                style: ElevatedButton.styleFrom(
                                  backgroundColor: theme.colorScheme.primary,
                                  foregroundColor: Colors.white,
                                  shape: RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(20),
                                  ),
                                  padding: const EdgeInsets.symmetric(vertical: 8),
                                ),
                                child: const Text(
                                  'Check In',
                                  style: TextStyle(fontSize: 10),
                                ),
                              ),
                            ),
                          ],
                        ),
                      ],
                    ),
                  );
                },
              ),
            ),
          ],
        ),
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