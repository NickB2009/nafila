import 'package:flutter/material.dart';
import '../../utils/brazilian_names_generator.dart';
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
          {"name": BrazilianNamesGenerator.generateNameWithInitial(), "position": 2, "inSalon": false},
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
    final isSmallScreen = screenSize.width < 600;
    
    return Scaffold(
      backgroundColor: theme.colorScheme.surface,
      body: SafeArea(
        child: LayoutBuilder(
          builder: (context, constraints) {
            return SingleChildScrollView(
              child: Padding(
                padding: EdgeInsets.all(constraints.maxWidth * 0.03),
                child: isSmallScreen
                    ? Column(
                        children: [
                          // Queue Information
                          _buildSalonHeader(theme, constraints),
                          SizedBox(height: constraints.maxHeight * 0.02),
                          _buildWaitTimeCard(theme, constraints),
                          SizedBox(height: constraints.maxHeight * 0.02),
                          SizedBox(
                            height: constraints.maxHeight * 0.4,
                            child: _buildCustomerQueue(theme, constraints),
                          ),
                          SizedBox(height: constraints.maxHeight * 0.02),
                          // Advertisement
                          SizedBox(
                            height: constraints.maxHeight * 0.4,
                            child: _buildAdvertisementSection(theme, constraints),
                          ),
                        ],
                      )
                    : Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          // Left Side - Queue Information
                          Expanded(
                            flex: 1,
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                _buildSalonHeader(theme, constraints),
                                SizedBox(height: constraints.maxHeight * 0.02),
                                _buildWaitTimeCard(theme, constraints),
                                SizedBox(height: constraints.maxHeight * 0.02),
                                Expanded(
                                  child: _buildCustomerQueue(theme, constraints),
                                ),
                              ],
                            ),
                          ),
                          SizedBox(width: constraints.maxWidth * 0.03),
                          // Right Side - Advertisement
                          Expanded(
                            flex: 1,
                            child: _buildAdvertisementSection(theme, constraints),
                          ),
                        ],
                      ),
              ),
            );
          },
        ),
      ),
    );
  }

  Widget _buildSalonHeader(ThemeData theme, BoxConstraints constraints) {
    final isSmallScreen = constraints.maxWidth < 600;
    final iconSize = isSmallScreen ? 40.0 : 60.0;
    final titleFontSize = isSmallScreen ? 20.0 : 24.0;
    final subtitleFontSize = isSmallScreen ? 12.0 : 14.0;
    
    return Container(
      padding: EdgeInsets.all(constraints.maxWidth * 0.02),
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
            width: iconSize,
            height: iconSize,
            decoration: BoxDecoration(
              color: theme.colorScheme.primary.withOpacity(0.1),
              borderRadius: BorderRadius.circular(iconSize / 2),
            ),
            child: Icon(
              Icons.content_cut,
              color: theme.colorScheme.primary,
              size: iconSize * 0.6,
            ),
          ),
          SizedBox(width: constraints.maxWidth * 0.02),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  salonName,
                  style: theme.textTheme.headlineMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                    fontSize: titleFontSize,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  'Painel de Fila',
                  style: theme.textTheme.titleMedium?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                    fontSize: subtitleFontSize,
                  ),
                ),
              ],
            ),
          ),
          Text(
            _getCurrentTime(),
            style: theme.textTheme.headlineSmall?.copyWith(
              fontWeight: FontWeight.w500,
              color: theme.colorScheme.primary,
              fontSize: isSmallScreen ? 18.0 : 24.0,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildWaitTimeCard(ThemeData theme, BoxConstraints constraints) {
    final isSmallScreen = constraints.maxWidth < 600;
    final waitTimeFontSize = isSmallScreen ? 36.0 : 48.0;
    
    return Container(
      padding: EdgeInsets.all(constraints.maxWidth * 0.02),
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
                    fontSize: isSmallScreen ? 10.0 : 12.0,
                  ),
                ),
                SizedBox(height: constraints.maxHeight * 0.01),
                Row(
                  crossAxisAlignment: CrossAxisAlignment.baseline,
                  textBaseline: TextBaseline.alphabetic,
                  children: [
                    Text(
                      '$currentWaitTime',
                      style: theme.textTheme.displayMedium?.copyWith(
                        color: Colors.white,
                        fontWeight: FontWeight.bold,
                        fontSize: waitTimeFontSize,
                      ),
                    ),
                    const SizedBox(width: 6),
                    Text(
                      'MIN.',
                      style: theme.textTheme.titleLarge?.copyWith(
                        color: Colors.white.withOpacity(0.9),
                        fontWeight: FontWeight.w600,
                        fontSize: isSmallScreen ? 16.0 : 20.0,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          SizedBox(width: constraints.maxWidth * 0.02),
          Column(
            children: [
              Container(
                padding: EdgeInsets.all(constraints.maxWidth * 0.015),
                decoration: BoxDecoration(
                  color: Colors.white.withOpacity(0.2),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Icon(
                  Icons.people,
                  color: Colors.white,
                  size: isSmallScreen ? 24.0 : 32.0,
                ),
              ),
              SizedBox(height: constraints.maxHeight * 0.01),
              Text(
                '$customersWaiting',
                style: theme.textTheme.headlineSmall?.copyWith(
                  color: Colors.white,
                  fontWeight: FontWeight.bold,
                  fontSize: isSmallScreen ? 20.0 : 24.0,
                ),
              ),
              Text(
                'na fila',
                style: theme.textTheme.labelMedium?.copyWith(
                  color: Colors.white.withOpacity(0.8),
                  fontWeight: FontWeight.w500,
                  fontSize: isSmallScreen ? 10.0 : 12.0,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildCustomerQueue(ThemeData theme, BoxConstraints constraints) {
    final isSmallScreen = constraints.maxWidth < 600;
    final headerFontSize = isSmallScreen ? 16.0 : 20.0;
    final rowFontSize = isSmallScreen ? 16.0 : 20.0;
    
    return Container(
      padding: EdgeInsets.all(constraints.maxWidth * 0.02),
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
              fontSize: headerFontSize,
            ),
          ),
          SizedBox(height: constraints.maxHeight * 0.02),
          
          // Queue header
          Padding(
            padding: EdgeInsets.symmetric(horizontal: constraints.maxWidth * 0.02),
            child: Row(
              children: [
                Expanded(
                  flex: 1,
                  child: Text(
                    'Nº.',
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: theme.colorScheme.onSurfaceVariant,
                      fontSize: headerFontSize * 0.8,
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
                      fontSize: headerFontSize * 0.8,
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
                      fontSize: headerFontSize * 0.8,
                    ),
                    textAlign: TextAlign.center,
                  ),
                ),
              ],
            ),
          ),
          
          SizedBox(height: constraints.maxHeight * 0.01),
          Divider(color: theme.dividerColor, thickness: 2),
          SizedBox(height: constraints.maxHeight * 0.01),
          
          // Queue list
          Expanded(
            child: ListView.builder(
              itemCount: customerQueue.length,
              itemBuilder: (context, index) {
                final customer = customerQueue[index];
                return _buildCustomerRow(theme, customer, index, constraints);
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildCustomerRow(ThemeData theme, Map<String, dynamic> customer, int index, BoxConstraints constraints) {
    final bool isInSalon = customer["inSalon"] ?? false;
    final isSmallScreen = constraints.maxWidth < 600;
    final fontSize = isSmallScreen ? 16.0 : 20.0;
    
    return Container(
      margin: EdgeInsets.only(bottom: constraints.maxHeight * 0.01),
      padding: EdgeInsets.symmetric(
        horizontal: constraints.maxWidth * 0.02,
        vertical: constraints.maxHeight * 0.015,
      ),
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
                fontSize: fontSize,
              ),
            ),
          ),
          Expanded(
            flex: 4,
            child: Text(
              customer["name"],
              style: theme.textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.w600,
                fontSize: fontSize,
              ),
            ),
          ),
          Expanded(
            flex: 2,
            child: Center(
              child: isInSalon
                  ? Icon(
                      Icons.check_circle,
                      color: AppTheme.primaryColor,
                      size: isSmallScreen ? 20 : 28,
                    )
                  : const SizedBox.shrink(),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildAdvertisementSection(ThemeData theme, BoxConstraints constraints) {
    final currentAd = ads[_currentAdIndex];
    final isSmallScreen = constraints.maxWidth < 600;
    final iconSize = isSmallScreen ? 80.0 : 120.0;
    final titleFontSize = isSmallScreen ? 24.0 : 28.0;
    final subtitleFontSize = isSmallScreen ? 14.0 : 18.0;
    
    return AnimatedContainer(
      duration: const Duration(milliseconds: 500),
      padding: EdgeInsets.all(constraints.maxWidth * 0.03),
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
                  width: iconSize,
                  height: iconSize,
                  decoration: BoxDecoration(
                    color: Colors.white.withOpacity(0.2),
                    borderRadius: BorderRadius.circular(iconSize / 2),
                  ),
                  child: Icon(
                    _getAdIcon(_currentAdIndex),
                    size: iconSize / 2,
                    color: Colors.white,
                  ),
                ),
                
                SizedBox(height: constraints.maxHeight * 0.02),
                
                Text(
                  currentAd["title"],
                  style: theme.textTheme.headlineMedium?.copyWith(
                    color: Colors.white,
                    fontWeight: FontWeight.bold,
                    fontSize: titleFontSize,
                  ),
                  textAlign: TextAlign.center,
                ),
                
                SizedBox(height: constraints.maxHeight * 0.015),
                
                Text(
                  currentAd["subtitle"],
                  style: theme.textTheme.titleLarge?.copyWith(
                    color: Colors.white.withOpacity(0.9),
                    fontSize: subtitleFontSize,
                    height: 1.4,
                  ),
                  textAlign: TextAlign.center,
                ),
              ],
            ),
          ),
          
          // Brand logo/name
          Container(
            padding: EdgeInsets.symmetric(
              horizontal: constraints.maxWidth * 0.03,
              vertical: constraints.maxHeight * 0.015,
            ),
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
                fontSize: isSmallScreen ? 12.0 : 14.0,
              ),
            ),
          ),
          
          SizedBox(height: constraints.maxHeight * 0.015),
          
          // Ad rotation indicator
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: List.generate(
              ads.length,
              (index) => Container(
                margin: const EdgeInsets.symmetric(horizontal: 4),
                width: isSmallScreen ? 6 : 8,
                height: isSmallScreen ? 6 : 8,
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