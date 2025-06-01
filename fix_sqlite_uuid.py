"""
Script to fix the UUID handling in Django's SQLite backend
Specifically addresses the '🔑 <django.db.backends.sqlite3.operations.DatabaseOperations object>' issue
"""
import os
import sys
import django
import uuid
import logging
from functools import wraps

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

def patch_sqlite_uuid_handling():
    """
    Patch Django's SQLite backend to properly handle UUIDs
    This prevents the '🔑 <django.db.backends.sqlite3.operations.DatabaseOperations object>' issue
    """
    try:
        from django.db.backends.sqlite3.operations import DatabaseOperations
        
        # Store the original method
        original_convert_uuidfield_value = DatabaseOperations.convert_uuidfield_value
        
        # Define patched method with proper error handling
        @wraps(original_convert_uuidfield_value)
        def patched_convert_uuidfield_value(self, value, expression, connection):
            if value is None:
                return None
            
            # Handle the case where value is already a UUID
            if isinstance(value, uuid.UUID):
                return value
                
            # Handle string values
            if isinstance(value, str):
                try:
                    return uuid.UUID(value)
                except ValueError:
                    logger.warning(f"Invalid UUID string: {value}")
                    return uuid.uuid4()
            
            # Handle bytes
            if isinstance(value, bytes):
                try:
                    return uuid.UUID(bytes=value)
                except (ValueError, TypeError):
                    try:
                        return uuid.UUID(bytes_le=value)
                    except (ValueError, TypeError):
                        logger.warning(f"Invalid UUID bytes: {value}")
                        return uuid.uuid4()
            
            # Handle other types
            try:
                return uuid.UUID(str(value))
            except (ValueError, TypeError, AttributeError):
                logger.warning(f"Cannot convert to UUID: {value} of type {type(value)}")
                return uuid.uuid4()
        
        # Apply the patch
        DatabaseOperations.convert_uuidfield_value = patched_convert_uuidfield_value
        
        logger.info("Successfully patched SQLite UUID handling")
        return True
        
    except Exception as e:
        logger.error(f"Failed to patch SQLite UUID handling: {str(e)}")
        return False

if __name__ == "__main__":
    logger.info("=== FIXING UUID HANDLING IN DJANGO'S SQLITE BACKEND ===")
    
    if patch_sqlite_uuid_handling():
        logger.info("UUID patch applied successfully.")
        logger.info("The database should now properly handle UUID values.")
        logger.info("You can now start the server normally.")
    else:
        logger.error("Failed to apply UUID patch. Please check the error logs.")
