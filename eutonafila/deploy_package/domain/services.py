from datetime import datetime, time
from typing import List, Optional
from .entities import (
    Barbearia, Barbeiro, BarbeiroStatus,
    EntradaFila, FilaStatus, FilaPrioridade,
    Servico
)
from .services.wait_time_calculator import WaitTimeCalculator

class OpeningHoursValidator:
    """Service for validating if a barbershop is open"""
    
    @staticmethod
    def is_open(current_weekday: int, current_time: time, weekdays: List[int], 
               opening_time: time, closing_time: time) -> bool:
        """Check if a barbershop is currently open"""
        # Check if today is a working day
        if current_weekday not in weekdays:
            return False
            
        # Check if current time is within opening hours
        return opening_time <= current_time < closing_time

class QueueManager:
    """Service for managing queue operations"""
    
    @staticmethod
    def get_position(entrada: EntradaFila, queue_entries: List[EntradaFila]) -> int:
        """Get position in queue based on priority and arrival time"""
        if not queue_entries:
            return 1
            
        # Count entries with higher priority
        higher_priority_count = sum(1 for e in queue_entries if e.prioridade.value > entrada.prioridade.value)
        
        # Count entries with same priority but earlier arrival
        same_priority_earlier_count = sum(1 for e in queue_entries 
                                       if e.prioridade == entrada.prioridade and e.horario_chegada < entrada.horario_chegada)
        
        # Position is 1-based
        return higher_priority_count + same_priority_earlier_count + 1
    
    @staticmethod
    def calculate_priority(entrada: EntradaFila, cliente_visits: int, is_vip: bool) -> FilaPrioridade:
        """Calculate priority based on client status"""
        if is_vip:
            return FilaPrioridade.VIP
        elif cliente_visits >= 20:
            return FilaPrioridade.GOLD
        elif cliente_visits >= 10:
            return FilaPrioridade.SILVER
        elif cliente_visits >= 5:
            return FilaPrioridade.BRONZE
        return FilaPrioridade.NORMAL
        
    @staticmethod
    def estimate_service_duration(servico: Servico, barbeiro: Optional[Barbeiro] = None) -> int:
        """Estimate service duration in minutes"""
        base_duration = servico.duracao
        
        # Apply adjustments based on service complexity
        # More complex services have more variability
        
        return base_duration
    
    @staticmethod
    def get_active_barbers_count(barbers: List[Barbeiro]) -> int:
        """Count active barbers (available or busy)"""
        return sum(1 for b in barbers if b.status in [BarbeiroStatus.AVAILABLE, BarbeiroStatus.BUSY]) 