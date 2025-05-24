from django.test import TestCase
from uuid import uuid4

from domain.models import Cliente, Barbearia, Servico, EntradaFila
from application.dtos import ClienteDTO
from infrastructure.repositories import (
    DjangoClienteRepository,
    DjangoFilaRepository,
    DjangoBarbeariaRepository,
    DjangoServicoRepository
)

class ClienteRepositoryTests(TestCase):
    """Tests for the Cliente repository implementation"""
    
    def setUp(self):
        self.repository = DjangoClienteRepository()
        self.cliente = Cliente.objects.create(
            nome="Cliente Teste",
            telefone="11999999999",
            email="cliente@teste.com"
        )
    
    def test_get_by_telefone_existing(self):
        """Test retrieving an existing client by phone number"""
        cliente = self.repository.get_by_telefone("11999999999")
        self.assertEqual(cliente, self.cliente)
        self.assertEqual(cliente.nome, "Cliente Teste")
        self.assertEqual(cliente.email, "cliente@teste.com")
    
    def test_get_by_telefone_not_found(self):
        """Test retrieving a non-existent client"""
        cliente = self.repository.get_by_telefone("11888888888")
        self.assertIsNone(cliente)
    
    def test_criar(self):
        """Test creating a new client"""
        dto = ClienteDTO(
            nome="Novo Cliente", 
            telefone="11777777777",
            email="novo@example.com"
        )
        
        cliente = self.repository.criar(dto)
        
        self.assertIsNotNone(cliente.id)
        self.assertEqual(cliente.nome, "Novo Cliente")
        self.assertEqual(cliente.telefone, "11777777777")
        self.assertEqual(cliente.email, "novo@example.com")
        
        # Verify it's in the database
        self.assertTrue(Cliente.objects.filter(telefone="11777777777").exists())

class BarbeariaRepositoryTests(TestCase):
    """Tests for the Barbearia repository implementation"""
    
    def setUp(self):
        self.repository = DjangoBarbeariaRepository()
        self.barbearia = Barbearia.objects.create(
            nome="Barbearia Teste",
            slug="barbearia-teste",
            telefone="1133333333",
            cores=["#000000", "#ffffff"]
        )
    
    def test_get_by_slug_existing(self):
        """Test retrieving an existing barbershop by slug"""
        barbearia = self.repository.get_by_slug("barbearia-teste")
        self.assertEqual(barbearia, self.barbearia)
        self.assertEqual(barbearia.nome, "Barbearia Teste")
        self.assertEqual(barbearia.telefone, "1133333333")
        self.assertEqual(barbearia.cores, ["#000000", "#ffffff"])
    
    def test_get_by_slug_not_found(self):
        """Test retrieving a non-existent barbershop"""
        barbearia = self.repository.get_by_slug("inexistente")
        self.assertIsNone(barbearia)

class ServicoRepositoryTests(TestCase):
    """Tests for the Servico repository implementation"""
    
    def setUp(self):
        self.repository = DjangoServicoRepository()
        self.barbearia = Barbearia.objects.create(
            nome="Barbearia Teste",
            slug="barbearia-teste"
        )
        self.servico = Servico.objects.create(
            nome="Corte",
            barbearia=self.barbearia,
            preco=30.00,
            duracao=30
        )
    
    def test_get_by_id_existing(self):
        """Test retrieving an existing service by ID"""
        servico = self.repository.get_by_id(self.servico.id)
        self.assertEqual(servico, self.servico)
        self.assertEqual(servico.nome, "Corte")
        self.assertEqual(servico.preco, 30.00)
        self.assertEqual(servico.duracao, 30)
        
        # Test eager loading of related barbearia
        self.assertEqual(servico.barbearia, self.barbearia)
    
    def test_get_by_id_not_found(self):
        """Test retrieving a non-existent service"""
        servico = self.repository.get_by_id(uuid4())
        self.assertIsNone(servico)

class FilaRepositoryTests(TestCase):
    """Tests for the Fila repository implementation"""
    
    def setUp(self):
        self.repository = DjangoFilaRepository()
        
        self.barbearia = Barbearia.objects.create(
            nome="Barbearia Teste",
            slug="barbearia-teste"
        )
        
        self.cliente1 = Cliente.objects.create(
            nome="Cliente 1",
            telefone="11111111111"
        )
        
        self.cliente2 = Cliente.objects.create(
            nome="Cliente 2",
            telefone="22222222222"
        )
        
        self.servico = Servico.objects.create(
            nome="Corte",
            barbearia=self.barbearia,
            preco=30.00,
            duracao=30
        )
        
        # Create queue entries with specific ordering
        self.entrada1 = EntradaFila.objects.create(
            barbearia=self.barbearia,
            cliente=self.cliente1,
            servico=self.servico,
            status=EntradaFila.STATUS_AGUARDANDO
        )
        
        # Force a later timestamp for the second entry
        import time
        time.sleep(0.001)
        
        self.entrada2 = EntradaFila.objects.create(
            barbearia=self.barbearia,
            cliente=self.cliente2,
            servico=self.servico,
            status=EntradaFila.STATUS_AGUARDANDO
        )
    
    def test_adicionar_cliente(self):
        """Test adding a client to the queue"""
        new_cliente = Cliente.objects.create(
            nome="Novo Cliente",
            telefone="33333333333"
        )
        
        new_entrada = EntradaFila(
            barbearia=self.barbearia,
            cliente=new_cliente,
            servico=self.servico
        )
        
        result = self.repository.adicionar_cliente(new_entrada)
        
        self.assertIsNotNone(result.id)
        self.assertEqual(result.barbearia, self.barbearia)
        self.assertEqual(result.cliente, new_cliente)
    
    def test_clientes_aguardando(self):
        """Test retrieving waiting clients"""
        # Create a non-waiting entry that should be excluded
        EntradaFila.objects.create(
            barbearia=self.barbearia,
            cliente=self.cliente1,
            servico=self.servico,
            status=EntradaFila.STATUS_CANCELADO
        )
        
        clientes = self.repository.clientes_aguardando(self.barbearia.id)
        
        self.assertEqual(len(clientes), 2)
        self.assertIn(self.entrada1, clientes)
        self.assertIn(self.entrada2, clientes)
    
    def test_posicao_na_fila(self):
        """Test position calculation in queue"""
        # First client should be at position 1
        posicao1 = self.repository.posicao_na_fila(self.entrada1.id)
        self.assertEqual(posicao1, 1)
        
        # Second client should be at position 2
        posicao2 = self.repository.posicao_na_fila(self.entrada2.id)
        self.assertEqual(posicao2, 2)
        
        # Test with non-existent entry
        posicao_inexistente = self.repository.posicao_na_fila(uuid4())
        self.assertEqual(posicao_inexistente, 0) 