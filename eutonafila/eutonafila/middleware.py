"""
Middleware for monkey patching and other application-wide fixes
"""
import logging
import uuid
from django.urls import resolve

logger = logging.getLogger(__name__)

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
        # Skip patching for admin URLs
        if request.path.startswith('/admin/'):
            return self.get_response(request)
            
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
            self.apply_i18n_patch()
            self.is_patched = True
            logger.info("Successfully applied monkey patches")
            
            # Run ensure_services after fixes are applied
            self.run_ensure_services()
        except Exception as e:
            logger.error(f"Error applying monkey patches: {str(e)}")
            # Don't raise the exception - allow the app to continue without patches
            self.is_patched = False
    
    def apply_i18n_patch(self):
        """Apply patch to Django's i18n template tag handling"""
        try:
            from django.templatetags.i18n import BlockTranslateNode
            
            def safe_render_token_list(self, tokens):
                """A safe version of render_token_list that handles token contents properly"""
                result = []
                vars = []
                for token in tokens:
                    contents = token.contents
                    if isinstance(contents, str):
                        # Handle variable interpolation
                        if contents.startswith('%') and contents.endswith('%'):
                            var_name = contents[1:-1].strip()
                            vars.append(var_name)
                            result.append(f"%%({var_name})s")
                        else:
                            result.append(contents.replace("%", "%%"))
                    else:
                        result.append(str(contents))
                return "".join(result), vars
            
            # Patch the render_token_list method
            BlockTranslateNode.render_token_list = safe_render_token_list
            logger.info("Successfully patched Django's i18n template tag handling")
        except Exception as e:
            logger.error(f"Error patching Django's i18n template tag handling: {str(e)}")
            raise
    
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
        """Apply patch to Django's UUID converter to handle integers safely"""
        # Define a safe UUID converter
        def safe_convert_uuidfield_value(value, expression, connection, context=None):
            """
            A safe version of the SQLite UUID converter that handles integers properly
            Added context parameter to match Django's expected signature
            """
            if value is None:
                return None
                
            # Handle DatabaseOperations object or other non-standard types
            if not isinstance(value, (str, int, uuid.UUID)):
                logger.warning(f"Received non-standard value type: {type(value)}")
                return value
            
            # Handle integer values by converting them to strings
            if isinstance(value, int):
                logger.debug(f"Converting integer {value} to UUID")
                try:
                    # Try to use it as a UUID integer representation
                    return uuid.UUID(int=value)
                except (ValueError, TypeError):
                    # If that fails, convert to string and use as a UUID string
                    return uuid.UUID(str(value))
            
            # Handle string values that might contain ints
            if isinstance(value, str) and value.isdigit():
                try:
                    # First try to parse as an integer UUID
                    int_val = int(value)
                    return uuid.UUID(int=int_val)
                except (ValueError, TypeError):
                    pass
            
            # Handle UUID objects directly
            if isinstance(value, uuid.UUID):
                return value
                
            # Original behavior
            try:
                return uuid.UUID(value)
            except (ValueError, TypeError, AttributeError):
                # Added AttributeError to catch 'replace' issues
                return value
        
        try:
            # Apply the patch to Django's SQLite operations
            from django.db.backends.sqlite3.operations import DatabaseOperations
            
            # Store the original converter for reference
            original_converter = DatabaseOperations.convert_uuidfield_value
            
            # Replace with our safe version 
            DatabaseOperations.convert_uuidfield_value = safe_convert_uuidfield_value
            
            logger.info("Successfully patched Django's UUID converter for SQLite")
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