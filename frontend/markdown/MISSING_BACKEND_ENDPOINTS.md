# Backend Issues

## Anonymous Queue Join Entity Framework Error

### Issue Description
The frontend anonymous queue joining functionality is failing due to a backend Entity Framework configuration issue, not a missing endpoint.

### Current Status
- ✅ `GET /api/Public/salons` - Working (displays salon list)
- ✅ `GET /api/Public/queue-status/{salonId}` - Working (displays queue status)
- ❌ `POST /api/Public/queue/join` - **EXISTS BUT FAILING** (EF configuration issue)

### Backend Error Details

**Error**: `System.InvalidCastException: Invalid cast from 'System.String' to 'Grande.Fila.API.Domain.Common.ValueObjects.Email'`

**Root Cause**: Entity Framework is trying to convert a string to a custom Email value object but the conversion is failing during the save operation.

**Technical Analysis**:
- Frontend correctly sends email as string: `"email": "rommelb@gmail.com"`
- Backend successfully receives request and finds salon/queue
- Error occurs during Entity Framework persistence when saving Email field
- The `Email` value object lacks proper EF Core conversion configuration

**Solution Needed**: 
1. Add EF Core value converter for Email value object in DbContext configuration
2. OR modify the Email value object to implement proper implicit string conversion
3. OR update entity model to accept string directly instead of Email value object

**EF Core Configuration Example**:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<QueueEntry>()
        .Property(e => e.Email)
        .HasConversion(
            email => email.Value,  // Email -> string
            value => new Email(value)  // string -> Email
        );
}
```

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

### Backend Fix Required
The backend needs to fix the Entity Framework configuration for the `Email` value object. The endpoint exists and is receiving requests correctly, but fails during data persistence.

**Logs show**:
1. ✅ Request received: "Anonymous user joining queue for salon..."
2. ✅ Salon found successfully 
3. ✅ Queue found successfully
4. ❌ EF conversion error when trying to save Email field

### Priority
**High** - This blocks the core anonymous user functionality that is essential for the MVP.