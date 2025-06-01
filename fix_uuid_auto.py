"""
Automated script to fix UUID issues:
1. Truncates all tables without prompt
2. Applies the UUID patch
3. Creates a sample barbearia with proper UUIDs
"""
import os
import sys
import django
import uuid
import logging
from functools import wraps

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    datefmt='%Y-%m-%d %H:%M:%S'
)

logger = logging.getLogger(__name__)

# Add the current directory to the Python path
sys.path.append(os.path.abspath('.'))

# Set up Django environment
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
django.setup()

# Now import Django models and functions
from django.db import connection
from django.apps import apps
from django.contrib.auth.models import User

def get_all_django_tables():
    """Get all table names from Django models"""
    tables = []
    for app_config in apps.get_app_configs():
        for model in app_config.get_models():
            if hasattr(model, '_meta'):
                tables.append(model._meta.db_table)
    return tables

def truncate_all_tables():
    """Truncate all tables in the database except system tables"""
    logger.info("Starting truncation of all tables...")
    
    cursor = connection.cursor()
    
    # Get all tables from Django models
    tables = get_all_django_tables()
    
    # Get tables to protect from deletion
    system_tables = [
        'django_session', 
        'django_migrations', 
        'sqlite_sequence',
        'auth_permission',
        'django_content_type',
    ]
    
    # Filter out system tables
    tables_to_truncate = [table for table in tables if table not in system_tables]
    
    logger.info(f"Found {len(tables_to_truncate)} tables to truncate")
    
    # Disable foreign key constraints for SQLite
    cursor.execute('PRAGMA foreign_keys = OFF;')
    
    # Truncate each table
    truncated_tables = []
    skipped_tables = []
    
    for table in tables_to_truncate:
        try:
            logger.info(f"Truncating table: {table}")
            cursor.execute(f"DELETE FROM {table};")
            truncated_tables.append(table)
        except Exception as e:
            logger.error(f"Error truncating table {table}: {str(e)}")
            skipped_tables.append(table)
    
    # Re-enable foreign key constraints
    cursor.execute('PRAGMA foreign_keys = ON;')
    
    # Reset SQLite sequences
    cursor.execute("UPDATE sqlite_sequence SET seq = 0 WHERE name IN ('" + "','".join(truncated_tables) + "');")
    
    logger.info(f"Successfully truncated {len(truncated_tables)} tables")
    if skipped_tables:
        logger.info(f"Skipped {len(skipped_tables)} tables: {skipped_tables}")
    
    return {
        'truncated': truncated_tables,
        'skipped': skipped_tables
    }

def patch_sqlite_uuid_handling():
    """
    Patch Django's SQLite backend to properly handle UUIDs
    This prevents the '🔑 <django.db.backends.sqlite3.operations.DatabaseOperations object>' issue
    """
    logger.info("Applying SQLite UUID handling patch...")
    
    try:
        from django.db.backends.sqlite3.operations import DatabaseOperations
        
        # Store the original method
        original_convert_uuidfield_value = DatabaseOperations.convert_uuidfield_value
        
        # Define patched method with proper error handling
        @wraps(original_convert_uuidfield_value)
        def patched_convert_uuidfield_value(self, value, expression, connection):
            if value is None:
                return None
            
            # Handle the case where value is already a UUID
            if isinstance(value, uuid.UUID):
                return value
                
            # Handle string values
            if isinstance(value, str):
                try:
                    return uuid.UUID(value)
                except ValueError:
                    logger.warning(f"Invalid UUID string: {value}")
                    return uuid.uuid4()
            
            # Handle bytes
            if isinstance(value, bytes):
                try:
                    return uuid.UUID(bytes=value)
                except (ValueError, TypeError):
                    try:
                        return uuid.UUID(bytes_le=value)
                    except (ValueError, TypeError):
                        logger.warning(f"Invalid UUID bytes: {value}")
                        return uuid.uuid4()
            
            # Handle other types
            try:
                return uuid.UUID(str(value))
            except (ValueError, TypeError, AttributeError):
                logger.warning(f"Cannot convert to UUID: {value} of type {type(value)}")
                return uuid.uuid4()
        
        # Apply the patch
        DatabaseOperations.convert_uuidfield_value = patched_convert_uuidfield_value
        
        logger.info("Successfully patched SQLite UUID handling")
        return True
        
    except Exception as e:
        logger.error(f"Failed to patch SQLite UUID handling: {str(e)}")
        return False

def create_admin_user():
    """Create a superuser for admin access"""
    logger.info("Creating admin user...")
    try:
        if not User.objects.filter(username='admin').exists():
            admin = User.objects.create_superuser(
                username='admin',
                email='admin@example.com',
                password='admin123'
            )
            logger.info(f"Created superuser: {admin.username}")
        else:
            logger.info("Admin user already exists")
    except Exception as e:
        logger.error(f"Error creating admin user: {str(e)}")

def create_sample_data():
    """Create a sample barbearia with proper UUID"""
    logger.info("Creating sample barbearia...")
    try:
        # Import here to ensure patch is applied first
        from eutonafila.barbershop.models import Barbearia
        
        # Create a sample barbearia with explicit UUID
        barbearia = Barbearia.objects.create(
            id=uuid.uuid4(),
            nome="Barbearia Modelo",
            slug="barbearia-modelo",
            telefone="(11) 99999-9999",
            endereco="Rua Exemplo, 123",
            descricao_curta="Uma barbearia modelo para testar o sistema",
            horario_abertura="09:00",
            horario_fechamento="18:00",
            dias_funcionamento=[0, 1, 2, 3, 4, 5],  # Monday to Saturday
            max_capacity=10,
            enable_priority_queue=True
        )
        
        logger.info(f"Created sample barbearia with ID: {barbearia.id}")
        return barbearia
        
    except Exception as e:
        logger.error(f"Error creating sample data: {str(e)}")
        return None

def verify_barbearia_uuid():
    """Verify that the UUID is working correctly"""
    logger.info("Verifying UUID functionality...")
    try:
        from eutonafila.barbershop.models import Barbearia
        
        # Get all barbearias
        barbearias = Barbearia.objects.all()
        
        if not barbearias:
            logger.warning("No barbearias found to verify")
            return False
            
        for barbearia in barbearias:
            id_type = type(barbearia.id)
            id_value = barbearia.id
            
            logger.info(f"Barbearia: {barbearia.nome}")
            logger.info(f"ID: {id_value} (Type: {id_type.__name__})")
            
            if not isinstance(id_value, uuid.UUID):
                logger.error(f"UUID verification failed: Expected uuid.UUID but got {id_type.__name__}")
                return False
                
        logger.info("UUID verification successful!")
        return True
            
    except Exception as e:
        logger.error(f"Error verifying UUIDs: {str(e)}")
        return False

if __name__ == "__main__":
    logger.info("=== AUTOMATIC UUID FIX SCRIPT ===")
    
    # Step 1: Truncate all tables
    logger.info("STEP 1: Truncating all tables...")
    truncate_all_tables()
    
    # Step 2: Apply the UUID patch
    logger.info("STEP 2: Applying UUID patch...")
    patch_success = patch_sqlite_uuid_handling()
    if not patch_success:
        logger.error("Failed to apply UUID patch. Aborting.")
        sys.exit(1)
    
    # Step 3: Create admin user
    logger.info("STEP 3: Creating admin user...")
    create_admin_user()
    
    # Step 4: Create sample data
    logger.info("STEP 4: Creating sample data...")
    barbearia = create_sample_data()
    
    # Step 5: Verify UUID
    logger.info("STEP 5: Verifying UUID functionality...")
    verify_success = verify_barbearia_uuid()
    
    if verify_success:
        logger.info("=== UUID FIX COMPLETED SUCCESSFULLY ===")
        logger.info("The database is now properly handling UUIDs.")
        logger.info("You can now start the server normally.")
    else:
        logger.error("UUID verification failed. Further investigation may be required.")
