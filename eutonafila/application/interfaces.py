from abc import ABC, abstractmethod
from uuid import UUID
from typing import List, Optional, Tuple, Dict, Any

from domain.models import Cliente, Barbearia, Servico, Barbeiro, PageSection, PageLayout, BarbeariaCustomPage
from domain.domain_models import EntradaFila
from .dtos import ClienteDTO

class IClienteRepository(ABC):
    """Interface for client repository operations"""
    
    @abstractmethod
    def get_by_telefone(self, telefone: str) -> Optional[Cliente]:
        """Get a client by phone number"""
        pass
        
    @abstractmethod
    def criar(self, cliente_dto: ClienteDTO) -> Cliente:
        """Create a new client"""
        pass
    
    @abstractmethod
    def get_by_id(self, id: UUID) -> Optional[Cliente]:
        """Get a client by ID"""
        pass
    
    @abstractmethod
    def update(self, cliente: Cliente) -> Cliente:
        """Update a client"""
        pass
    
    @abstractmethod
    def get_vips(self, barbearia_id: UUID) -> List[Cliente]:
        """Get VIP clients for a barbershop"""
        pass

class IFilaRepository(ABC):
    """Interface for queue repository operations"""
    
    @abstractmethod
    def adicionar_cliente(self, entrada: EntradaFila) -> EntradaFila:
        """Add client to queue"""
        pass
        
    @abstractmethod
    def clientes_aguardando(self, barbearia_id: UUID) -> List[EntradaFila]:
        """Get list of waiting clients for a barbershop"""
        pass
        
    @abstractmethod
    def posicao_na_fila(self, entrada_id: UUID) -> int:
        """Get position in queue for a specific entry"""
        pass
    
    @abstractmethod
    def get_by_id(self, id: UUID) -> Optional[EntradaFila]:
        """Get queue entry by ID"""
        pass
    
    @abstractmethod
    def update(self, entrada: EntradaFila) -> EntradaFila:
        """Update a queue entry"""
        pass
    
    @abstractmethod
    def get_next_in_line(self, barbearia_id: UUID) -> Optional[EntradaFila]:
        """Get the next client in line for a barbershop"""
        pass
    
    @abstractmethod
    def get_current_by_barbeiro(self, barbeiro_id: UUID) -> Optional[EntradaFila]:
        """Get the current client being served by a barber"""
        pass
    
    @abstractmethod
    def get_estimated_wait_time(self, barbearia_id: UUID) -> int:
        """Get the estimated wait time for a barbershop in minutes"""
        pass
    
    @abstractmethod
    def get_by_status(self, barbearia_id: UUID, status: str) -> List[EntradaFila]:
        """Get queue entries by status for a barbershop"""
        pass
    
    @abstractmethod
    def get_recent_completed(self, barbearia_id: UUID, limit: int = 5) -> List[EntradaFila]:
        """Get recently completed services for a barbershop"""
        pass
    
    @abstractmethod
    def get_all_by_cliente(self, cliente_id: UUID) -> List[EntradaFila]:
        """Get all queue entries for a client"""
        pass

class IBarbeariaRepository(ABC):
    """Interface for barbershop repository operations"""
    
    @abstractmethod
    def get_by_slug(self, slug: str) -> Optional[Barbearia]:
        """Get barbershop by slug"""
        pass
    
    @abstractmethod
    def get_by_id(self, id: UUID) -> Optional[Barbearia]:
        """Get barbershop by ID"""
        pass
    
    @abstractmethod
    def update(self, barbearia: Barbearia) -> Barbearia:
        """Update a barbershop"""
        pass
    
    @abstractmethod
    def get_all(self) -> List[Barbearia]:
        """Get all barbershops"""
        pass
        
class IServicoRepository(ABC):
    """Interface for service repository operations"""
    
    @abstractmethod
    def get_by_id(self, id: UUID) -> Optional[Servico]:
        """Get service by ID"""
        pass
    
    @abstractmethod
    def get_by_barbearia(self, barbearia_id: UUID) -> List[Servico]:
        """Get all services for a barbershop"""
        pass
    
    @abstractmethod
    def update(self, servico: Servico) -> Servico:
        """Update a service"""
        pass

class IBarbeiroRepository(ABC):
    """Interface for barber repository operations"""
    
    @abstractmethod
    def get_by_id(self, id: UUID) -> Optional[Barbeiro]:
        """Get barber by ID"""
        pass
    
    @abstractmethod
    def get_by_barbearia(self, barbearia_id: UUID) -> List[Barbeiro]:
        """Get all barbers for a barbershop"""
        pass
    
    @abstractmethod
    def get_available(self, barbearia_id: UUID) -> List[Barbeiro]:
        """Get available barbers for a barbershop"""
        pass
    
    @abstractmethod
    def update(self, barbeiro: Barbeiro) -> Barbeiro:
        """Update a barber"""
        pass
    
    @abstractmethod
    def update_status(self, barbeiro_id: UUID, status: str) -> bool:
        """Update barber status"""
        pass

class IPageSectionRepository(ABC):
    @abstractmethod
    def get_all(self) -> List[PageSection]:
        """Get all page sections"""
        pass
    
    @abstractmethod
    def get_by_id(self, id: int) -> Optional[PageSection]:
        """Get section by ID"""
        pass
    
    @abstractmethod
    def get_by_type(self, section_type: str) -> List[PageSection]:
        """Get sections by type"""
        pass
    
    @abstractmethod
    def get_required_sections(self) -> List[PageSection]:
        """Get required sections"""
        pass


class IPageLayoutRepository(ABC):
    @abstractmethod
    def get_all(self) -> List[PageLayout]:
        """Get all page layouts"""
        pass
    
    @abstractmethod
    def get_by_id(self, id: int) -> Optional[PageLayout]:
        """Get layout by ID"""
        pass
    
    @abstractmethod
    def get_active_layouts(self) -> List[PageLayout]:
        """Get active layouts"""
        pass
    
    
class ICustomPageRepository(ABC):
    @abstractmethod
    def get_by_barbearia(self, barbearia_id: UUID) -> Optional[BarbeariaCustomPage]:
        """Get custom page by barbershop ID"""
        pass
    
    @abstractmethod
    def get_by_id(self, id: int) -> Optional[BarbeariaCustomPage]:
        """Get custom page by ID"""
        pass
    
    @abstractmethod
    def save(self, page: BarbeariaCustomPage) -> BarbeariaCustomPage:
        """Save custom page"""
        pass

class ICacheService(ABC):
    """Interface for caching operations"""
    
    @abstractmethod
    def get(self, key: str) -> Any:
        """Get a value from cache"""
        pass
        
    @abstractmethod
    def set(self, key: str, value: Any, timeout: int = None) -> None:
        """Set a value in cache with optional timeout"""
        pass
        
    @abstractmethod
    def delete(self, key: str) -> None:
        """Delete a value from cache"""
        pass
        
    @abstractmethod
    def clear(self) -> None:
        """Clear all cache"""
        pass 