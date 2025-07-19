import 'package:flutter/material.dart';
import '../models/salon.dart';

SalonColors generateDarkPalette(SalonColors light) {
  // Helper to darken a color
  Color darken(Color color, [double amount = .3]) {
    final hsl = HSLColor.fromColor(color);
    final hslDark = hsl.withLightness((hsl.lightness - amount).clamp(0.0, 1.0));
    return hslDark.toColor();
  }

  // Helper to lighten a color
  Color lighten(Color color, [double amount = .3]) {
    final hsl = HSLColor.fromColor(color);
    final hslLight = hsl.withLightness((hsl.lightness + amount).clamp(0.0, 1.0));
    return hslLight.toColor();
  }

  // Helper to increase saturation for more vibrant colors in dark mode
  Color vibrant(Color color, [double saturation = 0.2, double lightness = 0.3]) {
    final hsl = HSLColor.fromColor(color);
    final hslVibrant = hsl.withSaturation((hsl.saturation + saturation).clamp(0.0, 1.0))
                         .withLightness((hsl.lightness + lightness).clamp(0.4, 0.9));
    return hslVibrant.toColor();
  }

  return SalonColors.base(
    primary: vibrant(light.primary, 0.3, 0.4),
    secondary: vibrant(light.secondary, 0.2, 0.3),
    background: const Color(0xFF1D1B20),
    onSurface: const Color(0xFFE6E1E5),
  );
} 