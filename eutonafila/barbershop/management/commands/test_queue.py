from django.core.management.base import BaseCommand
from django.utils import timezone
import random
import uuid

from domain.models import Cliente, Barbearia, Servico, Barbeiro, EntradaFila
from infrastructure.repositories import (
    DjangoClienteRepository,
    DjangoFilaRepository,
    DjangoBarbeariaRepository,
    DjangoServicoRepository,
    DjangoBarbeiroRepository
)
from application.services import FilaService
from application.dtos import ClienteDTO, CheckInDTO

class Command(BaseCommand):
    help = 'Test the enhanced queue functionality with simulated data'
    
    def add_arguments(self, parser):
        parser.add_argument(
            '--barbearia', 
            type=str, 
            help='Slug of barbershop to test (defaults to first found)'
        )
        parser.add_argument(
            '--clients', 
            type=int, 
            default=5,
            help='Number of clients to add to the queue'
        )
        
    def handle(self, *args, **options):
        # Initialize repositories
        cliente_repository = DjangoClienteRepository()
        fila_repository = DjangoFilaRepository()
        barbearia_repository = DjangoBarbeariaRepository()
        servico_repository = DjangoServicoRepository()
        barbeiro_repository = DjangoBarbeiroRepository()
        
        # Initialize service
        fila_service = FilaService(
            cliente_repository=cliente_repository,
            fila_repository=fila_repository,
            barbearia_repository=barbearia_repository,
            servico_repository=servico_repository,
            barbeiro_repository=barbeiro_repository
        )
        
        # Get barbershop
        if options['barbearia']:
            barbearia = barbearia_repository.get_by_slug(options['barbearia'])
            if not barbearia:
                self.stdout.write(self.style.ERROR(f"Barbershop with slug '{options['barbearia']}' not found"))
                return
        else:
            # Get first barbershop
            barbershops = Barbearia.objects.all()
            if not barbershops:
                self.stdout.write(self.style.ERROR("No barbershops found in database"))
                return
            barbearia = barbershops.first()
        
        self.stdout.write(self.style.SUCCESS(f"Using barbershop: {barbearia.nome}"))
        
        # Get services for barbershop
        servicos = Servico.objects.filter(barbearia=barbearia)
        if not servicos:
            self.stdout.write(self.style.ERROR(f"No services found for barbershop '{barbearia.nome}'"))
            return
        
        # Get or create barbers
        barbers = Barbeiro.objects.filter(barbearia=barbearia)
        if not barbers:
            # Create barbers
            self.stdout.write(self.style.WARNING(f"No barbers found for barbershop '{barbearia.nome}'. Creating test barbers..."))
            barbers = self._create_test_barbers(barbearia, servicos)
        
        # Activate barbers
        for barber in barbers:
            barber.set_status(Barbeiro.STATUS_AVAILABLE)
            self.stdout.write(self.style.SUCCESS(f"Barber {barber.nome} is now {barber.get_status_display()}"))
        
        # Add clients to queue
        num_clients = options['clients']
        self.stdout.write(self.style.SUCCESS(f"Adding {num_clients} clients to the queue..."))
        
        # First clear any existing queue
        EntradaFila.objects.filter(
            barbearia=barbearia,
            status__in=[EntradaFila.STATUS_AGUARDANDO, EntradaFila.STATUS_ATENDIMENTO]
        ).delete()
        
        # Create test clients with different priorities
        created_entries = []
        for i in range(num_clients):
            # Every 4th client is VIP, every 3rd client is returning
            is_vip = (i % 4 == 0)
            is_returning = (i % 3 == 0)
            
            # Create client
            phone_number = f"555{i:07d}"
            cliente = self._get_or_create_client(
                f"Test Client {i+1}",
                phone_number,
                f"client{i+1}@test.com",
                is_vip=is_vip,
                is_returning=is_returning
            )
            
            # Create DTO
            cliente_dto = ClienteDTO(
                nome=cliente.nome,
                telefone=cliente.telefone,
                email=cliente.email or ""
            )
            
            # Random service
            servico = random.choice(servicos)
            
            # Random barber (sometimes None)
            barber = random.choice(barbers) if random.random() > 0.3 else None
            
            # Create check-in DTO
            check_in_dto = CheckInDTO(
                cliente=cliente_dto,
                barbearia_slug=barbearia.slug,
                servico_id=servico.id,
                barbeiro_id=barber.id if barber else None
            )
            
            # Process check-in
            entrada, message = fila_service.check_in(check_in_dto)
            if entrada:
                created_entries.append(entrada)
                priority_text = entrada.get_prioridade_display()
                barber_text = barber.nome if barber else "any barber"
                self.stdout.write(
                    self.style.SUCCESS(
                        f"Added client {cliente.nome} to queue for {servico.nome} with {barber_text}. "
                        f"Priority: {priority_text}"
                    )
                )
            else:
                self.stdout.write(self.style.ERROR(f"Failed to add client {cliente.nome}: {message}"))
        
        # Display the queue
        self.stdout.write("\nCurrent Queue Status:")
        self.stdout.write("=====================")
        
        # Get the queue
        queue_data = fila_service.get_barbershop_queue(barbearia.slug)
        
        # Display waiting clients
        self.stdout.write("\nWaiting Clients:")
        for entry in queue_data['queue']['waiting']:
            self.stdout.write(
                f"Position {entry['position']}: {entry['cliente']} - {entry['servico']} "
                f"(Priority: {entry['prioridade_display']}, Wait: {entry['wait_time']})"
            )
        
        # Display available barbers
        self.stdout.write("\nBarbers:")
        for barber in queue_data['barbers']:
            self.stdout.write(f"{barber['nome']} - {barber['status_display']}")
        
        # Display overall stats
        self.stdout.write(f"\nTotal in queue: {queue_data['queue']['count']}")
        self.stdout.write(f"Estimated wait time: {queue_data['queue']['estimated_wait_time']}")
        
        # Simulate starting service for the first client
        if created_entries:
            next_entry = fila_repository.get_next_in_line(barbearia.id)
            if next_entry:
                # Find an available barber
                available_barber = None
                for barber in barbers:
                    if barber.status == Barbeiro.STATUS_AVAILABLE:
                        available_barber = barber
                        break
                
                self.stdout.write("\nStarting service for next client...")
                success, message = fila_service.start_service(
                    next_entry.id, 
                    available_barber.id if available_barber else None
                )
                
                if success:
                    self.stdout.write(
                        self.style.SUCCESS(
                            f"Started service for {next_entry.cliente.nome} with "
                            f"{available_barber.nome if available_barber else 'no specific barber'}"
                        )
                    )
                else:
                    self.stdout.write(self.style.ERROR(f"Failed to start service: {message}"))
    
    def _get_or_create_client(self, name, phone, email, is_vip=False, is_returning=False):
        """Get or create a test client"""
        try:
            client = Cliente.objects.get(telefone=phone)
        except Cliente.DoesNotExist:
            client = Cliente(
                nome=name,
                telefone=phone,
                email=email,
                is_vip=is_vip
            )
            client.save()
        
        # Update VIP status
        client.is_vip = is_vip
        
        # If returning, add some visit history
        if is_returning and client.total_visits == 0:
            client.total_visits = random.randint(5, 30)
            client.last_visit = timezone.now() - timezone.timedelta(days=random.randint(1, 30))
        
        client.save()
        return client
    
    def _create_test_barbers(self, barbearia, servicos):
        """Create test barbers for the barbershop"""
        names = ["João", "Pedro", "Lucas", "Rafael", "Gabriel"]
        barbers = []
        
        for i, name in enumerate(names):
            barber = Barbeiro(
                nome=f"{name} (Barber)",
                barbearia=barbearia,
                status=Barbeiro.STATUS_OFFLINE
            )
            barber.save()
            
            # Add all services to the barber's specialties
            for servico in servicos:
                barber.especialidades.add(servico)
            
            barbers.append(barber)
            self.stdout.write(self.style.SUCCESS(f"Created barber: {barber.nome}"))
        
        return barbers 