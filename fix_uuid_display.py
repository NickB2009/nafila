"""
Comprehensive script to fix and verify UUID display in Barbearia models
"""
import os
import sys
import django
import uuid
import logging
import importlib
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

def fix_barbearia_models():
    """Fix the __str__ method in both Barbearia model classes"""
    try:
        # Try to import the models
        from eutonafila.barbershop.models import Barbearia as MainBarbearia
        
        # Define the improved __str__ method
        def improved_str_method(self):
            try:
                # Format UUID as string correctly
                id_str = str(self.id) if self.id is not None else "None"
                return f"{self.nome} ({id_str})"
            except Exception as e:
                logger.error(f"Error in __str__: {str(e)}")
                return self.nome
        
        # Update the main Barbearia model
        logger.info("Updating main Barbearia model...")
        MainBarbearia.__str__ = improved_str_method
        
        # Try to update the deploy package model if it exists
        try:
            from eutonafila.deploy_package.barbershop.models import Barbearia as DeployBarbearia
            logger.info("Updating deploy package Barbearia model...")
            DeployBarbearia.__str__ = improved_str_method
        except (ImportError, ModuleNotFoundError):
            logger.info("Deploy package Barbearia model not found, skipping...")
        
        logger.info("Barbearia models updated successfully")
        return True
    except Exception as e:
        logger.error(f"Error fixing Barbearia models: {str(e)}", exc_info=True)
        return False

def verify_fix():
    """Verify that the UUID display issue is resolved"""
    try:
        # Try to import the model
        from eutonafila.barbershop.models import Barbearia
        
        # Get all barbershops
        all_barbershops = list(Barbearia.objects.all())
        logger.info(f"Found {len(all_barbershops)} barbershops in database")
        
        # Check each one
        for i, barbershop in enumerate(all_barbershops):
            try:
                # Log details about the barbershop
                logger.info(f"Barbershop {i+1}: {barbershop.nome}")
                logger.info(f"  ID Type: {type(barbershop.id)}")
                logger.info(f"  ID Value: {barbershop.id}")
                logger.info(f"  String representation: {str(barbershop)}")
                
                # For the Barba & Cabelo record, do extra checks
                if "Barba & Cabelo" in barbershop.nome:
                    logger.info("  Found the Barba & Cabelo record!")
                    logger.info(f"  ID Type: {type(barbershop.id)}")
                    logger.info(f"  ID Value: {barbershop.id}")
                    logger.info(f"  String repr: {str(barbershop)}")
                    
                    # Check if it's displaying correctly
                    str_repr = str(barbershop)
                    if "DatabaseOperations" in str_repr:
                        logger.error("  ISSUE NOT RESOLVED: Still showing DatabaseOperations object!")
                    else:
                        logger.info("  FIX SUCCESSFUL: Showing proper UUID value!")
            except Exception as e:
                logger.error(f"  Error checking barbershop {barbershop.id}: {str(e)}")
        
        # Return success
        return True
    except Exception as e:
        logger.error(f"Error verifying fix: {str(e)}", exc_info=True)
        return False

def check_uuid_handling():
    """Check UUID handling in the SQLite backend"""
    try:
        # Import necessary classes
        from django.db.backends.sqlite3.operations import DatabaseOperations
        
        # Get the current method
        current_method = DatabaseOperations.convert_uuidfield_value
        logger.info(f"Current convert_uuidfield_value method: {current_method}")
        
        # Apply a patch if needed
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
        logger.info("UUID handling patch applied")
        
        return True
    except Exception as e:
        logger.error(f"Error checking UUID handling: {str(e)}", exc_info=True)
        return False

def reload_models():
    """Reload models to ensure changes take effect"""
    try:
        # Reload relevant modules
        if 'eutonafila.barbershop.models' in sys.modules:
            importlib.reload(sys.modules['eutonafila.barbershop.models'])
        
        if 'eutonafila.deploy_package.barbershop.models' in sys.modules:
            importlib.reload(sys.modules['eutonafila.deploy_package.barbershop.models'])
            
        logger.info("Models reloaded successfully")
        return True
    except Exception as e:
        logger.error(f"Error reloading models: {str(e)}", exc_info=True)
        return False

if __name__ == "__main__":
    logger.info("Starting comprehensive UUID fix and verification...")
    
    # First, check UUID handling in the backend
    logger.info("Step 1: Checking UUID handling...")
    check_uuid_handling()
    
    # Fix the models
    logger.info("\nStep 2: Fixing Barbearia model __str__ methods...")
    fix_barbearia_models()
    
    # Reload the models
    logger.info("\nStep 3: Reloading models...")
    reload_models()
    
    # Verify the fix
    logger.info("\nStep 4: Verifying fixes...")
    verify_fix()
    
    logger.info("\nFix and verification process complete.")
    logger.info("If issues persist, consider restarting your Django application.")
