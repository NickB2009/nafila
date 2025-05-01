from typing import Optional
from uuid import UUID

class FilaService:
    def get_next_cliente(self, barbearia_id: UUID) -> Optional[EntradaFila]:
        """Get the next client in line."""
        return self.fila_repository.get_next_in_line(barbearia_id) 