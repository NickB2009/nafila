from django.test import TestCase
from unittest.mock import Mock, patch
from uuid import uuid4

from domain.models import Cliente, Barbearia, Servico, EntradaFila
from application.services import FilaService
from application.dtos import ClienteDTO, CheckInDTO

class FilaServiceTests(TestCase):
    def setUp(self):
        # Create mock repositories
        self.cliente_repo_mock = Mock()
        self.fila_repo_mock = Mock()
        self.barbearia_repo_mock = Mock()
        self.servico_repo_mock = Mock()
        
        # Create service with mock repositories
        self.service = FilaService(
            cliente_repository=self.cliente_repo_mock,
            fila_repository=self.fila_repo_mock,
            barbearia_repository=self.barbearia_repo_mock,
            servico_repository=self.servico_repo_mock
        )
        
        # Mock data
        self.cliente_id = uuid4()
        self.barbearia_id = uuid4()
        self.servico_id = uuid4()
        
        # Setup Cliente mock
        self.cliente_mock = Mock(spec=Cliente)
        self.cliente_mock.id = self.cliente_id
        self.cliente_mock.nome = "Cliente Teste"
        self.cliente_mock.telefone = "11999999999"
        
        # Setup Barbearia mock
        self.barbearia_mock = Mock(spec=Barbearia)
        self.barbearia_mock.id = self.barbearia_id
        self.barbearia_mock.nome = "Barbearia Teste"
        self.barbearia_mock.slug = "barbearia-teste"
        
        # Setup Servico mock
        self.servico_mock = Mock(spec=Servico)
        self.servico_mock.id = self.servico_id
        self.servico_mock.nome = "Corte"
        self.servico_mock.barbearia = self.barbearia_mock
        self.servico_mock.duracao = 30
    
    def test_cancel_queue_entry_success(self):
        """Test successful queue entry cancellation"""
        # Setup mock queue entry
        entrada_mock = Mock(spec=EntradaFila)
        entrada_mock.id = uuid4()
        entrada_mock.status = EntradaFila.STATUS_AGUARDANDO
        entrada_mock.cancelar.return_value = True
        entrada_mock.barbearia.slug = "barbearia-teste"
        
        # Configure repository mock
        self.fila_repo_mock.get_by_id.return_value = entrada_mock
        
        # Execute service with patch for notification
        with patch.object(self.service, '_notify_queue_update'):
            success, message = self.service.cancel_queue_entry(entrada_mock.id)
        
        # Assertions
        self.assertTrue(success)
        self.assertEqual(message, "Atendimento cancelado com sucesso.")
        
        # Verify calls
        self.fila_repo_mock.get_by_id.assert_called_once_with(entrada_mock.id)
        entrada_mock.cancelar.assert_called_once()
        self.fila_repo_mock.update.assert_called_once_with(entrada_mock)
    
    def test_cancel_queue_entry_not_found(self):
        """Test cancellation when queue entry is not found"""
        # Configure repository mock
        queue_id = uuid4()
        self.fila_repo_mock.get_by_id.return_value = None
        
        # Execute service
        success, message = self.service.cancel_queue_entry(queue_id)
        
        # Assertions
        self.assertFalse(success)
        self.assertEqual(message, "Entrada na fila não encontrada.")
        
        # Verify calls
        self.fila_repo_mock.get_by_id.assert_called_once_with(queue_id)
        self.fila_repo_mock.update.assert_not_called()
    
    def test_cancel_queue_entry_invalid_status(self):
        """Test cancellation when queue entry has an invalid status"""
        # Setup mock queue entry with non-waiting status
        entrada_mock = Mock(spec=EntradaFila)
        entrada_mock.id = uuid4()
        entrada_mock.status = EntradaFila.STATUS_FINALIZADO  # Already completed
        
        # Configure repository mock
        self.fila_repo_mock.get_by_id.return_value = entrada_mock
        
        # Execute service
        success, message = self.service.cancel_queue_entry(entrada_mock.id)
        
        # Assertions
        self.assertFalse(success)
        self.assertEqual(message, "Não é possível cancelar um atendimento que não está em espera.")
        
        # Verify calls
        self.fila_repo_mock.get_by_id.assert_called_once_with(entrada_mock.id)
        entrada_mock.cancelar.assert_not_called()
        self.fila_repo_mock.update.assert_not_called() 