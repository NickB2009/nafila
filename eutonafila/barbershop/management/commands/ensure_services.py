from django.core.management.base import BaseCommand
from barbershop.models import Barbearia, Servico
import logging

logger = logging.getLogger(__name__)

DEFAULT_SERVICES = [
    {
        'nome': 'Corte de Cabelo',
        'preco': 30.00,
        'duracao': 30,
        'descricao': 'Corte de cabelo personalizado de acordo com seu estilo.'
    },
    {
        'nome': 'Barba',
        'preco': 25.00,
        'duracao': 20,
        'descricao': 'Modelagem e aparagem da barba com toalha quente e produtos especiais.'
    },
    {
        'nome': 'Corte e Barba',
        'preco': 50.00,
        'duracao': 45,
        'descricao': 'Combinação de corte de cabelo e serviço de barba.'
    },
    {
        'nome': 'Acabamento',
        'preco': 15.00,
        'duracao': 15,
        'descricao': 'Acabamento nas laterais e na nuca para manter o visual entre cortes.'
    }
]

class Command(BaseCommand):
    help = 'Ensures all barbershops have the default services'

    def handle(self, *args, **kwargs):
        self.stdout.write('Checking and ensuring default services for all barbershops...')
        
        # Get all barbershops
        barbershops = Barbearia.objects.all()
        
        if not barbershops.exists():
            self.stdout.write(self.style.WARNING('No barbershops found.'))
            return
        
        for barbershop in barbershops:
            self.ensure_services(barbershop)
        
        self.stdout.write(self.style.SUCCESS('Successfully ensured services for all barbershops!'))
    
    def ensure_services(self, barbershop):
        """Ensure the barbershop has all default services"""
        existing_services = Servico.objects.filter(barbearia=barbershop)
        existing_service_names = [service.nome for service in existing_services]
        
        # Log current status
        self.stdout.write(f'Barbershop: {barbershop.nome} has {len(existing_service_names)} services')
        
        # Check for missing services
        services_created = 0
        for service_data in DEFAULT_SERVICES:
            if service_data['nome'] not in existing_service_names:
                # Create the missing service
                Servico.objects.create(
                    barbearia=barbershop,
                    nome=service_data['nome'],
                    preco=service_data['preco'],
                    duracao=service_data['duracao'],
                    descricao=service_data['descricao']
                )
                services_created += 1
                self.stdout.write(f'  - Created service: {service_data["nome"]}')
        
        if services_created > 0:
            self.stdout.write(self.style.SUCCESS(f'  Added {services_created} missing services to {barbershop.nome}'))
        else:
            self.stdout.write(f'  All default services already exist for {barbershop.nome}') 