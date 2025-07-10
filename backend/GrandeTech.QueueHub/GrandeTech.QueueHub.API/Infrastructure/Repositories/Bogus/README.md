# Bogus Singleton Data Store

This directory contains a singleton pattern implementation for Bogus repositories that maintains in-memory state across HTTP requests, making it perfect for testing with Postman and HTTP files.

## How It Works

The `BogusDataStore` is a static class that maintains a thread-safe, in-memory data store using `ConcurrentDictionary`. It provides:

- **Singleton Pattern**: Data persists across requests
- **Thread Safety**: Uses `ConcurrentDictionary` for thread-safe operations
- **Type Safety**: Generic methods for different entity types
- **Initialization**: Automatic data generation on first access

## Key Components

### 1. BogusDataStore.cs
The main singleton class that manages all in-memory data.

### 2. BogusBaseRepository.cs
Updated to use the singleton data store instead of local static dictionaries.

### 3. BogusUserRepository.cs
Example of a specific repository using the singleton pattern.

### 4. BogusDataStoreExtensions.cs
Extension methods for easier data manipulation.

## Usage

### Basic Operations

```csharp
// Get all entities of a type
var users = BogusDataStore.GetAll<User>();

// Get by ID
var user = BogusDataStore.Get<User>(userId);

// Add new entity
BogusDataStore.Add(newUser);

// Update entity
BogusDataStore.Update(updatedUser);

// Remove entity
BogusDataStore.Remove<User>(userId);

// Clear all data
BogusDataStore.Clear();

// Re-initialize with fresh data
BogusDataStore.Initialize();
```

### Adding New Entity Types

1. **Register the entity type**:
```csharp
BogusDataStore.RegisterEntityType<YourEntity>();
```

2. **Update the Initialize method** in `BogusDataStore.cs`:
```csharp
public static void Initialize()
{
    lock (_lock)
    {
        if (_isInitialized) return;
        
        RegisterEntityType<User>();
        RegisterEntityType<Organization>();
        RegisterEntityType<YourEntity>(); // Add this line
        
        GenerateInitialData();
        _isInitialized = true;
    }
}
```

3. **Add data generation** in the `GenerateInitialData` method:
```csharp
private static void GenerateInitialData()
{
    // Existing code...
    
    // Add your entity generation
    var yourEntityFaker = new Bogus.Faker<YourEntity>()
        .CustomInstantiator(f => new YourEntity(/* parameters */));
    
    var yourEntities = yourEntityFaker.Generate(10);
    foreach (var entity in yourEntities)
    {
        Add(entity);
    }
}
```

### Testing Endpoints

Use the `TestController` endpoints to verify the singleton behavior:

- `GET /api/test/status` - Check data counts
- `GET /api/test/users` - Get all users
- `GET /api/test/organizations` - Get all organizations
- `POST /api/test/clear` - Clear all data
- `POST /api/test/initialize` - Re-initialize data

## Benefits

1. **Persistent State**: Data survives across HTTP requests
2. **Easy Testing**: Perfect for Postman and HTTP file testing
3. **Thread Safe**: Safe for concurrent access
4. **Extensible**: Easy to add new entity types
5. **Memory Efficient**: Uses concurrent collections
6. **Reset Capability**: Can clear and re-initialize data

## Testing with HTTP Files

Use the provided `test-singleton.http` file to test the singleton functionality:

```http
### Get initial status
GET {{baseUrl}}/api/test/status

### Register a new user
POST {{baseUrl}}/api/auth/register
Content-Type: application/json

{
  "username": "testuser",
  "email": "test@example.com", 
  "password": "TestPassword123!"
}

### Check that user was added
GET {{baseUrl}}/api/test/users
```

## Migration from Old Pattern

If you have existing Bogus repositories that use local static dictionaries:

1. Remove the local static dictionary
2. Replace all operations with `BogusDataStore` calls
3. Remove the constructor if it only initializes data
4. Register the entity type in `BogusDataStore.Initialize()`

This pattern provides a clean, maintainable solution for in-memory testing while keeping the code simple and extensible. 