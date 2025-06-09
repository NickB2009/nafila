import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'ui/screens/salon_finder_screen.dart';
import 'ui/view_models/mock_queue_notifier.dart';

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
        theme: ThemeData(
          colorScheme: ColorScheme.fromSeed(
            seedColor: const Color(0xFF1DB584), // Green theme from mockup
            brightness: Brightness.light,
          ),
          useMaterial3: true,
          appBarTheme: const AppBarTheme(
            centerTitle: true,
            elevation: 2,
          ),
          cardTheme: CardTheme(
            elevation: 2,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(12),
            ),
          ),
        ),
        home: const SalonFinderScreen(),
      ),
    );
  }
}
