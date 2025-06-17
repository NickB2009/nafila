class SalonService {
  final String id;
  final String name;
  final String description;
  final double price;
  final int durationMinutes;
  final List<String>? categories;
  final Map<String, dynamic>? additionalInfo;

  const SalonService({
    required this.id,
    required this.name,
    required this.description,
    required this.price,
    required this.durationMinutes,
    this.categories,
    this.additionalInfo,
  });

  factory SalonService.fromJson(Map<String, dynamic> json) {
    return SalonService(
      id: json['id'] as String,
      name: json['name'] as String,
      description: json['description'] as String,
      price: (json['price'] as num).toDouble(),
      durationMinutes: json['durationMinutes'] as int,
      categories: (json['categories'] as List<dynamic>?)?.cast<String>(),
      additionalInfo: json['additionalInfo'] as Map<String, dynamic>?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'description': description,
      'price': price,
      'durationMinutes': durationMinutes,
      'categories': categories,
      'additionalInfo': additionalInfo,
    };
  }
} 