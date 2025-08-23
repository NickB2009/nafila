import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/ui/widgets/analytics_card.dart';
import 'package:eutonafila_frontend/models/analytics_models.dart';

void main() {
  group('Analytics Widgets', () {
    group('AnalyticsCard', () {
      testWidgets('should display analytics data correctly', (WidgetTester tester) async {
        // Arrange
        final analyticsData = {
          'title': 'Queue Performance',
          'value': '85%',
          'subtitle': 'Efficiency Score',
          'trend': '+5%',
          'trendDirection': 'up',
          'icon': Icons.trending_up,
        };

        // Act
        await tester.pumpWidget(
          MaterialApp(
            home: Scaffold(
              body: AnalyticsCard(
                title: analyticsData['title'] as String,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(analyticsData['value'] as String),
                    Text(analyticsData['subtitle'] as String),
                    Text(analyticsData['trend'] as String),
                    Icon(analyticsData['icon'] as IconData),
                  ],
                ),
              ),
            ),
          ),
        );

        // Assert
        expect(find.text('Queue Performance'), findsOneWidget);
        expect(find.text('85%'), findsOneWidget);
        expect(find.text('Efficiency Score'), findsOneWidget);
        expect(find.text('+5%'), findsOneWidget);
        expect(find.byIcon(Icons.trending_up), findsOneWidget);
      });
    });

    group('PerformanceMetricsCard', () {
      testWidgets('should display performance metrics correctly', (WidgetTester tester) async {
        // Arrange
        final metrics = QueuePerformanceMetrics(
          salonId: 'salon-123',
          periodStart: DateTime(2024, 1, 1),
          periodEnd: DateTime(2024, 1, 15),
          totalCustomers: 156,
          averageWaitTime: const Duration(minutes: 20),
          averageServiceTime: const Duration(minutes: 25),
          customerSatisfaction: 4.3,
          queueEfficiency: 0.85,
          peakWaitTime: const Duration(minutes: 45),
          peakHourCustomers: 18,
          completedServices: 150,
          cancelledServices: 6,
        );

        // Act
        await tester.pumpWidget(
          MaterialApp(
            home: Scaffold(
              body: PerformanceMetricsCard(metrics: metrics),
            ),
          ),
        );

        // Assert
        expect(find.text('Performance Metrics'), findsOneWidget);
        expect(find.text('156'), findsOneWidget); // Total customers
        expect(find.text('150'), findsOneWidget); // Completed services
        expect(find.text('20 min'), findsOneWidget); // Average wait time
        expect(find.text('85%'), findsOneWidget); // Efficiency
        expect(find.text('4.3/5'), findsOneWidget); // Satisfaction
      });
    });
  });
}
