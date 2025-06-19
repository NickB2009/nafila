import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'theme/app_theme.dart';
import 'ui/screens/salon_finder_screen.dart';
import 'ui/screens/salon_tv_dashboard.dart';
import 'ui/view_models/mock_queue_notifier.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  
  runApp(
    ChangeNotifierProvider(
      create: (_) => ThemeProvider(),
      child: const MyApp(),
    ),
  );
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    final themeProvider = Provider.of<ThemeProvider>(context);
    
    return ChangeNotifierProvider(
      create: (context) => MockQueueNotifier(),
      child: MaterialApp(
        title: 'Nafila',
        debugShowCheckedModeBanner: false,
        theme: themeProvider.getTheme(false),
        darkTheme: themeProvider.getTheme(true),
        themeMode: themeProvider.themeMode,
        builder: (context, child) {
          return MediaQuery(
            data: MediaQuery.of(context).copyWith(
              textScaler: TextScaler.linear(themeProvider.fontSize),
            ),
            child: child!,
          );
        },
        home: const SalonFinderScreen(),
        routes: {
          '/tv-dashboard': (context) => const SalonTvDashboard(),
        },
        localizationsDelegates: const [
          GlobalMaterialLocalizations.delegate,
          GlobalWidgetsLocalizations.delegate,
          GlobalCupertinoLocalizations.delegate,
        ],
        supportedLocales: const [
          Locale('pt', 'BR'), // Portuguese (Brazil)
          Locale('en', ''),   // English (default)
          // Add more if you want
        ],
        // Performance optimizations
        showPerformanceOverlay: false,
        showSemanticsDebugger: false,
      ),
    );
  }
}
