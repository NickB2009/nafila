# Clean Architecture Implementation Plan

## Current Structure Issues
- Inconsistent organization of domain and application layers
- Multiple model definitions across different files
- Repository pattern implementation is mixed in infrastructure layer
- Missing database column (`max_capacity`) causing errors

## Step 1: Fix Immediate Database Issues
1. Apply the migration to add the `max_capacity` column to `barbershop_barbearia` table
2. Run the `apply_migrations.bat` script

## Step 2: Reorganize Project Structure

### Domain Layer
- Contains business rules and enterprise logic
- Should be independent of other layers

```
domain/
  ├── entities/                  # Business objects with business logic
  │   ├── __init__.py
  │   ├── barbearia.py           # Barbershop entity
  │   ├── cliente.py             # Client entity
  │   ├── barbeiro.py            # Barber entity
  │   ├── servico.py             # Service entity
  │   └── fila.py                # Queue entry entity
  ├── value_objects/             # Immutable objects that represent concepts
  │   ├── __init__.py
  │   ├── status.py              # Status enums 
  │   └── location.py            # Location value object
  ├── services/                  # Domain services
  │   ├── __init__.py
  │   ├── queue_manager.py       # Queue management logic
  │   └── wait_time_calculator.py # Wait time calculation logic
  ├── exceptions/                # Domain-specific exceptions
  │   ├── __init__.py
  │   └── domain_exceptions.py
  └── repositories/              # Repository interfaces (not implementations)
      ├── __init__.py
      ├── barbearia_repository.py
      ├── cliente_repository.py
      ├── barbeiro_repository.py
      ├── servico_repository.py
      └── fila_repository.py
```

### Application Layer
- Contains application use cases
- Orchestrates entities to perform business tasks

```
application/
  ├── use_cases/                 # Application use cases
  │   ├── __init__.py
  │   ├── queue/                 # Queue-related use cases
  │   │   ├── __init__.py
  │   │   ├── add_to_queue.py
  │   │   ├── remove_from_queue.py
  │   │   └── get_queue_status.py
  │   ├── barbershop/            # Barbershop-related use cases
  │   │   ├── __init__.py
  │   │   ├── create_barbershop.py
  │   │   ├── update_barbershop.py
  │   │   └── get_barbershop_info.py
  │   └── client/                # Client-related use cases
  │       ├── __init__.py
  │       ├── register_client.py
  │       └── update_client.py
  ├── interfaces/                # Application interfaces
  │   ├── __init__.py
  │   └── services/              # External service interfaces
  │       ├── __init__.py
  │       ├── notification_service.py
  │       └── payment_service.py
  ├── dtos/                      # Data Transfer Objects
  │   ├── __init__.py
  │   ├── cliente_dto.py
  │   ├── barbeiro_dto.py
  │   ├── barbearia_dto.py
  │   └── fila_dto.py
  └── exceptions/                # Application-specific exceptions
      ├── __init__.py
      └── application_exceptions.py
```

### Infrastructure Layer
- Contains implementations of interfaces defined in inner layers
- Provides concrete implementations of repositories, services, etc.

```
infrastructure/
  ├── repositories/              # Repository implementations
  │   ├── __init__.py
  │   ├── django/                # Django ORM implementations
  │   │   ├── __init__.py
  │   │   ├── django_barbearia_repository.py
  │   │   ├── django_cliente_repository.py
  │   │   ├── django_barbeiro_repository.py
  │   │   ├── django_servico_repository.py
  │   │   └── django_fila_repository.py
  │   └── memory/                # In-memory implementations (for testing)
  │       ├── __init__.py
  │       └── memory_repositories.py
  ├── orm/                       # ORM models
  │   ├── __init__.py
  │   ├── barbershop_models.py   # Django ORM models for barbershop app
  │   └── website_models.py      # Django ORM models for website app
  ├── services/                  # External service implementations
  │   ├── __init__.py
  │   ├── sms_notification_service.py
  │   └── email_notification_service.py
  ├── cache/                     # Caching implementations
  │   ├── __init__.py
  │   └── redis_cache_service.py
  └── messaging/                 # Messaging implementations
      ├── __init__.py
      └── websocket_service.py
```

### Interfaces Layer (Presentation)
- Contains UI implementations
- Responsible for delivering data to and from users

```
interfaces/
  ├── api/                      # API controllers
  │   ├── __init__.py
  │   ├── rest/                 # REST API
  │   │   ├── __init__.py
  │   │   ├── serializers/
  │   │   │   ├── __init__.py
  │   │   │   ├── barbearia_serializer.py
  │   │   │   └── cliente_serializer.py
  │   │   └── views/
  │   │       ├── __init__.py
  │   │       ├── barbearia_views.py
  │   │       └── cliente_views.py
  │   └── graphql/             # GraphQL API
  │       ├── __init__.py
  │       └── schema.py
  └── web/                     # Web controllers
      ├── __init__.py
      └── views/               # Django views
          ├── __init__.py
          ├── barbearia_views.py
          └── cliente_views.py
```

## Step 3: Implementation Strategy

1. **Start with Domain Layer**
   - Move entities from existing models to dedicated entity files
   - Define repository interfaces in domain layer
   - Extract domain services from existing code
   
2. **Refactor Application Layer**
   - Create use cases for each business operation
   - Define DTOs for data transfer
   - Define application service interfaces
   
3. **Implement Infrastructure Layer**
   - Implement repository classes that implement domain interfaces
   - Create Django ORM models that match domain entities
   - Implement external service adapters
   
4. **Implement Interfaces Layer**
   - Create API controllers and serializers
   - Create web views and templates
   
5. **Set Up Dependency Injection**
   - Create a dependency injection container to manage dependencies
   - Configure DI container to inject the right implementations

## Step 4: Testing Strategy

1. **Domain Layer Tests**
   - Unit tests for entities and domain services
   - Mock repositories for testing entities
   
2. **Application Layer Tests**
   - Unit tests for use cases
   - Mock repositories and services
   
3. **Infrastructure Layer Tests**
   - Integration tests for repository implementations
   - Tests for external service adapters
   
4. **Interfaces Layer Tests**
   - API tests
   - End-to-end tests

## Migration Strategy

To avoid disrupting the current application, we'll use a gradual migration strategy:

1. Fix the immediate database issues
2. Implement the new architecture alongside the existing code
3. Gradually migrate functionality to the new architecture
4. Remove deprecated code after successful migration

## Benefits of Clean Architecture

1. **Maintainability**: Clear separation of concerns makes the code easier to maintain
2. **Testability**: Business logic is isolated and easily testable
3. **Flexibility**: The application can adapt to changing requirements
4. **Independence**: Business logic is independent of frameworks and external systems 