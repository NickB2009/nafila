import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

class AppTheme {
  // Single source of truth for the app's color
  static const Color brandColor = Color.from(alpha: 1, red: 0.702, green: 0.576, blue: 0.063); // DarkGoldenrod

  // Derived colors
  static Color get primaryColor => brandColor;
  static Color get secondaryColor => brandColor.withOpacity(0.8);
  static Color get accentColor => brandColor.withOpacity(0.6);

  static ThemeData lightTheme = ThemeData(
    useMaterial3: true,
    colorScheme: ColorScheme.light(
      primary: primaryColor,
      secondary: secondaryColor,
      surface: Colors.white,
      error: Colors.red[700]!,
      onPrimary: Colors.white,
      onSecondary: Colors.white,
      onSurface: Colors.black87,
    ),
    scaffoldBackgroundColor: Colors.grey[50],
    appBarTheme: AppBarTheme(
      centerTitle: true,
      elevation: 0,
      backgroundColor: primaryColor,
      foregroundColor: Colors.white,
      iconTheme: const IconThemeData(color: Colors.white),
    ),
    elevatedButtonTheme: ElevatedButtonThemeData(
      style: ElevatedButton.styleFrom(
        elevation: 0,
        backgroundColor: primaryColor,
        foregroundColor: Colors.white,
        padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
      ),
    ),
    outlinedButtonTheme: OutlinedButtonThemeData(
      style: OutlinedButton.styleFrom(
        foregroundColor: primaryColor,
        side: BorderSide(color: primaryColor),
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
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide(color: primaryColor, width: 2),
      ),
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
    ),
    cardTheme: CardThemeData(
      elevation: 2,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
      ),
      color: Colors.white,
    ),
  );

  static ThemeData darkTheme = ThemeData(
    useMaterial3: true,
    colorScheme: ColorScheme.fromSeed(
      seedColor: primaryColor,
      brightness: Brightness.dark,
      // Override specific colors for better dark mode experience
      surface: const Color(0xFF121212), // Material 3 dark surface
      onSurface: const Color(0xFFE6E1E5), // Better contrast
      surfaceContainerLowest: const Color(0xFF0F0D13),
      surfaceContainerLow: const Color(0xFF1D1B20),
      surfaceContainer: const Color(0xFF211F26),
      surfaceContainerHigh: const Color(0xFF2B2930),
      surfaceContainerHighest: const Color(0xFF36343B),
      outline: const Color(0xFF938F99),
      outlineVariant: const Color(0xFF49454F),
    ),
    scaffoldBackgroundColor: const Color(0xFF121212),
    appBarTheme: AppBarTheme(
      centerTitle: true,
      elevation: 0,
      backgroundColor: primaryColor,
      foregroundColor: Colors.white,
      iconTheme: const IconThemeData(color: Colors.white),
      surfaceTintColor: Colors.transparent,
    ),
    elevatedButtonTheme: ElevatedButtonThemeData(
      style: ElevatedButton.styleFrom(
        elevation: 3,
        backgroundColor: primaryColor,
        foregroundColor: Colors.white,
        padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
        shadowColor: Colors.black.withOpacity(0.3),
      ),
    ),
    outlinedButtonTheme: OutlinedButtonThemeData(
      style: OutlinedButton.styleFrom(
        foregroundColor: primaryColor,
        side: BorderSide(color: primaryColor),
        padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: Color(0xFF49454F)),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: Color(0xFF49454F)),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide(color: primaryColor, width: 2),
      ),
      fillColor: const Color(0xFF1D1B20),
      filled: true,
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
    ),
    cardTheme: const CardThemeData(
      elevation: 4,
      shadowColor: Colors.black,
      surfaceTintColor: Colors.transparent,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.all(Radius.circular(16)),
      ),
      color: Color(0xFF1D1B20), // Surface container low
    ),
    dividerTheme: const DividerThemeData(
      color: Color(0xFF49454F),
      thickness: 0.5,
    ),
    bottomNavigationBarTheme: const BottomNavigationBarThemeData(
      backgroundColor: Color(0xFF1D1B20),
      elevation: 8,
    ),
  );

  static ThemeData highContrastLightTheme = ThemeData(
    useMaterial3: true,
    colorScheme: ColorScheme.light(
      primary: Colors.black,
      secondary: Colors.black,
      surface: Colors.white,
      error: Colors.red[900]!,
      onPrimary: Colors.white,
      onSecondary: Colors.white,
      onSurface: Colors.black,
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

  static ThemeData highContrastDarkTheme = ThemeData(
    useMaterial3: true,
    colorScheme: ColorScheme.dark(
      primary: const Color(0xFFFFFF00), // Pure yellow for maximum contrast
      secondary: const Color(0xFFFFFFFF), // Pure white
      surface: const Color(0xFF000000), // Pure black
      error: const Color(0xFFFF4444),
      onPrimary: const Color(0xFF000000), // Black text on yellow
      onSecondary: const Color(0xFF000000), // Black text on white
      onSurface: const Color(0xFFFFFF00), // Yellow text on black
      outline: const Color(0xFFFFFFFF),
      outlineVariant: const Color(0xFF666666),
    ),
    scaffoldBackgroundColor: const Color(0xFF000000),
    appBarTheme: const AppBarTheme(
      centerTitle: true,
      elevation: 0,
      backgroundColor: Color(0xFF000000),
      foregroundColor: Color(0xFFFFFF00),
    ),
    elevatedButtonTheme: ElevatedButtonThemeData(
      style: ElevatedButton.styleFrom(
        elevation: 0,
        backgroundColor: const Color(0xFFFFFF00),
        foregroundColor: const Color(0xFF000000),
        padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
          side: const BorderSide(color: Color(0xFFFFFFFF), width: 2),
        ),
      ),
    ),
    outlinedButtonTheme: OutlinedButtonThemeData(
      style: OutlinedButton.styleFrom(
        foregroundColor: const Color(0xFFFFFF00),
        side: const BorderSide(color: Color(0xFFFFFF00), width: 2),
        padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: Color(0xFFFFFFFF), width: 2),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: Color(0xFFFFFFFF), width: 2),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: Color(0xFFFFFF00), width: 3),
      ),
      fillColor: const Color(0xFF000000),
      filled: true,
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
    ),
    cardTheme: const CardThemeData(
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.all(Radius.circular(12)),
        side: BorderSide(color: Color(0xFFFFFFFF), width: 2),
      ),
      color: Color(0xFF000000),
    ),
    dividerTheme: const DividerThemeData(
      color: Color(0xFFFFFFFF),
      thickness: 2,
    ),
  );
}

class ThemeProvider extends ChangeNotifier {
  static const String _themeModeKey = 'theme_mode';
  static const String _fontSizeKey = 'font_size';
  static const String _highContrastKey = 'high_contrast';
  static const String _animationsKey = 'animations';

  late SharedPreferences? _prefs;
  ThemeMode _themeMode = ThemeMode.system; // Changed to system default
  double _fontSize = 1.0;
  bool _highContrast = false;
  bool _animations = true;

  ThemeProvider() {
    _loadSettings();
  }

  ThemeMode get themeMode => _themeMode;
  double get fontSize => _fontSize;
  bool get highContrast => _highContrast;
  bool get animations => _animations;

  Future<void> _loadSettings() async {
    try {
      _prefs = await SharedPreferences.getInstance();
      final savedIndex = _prefs?.getInt(_themeModeKey);
      if (savedIndex != null && savedIndex >= 0 && savedIndex < ThemeMode.values.length) {
        _themeMode = ThemeMode.values[savedIndex];
      } else {
        _themeMode = ThemeMode.system; // Default to system if no saved preference
      }
      _fontSize = _prefs?.getDouble(_fontSizeKey) ?? 1.0;
      _highContrast = _prefs?.getBool(_highContrastKey) ?? false;
      _animations = _prefs?.getBool(_animationsKey) ?? true;
    } catch (e) {
      // If shared_preferences fails, use default values
      _themeMode = ThemeMode.system; // Changed to system default
      _fontSize = 1.0;
      _highContrast = false;
      _animations = true;
    }
    notifyListeners();
  }

  Future<void> setThemeMode(ThemeMode mode) async {
    _themeMode = mode;
    try {
      await _prefs?.setInt(_themeModeKey, mode.index);
    } catch (e) {
      // Ignore errors in web mode
    }
    notifyListeners();
  }

  Future<void> setFontSize(double size) async {
    _fontSize = size;
    try {
      await _prefs?.setDouble(_fontSizeKey, size);
    } catch (e) {
      // Ignore errors in web mode
    }
    notifyListeners();
  }

  Future<void> setHighContrast(bool value) async {
    _highContrast = value;
    try {
      await _prefs?.setBool(_highContrastKey, value);
    } catch (e) {
      // Ignore errors in web mode
    }
    notifyListeners();
  }

  Future<void> setAnimations(bool value) async {
    _animations = value;
    try {
      await _prefs?.setBool(_animationsKey, value);
    } catch (e) {
      // Ignore errors in web mode
    }
    notifyListeners();
  }

  ThemeData getTheme(bool isDark) {
    if (_highContrast) {
      return isDark ? AppTheme.highContrastDarkTheme : AppTheme.highContrastLightTheme;
    }
    return isDark ? AppTheme.darkTheme : AppTheme.lightTheme;
  }
} 