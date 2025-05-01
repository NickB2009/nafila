from typing import List, Optional
from uuid import UUID
from django.db.models import Count, F, Q, Min, Sum, Avg
from django.utils import timezone

from domain.models import Cliente, Barbearia, Servico, EntradaFila, Barbeiro
from application.interfaces import (
    IClienteRepository, 
    IFilaRepository, 
    IBarbeariaRepository, 
    IServicoRepository,
    IBarbeiroRepository
)
from application.dtos import ClienteDTO

class DjangoClienteRepository(IClienteRepository):
    """Repository implementation for Cliente using Django ORM"""
    
    def get_by_telefone(self, telefone: str) -> Optional[Cliente]:
        """Get cliente by phone number"""
        try:
            return Cliente.objects.get(telefone=telefone)
        except Cliente.DoesNotExist:
            return None
    
    def criar(self, cliente_dto: ClienteDTO) -> Cliente:
        """Create a new cliente"""
        cliente = Cliente(
            nome=cliente_dto.nome,
            telefone=cliente_dto.telefone,
            email=cliente_dto.email
        )
        cliente.save()
        return cliente
    
    def get_by_id(self, id: UUID) -> Optional[Cliente]:
        """Get a client by ID"""
        try:
            return Cliente.objects.get(id=id)
        except Cliente.DoesNotExist:
            return None
            
    def update(self, cliente: Cliente) -> Cliente:
        """Update a client"""
        cliente.save()
        return cliente
        
    def get_vips(self, barbearia_id: UUID) -> List[Cliente]:
        """Get VIP clients for a barbershop"""
        # Get clients who have VIP status or high loyalty
        vips = Cliente.objects.filter(
            is_vip=True
        )
        
        # Get clients with high visit count at this specific barbershop
        loyal_clients = Cliente.objects.filter(
            entradafila__barbearia_id=barbearia_id,
            entradafila__status=EntradaFila.STATUS_FINALIZADO
        ).annotate(
            visit_count=Count('entradafila')
        ).filter(
            visit_count__gte=10
        )
        
        # Combine the two querysets and remove duplicates
        return list(set(list(vips) + list(loyal_clients)))

class DjangoFilaRepository(IFilaRepository):
    """Repository implementation for Fila operations using Django ORM"""
    
    def adicionar_cliente(self, entrada: EntradaFila) -> EntradaFila:
        """Add client to queue"""
        entrada.save()
        return entrada
    
    def clientes_aguardando(self, barbearia_id: UUID) -> List[EntradaFila]:
        """Get all waiting clients for a barbershop."""
        return list(EntradaFila.objects.filter(
            barbearia_id=barbearia_id,
            status=EntradaFila.STATUS_AGUARDANDO
        ).select_related('cliente', 'servico').order_by('created_at'))
    
    def posicao_na_fila(self, entrada_id: UUID) -> int:
        """Get position in queue for a specific entry"""
        try:
            entrada = EntradaFila.objects.get(id=entrada_id)
            if entrada.status != EntradaFila.STATUS_AGUARDANDO:
                return 0
                
            # Use the new priority-based position method if barbershop has priorities enabled
            if entrada.barbearia.enable_priority_queue:
                return entrada.get_priority_position()
            else:
                return entrada.get_position()
        except EntradaFila.DoesNotExist:
            return 0
            
    def get_by_id(self, id: UUID) -> Optional[EntradaFila]:
        """Get queue entry by ID"""
        try:
            return EntradaFila.objects.select_related(
                'cliente', 'servico', 'barbearia', 'barbeiro'
            ).get(id=id)
        except EntradaFila.DoesNotExist:
            return None
    
    def update(self, entrada: EntradaFila) -> EntradaFila:
        """Update a queue entry"""
        entrada.save()
        return entrada
    
    def get_next_in_line(self, barbearia_id: UUID) -> Optional[EntradaFila]:
        """Get the next client in line."""
        return EntradaFila.objects.filter(
            barbearia_id=barbearia_id,
            status=EntradaFila.STATUS_AGUARDANDO
        ).order_by('created_at').first()
    
    def get_current_by_barbeiro(self, barbeiro_id: UUID) -> Optional[EntradaFila]:
        """Get the current client being served by a barber"""
        try:
            return EntradaFila.objects.filter(
                barbeiro_id=barbeiro_id,
                status=EntradaFila.STATUS_ATENDIMENTO
            ).order_by('-horario_atendimento').first()
        except EntradaFila.DoesNotExist:
            return None
    
    def get_estimated_wait_time(self, barbearia_id: UUID) -> int:
        """Get the estimated wait time for a barbershop in minutes"""
        # Get all waiting entries with related service data in a single query
        waiting_entries = EntradaFila.objects.filter(
            barbearia_id=barbearia_id,
            status=EntradaFila.STATUS_AGUARDANDO
        ).select_related('servico')
        
        if not waiting_entries:
            return 0
            
        # Get the count and average duration in a single query
        stats = EntradaFila.objects.filter(
            barbearia_id=barbearia_id,
            status=EntradaFila.STATUS_AGUARDANDO
        ).aggregate(
            count=Count('id'),
            avg_duration=Avg(F('servico__duracao'))
        )
        
        # Get the count of active barbers
        active_barbers_count = Barbeiro.objects.filter(
            barbearia_id=barbearia_id,
            status__in=[Barbeiro.STATUS_AVAILABLE, Barbeiro.STATUS_BUSY]
        ).count()
        
        if active_barbers_count == 0:
            active_barbers_count = 1  # Avoid division by zero
        
        # Calculate total service time using the aggregation results
        if stats['count'] == 0 or stats['avg_duration'] is None:
            return 0
            
        total_service_time = stats['count'] * stats['avg_duration']
        
        # Distribute workload among barbers
        return int(total_service_time // active_barbers_count)
    
    def get_by_status(self, barbearia_id: UUID, status: str) -> List[EntradaFila]:
        """Get queue entries by status for a barbershop"""
        return list(EntradaFila.objects.filter(
            barbearia_id=barbearia_id,
            status=status
        ).select_related('cliente', 'servico', 'barbeiro'))
    
    def get_recent_completed(self, barbearia_id: UUID, limit: int = 5) -> List[EntradaFila]:
        """Get recently completed services for a barbershop"""
        return list(EntradaFila.objects.filter(
            barbearia_id=barbearia_id,
            status=EntradaFila.STATUS_FINALIZADO
        ).select_related('cliente', 'servico', 'barbeiro').order_by('-horario_finalizacao')[:limit])
    
    def get_all_by_cliente(self, cliente_id: UUID) -> List[EntradaFila]:
        """Get all queue entries for a client"""
        return list(EntradaFila.objects.filter(
            cliente_id=cliente_id
        ).select_related('barbearia', 'servico', 'barbeiro').order_by('-created_at'))

class DjangoBarbeariaRepository(IBarbeariaRepository):
    """Repository implementation for Barbearia using Django ORM"""
    
    def get_by_slug(self, slug: str) -> Optional[Barbearia]:
        """Get barbershop by slug"""
        try:
            return Barbearia.objects.get(slug=slug)
        except Barbearia.DoesNotExist:
            return None
    
    def get_by_id(self, id: UUID) -> Optional[Barbearia]:
        """Get barbershop by ID"""
        try:
            return Barbearia.objects.get(id=id)
        except Barbearia.DoesNotExist:
            return None
    
    def update(self, barbearia: Barbearia) -> Barbearia:
        """Update a barbershop"""
        barbearia.save()
        return barbearia
    
    def get_all(self) -> List[Barbearia]:
        """Get all barbershops"""
        return list(Barbearia.objects.all())

class DjangoServicoRepository(IServicoRepository):
    """Repository implementation for Servico using Django ORM"""
    
    def get_by_id(self, id: UUID) -> Optional[Servico]:
        """Get service by ID"""
        try:
            return Servico.objects.select_related('barbearia').get(id=id)
        except Servico.DoesNotExist:
            return None
    
    def get_by_barbearia(self, barbearia_id: UUID) -> List[Servico]:
        """Get all services for a barbershop"""
        return list(Servico.objects.filter(barbearia_id=barbearia_id).order_by('nome'))
    
    def update(self, servico: Servico) -> Servico:
        """Update a service"""
        servico.save()
        return servico

class DjangoBarbeiroRepository(IBarbeiroRepository):
    """Repository implementation for Barbeiro using Django ORM"""
    
    def get_by_id(self, id: UUID) -> Optional[Barbeiro]:
        """Get barber by ID"""
        try:
            return Barbeiro.objects.select_related('barbearia').get(id=id)
        except Barbeiro.DoesNotExist:
            return None
    
    def get_by_barbearia(self, barbearia_id: UUID) -> List[Barbeiro]:
        """Get all barbers for a barbershop"""
        return list(Barbeiro.objects.filter(barbearia_id=barbearia_id))
    
    def get_available(self, barbearia_id: UUID) -> List[Barbeiro]:
        """Get available barbers for a barbershop"""
        return list(Barbeiro.objects.filter(
            barbearia_id=barbearia_id,
            status=Barbeiro.STATUS_AVAILABLE
        ))
    
    def update(self, barbeiro: Barbeiro) -> Barbeiro:
        """Update a barber"""
        barbeiro.save()
        return barbeiro
    
    def update_status(self, barbeiro_id: UUID, status: str) -> bool:
        """Update barber status"""
        try:
            barbeiro = Barbeiro.objects.get(id=barbeiro_id)
            return barbeiro.set_status(status)
        except Barbeiro.DoesNotExist:
            return False 