from django.core.management.base import BaseCommand
from django.db import transaction
from django.core.cache import cache
from barbershop.models import Barbeiro, Fila
from domain.domain_models import Barbeiro as DomainBarbeiro
from domain.domain_models import EntradaFila as DomainEntradaFila


class Command(BaseCommand):
    help = 'Updates all status fields in database to use new domain constants'

    def handle(self, *args, **options):
        self.stdout.write(self.style.SUCCESS('Starting status constants update...'))
        
        # Clear all caches first
        self.stdout.write('Clearing cache...')
        cache.clear()
        
        # Update barber statuses
        with transaction.atomic():
            self.stdout.write('Updating barber statuses...')
            
            # Status mapping from old to new
            barber_status_map = {
                'disponivel': DomainBarbeiro.Status.STATUS_AVAILABLE.value,
                'ocupado': DomainBarbeiro.Status.STATUS_BUSY.value,
                'pausa': DomainBarbeiro.Status.STATUS_BREAK.value,
                'ausente': DomainBarbeiro.Status.STATUS_OFFLINE.value
            }
            
            # Update each status
            for old_status, new_status in barber_status_map.items():
                count = Barbeiro.objects.filter(status=old_status).update(status=new_status)
                self.stdout.write(f'  - Updated {count} barbers from "{old_status}" to "{new_status}"')
        
        # Update queue entry statuses
        with transaction.atomic():
            self.stdout.write('Updating queue entry statuses...')
            
            # Status mapping from old to new
            queue_status_map = {
                'aguardando': DomainEntradaFila.Status.STATUS_AGUARDANDO.value,
                'em_atendimento': DomainEntradaFila.Status.STATUS_ATENDIMENTO.value,
                'finalizado': DomainEntradaFila.Status.STATUS_FINALIZADO.value,
                'cancelado': DomainEntradaFila.Status.STATUS_CANCELADO.value,
                'ausente': DomainEntradaFila.Status.STATUS_AUSENTE.value
            }
            
            # Update each status
            for old_status, new_status in queue_status_map.items():
                count = Fila.objects.filter(status=old_status).update(status=new_status)
                self.stdout.write(f'  - Updated {count} queue entries from "{old_status}" to "{new_status}"')
        
        self.stdout.write(self.style.SUCCESS('Status constants update complete!'))
        self.stdout.write('Run the command below to apply the migration:')
        self.stdout.write('  python manage.py migrate') 