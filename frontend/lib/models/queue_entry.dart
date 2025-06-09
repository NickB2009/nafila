/// Represents a person in the queue with their current status
class QueueEntry {
  final String id;
  final String name;
  final QueueStatus status;
  final DateTime joinTime;
  final int position;

  const QueueEntry({
    required this.id,
    required this.name,
    required this.status,
    required this.joinTime,
    required this.position,
  });

  /// Creates a copy of this QueueEntry with the given fields replaced
  QueueEntry copyWith({
    String? id,
    String? name,
    QueueStatus? status,
    DateTime? joinTime,
    int? position,
  }) {
    return QueueEntry(
      id: id ?? this.id,
      name: name ?? this.name,
      status: status ?? this.status,
      joinTime: joinTime ?? this.joinTime,
      position: position ?? this.position,
    );
  }

  @override
  String toString() {
    return 'QueueEntry(id: $id, name: $name, status: $status, position: $position)';
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is QueueEntry &&
        other.id == id &&
        other.name == name &&
        other.status == status &&
        other.joinTime == joinTime &&
        other.position == position;
  }

  @override
  int get hashCode {
    return id.hashCode ^
        name.hashCode ^
        status.hashCode ^
        joinTime.hashCode ^
        position.hashCode;
  }
}

/// Represents the different states a queue entry can be in
enum QueueStatus {
  waiting('Waiting'),
  inService('In Service'),
  completed('Completed');

  const QueueStatus(this.displayName);

  final String displayName;
}
