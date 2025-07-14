import 'package:flutter_test/flutter_test.dart';
import 'package:eutonafila_frontend/utils/brazilian_names_generator.dart';

void main() {
  group('BrazilianNamesGenerator', () {
    test('generateFullName returns a proper Brazilian name', () {
      final name = BrazilianNamesGenerator.generateFullName();
      expect(name, isNotEmpty);
      expect(name.contains(' '), isTrue);
      final parts = name.split(' ');
      expect(parts.length, equals(2));
      expect(parts[0], isNotEmpty);
      expect(parts[1], isNotEmpty);
    });

    test('generateFirstName returns a non-empty string', () {
      final firstName = BrazilianNamesGenerator.generateFirstName();
      expect(firstName, isNotEmpty);
    });

    test('generateNickname returns a non-empty string', () {
      final nickname = BrazilianNamesGenerator.generateNickname();
      expect(nickname, isNotEmpty);
    });

    test('generateNameWithInitial returns name with initial', () {
      final name = BrazilianNamesGenerator.generateNameWithInitial();
      expect(name, isNotEmpty);
      expect(name.contains(' '), isTrue);
      final parts = name.split(' ');
      expect(parts.length, equals(2));
      expect(parts[0], isNotEmpty);
      expect(parts[1].length, equals(1));
    });

    test('generateGreeting returns a greeting in Portuguese', () {
      final greeting = BrazilianNamesGenerator.generateGreeting();
      expect(greeting, isNotEmpty);
      expect(greeting.startsWith('Olá, '), isTrue);
      expect(greeting.endsWith('!'), isTrue);
    });

    test('generateEmail returns a valid email format', () {
      final email = BrazilianNamesGenerator.generateEmail();
      expect(email, isNotEmpty);
      expect(email.contains('@'), isTrue);
      expect(email.contains('.'), isTrue);
      
      final parts = email.split('@');
      expect(parts.length, equals(2));
      expect(parts[0], isNotEmpty);
      expect(parts[1], isNotEmpty);
    });

    test('generateNameList returns correct number of names', () {
      final names = BrazilianNamesGenerator.generateNameList(5);
      expect(names.length, equals(5));
      for (final name in names) {
        expect(name, isNotEmpty);
        expect(name.contains(' '), isTrue);
      }
    });

    test('generateNameWithInitialList returns correct number of names', () {
      final names = BrazilianNamesGenerator.generateNameWithInitialList(3);
      expect(names.length, equals(3));
      for (final name in names) {
        expect(name, isNotEmpty);
        expect(name.contains(' '), isTrue);
        final parts = name.split(' ');
        expect(parts.length, equals(2));
        expect(parts[1].length, equals(1));
      }
    });

    test('getConsistentName returns consistent names for same index', () {
      final name1 = BrazilianNamesGenerator.getConsistentName(0);
      final name2 = BrazilianNamesGenerator.getConsistentName(0);
      expect(name1, equals(name2));
      
      final name3 = BrazilianNamesGenerator.getConsistentName(1);
      expect(name1, isNot(equals(name3)));
    });

    test('getConsistentNameWithInitial returns consistent names for same index', () {
      final name1 = BrazilianNamesGenerator.getConsistentNameWithInitial(0);
      final name2 = BrazilianNamesGenerator.getConsistentNameWithInitial(0);
      expect(name1, equals(name2));
      
      final name3 = BrazilianNamesGenerator.getConsistentNameWithInitial(1);
      expect(name1, isNot(equals(name3)));
    });

    test('random generation produces different results', () {
      final names = <String>{};
      for (int i = 0; i < 10; i++) {
        names.add(BrazilianNamesGenerator.generateFullName());
      }
      // Should generate at least some different names
      expect(names.length, greaterThan(1));
    });
  });
} 