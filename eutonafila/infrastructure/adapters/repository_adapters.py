from typing import List, Optional
from uuid import UUID

from domain.interfaces.repositories import (
    IBarbeariaRepository,
    IBarbeiroRepository,
    IServicoRepository,
    IClienteRepository,
    IFilaRepository
)
from domain.entities import EntradaFila, FilaStatus
from application.interfaces import (
    IBarbeariaRepository as IAppBarbeariaRepository,
    IBarbeiroRepository as IAppBarbeiroRepository,
    IServicoRepository as IAppServicoRepository,
    IClienteRepository as IAppClienteRepository,
    IFilaRepository as IAppFilaRepository
)
from domain.entities import ClienteStatus


class DomainBarbeariaRepositoryAdapter(IBarbeariaRepository):
    """Adapter that implements domain barbershop repository interface using application repository"""
    
    def __init__(self, app_repository: IAppBarbeariaRepository):
        self.app_repository = app_repository
    
    def get_by_id(self, id: UUID) -> Optional['Barbearia']:
        """Get barbershop by ID"""
        return self.app_repository.get_by_id(id)
    
    def get_by_slug(self, slug: str) -> Optional['Barbearia']:
        """Get barbershop by slug"""
        return self.app_repository.get_by_slug(slug)
    
    def is_queue_full(self, barbearia_id: UUID) -> bool:
        """Check if barbershop queue is at max capacity"""
        barbearia = self.app_repository.get_by_id(barbearia_id)
        if not barbearia:
            return True  # If barbershop doesn't exist, consider it full
        
        # Check if the barbershop has a method to check if queue is full
        if hasattr(barbearia, 'is_queue_full'):
            return barbearia.is_queue_full()
        
        # Fallback implementation
        from barbershop.models import Fila
        waiting_count = Fila.objects.filter(
            barbearia_id=barbearia_id,
            status=FilaStatus.AGUARDANDO.value
        ).count()
        return waiting_count >= barbearia.max_capacity
    
    def is_open(self, barbearia_id: UUID) -> bool:
        """Check if barbershop is currently open"""
        barbearia = self.app_repository.get_by_id(barbearia_id)
        if not barbearia:
            return False
        
        # Check if the barbershop has a method to check if it's open
        if hasattr(barbearia, 'esta_aberto'):
            return barbearia.esta_aberto()
        
        return False  # Default to closed if method doesn't exist


class DomainBarbeiroRepositoryAdapter(IBarbeiroRepository):
    """Adapter that implements domain barber repository interface using application repository"""
    
    def __init__(self, app_repository: IAppBarbeiroRepository):
        self.app_repository = app_repository
    
    def get_by_id(self, id: UUID) -> Optional['Barbeiro']:
        """Get barber by ID"""
        return self.app_repository.get_by_id(id)
    
    def get_available_barbers(self, barbearia_id: UUID) -> List['Barbeiro']:
        """Get available barbers for a barbershop"""
        return self.app_repository.get_available(barbearia_id)
    
    def update_status(self, barbeiro_id: UUID, status: str) -> bool:
        """Update barber status"""
        return self.app_repository.update_status(barbeiro_id, status)


class DomainServicoRepositoryAdapter(IServicoRepository):
    """Adapter that implements domain service repository interface using application repository"""
    
    def __init__(self, app_repository: IAppServicoRepository):
        self.app_repository = app_repository
    
    def get_by_id(self, id: UUID) -> Optional['Servico']:
        """Get service by ID"""
        return self.app_repository.get_by_id(id)
    
    def get_duration(self, servico_id: UUID) -> int:
        """Get service duration in minutes"""
        servico = self.app_repository.get_by_id(servico_id)
        if not servico:
            return 0
        return servico.duracao


class DomainClienteRepositoryAdapter(IClienteRepository):
    """Adapter that implements domain client repository interface using application repository"""
    
    def __init__(self, app_repository: IAppClienteRepository):
        self.app_repository = app_repository
    
    def get_by_id(self, id: UUID) -> Optional['Cliente']:
        """Get client by ID"""
        return self.app_repository.get_by_id(id)
    
    def get_priority_level(self, cliente_id: UUID) -> int:
        """Get client priority level based on loyalty"""
        cliente = self.app_repository.get_by_id(cliente_id)
        if not cliente:
            return ClienteStatus.REGULAR.value
        
        # Check if the client has a loyalty_level property
        if hasattr(cliente, 'loyalty_level'):
            return cliente.loyalty_level.value
        
        # Fallback implementation
        if cliente.is_vip:
            return ClienteStatus.VIP.value
        elif cliente.total_visits >= 20:
            return ClienteStatus.GOLD.value
        elif cliente.total_visits >= 10:
            return ClienteStatus.SILVER.value
        elif cliente.total_visits >= 5:
            return ClienteStatus.BRONZE.value
        
        return ClienteStatus.REGULAR.value


class DomainFilaRepositoryAdapter(IFilaRepository):
    """Adapter that implements domain queue repository interface using application repository"""
    
    def __init__(self, app_repository: IAppFilaRepository):
        self.app_repository = app_repository
    
    def get_by_id(self, id: UUID) -> Optional[EntradaFila]:
        """Get queue entry by ID"""
        return self.app_repository.get_by_id(id)
    
    def get_waiting_entries(self, barbearia_id: UUID) -> List[EntradaFila]:
        """Get all waiting entries for a barbershop"""
        return self.app_repository.get_by_status(
            barbearia_id, 
            FilaStatus.AGUARDANDO.value
        )
    
    def get_in_service_entries(self, barbearia_id: UUID) -> List[EntradaFila]:
        """Get all entries currently being served"""
        return self.app_repository.get_by_status(
            barbearia_id,
            FilaStatus.ATENDIMENTO.value
        )
    
    def add_entry(self, entry: EntradaFila) -> EntradaFila:
        """Add a new entry to the queue"""
        return self.app_repository.adicionar_cliente(entry)
    
    def update_entry(self, entry: EntradaFila) -> EntradaFila:
        """Update an existing entry"""
        return self.app_repository.update(entry) 