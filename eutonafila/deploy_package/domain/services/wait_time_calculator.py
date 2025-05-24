from typing import List

class WaitTimeCalculator:
    """
    Domain service for calculating and formatting queue wait times.
    This is the canonical implementation to be used throughout the application.
    """
    
    @staticmethod
    def calculate(service_durations: List[int], active_barber_count: int) -> int:
        """
        Calculate wait time in minutes.
        
        Args:
            service_durations: List of service durations in minutes
            active_barber_count: Number of active barbers
            
        Returns:
            Estimated wait time in minutes
        """
        if not service_durations:
            return 0
            
        if active_barber_count <= 0:
            active_barber_count = 1  # Avoid division by zero
            
        total_service_time = sum(service_durations)
        return total_service_time // active_barber_count
    
    @staticmethod
    def format_wait_time(minutes: int) -> str:
        """
        Format wait time into human-readable string.
        
        Args:
            minutes: Wait time in minutes
            
        Returns:
            Formatted wait time string
        """
        if minutes == 0:
            return "Sem espera"
        elif minutes < 60:
            return f"{minutes} minutos"
        else:
            hours = minutes // 60
            remaining_mins = minutes % 60
            
            if remaining_mins == 0:
                return f"{hours} hora{'s' if hours > 1 else ''}"
            
            return f"{hours}h e {remaining_mins}min" 