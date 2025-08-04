import 'package:flutter_test/flutter_test.dart';

void main() {
  group('Day of Week Analysis', () {
    test('should show current day and time', () {
      final now = DateTime.now();
      final utcNow = DateTime.now().toUtc();
      final brazilTime = utcNow.subtract(const Duration(hours: 3)); // UTC-3 (Brazil standard time)
      
      print('🕒 Current Local Time: $now');
      print('🌍 Current UTC Time: $utcNow');
      print('🇧🇷 Brazil Time (UTC-3): $brazilTime');
      print('📅 Current Day of Week: ${_getDayName(now.weekday)}');
      print('📅 Brazil Day of Week: ${_getDayName(brazilTime.weekday)}');
      print('⏰ Current Hour: ${now.hour}:${now.minute.toString().padLeft(2, '0')}');
      print('⏰ Brazil Hour: ${brazilTime.hour}:${brazilTime.minute.toString().padLeft(2, '0')}');
      
      // Check against salon business hours
      final Map<String, String> businessHours = {
        'monday': '09:00-18:00',
        'tuesday': '09:00-18:00', 
        'wednesday': '09:00-18:00',
        'thursday': '09:00-18:00',
        'friday': '09:00-18:00',
        'saturday': '09:00-18:00',
        'sunday': 'closed'
      };
      
      final currentDayName = _getDayName(brazilTime.weekday).toLowerCase();
      final todayHours = businessHours[currentDayName];
      
      print('📋 Today ($currentDayName) hours: $todayHours');
      
      if (todayHours == 'closed') {
        print('❌ Salons are CLOSED today (Sunday)');
      } else {
        final parts = todayHours!.split('-');
        final openTime = parts[0];
        final closeTime = parts[1];
        
        final openHour = int.parse(openTime.split(':')[0]);
        final openMinute = int.parse(openTime.split(':')[1]);
        final closeHour = int.parse(closeTime.split(':')[0]);
        final closeMinute = int.parse(closeTime.split(':')[1]);
        
        final currentMinutes = brazilTime.hour * 60 + brazilTime.minute;
        final openMinutes = openHour * 60 + openMinute;
        final closeMinutes = closeHour * 60 + closeMinute;
        
        print('⏰ Open: $openTime (${openMinutes}min), Close: $closeTime (${closeMinutes}min)');
        print('⏰ Current: ${brazilTime.hour}:${brazilTime.minute.toString().padLeft(2, '0')} (${currentMinutes}min)');
        
        if (currentMinutes >= openMinutes && currentMinutes <= closeMinutes) {
          print('✅ Salons SHOULD BE OPEN now');
        } else {
          print('❌ Salons are CLOSED (outside business hours)');
        }
      }
      
      expect(now, isA<DateTime>());
    });
  });
}

String _getDayName(int weekday) {
  switch (weekday) {
    case 1: return 'Monday';
    case 2: return 'Tuesday';
    case 3: return 'Wednesday';
    case 4: return 'Thursday';
    case 5: return 'Friday';
    case 6: return 'Saturday';
    case 7: return 'Sunday';
    default: return 'Unknown';
  }
}