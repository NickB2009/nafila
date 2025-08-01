import 'package:flutter/material.dart';
import '../../models/salon.dart';
import '../../models/salon_service.dart';
import '../../models/salon_contact.dart';
import '../../models/salon_hours.dart';
import '../../models/salon_review.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:flutter/services.dart';
import 'package:share_plus/share_plus.dart';
import 'check_in_screen.dart';
import '../widgets/bottom_nav_bar.dart';

class SalonDetailsScreen extends StatefulWidget {
  final Salon salon;
  final List<SalonService> services;
  final SalonContact contact;
  final List<SalonHours> businessHours;
  final List<SalonReview> reviews;
  final Map<String, dynamic> additionalInfo;

  const SalonDetailsScreen({
    super.key,
    required this.salon,
    required this.services,
    required this.contact,
    required this.businessHours,
    required this.reviews,
    this.additionalInfo = const {},
  });

  @override
  State<SalonDetailsScreen> createState() => _SalonDetailsScreenState();
}

class _SalonDetailsScreenState extends State<SalonDetailsScreen> with SingleTickerProviderStateMixin {
  late AnimationController _animationController;
  late Animation<double> _fadeAnimation;
  late Animation<Offset> _slideAnimation;
  bool _isFavorite = false;

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
      body: CustomScrollView(
        slivers: [
          SliverAppBar(
            expandedHeight: isSmallScreen ? 200 : 250,
            floating: false,
            pinned: true,
            stretch: true,
            backgroundColor: salonColors.primary,
            flexibleSpace: FlexibleSpaceBar(
              background: Stack(
                fit: StackFit.expand,
                children: [
                  // Salon Image
                  Container(
                    decoration: BoxDecoration(
                      color: salonColors.primary.withOpacity(0.1),
                    ),
                    child: Center(
                      child: Icon(
                        Icons.store,
                        size: isSmallScreen ? 48 : 64,
                        color: salonColors.primary.withOpacity(0.3),
                      ),
                    ),
                  ),
                  // Gradient Overlay
                  Container(
                    decoration: BoxDecoration(
                      gradient: LinearGradient(
                        begin: Alignment.topCenter,
                        end: Alignment.bottomCenter,
                        colors: [
                          Colors.transparent,
                          salonColors.primary.withOpacity(0.8),
                          salonColors.background,
                        ],
                        stops: const [0.4, 0.8, 1.0],
                      ),
                    ),
                  ),
                  // Decorative Elements - only show on larger screens
                  if (!isSmallScreen) Positioned(
                    top: 0,
                    right: 0,
                    child: CustomPaint(
                      size: const Size(200, 200),
                      painter: SalonDecorationPainter(
                        color: salonColors.primary.withOpacity(0.1),
                      ),
                    ),
                  ),
                ],
              ),
            ),
            leading: IconButton(
              icon: Container(
                padding: EdgeInsets.all(isSmallScreen ? 6 : 8),
                decoration: BoxDecoration(
                  color: salonColors.primary.withOpacity(0.9),
                  shape: BoxShape.circle,
                ),
                child: Icon(
                  Icons.arrow_back,
                  color: Colors.white,
                  size: isSmallScreen ? 20 : 24,
                ),
              ),
              onPressed: () => Navigator.of(context).pop(),
            ),
            actions: [
              IconButton(
                icon: Container(
                  padding: EdgeInsets.all(isSmallScreen ? 6 : 8),
                  decoration: BoxDecoration(
                    color: salonColors.primary.withOpacity(0.9),
                    shape: BoxShape.circle,
                  ),
                  child: Icon(
                    _isFavorite ? Icons.favorite : Icons.favorite_border,
                    color: Colors.white,
                    size: isSmallScreen ? 20 : 24,
                  ),
                ),
                onPressed: () {
                  setState(() {
                    _isFavorite = !_isFavorite;
                  });
                },
              ),
              IconButton(
                icon: Container(
                  padding: EdgeInsets.all(isSmallScreen ? 6 : 8),
                  decoration: BoxDecoration(
                    color: salonColors.primary.withOpacity(0.9),
                    shape: BoxShape.circle,
                  ),
                  child: Icon(
                    Icons.share,
                    color: Colors.white,
                    size: isSmallScreen ? 20 : 24,
                  ),
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
                padding: EdgeInsets.all(isSmallScreen ? 12.0 : 16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    _buildHeader(context, salonColors),
                    SizedBox(height: isSmallScreen ? 16 : 24),
                    _buildInfoSection(context, salonColors),
                    SizedBox(height: isSmallScreen ? 16 : 24),
                    _buildServicesSection(context, salonColors),
                    SizedBox(height: isSmallScreen ? 16 : 24),
                    _buildBusinessHoursSection(context, salonColors),
                    SizedBox(height: isSmallScreen ? 16 : 24),
                    _buildContactSection(context, salonColors),
                    SizedBox(height: isSmallScreen ? 16 : 24),
                    _buildReviewsSection(context, salonColors),
                    if (widget.additionalInfo.isNotEmpty) ...[
                      SizedBox(height: isSmallScreen ? 16 : 24),
                      _buildAdditionalInfoSection(context, salonColors),
                    ],
                  ],
                ),
              ),
            ),
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
                onPressed: CheckInState.isCheckedIn ? null : () {
                  Navigator.of(context).push(
                    MaterialPageRoute(
                      builder: (_) => CheckInScreen(salon: widget.salon),
                    ),
                  );
                },
                icon: Icon(Icons.check_circle, size: isSmallScreen ? 18 : 24),
                label: Text('Check In', style: TextStyle(fontSize: isSmallScreen ? 14 : 16)),
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
            ListView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              itemCount: widget.services.length,
              itemBuilder: (context, index) {
                final service = widget.services[index];
                return Card(
                  margin: const EdgeInsets.only(bottom: 12),
                  elevation: 0,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                    side: BorderSide(
                      color: theme.colorScheme.outline.withOpacity(0.1),
                    ),
                  ),
                  child: ListTile(
                    contentPadding: const EdgeInsets.all(16),
                    title: Text(
                      service.name,
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    subtitle: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
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
                    trailing: Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 8,
                      ),
                      decoration: BoxDecoration(
                        color: colors.primary.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: Text(
                        'R\$ ${service.price.toStringAsFixed(2)}',
                        style: theme.textTheme.titleMedium?.copyWith(
                          color: colors.primary,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
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
              _buildContactItem(
                context,
                Icons.phone,
                widget.contact.phone,
                () {
                  // TODO: Implement phone call
                },
                copiable: true,
                colors: colors,
              ),
              const SizedBox(height: 12),
              _buildContactItem(
                context,
                Icons.email,
                widget.contact.email,
                () {
                  // TODO: Implement email
                },
                copiable: true,
                colors: colors,
              ),
              if (widget.contact.website != null) ...[
                const SizedBox(height: 12),
                _buildContactItem(
                  context,
                  Icons.language,
                  widget.contact.website!,
                  () {
                    // TODO: Implement website
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