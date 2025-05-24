import json
from channels.generic.websocket import AsyncWebsocketConsumer
from channels.db import database_sync_to_async
from domain.models import EntradaFila, Barbearia
from uuid import UUID
from asgiref.sync import sync_to_async
from infrastructure.repositories import DjangoFilaRepository


class QueueConsumer(AsyncWebsocketConsumer):
    """
    WebSocket consumer for individual queue entries.
    Allows clients to receive real-time updates about their queue position.
    """
    async def connect(self):
        self.queue_id = self.scope['url_route']['kwargs']['queue_id']
        self.queue_group_name = f'queue_{self.queue_id}'
        
        # Join room group
        await self.channel_layer.group_add(
            self.queue_group_name,
            self.channel_name
        )
        
        # Accept the connection
        await self.accept()
        
        # Send initial status
        try:
            queue_entry = await self.get_queue_entry(self.queue_id)
            if queue_entry:
                position, wait_time = await self.get_queue_position(self.queue_id)
                await self.send_status_update(queue_entry, position, wait_time)
            else:
                await self.send_error("Queue entry not found")
        except Exception as e:
            await self.send_error(str(e))
    
    async def disconnect(self, close_code):
        # Leave room group
        await self.channel_layer.group_discard(
            self.queue_group_name,
            self.channel_name
        )
    
    # Receive message from WebSocket
    async def receive(self, text_data):
        try:
            data = json.loads(text_data)
            action = data.get('action')
            
            if action == 'refresh':
                queue_entry = await self.get_queue_entry(self.queue_id)
                if queue_entry:
                    position, wait_time = await self.get_queue_position(self.queue_id)
                    await self.send_status_update(queue_entry, position, wait_time)
            elif action == 'cancel':
                success = await self.cancel_queue_entry(self.queue_id)
                if success:
                    await self.send(text_data=json.dumps({
                        'type': 'status_update',
                        'status': 'cancelado',
                        'message': 'Atendimento cancelado com sucesso'
                    }))
                else:
                    await self.send_error("Não foi possível cancelar o atendimento")
            
        except Exception as e:
            await self.send_error(str(e))
    
    # Receive message from room group
    async def queue_update(self, event):
        # Send message to WebSocket
        await self.send(text_data=json.dumps(event))
    
    # Helper methods
    @database_sync_to_async
    def get_queue_entry(self, queue_id):
        try:
            repository = DjangoFilaRepository()
            return repository.get_by_id(UUID(queue_id))
        except:
            return None
    
    @database_sync_to_async
    def get_queue_position(self, queue_id):
        try:
            repository = DjangoFilaRepository()
            entry = repository.get_by_id(UUID(queue_id))
            if not entry:
                return 0, "Queue entry not found"
                
            position = repository.posicao_na_fila(UUID(queue_id))
            
            # Format wait time
            if position <= 0:
                wait_time = "Você será atendido em seguida"
            else:
                minutes = position * entry.servico.duracao
                if minutes < 60:
                    wait_time = f"{minutes} minutos"
                else:
                    hours = minutes // 60
                    remaining_mins = minutes % 60
                    wait_time = f"{hours}h e {remaining_mins}min"
            
            return position, wait_time
        except Exception as e:
            return 0, str(e)
    
    @database_sync_to_async
    def cancel_queue_entry(self, queue_id):
        try:
            repository = DjangoFilaRepository()
            entry = repository.get_by_id(UUID(queue_id))
            if entry and entry.status == 'aguardando':
                entry.cancelar_atendimento()
                return True
            return False
        except:
            return False
    
    async def send_status_update(self, queue_entry, position, wait_time):
        await self.send(text_data=json.dumps({
            'type': 'status_update',
            'status': queue_entry.status,
            'status_display': queue_entry.get_status_display(),
            'position': position,
            'wait_time': wait_time,
            'service': queue_entry.servico.nome,
            'barbershop': queue_entry.barbearia.nome,
        }))
    
    async def send_error(self, message):
        await self.send(text_data=json.dumps({
            'type': 'error',
            'message': message
        }))


class BarbershopQueueConsumer(AsyncWebsocketConsumer):
    """
    WebSocket consumer for barbershop queues.
    Allows barbershop staff to see real-time updates of the queue.
    """
    async def connect(self):
        self.slug = self.scope['url_route']['kwargs']['slug']
        self.barbershop_group_name = f'barbershop_{self.slug}'
        
        # Join barbershop group
        await self.channel_layer.group_add(
            self.barbershop_group_name,
            self.channel_name
        )
        
        # Accept the connection
        await self.accept()
        
        # Send initial queue data
        await self.send_queue_data()
    
    async def disconnect(self, close_code):
        # Leave group
        await self.channel_layer.group_discard(
            self.barbershop_group_name,
            self.channel_name
        )
    
    # Receive message from WebSocket
    async def receive(self, text_data):
        try:
            data = json.loads(text_data)
            action = data.get('action')
            
            if action == 'refresh':
                await self.send_queue_data()
            
        except Exception as e:
            await self.send_error(str(e))
    
    # Receive message from barbershop group
    async def queue_update(self, event):
        # Send message to WebSocket
        await self.send(text_data=json.dumps(event))
    
    # Helper methods
    @database_sync_to_async
    def get_barbershop(self, slug):
        try:
            return Barbearia.objects.get(slug=slug)
        except:
            return None
    
    @database_sync_to_async
    def get_queue_entries(self, barbershop):
        try:
            # Get active entries for this barbershop
            entries = list(EntradaFila.objects.filter(
                barbearia=barbershop,
                status='aguardando'
            ).select_related('cliente', 'servico').order_by('created_at'))
            
            return [{
                'id': str(entry.id),
                'cliente': entry.cliente.nome,
                'telefone': entry.cliente.telefone,
                'servico': entry.servico.nome,
                'duracao': entry.servico.duracao,
                'hora_entrada': entry.created_at.strftime('%H:%M'),
                'position': idx + 1
            } for idx, entry in enumerate(entries)]
        except:
            return []
    
    async def send_queue_data(self):
        barbershop = await self.get_barbershop(self.slug)
        if not barbershop:
            await self.send_error("Barbershop not found")
            return
            
        queue_entries = await self.get_queue_entries(barbershop)
        
        await self.send(text_data=json.dumps({
            'type': 'queue_data',
            'barbershop': barbershop.nome,
            'queue': queue_entries,
            'queue_length': len(queue_entries)
        }))
    
    async def send_error(self, message):
        await self.send(text_data=json.dumps({
            'type': 'error',
            'message': message
        }))


class QueueUpdatesConsumer(AsyncWebsocketConsumer):
    """
    Consumer for handling real-time queue updates via WebSockets
    """
    async def connect(self):
        self.queue_id = self.scope['url_route']['kwargs']['queue_id']
        self.queue_group_name = f'queue_{self.queue_id}'

        # Join queue group
        await self.channel_layer.group_add(
            self.queue_group_name,
            self.channel_name
        )

        await self.accept()

    async def disconnect(self, close_code):
        # Leave queue group
        await self.channel_layer.group_discard(
            self.queue_group_name,
            self.channel_name
        )

    # Receive message from WebSocket
    async def receive(self, text_data):
        text_data_json = json.loads(text_data)
        message = text_data_json.get('message', '')
        
        # Send message to queue group
        await self.channel_layer.group_send(
            self.queue_group_name,
            {
                'type': 'queue_update',
                'message': message
            }
        )

    # Receive message from queue group
    async def queue_update(self, event):
        message = event['message']

        # Send message to WebSocket
        await self.send(text_data=json.dumps({
            'message': message
        })) 