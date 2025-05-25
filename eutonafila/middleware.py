"""
Middleware for monkey patching and other application-wide fixes
"""
import logging
import uuid

logger = logging.getLogger(__name__)

# Import the UUID field patch to apply it at startup
from eutonafila.uuid_patch import apply_uuid_field_patch

class MonkeyPatchMiddleware:
    """
    Apply monkey patches to the application at runtime
    """
    def __init__(self, get_response):
        self.get_response = get_response
        self.is_patched = False
        
        # Apply patches on startup
        self.apply_patches()
    
    def __call__(self, request):
        # If somehow it wasn't patched on startup, try again
        if not self.is_patched:
            self.apply_patches()
        
        # Process the request as normal
        response = self.get_response(request)
        return response
    
    def apply_patches(self):
        """Apply all required monkey patches"""
        # Fix for replace() on non-string enum names
        try:
            self.apply_enum_patches()
            self.apply_django_uuid_patch()
            self.is_patched = True
            logger.info("Successfully applied monkey patches")
            
            # Run ensure_services after fixes are applied
            self.run_ensure_services()
        except Exception as e:
            logger.error(f"Error applying monkey patches: {str(e)}")
    
    def apply_enum_patches(self):
        """Apply the enum.choices() fixes to prevent 'int' object has no attribute 'replace'"""
        def safe_choices(cls):
            """
            A safe version of choices that handles non-string attributes properly
            """
            def inner_safe_choices():
                result = []
                for status in cls:
                    # Get the value - will be used as is
                    value = status.value
                    
                    # Safely handle name - convert to string regardless of type
                    if hasattr(status, 'name'):
                        name = status.name
                        if name is None:
                            name_str = ""
                        else:
                            name_str = str(name)
                        
                        # Apply any common title formatting without replace()
                        if name_str.startswith("STATUS_"):
                            name_str = name_str[7:]  # Remove STATUS_ prefix
                    else:
                        name_str = str(status)
                        
                    # Title case the result
                    display_name = name_str.title() if name_str else ""
                    
                    # Add the tuple to results
                    result.append((value, display_name))
                    
                return result
            
            return inner_safe_choices
        
        from domain.domain_models import Barbeiro, EntradaFila
        from domain.entities import (
            ClienteStatus, BarbeiroStatus, FilaStatus, 
            FilaPrioridade, ServicoComplexidade
        )
        
        # Patch domain_models.py enums
        Barbeiro.Status.choices = safe_choices(Barbeiro.Status)
        EntradaFila.Status.choices = safe_choices(EntradaFila.Status)
          # Patch entities.py enums
        ClienteStatus.choices = safe_choices(ClienteStatus)
        BarbeiroStatus.choices = safe_choices(BarbeiroStatus)
        FilaStatus.choices = safe_choices(FilaStatus)
        FilaPrioridade.choices = safe_choices(FilaPrioridade)
        ServicoComplexidade.choices = safe_choices(ServicoComplexidade)
        logger.info("Successfully patched Enum.choices methods for safe string handling")
    
    def apply_django_uuid_patch(self):
        """Apply the UUID conversion fix to prevent 'int' object has no attribute 'replace'"""
        
        # Define our improved method using setattr to ensure compatibility
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
        
        try:
            # Apply the patch to Django's SQLite operations
            from django.db.backends.sqlite3.operations import DatabaseOperations
            
            # Store the original method in case we need to revert
            if not hasattr(DatabaseOperations, '_original_convert_uuidfield_value'):
                setattr(DatabaseOperations, '_original_convert_uuidfield_value', 
                        getattr(DatabaseOperations, 'convert_uuidfield_value', None))
            
            # Install the patch using setattr 
            setattr(DatabaseOperations, 'convert_uuidfield_value', patched_convert_uuidfield_value)
            
            # Also patch the Django UUIDField to_python method
            from django.db.models.fields import UUIDField
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
            
            logger.info("Successfully patched Django's UUID converter and UUIDField for SQLite")
        except Exception as e:
            logger.error(f"Error patching Django's UUID converter: {str(e)}")
            raise
    
    def run_ensure_services(self):
        """
        Run the ensure_services command safely after patching
        """
        import sys
        
        # Only run this if we're in a web context (not during migrations, etc.)
        if hasattr(sys, 'argv') and ('runserver' in sys.argv or 'uwsgi' in sys.argv or 'gunicorn' in sys.argv):
            logger.info("Running ensure_services after applying patches")
            try:
                # Import and run the command using Django's call_command
                from django.core.management import call_command
                call_command('ensure_services')
            except Exception as e:
                logger.error(f"Error running ensure_services: {str(e)}", exc_info=True) 