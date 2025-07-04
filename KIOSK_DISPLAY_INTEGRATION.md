# Kiosk Display Integration Guide

## Overview
The Kiosk Display API has been enhanced to return the current queue position number. This allows the kiosk to display "Atendendo senha: [X]" where X is the position number.

## API Endpoint
```
GET /api/kiosk/display/{locationId}
```

## Response Structure
The API now returns a `currentPosition` field along with other queue information:

```json
{
  "success": true,
  "currentPosition": 1,        // NEW FIELD - Position number currently being served
  "currentlyServing": "João Silva",  // Customer name being served
  "totalWaiting": 2,
  "queueEntries": [
    {
      "position": 1,
      "status": "CheckedIn",   // Currently being served
      "customerName": "João Silva",
      ...
    },
    {
      "position": 2,
      "status": "Waiting",
      "customerName": "Maria Santos",
      ...
    }
  ]
}
```

## Flutter Integration Example

```dart
// In your Flutter kiosk screen:

Future<void> fetchCurrentPosition() async {
  final response = await http.get(
    Uri.parse('${baseUrl}/api/kiosk/display/$locationId'),
  );

  if (response.statusCode == 200) {
    final data = json.decode(response.body);
    
    if (data['success']) {
      setState(() {
        currentPosition = data['currentPosition'];
      });
    }
  }
}

// Display the current position:
Text(
  currentPosition != null 
    ? 'Atendendo senha: $currentPosition'
    : 'Nenhuma senha sendo atendida',
  style: TextStyle(fontSize: 48, fontWeight: FontWeight.bold),
)
```

## Status Codes
- **200 OK**: Success with queue data
- **204 No Content**: Location exists but no queue entries
- **400 Bad Request**: Invalid location ID format
- **404 Not Found**: Location doesn't exist

## Notes
- The `currentPosition` field will be `null` if no customer is currently being served (status = "CheckedIn")
- The endpoint is public (no authentication required) for kiosk displays
- Poll this endpoint periodically (e.g., every 5-10 seconds) to keep the display updated