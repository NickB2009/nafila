from dataclasses import dataclass
from uuid import UUID
from typing import Optional

@dataclass
class ClienteDTO:
    nome: str
    telefone: str
    email: str = ""
    id: Optional[UUID] = None

@dataclass
class CheckInDTO:
    cliente: ClienteDTO
    barbearia_slug: str
    servico_id: UUID
    notification_sms: bool = False  # Whether to send SMS notification
    notification_email: bool = False  # Whether to send email notification 