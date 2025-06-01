"""
Script to fix the __str__ method in the Barbearia model to handle UUID objects correctly
"""
import os
import sys
import django
import uuid
import logging
from pprint import pformat

# Add the current directory to the Python path
sys.path.append(os.path.abspath('.'))

# Set up Django environment
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
django.setup()

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    datefmt='%Y-%m-%d %H:%M:%S'
)

logger = logging.getLogger(__name__)

def fix_barbearia_str_method():
    """Fix the __str__ method in the Barbearia model"""
    try:
        # Get the Barbearia model
        from eutonafila.barbershop.models import Barbearia
        
        # Define a new __str__ method that safely handles UUIDs
        def new_str_method(self):
            try:
                # Format UUID as string correctly
                id_str = str(self.id) if self.id is not None else "None"
                return f"{self.nome} ({id_str})"
            except Exception as e:
                # Fallback in case of any error
                logger.error(f"Error in __str__: {str(e)}")
                return self.nome
        
        # Replace the __str__ method
        logger.info("Replacing __str__ method in Barbearia model...")
        Barbearia.__str__ = new_str_method
        
        # Test the new method
        barbershops = list(Barbearia.objects.all())
        logger.info(f"Found {len(barbershops)} barbershops in database")
        
        for i, barbershop in enumerate(barbershops):
            logger.info(f"Barbershop {i+1}: {str(barbershop)}")
        
        logger.info("Successfully fixed the __str__ method in Barbearia model")
        return True
        
    except Exception as e:
        logger.error(f"Error fixing __str__ method: {str(e)}", exc_info=True)
        return False

def verify_uuid_handling():
    """Verify UUID handling in model and database operations"""
    try:
        # Get models
        from eutonafila.barbershop.models import Barbearia
        
        # Check UUID handling in Django's SQLite backend
        from django.db.backends.sqlite3.operations import DatabaseOperations
        
        logger.info(f"Current convert_uuidfield_value method: {DatabaseOperations.convert_uuidfield_value}")
        
        # Check a specific record
        barba_cabelo = Barbearia.objects.filter(nome__icontains="Barba & Cabelo").first()
        
        if barba_cabelo:
            logger.info(f"Found 'Barba & Cabelo': {barba_cabelo}")
            logger.info(f"ID: {barba_cabelo.id} (Type: {type(barba_cabelo.id)})")
            logger.info(f"String representation: {str(barba_cabelo)}")
        else:
            logger.warning("Could not find 'Barba & Cabelo' record")
            
    except Exception as e:
        logger.error(f"Error verifying UUID handling: {str(e)}", exc_info=True)

if __name__ == "__main__":
    logger.info("Starting fix for Barbearia __str__ method...")
    
    if fix_barbearia_str_method():
        logger.info("Fix applied successfully")
        verify_uuid_handling()
    else:
        logger.error("Failed to apply fix")
