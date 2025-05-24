from django.shortcuts import render, redirect, get_object_or_404
from django.views import View
from django.contrib import messages
from django.urls import reverse
from django.utils import timezone
from django.db import models, transaction
from django.db.models import Sum, Count
from django.http import JsonResponse, Http404, HttpResponseNotFound, HttpResponse
from uuid import UUID
import logging
from datetime import datetime, timedelta
import json
import traceback
import time
import uuid as uuid_module
from django.core.exceptions import ValidationError

# Import domain models and classes
from domain.models import EntradaFila as DomainEntradaFila
from domain.models import Barbeiro as DomainBarbeiro
from domain.models import BarbeariaCustomPage, ActivePageSection, PageLayout, PageSection

# Import ORM models - this is the correct Barbearia model to use
from barbershop.models import Barbearia, Servico, Fila, Barbeiro, Cliente
from application.services import FilaService
from application.dtos import CheckInDTO, ClienteDTO
from application.tasks.queue_tasks import process_checkin
from infrastructure.repositories import (
    DjangoClienteRepository,
    DjangoFilaRepository,
    DjangoBarbeariaRepository,
    DjangoServicoRepository
)
from .utils import add_test_queue_data

# Add a configured logger
logger = logging.getLogger(__name__)

class HomeView(View):
    """Home page showing the list of barbershops"""
    def get(self, request):
        barbershops = Barbearia.objects.all()
        return render(request, 'barbershop/home.html', {
            'barbershops': barbershops
        })

class BarbershopView(View):
    """View for displaying barbershop details"""
    template_name = 'barbershop/detail.html'
    
    def get(self, request, slug=None, *args, **kwargs):
        """Get barbershop details by slug"""
        barbershop = None # Initialize barbershop
        try:
            # Clean the slug - remove any @eutonafila suffix if present
            if slug and '@' in slug:
                slug = slug.split('@')[0]
            
            # Convert numeric slug to string
            if isinstance(slug, (int, float)):
                slug = str(slug)
            
            logger.info(f"Attempting to fetch Barbearia with slug: '{slug}'")
            try:
                barbershop = Barbearia.objects.get(slug=slug)
                logger.info(f"Successfully fetched Barbearia by slug: {barbershop.id if barbershop else 'None'}")
            except Barbearia.DoesNotExist:
                logger.warning(f"Barbearia with slug '{slug}' not found. Trying by name.")
                try:
                    logger.info(f"Attempting to fetch Barbearia with name: '{slug}'")
                    barbershop = Barbearia.objects.get(nome=slug)
                    logger.info(f"Successfully fetched Barbearia by name: {barbershop.id if barbershop else 'None'}")
                except Barbearia.DoesNotExist:
                    logger.error(f"No barbershop found with slug or name: {slug}")
                    raise Http404(f"Barbearia com slug '{slug}' não encontrada")
                except ValidationError as ve_name:
                    logger.error(f"ValidationError fetching Barbearia by name '{slug}': {ve_name.messages}", exc_info=True)
                    # Propagate the original validation error message structure if possible
                    error_message = f"Erro de validação ao buscar barbearia por nome: {ve_name.messages}"
                    if hasattr(ve_name, 'message_dict'):
                         error_message = f"Erro de validação ao buscar barbearia por nome: {ve_name.message_dict}"
                    raise Http404(error_message)

            except ValidationError as ve_slug:
                logger.error(f"ValidationError fetching Barbearia by slug '{slug}': {ve_slug.messages}", exc_info=True)
                # Propagate the original validation error message structure if possible
                error_message = f"Erro de validação ao buscar barbearia por slug: {ve_slug.messages}"
                if hasattr(ve_slug, 'message_dict'):
                    error_message = f"Erro de validação ao buscar barbearia por slug: {ve_slug.message_dict}"
                raise Http404(error_message)
            except Exception as ex_fetch: # Catch other exceptions during fetch
                logger.error(f"Unexpected error fetching Barbearia '{slug}': {type(ex_fetch).__name__} - {str(ex_fetch)}", exc_info=True)
                raise Http404(f"Erro inesperado ao buscar barbearia: {str(ex_fetch)}")

            if not barbershop: # Should be caught by DoesNotExist, but as a safeguard
                logger.error(f"Barbershop is None after attempting fetch for slug/name: {slug}")
                raise Http404(f"Barbearia não encontrada (inesperado): {slug}")

            # Calculate wait times
            tempo_espera = 0 # barbershop.calcular_tempo_espera() # Temporarily commented out
            
            # Get services
            services = [] # Servico.objects.filter(barbearia=barbershop) # Temporarily commented out
            
            # Count clients in queue
            clientes_na_fila = 0 # Fila.objects.filter(
            #     barbearia=barbershop,
            #     status=DomainEntradaFila.Status.AGUARDANDO.value
            # ).count() # Temporarily commented out
            
            # Check if there's a custom page
            # try:
            #     custom_page = barbershop.custom_page
            #     if custom_page and custom_page.active_sections.exists():
            #         self.template_name = 'barbershop/custom.html'
            # except BarbeariaCustomPage.DoesNotExist:
            #     pass
            
            context = {
                'barbershop': barbershop,
                'services': services,
                'tempo_espera': tempo_espera,
                'clientes_na_fila': clientes_na_fila,
            }
            
            return render(request, self.template_name, context)
            
        except Http404 as http_e: # Re-raise Http404 to let Django handle it
            logger.warning(f"Http404 raised for slug '{slug}': {str(http_e)}")
            raise 
        except Exception as e:
            logger.error(f"Outer catch: Error getting barbershop details for slug '{slug}'. Type: {type(e).__name__}, Args: {e.args}, Str: {str(e)}", exc_info=True)
            # The original error message indicates a list, so we try to replicate that if e.args is suitable
            error_detail = str(e)
            if e.args and isinstance(e.args[0], list):
                error_detail = e.args[0]
            elif isinstance(e.args, tuple) and len(e.args) > 0: # ValidationError often has messages in args[0]
                error_detail = e.args[0]

            raise Http404(f"Erro ao obter detalhes da barbearia: {error_detail}")

# Keep this class for backward compatibility but modify it to redirect
class BarbershopCustomPageView(View):
    """Redirects to the unified BarbershopView"""
    def get(self, request, slug):
        return redirect('barbershop:barbershop_detail', slug=slug)

class CheckInView(View):
    """
    Check-in page for clients to join the queue.
    Uses asynchronous processing via Celery/RabbitMQ.
    """
    template_name = 'barbershop/checkin.html'
    
    def get(self, request, slug):
        try:
            barbershop = Barbearia.objects.get(slug=slug)
            barbeiros = Barbeiro.objects.filter(barbearia=barbershop)
            servicos = Servico.objects.filter(barbearia=barbershop)
            
            context = {
                'barbershop': barbershop,
                'barbeiros': barbeiros,
                'servicos': servicos,
            }
            
            return render(request, self.template_name, context)
        except Barbearia.DoesNotExist:
            logger.error(f"Barbershop with slug {slug} not found")
            return redirect('home')
    
    def post(self, request, slug):
        try:
            barbershop = Barbearia.objects.get(slug=slug)
            
            # Check if barbershop is open
            if not barbershop.esta_aberto():
                messages.error(request, 'A barbearia está fechada no momento.')
                return redirect('barbershop:checkin', slug=slug)
            
            # Check if queue is full
            if barbershop.is_queue_full():
                messages.error(request, 'A fila está cheia no momento. Por favor, tente novamente mais tarde.')
                return redirect('barbershop:checkin', slug=slug)
            
            # Extract form data
            nome = request.POST.get('name')
            telefone = request.POST.get('phone')
            servico_id = request.POST.get('service')
            barbeiro_id = None  # This field isn't in the form
            notificacao = request.POST.get('notificacao') == 'on'
            observacoes = f"Notificação SMS: {'Sim' if notificacao else 'Não'}"
            
            # Validate required fields
            if not nome or not telefone or not servico_id:
                logger.warning(f"Missing required fields in check-in form: nome={bool(nome)}, telefone={bool(telefone)}, servico={bool(servico_id)}")
                messages.error(request, 'Por favor, preencha todos os campos obrigatórios.')
                return redirect('barbershop:checkin', slug=slug)
            
            # Validate service ID
            try:
                servico_id = UUID(servico_id)
            except ValueError:
                logger.error(f"Invalid service ID format: {servico_id}")
                messages.error(request, 'ID do serviço inválido.')
                return redirect('barbershop:checkin', slug=slug)
            
            # Step 1: Create or update the client
            cliente, created = Cliente.objects.get_or_create(
                telefone=telefone,
                defaults={'nome': nome}
            )
            if not created and cliente.nome != nome:
                cliente.nome = nome
                cliente.save()
            
            # Step 2: Get the service
            try:
                servico = Servico.objects.get(id=servico_id)
            except Servico.DoesNotExist:
                logger.error(f"Service ID {servico_id} not found")
                messages.error(request, 'Serviço selecionado não encontrado.')
                return redirect('barbershop:checkin', slug=slug)
            
            # Step 3: Handle optional barber
            barbeiro = None
            if barbeiro_id:
                try:
                    barbeiro = Barbeiro.objects.get(id=barbeiro_id)
                except Barbeiro.DoesNotExist:
                    logger.warning(f"Selected barber ID {barbeiro_id} does not exist")
                    barbeiro = None
            
            # Step 4: Create the queue entry
            with transaction.atomic():
                # Create queue entry with a new UUID
                fila_entry = Fila.objects.create(
                    barbearia=barbershop,
                    cliente=cliente,  # Reference the existing client
                    servico=servico,
                    barbeiro_preferido=barbeiro,
                    observacoes=observacoes,
                    status='aguardando',  # Explicitly set the status
                    horario_entrada=timezone.now()  # Set the entry time
                )
                
                # Log the created entry's UUID for debugging
                logger.info(f"Created Fila entry with ID {fila_entry.id} for client {cliente.nome}")
                
                # Create URL for redirect
                status_url = reverse('barbershop:queue_status', kwargs={'queue_id': fila_entry.id})
                
                # Log the redirect URL for debugging
                logger.debug(f"Redirecting to queue status URL: {status_url}")
                
                return redirect(status_url)
                
        except Barbearia.DoesNotExist:
            logger.error(f"Barbershop with slug {slug} not found during check-in process")
            messages.error(request, 'Barbearia não encontrada.')
            return redirect('home')
        except Exception as e:
            logger.exception(f"Error during check-in process: {str(e)}")
            messages.error(request, 'Erro ao finalizar check-in. Tente novamente em alguns instantes.')
            return redirect('barbershop:checkin', slug=slug)

class ProcessingView(View):
    """
    Processing page that waits for the async task to complete
    """
    def get(self, request, task_id):
        return render(request, 'barbershop/processing.html', {
            'task_id': task_id
        })
        
class CheckTaskStatusView(View):
    """
    API endpoint to check the status of a Celery task
    """
    def get(self, request, task_id):
        from celery.result import AsyncResult
        
        result = AsyncResult(task_id)
        
        if result.ready():
            if result.successful():
                response_data = result.get()
                if response_data.get('status') == 'success':
                    return JsonResponse({
                        'status': 'complete',
                        'success': True,
                        'queue_id': response_data.get('queue_id')
                    })
                else:
                    return JsonResponse({
                        'status': 'complete',
                        'success': False,
                        'message': response_data.get('message', 'Unknown error')
                    })
            else:
                return JsonResponse({
                    'status': 'failed',
                    'message': str(result.result)
                })
        else:
            return JsonResponse({
                'status': 'pending'
            })

class QueueStatusView(View):
    """Queue status page for clients to monitor their position"""
    template_name = 'barbershop/queue_status.html'
    
    def get(self, request, queue_id):
        try:
            # Log the UUID being requested
            logger.debug(f"Looking up Fila entry with UUID: {queue_id}")
            
            # Ensure we're using the correct UUID format
            if isinstance(queue_id, str):
                try:
                    uuid_obj = UUID(queue_id)
                    logger.debug(f"Converted string UUID to UUID object: {uuid_obj}")
                except ValueError:
                    logger.error(f"Invalid UUID format: {queue_id}")
                    messages.error(request, 'ID de fila inválido.')
                    return redirect('home')
            
            # Query for the Fila entry with explicit transaction
            with transaction.atomic():
                fila = Fila.objects.get(id=queue_id)
                logger.debug(f"Found Fila entry: {fila.id} - Status: {fila.status}")
                
                # Get position in line and other relevant data
                position = fila.get_position()
                wait_time = fila.barbearia.calcular_tempo_espera()
                expected_time = None
                
                if wait_time:
                    wait_minutes = wait_time.total_seconds() / 60
                    expected_time = timezone.now() + timedelta(minutes=wait_minutes * position)
                
                context = {
                    'fila': fila,
                    'position': position,
                    'wait_time': wait_time,
                    'expected_time': expected_time
                }
                
                return render(request, self.template_name, context)
                
        except Fila.DoesNotExist:
            logger.error(f"Fila entry with UUID {queue_id} not found")
            messages.error(request, 'Entrada na fila não encontrada.')
            return redirect('home')
        except Exception as e:
            logger.exception(f"Error retrieving queue status: {str(e)}")
            messages.error(request, 'Erro ao carregar o status da fila.')
            return redirect('home')

class DashboardView(View):
    """Dashboard for barbershop staff to manage the queue"""
    def get(self, request, slug):
        barbershop = get_object_or_404(Barbearia, slug=slug)
        
        # Get all active entries in the queue
        entradas = DomainEntradaFila.objects.filter(
            barbearia=barbershop,
            status=DomainEntradaFila.Status.AGUARDANDO
        ).order_by('posicao')
        
        # Get entries being served
        em_atendimento = DomainEntradaFila.objects.filter(
            barbearia=barbershop,
            status=DomainEntradaFila.Status.EM_ATENDIMENTO
        )
        
        # Get completed entries from today
        hoje = timezone.now().date()
        completados = DomainEntradaFila.objects.filter(
            barbearia=barbershop,
            status=DomainEntradaFila.Status.FINALIZADO,
            updated_at__date=hoje
        )
        
        # Get all barbers for this barbershop
        barbeiros = Barbeiro.objects.filter(barbearia=barbershop)
        
        return render(request, 'barbershop/dashboard.html', {
            'barbershop': barbershop,
            'entradas': entradas,
            'em_atendimento': em_atendimento,
            'completados': completados,
            'barbeiros': barbeiros,
            'total_na_fila': entradas.count(),
            'tempo_medio_espera': self._calcular_tempo_medio(barbershop)
        })
    
    def _calcular_tempo_medio(self, barbershop):
        # Calculate average service time for the barbershop
        hoje = timezone.now().date()
        entradas_finalizadas = DomainEntradaFila.objects.filter(
            barbearia=barbershop,
            status=DomainEntradaFila.Status.FINALIZADO,
            updated_at__date=hoje
        )
        
        if not entradas_finalizadas.exists():
            return "N/A"
        
        tempo_total = entradas_finalizadas.aggregate(
            total=Sum('servico__duracao_minutos')
        )['total'] or 0
        
        media = tempo_total / entradas_finalizadas.count()
        return f"{int(media)} minutos"

class GettingStartedView(View):
    """Getting started guide page"""
    def get(self, request):
        return render(request, 'barbershop/getting_started.html')

# Debug view - remove in production
def debug_add_queue_clients(request, slug, num_clients=3):
    """
    Debug utility to add test clients to the queue
    """
    if not request.user.is_superuser and not request.GET.get('debug') == 'secret123':
        return JsonResponse({'error': 'Unauthorized'}, status=403)
        
    result = add_test_queue_data(slug, int(num_clients))
    
    # Instead of returning JSON, redirect back to the barbershop page
    if result['success']:
        if num_clients > 0:
            messages.success(request, f"Added {num_clients} test clients to the queue successfully.")
        else:
            messages.success(request, "Queue cleared successfully.")
    else:
        messages.error(request, f"Error: {result.get('error', 'Unknown error')}")
    
    # Redirect back to the barbershop detail page
    return redirect('barbershop:barbershop_detail', slug=slug)

def debug_list_services(request, slug):
    """
    Debug utility to list all services for a barbershop
    """
    # Temporarily remove authentication for testing
    #if not request.GET.get('debug') == 'secret123':
    #    return JsonResponse({'error': 'Unauthorized'}, status=403)
    
    try:
        # Get the barbershop
        barbershop = get_object_or_404(Barbearia, slug=slug)
        
        # Get all services
        services = Servico.objects.filter(barbearia_id=barbershop.id)
        
        # Instead of returning JSON, redirect to the check-in page where services are visible
        if request.GET.get('redirect', 'true').lower() == 'true':
            messages.info(request, f"Found {services.count()} services for {barbershop.nome}")
            return redirect('barbershop:checkin', slug=slug)
        
        # If redirect=false, display the services in a simple HTML format
        context = {
            'barbershop': barbershop,
            'services': services,
        }
        return render(request, 'barbershop/debug_services.html', context)
        
    except Exception as e:
        messages.error(request, f"Error listing services: {str(e)}")
        return redirect('barbershop:barbershop_detail', slug=slug)

def debug_create_entry(request, slug):
    """
    Debug function to create a queue entry directly, bypassing all the complexities
    """
    import traceback
    from barbershop.models import Barbearia, Servico, Fila, Cliente
    from django.shortcuts import get_object_or_404
    from django.http import JsonResponse

    if not request.GET.get('debug') == 'secret123':
        return JsonResponse({'error': 'Unauthorized'}, status=403)
    
    try:
        # Get barbershop
        barbershop = get_object_or_404(Barbearia, slug=slug)
        
        # Get first service
        service = Servico.objects.filter(barbearia=barbershop).first()
        if not service:
            messages.error(request, "No services found for this barbershop. Please add services first.")
            return redirect('barbershop:barbershop_detail', slug=slug)
        
        # Create a test client
        client, created = Cliente.objects.get_or_create(
            telefone="123456789",
            defaults={
                'nome': "Test Client",
                'email': "test@example.com"
            }
        )
        
        # Create entry directly using ORM models
        entry = Fila.objects.create(
            barbearia=barbershop,
            cliente=client,
            servico=service,
            status='aguardando'
        )
        
        messages.success(request, f"Test entry created successfully for client: {client.nome}")
        return redirect('barbershop:queue_status', queue_id=entry.id)
        
    except Exception as e:
        messages.error(request, f"Error creating test entry: {str(e)}")
        return redirect('barbershop:barbershop_detail', slug=slug)

class OwnerDashboardView(View):
    """Admin dashboard for barbershop owners with analytics and management"""
    
    def get(self, request, slug):
        # Get the barbershop object or return 404
        barbershop = get_object_or_404(Barbearia, slug=slug)
        
        # Check if user is owner of the barbershop
        if request.user != barbershop.user:
            return redirect('barbershop:barbershop_detail', slug=slug)
        
        # Get current date
        today = datetime.now().date()
        
        # Get active barbers
        barbers = Barbeiro.objects.filter(barbearia=barbershop)
        active_barbers = barbers.filter(
            status__in=[
                DomainBarbeiro.Status.STATUS_AVAILABLE.value,
                DomainBarbeiro.Status.STATUS_BUSY.value,
                DomainBarbeiro.Status.STATUS_BREAK.value
            ]
        ).count()
        
        # Get status counts
        barber_status = {
            'available': barbers.filter(status=DomainBarbeiro.Status.STATUS_AVAILABLE.value).count(),
            'busy': barbers.filter(status=DomainBarbeiro.Status.STATUS_BUSY.value).count(),
            'break': barbers.filter(status=DomainBarbeiro.Status.STATUS_BREAK.value).count(),
            'offline': barbers.filter(status=DomainBarbeiro.Status.STATUS_OFFLINE.value).count(),
        }
        
        # Get waiting clients
        waiting_clients = Fila.objects.filter(
            barbearia=barbershop,
            status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value,
        ).count()
        
        # Get appointments today
        appointments_today = Fila.objects.filter(
            barbearia=barbershop,
            status=DomainEntradaFila.Status.STATUS_FINALIZADO.value,
            horario_finalizacao__date=today
        ).count()
        
        # Get appointments yesterday at this time for trend comparison
        yesterday = today - timedelta(days=1)
        current_time = datetime.now().time()
        appointments_yesterday = Fila.objects.filter(
            barbearia=barbershop,
            status=DomainEntradaFila.Status.STATUS_FINALIZADO.value,
            horario_finalizacao__date=yesterday,
            horario_finalizacao__time__lte=current_time
        ).count()
        
        # Calculate trend
        today_trend = 0
        trend_percent = 0
        if appointments_yesterday > 0:
            trend_percent = int(abs(appointments_today - appointments_yesterday) / appointments_yesterday * 100)
            if appointments_today > appointments_yesterday:
                today_trend = 1  # up
            elif appointments_today < appointments_yesterday:
                today_trend = -1  # down
        else:
            trend_percent = 100 if appointments_today > 0 else 0
            today_trend = 1 if appointments_today > 0 else 0
        
        # Get average wait time
        avg_wait_time = 0
        wait_time_records = Fila.objects.filter(
            barbearia=barbershop,
            status=DomainEntradaFila.Status.STATUS_FINALIZADO.value,
            horario_finalizacao__date=today
        )
        
        if wait_time_records.exists():
            wait_times = [
                (record.horario_finalizacao - record.horario_atendimento).total_seconds() / 60
                for record in wait_time_records
                if record.horario_atendimento is not None and record.horario_finalizacao is not None
            ]
            if wait_times:
                avg_wait_time = int(sum(wait_times) / len(wait_times))
        
        # Format avg wait time
        avg_wait_time_formatted = f"{avg_wait_time} minutos"
        
        # Get appointments for the last 7 days
        days_data = []
        days_labels = []
        
        for i in range(6, -1, -1):
            day = today - timedelta(days=i)
            day_name = day.strftime('%a')
            count = Fila.objects.filter(
                barbearia=barbershop,
                status=DomainEntradaFila.Status.STATUS_FINALIZADO.value,
                horario_finalizacao__date=day
            ).count()
            days_data.append(count)
            days_labels.append(day_name)
        
        # Get services by type
        services = Servico.objects.filter(barbearia=barbershop)
        service_data = []
        service_labels = []
        
        for service in services:
            count = Fila.objects.filter(
                barbearia=barbershop,
                servico=service,
                status=DomainEntradaFila.Status.STATUS_FINALIZADO.value,
                horario_finalizacao__date__gte=today - timedelta(days=30)
            ).count()
            if count > 0:
                service_data.append(count)
                service_labels.append(service.nome)
        
        # Get barber performance data
        barbers_data = []
        for barber in barbers:
            appointments = Fila.objects.filter(
                barbearia=barbershop,
                barbeiro=barber,
                status=DomainEntradaFila.Status.STATUS_FINALIZADO.value,
                horario_finalizacao__date=today
            ).count()
            
            # Calculate average service time
            barber_services = Fila.objects.filter(
                barbearia=barbershop,
                barbeiro=barber,
                status=DomainEntradaFila.Status.STATUS_FINALIZADO.value,
                horario_finalizacao__date__gte=today - timedelta(days=7)
            )
            
            avg_time = 0
            if barber_services.exists():
                service_times = [
                    (service.horario_finalizacao - service.horario_atendimento).total_seconds() / 60
                    for service in barber_services
                    if service.horario_atendimento is not None and service.horario_finalizacao is not None
                ]
                if service_times:
                    avg_time = int(sum(service_times) / len(service_times))
            
            barbers_data.append({
                'barber': barber,
                'appointments': appointments,
                'avg_time': avg_time
            })
        
        context = {
            'barbershop': barbershop,
            'active_barbers': active_barbers,
            'barber_status': barber_status,
            'waiting_clients': waiting_clients,
            'appointments_today': appointments_today,
            'today_trend': today_trend,
            'today_trend_percent': trend_percent,
            'avg_wait_time': avg_wait_time,
            'avg_wait_time_formatted': avg_wait_time_formatted,
            'days_data': json.dumps(days_data),
            'days_labels': json.dumps(days_labels),
            'service_data': json.dumps(service_data),
            'service_labels': json.dumps(service_labels),
            'barbers': barbers_data,
        }
        
        return render(request, 'barbershop/owner_dashboard.html', context)

def debug_add_service(request, slug):
    """
    Debug utility to add a test service to a barbershop
    """
    try:
        # Get the barbershop
        barbershop = get_object_or_404(Barbearia, slug=slug)
        
        # Create a test service
        service_name = request.GET.get('name', 'Corte de Cabelo Test')
        service_price = request.GET.get('price', '30.00')
        service_duration = request.GET.get('duration', '30')
        
        service = Servico.objects.create(
            barbearia=barbershop,
            nome=service_name,
            preco=service_price,
            duracao=service_duration,
            descricao="Serviço de teste criado via debug endpoint"
        )
        
        # Add success message and redirect
        messages.success(request, f"Test service '{service_name}' created successfully!")
        return redirect('barbershop:checkin', slug=slug)
        
    except Exception as e:
        # Add error message and redirect
        messages.error(request, f"Error creating test service: {str(e)}")
        return redirect('barbershop:barbershop_detail', slug=slug)

def debug_restore_services(request, slug):
    """
    Debug utility to restore default services for a barbershop
    """
    try:
        # Get the barbershop
        barbershop = get_object_or_404(Barbearia, slug=slug)
        
        # Manually call the management command
        from django.core.management import call_command
        call_command('ensure_services')
        
        # Count services
        services_count = Servico.objects.filter(barbearia=barbershop).count()
        
        # Add success message and redirect
        messages.success(request, f"Services restored. {barbershop.nome} now has {services_count} services.")
        return redirect('barbershop:checkin', slug=slug)
        
    except Exception as e:
        # Add error message and redirect
        messages.error(request, f"Error restoring services: {str(e)}")
        return redirect('barbershop:barbershop_detail', slug=slug) 