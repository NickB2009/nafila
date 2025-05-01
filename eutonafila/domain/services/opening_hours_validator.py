from datetime import time
from typing import List

class OpeningHoursValidator:
    """
    Domain service for validating business hours.
    This is the canonical implementation to be used throughout the application.
    """
    
    @staticmethod
    def is_open(current_weekday: int, current_time: time, weekdays: List[int], 
                opening_time: time, closing_time: time) -> bool:
        """
        Check if a business is currently open based on weekday and time.
        
        Args:
            current_weekday: Current day of the week (0=Monday, 6=Sunday)
            current_time: Current time
            weekdays: List of days the business is open (0=Monday, 6=Sunday)
            opening_time: Business opening time
            closing_time: Business closing time
            
        Returns:
            bool: True if business is currently open, False otherwise
        """
        # Check if current day is a working day
        if current_weekday not in weekdays:
            return False
            
        # Check if current time is within opening hours
        return opening_time <= current_time < closing_time 