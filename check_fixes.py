"""
Script to verify that the fixes succeeded
"""
import os
import sys
import logging

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(message)s')
logger = logging.getLogger(__name__)

def check_indentation_fixes():
    """Check if the indentation fixes succeeded"""
    # Check ensure_services.py
    ensure_services_path = os.path.join('eutonafila', 'barbershop', 'management', 'commands', 'ensure_services.py')
    
    if os.path.exists(ensure_services_path):
        with open(ensure_services_path, 'r', encoding='utf-8') as f:
            content = f.read()
            
        # Check for the indentation issue
        if "def ensure_services_for_one_barbershop(self, barbershop):" in content:
            # Make sure it's properly indented (should have 4 spaces before 'def')
            if "    def ensure_services_for_one_barbershop" in content:
                logger.info("✓ ensure_services.py indentation is fixed")
            else:
                logger.error("✗ ensure_services.py still has indentation issues")
        else:
            logger.warning("? Unable to check ensure_services.py indentation")
    else:
        logger.error(f"✗ File not found: {ensure_services_path}")
    
    # Check domain/models.py
    domain_models_path = os.path.join('domain', 'models.py')
    
    if os.path.exists(domain_models_path):
        with open(domain_models_path, 'r', encoding='utf-8') as f:
            content = f.read()
            
        # Check for expected content
        if "class EntradaFila:" in content and "def get_position(self):" in content:
            # Check if it's not the first line (which would indicate the indentation issue)
            first_line = content.strip().split('\n')[0]
            if first_line != "def get_position(self):":
                logger.info("✓ domain/models.py indentation is fixed")
            else:
                logger.error("✗ domain/models.py still has indentation issues")
        else:
            logger.warning("? Unable to check domain/models.py indentation")
    else:
        logger.error(f"✗ File not found: {domain_models_path}")

def check_uuid_fixes():
    """Check if the UUID fixes were installed"""
    try:
        # Try to import Django and the patched method
        os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
        import django
        django.setup()
        
        from django.db.backends.sqlite3.operations import DatabaseOperations
        
        # Check if the method is the patched one
        method_source = str(DatabaseOperations.convert_uuidfield_value)
        if "Improved method to handle UUID" in method_source or "for any other type, try to convert" in method_source.lower():
            logger.info("✓ UUID patch has been installed")
        else:
            logger.warning("? UUID patch status is unclear - method doesn't have expected comment")
            logger.info(f"Method source: {method_source[:100]}...")
    except Exception as e:
        logger.error(f"✗ Error checking UUID patch: {str(e)}")

def check_ensure_services_command():
    """Check if the ensure_services command works"""
    try:
        # Try to import the command
        from barbershop.management.commands.ensure_services import Command
        
        # Check if it has the required methods
        if hasattr(Command, 'ensure_services_for_one_barbershop'):
            logger.info("✓ ensure_services command has required methods")
        else:
            logger.error("✗ ensure_services command is missing required methods")
    except Exception as e:
        logger.error(f"✗ Error checking ensure_services command: {str(e)}")

if __name__ == "__main__":
    logger.info("===== CHECKING FIXES =====")
    
    # Change to the right directory
    os.chdir(os.path.abspath(os.path.join(os.path.dirname(__file__))))
    
    # Check indentation fixes
    logger.info("\nChecking indentation fixes:")
    check_indentation_fixes()
    
    # Check UUID fixes
    logger.info("\nChecking UUID fixes:")
    check_uuid_fixes()
    
    # Check ensure_services command
    logger.info("\nChecking ensure_services command:")
    check_ensure_services_command()
    
    logger.info("\n===== CHECK COMPLETE =====")
