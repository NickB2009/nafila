#!/bin/env python
"""
Fix script to correct indentation issues in Python files
"""
import os
import sys

def fix_ensure_services_file():
    """Fix indentation issues in ensure_services.py"""
    filepath = os.path.abspath(os.path.join('eutonafila', 'barbershop', 'management', 'commands', 'ensure_services.py'))
    fixed_filepath = os.path.abspath(os.path.join('eutonafila', 'barbershop', 'management', 'commands', 'ensure_services_fixed.py'))
    backup_filepath = filepath + '.bak'
    
    print(f"Fixing indentation in {filepath}")
    
    # Create backup if it doesn't exist
    if not os.path.exists(backup_filepath):
        print(f"Creating backup at {backup_filepath}")
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
        with open(backup_filepath, 'w', encoding='utf-8') as f:
            f.write(content)
    
    # Read the fixed version
    try:
        with open(fixed_filepath, 'r', encoding='utf-8') as f:
            fixed_content = f.read()
        
        # Write the fixed content to the original file
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(fixed_content)
        
        print(f"Successfully fixed {filepath}")
        return True
    except Exception as e:
        print(f"Error fixing {filepath}: {str(e)}")
        return False

def fix_domain_models_file():
    """Fix indentation issues in domain/models.py"""
    filepath = os.path.abspath(os.path.join('domain', 'models.py'))
    backup_filepath = filepath + '.bak'
    
    print(f"Fixing indentation in {filepath}")
    
    # Create backup if it doesn't exist
    if not os.path.exists(backup_filepath):
        print(f"Creating backup at {backup_filepath}")
        try:
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
            with open(backup_filepath, 'w', encoding='utf-8') as f:
                f.write(content)
        except Exception as e:
            print(f"Error backing up {filepath}: {str(e)}")
            return False
    
    # Replace content with the correctly indented version
    try:
        proper_content = """# Domain models module

class EntradaFila:
    STATUS_AGUARDANDO = 1
    STATUS_EM_ATENDIMENTO = 2
    STATUS_ATENDIDO = 3
    STATUS_DESISTIU = 4
    STATUS_ESCOLHEU_ESPERAR_EM_LOJA = 5
    STATUS_ESCOLHEU_ESPERAR_FORA = 6
    
    def get_position(self):
        \"\"\"Get position in queue (FIFO order)\"\"\"
        if self.status != self.STATUS_AGUARDANDO:
            return 0
        
        # Simply use position number (FIFO order)
        return self.position_number 

    def get_priority_position(self):
        \"\"\"Get position in queue, considering priority level\"\"\"
        if self.status != self.STATUS_AGUARDANDO:
            return 0
        
        # Count clients with higher priority
        higher_priority_count = EntradaFila.objects.filter(
            barbearia=self.barbearia,
            status=self.STATUS_AGUARDANDO,
            prioridade__gt=self.prioridade
        ).count()
        
        # Count clients with same priority but earlier arrival time
        same_priority_earlier_arrival = EntradaFila.objects.filter(
            barbearia=self.barbearia,
            status=self.STATUS_AGUARDANDO,
            prioridade=self.prioridade,
            data_entrada__lt=self.data_entrada
        ).count()
        
        # Result is the sum plus one (for 1-based indexing)
        return higher_priority_count + same_priority_earlier_arrival + 1

class Barbeiro:
    STATUS_DISPONIVEL = 1
    STATUS_EM_ATENDIMENTO = 2
    STATUS_PAUSA = 3
    STATUS_INDISPONIVEL = 4
    
    def get_current_client(self):
        \"\"\"Get the current client being served by this barber\"\"\"
        if self.status != self.STATUS_EM_ATENDIMENTO:
            return None
        
        # Find the client entry that's marked with this barber
        try:
            return EntradaFila.objects.get(
                barbeiro_atendendo=self,
                status=EntradaFila.STATUS_EM_ATENDIMENTO
            )
        except EntradaFila.DoesNotExist:
            return None
"""
        
        # Write the fixed content to the file
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(proper_content)
        
        print(f"Successfully fixed {filepath}")
        return True
    except Exception as e:
        print(f"Error fixing {filepath}: {str(e)}")
        return False

if __name__ == "__main__":
    print("===== FIXING INDENTATION ISSUES =====")
    
    # Fix ensure_services.py
    fixed_ensure_services = fix_ensure_services_file()
    
    # Fix domain/models.py
    fixed_domain_models = fix_domain_models_file()
    
    if fixed_ensure_services and fixed_domain_models:
        print("\nAll files fixed successfully!")
        sys.exit(0)
    else:
        print("\nSome files could not be fixed. Check the logs above.")
        sys.exit(1)
