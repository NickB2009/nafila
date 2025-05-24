from django.shortcuts import render, redirect, get_object_or_404
from django.views import View
from django.contrib import messages
from django.urls import reverse
from django.http import JsonResponse
from django.contrib.auth.mixins import LoginRequiredMixin
from django.views.decorators.csrf import csrf_exempt
from django.utils.decorators import method_decorator
import json

from domain.models import (
    Barbearia, BarbeariaCustomPage, PageLayout, 
    PageSection, ActivePageSection, GalleryImage, Testimonial
)
from application.services import WebsiteBuilderService
from infrastructure.repositories import (
    DjangoBarbeariaRepository,
    DjangoPageSectionRepository,
    DjangoPageLayoutRepository,
    DjangoCustomPageRepository
)


class WebsiteBuilderDashboardView(LoginRequiredMixin, View):
    """Website builder dashboard view for barbershop owners"""
    
    def get(self, request, barbershop_slug):
        barbearia = get_object_or_404(Barbearia, slug=barbershop_slug)
        
        # Check if user has permission to edit this barbershop
        if not request.user.has_perm('barbershop.change_barbearia') and request.user != barbearia.user:
            messages.error(request, "Você não tem permissão para editar esta barbearia.")
            return redirect('barbershop:barbershop_detail', slug=barbershop_slug)
        
        # Initialize service
        service = WebsiteBuilderService(
            barbearia_repo=DjangoBarbeariaRepository(),
            section_repo=DjangoPageSectionRepository(),
            layout_repo=DjangoPageLayoutRepository()
        )
        
        # Get or create custom page
        try:
            custom_page = BarbeariaCustomPage.objects.get(barbearia=barbearia)
        except BarbeariaCustomPage.DoesNotExist:
            custom_page = service.create_default_page(barbearia.id)
        
        # Get active sections
        active_sections = ActivePageSection.objects.filter(
            page=custom_page,
            is_enabled=True
        ).order_by('order')
        
        # Get available layouts
        layouts = PageLayout.objects.filter(is_active=True)
        
        # Get available sections (not yet added)
        available_sections = PageSection.objects.exclude(
            id__in=custom_page.active_sections.all().values_list('id', flat=True)
        )
        
        # Get gallery images
        gallery_images = GalleryImage.objects.filter(barbearia=barbearia).order_by('order')
        
        # Get testimonials
        testimonials = Testimonial.objects.filter(barbearia=barbearia, is_active=True)
        
        return render(request, 'barbershop/website_builder/dashboard.html', {
            'barbershop': barbearia,
            'custom_page': custom_page,
            'active_sections': active_sections,
            'available_sections': available_sections,
            'layouts': layouts,
            'gallery_images': gallery_images,
            'testimonials': testimonials
        })


class SectionEditorView(LoginRequiredMixin, View):
    """View for editing individual sections"""
    
    def get(self, request, barbershop_slug, section_id):
        barbearia = get_object_or_404(Barbearia, slug=barbershop_slug)
        
        # Check permission
        if not request.user.has_perm('barbershop.change_barbearia') and request.user != barbearia.user:
            messages.error(request, "Você não tem permissão para editar esta barbearia.")
            return redirect('barbershop:barbershop_detail', slug=barbershop_slug)
        
        # Get custom page
        custom_page = get_object_or_404(BarbeariaCustomPage, barbearia=barbearia)
        
        # Get active section
        active_section = get_object_or_404(
            ActivePageSection,
            page=custom_page,
            section_id=section_id
        )
        
        section_type = active_section.section.section_type
        
        # Get appropriate form context based on section type
        context = {
            'barbershop': barbearia,
            'custom_page': custom_page,
            'active_section': active_section,
            'section': active_section.section,
            'config': active_section.configuration
        }
        
        # Add additional context based on section type
        if section_type == 'gallery':
            context['gallery_images'] = GalleryImage.objects.filter(
                barbearia=barbearia
            ).order_by('order')
        elif section_type == 'team':
            context['barbeiros'] = barbearia.barbeiros.all()
        elif section_type == 'services':
            context['servicos'] = barbearia.servicos.all()
        elif section_type == 'testimonials':
            context['testimonials'] = Testimonial.objects.filter(
                barbearia=barbearia,
                is_active=True
            )
        
        return render(request, f'barbershop/website_builder/sections/{section_type}_editor.html', context)
    
    @method_decorator(csrf_exempt)
    def post(self, request, barbershop_slug, section_id):
        barbearia = get_object_or_404(Barbearia, slug=barbershop_slug)
        
        # Check permission
        if not request.user.has_perm('barbershop.change_barbearia') and request.user != barbearia.user:
            return JsonResponse({'success': False, 'error': 'Permissão negada'}, status=403)
        
        # Get custom page and active section
        custom_page = get_object_or_404(BarbeariaCustomPage, barbearia=barbearia)
        active_section = get_object_or_404(
            ActivePageSection, 
            page=custom_page,
            section_id=section_id
        )
        
        try:
            # Get configuration from request body
            data = json.loads(request.body)
            config = data.get('config', {})
            
            # Update configuration
            active_section.configuration.update(config)
            active_section.save()
            
            return JsonResponse({'success': True})
        except Exception as e:
            return JsonResponse({'success': False, 'error': str(e)}, status=400)


class AddSectionView(LoginRequiredMixin, View):
    """View for adding a new section to the page"""
    
    def post(self, request, barbershop_slug):
        barbearia = get_object_or_404(Barbearia, slug=barbershop_slug)
        
        # Check permission
        if not request.user.has_perm('barbershop.change_barbearia') and request.user != barbearia.user:
            messages.error(request, "Você não tem permissão para editar esta barbearia.")
            return redirect('barbershop:barbershop_detail', slug=barbershop_slug)
        
        # Get custom page
        custom_page = get_object_or_404(BarbeariaCustomPage, barbearia=barbearia)
        
        # Get section to add
        section_id = request.POST.get('section_id')
        if not section_id:
            messages.error(request, "Seção não especificada.")
            return redirect('barbershop:website_builder', barbershop_slug=barbershop_slug)
        
        section = get_object_or_404(PageSection, id=section_id)
        
        # Check if section already exists
        if ActivePageSection.objects.filter(page=custom_page, section=section).exists():
            messages.warning(request, "Esta seção já existe na página.")
            return redirect('barbershop:website_builder', barbershop_slug=barbershop_slug)
        
        # Get last order
        last_order = ActivePageSection.objects.filter(page=custom_page).order_by('-order').first()
        order = (last_order.order + 1) if last_order else 0
        
        # Create new active section
        service = WebsiteBuilderService()
        ActivePageSection.objects.create(
            page=custom_page,
            section=section,
            order=order,
            is_enabled=True,
            configuration=service._get_default_config(section.section_type, barbearia)
        )
        
        messages.success(request, f"Seção '{section.name}' adicionada com sucesso.")
        return redirect('barbershop:website_builder', barbershop_slug=barbershop_slug)


class RemoveSectionView(LoginRequiredMixin, View):
    """View for removing a section from the page"""
    
    def post(self, request, barbershop_slug, section_id):
        barbearia = get_object_or_404(Barbearia, slug=barbershop_slug)
        
        # Check permission
        if not request.user.has_perm('barbershop.change_barbearia') and request.user != barbearia.user:
            messages.error(request, "Você não tem permissão para editar esta barbearia.")
            return redirect('barbershop:barbershop_detail', slug=barbershop_slug)
        
        # Get custom page and active section
        custom_page = get_object_or_404(BarbeariaCustomPage, barbearia=barbearia)
        active_section = get_object_or_404(
            ActivePageSection, 
            page=custom_page,
            section_id=section_id
        )
        
        # Check if section is required
        if active_section.section.is_required:
            messages.error(request, "Esta seção é obrigatória e não pode ser removida.")
            return redirect('barbershop:website_builder', barbershop_slug=barbershop_slug)
        
        # Remove section
        section_name = active_section.section.name
        active_section.delete()
        
        messages.success(request, f"Seção '{section_name}' removida com sucesso.")
        return redirect('barbershop:website_builder', barbershop_slug=barbershop_slug)


class GalleryManagementView(LoginRequiredMixin, View):
    """View for managing gallery images"""
    
    def get(self, request, barbershop_slug):
        barbearia = get_object_or_404(Barbearia, slug=barbershop_slug)
        
        # Check permission
        if not request.user.has_perm('barbershop.change_barbearia') and request.user != barbearia.user:
            messages.error(request, "Você não tem permissão para editar esta barbearia.")
            return redirect('barbershop:barbershop_detail', slug=barbershop_slug)
        
        # Get gallery images
        gallery_images = GalleryImage.objects.filter(barbearia=barbearia).order_by('order')
        
        return render(request, 'barbershop/website_builder/gallery_management.html', {
            'barbershop': barbearia,
            'gallery_images': gallery_images
        })
    
    def post(self, request, barbershop_slug):
        barbearia = get_object_or_404(Barbearia, slug=barbershop_slug)
        
        # Check permission
        if not request.user.has_perm('barbershop.change_barbearia') and request.user != barbearia.user:
            messages.error(request, "Você não tem permissão para editar esta barbearia.")
            return redirect('barbershop:barbershop_detail', slug=barbershop_slug)
        
        # Get form data
        title = request.POST.get('title', '')
        description = request.POST.get('description', '')
        image = request.FILES.get('image')
        
        if not image:
            messages.error(request, "Nenhuma imagem selecionada.")
            return redirect('barbershop:gallery_management', barbershop_slug=barbershop_slug)
        
        # Get last order
        last_image = GalleryImage.objects.filter(barbearia=barbearia).order_by('-order').first()
        order = (last_image.order + 1) if last_image else 0
        
        # Create new gallery image
        GalleryImage.objects.create(
            barbearia=barbearia,
            image=image,
            title=title,
            description=description,
            order=order
        )
        
        messages.success(request, "Imagem adicionada à galeria com sucesso.")
        return redirect('barbershop:gallery_management', barbershop_slug=barbershop_slug)


@method_decorator(csrf_exempt, name='dispatch')
class ReorderSectionsView(LoginRequiredMixin, View):
    """AJAX view for reordering sections"""
    
    def post(self, request, barbershop_slug):
        barbearia = get_object_or_404(Barbearia, slug=barbershop_slug)
        
        # Check permission
        if not request.user.has_perm('barbershop.change_barbearia') and request.user != barbearia.user:
            return JsonResponse({'success': False, 'error': 'Permissão negada'}, status=403)
        
        try:
            # Get order data from request
            data = json.loads(request.body)
            order_data = data.get('orders', {})
            
            # Get custom page
            custom_page = get_object_or_404(BarbeariaCustomPage, barbearia=barbearia)
            
            # Initialize service
            service = WebsiteBuilderService()
            
            # Update section orders
            if service.reorder_sections(custom_page.id, order_data):
                return JsonResponse({'success': True})
            else:
                return JsonResponse({'success': False, 'error': 'Falha ao reordenar seções'}, status=400)
            
        except Exception as e:
            return JsonResponse({'success': False, 'error': str(e)}, status=400)


class UpdateSEOView(LoginRequiredMixin, View):
    """View for updating SEO settings"""
    
    def post(self, request, barbershop_slug):
        barbearia = get_object_or_404(Barbearia, slug=barbershop_slug)
        
        # Check permission
        if not request.user.has_perm('barbershop.change_barbearia') and request.user != barbearia.user:
            messages.error(request, "Você não tem permissão para editar esta barbearia.")
            return redirect('barbershop:website_builder', barbershop_slug=barbershop_slug)
        
        # Get custom page
        custom_page = get_object_or_404(BarbeariaCustomPage, barbearia=barbearia)
        
        # Update SEO settings
        meta_title = request.POST.get('meta_title', '')
        meta_description = request.POST.get('meta_description', '')
        
        custom_page.meta_title = meta_title
        custom_page.meta_description = meta_description
        custom_page.save()
        
        messages.success(request, "Configurações de SEO atualizadas com sucesso.")
        return redirect('barbershop:website_builder', barbershop_slug=barbershop_slug)


class UpdateColorsView(LoginRequiredMixin, View):
    """View for updating site colors"""
    
    def post(self, request, barbershop_slug):
        barbearia = get_object_or_404(Barbearia, slug=barbershop_slug)
        
        # Check permission
        if not request.user.has_perm('barbershop.change_barbearia') and request.user != barbearia.user:
            messages.error(request, "Você não tem permissão para editar esta barbearia.")
            return redirect('barbershop:website_builder', barbershop_slug=barbershop_slug)
        
        # Update color settings
        primary_color = request.POST.get('primary_color', '#3498db')
        secondary_color = request.POST.get('secondary_color', '#2c3e50')
        accent_color = request.POST.get('accent_color', '#f39c12')
        
        # Update the colors JSON field
        barbearia.cores = [primary_color, secondary_color, accent_color]
        barbearia.save()
        
        messages.success(request, "Cores do site atualizadas com sucesso.")
        return redirect('barbershop:website_builder', barbershop_slug=barbershop_slug) 