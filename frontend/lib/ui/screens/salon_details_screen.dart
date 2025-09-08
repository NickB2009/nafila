import 'package:flutter/material.dart';
import '../../models/salon.dart';
import '../../models/public_salon.dart';
import '../../models/salon_service.dart';
import '../../models/salon_contact.dart';
import '../../models/salon_hours.dart';
import '../../models/salon_review.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:flutter/services.dart';
import 'package:share_plus/share_plus.dart';
// import removed: anonymous flow not used in authenticated check-in
import 'package:provider/provider.dart';
import '../../controllers/app_controller.dart';
import 'anonymous_join_queue_screen.dart';
import '../widgets/bottom_nav_bar.dart';
import '../../models/queue_models.dart' as queue;
import '../../models/anonymous_user.dart';
import 'anonymous_queue_status_screen.dart';

class SalonDetailsScreen extends StatefulWidget {
  final Salon salon;
  final List<SalonService> services;
  final SalonContact contact;
  final List<SalonHours> businessHours;
  final List<SalonReview> reviews;
  final Map<String, dynamic> additionalInfo;
  final PublicSalon? publicSalon;

  const SalonDetailsScreen({
    super.key,
    required this.salon,
    required this.services,
    required this.contact,
    required this.businessHours,
    required this.reviews,
    this.additionalInfo = const {},
    this.publicSalon,
  });

  @override
  State<SalonDetailsScreen> createState() => _SalonDetailsScreenState();
}

class _SalonDetailsScreenState extends State<SalonDetailsScreen> with SingleTickerProviderStateMixin {
  late AnimationController _animationController;
  late Animation<double> _fadeAnimation;
  late Animation<Offset> _slideAnimation;
  bool _isFavorite = false;
  final Set<String> _selectedServices = {};

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 800),
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

    _animationController.forward();
  }

  PublicSalon _mapSalonToPublic(Salon salon) {
    return PublicSalon(
      id: salon.name.toLowerCase().replaceAll(' ', '_'),
      name: salon.name,
      address: salon.address,
      latitude: null,
      longitude: null,
      distanceKm: salon.distance,
      isOpen: salon.isOpen,
      currentWaitTimeMinutes: salon.waitTime,
      queueLength: salon.queueLength,
      services: widget.services.map((s) => s.name).toList(),
      rating: null,
      reviewCount: null,
    );
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final brightness = Theme.of(context).brightness;
    final salonColors = widget.salon.colors.forBrightness(brightness);
    final isSmallScreen = MediaQuery.of(context).size.width < 600;
    
    return Scaffold(
      backgroundColor: salonColors.background,
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
                'SalonDetailsScreen',
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
          SliverAppBar(
            expandedHeight: 80, // Much smaller header
            floating: false,
            pinned: true,
            backgroundColor: salonColors.background,
            elevation: 0,
            title: Text(
              widget.salon.name,
              style: TextStyle(
                color: salonColors.primary,
                fontWeight: FontWeight.bold,
              ),
            ),
            leading: IconButton(
              icon: Icon(
                Icons.arrow_back,
                color: salonColors.primary,
              ),
              onPressed: () => Navigator.of(context).pop(),
            ),
            actions: [
              IconButton(
                icon: Icon(
                  _isFavorite ? Icons.favorite : Icons.favorite_border,
                  color: salonColors.primary,
                ),
                onPressed: () {
                  setState(() {
                    _isFavorite = !_isFavorite;
                  });
                },
              ),
              IconButton(
                icon: Icon(
                  Icons.share,
                  color: salonColors.primary,
                ),
                onPressed: () async {
                  final shareText = '${widget.salon.name}\n${widget.salon.address}';
                  await Share.share(shareText);
                },
              ),
              SizedBox(width: isSmallScreen ? 4 : 8),
            ],
          ),
          SliverToBoxAdapter(
            child: Container(
              color: salonColors.background,
              child: Padding(
                padding: EdgeInsets.all(isSmallScreen ? 8.0 : 12.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    _buildHeader(context, salonColors),
                    SizedBox(height: isSmallScreen ? 8 : 12),
                    _buildInfoSection(context, salonColors),
                    SizedBox(height: isSmallScreen ? 8 : 12),
                    _buildServicesSection(context, salonColors),
                    SizedBox(height: isSmallScreen ? 8 : 12),
                    _buildBusinessHoursSection(context, salonColors),
                    SizedBox(height: isSmallScreen ? 8 : 12),
                    _buildContactSection(context, salonColors),
                    SizedBox(height: isSmallScreen ? 8 : 12),
                    _buildReviewsSection(context, salonColors),
                    if (widget.additionalInfo.isNotEmpty) ...[
                      SizedBox(height: isSmallScreen ? 8 : 12),
                      _buildAdditionalInfoSection(context, salonColors),
                    ],
                  ],
                ),
              ),
            ),
          ),
        ],
      ),
        ],
      ),
      bottomNavigationBar: Container(
        padding: EdgeInsets.all(isSmallScreen ? 12 : 16),
        decoration: BoxDecoration(
          color: salonColors.background,
          boxShadow: [
            BoxShadow(
              color: theme.shadowColor.withOpacity(0.1),
              blurRadius: 10,
              offset: const Offset(0, -4),
            ),
          ],
        ),
        child: Row(
          children: [
            Expanded(
              child: ElevatedButton.icon(
                onPressed: () async {
                  final app = Provider.of<AppController>(context, listen: false);
                  if (!app.auth.isAuthenticated) {
                    await _showLoginOrGuest(context);
                  } else {
                    await _showServiceSelection(context);
                  }
                },
                icon: Icon(Icons.check_circle, size: isSmallScreen ? 18 : 24),
                label: Text('Check-in', style: TextStyle(fontSize: isSmallScreen ? 14 : 16)),
                style: ElevatedButton.styleFrom(
                  backgroundColor: salonColors.primary,
                  foregroundColor: theme.colorScheme.onPrimary,
                  padding: EdgeInsets.symmetric(vertical: isSmallScreen ? 12 : 16),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
            ),
            SizedBox(width: isSmallScreen ? 8 : 12),
            Expanded(
              child: OutlinedButton.icon(
                onPressed: () async {
                  final url = Uri.parse(
                    'https://www.google.com/maps/search/?api=1&query=${Uri.encodeComponent(widget.salon.address)}'
                  );
                  if (await canLaunchUrl(url)) {
                    await launchUrl(url, mode: LaunchMode.externalApplication);
                  } else {
                    if (context.mounted) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(
                          content: Text('Não foi possível abrir o mapa'),
                        ),
                      );
                    }
                  }
                },
                icon: Icon(Icons.directions, size: isSmallScreen ? 18 : 24),
                label: Text('Como Chegar', style: TextStyle(fontSize: isSmallScreen ? 14 : 16)),
                style: OutlinedButton.styleFrom(
                  foregroundColor: salonColors.primary,
                  padding: EdgeInsets.symmetric(vertical: isSmallScreen ? 12 : 16),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                  side: BorderSide(color: salonColors.primary),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _showServiceSelection(BuildContext context) async {
    await showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
      ),
      builder: (ctx) {
        final theme = Theme.of(ctx);
        return StatefulBuilder(
          builder: (BuildContext context, void Function(void Function()) setSheetState) {
            return Padding(
              padding: EdgeInsets.only(
                bottom: MediaQuery.of(ctx).viewInsets.bottom + 16,
                left: 16,
                right: 16,
                top: 16,
              ),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Selecione os serviços', style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
                  const SizedBox(height: 12),
                  Wrap(
                    spacing: 8,
                    runSpacing: 6,
                    children: widget.services.map((s) {
                      final isSelected = _selectedServices.contains(s.id);
                      return FilterChip(
                        label: Text(s.name),
                        selected: isSelected,
                        onSelected: (selected) {
                          setSheetState(() {
                            if (selected) {
                              _selectedServices.add(s.id);
                            } else {
                              _selectedServices.remove(s.id);
                            }
                          });
                        },
                        selectedColor: theme.colorScheme.primary.withOpacity(0.15),
                        checkmarkColor: theme.colorScheme.primary,
                        side: BorderSide(
                          color: isSelected
                              ? theme.colorScheme.primary
                              : theme.colorScheme.outline.withOpacity(0.5),
                        ),
                      );
                    }).toList(),
                  ),
                  const SizedBox(height: 16),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: _selectedServices.isEmpty
                          ? null
                          : () async {
                              Navigator.of(ctx).pop();
                              final app = Provider.of<AppController>(context, listen: false);
                              final queueId = (widget.additionalInfo['queueId'] is String)
                                  ? widget.additionalInfo['queueId'] as String
                                  : null;
                              final publicId = widget.publicSalon?.id;
                              final resolvedQueueId = queueId ?? (publicId != null && _isValidGuid(publicId) ? publicId : null);

                              // Try backend integration if queueId is available
                              if (resolvedQueueId != null && app.auth.isAuthenticated) {
                                final selectedNames = widget.services
                                    .where((s) => _selectedServices.contains(s.id))
                                    .map((s) => s.name)
                                    .toList();
                                final ok = await app.queue.joinQueue(
                                  resolvedQueueId,
                                  queue.JoinQueueRequest(
                                    queueId: resolvedQueueId,
                                    customerName: app.auth.currentUser?.fullName ?? app.auth.currentPhoneNumber,
                                    email: null,
                                    isAnonymous: false,
                                    notes: selectedNames.join(', '),
                                    serviceTypeId: null,
                                  ),
                                );
                                if (!mounted) return;
                                if (ok) {
                                  // Mark global check-in for authenticated flow
                                  CheckInState.isCheckedIn = true;
                                  CheckInState.checkedInSalon = widget.salon;
                                  // Use the unified anonymous-style status screen for both flows
                                  final entry = AnonymousQueueEntry(
                                    id: 'entry',
                                    anonymousUserId: 'auth',
                                    salonId: widget.salon.name,
                                    salonName: widget.salon.name,
                                    position: 1,
                                    estimatedWaitMinutes: widget.salon.waitTime,
                                    joinedAt: DateTime.now(),
                                    lastUpdated: DateTime.now(),
                                    status: QueueEntryStatus.waiting,
                                    serviceRequested: selectedNames.join(', '),
                                  );
                                  final publicEquivalent = PublicSalon(
                                    id: widget.salon.name,
                                    name: widget.salon.name,
                                    address: widget.salon.address,
                                    latitude: null,
                                    longitude: null,
                                    distanceKm: widget.salon.distance,
                                    isOpen: widget.salon.isOpen,
                                    currentWaitTimeMinutes: widget.salon.waitTime,
                                    queueLength: widget.salon.queueLength,
                                    services: widget.services.map((s) => s.name).toList(),
                                    isFast: false,
                                    isPopular: false,
                                    rating: null,
                                    reviewCount: null,
                                  );
                                  Navigator.of(context).pushReplacement(
                                    MaterialPageRoute(
                                      builder: (_) => AnonymousQueueStatusScreen(
                                        queueEntry: entry,
                                        salon: publicEquivalent,
                                      ),
                                    ),
                                  );
                                } else {
                                  ScaffoldMessenger.of(context).showSnackBar(
                                    SnackBar(content: Text(app.queue.error ?? 'Falha ao entrar na fila')),
                                  );
                                }
                              } else {
                                // Fallback: create local queue entry and show the anonymous status screen
                                final selectedNames = widget.services
                                    .where((s) => _selectedServices.contains(s.id))
                                    .map((s) => s.name)
                                    .toList();
                                CheckInState.isCheckedIn = true;
                                CheckInState.checkedInSalon = widget.salon;
                                final entry = AnonymousQueueEntry(
                                  id: 'local-${DateTime.now().millisecondsSinceEpoch}',
                                  anonymousUserId: 'auth',
                                  salonId: widget.salon.name,
                                  salonName: widget.salon.name,
                                  position: 1,
                                  estimatedWaitMinutes: widget.salon.waitTime,
                                  joinedAt: DateTime.now(),
                                  lastUpdated: DateTime.now(),
                                  status: QueueEntryStatus.waiting,
                                  serviceRequested: selectedNames.join(', '),
                                );
                                final publicEquivalent = PublicSalon(
                                  id: widget.salon.name,
                                  name: widget.salon.name,
                                  address: widget.salon.address,
                                  latitude: null,
                                  longitude: null,
                                  distanceKm: widget.salon.distance,
                                  isOpen: widget.salon.isOpen,
                                  currentWaitTimeMinutes: widget.salon.waitTime,
                                  queueLength: widget.salon.queueLength,
                                  services: widget.services.map((s) => s.name).toList(),
                                  isFast: false,
                                  isPopular: false,
                                  rating: null,
                                  reviewCount: null,
                                );
                                if (mounted) {
                                  Navigator.of(context).pushReplacement(
                                    MaterialPageRoute(
                                      builder: (_) => AnonymousQueueStatusScreen(
                                        queueEntry: entry,
                                        salon: publicEquivalent,
                                      ),
                                    ),
                                  );
                                }
                              }
                            },
                      child: const Text('Confirmar check-in'),
                    ),
                  ),
                ],
              ),
            );
          },
        );
      },
    );
  }

  bool _isValidGuid(String value) {
    final guidRegex = RegExp(r'^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12} ?$');
    return guidRegex.hasMatch(value);
  }

  Future<void> _showLoginOrGuest(BuildContext context) async {
    final ps = widget.publicSalon ?? _mapSalonToPublic(widget.salon);
    await showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
      ),
      builder: (ctx) {
        final theme = Theme.of(ctx);
        return Padding(
          padding: const EdgeInsets.all(16.0),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Como deseja continuar?', style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
              const SizedBox(height: 8),
              Text('Faça login para salvar histórico e favoritos ou entre como convidado para testar.'),
              const SizedBox(height: 16),
              Row(
                children: [
                  Expanded(
                    child: OutlinedButton.icon(
                      onPressed: () {
                    Navigator.of(ctx).pop();
                    Navigator.of(context).pushNamed(
                      '/login',
                      arguments: {
                        'fromCheckIn': true,
                        'salon': ps,
                      },
                    );
                      },
                      icon: const Icon(Icons.login),
                      label: const Text('Fazer login'),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: ElevatedButton.icon(
                      onPressed: () {
                        Navigator.of(ctx).pop();
                        Navigator.of(context).push(
                          MaterialPageRoute(
                            builder: (_) => AnonymousJoinQueueScreen(salon: ps),
                          ),
                        );
                      },
                      icon: const Icon(Icons.person_outline),
                      label: const Text('Entrar como convidado'),
                    ),
                  ),
                ],
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildHeader(BuildContext context, SalonColors colors) {
    final theme = Theme.of(context);
    
    return FadeTransition(
      opacity: _fadeAnimation,
      child: SlideTransition(
        position: _slideAnimation,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              widget.salon.name,
              style: theme.textTheme.headlineMedium?.copyWith(
                fontWeight: FontWeight.bold,
                shadows: [
                  Shadow(
                    color: colors.primary.withOpacity(0.2),
                    offset: const Offset(0, 2),
                    blurRadius: 4,
                  ),
                ],
              ),
            ),
            const SizedBox(height: 8),
            Row(
              children: [
                Icon(
                  Icons.location_on_outlined,
                  size: 16,
                  color: theme.colorScheme.onSurface.withOpacity(0.7),
                ),
                const SizedBox(width: 4),
                Expanded(
                  child: Text(
                    widget.salon.address,
                    style: theme.textTheme.bodyLarge?.copyWith(
                      color: theme.colorScheme.onSurface.withOpacity(0.7),
                    ),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildInfoSection(BuildContext context, SalonColors colors) {
    final theme = Theme.of(context);
    final isSmallScreen = MediaQuery.of(context).size.width < 600;
    
    return FadeTransition(
      opacity: _fadeAnimation,
      child: SlideTransition(
        position: _slideAnimation,
        child: Container(
          padding: const EdgeInsets.all(20),
          decoration: BoxDecoration(
            color: colors.background,
            borderRadius: BorderRadius.circular(16),
            boxShadow: [
              BoxShadow(
                color: theme.shadowColor.withOpacity(0.05),
                blurRadius: 10,
                offset: const Offset(0, 4),
              ),
            ],
            gradient: LinearGradient(
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
              colors: [
                colors.background,
                colors.background.withOpacity(0.95),
              ],
            ),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Informações',
                style: theme.textTheme.titleLarge?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 16),
              Wrap(
                spacing: isSmallScreen ? 6 : 8,
                runSpacing: isSmallScreen ? 6 : 8,
                children: [
                  _buildInfoChip(
                    context,
                    Icons.access_time,
                    '${widget.salon.waitTime} min',
                    colors.primary,
                    isSmallScreen: isSmallScreen,
                  ),
                  _buildInfoChip(
                    context,
                    Icons.people_outline,
                    '${widget.salon.queueLength} na fila',
                    colors.primary,
                    isSmallScreen: isSmallScreen,
                  ),
                  _buildInfoChip(
                    context,
                    Icons.location_on_outlined,
                    '${widget.salon.distance} km',
                    colors.primary,
                    isSmallScreen: isSmallScreen,
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildServicesSection(BuildContext context, SalonColors colors) {
    final theme = Theme.of(context);
    
    return FadeTransition(
      opacity: _fadeAnimation,
      child: SlideTransition(
        position: _slideAnimation,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Serviços',
              style: theme.textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 16),
            Container(
              decoration: BoxDecoration(
                color: theme.colorScheme.surface,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: theme.colorScheme.outline.withOpacity(0.2),
                ),
              ),
              child: Column(
                children: widget.services.asMap().entries.map((entry) {
                  final index = entry.key;
                  final service = entry.value;
                  final isLast = index == widget.services.length - 1;
                  
                  return Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      border: isLast ? null : Border(
                        bottom: BorderSide(
                          color: theme.colorScheme.outline.withOpacity(0.1),
                          width: 1,
                        ),
                      ),
                    ),
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Service icon
                        Container(
                          width: 40,
                          height: 40,
                          decoration: BoxDecoration(
                            color: colors.primary.withOpacity(0.1),
                            borderRadius: BorderRadius.circular(8),
                          ),
                          child: Icon(
                            Icons.content_cut,
                            color: colors.primary,
                            size: 20,
                          ),
                        ),
                        const SizedBox(width: 12),
                        // Service details
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                service.name,
                                style: theme.textTheme.titleMedium?.copyWith(
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                service.description,
                                style: theme.textTheme.bodyMedium?.copyWith(
                                  color: theme.colorScheme.onSurface.withOpacity(0.7),
                                ),
                              ),
                              const SizedBox(height: 8),
                              Row(
                                children: [
                                  Icon(
                                    Icons.timer_outlined,
                                    size: 16,
                                    color: colors.primary,
                                  ),
                                  const SizedBox(width: 4),
                                  Text(
                                    '${service.durationMinutes} min',
                                    style: theme.textTheme.bodySmall?.copyWith(
                                      color: colors.primary,
                                    ),
                                  ),
                                ],
                              ),
                            ],
                          ),
                        ),
                        // Price
                        Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: 12,
                            vertical: 6,
                          ),
                          decoration: BoxDecoration(
                            color: colors.primary.withOpacity(0.1),
                            borderRadius: BorderRadius.circular(6),
                          ),
                          child: Text(
                            'R\$ ${service.price.toStringAsFixed(2)}',
                            style: theme.textTheme.titleMedium?.copyWith(
                              color: colors.primary,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                      ],
                    ),
                  );
                }).toList(),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildBusinessHoursSection(BuildContext context, SalonColors colors) {
    final theme = Theme.of(context);
    
    return FadeTransition(
      opacity: _fadeAnimation,
      child: SlideTransition(
        position: _slideAnimation,
        child: Container(
          padding: const EdgeInsets.all(20),
          decoration: BoxDecoration(
            color: colors.background,
            borderRadius: BorderRadius.circular(16),
            boxShadow: [
              BoxShadow(
                color: theme.shadowColor.withOpacity(0.05),
                blurRadius: 10,
                offset: const Offset(0, 4),
              ),
            ],
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Horário de Funcionamento',
                style: theme.textTheme.titleLarge?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 16),
              ...widget.businessHours.map((hours) => Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      hours.day,
                      style: theme.textTheme.bodyLarge,
                    ),
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 12,
                        vertical: 6,
                      ),
                      decoration: BoxDecoration(
                        color: hours.isOpen
                            ? colors.primary.withOpacity(0.1)
                            : theme.colorScheme.error.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(
                            hours.isOpen ? Icons.circle : Icons.circle_outlined,
                            size: 12,
                            color: hours.isOpen
                                ? colors.primary
                                : theme.colorScheme.error,
                          ),
                          const SizedBox(width: 4),
                          Text(
                            hours.isOpen ? '${hours.openTime} - ${hours.closeTime}' : 'Fechado',
                            style: theme.textTheme.labelMedium?.copyWith(
                              color: hours.isOpen
                                  ? colors.primary
                                  : theme.colorScheme.error,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              )),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildContactSection(BuildContext context, SalonColors colors) {
    final theme = Theme.of(context);
    
    return FadeTransition(
      opacity: _fadeAnimation,
      child: SlideTransition(
        position: _slideAnimation,
        child: Container(
          padding: const EdgeInsets.all(20),
          decoration: BoxDecoration(
            color: colors.background,
            borderRadius: BorderRadius.circular(16),
            boxShadow: [
              BoxShadow(
                color: theme.shadowColor.withOpacity(0.05),
                blurRadius: 10,
                offset: const Offset(0, 4),
              ),
            ],
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Contato',
                style: theme.textTheme.titleLarge?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 16),
              // Compact contact row with phone and WhatsApp
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: theme.colorScheme.surface,
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(
                    color: theme.colorScheme.outline.withOpacity(0.1),
                  ),
                ),
                child: Row(
                  children: [
                    // Phone number display
                    Expanded(
                      child: Row(
                        children: [
                          Container(
                            padding: const EdgeInsets.all(8),
                            decoration: BoxDecoration(
                              color: colors.primary.withOpacity(0.1),
                              borderRadius: BorderRadius.circular(8),
                            ),
                            child: Icon(
                              Icons.phone,
                              size: 18,
                              color: colors.primary,
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: Text(
                              widget.contact.phone,
                              style: theme.textTheme.bodyMedium?.copyWith(
                                fontWeight: FontWeight.w500,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(width: 16),
                    // WhatsApp button
                    InkWell(
                      onTap: () async {
                        final phoneNumber = widget.contact.phone.replaceAll(RegExp(r'[^\d+]'), '');
                        final whatsappUrl = Uri.parse('https://wa.me/$phoneNumber');
                        if (await canLaunchUrl(whatsappUrl)) {
                          await launchUrl(whatsappUrl, mode: LaunchMode.externalApplication);
                        } else {
                          if (context.mounted) {
                            ScaffoldMessenger.of(context).showSnackBar(
                              const SnackBar(content: Text('Não foi possível abrir o WhatsApp')),
                            );
                          }
                        }
                      },
                      borderRadius: BorderRadius.circular(8),
                      child: Container(
                        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                        decoration: BoxDecoration(
                          color: const Color(0xFF25D366).withOpacity(0.1),
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(
                            color: const Color(0xFF25D366).withOpacity(0.3),
                          ),
                        ),
                        child: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Icon(
                              Icons.chat,
                              size: 18,
                              color: const Color(0xFF25D366),
                            ),
                            const SizedBox(width: 6),
                            Text(
                              'WhatsApp',
                              style: theme.textTheme.bodySmall?.copyWith(
                                color: const Color(0xFF25D366),
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              if (widget.contact.website != null) ...[
                const SizedBox(height: 12),
                _buildContactItem(
                  context,
                  Icons.language,
                  widget.contact.website!,
                  () async {
                    final url = Uri.parse(widget.contact.website!);
                    if (await canLaunchUrl(url)) {
                      await launchUrl(url, mode: LaunchMode.externalApplication);
                    } else {
                      if (context.mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(content: Text('Não foi possível abrir o website')),
                        );
                      }
                    }
                  },
                  colors: colors,
                ),
              ],
              if (widget.contact.instagram != null || widget.contact.facebook != null) ...[
                const SizedBox(height: 16),
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    if (widget.contact.instagram != null)
                      IconButton(
                        onPressed: () async {
                          final handle = widget.contact.instagram!.replaceAll('@', '');
                          final url = Uri.parse('https://instagram.com/$handle');
                          if (await canLaunchUrl(url)) {
                            await launchUrl(url, mode: LaunchMode.externalApplication);
                          } else {
                            if (context.mounted) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                const SnackBar(content: Text('Não foi possível abrir o Instagram')),
                              );
                            }
                          }
                        },
                        icon: const Icon(Icons.camera_alt),
                        style: IconButton.styleFrom(
                          backgroundColor: colors.primary.withOpacity(0.1),
                          padding: const EdgeInsets.all(12),
                        ),
                      ),
                    if (widget.contact.facebook != null) ...[
                      const SizedBox(width: 16),
                      IconButton(
                        onPressed: () async {
                          final page = widget.contact.facebook!;
                          final url = Uri.parse('https://facebook.com/$page');
                          if (await canLaunchUrl(url)) {
                            await launchUrl(url, mode: LaunchMode.externalApplication);
                          } else {
                            if (context.mounted) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                const SnackBar(content: Text('Não foi possível abrir o Facebook')),
                              );
                            }
                          }
                        },
                        icon: const Icon(Icons.facebook),
                        style: IconButton.styleFrom(
                          backgroundColor: colors.primary.withOpacity(0.1),
                          padding: const EdgeInsets.all(12),
                        ),
                      ),
                    ],
                  ],
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildReviewsSection(BuildContext context, SalonColors colors) {
    final theme = Theme.of(context);
    
    return FadeTransition(
      opacity: _fadeAnimation,
      child: SlideTransition(
        position: _slideAnimation,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Avaliações',
                  style: theme.textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                TextButton.icon(
                  onPressed: () {
                    Navigator.of(context).push(
                      PageRouteBuilder(
                        pageBuilder: (context, animation, secondaryAnimation) => AllReviewsScreen(reviews: widget.reviews),
                        transitionsBuilder: (context, animation, secondaryAnimation, child) {
                          return FadeTransition(
                            opacity: animation,
                            child: child,
                          );
                        },
                      ),
                    );
                  },
                  icon: const Icon(Icons.arrow_forward),
                  label: const Text('Ver todas'),
                ),
              ],
            ),
            const SizedBox(height: 16),
            ListView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              itemCount: widget.reviews.take(3).length,
              itemBuilder: (context, index) {
                final review = widget.reviews[index];
                return Card(
                  margin: const EdgeInsets.only(bottom: 12),
                  elevation: 0,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                    side: BorderSide(
                      color: theme.colorScheme.outline.withOpacity(0.1),
                    ),
                  ),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            CircleAvatar(
                              radius: 20,
                              backgroundColor: colors.primary.withOpacity(0.1),
                              child: Text(
                                review.userName[0].toUpperCase(),
                                style: TextStyle(
                                  color: colors.primary,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    review.userName,
                                    style: theme.textTheme.titleMedium?.copyWith(
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                  Text(
                                    review.date,
                                    style: theme.textTheme.bodySmall?.copyWith(
                                      color: theme.colorScheme.onSurface.withOpacity(0.6),
                                    ),
                                  ),
                                ],
                              ),
                            ),
                            Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 8,
                                vertical: 4,
                              ),
                              decoration: BoxDecoration(
                                color: colors.primary.withOpacity(0.1),
                                borderRadius: BorderRadius.circular(8),
                              ),
                              child: Row(
                                mainAxisSize: MainAxisSize.min,
                                children: [
                                  Icon(
                                    Icons.star,
                                    size: 16,
                                    color: colors.primary,
                                  ),
                                  const SizedBox(width: 4),
                                  Text(
                                    review.rating.toString(),
                                    style: theme.textTheme.labelMedium?.copyWith(
                                      color: colors.primary,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ),
                        if (review.comment != null) ...[
                          const SizedBox(height: 12),
                          Text(
                            review.comment!,
                            style: theme.textTheme.bodyMedium,
                          ),
                        ],
                      ],
                    ),
                  ),
                );
              },
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildAdditionalInfoSection(BuildContext context, SalonColors colors) {
    final theme = Theme.of(context);
    
    return FadeTransition(
      opacity: _fadeAnimation,
      child: SlideTransition(
        position: _slideAnimation,
        child: Container(
          padding: const EdgeInsets.all(20),
          decoration: BoxDecoration(
            color: colors.background,
            borderRadius: BorderRadius.circular(16),
            boxShadow: [
              BoxShadow(
                color: theme.shadowColor.withOpacity(0.05),
                blurRadius: 10,
                offset: const Offset(0, 4),
              ),
            ],
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Informações Adicionais',
                style: theme.textTheme.titleLarge?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 16),
              ...widget.additionalInfo.entries.map((entry) => Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 12,
                        vertical: 6,
                      ),
                      decoration: BoxDecoration(
                        color: colors.primary.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: Text(
                        entry.key,
                        style: theme.textTheme.labelMedium?.copyWith(
                          color: colors.primary,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Text(
                        entry.value.toString(),
                        style: theme.textTheme.bodyLarge,
                      ),
                    ),
                  ],
                ),
              )),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildInfoChip(BuildContext context, IconData icon, String label, Color color, {bool isSmallScreen = false}) {
    return Container(
      padding: EdgeInsets.symmetric(horizontal: isSmallScreen ? 8 : 12, vertical: 6),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: isSmallScreen ? 14 : 16, color: color),
          const SizedBox(width: 4),
          Flexible(
            child: Text(
              label,
              style: Theme.of(context).textTheme.labelMedium?.copyWith(
                color: color,
                fontSize: isSmallScreen ? 11 : null,
              ),
              overflow: TextOverflow.ellipsis,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildContactItem(BuildContext context, IconData icon, String text, VoidCallback onTap, {bool copiable = false, required SalonColors colors}) {
    final theme = Theme.of(context);
    return InkWell(
      onTap: onTap,
      onLongPress: copiable
          ? () async {
              await Clipboard.setData(ClipboardData(text: text));
              if (context.mounted) {
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(content: Text('Copiado!'), duration: Duration(seconds: 1)),
                );
              }
            }
          : null,
      child: GestureDetector(
        onTap: copiable
            ? () async {
                await Clipboard.setData(ClipboardData(text: text));
                if (context.mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(content: Text('Copiado!'), duration: Duration(seconds: 1)),
                  );
                }
              }
            : null,
        child: Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: theme.colorScheme.surface,
            borderRadius: BorderRadius.circular(12),
            border: Border.all(
              color: theme.colorScheme.outline.withOpacity(0.1),
            ),
          ),
          child: Row(
            children: [
              Container(
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: colors.primary.withOpacity(0.1),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Icon(
                  icon,
                  size: 20,
                  color: colors.primary,
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Text(
                  text,
                  style: theme.textTheme.bodyLarge,
                ),
              ),
              Icon(
                Icons.arrow_forward_ios,
                size: 16,
                color: theme.colorScheme.onSurface.withOpacity(0.5),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class SalonDecorationPainter extends CustomPainter {
  final Color color;

  SalonDecorationPainter({required this.color});

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = color
      ..style = PaintingStyle.fill;

    final path = Path()
      ..moveTo(size.width, 0)
      ..quadraticBezierTo(
        size.width * 0.8,
        size.height * 0.2,
        size.width * 0.6,
        size.height * 0.1,
      )
      ..quadraticBezierTo(
        size.width * 0.4,
        0,
        size.width * 0.2,
        size.height * 0.2,
      )
      ..quadraticBezierTo(
        0,
        size.height * 0.4,
        0,
        size.height * 0.6,
      )
      ..quadraticBezierTo(
        size.width * 0.2,
        size.height * 0.8,
        size.width * 0.4,
        size.height * 0.7,
      )
      ..quadraticBezierTo(
        size.width * 0.6,
        size.height * 0.6,
        size.width * 0.8,
        size.height * 0.8,
      )
      ..quadraticBezierTo(
        size.width,
        size.height,
        size.width,
        0,
      );

    canvas.drawPath(path, paint);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
} 

class AllReviewsScreen extends StatelessWidget {
  final List<SalonReview> reviews;
  const AllReviewsScreen({super.key, required this.reviews});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final brightness = Theme.of(context).brightness;
    final colors = CheckInState.checkedInSalon?.colors.forBrightness(brightness);
    
    return Scaffold(
      backgroundColor: colors?.background ?? theme.colorScheme.surface,
      appBar: AppBar(
        title: Text(
          'Todas as Avaliações',
          style: theme.textTheme.titleLarge?.copyWith(
            color: colors?.onSurface ?? theme.colorScheme.onSurface,
            fontWeight: FontWeight.bold,
          ),
        ),
        backgroundColor: colors?.background ?? theme.colorScheme.surface,
        iconTheme: IconThemeData(color: colors?.primary ?? theme.colorScheme.primary),
        elevation: 0,
      ),
      body: reviews.isEmpty
          ? Center(
              child: Text(
                'Nenhuma avaliação ainda.',
                style: theme.textTheme.bodyLarge?.copyWith(
                  color: colors?.secondary ?? theme.colorScheme.onSurfaceVariant,
                ),
              ),
            )
          : ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: reviews.length,
              itemBuilder: (context, index) {
                final review = reviews[index];
                return Card(
                  margin: const EdgeInsets.only(bottom: 12),
                  elevation: 0,
                  color: colors?.background ?? theme.colorScheme.surface,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                    side: BorderSide(
                      color: (colors?.secondary ?? theme.colorScheme.outline).withOpacity(0.1),
                    ),
                  ),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            CircleAvatar(
                              radius: 20,
                              backgroundColor: (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.1),
                              child: Text(
                                review.userName[0].toUpperCase(),
                                style: TextStyle(
                                  color: colors?.primary ?? theme.colorScheme.primary,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    review.userName,
                                    style: theme.textTheme.titleMedium?.copyWith(
                                      fontWeight: FontWeight.bold,
                                      color: colors?.onSurface ?? theme.colorScheme.onSurface,
                                    ),
                                  ),
                                  Text(
                                    review.date,
                                    style: theme.textTheme.bodySmall?.copyWith(
                                      color: (colors?.secondary ?? theme.colorScheme.onSurface).withOpacity(0.6),
                                    ),
                                  ),
                                ],
                              ),
                            ),
                            Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 8,
                                vertical: 4,
                              ),
                              decoration: BoxDecoration(
                                color: (colors?.primary ?? theme.colorScheme.primary).withOpacity(0.1),
                                borderRadius: BorderRadius.circular(8),
                              ),
                              child: Row(
                                mainAxisSize: MainAxisSize.min,
                                children: [
                                  Icon(
                                    Icons.star,
                                    size: 16,
                                    color: colors?.primary ?? theme.colorScheme.primary,
                                  ),
                                  const SizedBox(width: 4),
                                  Text(
                                    review.rating.toString(),
                                    style: theme.textTheme.labelMedium?.copyWith(
                                      color: colors?.primary ?? theme.colorScheme.primary,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ),
                        if (review.comment != null) ...[
                          const SizedBox(height: 12),
                          Text(
                            review.comment!,
                            style: theme.textTheme.bodyMedium?.copyWith(
                              color: colors?.onSurface ?? theme.colorScheme.onSurface,
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                );
              },
            ),
    );
  }
} 