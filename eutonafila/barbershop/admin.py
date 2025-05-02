from django.contrib import admin
from django.utils import timezone
from .models import Barbearia, Barbeiro, Servico, Cliente, Fila
from domain.domain_models import EntradaFila as DomainEntradaFila
from domain.models import ActivePageSection, PageLayout, PageSection, BarbeariaCustomPage, GalleryImage, Testimonial

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
    list_display = ('nome', 'slug', 'telefone', 'created_at')
    search_fields = ('nome', 'slug')
    prepopulated_fields = {'slug': ('nome',)}

@admin.register(Barbeiro)
class BarbeiroAdmin(admin.ModelAdmin):
    list_display = ('nome', 'barbearia', 'status')
    list_filter = ('barbearia', 'status')
    search_fields = ('nome',)

@admin.register(Servico)
class ServicoAdmin(admin.ModelAdmin):
    list_display = ('nome', 'barbearia', 'preco', 'duracao')
    list_filter = ('barbearia',)
    search_fields = ('nome',)
    
    # Exclude fields that are missing from the database
    exclude = ('complexity', 'popularity')
    
    def get_queryset(self, request):
        """Override to avoid accessing missing columns"""
        qs = super().get_queryset(request)
        # Use defer to avoid loading non-existent columns
        return qs.defer('complexity', 'popularity')

@admin.register(Cliente)
class ClienteAdmin(admin.ModelAdmin):
    list_display = ('nome', 'telefone', 'email', 'created_at')
    search_fields = ('nome', 'telefone', 'email')
    list_filter = ('created_at',)

@admin.register(Fila)
class FilaAdmin(admin.ModelAdmin):
    list_display = ['cliente', 'servico', 'status', 'created_at', 'get_position']
    list_filter = ['status', 'barbearia']
    search_fields = ['cliente__nome', 'servico__nome']
    readonly_fields = ['get_position', 'get_estimated_wait_time']
    
    def get_position(self, obj):
        if obj.status == DomainEntradaFila.Status.STATUS_AGUARDANDO.value:
            return obj.get_position()
        return '-'
    get_position.short_description = 'Position'
    
    def get_estimated_wait_time(self, obj):
        if obj.status == DomainEntradaFila.Status.STATUS_AGUARDANDO.value:
            return obj.get_estimated_wait_time()
        return '-'
    get_estimated_wait_time.short_description = 'Estimated Wait Time'
