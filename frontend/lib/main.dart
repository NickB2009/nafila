import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'ui/screens/salon_finder_screen.dart';
import 'ui/screens/salon_tv_dashboard.dart';
import 'ui/view_models/mock_queue_notifier.dart';
import 'ui/theme/app_theme.dart';

void main() {
  runApp(const EutonautilaApp());
}

class EutonautilaApp extends StatelessWidget {
  const EutonautilaApp({super.key});

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider(
      create: (context) => MockQueueNotifier(),
      child: MaterialApp(
        title: 'Eutonafila Queue Management',
        debugShowCheckedModeBanner: false,
        theme: AppTheme.lightTheme,
        darkTheme: AppTheme.darkTheme,
        themeMode: ThemeMode.system, // Will follow system theme
        home: const SalonFinderScreen(),
        routes: {
          '/tv-dashboard': (context) => const SalonTvDashboard(),
        },
      ),
    );
  }
}
