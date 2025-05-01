# Clean Architecture Migration Plan

This document outlines concrete steps to eliminate duplications and properly implement Clean Architecture principles in the project.

## Phase 1: Consolidate Domain Services

1. **Wait Time Calculator Service**
   - ✅ Create canonical implementation in `domain/services/wait_time_calculator.py`
   - Update all references in:
     - `domain/services.py` -> Import from canonical source
     - `domain/domain_models.py` -> Remove duplicate implementation 
     - `barbershop/models.py` -> Update imports
     - `application/services.py` -> Update imports

2. **Queue Service**
   - ✅ Refactor to use canonical `WaitTimeCalculator`
   - Update any direct usage of other implementations

## Phase 2: Repository Interface Consolidation

1. **Create a Single Interface Source**
   - Move all application interfaces to domain interfaces:
     - `IClienteRepository` -> `domain/interfaces/repositories.py`
     - `IBarbeariaRepository` -> `domain/interfaces/repositories.py`
     - Ensure methods are domain-focused and free of infrastructure concerns

2. **Update Repository Implementations**
   - Update all implementations to use domain interfaces:
     - `DjangoClienteRepository` -> Implement domain interface
     - `DjangoBarbeariaRepository` -> Implement domain interface

3. **Remove Duplicate Interfaces**
   - Remove duplicated interfaces in `application/interfaces.py`
   - Update import references throughout the codebase

## Phase 3: Model/Entity Consolidation

1. **Separate Domain Entities from ORM Models**
   - Keep pure domain entities in `domain/entities.py`
   - Move all Django ORM models to `infrastructure/models/`:
     - `infrastructure/models/barbershop_models.py`
     - `infrastructure/models/cliente_models.py`
     - etc.

2. **Create Entity-to-Model Adapters**
   - Create adapters for converting between domain entities and ORM models:
     - `infrastructure/adapters/entity_adapters.py`
   - Implement two-way conversion methods

3. **Update Repository Implementations**
   - Convert between domain entities and ORM models in repositories
   - Ensure repositories return domain entities, not ORM models

## Phase 4: Repository Implementation Consolidation

1. **Centralize Repository Implementations**
   - Create one file per entity in `infrastructure/repositories/`:
     - `barbershop_repository.py`
     - `cliente_repository.py`
     - etc.

2. **Remove Duplicate Implementations**
   - Replace duplicate implementations with imports from canonical source
   - Update factory methods to use canonical implementations

## Implementation Order

For minimal disruption, follow this order:

1. Start with service consolidation (Phase 1)
2. Then repository interface consolidation (Phase 2)
3. Followed by model/entity consolidation (Phase 3)
4. Finally, repository implementation consolidation (Phase 4)

Always maintain test coverage during changes to catch regressions early.

## Example Implementation

Here's a comparison of before and after for converting the `WaitTimeCalculator` service:

### Before:
Multiple implementations in different files with slight variations.

### After:
```python
# domain/services/wait_time_calculator.py
from typing import List

class WaitTimeCalculator:
    """
    Domain service for calculating and formatting queue wait times.
    This is the canonical implementation to be used throughout the application.
    """
    
    @staticmethod
    def calculate(service_durations: List[int], active_barber_count: int) -> int:
        # Implementation
        
    @staticmethod
    def format_wait_time(minutes: int) -> str:
        # Implementation
```

Then use it in all places by importing:
```python
from domain.services.wait_time_calculator import WaitTimeCalculator
```

## Monitoring Progress

Create a tracking issue in your project management system and check off each step as it's completed. Always work in small, testable increments to minimize risk. 