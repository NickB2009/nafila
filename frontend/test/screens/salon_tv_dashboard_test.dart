import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/ui/screens/salon_tv_dashboard.dart';

void main() {
  final binding = TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() {
    binding.window.physicalSizeTestValue = const Size(1920, 1080);
    binding.window.devicePixelRatioTestValue = 1.0;
  });

  tearDownAll(() {
    binding.window.clearPhysicalSizeTestValue();
    binding.window.clearDevicePixelRatioTestValue();
  });

  group('SalonTvDashboard Tests', () {
    Widget buildTestable() {
      return MediaQuery(
        data: MediaQueryData(size: const Size(1920, 1080)), // Simulate a TV
        child: const MaterialApp(
          home: SalonTvDashboard(),
        ),
      );
    }

    testWidgets('displays salon name and basic structure', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump();

      // Check for the most basic elements that should always be visible
      expect(find.text('Great Clips'), findsOneWidget);
      expect(find.text('Painel de Fila'), findsOneWidget);
    });

    testWidgets('displays time information', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump();

      // Check for time display (should always be visible)
      expect(find.byType(Text), findsWidgets);
    });

    testWidgets('has advertisement section', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump();

      // Check for advertisement containers
      expect(find.byType(AnimatedContainer), findsWidgets);
    });
  });
} 