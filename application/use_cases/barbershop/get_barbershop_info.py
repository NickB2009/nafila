from typing import Optional
from uuid import UUID

# Import repository interface from domain layer
from domain.repositories.barbearia_repository import IBarbeariaRepository

# Use case output DTO
class BarbershopInfoDTO:
    def __init__(
        self,
        id: UUID,
        name: str,
        slug: str,
        phone: Optional[str],
        address: Optional[str],
        description: Optional[str],
        colors: list,
        logo_url: Optional[str],
        opening_time: str,
        closing_time: str,
        working_days: list,
        max_capacity: int,
        is_open: bool,
        estimated_wait_time: int
    ):
        self.id = id
        self.name = name
        self.slug = slug
        self.phone = phone
        self.address = address
        self.description = description
        self.colors = colors
        self.logo_url = logo_url
        self.opening_time = opening_time
        self.closing_time = closing_time
        self.working_days = working_days
        self.max_capacity = max_capacity
        self.is_open = is_open
        self.estimated_wait_time = estimated_wait_time

class GetBarbershopInfoUseCase:
    """
    Use case for retrieving barbershop information.
    This is a simple query use case that retrieves information about a barbershop.
    """
    
    def __init__(self, barbearia_repository: IBarbeariaRepository):
        """Initialize with required repositories"""
        self.barbearia_repository = barbearia_repository
    
    def execute_by_id(self, barbershop_id: UUID) -> Optional[BarbershopInfoDTO]:
        """Execute the use case by ID"""
        barbershop = self.barbearia_repository.get_by_id(barbershop_id)
        if not barbershop:
            return None
        
        return self._to_dto(barbershop)
    
    def execute_by_slug(self, slug: str) -> Optional[BarbershopInfoDTO]:
        """Execute the use case by slug"""
        barbershop = self.barbearia_repository.get_by_slug(slug)
        if not barbershop:
            return None
        
        return self._to_dto(barbershop)
    
    def _to_dto(self, barbershop) -> BarbershopInfoDTO:
        """Convert domain entity to DTO"""
        # In a real implementation, this would convert from domain entity
        # For now, assuming the barbershop is a Django model
        logo_url = barbershop.logo.url if barbershop.logo else None
        
        return BarbershopInfoDTO(
            id=barbershop.id,
            name=barbershop.nome,
            slug=barbershop.slug,
            phone=barbershop.telefone,
            address=barbershop.endereco,
            description=barbershop.descricao_curta,
            colors=barbershop.cores,
            logo_url=logo_url,
            opening_time=barbershop.horario_abertura.strftime('%H:%M'),
            closing_time=barbershop.horario_fechamento.strftime('%H:%M'),
            working_days=barbershop.dias_funcionamento,
            max_capacity=barbershop.max_capacity,
            is_open=barbershop.esta_aberto(),
            estimated_wait_time=barbershop.calcular_tempo_espera()
        ) 