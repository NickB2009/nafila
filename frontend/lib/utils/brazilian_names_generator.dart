import 'dart:math';

class BrazilianNamesGenerator {
  static final Random _random = Random();

  static final List<String> _firstNames = [
    'João', 'Antonio', 'Francisco', 'Carlos', 'Paulo', 'Pedro', 'Lucas', 'Luiz',
    'Marcos', 'Luis', 'Gabriel', 'Rafael', 'Daniel', 'Marcelo', 'Bruno',
    'Eduardo', 'Felipe', 'Ruan', 'Diego', 'Matheus', 'Guilherme', 'Arthur',
    'Gustavo', 'André', 'Rodrigo', 'Fernando', 'Fábio', 'Leonardo', 'Juliano',
    'Vitor', 'Caio', 'Thiago', 'Ricardo', 'Renato', 'Sergio', 'Maurício',
    'Henrique', 'Alexandre', 'Vinicius', 'Otávio', 'Raul', 'Renan', 'Nicolas',
    'Enzo', 'Davi', 'Miguel', 'Bernardo', 'Samuel', 'Joaquim', 'Théo',
    'Bento', 'Benício', 'Ravi', 'Gael', 'Noah', 'Anthony', 'Isaac',
    'Davi Lucca', 'Pietro', 'Cauã', 'Levi', 'Ian', 'Caleb', 'Antônio',
    'Vicente', 'Luan', 'Emanuel', 'Benjamin', 'Isaque', 'Murilo', 'Lorenzo'
  ];

  static final List<String> _lastNames = [
    'Silva', 'Santos', 'Oliveira', 'Souza', 'Rodrigues', 'Ferreira', 'Alves',
    'Pereira', 'Lima', 'Gomes', 'Ribeiro', 'Carvalho', 'Almeida', 'Lopes',
    'Monteiro', 'Mendes', 'Barros', 'Freitas', 'Barbosa', 'Machado', 'Nascimento',
    'Moreira', 'Moraes', 'Araújo', 'Rocha', 'Correia', 'Teixeira', 'Nunes',
    'Cardoso', 'Ramos', 'Vieira', 'Dias', 'Castro', 'Pinto', 'Azevedo',
    'Cunha', 'Melo', 'Fernandes', 'Campos', 'Martins', 'Costa', 'Coelho',
    'Fonseca', 'Morais', 'Pires', 'Batista', 'Cavalcante', 'Sampaio', 'Reis',
    'Rezende', 'Brito', 'Andrade', 'Guimarães', 'Nogueira', 'Tavares', 'Siqueira',
    'Paiva', 'Magalhães', 'Borges', 'Duarte', 'Medeiros', 'Carneiro', 'Sá'
  ];

  static final List<String> _funnyNicknames = [
    'Beto', 'Zé', 'Chico', 'Duda', 'Binho', 'Vitão', 'Cacá', 'Juca',
    'Nando', 'Tito', 'Caco', 'Dinho', 'Zeca', 'Netinho', 'Pedrinho',
    'Paulinho', 'Carlinhos', 'Marquinhos', 'Bruninho', 'Felipão', 'Thiagão',
    'Ricardinho', 'Serginho', 'Mauricinho', 'Rafa', 'Dani', 'Léo', 'Gui',
    'Guga', 'Dudu', 'Lulu', 'Nene', 'Gegê', 'Dede', 'Kiko', 'Zico'
  ];

  /// Generates a random Brazilian male name in the format "FirstName LastName"
  static String generateFullName() {
    final firstName = _firstNames[_random.nextInt(_firstNames.length)];
    final lastName = _lastNames[_random.nextInt(_lastNames.length)];
    return '$firstName $lastName';
  }

  /// Generates a random Brazilian male first name
  static String generateFirstName() {
    return _firstNames[_random.nextInt(_firstNames.length)];
  }

  /// Generates a random Brazilian male nickname
  static String generateNickname() {
    return _funnyNicknames[_random.nextInt(_funnyNicknames.length)];
  }

  /// Generates a random Brazilian male name with just first letter of last name
  /// Example: "João S" or "Carlos M"
  static String generateNameWithInitial() {
    final firstName = _firstNames[_random.nextInt(_firstNames.length)];
    final lastName = _lastNames[_random.nextInt(_lastNames.length)];
    return '$firstName ${lastName[0]}';
  }

  /// Generates a fun greeting with a random name
  static String generateGreeting() {
    final name = generateFirstName();
    return 'Olá, $name!';
  }

  /// Generates an email based on a random name
  static String generateEmail() {
    final firstName = _firstNames[_random.nextInt(_firstNames.length)].toLowerCase();
    final lastName = _lastNames[_random.nextInt(_lastNames.length)].toLowerCase();
    final domains = ['gmail.com', 'hotmail.com', 'yahoo.com.br', 'outlook.com'];
    final domain = domains[_random.nextInt(domains.length)];
    return '$firstName.$lastName@$domain';
  }

  /// Generates a list of random Brazilian male names
  static List<String> generateNameList(int count) {
    final List<String> names = [];
    for (int i = 0; i < count; i++) {
      names.add(generateFullName());
    }
    return names;
  }

  /// Generates a list of random Brazilian male names with initials
  static List<String> generateNameWithInitialList(int count) {
    final List<String> names = [];
    for (int i = 0; i < count; i++) {
      names.add(generateNameWithInitial());
    }
    return names;
  }

  /// Gets a random name from a predefined list for consistent testing
  static String getConsistentName(int index) {
    final names = [
      'João Silva', 'Carlos Santos', 'Pedro Oliveira', 'Lucas Souza',
      'Rafael Ferreira', 'Gabriel Alves', 'Bruno Pereira', 'Eduardo Lima',
      'Felipe Gomes', 'Matheus Ribeiro', 'Diego Carvalho', 'Thiago Almeida',
      'Vinicius Lopes', 'Gustavo Monteiro', 'André Mendes', 'Rodrigo Barros',
      'Leonardo Freitas', 'Marcelo Barbosa', 'Caio Machado', 'Henrique Nascimento'
    ];
    return names[index % names.length];
  }

  /// Gets a random name with initial from a predefined list for consistent testing
  static String getConsistentNameWithInitial(int index) {
    final names = [
      'João S', 'Carlos M', 'Pedro O', 'Lucas R', 'Rafael F', 'Gabriel A',
      'Bruno P', 'Eduardo L', 'Felipe G', 'Matheus R', 'Diego C', 'Thiago A',
      'Vinicius L', 'Gustavo M', 'André M', 'Rodrigo B', 'Leonardo F',
      'Marcelo B', 'Caio M', 'Henrique N'
    ];
    return names[index % names.length];
  }
} 