from dataclasses import dataclass, field
from typing import List, Optional
from uuid import UUID, uuid4
from datetime import time

@dataclass
class Barbearia:
    """
    Barbearia (Barbershop) domain entity.
    This is a pure domain entity with business logic.
    It does not depend on any external frameworks or libraries.
    """
    nome: str
    slug: str
    horario_abertura: time
    horario_fechamento: time
    dias_funcionamento: List[int]
    max_capacity: int
    id: UUID = field(default_factory=uuid4)
    telefone: Optional[str] = None
    endereco: Optional[str] = None
    descricao_curta: Optional[str] = None
    cores: List[str] = field(default_factory=list)
    logo_url: Optional[str] = None
    enable_priority_queue: bool = False
    
    def esta_aberto(self, current_weekday: int, current_time: time) -> bool:
        """
        Check if the barbershop is currently open.
        
        Args:
            current_weekday: Current day of the week (0=Monday, 6=Sunday)
            current_time: Current time
            
        Returns:
            bool: True if open, False otherwise
        """
        # Check if current day is a working day
        if current_weekday not in self.dias_funcionamento:
            return False
            
        # Check if current time is within opening hours
        return self.horario_abertura <= current_time < self.horario_fechamento
    
    def is_queue_full(self, waiting_count: int) -> bool:
        """
        Check if the queue is full based on max capacity.
        
        Args:
            waiting_count: Number of clients currently waiting
            
        Returns:
            bool: True if queue is full, False otherwise
        """
        return waiting_count >= self.max_capacity
    
    def validate(self) -> List[str]:
        """
        Validate the entity data.
        
        Returns:
            List[str]: List of validation errors (empty if valid)
        """
        errors = []
        
        if not self.nome:
            errors.append("Nome is required")
            
        if not self.slug:
            errors.append("Slug is required")
            
        if not self.dias_funcionamento:
            errors.append("At least one working day is required")
            
        if self.max_capacity <= 0:
            errors.append("Max capacity must be greater than zero")
            
        if self.horario_abertura >= self.horario_fechamento:
            errors.append("Opening time must be before closing time")
            
        return errors 