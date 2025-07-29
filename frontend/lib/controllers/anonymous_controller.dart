import 'package:flutter/foundation.dart';
import '../services/public_salon_service.dart';
import '../models/public_salon.dart';

/// Controller for anonymous users to browse public salon data
class AnonymousController extends ChangeNotifier {
  final PublicSalonService _publicSalonService;

  AnonymousController({required PublicSalonService publicSalonService}) 
      : _publicSalonService = publicSalonService;

  // State management
  bool _isLoading = false;
  String? _error;
  List<PublicSalon> _nearbySalons = [];
  PublicQueueStatus? _selectedQueueStatus;
  PublicSalon? _selectedSalon;

  // Getters
  bool get isLoading => _isLoading;
  String? get error => _error;
  List<PublicSalon> get nearbySalons => _nearbySalons;
  PublicQueueStatus? get selectedQueueStatus => _selectedQueueStatus;
  PublicSalon? get selectedSalon => _selectedSalon;

  /// Sets loading state and notifies listeners
  void _setLoading(bool loading) {
    _isLoading = loading;
    notifyListeners();
  }

  /// Sets error state and notifies listeners
  void _setError(String? error) {
    _error = error;
    notifyListeners();
  }

  /// Clears error state
  void clearError() {
    _setError(null);
  }

  /// Loads nearby salons for anonymous browsing
  Future<void> loadNearbySalons({
    double? latitude,
    double? longitude,
    double radiusKm = 10.0,
    bool openOnly = false,
  }) async {
    try {
      _setLoading(true);
      _setError(null);

      final salons = await _publicSalonService.getNearbySlons(
        latitude: latitude,
        longitude: longitude,
        radiusKm: radiusKm,
        openOnly: openOnly,
      );

      _nearbySalons = salons;
      notifyListeners();
    } catch (e) {
      _setError('Error loading nearby salons: ${e.toString()}');
    } finally {
      _setLoading(false);
    }
  }

  /// Loads all public salons
  Future<void> loadPublicSalons() async {
    try {
      _setLoading(true);
      _setError(null);

      final salons = await _publicSalonService.getPublicSalons();
      _nearbySalons = salons;
      notifyListeners();
    } catch (e) {
      _setError('Error loading salons: ${e.toString()}');
    } finally {
      _setLoading(false);
    }
  }

  /// Gets queue status for a specific salon
  Future<void> getQueueStatus(String salonId) async {
    try {
      _setLoading(true);
      _setError(null);

      final queueStatus = await _publicSalonService.getQueueStatus(salonId);
      _selectedQueueStatus = queueStatus;
      notifyListeners();
    } catch (e) {
      _setError('Error loading queue status: ${e.toString()}');
    } finally {
      _setLoading(false);
    }
  }

  /// Gets detailed information for a specific salon
  Future<void> getSalonDetails(String salonId) async {
    try {
      _setLoading(true);
      _setError(null);

      final salon = await _publicSalonService.getSalonDetails(salonId);
      _selectedSalon = salon;
      notifyListeners();
    } catch (e) {
      _setError('Error loading salon details: ${e.toString()}');
    } finally {
      _setLoading(false);
    }
  }

  /// Attempts to trigger check-in flow (will prompt for login)
  Future<bool> attemptCheckIn(String salonId) async {
    // This method indicates that the user wants to check in
    // The UI should detect this and prompt for login
    // For now, we'll just return false to indicate authentication is needed
    return false;
  }

  /// Refreshes the current data
  Future<void> refresh() async {
    if (_nearbySalons.isNotEmpty) {
      await loadPublicSalons();
    }
  }

  /// Clears all state
  void clear() {
    _nearbySalons = [];
    _selectedQueueStatus = null;
    _selectedSalon = null;
    _error = null;
    _isLoading = false;
    notifyListeners();
  }

  /// Gets fast salons (with "RÁPIDO" badge)
  List<PublicSalon> get fastSalons {
    return _nearbySalons.where((salon) => salon.isFast).toList();
  }

  /// Gets popular salons (with "POPULAR" badge)
  List<PublicSalon> get popularSalons {
    return _nearbySalons.where((salon) => salon.isPopular).toList();
  }

  /// Gets open salons only
  List<PublicSalon> get openSalons {
    return _nearbySalons.where((salon) => salon.isOpen).toList();
  }

  /// Gets salons sorted by distance (closest first)
  List<PublicSalon> get salonsByDistance {
    final sortedSalons = List<PublicSalon>.from(_nearbySalons);
    sortedSalons.sort((a, b) {
      if (a.distanceKm == null && b.distanceKm == null) return 0;
      if (a.distanceKm == null) return 1;
      if (b.distanceKm == null) return -1;
      return a.distanceKm!.compareTo(b.distanceKm!);
    });
    return sortedSalons;
  }

  /// Gets salons sorted by wait time (shortest first)
  List<PublicSalon> get salonsByWaitTime {
    final sortedSalons = List<PublicSalon>.from(_nearbySalons);
    sortedSalons.sort((a, b) {
      if (a.currentWaitTimeMinutes == null && b.currentWaitTimeMinutes == null) return 0;
      if (a.currentWaitTimeMinutes == null) return 1;
      if (b.currentWaitTimeMinutes == null) return -1;
      return a.currentWaitTimeMinutes!.compareTo(b.currentWaitTimeMinutes!);
    });
    return sortedSalons;
  }
}