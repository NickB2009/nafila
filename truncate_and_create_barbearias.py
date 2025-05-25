import os
import sys
import django
import uuid
from datetime import time, datetime

# Add the current directory to the Python path
sys.path.append(os.path.abspath('.'))

# Set up Django environment - adjust the settings module based on project structure
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
django.setup()

# Now we can import Django models - adjust the import based on actual module structure
try:
    # Try direct import first
    from barbershop.models import Barbearia
except ImportError:
    try:
        # Try with eutonafila prefix
        from eutonafila.barbershop.models import Barbearia
    except ImportError:
        print("Could not import Barbearia model. Trying absolute import...")
        # Print available modules for debugging
        print(f"sys.path = {sys.path}")
        import pkgutil
        print("Available modules:")
        for module in pkgutil.iter_modules():
            print(module.name)
        raise

from django.contrib.auth.models import User
from django.utils import timezone

print("Successfully imported required models")

def truncate_barbearias():
    """Truncate the Barbearias table by deleting all records"""
    count_before = Barbearia.objects.count()
    print(f"Found {count_before} barbershop records before truncating")
    
    # Delete all barbershops
    Barbearia.objects.all().delete()
    
    count_after = Barbearia.objects.count()
    print(f"Deleted all barbershops. Remaining count: {count_after}")

def create_sample_barbearias():
    """Create a few barbershops with clean data and good UUIDs"""
    # Create a test user if it doesn't exist to associate with barbershops
    admin_user, created = User.objects.get_or_create(
        username='admin_barber',
        defaults={
            'email': 'admin@nafila.com',
            'is_staff': True,
            'is_active': True,
            'is_superuser': True
        }
    )
    if created:
        admin_user.set_password('admin123')
        admin_user.save()
        print(f"Created admin user: {admin_user.username}")
    else:
        print(f"Using existing admin user: {admin_user.username}")

    # Sample barbershops data with predefined UUIDs
    barbershops_data = [
        {
            'id': 'a1b2c3d4-e5f6-47a8-b9c0-d1e2f3a4b5c6',
            'nome': 'Barbearia Vintage',
            'slug': 'barbearia-vintage',
            'endereco': 'Rua Augusta, 1234, São Paulo',
            'telefone': '(11) 3456-7890',
            'descricao_curta': 'Barbearia tradicional com toque moderno',
            'cores': ['#2A363B', '#E84A5F'],
            'horario_abertura': '09:00',
            'horario_fechamento': '19:00',
            'dias_funcionamento': [0, 1, 2, 3, 4, 5],  # Monday to Saturday
            'max_capacity': 8,
            'enable_priority_queue': True,
        },
        {
            'id': 'b2c3d4e5-f6a7-48b9-c0d1-e2f3a4b5c6d7',
            'nome': 'Corte & Estilo',
            'slug': 'corte-estilo',
            'endereco': 'Av. Paulista, 1000, São Paulo',
            'telefone': '(11) 2345-6789',
            'descricao_curta': 'Seu estilo, nossa especialidade',
            'cores': ['#45B29D', '#DF5A49'],
            'horario_abertura': '10:00',
            'horario_fechamento': '20:00',
            'dias_funcionamento': [0, 1, 2, 3, 4, 5, 6],  # Monday to Sunday
            'max_capacity': 12,
            'enable_priority_queue': True,
        },
        {
            'id': 'c3d4e5f6-a7b8-49c0-d1e2-f3a4b5c6d7e8',
            'nome': 'Barba & Cabelo',
            'slug': 'barba-cabelo',
            'endereco': 'Rua Oscar Freire, 500, São Paulo',
            'telefone': '(11) 9876-5432',
            'descricao_curta': 'Especialistas em barba e cabelo masculino',
            'cores': ['#334D5C', '#EFC94C'],
            'horario_abertura': '08:30',
            'horario_fechamento': '19:30',
            'dias_funcionamento': [1, 2, 3, 4, 5],  # Tuesday to Saturday
            'max_capacity': 6,
            'enable_priority_queue': False,
        }
    ]

    print("\nCreating sample barbershops...")
    for data in barbershops_data:
        # Convert the string ID to UUID
        uuid_id = uuid.UUID(data['id'])
        
        # Create barbershop
        barbershop = Barbearia(
            id=uuid_id,
            nome=data['nome'],
            slug=data['slug'],
            endereco=data['endereco'],
            telefone=data['telefone'],
            descricao_curta=data['descricao_curta'],
            cores=data['cores'],
            horario_abertura=data['horario_abertura'],
            horario_fechamento=data['horario_fechamento'],
            dias_funcionamento=data['dias_funcionamento'],
            max_capacity=data['max_capacity'],
            enable_priority_queue=data['enable_priority_queue'],
            user=admin_user
        )
        
        # Save with force_insert to ensure we use our provided ID
        barbershop.save(force_insert=True)
        
        print(f"Created barbershop: {barbershop.nome}")
        print(f"  ID: {barbershop.id}")
        print(f"  Slug: {barbershop.slug}")
        print(f"  Hours: {barbershop.horario_abertura} - {barbershop.horario_fechamento}")

def main():
    """Truncate Barbearias table and create new sample data"""
    print("Starting Barbearias table cleanup and recreation...")
    try:
        truncate_barbearias()
        create_sample_barbearias()
        print("\nProcess completed successfully!")
        print(f"Total barbershops now in database: {Barbearia.objects.count()}")
    except Exception as e:
        print(f"Error occurred: {str(e)}")
        import traceback
        traceback.print_exc()

if __name__ == '__main__':
    main()
