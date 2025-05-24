from django.contrib import admin
from django.utils import timezone
from .models import Barbearia, Barbeiro, Servico, Cliente, Fila
from domain.domain_models import EntradaFila as DomainEntradaFila
from domain.models import ActivePageSection, PageLayout, PageSection, BarbeariaCustomPage, GalleryImage, Testimonial
from django import forms
from django.shortcuts import render

# Inline classes for related models
class ServicoInline(admin.TabularInline):
    model = Servico
    extra = 1  # Number of empty forms to display
    fields = ('nome', 'preco', 'duracao', 'descricao')
    # Ensure we don't use columns that might be missing
    exclude = ('complexity', 'popularity')

# Website Builder Admin Classes
class ActivePageSectionInline(admin.TabularInline):
    model = ActivePageSection
    extra = 0
    fields = ('section', 'order', 'is_enabled')

@admin.register(PageLayout)
class PageLayoutAdmin(admin.ModelAdmin):
    list_display = ('name', 'is_active', 'created_at')
    list_filter = ('is_active',)
    search_fields = ('name', 'description')

@admin.register(PageSection)
class PageSectionAdmin(admin.ModelAdmin):
    list_display = ('name', 'section_type', 'is_required', 'order_index')
    list_filter = ('section_type', 'is_required')
    search_fields = ('name', 'description')

@admin.register(BarbeariaCustomPage)
class BarbeariaCustomPageAdmin(admin.ModelAdmin):
    list_display = ('barbearia', 'layout', 'updated_at')
    list_filter = ('layout',)
    search_fields = ('barbearia__nome',)
    inlines = [ActivePageSectionInline]
    fieldsets = (
        ('Informações Básicas', {
            'fields': ('barbearia', 'layout')
        }),
        ('SEO', {
            'fields': ('meta_title', 'meta_description')
        }),
        ('Personalização', {
            'fields': ('custom_css',)
        }),
    )

class GalleryImageInline(admin.TabularInline):
    model = GalleryImage
    extra = 0

class TestimonialInline(admin.TabularInline):
    model = Testimonial
    extra = 0

@admin.register(Barbearia)
class BarbeariaAdmin(admin.ModelAdmin):
    list_display = ('nome', 'display_uuid', 'slug', 'endereco', 'telefone', 'esta_aberto', 'created_at')
    list_filter = ('created_at', 'enable_priority_queue')
    search_fields = ('nome', 'slug', 'id', 'endereco', 'telefone')
    readonly_fields = ('id', 'created_at', 'updated_at')
    
    def display_uuid(self, obj):
        """Display UUID in a more visible format"""
        return f"🔑 {obj.id}"
    display_uuid.short_description = 'UUID'
    display_uuid.admin_order_field = 'id'
    
    fieldsets = (
        ('Informações Básicas', {
            'fields': ('id', 'nome', 'slug', 'telefone', 'endereco', 'descricao_curta'),
            'description': '🔑 Use o UUID acima para acessar a barbearia via URL: /b/uuid/{uuid}/'
        }),
        ('Configurações', {
            'fields': ('cores', 'logo', 'horario_abertura', 'horario_fechamento', 'dias_funcionamento', 'max_capacity', 'enable_priority_queue')
        }),
        ('Informações do Sistema', {
            'fields': ('created_at', 'updated_at', 'user'),
            'classes': ('collapse',)
        }),
    )

@admin.register(Barbeiro)
class BarbeiroAdmin(admin.ModelAdmin):
    list_display = ('nome', 'barbearia', 'status', 'get_current_client')
    list_filter = ('status', 'barbearia')
    search_fields = ('nome',)
    filter_horizontal = ('especialidades',)
    actions = ['mark_available', 'mark_on_break']
    
    def get_current_client(self, obj):
        current_service = Fila.objects.filter(
            barbeiro=obj,
            status='in_progress'
        ).first()
        return current_service.cliente.nome if current_service else '-'
    get_current_client.short_description = 'Current Client'
    
    def mark_available(self, request, queryset):
        queryset.update(status='available')
    mark_available.short_description = "Mark selected barbers as available"
    
    def mark_on_break(self, request, queryset):
        queryset.update(status='on_break')
    mark_on_break.short_description = "Mark selected barbers as on break"

@admin.register(Servico)
class ServicoAdmin(admin.ModelAdmin):
    list_display = ('nome', 'barbearia', 'preco', 'duracao', 'complexity', 'popularity', 'get_median_wait_time')
    list_filter = ('barbearia', 'complexity')
    search_fields = ('nome',)
    list_editable = ('preco', 'duracao', 'complexity')
    
    def get_median_wait_time(self, obj):
        return f"{obj.get_median_wait_time():.1f} min"
    get_median_wait_time.short_description = 'Median Wait Time'

@admin.register(Cliente)
class ClienteAdmin(admin.ModelAdmin):
    list_display = ('nome', 'telefone', 'email', 'created_at')
    search_fields = ('nome', 'telefone', 'email')
    list_filter = ('created_at',)

@admin.register(Fila)
class FilaAdmin(admin.ModelAdmin):
    list_display = ('barbearia', 'cliente', 'servico', 'barbeiro', 'status', 'get_position', 'get_wait_time', 'created_at')
    list_filter = ('status', 'barbearia', 'barbeiro')
    search_fields = ('cliente__nome', 'servico__nome')
    date_hierarchy = 'created_at'
    actions = ['assign_barber', 'complete_service']
    
    def get_position(self, obj):
        if obj.status == 'waiting':
            return obj.get_position()
        return '-'
    get_position.short_description = 'Position'
    
    def get_wait_time(self, obj):
        if obj.status == 'waiting':
            return f"{obj.get_wait_time():.1f} min"
        return '-'
    get_wait_time.short_description = 'Current Wait Time'
    
    def assign_barber(self, request, queryset):
        if queryset.count() != 1:
            self.message_user(request, "Please select exactly one waiting client to assign a barber.")
            return
            
        queue_entry = queryset.first()
        if queue_entry.status != 'waiting':
            self.message_user(request, "Can only assign barber to waiting clients.")
            return
            
        if queue_entry.get_position() != 0:
            self.message_user(request, "Can only assign barber to first position.")
            return
            
        # Get available barbers
        available_barbers = Barbeiro.objects.filter(
            barbearia=queue_entry.barbearia,
            status='available'
        )
        
        if not available_barbers:
            self.message_user(request, "No available barbers found.")
            return
            
        # Show barber selection form
        class BarberForm(forms.Form):
            barber = forms.ModelChoiceField(
                queryset=available_barbers,
                label="Select Barber"
            )
        
        if 'apply' in request.POST:
            form = BarberForm(request.POST)
            if form.is_valid():
                barber = form.cleaned_data['barber']
                queue_entry.assign_barber(barber)
                self.message_user(request, f"Assigned {barber.nome} to {queue_entry.cliente.nome}")
                return
        else:
            form = BarberForm()
        
        return render(
            request,
            'admin/assign_barber.html',
            context={
                'form': form,
                'queue_entry': queue_entry,
                'title': 'Assign Barber'
            }
        )
    assign_barber.short_description = "Assign barber to selected client"
    
    def complete_service(self, request, queryset):
        for queue_entry in queryset:
            if queue_entry.status == 'in_progress':
                queue_entry.complete_service()
                self.message_user(request, f"Completed service for {queue_entry.cliente.nome}")
    complete_service.short_description = "Mark selected services as completed"
