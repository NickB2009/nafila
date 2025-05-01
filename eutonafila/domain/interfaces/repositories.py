from abc import ABC, abstractmethod
from typing import List, Optional
from uuid import UUID

# This file defines repository interfaces from the domain perspective
# These are the core interfaces that domain services depend on

class IBarbeariaRepository(ABC):
    """Domain interface for barbershop repository"""
    
    @abstractmethod
    def get_by_id(self, id: UUID) -> Optional['Barbearia']:
        """Get barbershop by ID"""
        pass
    
    @abstractmethod
    def get_by_slug(self, slug: str) -> Optional['Barbearia']:
        """Get barbershop by slug"""
        pass
    
    @abstractmethod
    def is_queue_full(self, barbearia_id: UUID) -> bool:
        """Check if barbershop queue is at max capacity"""
        pass
    
    @abstractmethod
    def is_open(self, barbearia_id: UUID) -> bool:
        """Check if barbershop is currently open"""
        pass

class IBarbeiroRepository(ABC):
    """Domain interface for barber repository"""
    
    @abstractmethod
    def get_by_id(self, id: UUID) -> Optional['Barbeiro']:
        """Get barber by ID"""
        pass
    
    @abstractmethod
    def get_available_barbers(self, barbearia_id: UUID) -> List['Barbeiro']:
        """Get available barbers for a barbershop"""
        pass
    
    @abstractmethod
    def update_status(self, barbeiro_id: UUID, status: str) -> bool:
        """Update barber status"""
        pass

class IServicoRepository(ABC):
    """Domain interface for service repository"""
    
    @abstractmethod
    def get_by_id(self, id: UUID) -> Optional['Servico']:
        """Get service by ID"""
        pass
    
    @abstractmethod
    def get_duration(self, servico_id: UUID) -> int:
        """Get service duration in minutes"""
        pass

class IClienteRepository(ABC):
    """Domain interface for client repository"""
    
    @abstractmethod
    def get_by_id(self, id: UUID) -> Optional['Cliente']:
        """Get client by ID"""
        pass
    
    @abstractmethod
    def get_priority_level(self, cliente_id: UUID) -> int:
        """Get client priority level based on loyalty"""
        pass

class IFilaRepository(ABC):
    """Domain interface for queue repository"""
    
    @abstractmethod
    def get_by_id(self, id: UUID) -> Optional['EntradaFila']:
        """Get queue entry by ID"""
        pass
    
    @abstractmethod
    def get_waiting_entries(self, barbearia_id: UUID) -> List['EntradaFila']:
        """Get all waiting entries for a barbershop"""
        pass
    
    @abstractmethod
    def get_in_service_entries(self, barbearia_id: UUID) -> List['EntradaFila']:
        """Get all entries currently being served"""
        pass
    
    @abstractmethod
    def add_entry(self, entry: 'EntradaFila') -> 'EntradaFila':
        """Add a new entry to the queue"""
        pass
    
    @abstractmethod
    def update_entry(self, entry: 'EntradaFila') -> 'EntradaFila':
        """Update an existing entry"""
        pass 