"""
Enhanced script to fix the UUID handling issue in Barbearia model
and to correct indentation issues in Python files
"""
import os
import sys
import django
import uuid
import logging
from pprint import pformat

# Add the current directory to the Python path
sys.path.append(os.path.abspath('.'))

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    datefmt='%Y-%m-%d %H:%M:%S'
)

logger = logging.getLogger(__name__)

def fix_ensure_services_file():
    """Fix indentation issues in ensure_services.py"""
    filepath = os.path.abspath(os.path.join('eutonafila', 'barbershop', 'management', 'commands', 'ensure_services.py'))
    backup_filepath = filepath + '.bak'
    
    logger.info(f"Fixing indentation in {filepath}")
    
    # Create backup if it doesn't exist
    if not os.path.exists(backup_filepath):
        logger.info(f"Creating backup at {backup_filepath}")
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
        with open(backup_filepath, 'w', encoding='utf-8') as f:
            f.write(content)
    
    # Prepare the fixed content
    fixed_content = """from django.core.management.base import BaseCommand
from barbershop.models import Barbearia, Servico
import logging
import uuid

logger = logging.getLogger(__name__)

DEFAULT_SERVICES = [
    {
        'nome': 'Corte de Cabelo',
        'preco': 30.00,
        'duracao': 30,
        'descricao': 'Corte de cabelo personalizado de acordo com seu estilo.'
    },
    {
        'nome': 'Barba',
        'preco': 25.00,
        'duracao': 20,
        'descricao': 'Modelagem e aparagem da barba com toalha quente e produtos especiais.'
    },
    {
        'nome': 'Corte e Barba',
        'preco': 50.00,
        'duracao': 45,
        'descricao': 'Combinação de corte de cabelo e serviço de barba.'
    },
    {
        'nome': 'Acabamento',
        'preco': 15.00,
        'duracao': 15,
        'descricao': 'Acabamento nas laterais e na nuca para manter o visual entre cortes.'
    }
]

class Command(BaseCommand):
    help = 'Ensures all barbershops have the default services'

    def handle(self, *args, **kwargs):
        self.stdout.write('Checking and ensuring default services for all barbershops...')
        
        try:
            barbershops = Barbearia.objects.all()
            
            if not barbershops.exists():
                self.stdout.write(self.style.WARNING('No barbershops found.'))
                return
            
            for barbershop_instance in barbershops:
                current_barbershop_to_process = barbershop_instance

                # Defensive check for the iterated object itself
                if not isinstance(barbershop_instance, Barbearia):
                    logger.error(f"ensure_services command: Item in Barbearia.objects.all() is not a Barbearia instance. Type: {type(barbershop_instance)}. Value: {str(barbershop_instance)}. Skipping.")
                    self.stdout.write(self.style.ERROR(f"Skipping invalid object found during barbershop iteration: {str(barbershop_instance)}"))
                    continue

                # Check if the ID of the fetched instance is correct
                if not isinstance(barbershop_instance.id, uuid.UUID):
                    logger.warning(f"ensure_services command: Barbershop '{barbershop_instance.slug if hasattr(barbershop_instance, 'slug') else 'N/A'}' has problematic ID type: {type(barbershop_instance.id)}. Attempting to fix.")
                    self.stdout.write(self.style.WARNING(f"Barbershop '{barbershop_instance.slug if hasattr(barbershop_instance, 'slug') else 'N/A'}' has problematic ID. Fixing..."))
                    try:
                        # Try to directly fix the instance, instead of reloading
                        barbershop_instance.id = uuid.uuid4()
                        barbershop_instance.save()
                        logger.info(f"ensure_services command: Successfully fixed Barbershop '{barbershop_instance.slug}' with new UUID ID.")
                        self.stdout.write(self.style.SUCCESS(f"Successfully fixed Barbershop '{barbershop_instance.slug}' with new UUID."))
                        current_barbershop_to_process = barbershop_instance
                    except Exception as e_fix:
                        # Fall back to the original reload approach if direct fix fails
                        try:
                            # Use slug to find the barbershop if it has one
                            if hasattr(barbershop_instance, 'slug') and barbershop_instance.slug:
                                reloaded_barbershop = Barbearia.objects.get(slug=barbershop_instance.slug)
                            else:
                                # Use pk from the instance, assuming it's at least a valid PK even if id field is weird
                                reloaded_barbershop = Barbearia.objects.get(pk=barbershop_instance.pk)
                            
                            # Verify the reloaded instance's ID
                            if isinstance(reloaded_barbershop.id, uuid.UUID):
                                current_barbershop_to_process = reloaded_barbershop
                                logger.info(f"ensure_services command: Successfully reloaded Barbershop '{current_barbershop_to_process.slug}' with correct UUID ID.")
                                self.stdout.write(self.style.SUCCESS(f"Successfully reloaded Barbershop '{current_barbershop_to_process.slug}'."))
                            else:
                                # Last resort - force a new UUID
                                reloaded_barbershop.id = uuid.uuid4()
                                reloaded_barbershop.save()
                                current_barbershop_to_process = reloaded_barbershop
                                logger.info(f"ensure_services command: Forced new UUID for Barbershop '{reloaded_barbershop.slug}'.")
                                self.stdout.write(self.style.SUCCESS(f"Forced new UUID for Barbershop '{reloaded_barbershop.slug}'."))
                        except Exception as e_reload:
                            logger.error(f"ensure_services command: Error fixing/reloading Barbershop '{barbershop_instance.slug if hasattr(barbershop_instance, 'slug') else 'N/A'}': {str(e_reload)}", exc_info=True)
                            self.stdout.write(self.style.ERROR(f"Error fixing Barbershop '{barbershop_instance.slug if hasattr(barbershop_instance, 'slug') else 'N/A'}'. Skipping."))
                            continue
                
                self.ensure_services_for_one_barbershop(current_barbershop_to_process)
            
            self.stdout.write(self.style.SUCCESS('Successfully ensured services for all barbershops!'))
        except Exception as e:
            self.stdout.write(self.style.ERROR(f'Error in ensure_services command handle: {str(e)}'))
            logger.exception("Error in ensure_services command handle")
    
    def ensure_services_for_one_barbershop(self, barbershop):
        \"\"\"Ensure THE GIVEN barbershop has all default services\"\"\"
        # barbershop parameter here should be a valid Barbearia instance with a UUID id
        # due to the checks in the handle() method.

        if not isinstance(barbershop, Barbearia):
            logger.error(f"ensure_services_for_one_barbershop: Received invalid barbershop object. Type: {type(barbershop)}. Skipping.")
            self.stdout.write(self.style.ERROR(f"Skipping service check for invalid barbershop data: {str(barbershop)}"))
            return
            
        # Verify and fix ID if needed
        if not isinstance(barbershop.id, uuid.UUID):
            try:
                # Try to fix the ID
                old_id = barbershop.id
                barbershop.id = uuid.uuid4()
                barbershop.save()
                logger.info(f"ensure_services_for_one_barbershop: Fixed ID for barbershop {barbershop.nome}. Old ID type: {type(old_id)}, New ID: {barbershop.id}")
                self.stdout.write(self.style.SUCCESS(f"Fixed ID for barbershop {barbershop.nome}"))
            except Exception as e:
                logger.error(f"ensure_services_for_one_barbershop: Failed to fix barbershop ID. {str(e)}")
                self.stdout.write(self.style.ERROR(f"Failed to fix ID for barbershop {barbershop.nome}. Skipping."))
                return

        barbershop_name_for_log = barbershop.nome if barbershop.nome is not None else ""
        
        try:
            self.stdout.write(f"Processing services for Barbershop: '{barbershop_name_for_log}' (ID: {barbershop.id})")
            existing_services = Servico.objects.filter(barbearia=barbershop)
            existing_service_names = [str(s.nome) for s in existing_services if s.nome is not None]
            
            self.stdout.write(f"  Barbershop: '{barbershop_name_for_log}' currently has {len(existing_service_names)} services: {existing_service_names}")
            
            services_created_count = 0
            for service_data in DEFAULT_SERVICES:
                service_name = str(service_data['nome'])
                if service_name not in existing_service_names:
                    Servico.objects.create(
                        barbearia=barbershop,
                        nome=service_name,
                        preco=service_data['preco'],
                        duracao=service_data['duracao'],
                        descricao=str(service_data['descricao'])
                    )
                    services_created_count += 1
                    self.stdout.write(f"    - Created service: '{service_name}' for '{barbershop_name_for_log}'")
            
            if services_created_count > 0:
                self.stdout.write(self.style.SUCCESS(f"  Added {services_created_count} missing services to '{barbershop_name_for_log}'"))
            else:
                self.stdout.write(f"  All default services already exist for '{barbershop_name_for_log}'")
        except Exception as e:
            # This is the exception caught in your traceback
            # Log which barbershop caused it, using its already validated/reloaded ID.
            error_msg = f"Error processing services for barbershop ID {barbershop.id} ('{barbershop_name_for_log}'): {str(e)}"
            self.stdout.write(self.style.ERROR(error_msg))
            logger.exception(error_msg) # This will include the full traceback for this specific error.
"""
    
    # Write the fixed content to the file
    try:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(fixed_content)
        
        logger.info(f"Successfully fixed indentation in {filepath}")
        return True
    except Exception as e:
        logger.error(f"Error fixing {filepath}: {str(e)}", exc_info=True)
        return False

def fix_domain_models_file():
    """Fix indentation issues in domain/models.py"""
    filepath = os.path.abspath(os.path.join('domain', 'models.py'))
    
    if not os.path.exists(filepath):
        logger.warning(f"File not found: {filepath}")
        return False
        
    backup_filepath = filepath + '.bak'
    
    logger.info(f"Fixing indentation in {filepath}")
    
    # Create backup if it doesn't exist
    if not os.path.exists(backup_filepath):
        logger.info(f"Creating backup at {backup_filepath}")
        try:
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
            with open(backup_filepath, 'w', encoding='utf-8') as f:
                f.write(content)
        except Exception as e:
            logger.error(f"Error backing up {filepath}: {str(e)}")
            return False
    
    # Prepare the fixed content
    fixed_content = """# Domain models module

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
    try:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(fixed_content)
        
        logger.info(f"Successfully fixed indentation in {filepath}")
        return True
    except Exception as e:
        logger.error(f"Error fixing {filepath}: {str(e)}", exc_info=True)
        return False

def install_uuid_patch():
    """Install a patch for Django's UUID handling in SQLite"""
    try:
        from django.db.backends.sqlite3.operations import DatabaseOperations
        
        # Show current method information
        original_method = DatabaseOperations.convert_uuidfield_value
        logger.info(f"Found original convert_uuidfield_value method: {original_method}")
        
        # Define our new method
        def patched_convert_uuidfield_value(self, value, expression, connection):
            """Improved method to handle UUID conversion with fallbacks for problematic types"""
            if value is None:
                return None
            
            try:
                if isinstance(value, uuid.UUID):
                    return value
                    
                if isinstance(value, str):
                    return uuid.UUID(value)
                    
                if isinstance(value, int):
                    return uuid.UUID(int=value)
                    
                if isinstance(value, bytes):
                    return uuid.UUID(bytes=value)
                
                # For any other type, try to convert to string first
                logger.warning(f"Converting non-standard value type: {type(value)}")
                safe_string = str(value)
                try:
                    return uuid.UUID(safe_string)
                except (ValueError, TypeError):
                    logger.warning(f"Creating new UUID since conversion failed for: {safe_string}")
                    return uuid.uuid4()
                    
            except (ValueError, AttributeError, TypeError) as e:
                logger.error(f"Error converting {value} ({type(value)}) to UUID: {str(e)}")
                return uuid.uuid4()  # Fallback to a new random UUID
        
        # Install the patch
        setattr(DatabaseOperations, 'convert_uuidfield_value', patched_convert_uuidfield_value)
        logger.info("Successfully installed UUID patch")
        
        # Verify it's installed correctly
        current_method = DatabaseOperations.convert_uuidfield_value
        logger.info(f"Current method is now: {current_method}")
        return True
        
    except Exception as e:
        logger.error(f"Failed to install UUID patch: {str(e)}", exc_info=True)
        return False

def fix_barbershop_ids():
    """Fix any existing barbershop data with UUID issues"""
    try:
        # Set up Django environment
        os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
        django.setup()
        
        # First import the models
        from barbershop.models import Barbearia, Servico
        
        # Get all barbershops
        barbershops = list(Barbearia.objects.all())
        logger.info(f"Found {len(barbershops)} barbershops in database")
        
        # Display ID info for each
        for i, barbershop in enumerate(barbershops):
            try:
                logger.info(f"Barbershop {i+1}: {barbershop.nome} - ID: {barbershop.id} (Type: {type(barbershop.id)})")
            except Exception as e:
                logger.error(f"Error accessing barbershop attributes: {str(e)}")
        
        # Try to fix problematic IDs
        fixed_count = 0
        for barbershop in barbershops:
            try:
                if not isinstance(barbershop.id, uuid.UUID):
                    old_id = barbershop.id
                    barbershop.id = uuid.uuid4()
                    barbershop.save()
                    logger.info(f"Fixed barbershop '{barbershop.nome}' with invalid ID type: {type(old_id)} -> {barbershop.id}")
                    fixed_count += 1
            except Exception as e:
                logger.error(f"Error fixing barbershop: {str(e)}")
        
        logger.info(f"Fixed {fixed_count} barbershops with UUID issues")
        return True
        
    except Exception as e:
        logger.error(f"Error in fix_barbershop_ids: {str(e)}", exc_info=True)
        return False

if __name__ == "__main__":
    logger.info("===== ENHANCED UUID AND INDENTATION FIXES =====")
    
    # Fix indentation issues first
    logger.info("Starting to fix indentation issues...")
    fixed_ensure_services = fix_ensure_services_file()
    fixed_domain_models = fix_domain_models_file()
    
    if fixed_ensure_services and fixed_domain_models:
        logger.info("Successfully fixed indentation issues")
    else:
        logger.error("Some indentation fixes failed - check logs above")
    
    # Now set up Django env and fix UUID handling
    logger.info("Starting UUID patch installation...")
    
    # Set up Django environment
    os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
    django.setup()
    
    if install_uuid_patch():
        logger.info("UUID patch installed successfully - now fixing barbershop IDs")
        if fix_barbershop_ids():
            logger.info("UUID fixes completed successfully")
        else:
            logger.error("Failed to fix barbershop IDs")
    else:
        logger.error("Failed to install UUID patch")
