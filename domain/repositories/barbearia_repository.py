from abc import ABC, abstractmethod
from typing import List, Optional
from uuid import UUID

# This would normally import from domain.entities.barbearia,
# but we'll assume it's not created yet
# from domain.entities.barbearia import Barbearia

# For now, we'll use a type hint with string
Barbearia = "Barbearia"

class IBarbeariaRepository(ABC):
    """
    Repository interface for Barbearia entity.
    This defines the contract for any Barbearia repository implementation
    following Clean Architecture principles.
    """
    
    @abstractmethod
    def get_by_id(self, id: UUID) -> Optional[Barbearia]:
        """Get a barbershop by its ID"""
        pass
    
    @abstractmethod
    def get_by_slug(self, slug: str) -> Optional[Barbearia]:
        """Get a barbershop by its slug"""
        pass
    
    @abstractmethod
    def get_all(self) -> List[Barbearia]:
        """Get all barbershops"""
        pass
    
    @abstractmethod
    def save(self, barbearia: Barbearia) -> Barbearia:
        """Save a barbershop (create if does not exist, update if exists)"""
        pass
    
    @abstractmethod
    def update(self, barbearia: Barbearia) -> Barbearia:
        """Update an existing barbershop"""
        pass
    
    @abstractmethod
    def delete(self, id: UUID) -> bool:
        """Delete a barbershop by ID"""
        pass 