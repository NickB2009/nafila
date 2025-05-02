#!/usr/bin/env python
"""
Django management command to create a test barbershop
"""
from django.core.management.base import BaseCommand
from barbershop.models import Barbearia

class Command(BaseCommand):
    help = 'Create a test barbershop'

    def handle(self, *args, **options):
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
            
            self.stdout.write(self.style.SUCCESS(f"Successfully created barbershop: {barbershop.id}"))
            self.stdout.write(f"Name: {barbershop.nome}")
            self.stdout.write(f"Slug: {barbershop.slug}")
            self.stdout.write(f"Operating days: {barbershop.dias_funcionamento}")
            self.stdout.write(f"Hours: {barbershop.horario_abertura} - {barbershop.horario_fechamento}")
            
        except Exception as e:
            self.stdout.write(self.style.ERROR(f"Error creating barbershop: {str(e)}"))
            import traceback
            traceback.print_exc() 