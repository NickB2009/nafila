from typing import Any, Optional
from django.core.cache import cache
from application.interfaces import ICacheService


class DjangoCacheService(ICacheService):
    """Implementation of cache service using Django's cache framework"""
    
    def get(self, key: str) -> Any:
        """Get a value from cache"""
        return cache.get(key)
    
    def set(self, key: str, value: Any, timeout: int = None) -> None:
        """Set a value in cache with optional timeout"""
        if timeout is None:
            cache.set(key, value)
        else:
            cache.set(key, value, timeout)
    
    def delete(self, key: str) -> None:
        """Delete a value from cache"""
        cache.delete(key)
    
    def clear(self) -> None:
        """Clear all cache"""
        cache.clear()


class WaitTimeCache:
    """Specialized cache class for wait time operations"""
    
    def __init__(self, cache_service: ICacheService):
        self.cache_service = cache_service
    
    def get_wait_time(self, barbearia_id: str) -> Optional[int]:
        """Get cached wait time for a barbershop"""
        key = self._get_wait_time_key(barbearia_id)
        return self.cache_service.get(key)
    
    def set_wait_time(self, barbearia_id: str, wait_time: int, timeout: int = 30) -> None:
        """Set wait time for a barbershop with default 30 second timeout"""
        key = self._get_wait_time_key(barbearia_id)
        self.cache_service.set(key, wait_time, timeout)
    
    def invalidate_wait_time(self, barbearia_id: str) -> None:
        """Invalidate wait time cache for a barbershop"""
        key = self._get_wait_time_key(barbearia_id)
        self.cache_service.delete(key)
    
    def _get_wait_time_key(self, barbearia_id: str) -> str:
        """Generate cache key for wait time"""
        return f'tempo_espera:{barbearia_id}'


class BarbershopStatusCache:
    """Specialized cache class for barbershop status operations"""
    
    def __init__(self, cache_service: ICacheService):
        self.cache_service = cache_service
    
    def get_status(self, barbearia_id: str) -> Optional[bool]:
        """Get cached open/closed status for a barbershop"""
        key = self._get_status_key(barbearia_id)
        return self.cache_service.get(key)
    
    def set_status(self, barbearia_id: str, is_open: bool, timeout: int = 300) -> None:
        """Set open/closed status for a barbershop with default 5 minute timeout"""
        key = self._get_status_key(barbearia_id)
        self.cache_service.set(key, is_open, timeout)
    
    def invalidate_status(self, barbearia_id: str) -> None:
        """Invalidate status cache for a barbershop"""
        key = self._get_status_key(barbearia_id)
        self.cache_service.delete(key)
    
    def _get_status_key(self, barbearia_id: str) -> str:
        """Generate cache key for status"""
        return f'barbearia_status:{barbearia_id}' 