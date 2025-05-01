from typing import List, Optional
from uuid import UUID
from django.db.models import Count, F, Q, Min, Sum, Avg
from django.utils import timezone

from domain.models import Cliente, Barbearia, Servico, Barbeiro, PageSection, PageLayout, BarbeariaCustomPage
from domain.domain_models import EntradaFila, WaitTimeCalculator
from application.interfaces import (
    IClienteRepository, 
    IFilaRepository, 
    IBarbeariaRepository, 
    IServicoRepository,
    IBarbeiroRepository,
    IPageSectionRepository,
    IPageLayoutRepository,
    ICustomPageRepository
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
        from barbershop.models import Fila  # Import here to avoid circular imports
        
        return list(Fila.objects.filter(
            barbearia_id=barbearia_id,
            status=EntradaFila.Status.STATUS_AGUARDANDO.value
        ).select_related('cliente', 'servico').order_by('created_at'))
    
    def posicao_na_fila(self, entrada_id: UUID) -> int:
        """Get position in queue for a specific entry"""
        from barbershop.models import Fila  # Import here to avoid circular imports
        
        try:
            entrada = Fila.objects.get(id=entrada_id)
            if entrada.status != EntradaFila.Status.STATUS_AGUARDANDO.value:
                return 0
                
            return entrada.get_position()
        except Fila.DoesNotExist:
            return 0
            
    def get_by_id(self, id: UUID) -> Optional[EntradaFila]:
        """Get queue entry by ID"""
        from barbershop.models import Fila  # Import here to avoid circular imports
        
        try:
            return Fila.objects.select_related(
                'cliente', 'servico', 'barbearia', 'barbeiro'
            ).get(id=id)
        except Fila.DoesNotExist:
            return None
    
    def update(self, entrada: EntradaFila) -> EntradaFila:
        """Update a queue entry"""
        entrada.save()
        return entrada
    
    def get_next_cliente(self, barbearia_id: UUID) -> Optional[EntradaFila]:
        """Get the next client in line."""
        from barbershop.models import Fila  # Import here to avoid circular imports
        
        return Fila.objects.filter(
            barbearia_id=barbearia_id,
            status=EntradaFila.Status.STATUS_AGUARDANDO.value
        ).order_by('created_at').first()
        
    def get_next_in_line(self, barbearia_id: UUID) -> Optional[EntradaFila]:
        """Get the next client in line for a barbershop"""
        # Reuse the existing get_next_cliente method
        return self.get_next_cliente(barbearia_id)
    
    def get_current_by_barbeiro(self, barbeiro_id: UUID) -> Optional[EntradaFila]:
        """Get the current client being served by a barber"""
        from barbershop.models import Fila  # Import here to avoid circular imports
        
        try:
            return Fila.objects.filter(
                barbeiro_id=barbeiro_id,
                status=EntradaFila.Status.STATUS_ATENDIMENTO.value
            ).order_by('-horario_atendimento').first()
        except Fila.DoesNotExist:
            return None
    
    def get_estimated_wait_time(self, barbearia_id: UUID) -> int:
        """Get the estimated wait time for a barbershop in minutes"""
        # Import here to avoid circular imports
        from barbershop.models import Barbearia, Fila, Barbeiro
        
        try:
            # Try to get the barbershop first
            barbearia = Barbearia.objects.get(id=barbearia_id)
            
            # Use the barbershop's calculated wait time method which handles caching
            return barbearia.calcular_tempo_espera()
        except Barbearia.DoesNotExist:
            return 0
    
    def get_by_status(self, barbearia_id: UUID, status: str) -> List[EntradaFila]:
        """Get queue entries by status for a barbershop"""
        from barbershop.models import Fila  # Import here to avoid circular imports
        
        return list(Fila.objects.filter(
            barbearia_id=barbearia_id,
            status=status
        ).select_related('cliente', 'servico', 'barbeiro'))
    
    def get_recent_completed(self, barbearia_id: UUID, limit: int = 5) -> List[EntradaFila]:
        """Get recently completed services for a barbershop"""
        from barbershop.models import Fila  # Import here to avoid circular imports
        
        return list(Fila.objects.filter(
            barbearia_id=barbearia_id,
            status=EntradaFila.Status.STATUS_FINALIZADO.value
        ).select_related('cliente', 'servico', 'barbeiro').order_by('-horario_finalizacao')[:limit])
    
    def get_all_by_cliente(self, cliente_id: UUID) -> List[EntradaFila]:
        """Get all queue entries for a client"""
        from barbershop.models import Fila  # Import here to avoid circular imports
        
        return list(Fila.objects.filter(
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
            # Ensure we have a proper UUID object to avoid conversion issues
            if isinstance(id, str):
                from uuid import UUID
                id = UUID(id)
                
            print(f"DEBUG - Looking up service with ID: {id} of type {type(id)}")
            return Servico.objects.select_related('barbearia').get(id=id)
        except (Servico.DoesNotExist, ValueError, TypeError) as e:
            print(f"DEBUG - Service lookup failed: {str(e)}")
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

class DjangoPageSectionRepository(IPageSectionRepository):
    """Repository implementation for PageSection using Django ORM"""
    
    def get_all(self) -> List[PageSection]:
        """Get all page sections"""
        return list(PageSection.objects.all())
    
    def get_by_id(self, id: int) -> Optional[PageSection]:
        """Get section by ID"""
        try:
            return PageSection.objects.get(id=id)
        except PageSection.DoesNotExist:
            return None
    
    def get_by_type(self, section_type: str) -> List[PageSection]:
        """Get sections by type"""
        return list(PageSection.objects.filter(section_type=section_type))
    
    def get_required_sections(self) -> List[PageSection]:
        """Get required sections"""
        return list(PageSection.objects.filter(is_required=True))


class DjangoPageLayoutRepository(IPageLayoutRepository):
    """Repository implementation for PageLayout using Django ORM"""
    
    def get_all(self) -> List[PageLayout]:
        """Get all page layouts"""
        return list(PageLayout.objects.all())
    
    def get_by_id(self, id: int) -> Optional[PageLayout]:
        """Get layout by ID"""
        try:
            return PageLayout.objects.get(id=id)
        except PageLayout.DoesNotExist:
            return None
    
    def get_active_layouts(self) -> List[PageLayout]:
        """Get active layouts"""
        return list(PageLayout.objects.filter(is_active=True))


class DjangoCustomPageRepository(ICustomPageRepository):
    """Repository implementation for BarbeariaCustomPage using Django ORM"""
    
    def get_by_barbearia(self, barbearia_id: UUID) -> Optional[BarbeariaCustomPage]:
        """Get custom page by barbershop ID"""
        try:
            return BarbeariaCustomPage.objects.get(barbearia_id=barbearia_id)
        except BarbeariaCustomPage.DoesNotExist:
            return None
    
    def get_by_id(self, id: int) -> Optional[BarbeariaCustomPage]:
        """Get custom page by ID"""
        try:
            return BarbeariaCustomPage.objects.get(id=id)
        except BarbeariaCustomPage.DoesNotExist:
            return None
    
    def save(self, page: BarbeariaCustomPage) -> BarbeariaCustomPage:
        """Save custom page"""
        page.save()
        return page 