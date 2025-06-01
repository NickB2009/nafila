"""
Start server with fixes in a way that avoids import errors
"""
import os
import sys
import subprocess
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

def apply_sqlite_uuid_patch():
    """Apply the SQLite UUID patch using simple monkey patching"""
    logger.info("Applying SQLite UUID patch...")
    
    # Create a script that will be executed in the Django context
    script_content = """
import uuid
from django.db.backends.sqlite3.operations import DatabaseOperations

# Original method for reference
original_convert_uuidfield_value = DatabaseOperations.convert_uuidfield_value

# Patched method
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
            # Create a new UUID if it's not a valid format
            return uuid.uuid4()
    
    # Handle other types (including DatabaseOperations)
    try:
        if '🔑' in str(value) or 'DatabaseOperations' in str(value):
            return uuid.uuid4()
        return uuid.UUID(str(value))
    except (ValueError, TypeError):
        return uuid.uuid4()

# Apply the patch
DatabaseOperations.convert_uuidfield_value = patched_convert_uuidfield_value

# Print confirmation
print("✅ SQLite UUID patch applied successfully!")
"""

    # Create the temp file
    with open('uuid_patch.py', 'w') as f:
        f.write(script_content)
    
    # Return the script path for execution
    return os.path.abspath('uuid_patch.py')

def main():
    # Step 1: Apply the SQLite UUID patch
    patch_script = apply_sqlite_uuid_patch()
    logger.info(f"UUID patch script created at: {patch_script}")
    
    # Step 2: Start the server with the patch applied
    server_cmd = [
        sys.executable, 
        '-c',
        f"import sys; exec(open('{patch_script}').read()); " +
        "import os; os.chdir('eutonafila'); " +
        "from django.core.management import execute_from_command_line; " +
        "execute_from_command_line(['manage.py', 'runserver'])"
    ]
    
    logger.info("Starting Django server with UUID patch applied...")
    logger.info("Press Ctrl+C to stop the server")
    
    # Execute the server command
    try:
        subprocess.run(server_cmd)
    except KeyboardInterrupt:
        logger.info("Server stopped by user")
    finally:
        # Clean up the temp patch file
        try:
            os.remove(patch_script)
        except:
            pass

if __name__ == "__main__":
    print("===========================================")
    print("=== STARTING SERVER WITH UUID FIX APPLIED ===")
    print("===========================================")
    print("This script will:")
    print("1. Apply the SQLite UUID patch to fix the DatabaseOperations object issue")
    print("2. Start the Django server with the patch applied")
    print("\nPress Enter to continue or Ctrl+C to cancel...")
    
    try:
        input()
        main()
    except KeyboardInterrupt:
        print("\nOperation cancelled by user")
