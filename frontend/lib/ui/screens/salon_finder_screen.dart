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

/// Clean and efficient salon finder screen
class SalonFinderScreen extends StatefulWidget {
  const SalonFinderScreen({super.key});

  @override
  State<SalonFinderScreen> createState() => _SalonFinderScreenState();
}

class _SalonFinderScreenState extends State<SalonFinderScreen> with SingleTickerProviderStateMixin {
  final Set<String> _favoriteSalons = {};
  late AnimationController _animationController;
  late Animation<double> _fadeAnimation;
  late Animation<Offset> _slideAnimation;
  
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
      duration: const Duration(milliseconds: 600),
      vsync: this,
    );

    _fadeAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(parent: _animationController, curve: Curves.easeOut),
    );

    _slideAnimation = Tween<Offset>(
      begin: const Offset(0, 0.3),
      end: Offset.zero,
    ).animate(
      CurvedAnimation(parent: _animationController, curve: Curves.easeOut),
    );

    _animationController.forward();
  }

  @override
  void dispose() {
    _animationController.dispose();
    _waitTimeTimer?.cancel();
    _offerTimer?.cancel();
    super.dispose();
  }

  void _startDynamicUpdates() {
    _waitTimeTimer = Timer.periodic(const Duration(seconds: 30), (timer) {
      if (mounted) {
        setState(() {
          _updateSalonWaitTimes();
        });
      }
    });

    _offerTimer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (mounted && _secondsRemaining > 0) {
        setState(() {
          _secondsRemaining--;
        });
      }
    });
  }

  SalonColors _generateSalonColors(int index) {
    final colors = [
      SalonColors(
        primary: const Color(0xFFD4AF37),
        secondary: const Color(0xFF2C3E50),
        background: const Color(0xFFF5F5F5),
        onSurface: const Color(0xFF2C3E50),
      ),
      SalonColors(
        primary: const Color(0xFF6B73FF),
        secondary: const Color(0xFF4CAF50),
        background: const Color(0xFFF8F9FA),
        onSurface: const Color(0xFF2C3E50),
      ),
      SalonColors(
        primary: const Color(0xFFFF9800),
        secondary: const Color(0xFF1976D2),
        background: const Color(0xFFFFF8E1),
        onSurface: const Color(0xFF2C3E50),
      ),
    ];
    return colors[index % colors.length];
  }

  void _updateSalonWaitTimes() {
    final random = math.Random();
    for (int i = 0; i < _dynamicSalons.length; i++) {
      final currentWait = _dynamicSalons[i].waitTime;
      final change = random.nextInt(6) - 3; // -3 to +2 minutes
      _dynamicSalons[i] = _dynamicSalons[i].copyWith(
        waitTime: math.max(1, currentWait + change),
        queueLength: math.max(1, _dynamicSalons[i].queueLength + (change > 0 ? 1 : -1)),
      );
    }
  }

  void _generateDynamicSalons() {
    final random = math.Random();
    _dynamicSalons = [
      Salon(
        name: 'Barbearia Moderna',
        address: 'Rua das Flores, 123',
        waitTime: 25,
        distance: 0.8,
        isOpen: true,
        closingTime: '19:00',
        isFavorite: false,
        queueLength: 3,
        colors: _generateSalonColors(0),
      ),
      Salon(
        name: 'Studio Hair',
        address: 'Av. Paulista, 456',
        waitTime: 35,
        distance: 1.2,
        isOpen: true,
        closingTime: '20:00',
        isFavorite: false,
        queueLength: 2,
        colors: _generateSalonColors(1),
      ),
      Salon(
        name: 'Barbearia Clássica',
        address: 'Rua Augusta, 789',
        waitTime: 20,
        distance: 1.5,
        isOpen: true,
        closingTime: '18:00',
        isFavorite: false,
        queueLength: 4,
        colors: _generateSalonColors(2),
      ),
    ];
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final size = MediaQuery.of(context).size;
    final isDark = theme.brightness == Brightness.dark;
    final mainGradient = isDark ? AppTheme.darkGradient : [Colors.white, Colors.grey.shade50];

    return Scaffold(
      body: CustomScrollView(
        slivers: [
          // Clean Hero App Bar
          SliverAppBar(
            expandedHeight: size.height * 0.23,
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
          
          // Main content
          SliverToBoxAdapter(
            child: Container(
              color: theme.colorScheme.background,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Stats section
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
                    child: _buildStatsSection(context, theme, size),
                  ),
                  
                  // Divider
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 20),
                    child: Divider(
                      color: theme.colorScheme.outline.withOpacity(0.3),
                      thickness: 1,
                      height: 32,
                    ),
                  ),
                  
                  Padding(
                    padding: EdgeInsets.symmetric(horizontal: size.width > 600 ? 32.0 : 20.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const SizedBox(height: 20),
                        
                        // Closest salons header
                        _buildSectionHeader(context, theme, 'Salões mais próximos', 'Tempo real'),
                        const SizedBox(height: 16),
                        
                        // Dynamic salon cards
                        ..._buildDynamicSalonCards(context, theme),
                        
                        const SizedBox(height: 24),
                        
                        // Find salon card
                        _buildFindSalonCard(context, theme),
                        
                        const SizedBox(height: 32),
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
    return Container(
      width: double.infinity,
      height: double.infinity,
      color: theme.colorScheme.surface,
      child: SafeArea(
        child: Padding(
          padding: EdgeInsets.all(size.width < 400 ? 20.0 : 24.0),
          child: SlideTransition(
            position: _slideAnimation,
            child: FadeTransition(
              opacity: _fadeAnimation,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: [
                  SizedBox(height: size.width < 400 ? 12 : 16),
                  
                  // Greeting
                  Container(
                    padding: EdgeInsets.symmetric(
                      horizontal: size.width < 400 ? 14 : 16, 
                      vertical: size.width < 400 ? 6 : 8
                    ),
                    decoration: BoxDecoration(
                      color: theme.colorScheme.primaryContainer,
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(
                          Icons.waving_hand, 
                          color: theme.colorScheme.onPrimaryContainer, 
                          size: size.width < 400 ? 18 : 20
                        ),
                        const SizedBox(width: 8),
                        Text(
                          greetingName,
                          style: TextStyle(
                            color: theme.colorScheme.onPrimaryContainer,
                            fontWeight: FontWeight.bold,
                            fontSize: size.width < 400 ? 14 : 16,
                          ),
                        ),
                      ],
                    ),
                  ),
                  
                  SizedBox(height: size.width < 400 ? 16 : 20),
                  
                  // Main heading
                  Text(
                    "Transforme seu\nvisual hoje mesmo",
                    style: theme.textTheme.headlineMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                      height: 1.2,
                      color: theme.colorScheme.onSurface,
                      fontSize: size.width < 400 ? 22 : 28,
                    ),
                  ),
                  
                  SizedBox(height: size.width < 400 ? 8 : 12),
                  
                  // Simple subtitle
                  Text(
                    "Profissionais qualificados",
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: theme.colorScheme.onSurface.withOpacity(0.7),
                      fontSize: size.width < 400 ? 13 : 15,
                      fontWeight: FontWeight.w500,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
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
                  color: theme.colorScheme.onSurface,
                ),
              ),
              Row(
                children: [
                  Container(
                    width: 8,
                    height: 8,
                    decoration: BoxDecoration(
                      color: theme.colorScheme.tertiary,
                      shape: BoxShape.circle,
                    ),
                  ),
                  const SizedBox(width: 6),
                  Text(
                    subtitle,
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: theme.colorScheme.tertiary,
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
    final isSmallScreen = MediaQuery.of(context).size.width < 600;
    final isUrgent = salon.waitTime <= 10;
    final isPopular = salon.queueLength >= 4;
    final cardBackground = theme.colorScheme.surface;
    final cardBorder = theme.colorScheme.outline.withOpacity(0.2);
    final cardShadow = theme.shadowColor.withOpacity(0.08);
    final textColor = theme.colorScheme.onSurface;
    final subTextColor = theme.colorScheme.onSurfaceVariant;
    final urgentColor = theme.colorScheme.tertiary;
    final popularColor = theme.colorScheme.primary;
    final chipTimeColor = isUrgent ? urgentColor : theme.colorScheme.primary;
    final chipQueueColor = isPopular ? popularColor : theme.colorScheme.secondary;
    final chipDistanceColor = theme.colorScheme.secondary;
    final ctaBg = theme.colorScheme.primaryContainer;
    final ctaText = theme.colorScheme.onPrimaryContainer;
    final ctaSubText = theme.colorScheme.onSurfaceVariant;
    final buttonBg = theme.colorScheme.surface;
    final buttonText = theme.colorScheme.primary;
    final buttonShadow = theme.colorScheme.primary.withOpacity(0.1);
    final favColor = _favoriteSalons.contains(salon.name) ? theme.colorScheme.error : theme.colorScheme.onSurfaceVariant;
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
            padding: EdgeInsets.all(isSmallScreen ? 16 : 20),
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
                                  style: theme.textTheme.headlineSmall?.copyWith(
                                    fontWeight: FontWeight.w600,
                                    color: textColor,
                                    fontSize: isSmallScreen ? 18 : 20,
                                    letterSpacing: -0.2,
                                  ),
                                  overflow: TextOverflow.ellipsis,
                                  maxLines: 1,
                                ),
                              ),
                              if (isUrgent) ...[
                                Container(
                                  padding: EdgeInsets.symmetric(
                                    horizontal: isSmallScreen ? 5 : 7, 
                                    vertical: isSmallScreen ? 2 : 3
                                  ),
                                  decoration: BoxDecoration(
                                    color: urgentColor.withOpacity(0.18),
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                  child: Text(
                                    'RÁPIDO',
                                    style: TextStyle(
                                      color: urgentColor,
                                      fontSize: isSmallScreen ? 8 : 10,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                ),
                                SizedBox(width: isSmallScreen ? 4 : 8),
                              ],
                              if (isPopular) ...[
                                Container(
                                  padding: EdgeInsets.symmetric(
                                    horizontal: isSmallScreen ? 5 : 7, 
                                    vertical: isSmallScreen ? 2 : 3
                                  ),
                                  decoration: BoxDecoration(
                                    color: popularColor.withOpacity(0.18),
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                  child: Text(
                                    'POPULAR',
                                    style: TextStyle(
                                      color: popularColor,
                                      fontSize: isSmallScreen ? 8 : 10,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                ),
                                SizedBox(width: isSmallScreen ? 4 : 8),
                              ],
                              IconButton(
                                icon: Icon(
                                  _favoriteSalons.contains(salon.name) ? Icons.favorite : Icons.favorite_border,
                                  color: favColor,
                                  size: isSmallScreen ? 20 : 22,
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
                                constraints: BoxConstraints(
                                  minWidth: isSmallScreen ? 32 : 36, 
                                  minHeight: isSmallScreen ? 32 : 36
                                ),
                              ),
                            ],
                          ),
                          const SizedBox(height: 4),
                          Text(
                            salon.address,
                            style: theme.textTheme.bodyMedium?.copyWith(
                              color: subTextColor,
                              fontSize: isSmallScreen ? 14 : 15,
                              fontWeight: FontWeight.w400,
                            ),
                            overflow: TextOverflow.ellipsis,
                            maxLines: 1,
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
                SizedBox(height: isSmallScreen ? 12 : 16),
                // Info chips - make them wrap on small screens
                if (isSmallScreen) ...[
                  Wrap(
                    spacing: 8,
                    runSpacing: 8,
                    children: [
                      _buildBalancedInfoChip(
                        context,
                        Icons.access_time,
                        '${salon.waitTime} min',
                        chipTimeColor,
                        isSmallScreen: true,
                      ),
                      _buildBalancedInfoChip(
                        context,
                        Icons.people_outline,
                        '${salon.queueLength} fila',
                        chipQueueColor,
                        isSmallScreen: true,
                      ),
                      _buildBalancedInfoChip(
                        context,
                        Icons.location_on_outlined,
                        '${salon.distance.toStringAsFixed(1)} km',
                        chipDistanceColor,
                        isSmallScreen: true,
                      ),
                    ],
                  ),
                ] else ...[
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
                ],
                SizedBox(height: isSmallScreen ? 12 : 16),
                Container(
                  padding: EdgeInsets.all(isSmallScreen ? 12 : 16),
                  decoration: BoxDecoration(
                    color: ctaBg,
                    borderRadius: BorderRadius.circular(18),
                    border: Border.all(color: ctaText.withOpacity(0.2)),
                  ),
                  child: Row(
                    children: [
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              isUrgent ? 'Check-in rápido!' : 'Fazer check-in',
                              style: theme.textTheme.titleMedium?.copyWith(
                                color: ctaText,
                                fontWeight: FontWeight.w600,
                                fontSize: isSmallScreen ? 15 : 16,
                              ),
                            ),
                            Text(
                              isUrgent ? 'Sem espera' : 'Entre na fila',
                              style: theme.textTheme.bodyMedium?.copyWith(
                                color: ctaText.withOpacity(0.7),
                                fontSize: isSmallScreen ? 11 : 12,
                                fontWeight: FontWeight.w500,
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
                          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                          decoration: BoxDecoration(
                            color: buttonBg,
                            borderRadius: BorderRadius.circular(24),
                            boxShadow: [
                              BoxShadow(
                                color: buttonShadow,
                                blurRadius: 12,
                                offset: const Offset(0, 3),
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
                                style: theme.textTheme.labelLarge?.copyWith(
                                  color: buttonText,
                                  fontWeight: FontWeight.w600,
                                  fontSize: 14,
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

  Widget _buildBalancedInfoChip(BuildContext context, IconData icon, String label, Color color, {bool isSmallScreen = false}) {
    final theme = Theme.of(context);
    return Container(
      padding: EdgeInsets.symmetric(horizontal: isSmallScreen ? 10 : 12, vertical: isSmallScreen ? 8 : 10),
      decoration: BoxDecoration(
        color: theme.colorScheme.surface,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: theme.colorScheme.outline.withOpacity(0.3)),
        boxShadow: [
          BoxShadow(
            color: theme.shadowColor.withOpacity(0.04),
            blurRadius: 4,
            offset: const Offset(0, 1),
          ),
        ],
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: isSmallScreen ? 14 : 16, color: color),
          const SizedBox(width: 6),
          Expanded(
            child: Text(
              label,
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurface,
                fontWeight: FontWeight.w500,
                fontSize: isSmallScreen ? 11 : 12,
              ),
              overflow: TextOverflow.ellipsis,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFindSalonCard(BuildContext context, ThemeData theme) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            theme.colorScheme.primary,
            theme.colorScheme.primary.withOpacity(0.8),
          ],
        ),
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: theme.colorScheme.primary.withOpacity(0.3),
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
              color: Colors.white.withOpacity(0.2),
              borderRadius: BorderRadius.circular(30),
            ),
            child: Icon(
              Icons.explore,
              color: Colors.white,
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
                  style: theme.textTheme.titleLarge?.copyWith(
                    color: Colors.white,
                    fontWeight: FontWeight.w600,
                    fontSize: 18,
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
                        style: theme.textTheme.bodyMedium?.copyWith(
                          color: Colors.white.withOpacity(0.9),
                          fontSize: 14,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                      const SizedBox(width: 4),
                      Icon(
                        Icons.arrow_forward,
                        size: 16,
                        color: Colors.white.withOpacity(0.9),
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
        color: theme.colorScheme.surface,
        borderRadius: BorderRadius.circular(18),
        boxShadow: [
          BoxShadow(
            color: theme.shadowColor.withOpacity(0.08),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
        border: Border.all(color: theme.colorScheme.outline.withOpacity(0.2)),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        children: [
          _buildStatItem(context, theme, Icons.store, '$salonCount', 'Salões', theme.colorScheme.primary),
          _buildStatItem(context, theme, Icons.people, '${avgQueue.round()}', 'Fila média', theme.colorScheme.tertiary),
          _buildStatItem(context, theme, Icons.schedule, '${avgWait.round()}', 'Espera média', theme.colorScheme.secondary),
        ],
      ),
    );
  }

  Widget _buildStatItem(BuildContext context, ThemeData theme, IconData icon, String value, String label, Color color) {
    return Column(
      children: [
        Icon(icon, color: color, size: 22),
        const SizedBox(height: 6),
        Text(
          value,
          style: theme.textTheme.titleLarge?.copyWith(
            color: theme.colorScheme.onSurface,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 2),
        Text(
          label,
          style: theme.textTheme.bodySmall?.copyWith(
            color: theme.colorScheme.onSurfaceVariant,
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
