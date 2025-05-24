"""
WebSocket notification tasks in the application layer.
These tasks handle sending real-time updates to clients.
"""

from typing import Dict, Any, Optional
from datetime import datetime
import json
import asyncio
from channels.layers import get_channel_layer
from asgiref.sync import async_to_sync
from infrastructure.messaging.celery_app import app
from domain.models import EntradaFila
from infrastructure.repositories import DjangoFilaRepository


@app.task(name='application.tasks.websocket_tasks.notify_queue_status_update')
def notify_queue_status_update(queue_id: str, barbearia_slug: str) -> Dict[str, Any]:
    """
    Send a notification about queue status update to connected clients.
    
    Args:
        queue_id: ID of the queue entry
        barbearia_slug: Slug of the barbershop
    
    Returns:
        Dictionary with status information
    """
    # Get queue entry data
    fila_repository = DjangoFilaRepository()
    entrada = fila_repository.find_by_id(queue_id)
    
    if not entrada:
        return {
            'status': 'error',
            'message': f'Queue entry {queue_id} not found',
            'timestamp': datetime.now().isoformat()
        }
    
    # Prepare notification data
    status_data = {
        'type': 'status_update',
        'status': entrada.status,
        'position': entrada.posicao,
        'wait_time': entrada.tempo_espera_estimado,
        'timestamp': datetime.now().isoformat()
    }
    
    # Get channel layer for WebSocket communication
    channel_layer = get_channel_layer()
    
    # Send to individual client's channel group
    async_to_sync(channel_layer.group_send)(
        f'queue_{queue_id}',
        {
            'type': 'send_status_update',
            'message': status_data
        }
    )
    
    # Send to barbershop's channel group
    async_to_sync(channel_layer.group_send)(
        f'barbershop_{barbearia_slug}',
        {
            'type': 'send_queue_update',
            'message': {
                'queue_id': queue_id,
                'status': entrada.status,
                'client_name': entrada.cliente.nome,
                'service_name': entrada.servico.nome,
                'position': entrada.posicao,
                'wait_time': entrada.tempo_espera_estimado,
                'timestamp': datetime.now().isoformat()
            }
        }
    )
    
    return {
        'status': 'success',
        'queue_id': queue_id,
        'notifications_sent': 2,  # One to client, one to barbershop
        'timestamp': datetime.now().isoformat()
    }


@app.task(name='application.tasks.websocket_tasks.send_sms_notification')
def send_sms_notification(phone_number: str, message: str) -> Dict[str, Any]:
    """
    Send an SMS notification.
    
    Args:
        phone_number: The phone number to send to
        message: The message content
    
    Returns:
        Dictionary with status information
    """
    # This would integrate with an SMS provider
    # For now, just log the message
    print(f"SMS to {phone_number}: {message}")
    
    # In a real implementation, you would call an SMS gateway here
    
    return {
        'status': 'success',
        'phone': phone_number,
        'timestamp': datetime.now().isoformat()
    } 