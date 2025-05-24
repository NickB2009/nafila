import os
import sys

# Add the project directory to Python path
sys.path.insert(0, os.path.dirname(__file__))

# Set Django settings module
os.environ['DJANGO_SETTINGS_MODULE'] = 'eutonafila.settings_production'

# Import and create WSGI application
from django.core.wsgi import get_wsgi_application
application = get_wsgi_application()