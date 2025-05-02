"""
Monkey patching script to fix Enum classes by replacing the original
choices methods with safer versions that won't throw 'int' object has no attribute 'replace'
"""
import sys
import logging

logger = logging.getLogger(__name__)

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

def monkey_patch_enums():
    """Apply the monkey patch to all enum types in the application"""
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
    
    print("Successfully patched Enum.choices methods for safe string handling")
    return True

if __name__ == "__main__":
    print("Applying Enum.choices monkey patches...")
    success = monkey_patch_enums()
    print(f"Patch applied: {success}") 