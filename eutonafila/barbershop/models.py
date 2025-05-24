from django.db import models
from django.contrib.auth.models import User
from django.utils import timezone
from django.core.cache import cache
from django.utils.text import slugify
import datetime
import uuid
import logging
from django.db.models import Avg, F, ExpressionWrapper, DurationField
from domain.entities import (
    BarbeiroStatus, FilaStatus, FilaPrioridade,
    ServicoComplexidade, ClienteStatus
)
from domain.services import (
    OpeningHoursValidator,
    QueueManager
)
from domain.services.wait_time_calculator import WaitTimeCalculator

# Import domain models from domain_models for consistent status constants and services
from domain.domain_models import EntradaFila as DomainEntradaFila
from domain.domain_models import Barbeiro as DomainBarbeiro

logger = logging.getLogger(__name__)

class Barbearia(models.Model):
    """ORM model for Barbearia that implements domain.entities.Barbearia interface"""
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    nome = models.CharField(max_length=100)
    slug = models.SlugField(unique=True)
    telefone = models.CharField(max_length=20, blank=True, null=True)
    endereco = models.TextField(blank=True, null=True)
    descricao_curta = models.CharField(max_length=200, blank=True, null=True, help_text='Short description for SEO and previews')
    cores = models.JSONField(default=list)  # ["#FF0066", "#222222"]
    logo = models.ImageField(upload_to='logos/', blank=True, null=True)
    horario_abertura = models.TimeField(default='09:00')
    horario_fechamento = models.TimeField(default='18:00')
    dias_funcionamento = models.JSONField(default=list)  # [0, 1, 2, 3, 4, 5] (0=Monday, 6=Sunday)
    max_capacity = models.IntegerField(default=10)
    enable_priority_queue = models.BooleanField(default=False)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    user = models.ForeignKey(User, on_delete=models.CASCADE, null=True, blank=True, related_name='barbearias')
    
    # Status cache key
    @property
    def _status_cache_key(self):
        return f'barbearia_status:{self.id}'
    
    # Wait time cache key
    @property
    def _wait_time_cache_key(self):
        return f'tempo_espera:{self.id}'
    
    def __str__(self):
        return f"{self.nome} ({self.id})"
    
    def esta_aberto(self):
        """Check if the barbershop is currently open"""
        try:
            current_weekday = timezone.localtime().weekday()
            current_time = timezone.localtime().time()
            
            # Check if current day is a working day
            if current_weekday not in self.dias_funcionamento:
                return False
                
            # Check if current time is within opening hours
            return self.horario_abertura <= current_time < self.horario_fechamento
        except Exception as e:
            logger.error(f"Error checking if barbershop is open: {str(e)}")
            return False
    
    def invalidate_status_cache(self):
        """Invalidate the status cache when operating hours change"""
        cache.delete(self._status_cache_key)
    
    def invalidate_wait_time_cache(self):
        """Invalidate the wait time cache"""
        cache.delete(self._wait_time_cache_key)
    
    def save(self, *args, **kwargs):
        """Override save to handle UUIDs and invalidate caches"""
        try:
            # Ensure id is a valid UUID
            if not self.id:
                self.id = uuid.uuid4()
            elif isinstance(self.id, str):
                try:
                    self.id = uuid.UUID(self.id)
                except ValueError:
                    logger.error(f"Invalid UUID string: {self.id}")
                    self.id = uuid.uuid4()
            elif not isinstance(self.id, uuid.UUID):
                logger.error(f"Invalid ID type: {type(self.id)}")
                self.id = uuid.uuid4()
            
            # Ensure dias_funcionamento is a list
            if not isinstance(self.dias_funcionamento, list):
                self.dias_funcionamento = []
            
            # Ensure cores is a list
            if not isinstance(self.cores, list):
                self.cores = []
            
            # Ensure slug is a string
            if not isinstance(self.slug, str):
                self.slug = str(self.slug)

            # Ensure slug is unique
            base_slug = slugify(self.nome)
            slug_candidate = base_slug
            counter = 1
            while Barbearia.objects.filter(slug=slug_candidate).exclude(id=self.id).exists():
                slug_candidate = f"{base_slug}-{counter}"
                counter += 1
            self.slug = slug_candidate

            # Invalidate caches
            self.invalidate_status_cache()
            self.invalidate_wait_time_cache()
            
            # Call parent save
            super().save(*args, **kwargs)
            
        except Exception as e:
            logger.error(f"Error saving Barbearia: {str(e)}", exc_info=True)
            raise
    
    def calcular_tempo_espera(self):
        """Calculate average wait time based on current queue using domain service"""
        # Try to get from cache first
        cache_key = self._wait_time_cache_key
        cached_value = cache.get(cache_key)
        
        if cached_value is not None:
            return cached_value
        
        # Get active clients in queue with domain status constant
        clientes_na_fila = Fila.objects.filter(
            barbearia=self,
            status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value,
            created_at__date=timezone.now().date()
        ).select_related('servico')
        
        if not clientes_na_fila.exists():
            cache.set(cache_key, 0, timeout=30)
            return 0
        
        # Get active barbers with domain status constants
        barbeiros_ativos = Barbeiro.objects.filter(
            barbearia=self, 
            status__in=[
                DomainBarbeiro.Status.STATUS_AVAILABLE.value, 
                DomainBarbeiro.Status.STATUS_BUSY.value
            ]
        ).count()
        
        # Get service durations for waiting clients
        service_durations = [entry.servico.duracao for entry in clientes_na_fila]
        
        # Use domain service to calculate wait time
        tempo_estimado = WaitTimeCalculator.calculate(
            service_durations=service_durations,
            active_barber_count=barbeiros_ativos
        )
        
        # Cache for 30 seconds
        cache.set(cache_key, tempo_estimado, timeout=30)
        return tempo_estimado
    
    @property
    def tempo_espera_estimado(self):
        """Format wait time as a human-readable string using domain service"""
        wait_minutes = self.calcular_tempo_espera()
        return WaitTimeCalculator.format_wait_time(wait_minutes)
    
    def is_queue_full(self) -> bool:
        waiting_count = self.fila_set.filter(
            status=FilaStatus.AGUARDANDO.value
        ).count()
        return waiting_count >= self.max_capacity

    def reordenar_posicoes(self):
        """Reorder queue positions"""
        try:
            entries = self.entradas.filter(status=DomainEntradaFila.Status.AGUARDANDO).order_by('data_entrada')
            for i, entry in enumerate(entries, 1):
                entry.posicao = i
                entry.save()
        except Exception as e:
            logger.error(f"Error reordering queue positions: {str(e)}")

    def adicionar_cliente(self, cliente_nome, servico, phone=None, email=None):
        """Add client to queue"""
        try:
            # Check if queue is full
            if self.esta_cheia():
                raise ValueError("Queue is full")
            
            # Get current position
            last_position = self.entradas.filter(status=DomainEntradaFila.Status.AGUARDANDO).count()
            new_position = last_position + 1
            
            # Create new entry
            entry = EntradaFila.objects.create(
                fila=self,
                cliente_nome=cliente_nome,
                servico=servico,
                posicao=new_position,
                status=DomainEntradaFila.Status.AGUARDANDO,
                telefone=phone,
                email=email
            )
            
            return entry
        except Exception as e:
            logger.error(f"Error adding client to queue: {str(e)}")
            raise


class Barbeiro(models.Model):
    """ORM model for Barbeiro that implements domain.entities.Barbeiro interface"""
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    usuario = models.OneToOneField(User, on_delete=models.CASCADE)
    barbearia = models.ForeignKey(Barbearia, on_delete=models.CASCADE)
    nome = models.CharField(max_length=100)
    telefone = models.CharField(max_length=20, blank=True, null=True)
    foto = models.ImageField(upload_to='barbeiros/', blank=True, null=True)
    status = models.CharField(
        max_length=20,
        choices=[(s.value, s.name) for s in BarbeiroStatus],
        default=BarbeiroStatus.OFFLINE.value
    )
    especialidades = models.ManyToManyField('Servico', related_name='barbeiros_especializados')
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    def __str__(self):
        return self.nome
    
    @property
    def is_active(self) -> bool:
        return BarbeiroStatus.is_active(BarbeiroStatus(self.status))
    
    def atender_proximo(self):
        """Attend the next client in the queue"""
        # Get the next client in the queue by arrival time (FIFO)
        proximo_cliente = Fila.objects.filter(
            barbearia=self.barbearia,
            status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value,
            created_at__date=timezone.now().date()
        ).order_by('created_at').first()
        
        if proximo_cliente:
            # Update client status
            proximo_cliente.status = DomainEntradaFila.Status.STATUS_ATENDIMENTO.value
            proximo_cliente.barbeiro = self
            proximo_cliente.horario_atendimento = timezone.now()
            proximo_cliente.save()
            
            # Update barber status
            self.status = BarbeiroStatus.BUSY.value
            self.save()
            
            # Invalidate cache
            self.barbearia.invalidate_wait_time_cache()
            
            return proximo_cliente
        
        return None
    
    def save(self, *args, **kwargs):
        """Override save to invalidate wait time cache when status changes"""
        # If status changed, invalidate wait time cache
        if self.pk:
            old_status = Barbeiro.objects.get(pk=self.pk).status
            if old_status != self.status:
                self.barbearia.invalidate_wait_time_cache()
        
        super().save(*args, **kwargs)


class Servico(models.Model):
    """ORM model for Servico that implements domain.entities.Servico interface"""
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    barbearia = models.ForeignKey(Barbearia, on_delete=models.CASCADE, related_name='servicos')
    nome = models.CharField(max_length=100)
    descricao = models.TextField(blank=True, null=True)
    preco = models.DecimalField(max_digits=10, decimal_places=2)
    duracao = models.IntegerField(help_text='Duração em minutos')
    # Make complexity optional for backward compatibility with existing database
    complexity = models.IntegerField(
        choices=[(c.value, c.name) for c in ServicoComplexidade],
        default=ServicoComplexidade.MEDIUM.value,
        null=True,
        blank=True
    )
    # Make popularity optional for backward compatibility
    popularity = models.IntegerField(default=0, null=True, blank=True)
    imagem = models.ImageField(upload_to='servicos/', blank=True, null=True)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    def __str__(self):
        return self.nome
    
    def increment_popularity(self):
        """Safely increment the popularity counter if it exists"""
        try:
            if hasattr(self, 'popularity') and self.popularity is not None:
                self.popularity += 1
                self.save(update_fields=['popularity', 'updated_at'])
        except Exception as e:
            # Log the error but don't crash if the column doesn't exist
            logger.warning(f"Could not increment popularity for {self.nome}: {str(e)}")
            pass
    
    def get_median_wait_time(self):
        """Calculate median wait time for this service"""
        completed_services = Fila.objects.filter(
            servico=self,
            status='completed'
        ).order_by('created_at')
        
        if not completed_services:
            return self.duracao  # Return service duration as default
            
        wait_times = []
        for service in completed_services:
            if service.completed_at and service.created_at:
                wait_time = (service.completed_at - service.created_at).total_seconds() / 60
                wait_times.append(wait_time)
        
        if not wait_times:
            return self.duracao
            
        wait_times.sort()
        mid = len(wait_times) // 2
        return (wait_times[mid] + wait_times[~mid]) / 2


class Cliente(models.Model):
    """ORM model for Cliente that implements domain.entities.Cliente interface"""
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    nome = models.CharField(max_length=100)
    telefone = models.CharField(max_length=20)  # Changed to 20 to support international
    email = models.EmailField(blank=True, null=True)
    total_visits = models.IntegerField(default=0)
    is_vip = models.BooleanField(default=False)
    last_visit = models.DateTimeField(null=True, blank=True)
    created_at = models.DateTimeField(auto_now_add=True)
    
    def __str__(self):
        return self.nome
    
    @property
    def loyalty_level(self) -> ClienteStatus:
        if self.total_visits >= 20:
            return ClienteStatus.GOLD
        elif self.total_visits >= 10:
            return ClienteStatus.SILVER
        elif self.total_visits >= 5:
            return ClienteStatus.BRONZE
        return ClienteStatus.REGULAR
    
    def record_visit(self):
        self.total_visits += 1
        self.last_visit = timezone.now()
        self.save()


class Fila(models.Model):
    """ORM model for EntradaFila that implements domain.entities.EntradaFila interface"""
    STATUS_CHOICES = [
        ('waiting', 'Waiting'),
        ('in_progress', 'In Progress'),
        ('completed', 'Completed'),
        ('cancelled', 'Cancelled')
    ]
    
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    barbearia = models.ForeignKey(Barbearia, on_delete=models.CASCADE)
    cliente = models.ForeignKey(Cliente, on_delete=models.CASCADE)
    servico = models.ForeignKey(Servico, on_delete=models.CASCADE)
    barbeiro = models.ForeignKey(
        Barbeiro, 
        on_delete=models.SET_NULL,
        null=True, blank=True,
        related_name='atendimentos'
    )
    barbeiro_preferido = models.ForeignKey(
        Barbeiro,
        on_delete=models.SET_NULL,
        null=True, blank=True,
        related_name='clientes_preferenciais'
    )
    status = models.CharField(max_length=20, choices=STATUS_CHOICES, default='waiting')
    prioridade = models.IntegerField(
        choices=[(p.value, p.name) for p in FilaPrioridade],
        default=FilaPrioridade.NORMAL.value
    )
    horario_chegada = models.DateTimeField(auto_now_add=True)
    horario_atendimento = models.DateTimeField(null=True, blank=True)
    horario_finalizacao = models.DateTimeField(null=True, blank=True)
    estimativa_duracao = models.IntegerField(default=0)
    position_number = models.IntegerField(default=0)
    notified = models.BooleanField(default=False)
    observacoes = models.TextField(blank=True, null=True)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    started_at = models.DateTimeField(null=True, blank=True)
    completed_at = models.DateTimeField(null=True, blank=True)
    estimated_wait_time = models.IntegerField(default=0, help_text='Estimated wait time in minutes')
    
    def __str__(self):
        return f"{self.cliente.nome} - {self.servico.nome}"
    
    def save(self, *args, **kwargs):
        if not self.estimated_wait_time:
            self.estimated_wait_time = self.servico.get_median_wait_time()
        super().save(*args, **kwargs)
    
    def assign_barber(self, barbeiro):
        """Assign a barber to this queue entry"""
        if self.status != 'waiting':
            raise ValueError("Can only assign barber to waiting entries")
        if self.position_number != 0:
            raise ValueError("Can only assign barber to first position")
            
        self.barbeiro = barbeiro
        self.status = 'in_progress'
        self.started_at = timezone.now()
        self.save()
        
        # Update barber status
        barbeiro.status = BarbeiroStatus.BUSY.value
        barbeiro.status_since = timezone.now()
        barbeiro.save()
    
    def complete_service(self):
        """Mark the service as completed"""
        if self.status != 'in_progress':
            raise ValueError("Can only complete in-progress services")
            
        self.status = 'completed'
        self.completed_at = timezone.now()
        self.save()
        
        # Update barber status
        if self.barbeiro:
            self.barbeiro.status = BarbeiroStatus.AVAILABLE.value
            self.barbeiro.status_since = timezone.now()
            self.barbeiro.save()
        
        # Update queue positions
        Fila.objects.filter(
            barbearia=self.barbearia,
            status='waiting',
            position_number__gt=self.position_number
        ).update(position_number=F('position_number') - 1)
    
    def get_wait_time(self):
        """Get current wait time in minutes"""
        if self.status != 'waiting':
            return 0
        return (timezone.now() - self.created_at).total_seconds() / 60
    
    def finalizar_atendimento(self):
        """Mark service as completed and update associated records"""
        self.status = FilaStatus.FINALIZADO.value
        self.horario_finalizacao = timezone.now()
        
        # Record client visit if possible
        try:
            self.cliente.record_visit()
        except Exception as e:
            logger.warning(f"Could not record visit for client {self.cliente.id}: {str(e)}")
        
        # Increment service popularity if possible
        try:
            self.servico.increment_popularity()
        except Exception as e:
            logger.warning(f"Could not increment popularity for service {self.servico.id}: {str(e)}")
        
        self.save()
    
    def cancelar_atendimento(self):
        self.status = FilaStatus.CANCELADO.value
        self.save()
    
    def marcar_ausente(self):
        self.status = FilaStatus.AUSENTE.value
        self.save()
    
    def get_estimated_wait_time(self) -> int:
        """Calculate estimated wait time in minutes"""
        if self.status != FilaStatus.AGUARDANDO.value:
            return 0
            
        # Get all waiting entries before this one
        earlier_entries = self.barbearia.fila_set.filter(
            status=FilaStatus.AGUARDANDO.value,
            horario_chegada__lte=self.horario_chegada
        ).order_by('horario_chegada')
        
        # Get their durations
        durations = [entry.estimativa_duracao for entry in earlier_entries]
        
        # Get count of active barbers
        active_barbers = QueueManager.get_active_barbers_count(
            list(self.barbearia.barbeiro_set.all())
        )
        
        return WaitTimeCalculator.calculate(durations, active_barbers)
    
    def format_wait_time(self) -> str:
        minutes = self.get_estimated_wait_time()
        return WaitTimeCalculator.format_wait_time(minutes)
    
    def iniciar_atendimento(self, barbeiro=None):
        """Mark service as started and assign a barber"""
        try:
            if self.status != FilaStatus.AGUARDANDO.value:
                return False
                
            self.status = FilaStatus.ATENDIMENTO.value
            self.horario_atendimento = timezone.now()
            
            if barbeiro:
                self.barbeiro = barbeiro
                # Update barber status
                barbeiro.status = BarbeiroStatus.BUSY.value
                barbeiro.save()
            
            self.save()
            
            # Update barbershop wait time cache
            self.barbearia.invalidate_wait_time_cache()
            
            return True
        except Exception as e:
            logger.error(f"Error starting service: {str(e)}")
            return False
    
    class Meta:
        ordering = ['position_number', 'created_at']
