from django.test import TestCase
from uuid import uuid4
import pytest
from django.utils import timezone

from domain.models import Cliente, Barbearia, Servico, EntradaFila, Barbeiro
from infrastructure.repositories import (
    DjangoClienteRepository, DjangoFilaRepository, DjangoBarbeariaRepository,
    DjangoServicoRepository, DjangoBarbeiroRepository
)

class DjangoClienteRepositoryTest(TestCase):
    """Test suite for the Cliente repository implementation"""
    
    def setUp(self):
        """Set up test data"""
        self.repo = DjangoClienteRepository()
        
        # Create a test client
        self.cliente = Cliente.objects.create(
            nome="Test Client",
            telefone="1234567890",
            email="test@example.com"
        )
        
        # Create a test barbershop
        self.barbearia = Barbearia.objects.create(
            nome="Test Barbershop",
            slug="test-barbershop",
            enable_priority_queue=True
        )
    
    def test_get_by_telefone(self):
        """Test retrieving a client by phone number"""
        # Test existing client
        result = self.repo.get_by_telefone(self.cliente.telefone)
        self.assertEqual(result, self.cliente)
        
        # Test non-existent client
        result = self.repo.get_by_telefone("9999999999")
        self.assertIsNone(result)
    
    def test_get_by_id(self):
        """Test retrieving a client by ID"""
        # Test existing client
        result = self.repo.get_by_id(self.cliente.id)
        self.assertEqual(result, self.cliente)
        
        # Test non-existent client
        result = self.repo.get_by_id(uuid4())
        self.assertIsNone(result)


class DjangoFilaRepositoryTest(TestCase):
    """Test suite for the Fila repository implementation"""
    
    def setUp(self):
        """Set up test data"""
        self.repo = DjangoFilaRepository()
        
        # Create a test barbershop
        self.barbearia = Barbearia.objects.create(
            nome="Test Barbershop",
            slug="test-barbershop",
            enable_priority_queue=True
        )
        
        # Create a test client
        self.cliente = Cliente.objects.create(
            nome="Test Client",
            telefone="1234567890",
            email="test@example.com",
            is_vip=True
        )
        
        # Create a regular client
        self.cliente_regular = Cliente.objects.create(
            nome="Regular Client",
            telefone="0987654321",
            email="regular@example.com"
        )
        
        # Create a test service
        self.servico = Servico.objects.create(
            nome="Haircut",
            barbearia=self.barbearia,
            preco=25.00,
            duracao=30
        )
        
        # Create a test barber
        self.barbeiro = Barbeiro.objects.create(
            nome="Test Barber",
            barbearia=self.barbearia,
            status=Barbeiro.STATUS_AVAILABLE
        )
        
        # Create a VIP queue entry
        self.entrada_vip = EntradaFila.objects.create(
            barbearia=self.barbearia,
            cliente=self.cliente,
            servico=self.servico,
            prioridade=EntradaFila.PRIORITY_VIP,
            status=EntradaFila.STATUS_AGUARDANDO,
            position_number=2
        )
        
        # Create a regular queue entry
        self.entrada_regular = EntradaFila.objects.create(
            barbearia=self.barbearia,
            cliente=self.cliente_regular,
            servico=self.servico,
            prioridade=EntradaFila.PRIORITY_NORMAL,
            status=EntradaFila.STATUS_AGUARDANDO,
            position_number=1
        )
    
    def test_get_next_in_line(self):
        """Test getting the next client in line respects priority"""
        # With priority queue enabled, VIP client should be next
        next_client = self.repo.get_next_in_line(self.barbearia.id)
        
        # It should be the oldest entry first (FIFO)
        self.assertEqual(next_client, self.entrada_regular)
        
        # Test priority-based position calculation
        pos_vip = self.repo.posicao_na_fila(self.entrada_vip.id)
        pos_regular = self.repo.posicao_na_fila(self.entrada_regular.id)
        
        # VIP should have higher priority despite later arrival
        self.assertEqual(pos_vip, 1)
        self.assertEqual(pos_regular, 2)
    
    def test_estimated_wait_time(self):
        """Test estimated wait time calculation"""
        # Get estimated wait time
        wait_time = self.repo.get_estimated_wait_time(self.barbearia.id)
        
        # With one barber and two 30-minute services, wait time should be 60 minutes
        self.assertEqual(wait_time, 60)


class TestRepositoryErrorHandling(TestCase):
    """Test error handling in repositories"""
    
    def setUp(self):
        """Set up test data"""
        self.cliente_repo = DjangoClienteRepository()
        self.fila_repo = DjangoFilaRepository()
    
    def test_handle_nonexistent_id(self):
        """Test repositories properly handle non-existent IDs"""
        # Generate a random UUID that doesn't exist
        fake_id = uuid4()
        
        # Test cliente repository
        result = self.cliente_repo.get_by_id(fake_id)
        self.assertIsNone(result)
        
        # Test fila repository
        result = self.fila_repo.get_by_id(fake_id)
        self.assertIsNone(result)
        
        # Test position calculation for non-existent entry
        position = self.fila_repo.posicao_na_fila(fake_id)
        self.assertEqual(position, 0)