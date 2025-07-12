import 'package:flutter/material.dart';

/// Theme configuration for the application
class AppTheme {
  /// Primary brand color
  static const Color primaryColor = Color(0xFFD4AF37); // Rich Gold
  
  /// Secondary brand color
  static const Color secondaryColor = Color(0xFF2C3E50);
  
  /// Background color for the application
  static const Color backgroundColor = Color(0xFFF5F5F5);
  
  /// Surface color for cards and elevated surfaces
  static const Color surfaceColor = Color(0xFFFFFFFF);
  
  /// Error color for error states and messages
  static const Color errorColor = Color(0xFFB00020);

  /// Status colors for queue states
  static const Map<String, Color> statusColors = {
    'waiting': Color(0xFFFF9800),
    'inService': Color(0xFF4CAF50),
    'completed': Color(0xFF9E9E9E),
  };

  /// Extended color palette for enhanced UI
  static const Color accentColor = Color(0xFF6B73FF); // Purple accent
  static const Color successColor = Color(0xFF4CAF50); // Green for success/urgent
  static const Color warningColor = Color(0xFFFF9800); // Orange for warnings/popular
  static const Color infoColor = Color(0xFF1976D2); // Blue 700 for info/highlight
  static const Color dangerColor = Color(0xFF1976D2); // Blue for offers/urgency/CTA
  
  /// Gradient colors for hero sections and special elements
  static const List<Color> heroGradient = [
    primaryColor, // Gold
    infoColor,    // Blue
    accentColor,  // Purple accent (optional, can be removed if you want only blue/gold)
  ];
  static const List<Color> offerGradient = [
    primaryColor, // Gold
    infoColor,    // Blue
  ];
  static const List<Color> ctaGradient = [
    primaryColor, // Gold
    infoColor,    // Blue
  ];
  static const List<Color> darkGradient = [
    Color(0xFF2C3E50), // Dark Blue
    Color(0xFF34495E), // Darker Blue
  ];

  /// Semantic colors for different states
  static const Map<String, Color> semanticColors = {
    'urgent': Color(0xFF4CAF50),     // Green for urgent/fast
    'popular': Color(0xFFFF9800),    // Orange for popular
    'premium': Color(0xFFD4AF37),    // Gold for premium
    'distance': Color(0xFF009688),   // Teal for distance
    'time': Color(0xFF2196F3),       // Blue for time
    'queue': Color(0xFF6B73FF),      // Purple for queue
  };

  /// Get the light theme configuration
  static ThemeData get lightTheme {
    return ThemeData(
      useMaterial3: true,
      colorScheme: ColorScheme.fromSeed(
        seedColor: primaryColor,
        brightness: Brightness.light,
        secondary: accentColor,
        tertiary: successColor,
        error: errorColor,
      ),
      appBarTheme: const AppBarTheme(
        centerTitle: true,
        elevation: 0,
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          elevation: 0,
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
        ),
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
      ),
    );
  }

  /// Get the dark theme configuration
  static ThemeData get darkTheme {
    return ThemeData(
      useMaterial3: true,
      colorScheme: ColorScheme.fromSeed(
        seedColor: primaryColor,
        brightness: Brightness.dark,
        secondary: accentColor,
        tertiary: successColor,
        error: errorColor,
      ),
      appBarTheme: const AppBarTheme(
        centerTitle: true,
        elevation: 0,
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          elevation: 0,
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
        ),
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
      ),
    );
  }
} 