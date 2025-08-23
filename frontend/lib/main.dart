import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'theme/app_theme.dart';
import 'controllers/app_controller.dart';
import 'config/api_config.dart';
// import removed: HomeScreen deprecated in favor of SalonFinderScreen
import 'ui/screens/login_screen.dart';
import 'ui/screens/register_screen.dart';
import 'ui/screens/salon_finder_screen.dart';
import 'ui/screens/salon_tv_dashboard.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'ui/widgets/error_boundary.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  
  // 🔧 API Configuration - Using PRODUCTION API
  // 
  // Using production API instead of localhost
  ApiConfig.initialize(apiUrl: 'https://api.eutonafila.com.br');
  
  // ALTERNATIVE OPTIONS (commented out):
  // ApiConfig.initialize(); // Default localhost
  // ApiConfig.initialize(apiUrl: 'http://192.168.1.100:7126'); // Local network IP
  
  runApp(
    ErrorBoundary(
      child: ChangeNotifierProvider(
        create: (_) => ThemeProvider(),
        child: const MyApp(),
      ),
    ),
  );
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    final themeProvider = Provider.of<ThemeProvider>(context);
    
    return ChangeNotifierProvider(
      create: (context) {
        print('🏗️ Creating new AppController instance');
        return AppController();
      },
      child: MaterialApp(
        title: 'EutoNaFila',
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
        home: const AppInitializer(),
        routes: {
          '/home': (context) => const SalonFinderScreen(),
          '/salon-finder': (context) => const SalonFinderScreen(),
          '/tv-dashboard': (context) => const SalonTvDashboard(),
          '/login': (context) => const LoginScreen(),
          '/register': (context) => const RegisterScreen(),
        },
        localizationsDelegates: const [
          GlobalMaterialLocalizations.delegate,
          GlobalWidgetsLocalizations.delegate,
          GlobalCupertinoLocalizations.delegate,
        ],
        supportedLocales: const [
          Locale('pt', 'BR'),
          Locale('en', 'US'),
        ],
      ),
    );
  }
}

class AppInitializer extends StatefulWidget {
  const AppInitializer({super.key});

  @override
  State<AppInitializer> createState() => _AppInitializerState();
}

class _AppInitializerState extends State<AppInitializer> {
  bool _isInitializing = false;

  @override
  void initState() {
    super.initState();
    _initializeApp();
  }

  Future<void> _initializeApp() async {
    if (_isInitializing) return;
    
    _isInitializing = true;
    try {
      final appController = Provider.of<AppController>(context, listen: false);
      await appController.initialize();
    } finally {
      _isInitializing = false;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Consumer<AppController>(
      builder: (context, appController, child) {
        if (!appController.isInitialized && appController.error == null) {
          return const Scaffold(
            body: Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  CircularProgressIndicator(),
                  SizedBox(height: 16),
                  Text('Carregando dados...'),
                ],
              ),
            ),
          );
        }

        if (appController.error != null) {
          return Scaffold(
            body: Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.error, size: 64, color: Colors.red),
                  const SizedBox(height: 16),
                  Text('Error: ${appController.error}'),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () => _initializeApp(),
                    child: const Text('Tentar novamente'),
                  ),
                ],
              ),
            ),
          );
        }

        return const SalonFinderScreen();
      },
    );
  }
}
