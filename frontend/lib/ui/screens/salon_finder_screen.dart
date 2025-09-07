import 'package:flutter/material.dart';
import '../../utils/brazilian_names_generator.dart';
import 'package:provider/provider.dart';
import '../../controllers/app_controller.dart';
import '../widgets/bottom_nav_bar.dart';
import '../../models/public_salon.dart';
import '../../models/anonymous_user.dart';
import '../../models/salon.dart';
import '../../models/salon_service.dart';
import '../../models/salon_contact.dart';
import '../../services/anonymous_queue_service.dart';
import '../../services/anonymous_user_service.dart';
import 'salon_map_screen.dart';
import 'salon_details_screen.dart';
import 'anonymous_join_queue_screen.dart';
import 'anonymous_queue_status_screen.dart';
import 'account_screen.dart';
import 'qr_join_screen.dart';
import 'dart:async';
import 'dart:math' as math;

/// Enhanced salon finder screen with anonymous queue functionality
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
  List<PublicSalon> _dynamicSalons = [];
  late final String greetingName;

  // Anonymous queue services
  late AnonymousQueueService _queueService;
  late AnonymousUserService _userService;
  AnonymousUser? _currentUser;
  List<AnonymousQueueEntry> _activeQueues = [];
  final bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    greetingName = BrazilianNamesGenerator.generateGreeting();
    _setupAnimations();
    _setupServices();
    _startDynamicUpdates();
    _generateDynamicSalons();
    _loadUserData();
  }

  void _setupServices() {
    _userService = AnonymousUserService();
    _queueService = AnonymousQueueService(userService: _userService);
  }

  Future<void> _loadUserData() async {
    try {
      print('🔍 Loading user data...');
      _currentUser = await _userService.getAnonymousUser();
      if (_currentUser != null) {
        setState(() {
          _activeQueues = _currentUser!.activeQueues.where((q) => q.isActive).toList();
        });
        print('✅ User loaded: ${_currentUser!.name}, Active queues: ${_activeQueues.length}');
      } else {
        print('ℹ️ No existing user found');
      }
    } catch (e) {
      print('❌ Error loading user data: $e');
    }
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

  void _updateSalonWaitTimes() {
    final random = math.Random();
    for (int i = 0; i < _dynamicSalons.length; i++) {
      final currentWait = _dynamicSalons[i].currentWaitTimeMinutes ?? 15;
      final change = random.nextInt(6) - 3; // -3 to +2 minutes
      final newWaitTime = math.max(1, currentWait + change);
      final newQueueLength = math.max(1, (_dynamicSalons[i].queueLength ?? 1) + (change > 0 ? 1 : -1));
      
      _dynamicSalons[i] = PublicSalon(
        id: _dynamicSalons[i].id,
        name: _dynamicSalons[i].name,
        address: _dynamicSalons[i].address,
        latitude: _dynamicSalons[i].latitude,
        longitude: _dynamicSalons[i].longitude,
        distanceKm: _dynamicSalons[i].distanceKm,
        isOpen: _dynamicSalons[i].isOpen,
        currentWaitTimeMinutes: newWaitTime,
        queueLength: newQueueLength,
        isFast: _dynamicSalons[i].isFast,
        isPopular: _dynamicSalons[i].isPopular,
        imageUrl: _dynamicSalons[i].imageUrl,
        businessHours: _dynamicSalons[i].businessHours,
        services: _dynamicSalons[i].services,
        rating: _dynamicSalons[i].rating,
        reviewCount: _dynamicSalons[i].reviewCount,
      );
    }
  }

  // Convert PublicSalon to Salon for compatibility with existing screens
  Salon _convertToSalon(PublicSalon publicSalon) {
    return Salon(
      name: publicSalon.name,
      address: publicSalon.address,
      waitTime: publicSalon.currentWaitTimeMinutes ?? 15,
      distance: publicSalon.distanceKm ?? 1.0,
      isOpen: publicSalon.isOpen,
      closingTime: '19:00', // Default closing time
      isFavorite: _favoriteSalons.contains(publicSalon.name),
      queueLength: publicSalon.queueLength ?? 0,
      colors: _generateSalonColors(0), // Default colors
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

  // Generate salon colors for compatibility
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

  void _generateDynamicSalons() {
    final random = math.Random();
    _dynamicSalons = [
      PublicSalon(
        id: 'barbearia_moderna',
        name: 'Barbearia Moderna',
        address: 'Rua das Flores, 123',
        latitude: -23.5505 + (random.nextDouble() - 0.5) * 0.1,
        longitude: -46.6333 + (random.nextDouble() - 0.5) * 0.1,
        distanceKm: 0.8,
        isOpen: true,
        currentWaitTimeMinutes: 25,
        queueLength: 3,
        isFast: true,
        isPopular: false,
        services: ['Haircut', 'Beard Trim', 'Hair Styling'],
        rating: 4.5,
        reviewCount: 120,
      ),
      PublicSalon(
        id: 'studio_hair',
        name: 'Studio Hair',
        address: 'Av. Paulista, 456',
        latitude: -23.5505 + (random.nextDouble() - 0.5) * 0.1,
        longitude: -46.6333 + (random.nextDouble() - 0.5) * 0.1,
        distanceKm: 1.2,
        isOpen: true,
        currentWaitTimeMinutes: 35,
        queueLength: 2,
        isFast: false,
        isPopular: true,
        services: ['Haircut', 'Beard Trim', 'Hair Wash', 'Styling'],
        rating: 4.8,
        reviewCount: 89,
      ),
      PublicSalon(
        id: 'barbearia_classica',
        name: 'Barbearia Clássica',
        address: 'Rua Augusta, 789',
        latitude: -23.5505 + (random.nextDouble() - 0.5) * 0.1,
        longitude: -46.6333 + (random.nextDouble() - 0.5) * 0.1,
        distanceKm: 1.5,
        isOpen: true,
        currentWaitTimeMinutes: 20,
        queueLength: 4,
        isFast: true,
        isPopular: true,
        services: ['Haircut', 'Beard Trim', 'Mustache Trim'],
        rating: 4.2,
        reviewCount: 67,
      ),
    ];
  }

  // Check if user is in queue for a specific salon
  bool _isInQueue(String salonName) {
    return _activeQueues.any((queue) => queue.salonName == salonName && queue.isActive);
  }

  // Get queue entry for a specific salon
  AnonymousQueueEntry? _getQueueEntry(String salonName) {
    return _activeQueues.firstWhere(
      (queue) => queue.salonName == salonName && queue.isActive,
      orElse: () => AnonymousQueueEntry(
        id: '',
        anonymousUserId: '',
        salonId: '',
        salonName: '',
        position: 0,
        estimatedWaitMinutes: 0,
        joinedAt: DateTime.now(),
        lastUpdated: DateTime.now(),
        status: QueueEntryStatus.waiting,
        serviceRequested: '',
      ),
    );
  }

  // Join queue for a salon
  // Anonymous join queue flow retained for mocks; not used in authenticated finder flow

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final size = MediaQuery.of(context).size;
    final isAnonymous = Provider.of<AppController>(context).isAnonymousMode;

    return WillPopScope(
      onWillPop: () async {
        // Handle back button press gracefully
        return true;
      },
      child: Focus(
        autofocus: false,
        child: Scaffold(
          body: Stack(
            children: [
              // Debug name for screen identification
              Positioned(
                top: MediaQuery.of(context).padding.top + 10,
                right: 10,
                child: Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: Colors.black.withOpacity(0.7),
                    borderRadius: BorderRadius.circular(4),
                  ),
                  child: const Text(
                    'SalonFinderScreen',
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 12,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ),
              // Main content
              CustomScrollView(
        slivers: [
          // Clean Hero App Bar
          SliverAppBar(
            expandedHeight: size.height * 0.23,
            floating: false,
            pinned: true,
            backgroundColor: Colors.transparent,
            elevation: 0,
            flexibleSpace: FlexibleSpaceBar(
              background: _buildHeroSection(context, theme, size),
            ),
            leading: null,
            automaticallyImplyLeading: false,
            actions: [
              if (isAnonymous) ...[
                TextButton.icon(
                  onPressed: () => Navigator.pushNamed(context, '/login'),
                  icon: const Icon(Icons.login, size: 18),
                  label: const Text('Entrar'),
                ),
                const SizedBox(width: 8),
                TextButton.icon(
                  onPressed: () => Navigator.pushNamed(context, '/register'),
                  icon: const Icon(Icons.person_add_alt, size: 18),
                  label: const Text('Criar conta'),
                ),
                const SizedBox(width: 8),
              ],
              if (!isAnonymous) ...[
                Container(
                  margin: const EdgeInsets.only(right: 8),
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                  decoration: BoxDecoration(
                    color: theme.colorScheme.onSurface.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(Icons.person, color: theme.colorScheme.onSurface, size: 16),
                      const SizedBox(width: 4),
                      Text('Conta', style: TextStyle(color: theme.colorScheme.onSurface, fontWeight: FontWeight.bold)),
                    ],
                  ),
                ),
                PopupMenuButton<String>(
                  onSelected: (value) async {
                    if (value == 'logout') {
                      final app = Provider.of<AppController>(context, listen: false);
                      await app.auth.logout();
                      if (!mounted) return;
                      Navigator.pushNamedAndRemoveUntil(context, '/home', (route) => false);
                    } else if (value == 'account') {
                      Navigator.of(context).push(
                        MaterialPageRoute(builder: (_) => const AccountScreen()),
                      );
                    }
                  },
                  itemBuilder: (context) => [
                    const PopupMenuItem(
                      value: 'account',
                      child: Text('Conta'),
                    ),
                    const PopupMenuItem(
                      value: 'logout',
                      child: Text('Sair'),
                    ),
                  ],
                ),
              ],
            ],
          ),
          
          // Main content
          SliverToBoxAdapter(
            child: Container(
              color: theme.colorScheme.surface,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Stats section
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
                    child: _buildStatsSection(context, theme, size),
                  ),
                  
                  // Active queues section (if user has active queues)
                  if (_activeQueues.isNotEmpty) ...[
                    Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
                      child: _buildActiveQueuesSection(context, theme),
                    ),
                    Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 20),
                      child: Divider(
                        color: theme.colorScheme.outline.withOpacity(0.3),
                        thickness: 1,
                        height: 32,
                      ),
                    ),
                  ] else ...[
                    // Divider
                    Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 20),
                      child: Divider(
                        color: theme.colorScheme.outline.withOpacity(0.3),
                        thickness: 1,
                        height: 32,
                      ),
                    ),
                  ],
                  
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
                        
                        // QR Scanner card
                        _buildQrScannerCard(context, theme),
                        
                        const SizedBox(height: 20),
                        
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
            ],
          ),
      bottomNavigationBar: const BottomNavBar(currentIndex: 0),
    ),
    ),
    );
  }

  Widget _buildHeroSection(BuildContext context, ThemeData theme, Size size) {
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
              child: SingleChildScrollView(
                physics: const NeverScrollableScrollPhysics(),
                clipBehavior: Clip.hardEdge,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    SizedBox(height: size.width < 400 ? 8 : 12),
                    
                    // Greeting
                    Container(
                      padding: EdgeInsets.symmetric(
                        horizontal: size.width < 400 ? 12 : 14, 
                        vertical: size.width < 400 ? 4 : 6
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
                            size: size.width < 400 ? 16 : 18
                          ),
                          const SizedBox(width: 6),
                          Text(
                            greetingName,
                            style: TextStyle(
                              color: theme.colorScheme.onPrimaryContainer,
                              fontWeight: FontWeight.bold,
                              fontSize: size.width < 400 ? 12 : 14,
                            ),
                          ),
                        ],
                      ),
                    ),
                    
                    SizedBox(height: size.width < 400 ? 12 : 16),
                    
                    // Main heading
                    Text(
                      "Transforme seu\nvisual hoje mesmo",
                      style: theme.textTheme.headlineMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                        height: 1.1,
                        color: theme.colorScheme.onSurface,
                        fontSize: size.width < 400 ? 20 : 24,
                      ),
                    ),
                    
                    SizedBox(height: size.width < 400 ? 6 : 8),
                    
                    // Simple subtitle
                    Text(
                      "Profissionais qualificados",
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: theme.colorScheme.onSurface.withOpacity(0.7),
                        fontSize: size.width < 400 ? 12 : 14,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
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
        padding: const EdgeInsets.only(bottom: 20),
        child: TweenAnimationBuilder<double>(
          tween: Tween(begin: 0.0, end: 1.0),
          duration: Duration(milliseconds: 600 + (index * 200)),
          builder: (context, value, child) {
            return Transform.translate(
              offset: Offset(0, 20 * (1 - value)),
              child: Opacity(
                opacity: value,
                child: child,
              ),
            );
          },
          child: _buildSalonCard(context, theme, _dynamicSalons[index], index),
        ),
      ),
    );
  }

  Widget _buildSalonCard(BuildContext context, ThemeData theme, PublicSalon salon, int index) {
    final isSmallScreen = MediaQuery.of(context).size.width < 600;
    final isUrgent = (salon.currentWaitTimeMinutes ?? 0) <= 10;
    final isPopular = (salon.queueLength ?? 0) >= 4;
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
              // For finder: allow navigating to details regardless of login
              Navigator.of(context).push(
                MaterialPageRoute(
                  builder: (_) => SalonDetailsScreen(
                    salon: _convertToSalon(salon),
                    services: _convertToSalonServices(salon),
                    contact: SalonContact(phone: '', email: ''),
                    businessHours: [],
                    reviews: [],
                    additionalInfo: const {},
                    publicSalon: salon,
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
                // Info chips
                if (isSmallScreen) ...[
                  Wrap(
                    spacing: 8,
                    runSpacing: 8,
                    children: [
                      _buildInfoChip(
                        context,
                        Icons.access_time,
                        '${salon.currentWaitTimeMinutes ?? 0} min',
                        chipTimeColor,
                        isSmallScreen: true,
                      ),
                      _buildInfoChip(
                        context,
                        Icons.people_outline,
                        '${salon.queueLength ?? 0} fila',
                        chipQueueColor,
                        isSmallScreen: true,
                      ),
                      _buildInfoChip(
                        context,
                        Icons.location_on_outlined,
                        '${(salon.distanceKm ?? 0).toStringAsFixed(1)} km',
                        chipDistanceColor,
                        isSmallScreen: true,
                      ),
                    ],
                  ),
                ] else ...[
                  Row(
                    children: [
                      Expanded(
                        child: _buildInfoChip(
                          context,
                          Icons.access_time,
                          '${salon.currentWaitTimeMinutes ?? 0} min',
                          chipTimeColor,
                        ),
                      ),
                      const SizedBox(width: 10),
                      Expanded(
                        child: _buildInfoChip(
                          context,
                          Icons.people_outline,
                          '${salon.queueLength ?? 0} fila',
                          chipQueueColor,
                        ),
                      ),
                      const SizedBox(width: 10),
                      Expanded(
                        child: _buildInfoChip(
                          context,
                          Icons.location_on_outlined,
                          '${(salon.distanceKm ?? 0).toStringAsFixed(1)} km',
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
                              _isInQueue(salon.name) 
                                ? 'Na fila!' 
                                : (isUrgent ? 'Check-in rápido!' : 'Fazer check-in'),
                              style: theme.textTheme.titleMedium?.copyWith(
                                color: ctaText,
                                fontWeight: FontWeight.w600,
                                fontSize: isSmallScreen ? 15 : 16,
                              ),
                            ),
                            Text(
                              _isInQueue(salon.name)
                                ? 'Posição ${_getQueueEntry(salon.name)?.position ?? 0}'
                                : (isUrgent ? 'Sem espera' : 'Entre na fila'),
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
                        onTap: _isLoading ? null : () async {
                           Navigator.of(context).push(
                             MaterialPageRoute(
                               builder: (_) => AnonymousJoinQueueScreen(salon: salon),
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
                          child: _isLoading 
                            ? SizedBox(
                                width: 18,
                                height: 18,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                   valueColor: AlwaysStoppedAnimation<Color>(buttonText),
                                ),
                              )
                            : Row(
                                mainAxisSize: MainAxisSize.min,
                                children: [
                                  Icon(
                                     Icons.login, 
                                     color: buttonText, 
                                    size: 18
                                  ),
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

  Widget _buildInfoChip(BuildContext context, IconData icon, String label, Color color, {bool isSmallScreen = false}) {
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

  Widget _buildQrScannerCard(BuildContext context, ThemeData theme) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: theme.colorScheme.secondary,
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: theme.shadowColor.withOpacity(0.15),
            blurRadius: 16,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: InkWell(
        onTap: () => Navigator.of(context).push(
          MaterialPageRoute(builder: (_) => const QrJoinScreen()),
        ),
        borderRadius: BorderRadius.circular(20),
        child: Row(
          children: [
            Container(
              width: 60,
              height: 60,
              decoration: BoxDecoration(
                color: theme.colorScheme.onSecondary.withOpacity(0.2),
                borderRadius: BorderRadius.circular(30),
              ),
              child: Icon(
                Icons.qr_code_scanner,
                color: theme.colorScheme.onSecondary,
                size: 28,
              ),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Entrar com QR Code',
                    style: theme.textTheme.titleLarge?.copyWith(
                      color: theme.colorScheme.onSecondary,
                      fontWeight: FontWeight.w600,
                      fontSize: 18,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Row(
                    children: [
                      Text(
                        'Escaneie o código do salão',
                        style: theme.textTheme.bodyMedium?.copyWith(
                          color: theme.colorScheme.onSecondary.withOpacity(0.9),
                          fontSize: 14,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                      const SizedBox(width: 4),
                      Icon(
                        Icons.arrow_forward,
                        size: 16,
                        color: theme.colorScheme.onSecondary.withOpacity(0.9),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildFindSalonCard(BuildContext context, ThemeData theme) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: theme.colorScheme.primary,
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: theme.shadowColor.withOpacity(0.15),
            blurRadius: 16,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Row(
        children: [
          Container(
            width: 60,
            height: 60,
            decoration: BoxDecoration(
              color: theme.colorScheme.onPrimary.withOpacity(0.2),
              borderRadius: BorderRadius.circular(30),
            ),
            child: Icon(
              Icons.explore,
              color: theme.colorScheme.onPrimary,
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
                    color: theme.colorScheme.onPrimary,
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
                          color: theme.colorScheme.onPrimary.withOpacity(0.9),
                          fontSize: 14,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                      const SizedBox(width: 4),
                      Icon(
                        Icons.arrow_forward,
                        size: 16,
                        color: theme.colorScheme.onPrimary.withOpacity(0.9),
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
    final double avgQueue = salonCount > 0 ? _dynamicSalons.map((s) => s.queueLength ?? 0).reduce((a, b) => a + b) / salonCount : 0;
    final double avgWait = salonCount > 0 ? _dynamicSalons.map((s) => s.currentWaitTimeMinutes ?? 0).reduce((a, b) => a + b) / salonCount : 0;
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

  Widget _buildActiveQueuesSection(BuildContext context, ThemeData theme) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.green.withOpacity(0.1),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: Colors.green.withOpacity(0.3)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(Icons.queue, color: Colors.green, size: 20),
              const SizedBox(width: 8),
              Text(
                'Suas Filas Ativas',
                style: theme.textTheme.titleMedium?.copyWith(
                  color: Colors.green,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          ..._activeQueues.map((queue) => _buildActiveQueueItem(context, theme, queue)),
        ],
      ),
    );
  }

  Widget _buildActiveQueueItem(BuildContext context, ThemeData theme, AnonymousQueueEntry queue) {
    return Container(
      margin: const EdgeInsets.only(bottom: 8),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: theme.colorScheme.surface,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.green.withOpacity(0.2)),
      ),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  queue.salonName,
                  style: theme.textTheme.bodyMedium?.copyWith(
                    fontWeight: FontWeight.w600,
                    color: theme.colorScheme.onSurface,
                  ),
                ),
                Text(
                  'Posição ${queue.position} • ${queue.estimatedWaitMinutes} min',
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
              ],
            ),
          ),
          IconButton(
            onPressed: () {
              // Find the corresponding salon
              final salon = _dynamicSalons.firstWhere(
                (s) => s.name == queue.salonName,
                orElse: () => _dynamicSalons.first,
              );
              
              Navigator.of(context).push(
                MaterialPageRoute(
                  builder: (_) => AnonymousQueueStatusScreen(
                    queueEntry: queue,
                    salon: salon,
                    queueService: _queueService,
                  ),
                ),
              );
            },
            icon: Icon(Icons.arrow_forward_ios, color: Colors.green, size: 16),
            padding: EdgeInsets.zero,
            constraints: const BoxConstraints(minWidth: 32, minHeight: 32),
          ),
        ],
      ),
    );
  }
}
