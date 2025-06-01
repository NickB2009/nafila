"""
Script to truncate all tables in the database to resolve UUID issues
"""
import os
import sys
import django
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

# Now import Django models and functions
from django.db import connection
from django.apps import apps
from django.contrib.auth.models import User, Permission, Group
from django.db.models import Q

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
    
    return {
        'truncated': truncated_tables,
        'skipped': skipped_tables
    }

def create_admin_user():
    """Create a superuser for admin access"""
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

if __name__ == "__main__":
    logger.info("=== TRUNCATE ALL TABLES SCRIPT ===")
    
    confirm = input("This will delete ALL data from ALL tables. Are you sure? (y/n): ")
    
    if confirm.lower() != 'y':
        logger.info("Operation cancelled by user")
        sys.exit(0)
    
    results = truncate_all_tables()
    
    logger.info(f"Successfully truncated {len(results['truncated'])} tables")
    if results['skipped']:
        logger.info(f"Skipped {len(results['skipped'])} tables: {results['skipped']}")
    
    # Create admin user
    create_admin_user()
    
    logger.info("=== TRUNCATION COMPLETED SUCCESSFULLY ===")
    logger.info("You can now start the server and create new data")
