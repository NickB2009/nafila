import 'package:flutter/material.dart';
import '../theme/app_theme.dart';
import 'dart:async';

class SalonTvDashboard extends StatefulWidget {
  const SalonTvDashboard({super.key});

  @override
  State<SalonTvDashboard> createState() => _SalonTvDashboardState();
}

class _SalonTvDashboardState extends State<SalonTvDashboard> {
  Timer? _timer;
  int _currentAdIndex = 0;
  
  // Mock data for the salon
  final String salonName = "Great Clips";
  final int currentWaitTime = 37;
  final int customersWaiting = 5;
  
  // Mock customer queue
  final List<Map<String, dynamic>> customerQueue = [
    {"name": "David M.", "position": 1, "inSalon": false},
    {"name": "Rommel B.", "position": 2, "inSalon": false},
    {"name": "Wendy S.", "position": 3, "inSalon": true},
    {"name": "Tatiana T.", "position": 4, "inSalon": false},
    {"name": "Michael L.", "position": 5, "inSalon": false},
  ];
  
  // Mock ads data
  final List<Map<String, dynamic>> ads = [
    {
      "title": "Seu estilo, em arquivo.",
      "subtitle": "Salvamos os detalhes do seu corte\npara que você tenha, sempre.",
      "brand": "CLIP notes",
      "image": "assets/images/clip_notes_ad.jpg",
      "backgroundColor": AppTheme.primaryColor,
    },
    {
      "title": "Produtos Premium",
      "subtitle": "Os melhores produtos para\nseu cabelo e barba.",
      "brand": "SALON PRO",
      "image": "assets/images/products_ad.jpg",
      "backgroundColor": Colors.deepOrange,
    },
    {
      "title": "Agende Online",
      "subtitle": "Use nosso app para\nagendar seu próximo corte.",
      "brand": "EUTONAFILA",
      "image": "assets/images/app_ad.jpg",
      "backgroundColor": Colors.indigo,
    },
  ];

  @override
  void initState() {
    super.initState();
    _startAdRotation();
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  void _startAdRotation() {
    _timer = Timer.periodic(const Duration(seconds: 8), (timer) {
      setState(() {
        _currentAdIndex = (_currentAdIndex + 1) % ads.length;
      });
    });
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final screenSize = MediaQuery.of(context).size;
    
    return Scaffold(
      backgroundColor: theme.colorScheme.surface,
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Row(
            children: [
              // Left Side - Queue Information
              Expanded(
                flex: 1,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Salon Header
                    _buildSalonHeader(theme),
                    
                    const SizedBox(height: 20),
                    
                    // Wait Time Card
                    _buildWaitTimeCard(theme),
                    
                    const SizedBox(height: 20),
                    
                    // Customer Queue
                    Expanded(
                      child: _buildCustomerQueue(theme),
                    ),
                  ],
                ),
              ),
              
              const SizedBox(width: 32),
              
              // Right Side - Advertisement
              Expanded(
                flex: 1,
                child: _buildAdvertisementSection(theme),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildSalonHeader(ThemeData theme) {
    return Container(
      padding: const EdgeInsets.all(16),
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
          Container(
            width: 60,
            height: 60,
            decoration: BoxDecoration(
              color: theme.colorScheme.primary.withOpacity(0.1),
              borderRadius: BorderRadius.circular(30),
            ),
            child: Icon(
              Icons.content_cut,
              color: theme.colorScheme.primary,
              size: 32,
            ),
          ),
          const SizedBox(width: 20),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  salonName,
                  style: theme.textTheme.headlineMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                    fontSize: 24,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  'Painel de Fila',
                  style: theme.textTheme.titleMedium?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                    fontSize: 14,
                  ),
                ),
              ],
            ),
          ),
          // Current time
          Text(
            _getCurrentTime(),
            style: theme.textTheme.headlineSmall?.copyWith(
              fontWeight: FontWeight.w500,
              color: theme.colorScheme.primary,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildWaitTimeCard(ThemeData theme) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: theme.colorScheme.primary,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: theme.colorScheme.primary.withOpacity(0.3),
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
                  'TEMPO ESTIMADO ATUAL',
                  style: theme.textTheme.labelLarge?.copyWith(
                    color: Colors.white.withOpacity(0.9),
                    fontWeight: FontWeight.w600,
                    letterSpacing: 1.2,
                  ),
                ),
                const SizedBox(height: 8),
                Row(
                  crossAxisAlignment: CrossAxisAlignment.baseline,
                  textBaseline: TextBaseline.alphabetic,
                  children: [
                    Text(
                      '$currentWaitTime',
                      style: theme.textTheme.displayMedium?.copyWith(
                        color: Colors.white,
                        fontWeight: FontWeight.bold,
                        fontSize: 48,
                      ),
                    ),
                    const SizedBox(width: 6),
                    Text(
                      'MIN.',
                      style: theme.textTheme.titleLarge?.copyWith(
                        color: Colors.white.withOpacity(0.9),
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          const SizedBox(width: 20),
          Column(
            children: [
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: Colors.white.withOpacity(0.2),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: const Icon(
                  Icons.people,
                  color: Colors.white,
                  size: 32,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                '$customersWaiting',
                style: theme.textTheme.headlineSmall?.copyWith(
                  color: Colors.white,
                  fontWeight: FontWeight.bold,
                ),
              ),
              Text(
                'na fila',
                style: theme.textTheme.labelMedium?.copyWith(
                  color: Colors.white.withOpacity(0.8),
                  fontWeight: FontWeight.w500,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildCustomerQueue(ThemeData theme) {
    return Container(
      padding: const EdgeInsets.all(24),
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
          Text(
            'FILA DE CLIENTES',
            style: theme.textTheme.titleLarge?.copyWith(
              fontWeight: FontWeight.bold,
              letterSpacing: 0.5,
            ),
          ),
          const SizedBox(height: 24),
          
          // Queue header
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: Row(
              children: [
                Expanded(
                  flex: 1,
                  child: Text(
                    'Nº.',
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                  ),
                ),
                Expanded(
                  flex: 4,
                  child: Text(
                    'CLIENTE',
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                  ),
                ),
                Expanded(
                  flex: 2,
                  child: Text(
                    'NO SALÃO',
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                    textAlign: TextAlign.center,
                  ),
                ),
              ],
            ),
          ),
          
          const SizedBox(height: 16),
          Divider(color: theme.dividerColor, thickness: 2),
          const SizedBox(height: 16),
          
          // Queue list
          Expanded(
            child: ListView.builder(
              itemCount: customerQueue.length,
              itemBuilder: (context, index) {
                final customer = customerQueue[index];
                return _buildCustomerRow(theme, customer, index);
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildCustomerRow(ThemeData theme, Map<String, dynamic> customer, int index) {
    final bool isInSalon = customer["inSalon"] ?? false;
    
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
      decoration: BoxDecoration(
        color: isInSalon 
            ? theme.colorScheme.primary.withOpacity(0.1)
            : theme.colorScheme.surface,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: isInSalon 
              ? theme.colorScheme.primary.withOpacity(0.3)
              : theme.colorScheme.outline.withOpacity(0.2),
        ),
      ),
      child: Row(
        children: [
          Expanded(
            flex: 1,
            child: Text(
              '${customer["position"]}.',
              style: theme.textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.bold,
                fontSize: 20,
              ),
            ),
          ),
          Expanded(
            flex: 4,
            child: Text(
              customer["name"],
              style: theme.textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.w600,
                fontSize: 20,
              ),
            ),
          ),
          Expanded(
            flex: 2,
            child: Center(
              child: isInSalon
                  ? const Icon(
                      Icons.check_circle,
                      color: AppTheme.primaryColor,
                      size: 28,
                    )
                  : const SizedBox.shrink(),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildAdvertisementSection(ThemeData theme) {
    final currentAd = ads[_currentAdIndex];
    
    return AnimatedContainer(
      duration: const Duration(milliseconds: 500),
      padding: const EdgeInsets.all(32),
      decoration: BoxDecoration(
        color: currentAd["backgroundColor"],
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: currentAd["backgroundColor"].withOpacity(0.3),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          // Ad content
          Expanded(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                // Mock image placeholder
                Container(
                  width: 120,
                  height: 120,
                  decoration: BoxDecoration(
                    color: Colors.white.withOpacity(0.2),
                    borderRadius: BorderRadius.circular(60),
                  ),
                  child: Icon(
                    _getAdIcon(_currentAdIndex),
                    size: 60,
                    color: Colors.white,
                  ),
                ),
                
                const SizedBox(height: 24),
                
                Text(
                  currentAd["title"],
                  style: theme.textTheme.headlineMedium?.copyWith(
                    color: Colors.white,
                    fontWeight: FontWeight.bold,
                    fontSize: 28,
                  ),
                  textAlign: TextAlign.center,
                ),
                
                const SizedBox(height: 16),
                
                Text(
                  currentAd["subtitle"],
                  style: theme.textTheme.titleLarge?.copyWith(
                    color: Colors.white.withOpacity(0.9),
                    fontSize: 18,
                    height: 1.4,
                  ),
                  textAlign: TextAlign.center,
                ),
              ],
            ),
          ),
          
          // Brand logo/name
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
            decoration: BoxDecoration(
              color: Colors.white.withOpacity(0.2),
              borderRadius: BorderRadius.circular(25),
            ),
            child: Text(
              currentAd["brand"],
              style: theme.textTheme.titleMedium?.copyWith(
                color: Colors.white,
                fontWeight: FontWeight.bold,
                letterSpacing: 1.2,
              ),
            ),
          ),
          
          const SizedBox(height: 16),
          
          // Ad rotation indicator
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: List.generate(
              ads.length,
              (index) => Container(
                margin: const EdgeInsets.symmetric(horizontal: 4),
                width: 8,
                height: 8,
                decoration: BoxDecoration(
                  color: index == _currentAdIndex 
                      ? Colors.white 
                      : Colors.white.withOpacity(0.4),
                  shape: BoxShape.circle,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  IconData _getAdIcon(int index) {
    switch (index) {
      case 0:
        return Icons.description;
      case 1:
        return Icons.shopping_bag;
      case 2:
        return Icons.phone_android;
      default:
        return Icons.star;
    }
  }

  String _getCurrentTime() {
    final now = DateTime.now();
    return '${now.hour.toString().padLeft(2, '0')}:${now.minute.toString().padLeft(2, '0')}';
  }
} 