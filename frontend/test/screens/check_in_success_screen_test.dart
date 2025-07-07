import 'package:eutonafila_frontend/ui/screens/check_in_success_screen.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

class DummyQueueStatusScreen extends StatelessWidget {
  const DummyQueueStatusScreen({super.key});
  @override
  Widget build(BuildContext context) {
    return const Scaffold(body: Center(child: Text('Queue Status')));
  }
}

void main() {
  Widget buildTestable({bool delayNavigation = false}) {
    return MaterialApp(
      home: CheckInSuccessScreen(delayNavigation: delayNavigation),
      onGenerateRoute: (settings) {
        if (settings.name == '/') {
          return MaterialPageRoute(builder: (_) => CheckInSuccessScreen(delayNavigation: delayNavigation));
        }
        return MaterialPageRoute(builder: (_) => const DummyQueueStatusScreen());
      },
    );
  }

  testWidgets('shows success icon and message', (WidgetTester tester) async {
    await tester.binding.setSurfaceSize(const Size(800, 1200));
    await tester.pumpWidget(buildTestable());
    expect(find.byIcon(Icons.check), findsOneWidget);
    expect(find.text('Check-in realizado com sucesso.'), findsOneWidget);
  });

  testWidgets('has correct background color', (WidgetTester tester) async {
    await tester.pumpWidget(buildTestable());
    final scaffold = tester.widget<Scaffold>(find.byType(Scaffold));
    // Just check it's not the default white
    expect(scaffold.backgroundColor, isNot(equals(Colors.white)));
  });

  testWidgets('centers icon and message', (WidgetTester tester) async {
    await tester.pumpWidget(buildTestable());
    final column = tester.widget<Column>(find.byType(Column));
    expect(column.mainAxisAlignment, MainAxisAlignment.center);
    expect(find.byIcon(Icons.check), findsOneWidget);
    expect(find.text('Check-in realizado com sucesso.'), findsOneWidget);
  });

  testWidgets('has proper layout structure', (WidgetTester tester) async {
    await tester.pumpWidget(buildTestable());
    expect(find.byType(Scaffold), findsOneWidget);
    expect(find.byType(SafeArea), findsOneWidget);
    expect(find.byType(Center), findsWidgets);
    expect(find.byType(Column), findsOneWidget);
    expect(find.byType(Container), findsWidgets);
  });

  testWidgets('has proper styling elements', (WidgetTester tester) async {
    await tester.pumpWidget(buildTestable());
    // Check for Container with BoxDecoration and Border
    final container = tester.widget<Container>(find.byType(Container).first);
    expect(container.decoration, isA<BoxDecoration>());
    expect((container.decoration as BoxDecoration).border, isNotNull);
    expect(find.byType(Icon), findsOneWidget);
    expect(find.byType(Text), findsOneWidget);
  });

  testWidgets('does not navigate when delayNavigation is false', (WidgetTester tester) async {
    await tester.pumpWidget(buildTestable(delayNavigation: false));
    expect(find.byType(CheckInSuccessScreen), findsOneWidget);
    expect(find.text('Check-in realizado com sucesso.'), findsOneWidget);
    
    // Wait for 3 seconds to ensure no navigation happens
    await tester.pump(const Duration(seconds: 3));
    await tester.pumpAndSettle();
    
    // Should still be on the success screen
    expect(find.byType(CheckInSuccessScreen), findsOneWidget);
    expect(find.text('Check-in realizado com sucesso.'), findsOneWidget);
  });
} 