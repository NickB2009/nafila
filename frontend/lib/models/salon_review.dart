class SalonReview {
  final String id;
  final String userName;
  final double rating;
  final String? comment;
  final String date;
  final Map<String, dynamic>? additionalInfo;

  const SalonReview({
    required this.id,
    required this.userName,
    required this.rating,
    this.comment,
    required this.date,
    this.additionalInfo,
  });

  factory SalonReview.fromJson(Map<String, dynamic> json) {
    return SalonReview(
      id: json['id'] as String,
      userName: json['userName'] as String,
      rating: (json['rating'] as num).toDouble(),
      comment: json['comment'] as String?,
      date: json['date'] as String,
      additionalInfo: json['additionalInfo'] as Map<String, dynamic>?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'userName': userName,
      'rating': rating,
      'comment': comment,
      'date': date,
      'additionalInfo': additionalInfo,
    };
  }
} 