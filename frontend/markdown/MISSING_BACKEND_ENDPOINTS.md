# Missing Backend Endpoints

## Anonymous Queue Join Endpoint

### Issue Description
The frontend anonymous queue joining functionality is currently failing because the backend is missing a public endpoint for anonymous users to join queues.

### Current Status
- ✅ `GET /api/Public/salons` - Working (displays salon list)
- ✅ `GET /api/Public/queue-status/{salonId}` - Working (displays queue status)
- ❌ `POST /api/Public/queue/join` - **MISSING** (anonymous queue join)

### Required Backend Implementation

**Endpoint**: `POST /api/Public/queue/join`
**Access**: `[AllowAnonymous]`
**Description**: Allows anonymous users to join a queue without authentication

#### Request Body
```json
{
  "salonId": "99999999-9999-9999-9999-999999999993",
  "name": "rommel",
  "email": "rommelb@gmail.com", 
  "anonymousUserId": "generated-uuid",
  "serviceRequested": "Haircut",
  "emailNotifications": true,
  "browserNotifications": true
}
```

#### Response Body (200 OK)
```json
{
  "id": "queue-entry-id",
  "position": 1,
  "estimatedWaitMinutes": 15,
  "joinedAt": "2025-01-XX...",
  "status": "waiting"
}
```

#### Error Responses
- `400 Bad Request`: Invalid request data
- `404 Not Found`: Salon not found or not accepting customers
- `409 Conflict`: User already in queue for this salon
- `500 Internal Server Error`: Server error

### Frontend Integration Ready
The frontend `AnonymousQueueService.joinQueue()` method is already implemented and ready to consume this endpoint once it's available on the backend.

### Testing Instructions
Once implemented, test with:
```bash
curl -X POST https://localhost:7126/api/Public/queue/join \
  -H "Content-Type: application/json" \
  -d '{
    "salonId": "99999999-9999-9999-9999-999999999993",
    "name": "Test User",
    "email": "test@example.com",
    "anonymousUserId": "test-uuid",
    "serviceRequested": "Haircut",
    "emailNotifications": true,
    "browserNotifications": true
  }'
```

### Priority
**High** - This blocks the core anonymous user functionality that is essential for the MVP.