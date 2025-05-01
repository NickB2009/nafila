from django.utils.deprecation import MiddlewareMixin
from django.http import Http404


class TenantMiddleware:
    """
    Middleware to extract tenant slug from URL path.
    Sets the tenant object in the request.
    """
    def __init__(self, get_response):
        self.get_response = get_response

    def __call__(self, request):
        # Extract tenant slug from URL path
        path_parts = request.path.split('/')
        request.tenant = None
        
        if len(path_parts) > 1 and path_parts[1]:
            tenant_slug = path_parts[1]
            # Skip tenant extraction for admin and static/media paths
            if tenant_slug not in ['admin', 'static', 'media', 'api']:
                try:
                    # Import here to avoid circular imports
                    from barbershop.models import Barbearia
                    request.tenant = Barbearia.objects.get(slug=tenant_slug)
                except Barbearia.DoesNotExist:
                    # For now, just continue (we'll handle this better in production)
                    pass
                
        return self.get_response(request) 