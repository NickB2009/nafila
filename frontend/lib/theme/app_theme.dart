import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';

class AppTheme {
  static const Color primaryColor = Color(0xFFD4AF37);
  static const Color secondaryColor = Color(0xFF2C3E50);
  static const Color accentColor = Color(0xFFE74C3C);

  static ThemeData lightTheme = ThemeData(
    useMaterial3: true,
    colorScheme: ColorScheme.light(
      primary: primaryColor,
      secondary: secondaryColor,
      surface: Colors.white,
      background: Colors.grey[50]!,
      error: Colors.red[700]!,
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

  static ThemeData darkTheme = ThemeData(
    useMaterial3: true,
    colorScheme: ColorScheme.dark(
      primary: primaryColor,
      secondary: secondaryColor,
      surface: Colors.grey[900]!,
      background: Colors.black,
      error: Colors.red[300]!,
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

  static ThemeData highContrastLightTheme = ThemeData(
    useMaterial3: true,
    colorScheme: ColorScheme.light(
      primary: Colors.black,
      secondary: Colors.black,
      surface: Colors.white,
      background: Colors.white,
      error: Colors.red[900]!,
      onPrimary: Colors.white,
      onSecondary: Colors.white,
      onSurface: Colors.black,
      onBackground: Colors.black,
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
      primary: Colors.yellow,
      secondary: Colors.yellow,
      surface: Colors.black,
      background: Colors.black,
      error: Colors.red[300]!,
      onPrimary: Colors.black,
      onSecondary: Colors.black,
      onSurface: Colors.yellow,
      onBackground: Colors.yellow,
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

class ThemeProvider extends ChangeNotifier {
  static const String _themeModeKey = 'theme_mode';
  static const String _fontSizeKey = 'font_size';
  static const String _highContrastKey = 'high_contrast';
  static const String _animationsKey = 'animations';

  late SharedPreferences? _prefs;
  ThemeMode _themeMode = ThemeMode.system;
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
      _themeMode = ThemeMode.values[_prefs?.getInt(_themeModeKey) ?? 2];
      _fontSize = _prefs?.getDouble(_fontSizeKey) ?? 1.0;
      _highContrast = _prefs?.getBool(_highContrastKey) ?? false;
      _animations = _prefs?.getBool(_animationsKey) ?? true;
    } catch (e) {
      // If shared_preferences fails, use default values
      _themeMode = ThemeMode.system;
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