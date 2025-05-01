"""
Utility module for standardized error handling across the application.
"""

import logging
from typing import Dict, Any, Tuple, Optional, Callable
from functools import wraps

logger = logging.getLogger(__name__)

def api_error_response(message: str, status_code: int = 400, errors: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
    """
    Standard error response format for API endpoints.
    
    Args:
        message: Human-readable error message
        status_code: HTTP status code
        errors: Optional dictionary of field-specific errors
        
    Returns:
        Dict containing error information in consistent format
    """
    response = {
        'success': False,
        'message': message,
        'status_code': status_code
    }
    
    if errors:
        response['errors'] = errors
        
    return response


def log_and_format_error(e: Exception, default_message: str = "An unexpected error occurred") -> Dict[str, Any]:
    """
    Log an exception and format it for API response.
    
    Args:
        e: The exception that was raised
        default_message: Default message if exception has no message
        
    Returns:
        Formatted error response
    """
    logger.error(f"Error: {str(e)}", exc_info=True)
    
    message = str(e) if str(e) else default_message
    return api_error_response(message)


def exception_handler(func: Callable) -> Callable:
    """
    Decorator to standardize exception handling for service methods.
    
    Args:
        func: The function to decorate
        
    Returns:
        Decorated function with standardized exception handling
    """
    @wraps(func)
    def wrapper(*args, **kwargs):
        try:
            return func(*args, **kwargs)
        except Exception as e:
            logger.error(f"Error in {func.__name__}: {str(e)}", exc_info=True)
            return False, f"Ocorreu um erro: {str(e)}"
    return wrapper


def transaction_handler(func: Callable) -> Callable:
    """
    Decorator to manage database transactions for service methods.
    
    Args:
        func: The function to decorate
        
    Returns:
        Decorated function with transaction handling
    """
    from django.db import transaction
    
    @wraps(func)
    def wrapper(*args, **kwargs):
        try:
            with transaction.atomic():
                return func(*args, **kwargs)
        except Exception as e:
            logger.error(f"Transaction error in {func.__name__}: {str(e)}", exc_info=True)
            return False, f"Erro na transação: {str(e)}"
    return wrapper 