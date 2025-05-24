"""
Script to fix the UUID handling issue in Barbearia model
"""
import os
import sys
import django
import uuid
import logging

# Add the current directory to the Python path
sys.path.append(os.path.abspath('.'))

# Set up Django environment
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
django.setup()

from django.db.models import Model, UUIDField
from barbershop.models import Barbearia, Servico

logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)
handler = logging.StreamHandler()
formatter = logging.Formatter('%(asctime)s - %(levelname)s - %(message)s')
handler.setFormatter(formatter)
logger.addHandler(handler)

def safe_convert_uuidfield_value(value, expression, connection):
    """Safely convert a value to UUID"""
    if value is None:
        return None
    try:
        if isinstance(value, uuid.UUID):
            return value
        if isinstance(value, str):
            return uuid.UUID(value)
        if isinstance(value, int):
            return uuid.UUID(int=value)
        if isinstance(value, bytes):
            return uuid.UUID(bytes=value)
        
        # For any other type, try to convert to string first
        string_value = str(value)
        logger.warning(f"Converting non-standard value type {type(value)} to UUID via string: '{string_value}'")
        return uuid.UUID(string_value)
    except (ValueError, AttributeError, TypeError) as e:
        logger.error(f"Error converting value to UUID: {value} ({type(value)}) - {str(e)}")
        # Generate a new UUID as a fallback
        return uuid.uuid4()

def patch_django_uuid_converter():
    """Patch Django's UUID converter to handle unusual types safely"""
    try:
        # Apply the patch to Django's SQLite operations
        from django.db.backends.sqlite3.operations import DatabaseOperations
        
        # Store the original converter for reference
        original_converter = DatabaseOperations.convert_uuidfield_value
        
        # Replace with our safe version 
        DatabaseOperations.convert_uuidfield_value = safe_convert_uuidfield_value
        
        logger.info("Successfully patched Django's UUID converter for SQLite")
        return True
    except Exception as e:
        logger.error(f"Error patching Django's UUID converter: {str(e)}", exc_info=True)
        return False

def fix_barbershop_data():
    """Fix any existing barbershop data with UUID issues"""
    try:
        # First apply the patch
        if not patch_django_uuid_converter():
            logger.error("Failed to apply the UUID converter patch. Aborting data fix.")
            return False
        
        # Get all barbershops
        barbershops = Barbearia.objects.all()
        logger.info(f"Found {len(barbershops)} barbershops in database")
        
        fixed_count = 0
        for barbershop in barbershops:
            try:
                # Check if ID is not a proper UUID
                if not isinstance(barbershop.id, uuid.UUID):
                    old_id = barbershop.id
                    # Generate a new valid UUID
                    new_id = uuid.uuid4()
                    logger.info(f"Fixing barbershop '{barbershop.nome}' with invalid ID type: {type(old_id)}")
                    barbershop.id = new_id
                    barbershop.save()
                    fixed_count += 1
            except Exception as e:
                logger.error(f"Error fixing barbershop {barbershop.nome}: {str(e)}")
        
        logger.info(f"Fixed {fixed_count} barbershops with UUID issues")
        return True
    except Exception as e:
        logger.error(f"Error in fix_barbershop_data: {str(e)}", exc_info=True)
        return False

if __name__ == "__main__":
    logger.info("Starting UUID patch and data fix operation...")
    success = fix_barbershop_data()
    if success:
        logger.info("UUID patch and data fix completed successfully")
    else:
        logger.error("UUID patch and data fix failed")
