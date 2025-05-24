from uuid import UUID
from typing import Tuple, Dict, Any, Optional, List
from django.utils import timezone
from django.core.cache import cache
from channels.layers import get_channel_layer
from asgiref.sync import async_to_sync
import json
import datetime

from domain.models import Cliente, Barbearia, Servico
from domain.domain_models import EntradaFila, Barbeiro
from domain.services.wait_time_calculator import WaitTimeCalculator
from .interfaces import (
    IClienteRepository, 
    IFilaRepository, 
    IBarbeariaRepository, 
    IServicoRepository,
    IBarbeiroRepository,
    ICacheService
)
from .dtos import CheckInDTO, ClienteDTO
from infrastructure.cache_service import WaitTimeCache, BarbershopStatusCache

class FilaService:
    """
    Service for queue management operations.
    This class coordinates domain operations related to the queue.
    """
    
    def __init__(
        self,
        cliente_repository: IClienteRepository,
        fila_repository: IFilaRepository,
        barbearia_repository: IBarbeariaRepository,
        servico_repository: IServicoRepository,
        barbeiro_repository: IBarbeiroRepository = None,
        cache_service: ICacheService = None
    ):
        self.cliente_repository = cliente_repository
        self.fila_repository = fila_repository
        self.barbearia_repository = barbearia_repository
        self.servico_repository = servico_repository
        self.barbeiro_repository = barbeiro_repository
        
        # Create specialized cache handlers if cache service is provided
        if cache_service:
            self.wait_time_cache = WaitTimeCache(cache_service)
            self.status_cache = BarbershopStatusCache(cache_service)
        else:
            self.wait_time_cache = None
            self.status_cache = None
    
    def check_in(self, check_in_dto: CheckInDTO) -> Tuple[Optional[EntradaFila], str]:
        """
        Process a client check-in to join the queue.
        
        Args:
            check_in_dto: Data required for check-in
            
        Returns:
            A tuple containing the queue entry object (or None) and a result message
        """
        # Get or create the cliente
        cliente = self.cliente_repository.get_by_telefone(check_in_dto.cliente.telefone)
        if not cliente:
            cliente = Cliente(
                nome=check_in_dto.cliente.nome,
                telefone=check_in_dto.cliente.telefone,
                email=check_in_dto.cliente.email
            )
            cliente = self.cliente_repository.criar(cliente)
        
        # Get the barbershop
        barbearia = self.barbearia_repository.get_by_slug(check_in_dto.barbearia_slug)
        if not barbearia:
            return None, "Barbearia não encontrada."
        
        # OVERRIDE: Force shops to be open during Brazil business hours for testing
        # Remove this in production
        business_hours = (9, 18)  # 9 AM to 6 PM
        business_days = [0, 1, 2, 3, 4]  # Monday to Friday
        
        now = datetime.datetime.now()
        current_hour = now.hour
        is_business_day = now.weekday() in business_days
        is_business_hour = business_hours[0] <= current_hour < business_hours[1]
        
        is_open = is_business_day and is_business_hour
        
        # Only check if the shop is open if we're outside business hours
        if not is_open and not barbearia.esta_aberto():
            return None, "A barbearia está fechada no momento."
        
        # Check if the queue is full
        if barbearia.is_queue_full():
            return None, "A fila está cheia no momento. Por favor, tente novamente mais tarde."
        
        # Get the service
        servico = self.servico_repository.get_by_id(check_in_dto.servico_id)
        if not servico:
            return None, "Serviço não encontrado."
        
        # Check if the service belongs to the barbershop
        if servico.barbearia_id != barbearia.id:
            return None, "O serviço selecionado não pertence a esta barbearia."
            
        # Create queue entry
        entrada_fila = EntradaFila(
            cliente=cliente,
            barbearia=barbearia,
            servico=servico,
            status=EntradaFila.STATUS_AGUARDANDO
        )
        
        # Get position number (FIFO order)
        entrada_fila.position_number = EntradaFila.objects.filter(
            barbearia=barbearia,
            status=EntradaFila.STATUS_AGUARDANDO
        ).count() + 1
        
        # Set estimated duration based on service
        entrada_fila.estimativa_duracao = servico.duracao
        
        # Save to repository
        entrada_fila = self.fila_repository.adicionar_cliente(entrada_fila)
        
        # Notify other clients about queue update
        self._notify_queue_update(barbearia.slug)
        
        return entrada_fila, "Check-in realizado com sucesso."
    
    def check_in_from_dict(self, data: Dict[str, Any]) -> Tuple[Optional[EntradaFila], str]:
        """
        Process a check-in using dictionary data, convenient for async processing.
        
        Args:
            data: Dictionary with check-in data
            
        Returns:
            A tuple containing the queue entry object (or None) and a result message
        """
        # Get Barbershop first to check if it's open
        barbearia_slug = data.get('barbearia_slug')
        barbearia = self.barbearia_repository.get_by_slug(barbearia_slug)
        if not barbearia:
            return None, "Barbearia não encontrada."
        
        # OVERRIDE: Force shops to be open during Brazil business hours for testing
        # Remove this in production
        business_hours = (9, 18)  # 9 AM to 6 PM
        business_days = [0, 1, 2, 3, 4]  # Monday to Friday
        
        now = datetime.datetime.now()
        current_hour = now.hour
        is_business_day = now.weekday() in business_days
        is_business_hour = business_hours[0] <= current_hour < business_hours[1]
        
        is_open = is_business_day and is_business_hour
        
        # Only check if the shop is open if we're outside business hours
        if not is_open and not barbearia.esta_aberto():
            return None, "A barbearia está fechada no momento."
            
        # Create DTOs from dictionary data
        cliente_dto = ClienteDTO(
            nome=data.get('nome'),
            telefone=data.get('telefone'),
            email=data.get('email', '')
        )
        
        # SIMPLIFY SERVICE ID HANDLING - The web view already gives us a clean service ID
        servico_id_str = data.get('servico_id')
        print(f"DEBUG - Service ID in check_in_from_dict: {servico_id_str!r}")
        
        try:
            # Directly get the service from the repository using the provided ID
            from uuid import UUID
            
            # First try to convert the string to UUID directly
            try:
                servico_id_uuid = UUID(servico_id_str)
                servico = self.servico_repository.get_by_id(servico_id_uuid)
                if servico:
                    print(f"DEBUG - Service found directly from UUID: {servico}")
                else:
                    print(f"DEBUG - Service not found with UUID: {servico_id_uuid}")
                    return None, "Serviço não encontrado. Por favor, selecione um serviço válido."
            except (ValueError, TypeError) as e:
                print(f"DEBUG - UUID conversion failed: {str(e)}")
                return None, "ID do serviço inválido. Por favor, selecione um serviço válido."
            
            # Check if the service belongs to the barbershop
            if servico.barbearia_id != barbearia.id:
                print(f"DEBUG - Service barbershop mismatch: {servico.barbearia_id} != {barbearia.id}")
                return None, "O serviço selecionado não pertence a esta barbearia."
            
            # Create the DTO with the valid UUID
            check_in_dto = CheckInDTO(
                cliente=cliente_dto,
                barbearia_slug=barbearia_slug,
                servico_id=servico_id_uuid
            )
            
        except Exception as e:
            import traceback
            print(f"DEBUG - Exception in service ID processing: {str(e)}")
            print(f"DEBUG - Traceback: {traceback.format_exc()}")
            return None, f"Erro ao processar o serviço selecionado: {str(e)}"
            
        # Skip the shop closed check in the main check_in method
        cliente = self.cliente_repository.get_by_telefone(check_in_dto.cliente.telefone)
        if not cliente:
            cliente = Cliente(
                nome=check_in_dto.cliente.nome,
                telefone=check_in_dto.cliente.telefone,
                email=check_in_dto.cliente.email
            )
            cliente = self.cliente_repository.criar(cliente)
        
        # Check if the queue is full
        if barbearia.is_queue_full():
            return None, "A fila está cheia no momento. Por favor, tente novamente mais tarde."
        
        # Get the service again for redundancy
        servico = self.servico_repository.get_by_id(check_in_dto.servico_id)
        if not servico:
            return None, "Serviço não encontrado."
        
        # Check if the service belongs to the barbershop (redundant check)
        if servico.barbearia_id != barbearia.id:
            return None, "O serviço selecionado não pertence a esta barbearia."
            
        # Create queue entry
        entrada_fila = EntradaFila(
            cliente=cliente,
            barbearia=barbearia,
            servico=servico,
            status=EntradaFila.STATUS_AGUARDANDO
        )
        
        # Get position number (FIFO order)
        entrada_fila.position_number = EntradaFila.objects.filter(
            barbearia=barbearia,
            status=EntradaFila.STATUS_AGUARDANDO
        ).count() + 1  # Add 1 for current position
        
        # Set estimated duration based on service
        entrada_fila.estimativa_duracao = servico.duracao
        
        # Save to repository
        entrada_fila = self.fila_repository.adicionar_cliente(entrada_fila)
        
        # Notify other clients about queue update
        self._notify_queue_update(barbearia.slug)
        
        return entrada_fila, "Check-in realizado com sucesso."
    
    def get_queue_status(self, queue_id: UUID) -> Dict[str, Any]:
        """
        Get the status of a specific queue entry.
        
        Args:
            queue_id: The ID of the queue entry
            
        Returns:
            A dictionary with status information
        """
        entrada = self.fila_repository.get_by_id(queue_id)
        if not entrada:
            return {
                'success': False,
                'message': 'Entrada na fila não encontrada.'
            }
        
        # Get position and wait time
        if entrada.status == EntradaFila.STATUS_AGUARDANDO:
            position = entrada.get_position()
            wait_time = entrada.format_wait_time()
        else:
            position = 0
            wait_time = "0 minutos"
        
        # Get barber information
        barbeiro_info = None
        if entrada.barbeiro:
            barbeiro_info = {
                'nome': entrada.barbeiro.nome,
                'status': entrada.barbeiro.status,
                'status_display': entrada.barbeiro.get_status_display(),
            }
        
        return {
            'success': True,
            'id': str(entrada.id),
            'status': entrada.status,
            'status_display': entrada.get_status_display(),
            'position': position,
            'position_number': entrada.position_number,
            'estimated_wait_time': wait_time,
            'barbershop': {
                'id': str(entrada.barbearia.id),
                'nome': entrada.barbearia.nome,
                'slug': entrada.barbearia.slug
            },
            'service': {
                'id': str(entrada.servico.id),
                'nome': entrada.servico.nome,
                'duracao': entrada.servico.duracao
            },
            'client': {
                'id': str(entrada.cliente.id),
                'nome': entrada.cliente.nome,
                'telefone': entrada.cliente.telefone
            },
            'barbeiro': barbeiro_info,
            'created_at': entrada.created_at.isoformat(),
            'horario_atendimento': entrada.horario_atendimento.isoformat() if entrada.horario_atendimento else None,
            'horario_finalizacao': entrada.horario_finalizacao.isoformat() if entrada.horario_finalizacao else None,
            'prioridade': entrada.prioridade,
            'prioridade_display': entrada.get_prioridade_display(),
            'time_in_queue': entrada.time_in_queue
        }
    
    def cancel_queue_entry(self, queue_id: UUID) -> Tuple[bool, str]:
        """
        Cancel a queue entry.
        
        Args:
            queue_id: The ID of the queue entry to cancel
            
        Returns:
            A tuple with success status and message
        """
        entrada = self.fila_repository.get_by_id(queue_id)
        if not entrada:
            return False, "Entrada na fila não encontrada."
        
        # If already cancelled or finished, return error
        if entrada.status != EntradaFila.STATUS_AGUARDANDO:
            return False, "Não é possível cancelar um atendimento que não está em espera."
        
        # Cancel entry
        success = entrada.cancelar()
        if not success:
            return False, "Não foi possível cancelar o atendimento."
            
        # Update entry in repository
        self.fila_repository.update(entrada)
        
        # Notify other clients about queue update
        self._notify_queue_update(entrada.barbearia.slug)
        
        return True, "Atendimento cancelado com sucesso."
    
    def mark_no_show(self, queue_id: UUID) -> Tuple[bool, str]:
        """
        Mark a client as no-show (absent).
        
        Args:
            queue_id: The ID of the queue entry
            
        Returns:
            A tuple with success status and message
        """
        entrada = self.fila_repository.get_by_id(queue_id)
        if not entrada:
            return False, "Entrada na fila não encontrada."
        
        # Mark as absent
        success = entrada.marcar_ausente()
        if not success:
            return False, "Não foi possível marcar o cliente como ausente."
            
        # Update entry in repository
        self.fila_repository.update(entrada)
        
        # Notify other clients about queue update
        self._notify_queue_update(entrada.barbearia.slug)
        
        return True, "Cliente marcado como ausente com sucesso."
    
    def start_service(self, queue_id: UUID) -> Tuple[bool, str]:
        """
        Start serving a client.
        
        Args:
            queue_id: The ID of the queue entry
            
        Returns:
            A tuple with success status and message
        """
        entrada = self.fila_repository.get_by_id(queue_id)
        if not entrada:
            return False, "Entrada na fila não encontrada."
        
        # Start service
        success = entrada.iniciar_atendimento()
        if not success:
            return False, "Não foi possível iniciar o atendimento."
            
        # Update entry in repository
        self.fila_repository.update(entrada)
        
        # Notify other clients about queue update
        self._notify_queue_update(entrada.barbearia.slug)
        
        return True, "Atendimento iniciado com sucesso."
    
    def finish_service(self, queue_id: UUID) -> Tuple[bool, str]:
        """
        Finish serving a client.
        
        Args:
            queue_id: The ID of the queue entry
            
        Returns:
            A tuple with success status and message
        """
        entrada = self.fila_repository.get_by_id(queue_id)
        if not entrada:
            return False, "Entrada na fila não encontrada."
        
        # Finish service
        success = entrada.finalizar_atendimento()
        if not success:
            return False, "Não foi possível finalizar o atendimento."
            
        # Update entry in repository
        self.fila_repository.update(entrada)
        
        # Notify other clients about queue update
        self._notify_queue_update(entrada.barbearia.slug)
        
        return True, "Atendimento finalizado com sucesso."
    
    def get_barbershop_queue(self, barbearia_slug: str) -> Dict[str, Any]:
        """
        Get the current queue status for a barbershop.
        
        Args:
            barbearia_slug: The slug of the barbershop
            
        Returns:
            A dictionary with queue information
        """
        barbearia = self.barbearia_repository.get_by_slug(barbearia_slug)
        if not barbearia:
            return {
                'success': False,
                'error': 'Barbearia não encontrada'
            }
        
        # Check if barbershop is open
        aberto = barbearia.esta_aberto()
        
        # Get waiting clients using domain status
        waiting_entries = self.fila_repository.get_by_status(
            barbearia.id, 
            EntradaFila.Status.STATUS_AGUARDANDO.value
        )
        
        # Get in-service clients using domain status
        in_service_entries = self.fila_repository.get_by_status(
            barbearia.id, 
            EntradaFila.Status.STATUS_ATENDIMENTO.value
        )
        
        # Get completed services
        completed_entries = self.fila_repository.get_recent_completed(barbearia.id, 5)
        
        # Get barbers
        barbers = self.barbeiro_repository.get_by_barbearia(barbearia.id)
        
        # Get estimated wait time directly from the barbershop model
        estimated_wait_time = barbearia.calcular_tempo_espera()
        
        # Format entries
        waiting_list = []
        for entry in waiting_entries:
            waiting_list.append({
                'id': str(entry.id),
                'cliente_nome': entry.cliente.nome,
                'servico_nome': entry.servico.nome,
                'prioridade': entry.prioridade,
                'prioridade_display': entry.get_prioridade_display(),
                'horario_chegada': entry.created_at.strftime('%H:%M'),
                'position': entry.get_position(),
                'wait_time': entry.get_estimated_wait_time()
            })
            
        in_service_list = []
        for entry in in_service_entries:
            in_service_list.append({
                'id': str(entry.id),
                'cliente_nome': entry.cliente.nome,
                'servico_nome': entry.servico.nome,
                'barbeiro_nome': entry.barbeiro.nome if entry.barbeiro else 'Não atribuído',
                'horario_inicio': entry.horario_atendimento.strftime('%H:%M') if entry.horario_atendimento else '---',
                'tempo_em_atendimento': self._calculate_time_in_service(entry)
            })
            
        completed_list = []
        for entry in completed_entries:
            completed_list.append({
                'id': str(entry.id),
                'cliente_nome': entry.cliente.nome,
                'servico_nome': entry.servico.nome,
                'barbeiro_nome': entry.barbeiro.nome if entry.barbeiro else 'Não atribuído',
                'horario_finalizacao': entry.horario_finalizacao.strftime('%H:%M') if entry.horario_finalizacao else '---'
            })
            
        barber_list = []
        for barber in barbers:
            barber_list.append({
                'id': str(barber.id),
                'nome': barber.nome,
                'status': barber.status,
                'status_display': barber.get_status_display(),
                'is_active': barber.is_active()
            })
            
        # Format estimated wait time using domain service
        formatted_wait_time = WaitTimeCalculator.format_wait_time(estimated_wait_time)
            
        return {
            'success': True,
            'queue': {
                'barbershop_name': barbearia.nome,
                'is_open': aberto,
                'waiting_count': len(waiting_list),
                'in_service_count': len(in_service_list),
                'estimated_wait_time': formatted_wait_time,
                'raw_wait_time_minutes': estimated_wait_time,
                'waiting': waiting_list,
                'in_service': in_service_list,
                'completed': completed_list,
                'barbers': barber_list
            }
        }
    
    def update_barber_status(self, barbeiro_id: UUID, status: str) -> Tuple[bool, str]:
        """
        Update a barber's status.
        
        Args:
            barbeiro_id: The ID of the barber
            status: The new status
            
        Returns:
            A tuple with success status and message
        """
        if not self.barbeiro_repository:
            return False, "Repositório de barbeiros não configurado."
            
        # Validate status before updating
        valid_statuses = [s.value for s in Barbeiro.Status]
        if status not in valid_statuses:
            return False, f"Status inválido. Opções válidas: {', '.join(valid_statuses)}"
            
        # Get barber
        barbeiro = self.barbeiro_repository.get_by_id(barbeiro_id)
        if not barbeiro:
            return False, "Barbeiro não encontrado."
            
        # Update status
        barbeiro.status = status
        self.barbeiro_repository.update(barbeiro)
            
        # Get barber to notify queue update
        if barbeiro.barbearia:
            self._notify_queue_update(barbeiro.barbearia.slug)
            
        return True, "Status atualizado com sucesso."
    
    def get_client_history(self, cliente_id: UUID) -> Dict[str, Any]:
        """
        Get a client's service history.
        
        Args:
            cliente_id: The ID of the client
            
        Returns:
            A dictionary with client history
        """
        cliente = self.cliente_repository.get_by_id(cliente_id)
        if not cliente:
            return {
                'success': False,
                'message': 'Cliente não encontrado.'
            }
            
        # Get all entries for this client
        entries = self.fila_repository.get_all_by_cliente(cliente_id)
        
        # Format response
        history = []
        for entry in entries:
            history.append({
                'id': str(entry.id),
                'barbearia': entry.barbearia.nome,
                'servico': entry.servico.nome,
                'status': entry.status,
                'status_display': entry.get_status_display(),
                'created_at': entry.created_at.isoformat(),
                'horario_atendimento': entry.horario_atendimento.isoformat() if entry.horario_atendimento else None,
                'horario_finalizacao': entry.horario_finalizacao.isoformat() if entry.horario_finalizacao else None
            })
            
        return {
            'success': True,
            'client': {
                'id': str(cliente.id),
                'nome': cliente.nome,
                'telefone': cliente.telefone,
                'email': cliente.email,
                'total_visits': cliente.total_visits,
                'is_vip': cliente.is_vip,
                'loyalty_level': cliente.loyalty_level,
                'last_visit': cliente.last_visit.isoformat() if cliente.last_visit else None
            },
            'history': history
        }
    
    def _notify_queue_update(self, barbearia_slug: str) -> None:
        """
        Notify WebSocket clients about a queue update.
        
        Args:
            barbearia_slug: The slug of the barbershop
        """
        try:
            channel_layer = get_channel_layer()
            
            # Notify the barbershop group
            async_to_sync(channel_layer.group_send)(
                f'barbershop_{barbearia_slug}',
                {
                    'type': 'queue_update',
                    'action': 'queue_changed'
                }
            )
        except Exception:
            # Fail silently if notification fails
            pass
    
    def _calculate_time_in_service(self, entrada) -> str:
        """
        Calculate how long a client has been in service.
        
        Args:
            entrada: The queue entry
            
        Returns:
            A formatted string with the time in service
        """
        if not entrada.horario_atendimento:
            return "0 minutos"
            
        # Calculate minutes since service started
        now = timezone.now()
        minutes = int((now - entrada.horario_atendimento).total_seconds() // 60)
        
        # Use domain service for formatting
        return WaitTimeCalculator.format_wait_time(minutes)

class WebsiteBuilderService:
    """Service for website builder functionality"""
    
    def __init__(self, barbearia_repo=None, section_repo=None, layout_repo=None):
        self.barbearia_repo = barbearia_repo
        self.section_repo = section_repo
        self.layout_repo = layout_repo
    
    def get_barbershop_page(self, barbearia_id):
        """Get custom page for a barbershop"""
        barbearia = self.barbearia_repo.get_by_id(barbearia_id)
        if not barbearia:
            return None
        
        try:
            return barbearia.custom_page
        except:
            return None
    
    def create_default_page(self, barbearia_id):
        """Create default page with standard sections for a barbershop"""
        from domain.models import BarbeariaCustomPage, PageLayout, ActivePageSection, PageSection
        
        barbearia = self.barbearia_repo.get_by_id(barbearia_id)
        if not barbearia:
            return None
            
        # Get default layout
        default_layout = PageLayout.objects.filter(is_active=True).first()
        
        # Create custom page
        custom_page = BarbeariaCustomPage.objects.create(
            barbearia=barbearia,
            layout=default_layout,
            meta_title=f"{barbearia.nome} - Barbearia",
            meta_description=f"Agende seu horário na {barbearia.nome} e evite filas."
        )
        
        # Add default sections
        default_sections = PageSection.objects.filter(is_required=True).order_by('order_index')
        
        for index, section in enumerate(default_sections):
            ActivePageSection.objects.create(
                page=custom_page,
                section=section,
                order=index,
                is_enabled=True,
                configuration=self._get_default_config(section.section_type, barbearia)
            )
            
        return custom_page
    
    def update_page_config(self, page_id, section_configs):
        """Update configuration for a custom page"""
        from domain.models import BarbeariaCustomPage, ActivePageSection
        
        try:
            page = BarbeariaCustomPage.objects.get(id=page_id)
        except BarbeariaCustomPage.DoesNotExist:
            return False
            
        # Update active sections configurations
        for section_id, config in section_configs.items():
            try:
                active_section = ActivePageSection.objects.get(
                    page=page,
                    section_id=section_id
                )
                
                # Update configuration
                active_section.configuration.update(config)
                active_section.save()
                
            except ActivePageSection.DoesNotExist:
                continue
                
        return True
    
    def toggle_section(self, page_id, section_id, is_enabled):
        """Enable or disable a section on a page"""
        from domain.models import ActivePageSection
        
        try:
            active_section = ActivePageSection.objects.get(
                page_id=page_id,
                section_id=section_id
            )
            
            active_section.is_enabled = is_enabled
            active_section.save()
            return True
            
        except ActivePageSection.DoesNotExist:
            return False
    
    def reorder_sections(self, page_id, section_orders):
        """Reorder sections on a page"""
        from domain.models import ActivePageSection
        
        try:
            page = BarbeariaCustomPage.objects.get(id=page_id)
        except BarbeariaCustomPage.DoesNotExist:
            return False
            
        # Update section orders
        for section_id, order in section_orders.items():
            try:
                active_section = ActivePageSection.objects.get(
                    page=page,
                    section_id=section_id
                )
                
                active_section.order = order
                active_section.save()
                
            except ActivePageSection.DoesNotExist:
                continue
                
        return True
    
    def _get_default_config(self, section_type, barbearia):
        """Get default configuration for a section type"""
        if section_type == 'hero':
            return {
                'title': f"Bem-vindo à {barbearia.nome}",
                'subtitle': "Cortes e serviços de qualidade sem precisar enfrentar filas",
                'button_text': "Agendar Agora",
                'button_url': f"/{barbearia.slug}/checkin",
                'background_type': "gradient",
                'background_color': barbearia.cores[0] if barbearia.cores else "#222222"
            }
        elif section_type == 'about':
            return {
                'title': "Sobre Nós",
                'content': "Conte um pouco sobre a sua barbearia, história e diferenciais.",
                'image_position': "right"
            }
        elif section_type == 'services':
            return {
                'title': "Nossos Serviços",
                'description': "Oferecemos os melhores serviços para você",
                'layout': "grid",
                'show_prices': True
            }
        elif section_type == 'team':
            return {
                'title': "Nossa Equipe",
                'description': "Conheça nossos profissionais qualificados",
                'show_specialties': True
            }
        elif section_type == 'contact':
            return {
                'title': "Entre em Contato",
                'show_map': True,
                'show_form': True,
                'address': barbearia.endereco or "",
                'phone': barbearia.telefone or "",
                'email': ""
            }
        else:
            return {} 