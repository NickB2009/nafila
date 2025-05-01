from abc import ABC, abstractmethod
from typing import Any, Dict


class MessageProducer(ABC):
    """Abstract interface for producing messages to a queue.
    This is in the domain layer and doesn't know about RabbitMQ or Celery."""
    
    @abstractmethod
    def publish_message(self, queue_name: str, message_data: Dict[str, Any]) -> None:
        """Publish a message to the specified queue.
        
        Args:
            queue_name: The name of the queue to publish to
            message_data: The data to publish as a message
        """
        pass


class MessageConsumer(ABC):
    """Abstract interface for consuming messages from a queue."""
    
    @abstractmethod
    def register_handler(self, queue_name: str, handler_func) -> None:
        """Register a handler function for a specific queue.
        
        Args:
            queue_name: The name of the queue to consume from
            handler_func: The function to call when a message is received
        """
        pass
    
    @abstractmethod
    def start_consuming(self) -> None:
        """Start consuming messages from registered queues."""
        pass 