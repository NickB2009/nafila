#!/usr/bin/env python
"""
Script to create a test barbershop
"""
from barbershop.models import Barbearia
import json

# Delete any existing test barbershops
Barbearia.objects.filter(slug='test-barbershop').delete()

# Create a new barbershop
try:
    barbershop = Barbearia(
        nome='Test Barbershop',
        slug='test-barbershop',
        endereco='123 Main St',
        telefone='(123) 456-7890',
        descricao_curta='A modern barbershop for all your grooming needs',
        cores=['#FF5733', '#33FF57'],
        horario_abertura='09:00',
        horario_fechamento='18:00',
        dias_funcionamento=[0, 1, 2, 3, 4, 5],
        max_capacity=10,
        enable_priority_queue=True
    )
    
    barbershop.save(force_insert=True)
    
    print(f"Successfully created barbershop: {barbershop.id}")
    print(f"Name: {barbershop.nome}")
    print(f"Slug: {barbershop.slug}")
    print(f"Operating days: {barbershop.dias_funcionamento}")
    print(f"Hours: {barbershop.horario_abertura} - {barbershop.horario_fechamento}")
    
except Exception as e:
    print(f"Error creating barbershop: {str(e)}")
    import traceback
    traceback.print_exc() 