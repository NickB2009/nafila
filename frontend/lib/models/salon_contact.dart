class SalonContact {
  final String phone;
  final String email;
  final String? website;
  final String? instagram;
  final String? facebook;
  final Map<String, dynamic>? additionalInfo;

  const SalonContact({
    required this.phone,
    required this.email,
    this.website,
    this.instagram,
    this.facebook,
    this.additionalInfo,
  });

  factory SalonContact.fromJson(Map<String, dynamic> json) {
    return SalonContact(
      phone: json['phone'] as String,
      email: json['email'] as String,
      website: json['website'] as String?,
      instagram: json['instagram'] as String?,
      facebook: json['facebook'] as String?,
      additionalInfo: json['additionalInfo'] as Map<String, dynamic>?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'phone': phone,
      'email': email,
      'website': website,
      'instagram': instagram,
      'facebook': facebook,
      'additionalInfo': additionalInfo,
    };
  }
} 