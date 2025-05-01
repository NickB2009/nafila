from celery import Celery
import os

# Set default Django settings module for the Celery application
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')

# Create the Celery app
app = Celery('eutonafila')

# Load settings from Django settings.py
app.config_from_object('django.conf:settings', namespace='CELERY')

# No need to set broker URL explicitly as it's loaded from settings
# app.conf.broker_url = os.environ.get('RABBITMQ_URL', 'amqp://guest:guest@localhost:5672//')

# Configure Celery to serialize tasks using JSON
app.conf.task_serializer = 'json'
app.conf.result_serializer = 'json'
app.conf.accept_content = ['json']

# Auto-discover tasks from all registered Django app configs
app.autodiscover_tasks()


@app.task(bind=True)
def debug_task(self):
    print(f'Request: {self.request!r}') 