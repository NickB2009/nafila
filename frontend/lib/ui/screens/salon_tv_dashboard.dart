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
  bool? isAdLarge; // User toggle for ad size
  
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

  // Add state for custom ad size
  double? customAdWidth;
  double? customAdHeight;

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
    final minLeftWidth = 260.0;
    final minAdWidth = 250.0;
    final maxAdWidth = screenSize.width - minLeftWidth - 64.0;
    final isLargeScreen = screenSize.width > 900;
    final defaultAdWidth = isLargeScreen ? 700.0 : 340.0;
    final defaultAdHeight = isLargeScreen ? 480.0 : 180.0;
    final adWidth = (customAdWidth ?? defaultAdWidth).clamp(minAdWidth, maxAdWidth);
    final adHeight = customAdHeight ?? defaultAdHeight;
    final leftWidth = (screenSize.width - adWidth - 64.0).clamp(minLeftWidth, screenSize.width * 0.7);
    final adFlex = (adWidth / screenSize.width * 100).clamp(20, 80).round();
    final leftFlex = 100 - adFlex;
    final showCompactLeft = leftWidth <= minLeftWidth + 20;

    return Scaffold(
      backgroundColor: theme.colorScheme.surface,
      body: SafeArea(
        child: LayoutBuilder(
          builder: (context, constraints) {
            if (isSmallScreen) {
              return SingleChildScrollView(
                child: Padding(
                  padding: EdgeInsets.all(constraints.maxWidth * 0.03),
                  child: Column(
                    children: [
                      _buildSalonHeader(theme, constraints),
                      SizedBox(height: constraints.maxHeight * 0.02),
                      _buildWaitTimeCard(theme, constraints),
                      SizedBox(height: constraints.maxHeight * 0.02),
                      _buildCustomerQueue(theme, constraints),
                      SizedBox(height: constraints.maxHeight * 0.02),
                      _buildAdvertisementSection(theme, constraints, forceNoExpanded: true),
                    ],
                  ),
                ),
              );
            } else {
              return SizedBox.expand(
                child: Padding(
                  padding: const EdgeInsets.all(32.0),
                  child: Stack(
                    children: [
                      // Left content fills up to the ad's left edge
                      AnimatedPositioned(
                        duration: const Duration(milliseconds: 300),
                        curve: Curves.easeInOut,
                        left: 0,
                        top: 0,
                        bottom: 0,
                        right: adWidth + 24, // 24px gap between left and ad
                        child: Container(
                          decoration: BoxDecoration(
                            gradient: LinearGradient(
                              colors: [theme.colorScheme.surface, theme.colorScheme.primary.withOpacity(0.04)],
                              begin: Alignment.topLeft,
                              end: Alignment.bottomRight,
                            ),
                            borderRadius: BorderRadius.circular(24),
                            boxShadow: [
                              BoxShadow(
                                color: Colors.black.withOpacity(0.06),
                                blurRadius: 16,
                                offset: const Offset(0, 4),
                              ),
                            ],
                          ),
                          padding: const EdgeInsets.all(24.0),
                          child: showCompactLeft
                              ? _buildCompactLeft(theme, constraints)
                              : Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    _buildSalonHeader(theme, constraints),
                                    SizedBox(height: 24),
                                    _buildWaitTimeCard(theme, constraints),
                                    SizedBox(height: 24),
                                    Expanded(child: _buildCustomerQueue(theme, constraints)),
                                  ],
                                ),
                        ),
                      ),
                      // Ad absolutely positioned to the right
                      Positioned(
                        top: 0,
                        bottom: 0,
                        right: 0,
                        width: adWidth,
                        child: _buildAdvertisementSection(
                          theme,
                          constraints,
                          forceNoExpanded: false,
                          adWidth: adWidth,
                          adHeight: adHeight,
                          minAdWidth: minAdWidth,
                          maxAdWidth: maxAdWidth,
                          onResize: (w, h) {
                            setState(() {
                              customAdWidth = w;
                              customAdHeight = h;
                            });
                          },
                        ),
                      ),
                    ],
                  ),
                ),
              );
            }
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
          // Queue list - let it size naturally for small screens
          if (isSmallScreen)
            ...customerQueue.asMap().entries.map((entry) {
              final index = entry.key;
              final customer = entry.value;
              return _buildCustomerRow(theme, customer, index, constraints);
            }).toList()
          else
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

  // Compact left content for very wide ad
  Widget _buildCompactLeft(ThemeData theme, BoxConstraints constraints) {
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.content_cut, color: theme.colorScheme.primary, size: 32),
            SizedBox(width: 12),
            Text(salonName, style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
          ],
        ),
        SizedBox(height: 18),
        Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.timer, color: theme.colorScheme.primary, size: 28),
            SizedBox(width: 8),
            Text('$currentWaitTime min', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w600)),
            SizedBox(width: 16),
            Icon(Icons.people, color: theme.colorScheme.primary, size: 28),
            SizedBox(width: 8),
            Text('$customersWaiting', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w600)),
          ],
        ),
      ],
    );
  }

  // Update _buildAdvertisementSection to accept adWidth, adHeight, minAdWidth, maxAdWidth, and onResize
  Widget _buildAdvertisementSection(
    ThemeData theme,
    BoxConstraints constraints, {
    bool forceNoExpanded = false,
    double? adWidth,
    double? adHeight,
    double? minAdWidth,
    double? maxAdWidth,
    void Function(double, double)? onResize,
  }) {
    final isLargeScreen = constraints.maxWidth > 900;
    final defaultAdWidth = isLargeScreen ? 700.0 : 340.0;
    final defaultAdHeight = isLargeScreen ? 480.0 : 180.0;
    final _adWidth = adWidth ?? customAdWidth ?? defaultAdWidth;
    final _adHeight = adHeight ?? customAdHeight ?? defaultAdHeight;
    final _minAdWidth = minAdWidth ?? 250.0;
    final _maxAdWidth = maxAdWidth ?? constraints.maxWidth - 40;
    final minAdHeight = 120.0;
    final verticalPadding = 80.0;
    final maxAdHeight = constraints.maxHeight - verticalPadding;
    final ad = ads[_currentAdIndex];
    final adFontSize = _adHeight > 300 ? 40.0 : 18.0;
    final adSubtitleFontSize = _adHeight > 300 ? 24.0 : 12.0;
    final adBrandFontSize = _adHeight > 300 ? 20.0 : 10.0;
    final adImageHeight = _adHeight > 300 ? 220.0 : 70.0;
    // Remove expand/reset button UI
    // final toggleLabel = (customAdWidth != null || customAdHeight != null) ? 'Redefinir tamanho' : 'Expandir anúncio';
    // final toggleIcon = (customAdWidth != null || customAdHeight != null) ? Icons.refresh : Icons.open_in_full;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        // Remove the expand/reset button row
        // Row(
        //   mainAxisAlignment: MainAxisAlignment.end,
        //   children: [ ... ],
        // ),
        Center(
          child: Stack(
            children: [
              AnimatedContainer(
                duration: const Duration(milliseconds: 300),
                curve: Curves.easeInOut,
                width: _adWidth,
                height: _adHeight,
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    colors: [
                      (ad["backgroundColor"] as Color).withOpacity(0.95),
                      (ad["backgroundColor"] as Color).withOpacity(0.7),
                    ],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
                  borderRadius: BorderRadius.circular(28),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withOpacity(0.13),
                      blurRadius: 24,
                      offset: const Offset(0, 8),
                    ),
                  ],
                ),
                child: Row(
                  children: [
                    Expanded(
                      flex: 2,
                      child: Padding(
                        padding: const EdgeInsets.all(32.0),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Text(
                              ad["title"] as String,
                              style: theme.textTheme.headlineMedium?.copyWith(
                                color: Colors.white,
                                fontWeight: FontWeight.bold,
                                fontSize: adFontSize,
                                shadows: [
                                  Shadow(
                                    color: Colors.black.withOpacity(0.18),
                                    blurRadius: 6,
                                  ),
                                ],
                              ),
                              maxLines: 2,
                              overflow: TextOverflow.ellipsis,
                            ),
                            SizedBox(height: _adHeight > 300 ? 28 : 8),
                            Text(
                              ad["subtitle"] as String,
                              style: theme.textTheme.titleMedium?.copyWith(
                                color: Colors.white.withOpacity(0.9),
                                fontSize: adSubtitleFontSize,
                              ),
                              maxLines: 2,
                              overflow: TextOverflow.ellipsis,
                            ),
                            SizedBox(height: _adHeight > 300 ? 24 : 6),
                            Text(
                              ad["brand"] as String,
                              style: theme.textTheme.labelLarge?.copyWith(
                                color: Colors.white.withOpacity(0.8),
                                fontSize: adBrandFontSize,
                                fontWeight: FontWeight.w600,
                              ),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                          ],
                        ),
                      ),
                    ),
                    Expanded(
                      flex: 1,
                      child: Padding(
                        padding: const EdgeInsets.all(16.0),
                        child: ClipRRect(
                          borderRadius: BorderRadius.circular(16),
                          child: ConstrainedBox(
                            constraints: BoxConstraints(
                              maxWidth: double.infinity,
                              maxHeight: _adHeight - 32, // Prevent overflow
                            ),
                            child: Image.asset(
                              ad["image"] as String,
                              height: adImageHeight,
                              fit: BoxFit.contain,
                              errorBuilder: (context, error, stackTrace) => Center(
                                child: Text(
                                  'Imagem não encontrada',
                                  style: TextStyle(color: Colors.white, fontSize: 12),
                                ),
                              ),
                            ),
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              // Draggable handle (left edge, vertically centered)
              Positioned(
                left: 0,
                top: (_adHeight / 2) - 24,
                child: GestureDetector(
                  onPanUpdate: (details) {
                    if (onResize != null) {
                      final newWidth = (_adWidth - details.delta.dx).clamp(_minAdWidth, _maxAdWidth);
                      onResize(newWidth, _adHeight);
                    }
                  },
                  child: MouseRegion(
                    cursor: SystemMouseCursors.resizeLeftRight,
                    child: Container(
                      width: 16,
                      height: 48,
                      decoration: BoxDecoration(
                        color: Colors.transparent,
                        borderRadius: const BorderRadius.only(
                          topRight: Radius.circular(16),
                          bottomRight: Radius.circular(16),
                        ),
                        border: Border.all(color: Colors.black12, width: 1),
                        boxShadow: [
                          BoxShadow(
                            color: Colors.black.withOpacity(0.04),
                            blurRadius: 2,
                            offset: Offset(1, 1),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              ),
              // Draggable handle (bottom-right corner for height)
              Positioned(
                right: 0,
                bottom: 0,
                child: GestureDetector(
                  onPanUpdate: (details) {
                    if (onResize != null) {
                      final newHeight = (_adHeight + details.delta.dy).clamp(minAdHeight, maxAdHeight);
                      onResize(_adWidth, newHeight);
                    }
                  },
                  child: MouseRegion(
                    cursor: SystemMouseCursors.resizeUpLeftDownRight,
                    child: Container(
                      width: 28,
                      height: 16,
                      decoration: BoxDecoration(
                        color: Colors.transparent,
                        borderRadius: const BorderRadius.only(
                          bottomRight: Radius.circular(20),
                          topLeft: Radius.circular(8),
                        ),
                        border: Border.all(color: Colors.black26, width: 1),
                        boxShadow: [
                          BoxShadow(
                            color: Colors.black.withOpacity(0.04),
                            blurRadius: 2,
                            offset: Offset(1, 1),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  List<Widget> _adContentChildren(ThemeData theme, double iconSize, double titleFontSize, double subtitleFontSize, BoxConstraints constraints) {
    return [
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
        ads[_currentAdIndex]["title"],
        style: theme.textTheme.headlineMedium?.copyWith(
          color: Colors.white,
          fontWeight: FontWeight.bold,
          fontSize: titleFontSize,
        ),
        textAlign: TextAlign.center,
      ),
      SizedBox(height: constraints.maxHeight * 0.015),
      Text(
        ads[_currentAdIndex]["subtitle"],
        style: theme.textTheme.titleLarge?.copyWith(
          color: Colors.white.withOpacity(0.9),
          fontSize: subtitleFontSize,
          height: 1.4,
        ),
        textAlign: TextAlign.center,
      ),
    ];
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