"""
Simple script to check if the Barbearia model's UUID display issue is fixed
"""
import os
import sys
import django

# Add the current directory to the Python path
sys.path.append(os.path.abspath('.'))

# Set up Django environment
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
django.setup()

def check_barbearias():
    """Check all barbearias for UUID display issues"""
    
    # Import the model
    try:
        from eutonafila.barbershop.models import Barbearia
        
        # Get all barbershops
        barbearias = Barbearia.objects.all()
        
        print(f"Found {len(barbearias)} barbershops:")
        
        for i, barb in enumerate(barbearias):
            print(f"\n{i+1}. {barb.nome}")
            print(f"   ID: {barb.id} (Type: {type(barb.id).__name__})")
            print(f"   String representation: {str(barb)}")
            
            # Check specifically for 'Barba & Cabelo'
            if "Barba & Cabelo" in barb.nome:
                print("   This is the 'Barba & Cabelo' record we're concerned about")
                
                # Check if database operations object appears in string representation
                if "DatabaseOperations" in str(barb):
                    print("   ERROR: DatabaseOperations object is still showing in the string representation")
                else:
                    print("   SUCCESS: UUID is displaying correctly now!")
        
    except Exception as e:
        print(f"Error: {str(e)}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    print("Checking Barbearia records for UUID display issues...")
    check_barbearias()
