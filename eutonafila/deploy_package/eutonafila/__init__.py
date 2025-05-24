"""
EuTôNaFila - Queue Management System for Barbershops
"""

# Import the Celery app instance when Django starts
from infrastructure.messaging.celery_app import app as celery_app

__all__ = ('celery_app',)
