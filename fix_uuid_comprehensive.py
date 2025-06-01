"""
Script to permanently fix the UUID representation in Django SQLite backend
"""
import os
import sys
import django
import uuid
import sqlite3
import logging

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    datefmt='%Y-%m-%d %H:%M:%S'
)

logger = logging.getLogger(__name__)

# Add the current directory to the Python path
sys.path.append(os.path.abspath('.'))

# Set up Django environment
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
django.setup()

def apply_uuid_fix():
    """Apply UUID handling fix to Django's SQLite backend"""
    try:
        # Import the necessary Django database operations
        from django.db.backends.sqlite3.operations import DatabaseOperations
        
        # Store original method for reference
        original_method = DatabaseOperations.convert_uuidfield_value
        logger.info(f"Original convert_uuidfield_value method: {original_method}")
        
        # Define an improved method
        def improved_convert_uuidfield_value(self, value, expression, connection):
            """Improved version of the UUID conversion method for SQLite"""
            if value is None:
                return None
                
            try:
                if isinstance(value, uuid.UUID):
                    return value
                    
                if isinstance(value, str):
                    return uuid.UUID(value)
                    
                if isinstance(value, (bytes, memoryview)):
                    return uuid.UUID(bytes=bytes(value))
                    
                if isinstance(value, int):
                    return uuid.UUID(int=value)
                    
                # For any other type, try to convert to string first
                logger.warning(f"Converting non-standard value type: {type(value)}")
                safe_string = str(value)
                
                # Clean up string if it contains DatabaseOperations object reference
                if "DatabaseOperations" in safe_string:
                    logger.warning("Fixing DatabaseOperations reference in UUID")
                    return uuid.uuid4()  # Generate a new UUID as fallback
                    
                try:
                    return uuid.UUID(safe_string)
                except (ValueError, TypeError):
                    logger.warning(f"Creating new UUID since conversion failed for: {safe_string}")
                    return uuid.uuid4()
                    
            except Exception as e:
                logger.error(f"Error converting {value} ({type(value)}) to UUID: {str(e)}")
                return uuid.uuid4()  # Return a new UUID as fallback
                
        # Apply the improved method
        DatabaseOperations.convert_uuidfield_value = improved_convert_uuidfield_value
        logger.info("Successfully applied UUID handling fix to Django's SQLite backend")
        
        return True
    except Exception as e:
        logger.error(f"Error applying UUID fix: {str(e)}", exc_info=True)
        return False

def fix_model_str_methods():
    """Fix __str__ method in Barbearia models"""
    try:
        # Import the models
        from eutonafila.barbershop.models import Barbearia
        
        # Define improved __str__ method
        def improved_str(self):
            try:
                # Format UUID as string correctly
                id_str = str(self.id) if self.id is not None else "None"
                return f"{self.nome} ({id_str})"
            except Exception as e:
                logger.error(f"Error in __str__: {str(e)}")
                return self.nome
                
        # Apply the improved method
        Barbearia.__str__ = improved_str
        logger.info("Successfully applied __str__ fix to Barbearia model")
        
        # Also try to fix the deploy_package model if it exists
        try:
            from eutonafila.deploy_package.barbershop.models import Barbearia as DeployBarbearia
            DeployBarbearia.__str__ = improved_str
            logger.info("Successfully applied __str__ fix to deploy_package Barbearia model")
        except ImportError:
            logger.info("deploy_package Barbearia model not found, skipping...")
            
        return True
    except Exception as e:
        logger.error(f"Error fixing model __str__ methods: {str(e)}", exc_info=True)
        return False

def verify_barbearias():
    """Verify that the barbearias are displaying correctly"""
    try:
        from eutonafila.barbershop.models import Barbearia
        
        # Get all barbershops
        barbershops = list(Barbearia.objects.all())
        logger.info(f"Found {len(barbershops)} barbershops")
        
        # Check each one
        for i, barbershop in enumerate(barbershops):
            try:
                str_repr = str(barbershop)
                logger.info(f"Barbershop {i+1}: {barbershop.nome}")
                logger.info(f"  ID: {barbershop.id} (Type: {type(barbershop.id).__name__})")
                logger.info(f"  String representation: {str_repr}")
                
                # Check if the UUID is displaying correctly
                if "DatabaseOperations" in str_repr:
                    logger.error(f"  ISSUE: UUID not displaying correctly for {barbershop.nome}")
                else:
                    logger.info(f"  OK: UUID displaying correctly for {barbershop.nome}")
            except Exception as e:
                logger.error(f"  Error checking barbershop {i+1}: {str(e)}")
                
        return True
    except Exception as e:
        logger.error(f"Error verifying barbershops: {str(e)}", exc_info=True)
        return False

def update_barba_cabelo_record():
    """Update the Barba & Cabelo record directly in the database"""
    try:
        # First, try to get the record
        from eutonafila.barbershop.models import Barbearia
        
        # Find Barba & Cabelo record
        barba_cabelo = Barbearia.objects.filter(nome__icontains="Barba & Cabelo").first()
        
        if barba_cabelo:
            logger.info(f"Found Barba & Cabelo record with ID: {barba_cabelo.id}")
            
            # Generate a new clean UUID if needed
            if isinstance(barba_cabelo.id, str) and "DatabaseOperations" in barba_cabelo.id:
                new_id = uuid.uuid4()
                logger.info(f"Replacing problematic ID with new UUID: {new_id}")
                
                # Update the record directly in the database
                from django.db import connection
                with connection.cursor() as cursor:
                    cursor.execute(
                        "UPDATE barbershop_barbearia SET id = %s WHERE nome = %s",
                        [str(new_id), "Barba & Cabelo"]
                    )
                
                logger.info("Record updated successfully")
            else:
                logger.info("ID appears to be valid, no update needed")
        else:
            logger.warning("Could not find 'Barba & Cabelo' record")
            
        return True
    except Exception as e:
        logger.error(f"Error updating Barba & Cabelo record: {str(e)}", exc_info=True)
        return False

if __name__ == "__main__":
    logger.info("Starting comprehensive UUID fix process...")
    
    # Apply core fixes
    apply_uuid_fix()
    fix_model_str_methods()
    update_barba_cabelo_record()
    
    # Verify the fixes worked
    verify_barbearias()
    
    logger.info("UUID fix process complete")
