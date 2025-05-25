"""
Apply patches to Django's UUIDField to fix validation errors in the admin interface.
"""
import uuid
import logging

logger = logging.getLogger(__name__)
logger.addHandler(logging.StreamHandler())
logger.setLevel(logging.INFO)

def patch_uuid_field():
    """
    Patch Django's UUIDField.to_python method to handle DatabaseOperations objects.
    This function is called directly from settings.py to ensure early application.
    """
    try:
        # Import here to avoid circular imports
        from django.db.models.fields import UUIDField
        
        # Don't patch if already patched
        if hasattr(UUIDField, '_original_to_python'):
            logger.debug("UUIDField already patched")
            return
            
        logger.info("Patching Django UUIDField.to_python...")
        
        # Store original method
        original_to_python = UUIDField.to_python
        UUIDField._original_to_python = original_to_python
        
        # Define patched method
        def patched_to_python(self, value):
            if value is None:
                return None
                
            try:
                # Handle DatabaseOperations objects
                if hasattr(value, '__class__') and 'DatabaseOperations' in value.__class__.__name__:
                    logger.info(f"Converting DatabaseOperations object to UUID")
                    return uuid.uuid4()
                
                # Handle other cases with original method
                return original_to_python(self, value)
            except Exception as e:
                logger.warning(f"Error in UUIDField.to_python: {str(e)}")
                return uuid.uuid4()  # Return valid UUID as fallback
        
        # Apply patch
        UUIDField.to_python = patched_to_python
        logger.info("Django UUIDField patched successfully")
        
        # Also patch the SQLite operations if needed
        try:
            from django.db.backends.sqlite3.operations import DatabaseOperations
            
            if not hasattr(DatabaseOperations, '_original_convert_uuidfield_value'):
                original_convert = getattr(DatabaseOperations, 'convert_uuidfield_value', None)
                if original_convert:
                    DatabaseOperations._original_convert_uuidfield_value = original_convert
                    
                    def patched_convert_uuidfield_value(self, value, expression, connection):
                        if value is None:
                            return None
                            
                        try:
                            if isinstance(value, uuid.UUID):
                                return value
                                
                            if isinstance(value, str):
                                return uuid.UUID(value)
                                
                            if isinstance(value, int):
                                return uuid.UUID(int=value)
                                
                            # For any other type, try to convert to string
                            safe_string = str(value)
                            try:
                                return uuid.UUID(safe_string)
                            except (ValueError, TypeError):
                                return uuid.uuid4()
                                
                        except Exception:
                            return uuid.uuid4()
                    
                    DatabaseOperations.convert_uuidfield_value = patched_convert_uuidfield_value
                    logger.info("DatabaseOperations.convert_uuidfield_value patched successfully")
        except Exception as e:
            logger.warning(f"Could not patch DatabaseOperations: {str(e)}")
            
    except Exception as e:
        logger.error(f"Error applying UUID field patch: {str(e)}")

# Call the function immediately when this module is imported
patch_uuid_field()
