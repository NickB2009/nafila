"""
Script to check barbershop DB records after the UUID fix
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

# Now we can import models
from barbershop.models import Barbearia, Servico

def check_barbershop_records():
    """Check barbershop records for UUID validity"""
    print("\n=== CHECKING BARBERSHOP RECORDS ===\n")
    barbershops = Barbearia.objects.all()
    print(f"Found {barbershops.count()} barbershops:")
    
    for i, barbershop in enumerate(barbershops):
        print(f"{i+1}. {barbershop.nome}")
        print(f"   ID: {barbershop.id}")
        print(f"   ID Type: {type(barbershop.id)}")
        print(f"   Slug: {barbershop.slug}")
        try:
            print(f"   Working days: {barbershop.dias_funcionamento}")
            print(f"   Hours: {barbershop.horario_abertura} - {barbershop.horario_fechamento}")
            print(f"   Services count: {Servico.objects.filter(barbearia=barbershop).count()}")
        except Exception as e:
            print(f"   Error accessing barbershop data: {str(e)}")
        print("")

if __name__ == "__main__":
    check_barbershop_records()
