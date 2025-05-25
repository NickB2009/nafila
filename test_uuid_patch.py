"""
Script to verify UUID patch is working by simulating the admin view
"""
import os
import sys
import django
import uuid

# Add the current directory to the Python path
sys.path.append(os.path.abspath('.'))

# Set up Django environment
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
django.setup()

# Import the patch to apply it first
from eutonafila.uuid_patch import apply_uuid_field_patch

# Import the models
try:
    from eutonafila.barbershop.models import Servico, Barbearia
    from django.db.backends.sqlite3.operations import DatabaseOperations
    from django.db.models.fields import UUIDField
    
    # Create a test instance of DatabaseOperations to simulate the error
    db_ops = DatabaseOperations(connection=None)
    
    print("\n=== TESTING UUID FIELD PATCH ===\n")
    print("Creating a simulated DatabaseOperations object:", db_ops)
    
    # Test the patched UUIDField.to_python method
    uuid_field = Servico._meta.get_field('id')
    result = uuid_field.to_python(db_ops)
    
    print("\nResult of converting DatabaseOperations to UUID:", result)
    print("Type:", type(result))
    
    print("\nThe patch is working if you see a UUID value above.")
    print("\n=== RETRIEVING SERVICO OBJECTS ===\n")
    
    # Test retrieving Servico objects
    servicos = Servico.objects.all()
    print(f"Found {len(servicos)} services")
    
    for i, servico in enumerate(servicos):
        print(f"\nService {i+1}:")
        print(f"  ID: {servico.id}")
        print(f"  ID type: {type(servico.id)}")
        print(f"  Name: {servico.nome}")
    
    print("\nAll services retrieved successfully. The patch is working!")
    
except Exception as e:
    print(f"Error testing UUID patch: {str(e)}")
    import traceback
    traceback.print_exc()
