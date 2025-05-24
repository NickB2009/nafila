"""
Celery stub to avoid dependency issues
"""
import os

# Create a stub Celery app
class CeleryStub:
    def __init__(self, name):
        self.name = name
        self.conf = type('ConfStub', (), {
            'task_serializer': 'json',
            'result_serializer': 'json',
            'accept_content': ['json'],
        })
    
    def task(self, *args, **kwargs):
        # Return a decorator that does nothing
        def decorator(func):
            return func
        return decorator
        
    def config_from_object(self, *args, **kwargs):
        # Do nothing
        pass
        
    def autodiscover_tasks(self, *args, **kwargs):
        # Do nothing
        pass

# Set default Django settings module
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')

# Create the Celery app stub
app = CeleryStub('eutonafila')

# Define a no-op debug task
def debug_task(self):
    print(f'Debug task stub') 