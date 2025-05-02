import os
import sys
import django

# Add the current directory to Python path
sys.path.append(os.path.abspath('.'))

# Set up Django environment
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')

# Initialize Django
try:
    django.setup()
    print("Django setup complete")
except Exception as e:
    print(f"Error setting up Django: {str(e)}")
    import traceback
    traceback.print_exc()
    sys.exit(1)

try:
    # Import the User model
    from django.contrib.auth.models import User
    print("Successfully imported User model")

    # Check if superuser already exists
    if not User.objects.filter(username='admin').exists():
        # Create a superuser
        User.objects.create_superuser('admin', 'admin@example.com', 'admin')
        print("Superuser 'admin' created with password 'admin'")
    else:
        print("Superuser 'admin' already exists")
        # Reset password
        admin = User.objects.get(username='admin')
        admin.set_password('admin')
        admin.save()
        print("Password reset to 'admin'")
except Exception as e:
    print(f"Error creating/updating superuser: {str(e)}")
    import traceback
    traceback.print_exc() 