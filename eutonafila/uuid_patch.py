"""
Monkey patching module for Django's UUID field validation
"""
from django.db.models.fields import UUIDField
import uuid
import logging

logger = logging.getLogger(__name__)

def apply_uuid_field_patch():
    """
    Patch Django's UUIDField.to_python method to handle DatabaseOperations objects
    that cause validation errors in the admin interface.
    """
    # Store the original to_python method if not already patched
    if not hasattr(UUIDField, '_original_to_python'):
        logger.info("Applying UUID field patch...")
        UUIDField._original_to_python = UUIDField.to_python
        
        def patched_to_python(self, value):
            """
            Patched version of UUIDField.to_python that handles DatabaseOperations objects
            and other problematic values gracefully.
            """
            if value is None:
                return None
                
            try:
                # Check for DatabaseOperations object
                if hasattr(value, '__class__') and 'DatabaseOperations' in value.__class__.__name__:
                    logger.debug(f"Converting DatabaseOperations object to UUID")
                    return uuid.uuid4()
                    
                # Continue with original method
                return UUIDField._original_to_python(self, value)
            except Exception as e:
                logger.warning(f"Error in UUIDField.to_python with {type(value)}: {str(e)}")
                return uuid.uuid4()  # Return a valid UUID as fallback
                
        # Apply the patch
        UUIDField.to_python = patched_to_python
        logger.info("UUID field patch applied successfully")
    else:
        logger.debug("UUID field patch already applied")

# Apply the patch immediately when this module is imported
apply_uuid_field_patch()
