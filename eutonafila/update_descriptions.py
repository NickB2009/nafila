import os
import django

# Set up Django environment
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
django.setup()

# Import models after Django setup
from barbershop.models import Barbearia

def update_descriptions():
    """Update all barbershops with a default description if not set"""
    barbershops = Barbearia.objects.all()
    
    print(f"Found {len(barbershops)} barbershops")
    
    for b in barbershops:
        print(f"Processing: {b.nome} (ID: {b.id})")
        print(f"  Current descricao_curta: '{b.descricao_curta}'")
        
        # Force set the description regardless of current value
        b.descricao_curta = "Barbearia especializada em cuidados masculinos. Venha nos conhecer!"
        b.save()
        
        # Verify it was saved properly
        b_check = Barbearia.objects.get(id=b.id)
        print(f"  Updated descricao_curta: '{b_check.descricao_curta}'")
        print("-" * 40)

if __name__ == "__main__":
    print("Updating barbershop descriptions...")
    update_descriptions()
    print("Update complete.") 