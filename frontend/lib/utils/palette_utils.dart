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

  return SalonColors(
    primary: lighten(light.primary, 0.2),
    secondary: lighten(light.secondary, 0.2),
    background: darken(light.background, 0.7),
    onSurface: Colors.white, // Always white for dark backgrounds
  );
} 