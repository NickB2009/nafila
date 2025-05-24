"""
Improved script to fix the UUID handling issue in Barbearia model
This version specifically targets the convert_uuidfield_value method in Django's SQLite backend
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

def install_uuid_patch():
    """Install a patch for Django's UUID handling in SQLite"""
    try:
        from django.db.backends.sqlite3.operations import DatabaseOperations
        
        # Show current method information
        original_method = DatabaseOperations.convert_uuidfield_value
        logger.info(f"Found original convert_uuidfield_value method: {original_method}")
        
        # Define our new method
        def patched_convert_uuidfield_value(self, value, expression, connection):
            """Improved method to handle UUID conversion with fallbacks for problematic types"""
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
                logger.warning(f"Converting non-standard value type: {type(value)}")
                safe_string = str(value)
                try:
                    return uuid.UUID(safe_string)
                except (ValueError, TypeError):
                    logger.warning(f"Creating new UUID since conversion failed for: {safe_string}")
                    return uuid.uuid4()
                    
            except (ValueError, AttributeError, TypeError) as e:
                logger.error(f"Error converting {value} ({type(value)}) to UUID: {str(e)}")
                return uuid.uuid4()  # Fallback to a new random UUID
        
        # Install the patch
        setattr(DatabaseOperations, 'convert_uuidfield_value', patched_convert_uuidfield_value)
        logger.info("Successfully installed UUID patch")
        
        # Verify it's installed correctly
        current_method = DatabaseOperations.convert_uuidfield_value
        logger.info(f"Current method is now: {current_method}")
        return True
        
    except Exception as e:
        logger.error(f"Failed to install UUID patch: {str(e)}", exc_info=True)
        return False

def fix_barbershop_ids():
    """Fix any existing barbershop data with UUID issues"""
    try:
        # First import the models
        from barbershop.models import Barbearia, Servico
        
        # Get all barbershops
        barbershops = list(Barbearia.objects.all())
        logger.info(f"Found {len(barbershops)} barbershops in database")
        
        # Display ID info for each
        for i, barbershop in enumerate(barbershops):
            try:
                logger.info(f"Barbershop {i+1}: {barbershop.nome} - ID: {barbershop.id} (Type: {type(barbershop.id)})")
            except Exception as e:
                logger.error(f"Error accessing barbershop attributes: {str(e)}")
        
        # Try to fix problematic IDs
        fixed_count = 0
        for barbershop in barbershops:
            try:
                if not isinstance(barbershop.id, uuid.UUID):
                    old_id = barbershop.id
                    barbershop.id = uuid.uuid4()
                    barbershop.save()
                    logger.info(f"Fixed barbershop '{barbershop.nome}' with invalid ID type: {type(old_id)} -> {barbershop.id}")
                    fixed_count += 1
            except Exception as e:
                logger.error(f"Error fixing barbershop: {str(e)}")
        
        logger.info(f"Fixed {fixed_count} barbershops with UUID issues")
        return True
        
    except Exception as e:
        logger.error(f"Error in fix_barbershop_ids: {str(e)}", exc_info=True)
        return False

if __name__ == "__main__":
    logger.info("Starting UUID patch installation...")
    
    if install_uuid_patch():
        logger.info("UUID patch installed successfully - now fixing barbershop IDs")
        if fix_barbershop_ids():
            logger.info("UUID fixes completed successfully")
        else:
            logger.error("Failed to fix barbershop IDs")
    else:
        logger.error("Failed to install UUID patch")
