from django.test import TestCase
from django.utils import timezone
from django.contrib.auth.models import User
import uuid
from datetime import time, timedelta
from unittest.mock import patch, MagicMock

from barbershop.models import Barbearia, Barbeiro, Servico, Cliente, Fila
from domain.domain_models import WaitTimeCalculator
from domain.domain_models import EntradaFila as DomainEntradaFila
from domain.domain_models import Barbeiro as DomainBarbeiro


class WaitTimeCalculatorTests(TestCase):
    """Test the domain wait time calculator logic"""
    
    def test_calculate_empty_list(self):
        """Test calculation with no services"""
        result = WaitTimeCalculator.calculate([], 5)
        self.assertEqual(result, 0)
    
    def test_calculate_no_barbers(self):
        """Test calculation with zero barbers (should avoid division by zero)"""
        result = WaitTimeCalculator.calculate([10, 15, 20], 0)
        self.assertEqual(result, 45)  # Sum of durations since barber count becomes 1
    
    def test_calculate_normal_case(self):
        """Test normal calculation case"""
        durations = [30, 15, 45, 30]  # Total: 120 minutes
        barbers = 2
        expected = 60  # 120 / 2 = 60
        
        result = WaitTimeCalculator.calculate(durations, barbers)
        self.assertEqual(result, expected)
    
    def test_format_wait_time_zero(self):
        """Test formatting zero wait time"""
        result = WaitTimeCalculator.format_wait_time(0)
        self.assertEqual(result, "Sem espera")
    
    def test_format_wait_time_minutes(self):
        """Test formatting wait time in minutes"""
        result = WaitTimeCalculator.format_wait_time(45)
        self.assertEqual(result, "45 minutos")
    
    def test_format_wait_time_hours(self):
        """Test formatting wait time in hours and minutes"""
        result = WaitTimeCalculator.format_wait_time(125)
        self.assertEqual(result, "2h e 5min")


class BarbeariaWaitTimeTests(TestCase):
    """Test wait time calculation integration in the Barbershop model"""
    
    def setUp(self):
        """Set up test data"""
        # Create user
        self.user = User.objects.create_user('testuser', 'test@example.com', 'password')
        
        # Create barbershop
        self.barbearia = Barbearia.objects.create(
            nome="Test Barbershop",
            slug="test-barbershop",
            horario_abertura=time(9, 0),
            horario_fechamento=time(18, 0),
            dias_funcionamento=[0, 1, 2, 3, 4],  # Monday-Friday
            user=self.user
        )
        
        # Create barbers
        self.barber1 = Barbeiro.objects.create(
            usuario=self.user,
            barbearia=self.barbearia,
            nome="Barber 1",
            status=DomainBarbeiro.Status.STATUS_AVAILABLE.value
        )
        
        # Create services
        self.service1 = Servico.objects.create(
            barbearia=self.barbearia,
            nome="Haircut",
            preco=30.00,
            duracao=30  # 30 minutes
        )
        
        self.service2 = Servico.objects.create(
            barbearia=self.barbearia,
            nome="Beard Trim",
            preco=20.00,
            duracao=15  # 15 minutes
        )
        
        # Create client
        self.client1 = Cliente.objects.create(
            nome="Test Client",
            telefone="123456789"
        )
    
    def test_empty_queue_wait_time(self):
        """Test wait time with empty queue"""
        # No entries in queue
        wait_time = self.barbearia.calcular_tempo_espera()
        self.assertEqual(wait_time, 0)
        self.assertEqual(self.barbearia.tempo_espera_estimado, "Sem espera")
    
    def test_wait_time_with_one_client(self):
        """Test wait time with one client in queue"""
        # Add one client to queue
        Fila.objects.create(
            barbearia=self.barbearia,
            cliente=self.client1,
            servico=self.service1,  # 30 minutes
            status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value
        )
        
        # With one barber and one client, wait time should be service duration
        wait_time = self.barbearia.calcular_tempo_espera()
        self.assertEqual(wait_time, 30)
        self.assertEqual(self.barbearia.tempo_espera_estimado, "30 minutos")
    
    def test_wait_time_with_multiple_clients(self):
        """Test wait time with multiple clients in queue"""
        # Add multiple clients to queue
        Fila.objects.create(
            barbearia=self.barbearia,
            cliente=self.client1,
            servico=self.service1,  # 30 minutes
            status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value
        )
        
        Fila.objects.create(
            barbearia=self.barbearia,
            cliente=self.client1,
            servico=self.service2,  # 15 minutes
            status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value
        )
        
        Fila.objects.create(
            barbearia=self.barbearia,
            cliente=self.client1,
            servico=self.service1,  # 30 minutes
            status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value
        )
        
        # Total: 75 minutes with 1 barber
        wait_time = self.barbearia.calcular_tempo_espera()
        self.assertEqual(wait_time, 75)
        self.assertEqual(self.barbearia.tempo_espera_estimado, "1h e 15min")
    
    def test_wait_time_with_multiple_barbers(self):
        """Test wait time with multiple barbers"""
        # Add second barber
        Barbeiro.objects.create(
            usuario=User.objects.create_user('barber2', 'barber2@example.com', 'password'),
            barbearia=self.barbearia,
            nome="Barber 2",
            status=DomainBarbeiro.Status.STATUS_AVAILABLE.value
        )
        
        # Add multiple clients to queue
        Fila.objects.create(
            barbearia=self.barbearia,
            cliente=self.client1,
            servico=self.service1,  # 30 minutes
            status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value
        )
        
        Fila.objects.create(
            barbearia=self.barbearia,
            cliente=self.client1,
            servico=self.service2,  # 15 minutes
            status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value
        )
        
        Fila.objects.create(
            barbearia=self.barbearia,
            cliente=self.client1,
            servico=self.service1,  # 30 minutes
            status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value
        )
        
        # Total: 75 minutes with 2 barbers = 37.5 minutes (floor division = 37)
        wait_time = self.barbearia.calcular_tempo_espera()
        self.assertEqual(wait_time, 37)
    
    def test_wait_time_ignores_non_waiting_clients(self):
        """Test that wait time only counts waiting clients"""
        # Add a mix of waiting and non-waiting clients
        Fila.objects.create(
            barbearia=self.barbearia,
            cliente=self.client1,
            servico=self.service1,  # 30 minutes
            status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value
        )
        
        Fila.objects.create(
            barbearia=self.barbearia,
            cliente=self.client1,
            servico=self.service2,  # 15 minutes
            status=DomainEntradaFila.Status.STATUS_ATENDIMENTO.value  # In service, should be ignored
        )
        
        Fila.objects.create(
            barbearia=self.barbearia,
            cliente=self.client1,
            servico=self.service1,  # 30 minutes
            status=DomainEntradaFila.Status.STATUS_FINALIZADO.value  # Completed, should be ignored
        )
        
        # Only the first client is waiting, so wait time should be 30 minutes
        wait_time = self.barbearia.calcular_tempo_espera()
        self.assertEqual(wait_time, 30)
    
    @patch('django.core.cache.cache.get')
    @patch('django.core.cache.cache.set')
    def test_wait_time_caching(self, mock_cache_set, mock_cache_get):
        """Test that wait time uses caching"""
        # Configure cache mock to return None (cache miss)
        mock_cache_get.return_value = None
        
        # Add client to queue
        Fila.objects.create(
            barbearia=self.barbearia,
            cliente=self.client1,
            servico=self.service1,  # 30 minutes
            status=DomainEntradaFila.Status.STATUS_AGUARDANDO.value
        )
        
        # First call should calculate and cache
        wait_time = self.barbearia.calcular_tempo_espera()
        self.assertEqual(wait_time, 30)
        
        # Verify cache was checked
        mock_cache_get.assert_called_once()
        
        # Verify result was cached with 30 second timeout
        mock_cache_set.assert_called_once()
        args, kwargs = mock_cache_set.call_args
        self.assertEqual(args[1], 30)  # Second arg is the value cached
        self.assertEqual(kwargs.get('timeout'), 30)  # Timeout should be 30 seconds
        
        # Configure cache mock to return cached value
        mock_cache_get.return_value = 30
        mock_cache_get.reset_mock()
        mock_cache_set.reset_mock()
        
        # Second call should use cached value
        wait_time = self.barbearia.calcular_tempo_espera()
        self.assertEqual(wait_time, 30)
        
        # Verify cache was checked
        mock_cache_get.assert_called_once()
        
        # Verify no new cache set occurred
        mock_cache_set.assert_not_called()
    
    def test_wait_time_invalidation_on_model_change(self):
        """Test wait time cache is invalidated when model changes"""
        with patch('django.core.cache.cache.delete') as mock_cache_delete:
            # Save barbershop model
            self.barbearia.save()
            
            # Verify cache was invalidated
            mock_cache_delete.assert_any_call(self.barbearia._wait_time_cache_key)
    
    def test_tempo_espera_estimado_format(self):
        """Test tempo_espera_estimado property formats correctly"""
        with patch('barbershop.models.Barbearia.calcular_tempo_espera') as mock_calc:
            # Test zero minutes
            mock_calc.return_value = 0
            self.assertEqual(self.barbearia.tempo_espera_estimado, "Sem espera")
            
            # Test minutes only
            mock_calc.return_value = 45
            self.assertEqual(self.barbearia.tempo_espera_estimado, "45 minutos")
            
            # Test hours and minutes
            mock_calc.return_value = 125
            self.assertEqual(self.barbearia.tempo_espera_estimado, "2h e 5min") 