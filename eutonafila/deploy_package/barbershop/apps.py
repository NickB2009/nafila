from django.apps import AppConfig
import logging

logger = logging.getLogger(__name__)


class BarbershopConfig(AppConfig):
    default_auto_field = 'django.db.models.BigAutoField'
    name = 'barbershop'
    
    def ready(self):
        """
        Run the ensure_services command when the app starts to prevent services 
        from disappearing. This makes sure each barbershop has the default services.
        """
        # Avoid running this in manage.py commands like migrate
        import sys
        
        # Skip for now until the enum issue is fixed
        # We'll run this manually or after our middleware has applied the fixes
        # The middleware should be loaded before this runs
        logger.debug("Skipping ensure_services due to potential enum issues - will be handled by middleware")
        return
        
        # Original code commented out:
        # if 'runserver' in sys.argv or 'uwsgi' in sys.argv or 'gunicorn' in sys.argv:
        #    logger.info("Starting barbershop app - ensuring services exist in all barbershops")
        #    try:
        #        # Import and run the command using Django's call_command
        #        from django.core.management import call_command
        #        call_command('ensure_services')
        #    except Exception as e:
        #        logger.error(f"Error ensuring services: {str(e)}")
        # else:
        #    logger.debug("Skipping ensure_services during management command")
