import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'theme/app_theme.dart';
import 'controllers/app_controller.dart';
import 'config/api_config.dart';
import 'services/signalr_service.dart';
// import removed: HomeScreen deprecated in favor of SalonFinderScreen
import 'ui/screens/login_screen.dart';
import 'ui/screens/register_screen.dart';
import 'ui/screens/salon_finder_screen.dart';
import 'ui/screens/salon_tv_dashboard.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'ui/widgets/error_boundary.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  
  // 🔧 Disable WebSocket connections until backend supports them
  SignalRService.enableWebSocket = false;
  
  // 🔧 API Configuration - DEVELOPMENT vs PRODUCTION
  // 
  // For web development (localhost), we need to use a local API or proxy
  // For production deployment, we use the production API
  final bool isWebDevelopment = kIsWeb && kDebugMode;
  
  if (isWebDevelopment) {
    // 🔧 DEVELOPMENT WORKAROUND for CORS issues
    // 
    // When running locally, browsers block cross-origin requests due to CORS.
    // Solutions (choose one):
    
    // OPTION 1: Run Chrome with CORS disabled (RECOMMENDED for development)
    // Close Chrome completely, then run: 
    // chrome.exe --user-data-dir="C:/chrome-dev-session" --disable-web-security --disable-features=VizDisplayCompositor
    
    // OPTION 2: Use localhost API if you have the backend running locally
    // ApiConfig.initialize(apiUrl: 'http://localhost:7126/api');
    
    // For now, we'll try the production API directly and show better error messages
    ApiConfig.initialize(apiUrl: 'https://api.eutonafila.com.br/api');
    
    print('🔧 DEVELOPMENT MODE');
    print('🚨 If you get CORS errors, run Chrome with --disable-web-security');
    print('   or set up a local backend API');
  } else {
    // Production: Use direct API
    ApiConfig.initialize(apiUrl: 'https://api.eutonafila.com.br/api');
    print('🔧 PRODUCTION MODE: Using direct API');
  }
  
  // Debug: Print API URLs to help with troubleshooting
  print('🔗 API Base URL: ${ApiConfig.currentBaseUrl}');
  print('🔌 WebSocket connections: ${SignalRService.enableWebSocket ? 'ENABLED' : 'DISABLED'}');
  print('🔗 Salons URL: ${ApiConfig.getUrl(ApiConfig.publicSalonsEndpoint)}');
  print('🔗 Queue Join URL: ${ApiConfig.getUrl('${ApiConfig.publicEndpoint}/queue/join')}');
  
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
      child: Directionality(
        textDirection: TextDirection.ltr,
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
  @override
  void initState() {
    super.initState();
    // Use Future.microtask to avoid calling setState during build
    Future.microtask(() => _initializeApp());
  }

  Future<void> _initializeApp() async {
    try {
      final appController = Provider.of<AppController>(context, listen: false);
      await appController.initialize();
    } catch (e) {
      print('❌ App initialization failed: $e');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Consumer<AppController>(
      builder: (context, appController, child) {
        // Show loading screen while initializing
        if (appController.isInitializing || (!appController.isInitialized && appController.error == null)) {
          return const Directionality(
            textDirection: TextDirection.ltr,
            child: Scaffold(
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
            ),
          );
        }

        // Show error screen if initialization failed
        if (appController.error != null) {
          return Directionality(
            textDirection: TextDirection.ltr,
            child: Scaffold(
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
            ),
          );
        }

        // Show main app when initialized
        return const SalonFinderScreen();
      },
    );
  }
}
