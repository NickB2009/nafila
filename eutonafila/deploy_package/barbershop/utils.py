from django.utils import timezone
from domain.models import EntradaFila as DomainEntradaFila
from domain.models import Barbeiro as DomainBarbeiro
from barbershop.models import Barbearia, Servico, Barbeiro, Cliente, Fila

def add_test_queue_data(barbershop_slug, num_clients=3):
    """
    Add test clients to the queue for a specific barbershop
    
    Args:
        barbershop_slug: The slug of the barbershop
        num_clients: Number of clients to add to the queue (0 to clear the queue)
        
    Returns:
        Dictionary with debug information
    """
    try:
        # Get barbershop
        barbershop = Barbearia.objects.get(slug=barbershop_slug)
        
        # Clear the queue if num_clients is 0
        if num_clients == 0:
            Fila.objects.filter(
                barbearia=barbershop,
                status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value
            ).delete()
            
            # Force cache invalidation
            barbershop.invalidate_wait_time_cache()
            
            return {
                'success': True,
                'message': 'Queue cleared successfully',
                'wait_time': 0,
                'wait_time_formatted': 'Sem espera',
                'queue_count': 0,
                'active_barbers': Barbeiro.objects.filter(
                    barbearia=barbershop,
                    status__in=[
                        DomainBarbeiro.Status.STATUS_AVAILABLE.value,
                        DomainBarbeiro.Status.STATUS_BUSY.value
                    ]
                ).count()
            }
        
        # Get or create a service
        service, created = Servico.objects.get_or_create(
            barbearia=barbershop,
            defaults={
                'nome': 'Corte de Cabelo',
                'descricao': 'Corte de cabelo básico',
                'preco': 35.00,
                'duracao': 30
            }
        )
        
        # Create active barbers if none exist
        active_barbers = Barbeiro.objects.filter(
            barbearia=barbershop,
            status__in=[
                DomainBarbeiro.Status.STATUS_AVAILABLE.value,
                DomainBarbeiro.Status.STATUS_BUSY.value
            ]
        )
        
        if not active_barbers.exists():
            # Create at least one active barber
            from django.contrib.auth.models import User
            user, created = User.objects.get_or_create(
                username=f"barber_{barbershop_slug}",
                defaults={'email': f"barber@{barbershop_slug}.com"}
            )
            
            if created:
                user.set_password("password123")
                user.save()
            
            barber, created = Barbeiro.objects.get_or_create(
                barbearia=barbershop,
                usuario=user,
                defaults={
                    'nome': 'John Barber',
                    'status': DomainBarbeiro.Status.STATUS_AVAILABLE.value
                }
            )
        
        # Create clients and add to queue
        for i in range(num_clients):
            # Create client
            client, created = Cliente.objects.get_or_create(
                nome=f"Test Client {i+1}",
                telefone=f"555-000-{1000+i}"
            )
            
            # Add to queue if not already in queue
            if not Fila.objects.filter(
                barbearia=barbershop,
                cliente=client,
                status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value
            ).exists():
                Fila.objects.create(
                    barbearia=barbershop,
                    cliente=client,
                    servico=service,
                    status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value
                )
        
        # Force cache invalidation
        barbershop.invalidate_wait_time_cache()
        
        # Get updated wait time
        wait_time = barbershop.calcular_tempo_espera()
        wait_time_formatted = barbershop.tempo_espera_estimado
        
        # Count clients in queue
        queue_count = Fila.objects.filter(
            barbearia=barbershop,
            status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value
        ).count()
        
        # Update active barbers count
        active_barbers_count = Barbeiro.objects.filter(
            barbearia=barbershop,
            status__in=[
                DomainBarbeiro.Status.STATUS_AVAILABLE.value,
                DomainBarbeiro.Status.STATUS_BUSY.value
            ]
        ).count()
        
        return {
            'success': True,
            'wait_time': wait_time,
            'wait_time_formatted': wait_time_formatted,
            'queue_count': queue_count,
            'active_barbers': active_barbers_count
        }
        
    except Exception as e:
        import traceback
        return {
            'success': False,
            'error': str(e),
            'traceback': traceback.format_exc()
        } 