import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import '../../lib/ui/screens/anonymous_join_queue_screen.dart';
import '../../lib/models/public_salon.dart';

void main() {
  group('AnonymousJoinQueueScreen - Service Dropdown', () {
    late PublicSalon testSalon;
    late PublicSalon salonWithServices;

    setUp(() {
      testSalon = const PublicSalon(
        id: 'salon123',
        name: 'Test Salon',
        address: 'Test Address',
        latitude: -23.5505,
        longitude: -46.6333,
        isOpen: true,
        queueLength: 5,
        currentWaitTimeMinutes: 15,
        services: null, // No services defined
      );

      salonWithServices = const PublicSalon(
        id: 'salon456',
        name: 'Full Service Salon',
        address: 'Service Address',
        latitude: -23.5505,
        longitude: -46.6333,
        isOpen: true,
        queueLength: 3,
        currentWaitTimeMinutes: 10,
        services: ['Haircut', 'Beard Trim', 'Hair Wash'], // Has services
      );
    });

         testWidgets('should show default services as selectable chips when salon has no services', (WidgetTester tester) async {
       await tester.pumpWidget(
         MaterialApp(
           home: AnonymousJoinQueueScreen(salon: testSalon),
         ),
       );

       // Should show service selection with chips
       expect(find.text('Services Requested (Optional)'), findsOneWidget);
       expect(find.text('Select all services you need - you can choose multiple'), findsOneWidget);
       
       // Should show default services as chips
       expect(find.text('Haircut'), findsOneWidget);
       expect(find.text('Beard Trim'), findsOneWidget);
       expect(find.text('Hair Wash'), findsOneWidget);
       expect(find.text('Styling'), findsOneWidget);
     });

         testWidgets('should show salon-specific services as chips when available', (WidgetTester tester) async {
       await tester.pumpWidget(
         MaterialApp(
           home: AnonymousJoinQueueScreen(salon: salonWithServices),
         ),
       );

       // Should show service selection with chips
       expect(find.text('Services Requested (Optional)'), findsOneWidget);
       
       // Should show salon's specific services as chips
       expect(find.text('Haircut'), findsOneWidget);
       expect(find.text('Beard Trim'), findsOneWidget);
       expect(find.text('Hair Wash'), findsOneWidget);
       
       // Should NOT show default services that aren't in salon's service list
       expect(find.text('Styling'), findsNothing);
       expect(find.text('Hair Treatment'), findsNothing);
     });

         testWidgets('should allow selecting multiple services', (WidgetTester tester) async {
       await tester.pumpWidget(
         MaterialApp(
           home: AnonymousJoinQueueScreen(salon: salonWithServices),
         ),
       );

       // Initially, "Haircut" should be selected by default
       expect(find.text('Selected Services:'), findsOneWidget);
       expect(find.text('Haircut'), findsWidgets);

       // Find and tap "Beard Trim" chip to add it to selection
       final beardTrimChip = find.text('Beard Trim');
       await tester.tap(beardTrimChip);
       await tester.pumpAndSettle();

       // Should now show both services selected
       expect(find.text('Haircut + Beard Trim'), findsOneWidget);
     });

         testWidgets('should default to "Haircut" when available', (WidgetTester tester) async {
       await tester.pumpWidget(
         MaterialApp(
           home: AnonymousJoinQueueScreen(salon: salonWithServices),
         ),
       );

       // Should show services section
       expect(find.text('Services Requested (Optional)'), findsOneWidget);
       
       // Should have "Haircut" selected by default (shows in summary)
       expect(find.text('Selected Services:'), findsOneWidget);
       expect(find.text('Haircut'), findsWidgets);
     });

     testWidgets('should have proper form validation with multi-service selection', (WidgetTester tester) async {
       await tester.pumpWidget(
         MaterialApp(
           home: AnonymousJoinQueueScreen(salon: testSalon),
         ),
       );

       // Fill in required fields, service should default to "Haircut"
       await tester.enterText(find.byType(TextFormField).first, 'Test User');
       await tester.enterText(find.byType(TextFormField).last, 'test@example.com');

       // Should show services section
       expect(find.text('Services Requested (Optional)'), findsOneWidget);

       // The join button should be available
       final joinButton = find.text('Join Queue');
       expect(joinButton, findsOneWidget);
     });
  });
} 