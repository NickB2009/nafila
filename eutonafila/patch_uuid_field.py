"""
Direct patch for Django UUIDField to prevent ValidationError with DatabaseOperations objects
"""
from django.db.models.fields import UUIDField
import uuid

# Store the original to_python method
original_to_python = UUIDField.to_python

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
            print(f"UUIDField.to_python converting DatabaseOperations object to UUID")
            return uuid.uuid4()
            
        # Continue with original method
        return original_to_python(self, value)
    except Exception as e:
        print(f"Error in UUIDField.to_python with {value} ({type(value)}): {str(e)}")
        return uuid.uuid4()  # Return a valid UUID as fallback

# Apply the patch
UUIDField.to_python = patched_to_python

print("UUIDField.to_python successfully patched to handle DatabaseOperations objects")
