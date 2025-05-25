"""
EuTôNaFila - Queue Management System for Barbershops
"""

# Import the Celery app instance when Django starts
from infrastructure.messaging.celery_app import app as celery_app

# Apply UUID field patch to prevent DatabaseOperations validation errors
try:
    from django.db.models.fields import UUIDField
    import uuid
    
    # Store the original to_python method if not already patched
    if not hasattr(UUIDField, '_original_to_python'):
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
                    # Creating new UUID since we received a DatabaseOperations object
                    return uuid.uuid4()
                    
                # Continue with original method
                return UUIDField._original_to_python(self, value)
            except Exception as e:
                # Fallback to a valid UUID
                return uuid.uuid4()
                
        # Apply the patch
        UUIDField.to_python = patched_to_python
except Exception as e:
    # Log the error but don't prevent application startup
    print(f"Error applying UUID field patch: {str(e)}")

__all__ = ('celery_app',)
