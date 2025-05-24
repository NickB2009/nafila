from channels.layers import get_channel_layer
from asgiref.sync import async_to_sync
from uuid import UUID
import json

from domain.models import EntradaFila
from infrastructure.repositories import DjangoFilaRepository

def notify_queue_status_change(queue_id, status=None):
    """
    Notify WebSocket clients about a queue status change
    """
    try:
        # Get queue entry
        repository = DjangoFilaRepository()
        entrada = repository.get_by_id(UUID(str(queue_id)))
        
        if not entrada:
            return False
            
        # If status is not provided, use the current status
        if status is None:
            status = entrada.status
            
        # Get position and wait time
        position = repository.posicao_na_fila(UUID(str(queue_id)))
        
        # Format wait time
        if position <= 0:
            wait_time = "Você será atendido em seguida"
        else:
            minutes = position * entrada.servico.duracao
            if minutes < 60:
                wait_time = f"{minutes} minutos"
            else:
                hours = minutes // 60
                remaining_mins = minutes % 60
                wait_time = f"{hours}h e {remaining_mins}min"
        
        # Get channel layer
        channel_layer = get_channel_layer()
        
        # Send message to individual queue entry group
        async_to_sync(channel_layer.group_send)(
            f'queue_{queue_id}',
            {
                'type': 'queue_update',
                'status': status,
                'status_display': entrada.get_status_display(),
                'position': position,
                'wait_time': wait_time,
                'service': entrada.servico.nome,
                'barbershop': entrada.barbearia.nome,
            }
        )
        
        # Send message to barbershop group
        async_to_sync(channel_layer.group_send)(
            f'barbershop_{entrada.barbearia.slug}',
            {
                'type': 'queue_update',
                'action': 'queue_changed'
            }
        )
        
        return True
    except Exception as e:
        print(f"Error notifying queue status change: {e}")
        return False


def broadcast_queue_update(barbershop_slug):
    """
    Broadcast a queue update to all clients connected to a barbershop
    """
    try:
        channel_layer = get_channel_layer()
        
        # Send message to barbershop group
        async_to_sync(channel_layer.group_send)(
            f'barbershop_{barbershop_slug}',
            {
                'type': 'queue_update',
                'action': 'queue_changed'
            }
        )
        
        return True
    except Exception as e:
        print(f"Error broadcasting queue update: {e}")
        return False 