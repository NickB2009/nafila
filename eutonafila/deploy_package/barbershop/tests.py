from django.test import TestCase
from django.urls import reverse
from rest_framework.test import APITestCase
from rest_framework import status
from domain.models import Barbearia, Servico, Cliente, EntradaFila
import uuid
from django.utils import timezone

class BarbershopAPITests(APITestCase):
    def setUp(self):
        # Create test barbershop
        self.barbearia = Barbearia.objects.create(
            nome="Barbearia Teste",
            slug="barbearia-teste",
            telefone="11999999999",
            endereco="Rua de Teste, 123",
            dias_funcionamento=[0, 1, 2, 3, 4, 5]  # Monday to Saturday
        )
        
        # Create test service
        self.servico = Servico.objects.create(
            barbearia=self.barbearia,
            nome="Corte Simples",
            descricao="Corte de cabelo simples",
            preco=35.00,
            duracao=30
        )
        
    def test_list_barbershops(self):
        """Test retrieving barbershop list"""
        url = reverse('barbershop:api-barbershop-list')
        response = self.client.get(url)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 1)
        self.assertEqual(response.data[0]['nome'], 'Barbearia Teste')
    
    def test_barbershop_detail(self):
        """Test retrieving barbershop details"""
        url = reverse('barbershop:api-barbershop-detail', kwargs={'slug': self.barbearia.slug})
        response = self.client.get(url)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['nome'], 'Barbearia Teste')
        self.assertEqual(len(response.data['servicos']), 1)
    
    def test_checkin(self):
        """Test client check-in to queue"""
        url = reverse('barbershop:api-checkin')
        data = {
            'nome': 'Cliente Teste',
            'telefone': '11999999999',
            'email': 'cliente@teste.com',
            'barbearia_slug': self.barbearia.slug,
            'servico_id': str(self.servico.id)  # Make sure we're using the string representation
        }
        
        response = self.client.post(url, data, format='json')
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        self.assertEqual(response.data['cliente'], 'Cliente Teste')
        self.assertEqual(response.data['barbearia'], 'Barbearia Teste')
        
        # Test queue status using the ID returned in the response
        queue_id = response.data['id']
        url = reverse('barbershop:api-queue-status', kwargs={'queue_id': queue_id})
        response = self.client.get(url)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
