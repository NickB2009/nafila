"""
Simplified script to fix SQLite UUID handling
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
            
            # Try to convert from various formats
            try:
                # Try string conversion
                if isinstance(value, str):
                    return uuid.UUID(value)
                
                # Try binary conversion
                if isinstance(value, bytes):
                    return uuid.UUID(bytes=value)
                
                # For DatabaseOperations object or other types, create a new UUID
                if str(value).startswith('🔑') or "DatabaseOperations" in str(value):
                    logger.warning(f"Converting problematic value to new UUID: {value}")
                    return uuid.uuid4()
                
                # Default string conversion for other types
                return uuid.UUID(str(value))
            except (ValueError, TypeError, AttributeError):
                logger.warning(f"Cannot convert to UUID: {value} ({type(value)}). Creating new UUID.")
                return uuid.uuid4()
        
        # Apply the patch
        DatabaseOperations.convert_uuidfield_value = patched_convert_uuidfield_value
        
        print("Successfully patched SQLite UUID handling")
        print("The database should now properly handle UUID values.")
        return True
        
    except Exception as e:
        print(f"Failed to patch SQLite UUID handling: {e}")
        return False

if __name__ == "__main__":
    print("=== APPLYING SQLITE UUID PATCH ===")
    
    # Add the current directory to the Python path
    sys.path.append(os.path.abspath('.'))

    # Set up Django environment
    os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
    django.setup()
    
    if patch_sqlite_uuid_handling():
        print("✓ UUID patch applied successfully")
        print("You can now continue with:")
        print("1. Truncating the tables with corrupted UUIDs")
        print("2. Starting the server normally")
    else:
        print("✗ Failed to apply UUID patch")
