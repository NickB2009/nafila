"""
Domain models and business logic for the barbershop management system.
This module contains pure domain logic without any framework dependencies.
"""

from django.db import models
from django.utils import timezone
from .entities import (
    Cliente, ClienteStatus,
    Barbeiro, BarbeiroStatus,
    Servico, ServicoComplexidade,
    EntradaFila, FilaStatus, FilaPrioridade
)

from .services import (
    WaitTimeCalculator,
    OpeningHoursValidator,
    QueueManager
)

class Barbearia(models.Model):
    """Django model for Barbearia"""
    nome = models.CharField(max_length=100)
    slug = models.SlugField(unique=True)
    telefone = models.CharField(max_length=20, blank=True, null=True)
    endereco = models.TextField(blank=True, null=True)
    horario_abertura = models.TimeField()
    horario_fechamento = models.TimeField()
    dias_funcionamento = models.JSONField(default=list)  # List of integers (0=Monday, 6=Sunday)
    max_capacity = models.IntegerField(default=10)
    enable_priority_queue = models.BooleanField(default=False)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)

    def __str__(self):
        return self.nome

    class Meta:
        verbose_name = "Barbearia"
        verbose_name_plural = "Barbearias"

class PageLayout(models.Model):
    """Model for website page layouts"""
    name = models.CharField(max_length=100)
    description = models.TextField(blank=True, null=True)
    thumbnail = models.ImageField(upload_to='layouts/', blank=True, null=True)
    is_active = models.BooleanField(default=True)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)

    def __str__(self):
        return self.name

class PageSection(models.Model):
    """Model for available page sections"""
    name = models.CharField(max_length=100)
    section_type = models.CharField(max_length=50)
    description = models.TextField(blank=True, null=True)
    template_path = models.CharField(max_length=200)
    is_required = models.BooleanField(default=False)
    order_index = models.IntegerField(default=0)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)

    def __str__(self):
        return self.name

    class Meta:
        ordering = ['order_index']

class BarbeariaCustomPage(models.Model):
    """Model for custom barbershop pages"""
    barbearia = models.OneToOneField(Barbearia, on_delete=models.CASCADE, related_name='custom_page')
    layout = models.ForeignKey(PageLayout, null=True, on_delete=models.SET_NULL, related_name='barbershop_pages')
    meta_title = models.CharField(max_length=100, blank=True, null=True)
    meta_description = models.TextField(blank=True, null=True)
    custom_css = models.TextField(blank=True, null=True)
    updated_at = models.DateTimeField(auto_now=True)
    active_sections = models.ManyToManyField(PageSection, through='ActivePageSection')

    def __str__(self):
        return f"Custom Page - {self.barbearia.nome}"

class ActivePageSection(models.Model):
    """Model for active sections on a custom page"""
    page = models.ForeignKey(BarbeariaCustomPage, on_delete=models.CASCADE)
    section = models.ForeignKey(PageSection, on_delete=models.CASCADE)
    order = models.IntegerField(default=0)
    is_enabled = models.BooleanField(default=True)
    configuration = models.JSONField(default=dict)

    def __str__(self):
        return f"{self.section.name} - {self.page.barbearia.nome}"

    class Meta:
        ordering = ['order']

class GalleryImage(models.Model):
    """Model for barbershop gallery images"""
    barbearia = models.ForeignKey(Barbearia, on_delete=models.CASCADE, related_name='gallery_images')
    image = models.ImageField(upload_to='gallery/')
    caption = models.CharField(max_length=200, blank=True, null=True)
    order = models.IntegerField(default=0)
    created_at = models.DateTimeField(auto_now_add=True)

    def __str__(self):
        return f"Gallery Image - {self.barbearia.nome}"

    class Meta:
        ordering = ['order']

class Testimonial(models.Model):
    """Model for customer testimonials"""
    barbearia = models.ForeignKey(Barbearia, on_delete=models.CASCADE, related_name='testimonials')
    customer_name = models.CharField(max_length=100)
    rating = models.IntegerField(choices=[(i, i) for i in range(1, 6)])
    content = models.TextField()
    is_active = models.BooleanField(default=True)
    created_at = models.DateTimeField(auto_now_add=True)

    def __str__(self):
        return f"Testimonial - {self.customer_name}"

    class Meta:
        ordering = ['-created_at']

__all__ = [
    'Cliente', 'ClienteStatus',
    'Barbeiro', 'BarbeiroStatus',
    'Servico', 'ServicoComplexidade',
    'EntradaFila', 'FilaStatus', 'FilaPrioridade',
    'Barbearia',
    'WaitTimeCalculator',
    'OpeningHoursValidator',
    'QueueManager',
    'PageLayout',
    'PageSection',
    'BarbeariaCustomPage',
    'ActivePageSection',
    'GalleryImage',
    'Testimonial'
] 