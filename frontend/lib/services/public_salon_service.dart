import '../models/public_salon.dart';
import '../config/api_config.dart';
import 'api_client.dart';
import 'package:dio/dio.dart';

/// Service for public salon data (no authentication required)
class PublicSalonService {
  final ApiClient _apiClient;

  const PublicSalonService({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  /// Creates PublicSalonService with default dependencies
  static PublicSalonService create() {
    final apiClient = ApiClient();
    return PublicSalonService(apiClient: apiClient);
  }

  /// Gets nearby salons without authentication
  Future<List<PublicSalon>> getNearbySlons({
    double? latitude,
    double? longitude,
    double radiusKm = 10.0,
    int limit = 20,
    bool openOnly = false,
  }) async {
    print('🔥 getNearbySlons() called - SHOULD NOT BE USED FOR MAIN PAGE!');
    
    final request = NearbySearchRequest(
      latitude: latitude,
      longitude: longitude,
      radiusKm: radiusKm,
      limit: limit,
      openOnly: openOnly,
    );

    final response = await _apiClient.postPublic(
      ApiConfig.publicLocationSearchEndpoint,
      data: request.toJson(),
    );

    final List<dynamic> data = response.data;
    return data.map((json) => PublicSalon.fromJson(json)).toList();
  }

  /// Gets all public salons without authentication
  Future<List<PublicSalon>> getPublicSalons() async {
    // Use direct Dio call with centralized API configuration
    final dio = Dio();
    final response = await dio.get(
      ApiConfig.getUrl(ApiConfig.publicSalonsEndpoint),
      options: Options(
        headers: ApiConfig.defaultHeaders,
      ),
    );
    
    final List<dynamic> data = response.data;
    final salons = data.map((json) => PublicSalon.fromJson(json)).toList();
    
    return salons;
  }

  /// Gets public queue status for a salon
  Future<PublicQueueStatus?> getQueueStatus(String salonId) async {
    try {
      final response = await _apiClient.getPublic(
        '${ApiConfig.publicQueueStatusEndpoint}/$salonId',
      );
      
      return PublicQueueStatus.fromJson(response.data);
    } catch (e) {
      // Return mock data if API fails
      return _getMockQueueStatus(salonId);
    }
  }

  /// Gets salon details without authentication
  Future<PublicSalon?> getSalonDetails(String salonId) async {
    try {
      final response = await _apiClient.getPublic(
        '${ApiConfig.publicSalonsEndpoint}/$salonId',
      );
      
      return PublicSalon.fromJson(response.data);
    } catch (e) {
      // Return mock data if API fails
      return _getMockSalonDetails(salonId);
    }
  }

  // Mock data methods for development/fallback
  List<PublicSalon> _getMockNearbySlons() {
    return [
      const PublicSalon(
        id: 'salon-1',
        name: 'Barbearia Moderna',
        address: 'Rua das Flores, 123',
        latitude: -23.5505,
        longitude: -46.6333,
        distanceKm: 0.8,
        isOpen: true,
        currentWaitTimeMinutes: 2,
        queueLength: 1,
        isFast: true,
        isPopular: false,
        rating: 4.8,
        reviewCount: 142,
        services: ['Corte Masculino', 'Barba', 'Sobrancelha'],
      ),
      const PublicSalon(
        id: 'salon-2',
        name: 'Salão Elite',
        address: 'Av. Paulista, 1000',
        latitude: -23.5618,
        longitude: -46.6565,
        distanceKm: 1.2,
        isOpen: true,
        currentWaitTimeMinutes: 15,
        queueLength: 3,
        isFast: false,
        isPopular: true,
        rating: 4.6,
        reviewCount: 89,
        services: ['Corte Feminino', 'Coloração', 'Escova'],
      ),
      const PublicSalon(
        id: 'salon-3',
        name: 'Barber Shop Classic',
        address: 'Rua Augusta, 456',
        latitude: -23.5519,
        longitude: -46.6593,
        distanceKm: 2.1,
        isOpen: false,
        currentWaitTimeMinutes: null,
        queueLength: 0,
        isFast: false,
        isPopular: false,
        rating: 4.2,
        reviewCount: 67,
        services: ['Corte Masculino', 'Barba Completa'],
      ),
    ];
  }



  PublicQueueStatus _getMockQueueStatus(String salonId) {
    return PublicQueueStatus(
      salonId: salonId,
      salonName: 'Barbearia Moderna',
      queueLength: 1,
      estimatedWaitTimeMinutes: 2,
      isAcceptingCustomers: true,
      lastUpdated: DateTime.now(),
      availableServices: ['Corte Masculino', 'Barba', 'Sobrancelha'],
    );
  }

  PublicSalon _getMockSalonDetails(String salonId) {
    return const PublicSalon(
      id: 'salon-1',
      name: 'Barbearia Moderna',
      address: 'Rua das Flores, 123',
      latitude: -23.5505,
      longitude: -46.6333,
      distanceKm: 0.8,
      isOpen: true,
      currentWaitTimeMinutes: 2,
      queueLength: 1,
      isFast: true,
      isPopular: false,
      rating: 4.8,
      reviewCount: 142,
      services: ['Corte Masculino', 'Barba', 'Sobrancelha'],
      businessHours: {
        'monday': '08:00-18:00',
        'tuesday': '08:00-18:00',
        'wednesday': '08:00-18:00',
        'thursday': '08:00-18:00',
        'friday': '08:00-19:00',
        'saturday': '08:00-17:00',
        'sunday': 'closed',
      },
    );
  }
}