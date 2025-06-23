import 'package:flutter/foundation.dart';
import '../../models/queue_entry.dart';

/// Mock state management for queue entries during development phase
class MockQueueNotifier extends ChangeNotifier {
  // Changed to protected for test access
  List<QueueEntry> entries = [];
  bool isLoading = false;

  /// Current list of queue entries
  List<QueueEntry> get _entries => entries;

  /// Whether the queue is currently loading
  bool get _isLoading => isLoading;

  /// Number of people currently waiting
  int get waitingCount =>
      entries.where((entry) => entry.status == QueueStatus.waiting).length;

  /// Number of people currently in service
  int get inServiceCount =>
      entries.where((entry) => entry.status == QueueStatus.inService).length;

  // Default constructor loads mock data
  MockQueueNotifier() {
    _loadMockData();
  }

  // Named constructor for tests: does not load mock data
  MockQueueNotifier.forTest();

  /// Loads mock data for development
  void _loadMockData() {
    isLoading = true;
    notifyListeners();

    // Reduced delay for faster loading
    Future.delayed(const Duration(milliseconds: 100), () {
      entries = [
        QueueEntry(
          id: '1',
          name: 'John Doe',
          status: QueueStatus.waiting,
          joinTime: DateTime.now().subtract(const Duration(minutes: 15)),
          position: 1,
        ),
        QueueEntry(
          id: '2',
          name: 'Jane Smith',
          status: QueueStatus.waiting,
          joinTime: DateTime.now().subtract(const Duration(minutes: 10)),
          position: 2,
        ),
        QueueEntry(
          id: '3',
          name: 'Bob Johnson',
          status: QueueStatus.inService,
          joinTime: DateTime.now().subtract(const Duration(minutes: 25)),
          position: 0,
        ),
        QueueEntry(
          id: '4',
          name: 'Alice Brown',
          status: QueueStatus.waiting,
          joinTime: DateTime.now().subtract(const Duration(minutes: 5)),
          position: 3,
        ),
        QueueEntry(
          id: '5',
          name: 'Charlie Wilson',
          status: QueueStatus.completed,
          joinTime: DateTime.now().subtract(const Duration(minutes: 45)),
          position: 0,
        ),
      ];

      isLoading = false;
      notifyListeners();
    });
  }

  /// Adds a new person to the queue
  void addToQueue(String name) {
    final newEntry = QueueEntry(
      id: DateTime.now().millisecondsSinceEpoch.toString(),
      name: name,
      status: QueueStatus.waiting,
      joinTime: DateTime.now(),
      position: waitingCount + 1,
    );

    entries.add(newEntry);
    _updatePositions();
    notifyListeners();
  }

  /// Updates the status of a queue entry
  void updateStatus(String id, QueueStatus newStatus) {
    final index = entries.indexWhere((entry) => entry.id == id);
    if (index != -1) {
      entries[index] = entries[index].copyWith(status: newStatus);
      _updatePositions();
      notifyListeners();
    }
  }

  /// Removes a person from the queue
  void removeFromQueue(String id) {
    entries.removeWhere((entry) => entry.id == id);
    _updatePositions();
    notifyListeners();
  }

  /// Updates positions for waiting entries
  void _updatePositions() {
    final waitingEntries = entries
        .where((entry) => entry.status == QueueStatus.waiting)
        .toList()
      ..sort((a, b) => a.joinTime.compareTo(b.joinTime));

    for (int i = 0; i < waitingEntries.length; i++) {
      final index =
          entries.indexWhere((entry) => entry.id == waitingEntries[i].id);
      if (index != -1) {
        entries[index] = entries[index].copyWith(position: i + 1);
      }
    }

    // Set position to 0 for non-waiting entries
    for (int i = 0; i < entries.length; i++) {
      if (entries[i].status != QueueStatus.waiting) {
        entries[i] = entries[i].copyWith(position: 0);
      }
    }
  }

  /// Refreshes the queue data
  void refresh() {
    _loadMockData();
  }
}
