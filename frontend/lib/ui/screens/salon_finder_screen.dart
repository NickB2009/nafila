import 'package:flutter/material.dart';
import '../../utils/brazilian_names_generator.dart';
import '../widgets/bottom_nav_bar.dart';
import '../../models/salon.dart';
import '../../models/salon_service.dart';
import '../../models/salon_contact.dart';
import '../../models/salon_hours.dart';
import '../../models/salon_review.dart';
import 'notifications_screen.dart';
import 'salon_map_screen.dart';
import 'check_in_screen.dart';
import 'salon_details_screen.dart';
import '../theme/app_theme.dart';
import 'dart:async';
import 'dart:math' as math;
import '../../utils/palette_utils.dart';

/// Salon finder screen for mobile web interface
class SalonFinderScreen extends StatefulWidget {
  const SalonFinderScreen({super.key});

  @override
  State<SalonFinderScreen> createState() => _SalonFinderScreenState();
}

class _SalonFinderScreenState extends State<SalonFinderScreen> with TickerProviderStateMixin {
  final Set<String> _favoriteSalons = {};
  late AnimationController _animationController;
  late AnimationController _heroAnimationController;
  late AnimationController _pulseController;
  late AnimationController _floatingController;
  late Animation<double> _fadeAnimation;
  late Animation<Offset> _slideAnimation;
  late Animation<double> _heroScaleAnimation;
  late Animation<double> _pulseAnimation;
  late Animation<double> _floatingAnimation;
  late Animation<Color?> _gradientAnimation;
  
  Timer? _waitTimeTimer;
  Timer? _offerTimer;
  int _secondsRemaining = 300; // 5 minutes countdown
  List<Salon> _dynamicSalons = [];
  late final String greetingName;

  @override
  void initState() {
    super.initState();
    greetingName = BrazilianNamesGenerator.generateGreeting();
    _setupAnimations();
    _startDynamicUpdates();
    _generateDynamicSalons();
  }

  void _setupAnimations() {
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 800),
      vsync: this,
    );

    _heroAnimationController = AnimationController(
      duration: const Duration(milliseconds: 1200),
      vsync: this,
    );

    _pulseController = AnimationController(
      duration: const Duration(milliseconds: 2000),
      vsync: this,
    );

    _floatingController = AnimationController(
      duration: const Duration(milliseconds: 3000),
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

    _heroScaleAnimation = Tween<double>(begin: 0.8, end: 1.0).animate(
      CurvedAnimation(parent: _heroAnimationController, curve: Curves.elasticOut),
    );

    _pulseAnimation = Tween<double>(begin: 1.0, end: 1.1).animate(
      CurvedAnimation(parent: _pulseController, curve: Curves.easeInOut),
    );

    _floatingAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(parent: _floatingController, curve: Curves.easeInOut),
    );

    _gradientAnimation = ColorTween(
      begin: AppTheme.heroGradient[0],
      end: AppTheme.heroGradient[1],
    ).animate(_floatingController);

    _animationController.forward();
    _heroAnimationController.forward();
    _pulseController.repeat(reverse: true);
    _floatingController.repeat(reverse: true);
  }

  void _startDynamicUpdates() {
    _waitTimeTimer = Timer.periodic(const Duration(seconds: 10), (timer) {
      setState(() {
        _updateWaitTimes();
      });
    });

    _offerTimer = Timer.periodic(const Duration(seconds: 1), (timer) {
      setState(() {
        _secondsRemaining--;
        if (_secondsRemaining <= 0) {
          _secondsRemaining = 300; // Reset to 5 minutes
        }
      });
    });
  }

  void _generateDynamicSalons() {
    final random = math.Random();
    _dynamicSalons = List.generate(4, (index) {
      final baseWaitTime = [5, 12, 18, 25][index];
      final names = ['Studio Elegance', 'Beleza Premium', 'Hair Lounge', 'Salon Luxe'];
      final addresses = [
        'Rua das Flores, 123 - Centro',
        'Av. Paulista, 456 - Bela Vista',
        'Rua Augusta, 789 - Consolação',
        'Av. Brigadeiro, 321 - Jardins'
      ];
      
      return Salon(
        name: names[index],
        address: addresses[index],
        waitTime: baseWaitTime + random.nextInt(10),
        distance: 0.5 + (index * 0.3) + (random.nextDouble() * 0.5),
        isOpen: true,
        closingTime: index < 2 ? '9 PM' : '7 PM',
        isFavorite: index == 0,
        queueLength: math.max(1, (baseWaitTime / 5).round() + random.nextInt(3)),
        colors: _randomSalonColors(index),
      );
    });
  }

  void _updateWaitTimes() {
    final random = math.Random();
    for (int i = 0; i < _dynamicSalons.length; i++) {
      final currentWait = _dynamicSalons[i].waitTime;
      final change = random.nextInt(6) - 3; // -3 to +2 minutes
      _dynamicSalons[i] = Salon(
        name: _dynamicSalons[i].name,
        address: _dynamicSalons[i].address,
        waitTime: math.max(1, currentWait + change),
        distance: _dynamicSalons[i].distance,
        isOpen: _dynamicSalons[i].isOpen,
        closingTime: _dynamicSalons[i].closingTime,
        isFavorite: _dynamicSalons[i].isFavorite,
        queueLength: math.max(1, _dynamicSalons[i].queueLength + (change > 0 ? 1 : -1)),
        colors: _dynamicSalons[i].colors,
      );
    }
  }

  @override
  void dispose() {
    _animationController.dispose();
    _heroAnimationController.dispose();
    _pulseController.dispose();
    _floatingController.dispose();
    _waitTimeTimer?.cancel();
    _offerTimer?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final size = MediaQuery.of(context).size;
    final brightness = theme.brightness;
    final isDark = brightness == Brightness.dark;
    final background = theme.colorScheme.background;
    final surface = theme.colorScheme.surface;
    final onBackground = theme.colorScheme.onBackground;
    final onSurface = theme.colorScheme.onSurface;
    final cardShadow = theme.shadowColor.withOpacity(isDark ? 0.18 : 0.06);
    final cardBorder = isDark ? Colors.white12 : Colors.grey.shade100;
    final lightGradient = [Colors.white, Colors.grey.shade50];
    final darkGradient = [surface, background];
    final mainGradient = isDark ? darkGradient : lightGradient;
    
    return Scaffold(
      body: CustomScrollView(
        slivers: [
          // Hero App Bar with stunning gradient - SLIGHTLY LARGER
          SliverAppBar(
            expandedHeight: size.height * 0.29, // Up from 0.24
            floating: false,
            pinned: true,
            backgroundColor: Colors.transparent,
            elevation: 0,
            flexibleSpace: FlexibleSpaceBar(
              background: _buildHeroSection(context, theme, size, isDark),
            ),
            leading: null,
            automaticallyImplyLeading: false,
            actions: [
              Container(
                margin: const EdgeInsets.only(right: 16),
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                decoration: BoxDecoration(
                  color: Colors.white.withOpacity(0.2),
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(Icons.person, color: Colors.white, size: 16),
                    const SizedBox(width: 4),
                    Text('1', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
                    const SizedBox(width: 8),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                      decoration: BoxDecoration(
                        color: AppTheme.dangerColor,
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Text('Q2', style: TextStyle(color: Colors.white, fontSize: 12, fontWeight: FontWeight.bold)),
                    ),
                  ],
                ),
              ),
            ],
          ),
          
          // Main content with balanced spacing
          SliverToBoxAdapter(
            child: Container(
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.topCenter,
                  end: Alignment.bottomCenter,
                  colors: mainGradient,
                ),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Stats section
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 18),
                    child: _buildStatsSection(context, theme, size),
                  ),
                  
                  // Divider for visual grouping
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 20),
                    child: Divider(
                      color: Colors.grey.shade200,
                      thickness: 1.2,
                      height: 36, // Slightly more space
                    ),
                  ),
                  
                  Padding(
                    padding: EdgeInsets.symmetric(horizontal: size.width > 600 ? 36.0 : 24.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const SizedBox(height: 24), // More space before section header
                        
                        // Closest salons header
                        _buildSectionHeader(context, theme, 'Salões mais próximos', 'Tempo real'),
                        const SizedBox(height: 22), // More space before cards
                        
                        // Dynamic salon cards
                        ..._buildDynamicSalonCards(context, theme),
                        
                        const SizedBox(height: 32), // More space before find card
                        
                        // Find salon card
                        _buildFindSalonCard(context, theme),
                        
                        const SizedBox(height: 36), // More space at bottom
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
      bottomNavigationBar: const BottomNavBar(currentIndex: 0),
    );
  }

  Widget _buildHeroSection(BuildContext context, ThemeData theme, Size size, bool isDark) {
    final onBackground = theme.colorScheme.onBackground;
    return AnimatedBuilder(
      animation: _floatingController,
      builder: (context, child) {
        final theme = Theme.of(context);
        final isDark = theme.brightness == Brightness.dark;
        return Container(
          width: double.infinity,
          height: double.infinity,
          decoration: BoxDecoration(
            gradient: LinearGradient(
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
              colors: isDark
                ? [theme.colorScheme.primary, theme.colorScheme.secondary, theme.colorScheme.background]
                : [_gradientAnimation.value ?? AppTheme.heroGradient[0], AppTheme.heroGradient[1], AppTheme.heroGradient[2]],
            ),
          ),
          child: Stack(
            children: [
              // Floating particles
              ...List.generate(8, (index) {
                final delay = index * 0.2;
                return AnimatedBuilder(
                  animation: _floatingAnimation,
                  builder: (context, child) {
                    final progress = (_floatingAnimation.value + delay) % 1.0;
                    return Positioned(
                      left: size.width * (0.1 + (index % 3) * 0.3),
                      top: size.height * (0.1 + progress * 0.6),
                      child: Opacity(
                        opacity: 0.3 + (math.sin(progress * math.pi) * 0.4),
                        child: Container(
                          width: 4 + (index % 3) * 2,
                          height: 4 + (index % 3) * 2,
                          decoration: BoxDecoration(
                            color: Colors.white,
                            shape: BoxShape.circle,
                            boxShadow: [
                              BoxShadow(
                                color: Colors.white.withOpacity(0.5),
                                blurRadius: 8,
                                spreadRadius: 2,
                              ),
                            ],
                          ),
                        ),
                      ),
                    );
                  },
                );
              }),
              
              // Main hero content
              SafeArea(
                child: Padding(
                  padding: const EdgeInsets.all(16.0), // Up from 12
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const SizedBox(height: 14), // Up from 8
                      
                      // Animated greeting
                      ScaleTransition(
                        scale: _heroScaleAnimation,
                        child: FadeTransition(
                          opacity: _fadeAnimation,
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(
                                children: [
                                  Container(
                                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6), // Up from 10/4
                                    decoration: BoxDecoration(
                                      color: Colors.white.withOpacity(0.20),
                                      borderRadius: BorderRadius.circular(16),
                                    ),
                                    child: Row(
                                      mainAxisSize: MainAxisSize.min,
                                      children: [
                                        Icon(Icons.waving_hand, color: Colors.white, size: 16), // Up from 14
                                        const SizedBox(width: 6),
                                        Text(
                                          greetingName,
                                          style: TextStyle(
                                            color: onBackground,
                                            fontSize: 14, // Up from 12
                                            fontWeight: FontWeight.w600,
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 10), // Up from 8/6
                              Text(
                                "Transforme seu\nvisual hoje mesmo",
                                style: TextStyle(
                                  color: onBackground,
                                  fontSize: 24, // Up from 20
                                  fontWeight: FontWeight.bold,
                                  height: 1.2,
                                  shadows: [
                                    Shadow(
                                      color: Colors.black.withOpacity(0.22),
                                      offset: const Offset(0, 2),
                                      blurRadius: 4,
                                    ),
                                  ],
                                ),
                              ),
                              const SizedBox(height: 8), // Up from 6
                              Container(
                                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6), // Up from 10/4
                                decoration: BoxDecoration(
                                  color: Colors.white.withOpacity(0.13),
                                  borderRadius: BorderRadius.circular(12),
                                ),
                                child: Text(
                                  "Profissionais qualificados • Produtos premium • Resultados garantidos",
                                  style: TextStyle(
                                    color: onBackground.withOpacity(0.90),
                                    fontSize: 12, // Up from 10
                                    height: 1.3,
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
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildSpecialOfferSection(BuildContext context, ThemeData theme) {
    final isDark = theme.brightness == Brightness.dark;
    final minutes = _secondsRemaining ~/ 60;
    final seconds = _secondsRemaining % 60;
    
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: isDark ? [theme.colorScheme.primary, theme.colorScheme.secondary] : AppTheme.offerGradient,
        ),
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: AppTheme.offerGradient[0].withOpacity(0.3),
            blurRadius: 20,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Column(
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: Colors.white.withOpacity(0.2),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Icon(Icons.local_fire_department, color: Colors.white, size: 24),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'OFERTA RELÂMPAGO',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                        letterSpacing: 1.2,
                      ),
                    ),
                    Text(
                      '30% OFF no primeiro agendamento',
                      style: TextStyle(
                        color: Colors.white.withOpacity(0.9),
                        fontSize: 14,
                      ),
                    ),
                  ],
                ),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(16),
                ),
                child: Text(
                  '${minutes.toString().padLeft(2, '0')}:${seconds.toString().padLeft(2, '0')}',
                  style: TextStyle(
                    color: isDark ? theme.colorScheme.primary : AppTheme.offerGradient[0],
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Container(
            width: double.infinity,
            height: 48,
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(24),
            ),
            child: Material(
              color: Colors.transparent,
              child: InkWell(
                borderRadius: BorderRadius.circular(24),
                onTap: () {
                  // Navigate to booking with discount
                },
                child: Center(
                  child: Text(
                    'ENTRAR NA FILA',
                    style: TextStyle(
                      color: isDark ? theme.colorScheme.primary : AppTheme.offerGradient[0],
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                      letterSpacing: 1.0,
                    ),
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSectionHeader(BuildContext context, ThemeData theme, String title, String subtitle) {
    return Row(
      children: [
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                title,
                style: theme.textTheme.headlineSmall?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: Colors.grey.shade800,
                ),
              ),
              Row(
                children: [
                  Container(
                    width: 8,
                    height: 8,
                    decoration: BoxDecoration(
                      color: AppTheme.successColor,
                      shape: BoxShape.circle,
                    ),
                  ),
                  const SizedBox(width: 6),
                  Text(
                    subtitle,
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: AppTheme.successColor,
                      fontWeight: FontWeight.w500,
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
        TextButton(
          onPressed: () => Navigator.of(context).push(
            MaterialPageRoute(builder: (_) => const SalonMapScreen()),
          ),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text('Ver mapa'),
              const SizedBox(width: 4),
              Icon(Icons.arrow_forward, size: 16),
            ],
          ),
        ),
      ],
    );
  }

  List<Widget> _buildDynamicSalonCards(BuildContext context, ThemeData theme) {
    return List.generate(
      _dynamicSalons.length,
      (index) => Padding(
        padding: const EdgeInsets.only(bottom: 20), // Up from 16
        child: TweenAnimationBuilder<double>(
          tween: Tween(begin: 0.0, end: 1.0),
          duration: Duration(milliseconds: 600 + (index * 200)),
          builder: (context, value, child) {
            final theme = Theme.of(context);
            final isDark = theme.brightness == Brightness.dark;
            return Transform.translate(
              offset: Offset(0, 20 * (1 - value)),
              child: Opacity(
                opacity: value,
                child: child,
              ),
            );
          },
          child: _buildBalancedSalonCard(context, theme, _dynamicSalons[index], index),
        ),
      ),
    );
  }

  Widget _buildBalancedSalonCard(BuildContext context, ThemeData theme, Salon salon, int index) {
    final isDark = theme.brightness == Brightness.dark;
    final isUrgent = salon.waitTime <= 10;
    final isPopular = salon.queueLength >= 4;
    final cardBackground = isDark ? theme.colorScheme.surface : Colors.white;
    final cardBorder = isDark ? Colors.white12 : Colors.grey.shade100;
    final cardShadow = theme.shadowColor.withOpacity(isDark ? 0.18 : 0.06);
    final textColor = theme.colorScheme.onSurface;
    final subTextColor = theme.colorScheme.onSurface.withOpacity(0.7);
    final urgentColor = isDark ? theme.colorScheme.secondary : AppTheme.semanticColors['urgent']!;
    final popularColor = isDark ? theme.colorScheme.primary : AppTheme.semanticColors['popular']!;
    final chipTimeColor = isUrgent ? urgentColor : (isDark ? theme.colorScheme.primary : AppTheme.semanticColors['time']!);
    final chipQueueColor = isPopular ? popularColor : (isDark ? theme.colorScheme.secondary : AppTheme.semanticColors['queue']!);
    final chipDistanceColor = isDark ? theme.colorScheme.secondary : AppTheme.semanticColors['distance']!;
    final ctaBg = theme.colorScheme.primary.withOpacity(0.13);
    final ctaText = theme.colorScheme.primary;
    final ctaSubText = theme.colorScheme.onSurface.withOpacity(0.8);
    final buttonBg = isDark ? theme.colorScheme.surface : Colors.white;
    final buttonText = theme.colorScheme.primary;
    final buttonShadow = theme.colorScheme.primary.withOpacity(0.10);
    final favColor = _favoriteSalons.contains(salon.name) ? AppTheme.dangerColor : (isDark ? theme.colorScheme.onSurface.withOpacity(0.4) : Colors.grey.shade300);
    return Container(
      decoration: BoxDecoration(
        color: cardBackground,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: cardBorder),
        boxShadow: [
          BoxShadow(
            color: cardShadow,
            blurRadius: 14,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          borderRadius: BorderRadius.circular(18),
          onTap: () {
            Navigator.of(context).push(
              MaterialPageRoute(
                builder: (_) => SalonDetailsScreen(
                  salon: salon,
                  services: [],
                  contact: SalonContact(phone: '', email: ''),
                  businessHours: [],
                  reviews: [],
                  additionalInfo: const {},
                ),
              ),
            );
          },
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            children: [
                              Expanded(
                                child: Text(
                                  salon.name,
                                  style: theme.textTheme.titleLarge?.copyWith(
                                    fontWeight: FontWeight.bold,
                                    color: textColor,
                                  ),
                                ),
                              ),
                              if (isUrgent) ...[
                                Container(
                                  padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 3),
                                  decoration: BoxDecoration(
                                    color: urgentColor.withOpacity(0.18),
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                  child: Text(
                                    'RÁPIDO',
                                    style: TextStyle(
                                      color: urgentColor,
                                      fontSize: 10,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                ),
                                const SizedBox(width: 8),
                              ],
                              if (isPopular) ...[
                                Container(
                                  padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 3),
                                  decoration: BoxDecoration(
                                    color: popularColor.withOpacity(0.18),
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                  child: Text(
                                    'POPULAR',
                                    style: TextStyle(
                                      color: popularColor,
                                      fontSize: 10,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                ),
                                const SizedBox(width: 8),
                              ],
                              IconButton(
                                icon: Icon(
                                  _favoriteSalons.contains(salon.name) ? Icons.favorite : Icons.favorite_border,
                                  color: favColor,
                                  size: 22,
                                ),
                                onPressed: () {
                                  setState(() {
                                    if (_favoriteSalons.contains(salon.name)) {
                                      _favoriteSalons.remove(salon.name);
                                    } else {
                                      _favoriteSalons.add(salon.name);
                                    }
                                  });
                                },
                                padding: EdgeInsets.zero,
                                constraints: const BoxConstraints(minWidth: 36, minHeight: 36),
                              ),
                            ],
                          ),
                          const SizedBox(height: 4),
                          Text(
                            salon.address,
                            style: theme.textTheme.bodyMedium?.copyWith(
                              color: subTextColor,
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
                    Expanded(
                      child: _buildBalancedInfoChip(
                        context,
                        Icons.access_time,
                        '${salon.waitTime} min',
                        chipTimeColor,
                      ),
                    ),
                    const SizedBox(width: 10),
                    Expanded(
                      child: _buildBalancedInfoChip(
                        context,
                        Icons.people_outline,
                        '${salon.queueLength} fila',
                        chipQueueColor,
                      ),
                    ),
                    const SizedBox(width: 10),
                    Expanded(
                      child: _buildBalancedInfoChip(
                        context,
                        Icons.location_on_outlined,
                        '${salon.distance.toStringAsFixed(1)} km',
                        chipDistanceColor,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 16),
                Container(
                  padding: const EdgeInsets.all(14),
                  decoration: BoxDecoration(
                    color: ctaBg,
                    borderRadius: BorderRadius.circular(16),
                  ),
                  child: Row(
                    children: [
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              isUrgent ? 'Check-in rápido!' : 'Fazer check-in',
                              style: TextStyle(
                                color: ctaText,
                                fontSize: 16,
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                            Text(
                              isUrgent ? 'Sem espera' : 'Entre na fila',
                              style: TextStyle(
                                color: ctaSubText,
                                fontSize: 13,
                              ),
                            ),
                          ],
                        ),
                      ),
                      GestureDetector(
                        onTap: CheckInState.isCheckedIn ? null : () {
                          Navigator.of(context).push(
                            MaterialPageRoute(
                              builder: (_) => CheckInScreen(salon: salon),
                            ),
                          );
                        },
                        child: Container(
                          padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 10),
                          decoration: BoxDecoration(
                            color: buttonBg,
                            borderRadius: BorderRadius.circular(22),
                            boxShadow: [
                              BoxShadow(
                                color: buttonShadow,
                                blurRadius: 8,
                                offset: const Offset(0, 2),
                              ),
                            ],
                          ),
                          child: Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              Icon(Icons.login, color: buttonText, size: 18),
                              const SizedBox(width: 6),
                              Text(
                                'Check-in',
                                style: TextStyle(
                                  color: buttonText,
                                  fontWeight: FontWeight.bold,
                                  fontSize: 15,
                                ),
                              ),
                            ],
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
      ),
    );
  }

  Widget _buildBalancedInfoChip(BuildContext context, IconData icon, String label, Color color) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;
    final chipBg = isDark ? color.withOpacity(0.13) : color.withOpacity(0.09);
    final chipBorder = isDark ? color.withOpacity(0.22) : color.withOpacity(0.18);
    final chipText = isDark ? theme.colorScheme.onSurface : color;
    final chipIcon = isDark ? theme.colorScheme.onSurface.withOpacity(0.85) : color.withOpacity(0.85);
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
      decoration: BoxDecoration(
        color: chipBg,
        borderRadius: BorderRadius.circular(10),
        border: Border.all(color: chipBorder),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 16, color: chipIcon),
          const SizedBox(width: 6),
          Expanded(
            child: Text(
              label,
              style: TextStyle(
                color: chipText,
                fontWeight: FontWeight.w600,
                fontSize: 12,
              ),
              overflow: TextOverflow.ellipsis,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFindSalonCard(BuildContext context, ThemeData theme) {
    final isDark = theme.brightness == Brightness.dark;
    final gradient = isDark
        ? [theme.colorScheme.primary, theme.colorScheme.secondary]
        : AppTheme.ctaGradient;
    final textColor = isDark ? theme.colorScheme.onPrimary : Colors.white;
    final subTextColor = isDark ? theme.colorScheme.onPrimary.withOpacity(0.9) : Colors.white.withOpacity(0.9);
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: gradient,
        ),
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.2),
            blurRadius: 20,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Row(
        children: [
          Container(
            width: 60,
            height: 60,
            decoration: BoxDecoration(
              color: textColor.withOpacity(0.15),
              borderRadius: BorderRadius.circular(30),
            ),
            child: Icon(
              Icons.explore,
              color: textColor,
              size: 28,
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Explorar mais salões',
                  style: TextStyle(
                    color: textColor,
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                    shadows: [
                      Shadow(
                        color: Colors.black.withOpacity(0.15),
                        blurRadius: 2,
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 4),
                GestureDetector(
                  onTap: () => Navigator.of(context).push(
                    MaterialPageRoute(builder: (_) => const SalonMapScreen()),
                  ),
                  child: Row(
                    children: [
                      Text(
                        'Ver mapa interativo',
                        style: TextStyle(
                          color: subTextColor,
                          fontSize: 14,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                      const SizedBox(width: 4),
                      Icon(
                        Icons.arrow_forward,
                        size: 16,
                        color: subTextColor,
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatsSection(BuildContext context, ThemeData theme, Size size) {
    final int salonCount = _dynamicSalons.length;
    final double avgQueue = salonCount > 0 ? _dynamicSalons.map((s) => s.queueLength).reduce((a, b) => a + b) / salonCount : 0;
    final double avgWait = salonCount > 0 ? _dynamicSalons.map((s) => s.waitTime).reduce((a, b) => a + b) / salonCount : 0;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 18),
      decoration: BoxDecoration(
        color: Colors.white, // Use white background for less visual noise
        borderRadius: BorderRadius.circular(18),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
        border: Border.all(color: Colors.grey.shade100),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        children: [
          _buildStatItem(context, Icons.store, '$salonCount', 'Salões', AppTheme.primaryColor),
          _buildStatItem(context, Icons.people, '${avgQueue.round()}', 'Fila média', AppTheme.warningColor),
          _buildStatItem(context, Icons.schedule, '${avgWait.round()}', 'Espera média', AppTheme.successColor),
        ],
      ),
    );
  }

  Widget _buildStatItem(BuildContext context, IconData icon, String value, String label, Color color) {
    return Column(
      children: [
        Icon(icon, color: color.withOpacity(0.85), size: 22),
        const SizedBox(height: 6),
        Text(
          value,
          style: TextStyle(
            color: Colors.grey.shade800,
            fontSize: 18,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 2),
        Text(
          label,
          style: TextStyle(
            color: Colors.grey.shade500,
            fontSize: 12,
          ),
        ),
      ],
    );
  }
}

SalonColors _randomSalonColors(int seed) {
  final palettes = [
    SalonColors(primary: Colors.redAccent, secondary: Colors.orange, background: Colors.red.shade50, onSurface: Colors.red.shade900),
    SalonColors(primary: Colors.blueAccent, secondary: Colors.cyan, background: Colors.blue.shade50, onSurface: Colors.blue.shade900),
    SalonColors(primary: Colors.green, secondary: Colors.teal, background: Colors.green.shade50, onSurface: Colors.green.shade900),
    SalonColors(primary: Colors.purple, secondary: Colors.pinkAccent, background: Colors.purple.shade50, onSurface: Colors.purple.shade900),
    SalonColors(primary: Colors.amber, secondary: Colors.deepOrange, background: Colors.amber.shade50, onSurface: Colors.amber.shade900),
    SalonColors(primary: Colors.indigo, secondary: Colors.lime, background: Colors.indigo.shade50, onSurface: Colors.indigo.shade900),
  ];
  return palettes[seed % palettes.length];
}

/// Custom painter for the salon decoration
class SalonDecorationPainter extends CustomPainter {
  final Color color;

  SalonDecorationPainter({required this.color});

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = color
      ..style = PaintingStyle.stroke
      ..strokeWidth = 2;

    // Draw a modern, abstract pattern representing hair styling
    final path = Path();
    
    // Main curve
    path.moveTo(size.width * 0.2, size.height * 0.5);
    path.quadraticBezierTo(
      size.width * 0.5,
      size.height * 0.2,
      size.width * 0.8,
      size.height * 0.5,
    );

    // Decorative elements
    for (var i = 0; i < 3; i++) {
      final offset = i * (size.width * 0.2);
      path.moveTo(size.width * 0.3 + offset, size.height * 0.6);
      path.quadraticBezierTo(
        size.width * 0.4 + offset,
        size.height * 0.4,
        size.width * 0.5 + offset,
        size.height * 0.6,
      );
    }

    // Draw the path
    canvas.drawPath(path, paint);

    // Add some dots for visual interest
    final dotPaint = Paint()
      ..color = color
      ..style = PaintingStyle.fill;

    for (var i = 0; i < 5; i++) {
      final x = size.width * (0.2 + (i * 0.15));
      final y = size.height * (0.3 + (i % 2) * 0.2);
      canvas.drawCircle(Offset(x, y), 2, dotPaint);
    }
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}
