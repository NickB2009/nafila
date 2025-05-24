import os
import sys
import django

# Add the current directory to the Python path
sys.path.append(os.path.abspath('.'))

# Set up Django environment
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
django.setup()

# Now we can import Django models
from barbershop.models import Barbearia
from django.contrib.auth.models import User
from django.utils import timezone

def create_test_barbershop():
    """Create a test barbershop with proper data types"""
    # First delete any existing test barbershops with the same slug
    Barbearia.objects.filter(slug='test-barbershop').delete()
    
    # Create a test user if it doesn't exist
    test_user, created = User.objects.get_or_create(
        username='test_barber',
        defaults={
            'email': 'test@example.com',
            'is_staff': True,
            'is_active': True
        }
    )
    if created:
        test_user.set_password('testpassword123')
        test_user.save()
    
    # Create a new barbershop
    try:
        # Note: Django handles JSON serialization for JSONField automatically
        # For SQLite, the field is stored as TEXT so we need to ensure it's a valid JSON string
        barbershop = Barbearia(
            nome='Test Barbershop',
            slug='test-barbershop',
            endereco='123 Main St',
            telefone='(123) 456-7890',
            descricao_curta='A modern barbershop for all your grooming needs',
            cores=['#FF5733', '#33FF57'],  # This will be JSON-serialized by Django
            horario_abertura='09:00',
            horario_fechamento='18:00',
            dias_funcionamento=[0, 1, 2, 3, 4, 5],  # This will be JSON-serialized by Django
            max_capacity=10,
            enable_priority_queue=True,
            user=test_user  # Associate with the test user
        )
        
        # Save without accessing any methods that might trigger errors
        barbershop.save(force_insert=True)
        
        print(f"Successfully created barbershop: {barbershop.id}")
        print(f"Name: {barbershop.nome}")
        print(f"Slug: {barbershop.slug}")
        print(f"Operating days: {barbershop.dias_funcionamento}")
        print(f"Hours: {barbershop.horario_abertura} - {barbershop.horario_fechamento}")
        print(f"Associated with user: {test_user.username}")
        
        return barbershop
        
    except Exception as e:
        print(f"Error creating barbershop: {str(e)}")
        import traceback
        traceback.print_exc()
        return None

if __name__ == '__main__':
    create_test_barbershop() 