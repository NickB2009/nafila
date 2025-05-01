# Projeto Na Fila - Clean Architecture Implementation

## Fixing the Database Issue

1. The project is experiencing a database error related to a missing column `max_capacity` in the `barbershop_barbearia` table.

2. To fix this issue, run the following commands:

```bash
# Activate virtual environment
.\venv\Scripts\activate

# Apply migrations
python apply_migrations.py
```

3. Alternatively, you can run the batch file:

```bash
.\apply_migrations.bat
```

## Implementing Clean Architecture

This project includes a plan to restructure the codebase according to Clean Architecture principles.

### Step 1: Set Up Directory Structure

Run the following command to set up the Clean Architecture directory structure:

```bash
.\setup_clean_architecture.bat
```

This will create the following directories:

- `domain/` - Business rules and entities
- `application/` - Use cases and application services
- `infrastructure/` - External frameworks and tools
- `interfaces/` - Controllers and presenters

### Step 2: Migrate Code to New Structure

The project includes examples of how to implement Clean Architecture:

- **Domain Entity**: `domain/entities/barbearia.py`
- **Repository Interface**: `domain/repositories/barbearia_repository.py`
- **Repository Implementation**: `infrastructure/repositories/django/django_barbearia_repository.py`
- **Use Case**: `application/use_cases/barbershop/get_barbershop_info.py`

Follow these examples to migrate existing code to the new structure.

### Key Principles to Follow

1. **Dependency Rule**: Dependencies point inward. Outer layers depend on inner layers.
2. **Domain Independence**: Domain entities and business rules should not depend on any external frameworks.
3. **Separation of Concerns**: Each layer has its own responsibility.
4. **Testability**: Each layer can be tested independently.

### Step 3: Gradual Migration Strategy

To avoid disrupting the current application:

1. Start with defining domain entities and repository interfaces
2. Implement repository implementations in the infrastructure layer
3. Create use cases to handle business operations
4. Update the presentation layer to use the new use cases
5. Gradually phase out old code when new implementations are stable

## Documentation

A comprehensive plan for implementing Clean Architecture is available in `clean_architecture_plan.md`.

## Benefits

- **Maintainability**: Clear separation of concerns makes the code easier to understand and maintain
- **Testability**: Business logic is isolated and easily testable
- **Flexibility**: The application can adapt to changing requirements
- **Independence**: Business logic is independent of frameworks and external systems 