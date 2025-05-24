from django.core.management.base import BaseCommand
from barbershop.models import Barbearia
import json

class Command(BaseCommand):
    help = 'Create a test barbershop'

    def handle(self, *args, **options):
        # Delete any existing test barbershops
        Barbearia.objects.filter(slug='test-barbershop').delete()
        
        # Create a new barbershop with minimal fields
        try:
            self.stdout.write("Creating test barbershop with minimal fields...")
            
            # Try with minimal fields
            barbershop = Barbearia(
                nome='Test Barbershop',
                slug='test-barbershop',
                horario_abertura='09:00',
                horario_fechamento='18:00'
            )
            
            # Check field types
            self.stdout.write(f"Nome type: {type(barbershop.nome)}")
            self.stdout.write(f"Slug type: {type(barbershop.slug)}")
            self.stdout.write(f"Horario abertura type: {type(barbershop.horario_abertura)}")
            self.stdout.write(f"Horario fechamento type: {type(barbershop.horario_fechamento)}")
            
            # Try setting dias_funcionamento to string
            barbershop.dias_funcionamento = "[]"
            self.stdout.write(f"Dias funcionamento (string): {barbershop.dias_funcionamento}")
            
            # Save and see if it works
            barbershop.save(force_insert=True)
            
            self.stdout.write(self.style.SUCCESS(f"Successfully created barbershop: {barbershop.id}"))
            
        except Exception as e:
            self.stdout.write(self.style.ERROR(f"Error creating barbershop: {str(e)}"))
            import traceback
            traceback.print_exc() 