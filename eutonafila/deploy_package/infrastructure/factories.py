from domain.services.queue_service import QueueService
from infrastructure.adapters.repository_adapters import (
    DomainBarbeariaRepositoryAdapter,
    DomainBarbeiroRepositoryAdapter,
    DomainServicoRepositoryAdapter,
    DomainClienteRepositoryAdapter,
    DomainFilaRepositoryAdapter
)
from infrastructure.repositories import (
    DjangoClienteRepository,
    DjangoFilaRepository,
    DjangoBarbeariaRepository,
    DjangoServicoRepository,
    DjangoBarbeiroRepository
)


class ServiceFactory:
    """Factory to create domain services with proper dependencies"""
    
    @staticmethod
    def create_queue_service() -> QueueService:
        """Create a fully configured QueueService with all dependencies"""
        
        # Create application-level repositories
        app_cliente_repo = DjangoClienteRepository()
        app_fila_repo = DjangoFilaRepository()
        app_barbearia_repo = DjangoBarbeariaRepository()
        app_servico_repo = DjangoServicoRepository()
        app_barbeiro_repo = DjangoBarbeiroRepository()
        
        # Create domain-level repository adapters
        domain_cliente_repo = DomainClienteRepositoryAdapter(app_cliente_repo)
        domain_fila_repo = DomainFilaRepositoryAdapter(app_fila_repo)
        domain_barbearia_repo = DomainBarbeariaRepositoryAdapter(app_barbearia_repo)
        domain_servico_repo = DomainServicoRepositoryAdapter(app_servico_repo)
        domain_barbeiro_repo = DomainBarbeiroRepositoryAdapter(app_barbeiro_repo)
        
        # Create and return the service with injected dependencies
        return QueueService(
            fila_repository=domain_fila_repo,
            barbearia_repository=domain_barbearia_repo,
            barbeiro_repository=domain_barbeiro_repo,
            servico_repository=domain_servico_repo,
            cliente_repository=domain_cliente_repo
        ) 