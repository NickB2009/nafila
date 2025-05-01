import os
import sys
import django

# Set up Django environment
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'nafila.settings')
django.setup()

# Import Django migration tools
from django.core.management import call_command

def main():
    """Apply all pending migrations"""
    print("Applying migrations...")
    call_command('migrate', interactive=False)
    print("Migrations applied successfully!")

if __name__ == "__main__":
    main() 