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
        
        try:
            # Get all barbershops
            barbershops = Barbearia.objects.all()
            
            if not barbershops.exists():
                self.stdout.write(self.style.WARNING('No barbershops found.'))
                return
            
            for barbershop in barbershops:
                self.ensure_services(barbershop)
            
            self.stdout.write(self.style.SUCCESS('Successfully ensured services for all barbershops!'))
        except Exception as e:
            self.stdout.write(self.style.ERROR(f'Error: {str(e)}'))
            logger.exception("Error in ensure_services command")
    
    def ensure_services(self, barbershop):
        """Ensure the barbershop has all default services"""
        try:
            # Ensure barbershop nome is a string to prevent 'replace' errors
            # This is a defensive check for all string fields
            barbershop_name = str(barbershop.nome) if barbershop.nome is not None else ""
            
            existing_services = Servico.objects.filter(barbearia=barbershop)
            existing_service_names = []
            
            # Safely get service names, ensuring we convert any non-string values
            for service in existing_services:
                if service.nome is not None:
                    existing_service_names.append(str(service.nome))
            
            # Log current status
            self.stdout.write(f'Barbershop: {barbershop_name} has {len(existing_service_names)} services')
            
            # Check for missing services
            services_created = 0
            for service_data in DEFAULT_SERVICES:
                service_name = str(service_data['nome']) if service_data['nome'] is not None else ""
                
                if service_name not in existing_service_names:
                    # Create the missing service
                    Servico.objects.create(
                        barbearia=barbershop,
                        nome=service_name,
                        preco=service_data['preco'],
                        duracao=service_data['duracao'],
                        descricao=str(service_data['descricao']) if service_data['descricao'] is not None else ""
                    )
                    services_created += 1
                    self.stdout.write(f'  - Created service: {service_name}')
            
            if services_created > 0:
                self.stdout.write(self.style.SUCCESS(f'  Added {services_created} missing services to {barbershop_name}'))
            else:
                self.stdout.write(f'  All default services already exist for {barbershop_name}')
        except Exception as e:
            self.stdout.write(self.style.ERROR(f'Error processing barbershop: {str(e)}'))
            logger.exception(f"Error ensuring services for barbershop {getattr(barbershop, 'id', 'unknown')}") 