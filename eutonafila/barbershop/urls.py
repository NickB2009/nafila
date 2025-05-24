from django.urls import path
from django.conf import settings
from rest_framework.urlpatterns import format_suffix_patterns
from .api_views import (
    BarbershopListView, BarbershopDetailView,
    ServiceListView, BarberListView,
    QueueListView, CheckInView, QueueStatusView,
    CancelQueueEntryView, StartServiceView, FinishServiceView
)
from .web_views import (
    HomeView, 
    BarbershopView, 
    CheckInView as WebCheckInView, 
    QueueStatusView as WebQueueStatusView,
    DashboardView,
    GettingStartedView,
    ProcessingView,
    CheckTaskStatusView,
    BarbershopCustomPageView,
    debug_add_queue_clients,
    debug_list_services,
    debug_create_entry,
    debug_add_service,
    OwnerDashboardView
)
from .website_builder_views import (
    WebsiteBuilderDashboardView,
    SectionEditorView,
    AddSectionView,
    RemoveSectionView,
    GalleryManagementView,
    ReorderSectionsView,
    UpdateSEOView,
    UpdateColorsView
)

app_name = 'barbershop'

# API URLs
api_urlpatterns = [
    # Barbershop endpoints
    path('api/barbershops/', BarbershopListView.as_view(), name='barbershop-list'),
    path('api/barbershops/<str:slug>/', BarbershopDetailView.as_view(), name='barbershop-detail'),
    
    # Service endpoints
    path('api/barbershops/<str:slug>/services/', ServiceListView.as_view(), name='service-list'),
    
    # Barber endpoints
    path('api/barbershops/<str:slug>/barbers/', BarberListView.as_view(), name='barber-list'),
    
    # Queue endpoints
    path('api/barbershops/<str:slug>/queue/', QueueListView.as_view(), name='queue-list'),
    
    # Client check-in
    path('api/checkin/', CheckInView.as_view(), name='client-checkin'),
    
    # Queue status and operations
    path('api/queue-status/<uuid:queue_id>/', QueueStatusView.as_view(), name='api-queue-status'),
    path('api/cancel-queue/<uuid:queue_id>/', CancelQueueEntryView.as_view(), name='api-cancel-queue'),
    
    # Add proper views for start and finish service
    path('api/queue-status/<uuid:queue_id>/start/', StartServiceView.as_view(), name='api-start-service'),
    path('api/queue-status/<uuid:queue_id>/finish/', FinishServiceView.as_view(), name='api-finish-service'),
]

# Web URLs
urlpatterns = [
    path('', HomeView.as_view(), name='home'),
    path('getting-started/', GettingStartedView.as_view(), name='getting-started'),
    path('b/<str:slug>/', BarbershopView.as_view(), name='barbershop_detail'),
    path('b/<str:slug>/checkin/', WebCheckInView.as_view(), name='checkin'),
    path('b/<str:slug>/dashboard/', DashboardView.as_view(), name='dashboard'),
    path('b/<str:slug>/owner-dashboard/', OwnerDashboardView.as_view(), name='owner_dashboard'),
    path('b/<str:slug>/website-builder/', WebsiteBuilderDashboardView.as_view(), name='website_builder'),
    path('b/<str:slug>/website-builder/section/<int:section_id>/', SectionEditorView.as_view(), name='section_editor'),
    path('b/<str:slug>/website-builder/add-section/', AddSectionView.as_view(), name='add_section'),
    path('b/<str:slug>/website-builder/remove-section/<int:section_id>/', RemoveSectionView.as_view(), name='remove_section'),
    path('b/<str:slug>/website-builder/gallery/', GalleryManagementView.as_view(), name='gallery_management'),
    path('b/<str:slug>/website-builder/reorder-sections/', ReorderSectionsView.as_view(), name='reorder_sections'),
    path('b/<str:slug>/website-builder/seo/', UpdateSEOView.as_view(), name='update_seo'),
    path('b/<str:slug>/website-builder/colors/', UpdateColorsView.as_view(), name='update_colors'),
    path('processing/<str:task_id>/', ProcessingView.as_view(), name='processing'),
    path('check-task-status/<str:task_id>/', CheckTaskStatusView.as_view(), name='check_task_status'),
]

# Debug URLs (only in development)
if settings.DEBUG:
    urlpatterns += [
        path('debug/add-queue-clients/', debug_add_queue_clients, name='debug_add_queue_clients'),
        path('debug/list-services/', debug_list_services, name='debug_list_services'),
        path('debug/create-entry/', debug_create_entry, name='debug_create_entry'),
        path('debug/add-service/', debug_add_service, name='debug_add_service'),
    ]

# Combine URL patterns
urlpatterns = api_urlpatterns + urlpatterns

# Add format suffix patterns only to API URLs
api_urlpatterns = format_suffix_patterns(api_urlpatterns) 