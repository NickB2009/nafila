"""
Patch Django's UUID converter for SQLite to handle integer values properly
"""
import uuid
import logging

logger = logging.getLogger(__name__)

def safe_convert_uuidfield_value(value, expression, connection):
    """
    A safe version of the SQLite UUID converter that handles integers properly
    """
    if value is None:
        return None
    
    # Handle integer values by converting them to strings
    if isinstance(value, int):
        logger.debug(f"Converting integer {value} to UUID")
        try:
            # Try to use it as a UUID integer representation
            return uuid.UUID(int=value)
        except (ValueError, TypeError):
            # If that fails, convert to string and use as a UUID string
            return uuid.UUID(str(value))
    
    # Original behavior
    try:
        return uuid.UUID(value)
    except (ValueError, TypeError):
        return value

def patch_django_uuid_converter():
    """Patch Django's UUID converter to handle integers safely"""
    from django.db.backends.sqlite3.operations import DatabaseOperations
    
    # Store the original converter for reference
    original_converter = DatabaseOperations.convert_uuidfield_value
    
    # Replace with our safe version
    DatabaseOperations.convert_uuidfield_value = safe_convert_uuidfield_value
    
    logger.info("Successfully patched Django's UUID converter for SQLite")
    return True

if __name__ == "__main__":
    success = patch_django_uuid_converter()
    print(f"Django UUID converter patch applied: {success}") 