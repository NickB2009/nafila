/// Model representing a salon/barbershop
class Salon {
  final String name;
  final String address;
  final int waitTime; // in minutes
  final double distance; // in miles
  final bool isOpen;
  final String closingTime;
  final bool isFavorite;
  final int queueLength; // number of people in queue

  const Salon({
    required this.name,
    required this.address,
    required this.waitTime,
    required this.distance,
    required this.isOpen,
    required this.closingTime,
    required this.isFavorite,
    required this.queueLength,
  });

  /// Create a copy of this salon with updated properties
  Salon copyWith({
    String? name,
    String? address,
    int? waitTime,
    double? distance,
    bool? isOpen,
    String? closingTime,
    bool? isFavorite,
    int? queueLength,
  }) {
    return Salon(
      name: name ?? this.name,
      address: address ?? this.address,
      waitTime: waitTime ?? this.waitTime,
      distance: distance ?? this.distance,
      isOpen: isOpen ?? this.isOpen,
      closingTime: closingTime ?? this.closingTime,
      isFavorite: isFavorite ?? this.isFavorite,
      queueLength: queueLength ?? this.queueLength,
    );
  }

  @override
  String toString() {
    return 'Salon(name: $name, address: $address, waitTime: $waitTime, distance: $distance, queueLength: $queueLength)';
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is Salon &&
        other.name == name &&
        other.address == address &&
        other.waitTime == waitTime &&
        other.distance == distance &&
        other.isOpen == isOpen &&
        other.closingTime == closingTime &&
        other.isFavorite == isFavorite &&
        other.queueLength == queueLength;
  }

  @override
  int get hashCode {
    return Object.hash(
      name,
      address,
      waitTime,
      distance,
      isOpen,
      closingTime,
      isFavorite,
      queueLength,
    );
  }
}
