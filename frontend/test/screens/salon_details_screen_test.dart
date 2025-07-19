import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/ui/screens/salon_details_screen.dart';
import 'package:eutonafila_frontend/models/salon.dart';
import 'package:eutonafila_frontend/models/salon_service.dart';
import 'package:eutonafila_frontend/models/salon_contact.dart';
import 'package:eutonafila_frontend/models/salon_hours.dart';
import 'package:eutonafila_frontend/models/salon_review.dart';

void main() {
  group('SalonDetailsScreen Tests', () {
    late Salon mockSalon;
    late List<SalonService> mockServices;
    late SalonContact mockContact;
    late List<SalonHours> mockBusinessHours;
    late List<SalonReview> mockReviews;

    setUp(() {
      mockSalon = Salon(
        name: 'Great Clips',
        address: '123 Main St, Tampa, FL',
        waitTime: 34,
        distance: 2.3,
        isOpen: true,
        closingTime: '8:00 PM',
        isFavorite: false,
        queueLength: 5,
        colors: SalonColors(
          primary: Colors.blue,
          secondary: Colors.blueAccent,
          background: Colors.white,
          onSurface: Colors.black,
        ),
      );

      mockServices = [
        const SalonService(
          id: '1',
          name: 'Haircut',
          description: 'Professional haircut service',
          price: 25.0,
          durationMinutes: 30,
          categories: ['Hair'],
        ),
        const SalonService(
          id: '2',
          name: 'Hair Styling',
          description: 'Professional hair styling',
          price: 35.0,
          durationMinutes: 45,
          categories: ['Hair'],
        ),
      ];

      mockContact = const SalonContact(
        phone: '(813) 555-0123',
        email: 'info@greatclips.com',
        website: 'https://greatclips.com',
      );

      mockBusinessHours = [
        const SalonHours(
          day: 'Monday',
          openTime: '9:00 AM',
          closeTime: '8:00 PM',
          isOpen: true,
        ),
        const SalonHours(
          day: 'Tuesday',
          openTime: '9:00 AM',
          closeTime: '8:00 PM',
          isOpen: true,
        ),
      ];

      mockReviews = [
        const SalonReview(
          id: '1',
          userName: 'John Doe',
          rating: 5.0,
          comment: 'Great service!',
          date: '2024-01-15',
        ),
        const SalonReview(
          id: '2',
          userName: 'Jane Smith',
          rating: 4.0,
          comment: 'Good experience',
          date: '2024-01-14',
        ),
      ];
    });

    Widget buildTestable() {
      return MaterialApp(
        home: SalonDetailsScreen(
          salon: mockSalon,
          services: mockServices,
          contact: mockContact,
          businessHours: mockBusinessHours,
          reviews: mockReviews,
        ),
      );
    }

    testWidgets('displays salon name and basic info', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump();

      expect(find.text('Great Clips'), findsOneWidget);
      expect(find.text('123 Main St, Tampa, FL'), findsOneWidget);
      expect(find.text('(813) 555-0123'), findsOneWidget);
    });

    testWidgets('displays wait time information', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump();

      expect(find.text('34 min'), findsOneWidget);
    });

    testWidgets('displays services section', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump();

      expect(find.text('Serviços'), findsOneWidget);
      expect(find.text('Haircut'), findsOneWidget);
      expect(find.text('Hair Styling'), findsOneWidget);
    });

    testWidgets('displays business hours section', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump();

      expect(find.text('Horário de Funcionamento'), findsOneWidget);
      expect(find.text('Monday'), findsOneWidget);
      expect(find.text('Tuesday'), findsOneWidget);
      expect(find.text('9:00 AM - 8:00 PM'), findsWidgets);
    });

    testWidgets('displays contact section', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump();

      expect(find.text('Contato'), findsOneWidget);
      expect(find.text('info@greatclips.com'), findsOneWidget);
      expect(find.text('https://greatclips.com'), findsOneWidget);
    });

    testWidgets('displays reviews section', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump();

      expect(find.text('Avaliações'), findsOneWidget);
      expect(find.text('John Doe'), findsOneWidget);
      expect(find.text('Jane Smith'), findsOneWidget);
      expect(find.text('Great service!'), findsOneWidget);
      expect(find.text('Good experience'), findsOneWidget);
    });

    testWidgets('has navigation buttons', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump();

      expect(find.byIcon(Icons.arrow_back), findsOneWidget);
      expect(find.byIcon(Icons.favorite_border), findsOneWidget);
      expect(find.byIcon(Icons.share), findsOneWidget);
    });

    testWidgets('has check-in button', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump();

      expect(find.text('Check In'), findsOneWidget);
    });

    testWidgets('has directions button', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump();

      expect(find.text('Como Chegar'), findsOneWidget);
    });

    testWidgets('displays distance information', (WidgetTester tester) async {
      await tester.pumpWidget(buildTestable());
      await tester.pump();

      expect(find.text('2.3 km'), findsOneWidget);
    });
  });
} 