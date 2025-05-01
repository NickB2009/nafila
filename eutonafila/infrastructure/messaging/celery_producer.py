from typing import Any, Dict
from domain.interfaces.message_queue import MessageProducer
from infrastructure.messaging.celery_app import app


class CeleryMessageProducer(MessageProducer):
    """Celery implementation of the MessageProducer interface.
    This is in the infrastructure layer and handles the Celery-specific details."""

    def publish_message(self, queue_name: str, message_data: Dict[str, Any]) -> None:
        """Publish a message to a Celery queue.
        
        Args:
            queue_name: The name of the task to call
            message_data: The data to pass to the task
        """
        # In Celery, queue_name corresponds to the task name
        task = app.tasks.get(queue_name)
        if not task:
            raise ValueError(f"No task registered with name {queue_name}")
        
        # Send the task to Celery
        task.apply_async(kwargs=message_data, queue=queue_name.split('.')[-1]) 