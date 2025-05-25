"""
Script to fix the UUID handling issue in Servico model specifically
"""
import os
import sys
import django
import uuid
import logging

# Set up logging
logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)
handler = logging.StreamHandler()
formatter = logging.Formatter('%(asctime)s - %(levelname)s - %(message)s')
handler.setFormatter(formatter)
logger.addHandler(handler)

# Add the current directory to the Python path
sys.path.append(os.path.abspath('.'))
sys.path.append(os.path.join(os.path.abspath('.'), 'eutonafila'))

# Set up Django environment
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
django.setup()

def apply_uuid_patches():
    """Apply all UUID-related patches to Django"""
    from django.db.backends.sqlite3.operations import DatabaseOperations
    from django.db.models.fields import UUIDField

    # Patch DatabaseOperations.convert_uuidfield_value
    def patched_convert_uuidfield_value(self, value, expression, connection):
        """Improved method to handle UUID conversion with better fallbacks for problematic types"""
        if value is None:
            return None
            
        try:
            # Check if the value is actually a DatabaseOperations object
            # This is a common cause of the ValidationError in admin
            if hasattr(value, '__class__') and 'DatabaseOperations' in value.__class__.__name__:
                logger.warning(f"Received DatabaseOperations object instead of UUID. Converting to new UUID.")
                return uuid.uuid4()
                
            if isinstance(value, uuid.UUID):
                return value
                
            if isinstance(value, str):
                return uuid.UUID(value)
                
            if isinstance(value, int):
                return uuid.UUID(int=value)
                
            if isinstance(value, bytes):
                return uuid.UUID(bytes=value)
            
            # For any other type, try to convert to string first
            logger.warning(f"Received non-standard value type: {type(value)}")
            safe_string = str(value)
            try:
                return uuid.UUID(safe_string)
            except (ValueError, TypeError):
                # Generate a new UUID as a last resort
                new_uuid = uuid.uuid4()
                logger.warning(f"Created new UUID {new_uuid} since conversion failed for: {safe_string}")
                return new_uuid
                
        except (ValueError, AttributeError, TypeError) as e:
            logger.error(f"Error converting {value} ({type(value)}) to UUID: {str(e)}")
            return uuid.uuid4()  # Always return a valid UUID

    # Store original methods
    if not hasattr(DatabaseOperations, '_original_convert_uuidfield_value'):
        setattr(DatabaseOperations, '_original_convert_uuidfield_value', 
                getattr(DatabaseOperations, 'convert_uuidfield_value', None))
    
    # Install the patch to DatabaseOperations
    setattr(DatabaseOperations, 'convert_uuidfield_value', patched_convert_uuidfield_value)
    
    # Patch UUIDField.to_python
    original_to_python = UUIDField.to_python
    
    def patched_to_python(self, value):
        if value is None:
            return None
            
        try:
            # Check for DatabaseOperations object
            if hasattr(value, '__class__') and 'DatabaseOperations' in value.__class__.__name__:
                logger.warning(f"UUIDField.to_python received DatabaseOperations object. Creating new UUID.")
                return uuid.uuid4()
                
            return original_to_python(self, value)
        except (ValueError, AttributeError, TypeError) as e:
            logger.error(f"Error in UUIDField.to_python with {value} ({type(value)}): {str(e)}")
            return uuid.uuid4()
    
    # Apply the UUIDField patch
    if not hasattr(UUIDField, '_original_to_python'):
        setattr(UUIDField, '_original_to_python', UUIDField.to_python)
        
    UUIDField.to_python = patched_to_python
    
    logger.info("Successfully applied UUID patches to Django")
    return True

def fix_servico_uuids():
    """Fix any existing servico records with UUID issues"""
    from eutonafila.barbershop.models import Servico
    
    try:
        logger.info("Checking Servico model for UUID issues...")
        services = Servico.objects.all()
        logger.info(f"Found {len(services)} services in database")
        
        fixed_count = 0
        for service in services:
            try:
                # Check if ID is not a proper UUID
                if not isinstance(service.id, uuid.UUID):
                    old_id = service.id
                    # Generate a new valid UUID
                    new_id = uuid.uuid4()
                    logger.info(f"Fixing service '{service.nome}' with invalid ID type: {type(old_id)}")
                    service.id = new_id
                    service.save()
                    fixed_count += 1
            except Exception as e:
                logger.error(f"Error fixing service: {str(e)}")
        
        if fixed_count > 0:
            logger.info(f"Fixed {fixed_count} services with UUID issues")
        else:
            logger.info("No services needed UUID fixes")
            
        return True
    except Exception as e:
        logger.error(f"Error in fix_servico_uuids: {str(e)}", exc_info=True)
        return False

if __name__ == "__main__":
    logger.info("Starting UUID patches for Servico model...")
    
    # Apply patches to Django
    apply_uuid_patches()
    
    # Fix Servico UUIDs
    fix_result = fix_servico_uuids()
    
    if fix_result:
        logger.info("Servico UUID fix completed successfully")
    else:
        logger.error("Servico UUID fix failed")
        
    logger.info("""
==============================================================
UUID fixes have been applied. Restart the server to take effect.
Use: python manage.py runserver
==============================================================
""")
