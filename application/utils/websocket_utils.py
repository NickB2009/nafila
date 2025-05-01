"""
Utilities for more robust WebSocket notifications.
"""

import logging
import json
import time
from typing import Dict, Any, Optional, List
from functools import wraps
from channels.layers import get_channel_layer
from asgiref.sync import async_to_sync
from django.core.cache import cache

logger = logging.getLogger(__name__)

class WebSocketNotifier:
    """
    Helper class for robust WebSocket notifications with retries and error handling.
    """
    
    def __init__(self, max_retries: int = 3, retry_delay: float = 0.5):
        """
        Initialize the WebSocket notifier.
        
        Args:
            max_retries: Maximum number of retry attempts
            retry_delay: Delay between retries in seconds
        """
        self.max_retries = max_retries
        self.retry_delay = retry_delay
        self.channel_layer = get_channel_layer()
    
    def send_to_group(self, group_name: str, message: Dict[str, Any]) -> bool:
        """
        Send a message to a channel group with retry logic.
        
        Args:
            group_name: The name of the group to send to
            message: The message to send
            
        Returns:
            True if successful, False otherwise
        """
        if not message.get('type'):
            logger.error(f"Missing 'type' in message: {message}")
            return False
        
        attempt = 0
        while attempt < self.max_retries:
            try:
                async_to_sync(self.channel_layer.group_send)(group_name, message)
                return True
            except Exception as e:
                attempt += 1
                if attempt >= self.max_retries:
                    logger.error(f"Failed to send WebSocket message to {group_name} after {self.max_retries} attempts: {e}")
                    return False
                
                logger.warning(f"WebSocket send attempt {attempt} failed, retrying: {e}")
                time.sleep(self.retry_delay)
        
        return False
    
    def notify_queue_update(self, barbearia_slug: str, action: str = 'queue_changed') -> bool:
        """
        Notify all clients connected to a barbershop about a queue update.
        
        Args:
            barbearia_slug: The slug of the barbershop
            action: The action that occurred
            
        Returns:
            True if successful, False otherwise
        """
        message = {
            'type': 'queue_update',
            'action': action
        }
        
        return self.send_to_group(f'barbershop_{barbearia_slug}', message)
    
    def notify_queue_entry(self, queue_id: str, status: str, position: int, wait_time: str) -> bool:
        """
        Notify a specific queue entry about status changes.
        
        Args:
            queue_id: The ID of the queue entry
            status: The new status
            position: The position in queue
            wait_time: Formatted wait time
            
        Returns:
            True if successful, False otherwise
        """
        message = {
            'type': 'queue_update',
            'action': 'status_update',
            'status': status,
            'position': position,
            'wait_time': wait_time
        }
        
        # Set a cache key to track this notification was sent
        cache_key = f'notification_sent:{queue_id}:{status}'
        cache.set(cache_key, True, timeout=3600)  # Cache for 1 hour
        
        return self.send_to_group(f'queue_{queue_id}', message)
    
    def broadcast_barber_status(self, barbearia_slug: str, barbeiro_id: str, status: str) -> bool:
        """
        Broadcast a barber's status change to all clients.
        
        Args:
            barbearia_slug: The slug of the barbershop
            barbeiro_id: The ID of the barber
            status: The new status
            
        Returns:
            True if successful, False otherwise
        """
        message = {
            'type': 'barber_update',
            'barber_id': barbeiro_id,
            'status': status
        }
        
        return self.send_to_group(f'barbershop_{barbearia_slug}', message)


def retry_websocket_notification(max_retries: int = 3):
    """
    Decorator to retry WebSocket notifications.
    
    Args:
        max_retries: Maximum number of retry attempts
        
    Returns:
        Decorated function
    """
    def decorator(func):
        @wraps(func)
        def wrapper(*args, **kwargs):
            last_exception = None
            for attempt in range(max_retries):
                try:
                    return func(*args, **kwargs)
                except Exception as e:
                    last_exception = e
                    logger.warning(f"WebSocket notification attempt {attempt+1} failed: {e}")
                    time.sleep(0.5)  # Short delay before retry
            
            logger.error(f"WebSocket notification failed after {max_retries} attempts: {last_exception}")
            return False  # Return False to indicate failure
        return wrapper
    return decorator


# Create a singleton instance for easy import
notifier = WebSocketNotifier() 