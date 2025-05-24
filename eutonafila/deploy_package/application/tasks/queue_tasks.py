"""
Queue management tasks in the application layer.
These define the business operations that will be executed asynchronously.
"""

from typing import Dict, Any
from datetime import datetime
from application.services import FilaService
from infrastructure.messaging.celery_app import app
from domain.models import EntradaFila
from infrastructure.repositories import (
    DjangoClienteRepository,
    DjangoFilaRepository,
    DjangoBarbeariaRepository,
    DjangoServicoRepository
)
from .websocket_tasks import notify_queue_status_update


@app.task(name='application.tasks.queue_tasks.process_checkin')
def process_checkin(checkin_data: Dict[str, Any]) -> Dict[str, Any]:
    """
    Process a check-in request asynchronously.
    
    Args:
        checkin_data: Dictionary containing check-in information
    
    Returns:
        Dictionary with status information
    """
    # Initialize repositories
    cliente_repository = DjangoClienteRepository()
    fila_repository = DjangoFilaRepository()
    barbearia_repository = DjangoBarbeariaRepository()
    servico_repository = DjangoServicoRepository()
    
    # Create the FilaService with dependencies injected
    fila_service = FilaService(
        cliente_repository=cliente_repository,
        fila_repository=fila_repository,
        barbearia_repository=barbearia_repository,
        servico_repository=servico_repository
    )
    
    # Call the domain service to process the check-in
    entrada_fila, result = fila_service.check_in_from_dict(checkin_data)
    
    if entrada_fila:
        # Send notification about queue update via websocket
        # This uses another async task to broadcast the update
        notify_queue_status_update.delay(
            barbearia_slug=checkin_data.get('barbearia_slug'),
            queue_id=str(entrada_fila.id)
        )
        
        return {
            'status': 'success',
            'queue_id': str(entrada_fila.id),
            'position': entrada_fila.posicao,
            'timestamp': datetime.now().isoformat()
        }
    else:
        return {
            'status': 'error',
            'message': result,
            'timestamp': datetime.now().isoformat()
        }


@app.task(name='application.tasks.queue_tasks.update_queue_positions')
def update_queue_positions(barbearia_slug: str) -> Dict[str, Any]:
    """
    Update positions of entries in a barbershop queue.
    
    Args:
        barbearia_slug: The slug of the barbershop
    
    Returns:
        Dictionary with status information
    """
    fila_repository = DjangoFilaRepository()
    barbearia_repository = DjangoBarbeariaRepository()
    
    # Get barbershop
    barbearia = barbearia_repository.find_by_slug(barbearia_slug)
    if not barbearia:
        return {'status': 'error', 'message': 'Barbershop not found'}
    
    # Get active queue entries
    entradas = fila_repository.find_by_barbearia_and_status(
        barbearia_id=barbearia.id,
        status=EntradaFila.Status.AGUARDANDO
    )
    
    # Update positions
    for index, entrada in enumerate(entradas, 1):
        entrada.posicao = index
        fila_repository.save(entrada)
    
    # Notify all clients in the queue about their updated positions
    for entrada in entradas:
        notify_queue_status_update.delay(
            barbearia_slug=barbearia_slug,
            queue_id=str(entrada.id)
        )
    
    return {
        'status': 'success',
        'barbershop': barbearia_slug,
        'updated_entries': len(entradas),
        'timestamp': datetime.now().isoformat()
    } 