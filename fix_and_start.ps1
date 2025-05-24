# Fix the UUID issues and start the server
$ErrorActionPreference = "Stop"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "   FIXING INDENTATION AND UUID ISSUES" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Path variables
$baseDir = "C:\git\eutonafila\nafila"
$ensureServicesPath = Join-Path -Path $baseDir -ChildPath "eutonafila\barbershop\management\commands\ensure_services.py"
$domainModelsPath = Join-Path -Path $baseDir -ChildPath "domain\models.py"

function Fix-EnsureServices {
    Write-Host "Fixing indentation in ensure_services.py..." -ForegroundColor Yellow
    
    # Create the fixed content
    $content = @"
from django.core.management.base import BaseCommand
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
        """Ensure THE GIVEN barbershop has all default services"""
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
"@

    # Backup the original file if needed
    $backupPath = "$ensureServicesPath.bak"
    if (!(Test-Path $backupPath)) {
        Write-Host "Creating backup at $backupPath"
        Copy-Item $ensureServicesPath $backupPath -Force
    }
    
    # Write the fixed content
    Set-Content -Path $ensureServicesPath -Value $content -Encoding UTF8
    Write-Host "Fixed ensure_services.py" -ForegroundColor Green
}

function Fix-DomainModels {
    Write-Host "Fixing indentation in domain/models.py..." -ForegroundColor Yellow
    
    # Check if the file exists
    if (!(Test-Path $domainModelsPath)) {
        Write-Host "Warning: domain/models.py not found at $domainModelsPath" -ForegroundColor Yellow
        return
    }
    
    # Create the fixed content
    $content = @"
# Domain models module

class EntradaFila:
    STATUS_AGUARDANDO = 1
    STATUS_EM_ATENDIMENTO = 2
    STATUS_ATENDIDO = 3
    STATUS_DESISTIU = 4
    STATUS_ESCOLHEU_ESPERAR_EM_LOJA = 5
    STATUS_ESCOLHEU_ESPERAR_FORA = 6
    
    def get_position(self):
        """Get position in queue (FIFO order)"""
        if self.status != self.STATUS_AGUARDANDO:
            return 0
        
        # Simply use position number (FIFO order)
        return self.position_number 

    def get_priority_position(self):
        """Get position in queue, considering priority level"""
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
        """Get the current client being served by this barber"""
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
"@

    # Backup the original file if needed
    $backupPath = "$domainModelsPath.bak"
    if (!(Test-Path $backupPath)) {
        Write-Host "Creating backup at $backupPath"
        Copy-Item $domainModelsPath $backupPath -Force
    }
    
    # Write the fixed content
    Set-Content -Path $domainModelsPath -Value $content -Encoding UTF8
    Write-Host "Fixed domain/models.py" -ForegroundColor Green
}

# Fix the files
Fix-EnsureServices
Fix-DomainModels

# Execute fix_uuid.py
Write-Host ""
Write-Host "Running fix_uuid.py..." -ForegroundColor Yellow
Set-Location $baseDir
& "$baseDir\venv\Scripts\python.exe" fix_uuid.py

# Change to the project directory
Set-Location (Join-Path -Path $baseDir -ChildPath "eutonafila")

# Apply the middleware patches
Write-Host ""
Write-Host "Applying middleware patches..." -ForegroundColor Yellow
& "$baseDir\venv\Scripts\python.exe" -c "from middleware import MonkeyPatchMiddleware; patch = MonkeyPatchMiddleware(lambda x: x); patch.apply_patches()"

# Run the ensure_services command
Write-Host ""
Write-Host "Running ensure_services command..." -ForegroundColor Yellow
& "$baseDir\venv\Scripts\python.exe" manage.py ensure_services

# Start the Django server
Write-Host ""
Write-Host "Starting Django server..." -ForegroundColor Yellow
Write-Host "Access http://127.0.0.1:8000/ in your browser"
Write-Host "Press Ctrl+C to stop the server"
Write-Host ""
& "$baseDir\venv\Scripts\python.exe" manage.py runserver
