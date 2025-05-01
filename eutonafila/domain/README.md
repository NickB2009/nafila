# Domain Layer Documentation

## Identified Duplications in the Codebase

After a thorough analysis of the project structure, I've identified the following duplications that should be addressed:

### 1. Repository Interface Duplications

There are duplicate repository interfaces in:
- `domain/interfaces/repositories.py`: Contains domain-specific repository interfaces
- `application/interfaces.py`: Contains application-level repository interfaces

**Recommendation**: 
- Maintain only one set of repository interfaces in `domain/interfaces/` 
- Have application layer use the domain interfaces 
- Remove duplicate interfaces from `application/interfaces.py`

### 2. Entity/Model Duplications

The `Barbearia` entity/model exists in multiple places:
- `domain/entities.py`: Pure domain entity
- `domain/models.py`: Django model version
- `barbershop/models.py`: ORM implementation

**Recommendation**:
- Keep pure domain entities in `domain/entities.py`
- Move all Django ORM implementations to `infrastructure/models/`
- Ensure all application code references domain entities
- Use adapters to convert between ORM models and domain entities

### 3. Service Implementation Duplications

`WaitTimeCalculator` service is implemented in multiple places:
- `domain/services.py`: Original implementation
- `domain/domain_models.py`: Duplicate implementation
- `domain/services/queue_service.py`: Partial reimplementation

**Recommendation**:
- Keep a single implementation in `domain/services/`
- Remove duplicate implementations
- Ensure all code references the canonical implementation

### 4. Repository Implementation Duplications

`DjangoBarbeariaRepository` and other repository implementations appear in multiple files:
- `infrastructure/repositories.py`
- `eutonafila/infrastructure/repositories.py` 

**Recommendation**:
- Consolidate repository implementations into a single location in `infrastructure/repositories/`
- Organize by domain entity (one file per entity)
- Use proper importing to avoid circular dependencies

### 5. Adapter Pattern Duplication

- New adapters in `infrastructure/adapters/repository_adapters.py` might duplicate functionality already in repositories

**Recommendation**:
- Use a consistent adapter pattern throughout
- Ensure adapters convert between domain and infrastructure concerns
- Avoid implementing business logic in adapters

## Clean Architecture Implementation Strategy

To consolidate these duplications while maintaining Clean Architecture principles:

1. **Domain Layer**:
   - Keep pure domain entities and interfaces
   - Move all business logic to domain services
   - Remove framework dependencies

2. **Application Layer**:
   - Use domain interfaces
   - Implement use cases
   - Coordinate between UI and domain

3. **Infrastructure Layer**:
   - Implement domain interfaces
   - Adapt external concerns to domain interfaces
   - Handle framework-specific code

4. **Presentation Layer**:
   - Focus on UI/API concerns
   - Use application services
   - Handle user interaction

By addressing these duplications, the codebase will become more maintainable, easier to test, and better aligned with Clean Architecture principles. 