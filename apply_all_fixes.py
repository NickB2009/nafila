#!/usr/bin/env python
"""
Combined fix script to address all issues with UUID handling and indentation
"""
import os
import sys
import subprocess
import logging

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(message)s',
    datefmt='%Y-%m-%d %H:%M:%S'
)

logger = logging.getLogger(__name__)

def run_command(command, cwd=None):
    """Run a command and return the output"""
    try:
        logger.info(f"Running: {command}")
        result = subprocess.run(
            command,
            shell=True,
            check=True,
            text=True,
            capture_output=True,
            cwd=cwd
        )
        logger.info(result.stdout)
        return True
    except subprocess.CalledProcessError as e:
        logger.error(f"Command failed: {e}")
        logger.error(f"STDERR: {e.stderr}")
        return False

def main():
    """Main function to fix all issues"""
    logger.info("======================================")
    logger.info("       FIXING ALL UUID ISSUES")
    logger.info("======================================")
    
    # Get the base directory (assuming the script is run from the nafila directory)
    base_dir = os.path.abspath(os.path.dirname(__file__))
    logger.info(f"Working directory: {base_dir}")
      # Fix steps
    steps = [
        {
            "name": "1. Fix indentation in domain/models.py",
            "command": r"""python -c "
with open('domain/models.py', 'r') as f:
    content = f.read()
# Fix indentation at the beginning of the file
if content.startswith('class EntradaFila:'):
    content = content.replace('class EntradaFila:', 'class EntradaFila:')
with open('domain/models.py', 'w') as f:
    f.write(content)
print('Indentation fixed in domain/models.py')
"
""",
            "cwd": base_dir
        },
        {
            "name": "2. Fix indentation in middleware.py",
            "command": r"""python -c "
with open('eutonafila/middleware.py', 'r') as f:
    content = f.read()
# Fix the indentation in middleware.py
content = content.replace('  logger.info', '        logger.info')
content = content.replace('  def apply_django_uuid_patch', '    def apply_django_uuid_patch')
with open('eutonafila/middleware.py', 'w') as f:
    f.write(content)
print('Indentation fixed in middleware.py')
"
""",
            "cwd": base_dir
        },
        {
            "name": "3. Fix indentation in ensure_services.py",
            "command": r"""python -c "
with open('eutonafila/barbershop/management/commands/ensure_services.py', 'r') as f:
    content = f.read()
# Fix previous indentation issue
content = content.replace('except Exception as e:            self', 'except Exception as e:\\n            self')
# Fix the new indentation issue
content = content.replace('  self.stdout.write(self.style.SUCCESS', '        self.stdout.write(self.style.SUCCESS')
with open('eutonafila/barbershop/management/commands/ensure_services.py', 'w') as f:
    f.write(content)
print('Indentation fixed in ensure_services.py')
"
""",
            "cwd": base_dir
        },
        {
            "name": "4. Apply UUID patch fixes",
            "command": "python fix_uuid.py",
            "cwd": base_dir
        },
        {
            "name": "5. Verify ensure_services command works",
            "command": "python manage.py ensure_services",
            "cwd": os.path.join(base_dir, "eutonafila")
        }
    ]
    
    # Execute each step
    success = True
    for step in steps:
        logger.info(f"\n{step['name']}...")
        step_success = run_command(step["command"], cwd=step.get("cwd"))
        if not step_success:
            logger.error(f"Step failed: {step['name']}")
            success = False
            break
    
    # Final status
    if success:
        logger.info("\n======================================")
        logger.info("       ALL FIXES APPLIED SUCCESSFULLY")
        logger.info("======================================")
        logger.info("\nYou can now run start_server.bat to start the server.")
    else:
        logger.error("\n======================================")
        logger.error("       SOME FIXES FAILED")
        logger.error("======================================")
        logger.error("\nPlease review the errors above.")
    
    return success

if __name__ == "__main__":
    try:
        success = main()
        input("\nPress Enter to continue...")
        sys.exit(0 if success else 1)
    except KeyboardInterrupt:
        logger.info("\nOperation cancelled by user.")
        sys.exit(130)
    except Exception as e:
        logger.error(f"Unexpected error: {e}", exc_info=True)
        input("\nAn error occurred. Press Enter to continue...")
        sys.exit(1)
