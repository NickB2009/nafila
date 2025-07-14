import 'package:flutter/material.dart';

class SalonColors {
  final Color primary;
  final Color secondary;
  final Color background;
  final Color onSurface;
  final SalonColors? dark;

  const SalonColors({
    required this.primary,
    required this.secondary,
    required this.background,
    required this.onSurface,
    this.dark,
  });

  /// Returns the correct palette for the current brightness (light/dark)
  SalonColors forBrightness(Brightness brightness) {
    if (brightness == Brightness.dark && dark != null) {
      return dark!;
    }
    return this;
  }

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is SalonColors &&
          runtimeType == other.runtimeType &&
          primary == other.primary &&
          secondary == other.secondary &&
          background == other.background &&
          onSurface == other.onSurface;

  @override
  int get hashCode => Object.hash(primary, secondary, background, onSurface);
}

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
  final SalonColors colors;

  const Salon({
    required this.name,
    required this.address,
    required this.waitTime,
    required this.distance,
    required this.isOpen,
    required this.closingTime,
    required this.isFavorite,
    required this.queueLength,
    required this.colors,
  });

  Salon copyWith({
    String? name,
    String? address,
    int? waitTime,
    double? distance,
    bool? isOpen,
    String? closingTime,
    bool? isFavorite,
    int? queueLength,
    SalonColors? colors,
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
      colors: colors ?? this.colors,
    );
  }

  @override
  String toString() {
    return 'Salon(name: $name, address: $address, waitTime: $waitTime, distance: $distance, queueLength: $queueLength, colors: $colors)';
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
        other.queueLength == queueLength &&
        other.colors == colors;
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
      colors,
    );
  }
}
