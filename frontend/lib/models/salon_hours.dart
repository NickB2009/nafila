class SalonHours {
  final String day;
  final bool isOpen;
  final String? openTime;
  final String? closeTime;
  final String? notes;

  const SalonHours({
    required this.day,
    required this.isOpen,
    this.openTime,
    this.closeTime,
    this.notes,
  });

  factory SalonHours.fromJson(Map<String, dynamic> json) {
    return SalonHours(
      day: json['day'] as String,
      isOpen: json['isOpen'] as bool,
      openTime: json['openTime'] as String?,
      closeTime: json['closeTime'] as String?,
      notes: json['notes'] as String?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'day': day,
      'isOpen': isOpen,
      'openTime': openTime,
      'closeTime': closeTime,
      'notes': notes,
    };
  }
} 