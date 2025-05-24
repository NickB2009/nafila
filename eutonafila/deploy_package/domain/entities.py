from dataclasses import dataclass
from datetime import datetime
from enum import Enum
from typing import List, Optional, Dict

class ClienteStatus(Enum):
    REGULAR = "regular"
    BRONZE = "bronze"
    SILVER = "silver"
    GOLD = "gold"
    VIP = "vip"
    
    @classmethod
    def choices(cls):
        """Return choices with proper string handling for all value types"""
        result = []
        for status in cls:
            # Safely convert name to string and then format
            name = str(status.name) if status.name is not None else ""
            result.append((status.value, name.title()))
        return result

@dataclass
class Cliente:
    """Pure domain entity for Cliente"""
    id: str
    nome: str
    telefone: str
    email: Optional[str]
    total_visits: int = 0
    is_vip: bool = False
    last_visit: Optional[datetime] = None
    
    @property
    def is_returning_customer(self) -> bool:
        return self.total_visits > 0
    
    @property
    def loyalty_level(self) -> ClienteStatus:
        if self.total_visits >= 20:
            return ClienteStatus.GOLD
        elif self.total_visits >= 10:
            return ClienteStatus.SILVER
        elif self.total_visits >= 5:
            return ClienteStatus.BRONZE
        return ClienteStatus.REGULAR

class BarbeiroStatus(Enum):
    AVAILABLE = "available"
    BUSY = "busy"
    BREAK = "break"
    OFFLINE = "offline"
    
    @classmethod
    def is_active(cls, status):
        # Handle integer status
        if isinstance(status, int):
            return False  # Invalid status
        
        # Handle the status object itself
        if isinstance(status, cls):
            status = status.value
            
        return status in [cls.AVAILABLE.value, cls.BUSY.value]
    
    @classmethod
    def choices(cls):
        """Return choices with proper string handling for all value types"""
        result = []
        for status in cls:
            # Safely convert name to string and then format
            name = str(status.name) if status.name is not None else ""
            result.append((status.value, name.title()))
        return result

@dataclass
class Barbeiro:
    """Pure domain entity for Barbeiro"""
    id: str
    nome: str
    status: BarbeiroStatus
    especialidades: List[str]  # List of service IDs
    barbearia_id: str

class FilaStatus(Enum):
    AGUARDANDO = "waiting"
    ATENDIMENTO = "in_service"
    FINALIZADO = "completed"
    CANCELADO = "cancelled"
    AUSENTE = "no_show"
    
    @classmethod
    def choices(cls):
        """Return choices with proper string handling for all value types"""
        result = []
        for status in cls:
            # Safely convert name to string and then format
            name = str(status.name) if status.name is not None else ""
            result.append((status.value, name.title()))
        return result

class FilaPrioridade(Enum):
    NORMAL = 1
    BRONZE = 2
    SILVER = 3
    GOLD = 4
    VIP = 5
    
    @classmethod
    def choices(cls):
        """Return choices with proper string handling for all value types"""
        result = []
        for status in cls:
            # Safely convert name to string and then format
            name = str(status.name) if status.name is not None else ""
            result.append((status.value, name.title()))
        return result

@dataclass
class EntradaFila:
    """Pure domain entity for queue entry"""
    id: str
    barbearia_id: str
    cliente_id: str
    servico_id: str
    barbeiro_id: Optional[str]
    status: FilaStatus
    prioridade: FilaPrioridade
    horario_chegada: datetime
    horario_atendimento: Optional[datetime]
    horario_finalizacao: Optional[datetime]
    estimativa_duracao: int  # minutes
    position_number: int
    notified: bool = False
    
    def get_time_in_queue(self, current_time: datetime) -> int:
        """Calculate time spent in queue in minutes"""
        if self.status != FilaStatus.AGUARDANDO:
            return 0
        delta = current_time - self.horario_chegada
        return int(delta.total_seconds() / 60)

class ServicoComplexidade(Enum):
    SIMPLE = 1
    MEDIUM = 2
    COMPLEX = 3
    
    @classmethod
    def choices(cls):
        """Return choices with proper string handling for all value types"""
        result = []
        for status in cls:
            # Safely convert name to string and then format
            name = str(status.name) if status.name is not None else ""
            result.append((status.value, name.title()))
        return result

@dataclass
class Servico:
    """Pure domain entity for Servico"""
    id: str
    nome: str
    descricao: Optional[str]
    preco: float
    duracao: int  # minutes
    complexity: ServicoComplexidade
    barbearia_id: str
    popularity: int = 0

@dataclass
class Barbearia:
    """Pure domain entity for Barbearia"""
    id: str
    nome: str
    slug: str
    telefone: Optional[str]
    endereco: Optional[str]
    horario_abertura: str
    horario_fechamento: str
    dias_funcionamento: List[int]  # 0=Monday, 6=Sunday
    max_capacity: int = 10
    enable_priority_queue: bool = False 