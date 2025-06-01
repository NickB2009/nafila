"""
Simple script to test if the UUID fix was applied successfully
"""
import os
import sys
import django
import uuid
import logging

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

def test_uuid_handling():
    """Test if the UUID patch was applied correctly by querying the database directly"""
    
    try:
        # Use Django's low-level database API to avoid model imports
        from django.db import connection
        
        logger.info("Testing UUID handling with raw SQL...")
        
        # Execute a simple query to get IDs from the barbershop table
        with connection.cursor() as cursor:
            cursor.execute("SELECT id, nome FROM barbershop_barbearia LIMIT 10")
            rows = cursor.fetchall()
            
            if not rows:
                logger.warning("No records found in barbershop_barbearia table")
                return
            
            logger.info(f"Found {len(rows)} records")
            
            for i, (id_value, nome) in enumerate(rows):
                logger.info(f"Record {i+1}: {nome}")
                logger.info(f"  Raw ID: {id_value}")
                
                # Try to convert ID to UUID
                try:
                    uuid_obj = uuid.UUID(id_value)
                    logger.info(f"  UUID: {uuid_obj}")
                except ValueError:
                    logger.error(f"  Invalid UUID format: {id_value}")
                
        # Now use Django's ORM to check if UUID handling works
        logger.info("\nTesting UUID handling with Django ORM...")
        
        try:
            # Import Django settings without importing models
            from django.conf import settings
            
            # Try to use a generic model query to avoid importing problematic models
            from django.db.models import Model
            
            # Find a model class for Barbearia
            for app_config in django.apps.apps.get_app_configs():
                for model in app_config.get_models():
                    if model._meta.db_table == 'barbershop_barbearia':
                        BarbeariaModel = model
                        break
            
            # Query the model
            barbearias = BarbeariaModel.objects.all()[:5]
            
            logger.info(f"Found {len(barbearias)} barbearias with ORM")
            
            for i, barb in enumerate(barbearias):
                logger.info(f"Barbearia {i+1}: {barb.nome}")
                logger.info(f"  ID: {barb.id} (Type: {type(barb.id).__name__})")
                logger.info(f"  String representation: {str(barb)}")
                
                # Check for the UUID issue
                if "DatabaseOperations" in str(barb):
                    logger.error("  UUID ISSUE DETECTED: str(barb) contains DatabaseOperations")
                
                if not isinstance(barb.id, uuid.UUID):
                    logger.error(f"  UUID ISSUE DETECTED: ID is not a UUID object, but {type(barb.id).__name__}")
                else:
                    logger.info("  UUID is correctly handled!")
        
        except Exception as e:
            logger.error(f"Error using Django ORM: {str(e)}")
            import traceback
            traceback.print_exc()
            
    except Exception as e:
        logger.error(f"Error testing UUID handling: {str(e)}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    print("==============================")
    print("=== TESTING UUID HANDLING ===")
    print("==============================")
    
    # Test UUID handling
    test_uuid_handling()
    
    print("\n==============================")
    print("=== TEST COMPLETE ===")
    print("==============================")
