from typing import List, Dict, Any, Optional
from uuid import UUID

class QueueManager:
    """
    Domain service for managing queue operations.
    This class provides high-level queue management functionality.
    """
    
    @staticmethod
    def reorder_queue(queue_entries: List[Dict[str, Any]], barbershop_has_priority: bool = False) -> List[Dict[str, Any]]:
        """
        Reorder queue entries based on priority (if enabled) and arrival time.
        
        Args:
            queue_entries: List of queue entry dictionaries
            barbershop_has_priority: Whether the barbershop uses priority queue
            
        Returns:
            List of reordered queue entries
        """
        if not queue_entries:
            return []
            
        if barbershop_has_priority:
            # Sort by priority first, then by arrival time
            return sorted(queue_entries, key=lambda e: (-e.get('priority', 0), e.get('arrival_time')))
        else:
            # Sort by arrival time only (FIFO)
            return sorted(queue_entries, key=lambda e: e.get('arrival_time'))
    
    @staticmethod
    def calculate_positions(queue_entries: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """
        Calculate queue positions for each entry.
        
        Args:
            queue_entries: List of queue entry dictionaries
            
        Returns:
            List of queue entries with position field updated
        """
        for i, entry in enumerate(queue_entries, 1):
            entry['position'] = i
        return queue_entries
    
    @staticmethod
    def get_next_client(queue_entries: List[Dict[str, Any]], 
                        preferred_barber_id: Optional[UUID] = None) -> Optional[Dict[str, Any]]:
        """
        Get the next client in line, potentially considering barber preferences.
        
        Args:
            queue_entries: List of queue entry dictionaries
            preferred_barber_id: Optional barber ID to filter by
            
        Returns:
            Next queue entry or None if queue is empty
        """
        if not queue_entries:
            return None
            
        ordered_entries = QueueManager.reorder_queue(queue_entries)
        
        if preferred_barber_id:
            # First try to find clients who requested this barber
            for entry in ordered_entries:
                if entry.get('preferred_barber_id') == str(preferred_barber_id):
                    return entry
        
        # If none found or no preference specified, return the first entry
        return ordered_entries[0] if ordered_entries else None 