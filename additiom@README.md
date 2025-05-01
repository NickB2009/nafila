# 🗺️ Migration Guide: Node.js → Django

## Overview

This document serves as a migration guide for transitioning the EuTôNaFila project from Node.js/Express to Django. It contains key learnings, architectural considerations, and patterns to follow or avoid.

## 🏗️ Architecture Migration

### Clean Architecture in Django

| Node.js Structure | Django Equivalent | Notes |
|-------------------|-------------------|-------|
| `domain/` | `models.py` | Domain entities become Django models |
| `application/` | `services.py` | Use cases become service classes |
| `infrastructure/` | Django settings, managers | Database and infrastructure config handled by Django |
| `interfaces/` | `views.py`, `urls.py`, `templates/` | Controllers and routes become views and URL patterns |

**Key Learning:** While Django has its own MTV (Model-Template-View) architecture, we can still apply Clean Architecture principles by:
- Keeping business logic in models or dedicated service classes
- Using managers for database operations
- Creating service layers for use cases
- Using views only for request handling

## 🗄️ Database Migration

**From:** MySQL with custom queries
**To:** Django ORM with MySQL backend

```python
# Example domain entity migration
# From: Custom entity in domain/cliente.js
# To: Django model

from django.db import models

class Cliente(models.Model):
    nome = models.CharField(max_length=100)
    telefone = models.CharField(max_length=15)
    email = models.EmailField(blank=True, null=True)
    created_at = models.DateTimeField(auto_now_add=True)
    
    def __str__(self):
        return self.nome
    
    # Domain logic methods can remain here
    def calcular_tempo_espera(self, barbearia):
        # Implementation
        pass
```

**Key Learning:** Django ORM is powerful but can lead to N+1 query problems. Use `select_related()` and `prefetch_related()` to optimize database access.

## 🌐 Multi-Tenancy Implementation

**Approach:** Use Django's sites framework or a custom middleware approach

```python
# Option 1: URL-based multi-tenancy with middleware
class TenantMiddleware:
    def __init__(self, get_response):
        self.get_response = get_response

    def __call__(self, request):
        # Extract tenant slug from URL path
        path_parts = request.path.split('/')
        if len(path_parts) > 1 and path_parts[1]:
            tenant_slug = path_parts[1]
            try:
                # Set tenant in request object
                request.tenant = Barbearia.objects.get(slug=tenant_slug)
            except Barbearia.DoesNotExist:
                pass
                
        return self.get_response(request)
```

**Things to Avoid:**
- Don't use separate databases per tenant (excessive complexity)
- Don't hardcode tenant information in views/templates

## 🧠 Caching Strategy

**From:** Local in-memory cache with `node-cache`
**To:** Django's cache framework with local memory cache

```python
# settings.py
CACHES = {
    'default': {
        'BACKEND': 'django.core.cache.backends.locmem.LocMemCache',
        'LOCATION': 'eutonafila',
    }
}

# Usage in views or services
from django.core.cache import cache

def get_tempo_espera(barbearia_id):
    cache_key = f'tempo:{barbearia_id}'
    cached_value = cache.get(cache_key)
    
    if cached_value is not None:
        return cached_value
        
    # Calculate from database
    tempo = calcular_media_do_banco(barbearia_id)
    cache.set(cache_key, tempo, timeout=30)  # 30 seconds
    return tempo
```

**Key Learning:** Django's cache framework supports multiple backends while maintaining a consistent API, making it easy to switch to Redis later if needed.

## 🔐 Authentication System

**From:** Custom JWT implementation
**To:** Django's built-in auth + optional DRF for API auth

```python
# settings.py for API JWT (using djangorestframework-simplejwt)
REST_FRAMEWORK = {
    'DEFAULT_AUTHENTICATION_CLASSES': (
        'rest_framework_simplejwt.authentication.JWTAuthentication',
    )
}

# For session auth, Django's built-in system works out of the box
```

**Key Learning:** Use Django's permissions system for role-based access control (admin vs barbeiro).

## 📱 Real-time Updates

**From:** Polling the API
**To:** Django Channels for WebSocket support

```python
# consumers.py
from channels.generic.websocket import JsonWebsocketConsumer

class QueueConsumer(JsonWebsocketConsumer):
    def connect(self):
        self.accept()
        
        # Get tenant from URL
        tenant_slug = self.scope['url_route']['kwargs']['tenant_slug']
        self.tenant = get_tenant(tenant_slug)
        
        # Join tenant-specific group
        self.group_name = f"queue_{self.tenant.id}"
        async_to_sync(self.channel_layer.group_add)(
            self.group_name,
            self.channel_name
        )
    
    # Additional methods for handling events
```

**Key Learning:** Django Channels provides WebSockets support but requires additional setup with ASGI.

## 🎨 Templates and Static Files

**From:** EJS templates with tenant-specific folders
**To:** Django templates with template inheritance

```
templates/
├── base.html
├── barbershops/
│   ├── base_tenant.html
│   ├── checkin.html
│   ├── queue.html
│   └── tenant_specific/
│       ├── barbershop1/
│       └── barbershop2/
```

**Key Learning:** Use Django's template inheritance with a base template and tenant-specific extensions.

## 🚫 Things to Avoid

1. **Overcomplicating the ORM:** Django's ORM is powerful; avoid raw SQL except for specific performance optimizations
2. **Ignoring Django's conventions:** Follow Django's project structure patterns
3. **Session-based multi-tenancy:** URL-based is much more maintainable
4. **Premature optimization:** Start with Django's built-in tools before adding complexity
5. **Mixing concerns in views:** Keep views thin, business logic in models/services

## 📊 404 Issues Observed

We noticed many 404 errors hitting `/api/queue/stats` in the Node.js logs. When migrating to Django:

1. Ensure proper URL routing in Django's urls.py
2. Implement proper error handling and logging
3. Create appropriate API endpoints for queue statistics

## 💡 Additional Recommendations

1. **Use Django Rest Framework** for API endpoints
2. **Django Debug Toolbar** for development and optimization
3. **Implement proper logging** to catch and debug 404s and other errors
4. **Create custom template tags** for tenant-specific customization
5. **Use Django forms** for validation instead of manual validation
6. **Use Django's signals** for event-based actions (e.g., notifying when a client is added to queue)

## 🧪 Testing Strategy

Django provides excellent testing tools:

```python
from django.test import TestCase, Client

class QueueAPITests(TestCase):
    def setUp(self):
        # Create test data
        self.barbershop = Barbearia.objects.create(nome="Test Barber")
        
    def test_queue_stats_endpoint(self):
        # Test the API endpoint
        client = Client()
        response = client.get(f'/{self.barbershop.slug}/api/queue/stats')
        self.assertEqual(response.status_code, 200)
        self.assertIn('waiting', response.json())
```

## 🛠️ Deployment Lessons & Improvements

Based on our previous deployment challenges:

1. **Simplified Deployment Process**
   - Use Django's `collectstatic` to properly manage static files
   - Leverage Gunicorn + Nginx for production (rather than direct Node.js server)
   - No need to manually upload node_modules (use requirements.txt + pip instead)

2. **SSH Connection Issues**
   - Document required ports for Django (80/443 for web, 22 for SSH)
   - Set up proper user permissions for Django deployment
   - Consider using deployment tools like Fabric or Ansible

3. **File Management**
   - Django's static/media file separation simplifies file management
   - No need for complex FileZilla uploads of dependencies

```bash
# Sample Django deployment commands
python manage.py collectstatic
gunicorn eutonafila.wsgi:application --bind 0.0.0.0:8000
```

## 📝 Project Management

Convert the task tracking approach to Django context:

1. **Task Status Integration**
   - Create a Django admin interface for task status tracking
   - Use Django models to represent tasks and their statuses

2. **README-First Approach**
   - Maintain the "check README before changes" policy
   - Create Django custom commands for task status checking:

```python
# management/commands/check_tasks.py
from django.core.management.base import BaseCommand

class Command(BaseCommand):
    help = 'Check task completion status against README'
    
    def handle(self, *args, **options):
        # Implementation
        pass
```

## 🎯 Frontend Migration Strategy

From our previous frontend struggles:

1. **Mockup Integration**
   - Use Django's template system to integrate existing mockups
   - Create a `mockups` app that serves static HTML versions during development:

```python
# mockups/views.py
def mockup_view(request, template_name):
    return render(request, f'mockups/{template_name}.html')
```

2. **Modern UI Development**
   - Consider adding a lightweight frontend build process (Webpack/Vite) 
   - Use Django Compressor for SASS/JS optimization
   - Maintain the existing design system but with Django template tags

3. **Responsive Design**
   - Leverage Django's template inheritance for device-specific templates
   - Use the same CSS framework but with Django's static file handling

## 📋 Tenant-Specific UI Patterns

Based on our work with the check-in and contact pages, we've developed several patterns for tenant-specific UIs:

1. **Consistent Layout Structure**
   - Convert tenant layout EJS templates to Django base templates
   - Use blocks for customizable sections:

```python
# base_tenant.html
{% extends "base.html" %}

{% block content %}
  <div class="tenant-header">
    {% block tenant_header %}{% endblock %}
  </div>
  <div class="tenant-main">
    {% block tenant_content %}{% endblock %}
  </div>
  <div class="tenant-footer">
    {% block tenant_footer %}{% endblock %}
  </div>
{% endblock %}
```

2. **Dynamic Form Handling**
   - Convert EJS form structures to Django forms
   - Use Django's CSRF protection instead of custom tokens
   - Leverage Django form validation instead of client-side validation:

```python
# forms.py
from django import forms

class CheckInForm(forms.Form):
    nome = forms.CharField(max_length=100, label="Nome")
    telefone = forms.CharField(max_length=15, label="Telefone")
    servico = forms.ModelChoiceField(queryset=Servico.objects.none(), label="Serviço")
    
    def __init__(self, *args, barbearia=None, **kwargs):
        super().__init__(*args, **kwargs)
        if barbearia:
            self.fields['servico'].queryset = Servico.objects.filter(barbearia=barbearia)
```

3. **Queue Management Interface**
   - Replace JavaScript polling with Django Channels
   - Convert status indicator colors to Django template tags:

```python
# templatetags/queue_tags.py
@register.simple_tag
def queue_status_color(wait_time):
    if wait_time < 15:
        return "success"
    elif wait_time < 30:
        return "warning"
    else:
        return "danger"
```

4. **Contact Page Implementation**
   - Create consistent contact form patterns across tenants
   - Use Django messages framework for form submission feedback
   - Store tenant business hours in structured format:

```python
# models.py
class BusinessHours(models.Model):
    barbearia = models.ForeignKey(Barbearia, on_delete=models.CASCADE)
    day = models.IntegerField(choices=DAY_CHOICES)
    opening_time = models.TimeField()
    closing_time = models.TimeField()
    is_closed = models.BooleanField(default=False)
```

Remember: The 404 errors in the logs indicate that proper API endpoint implementation and error handling should be a priority in the Django migration. 

## 📱 Mobile Responsiveness Lessons

Our work on the check-in page revealed important considerations for mobile responsiveness:

1. **Viewport-Specific Layouts**
   - Use Django's template system to serve different layouts based on viewport size
   - Implement mobile-first design patterns in Django templates

2. **Touch-Friendly UI Elements**
   - Convert click events to handle both click and touch interactions
   - Ensure form elements have appropriate size and spacing for mobile:

```python
# Include in base template
{% block extra_head %}
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <style>
    /* Touch-friendly form elements */
    input, select, button {
      min-height: 44px;
      min-width: 44px;
    }
  </style>
{% endblock %}
```

3. **Performance Optimization**
   - Implement Django template fragment caching for heavy UI elements
   - Optimize image delivery with Django's sorl-thumbnail:

```python
# settings.py
INSTALLED_APPS = [
    # ...
    'sorl.thumbnail',
]

# In templates
{% load thumbnail %}
{% thumbnail tenant.logo "150x150" crop="center" as im %}
  <img src="{{ im.url }}" width="{{ im.width }}" height="{{ im.height }}">
{% endthumbnail %}
```

## 🚫 Common Mistakes and Lessons Learned

Based on our development experience, here are critical mistakes to avoid and important lessons:

### Template System Mistakes

1. **❌ Inconsistent Layout References**
   - **Mistake**: Using different render parameters across controllers
   - **Solution**: Create a consistent rendering helper:

```python
# utils/rendering.py
def render_with_tenant(request, template, tenant=None, context={}):
    context['tenant'] = tenant
    context['is_home'] = template == 'home.html'
    return render(request, template, context)
```

2. **❌ Hardcoded Static Page Routing**
   - **Mistake**: Creating individual routes for each static page
   - **Solution**: Use a dynamic route handler with template lookup:

```python
# urls.py
path('<str:page_name>/', views.static_page, name='static_page')

# views.py
def static_page(request, page_name):
    try:
        return render_with_tenant(request, f'static/{page_name}.html')
    except TemplateDoesNotExist:
        raise Http404("Page not found")
```

3. **❌ Missing Mobile Breakpoints**
   - **Mistake**: Designing only for desktop with CSS overrides for mobile
   - **Solution**: Implement mobile-first design with breakpoints for larger screens

### Controller Logic Mistakes

1. **❌ Business Logic in Views**
   - **Mistake**: Placing complex calculations and business rules in views
   - **Solution**: Move business logic to models or service classes:

```python
# AVOID THIS IN VIEWS:
def average_wait_time(request, tenant_id):
    # Complex calculation directly in view
    wait_time = perform_complex_calculation()
    return JsonResponse({'wait_time': wait_time})

# BETTER APPROACH:
# models.py or services.py
def calculate_average_wait_time(tenant_id):
    # Complex calculation here
    return result

# views.py
def average_wait_time(request, tenant_id):
    wait_time = calculate_average_wait_time(tenant_id)
    return JsonResponse({'wait_time': wait_time})
```

2. **❌ Inconsistent Error Handling**
   - **Mistake**: Different error handling approaches across endpoints
   - **Solution**: Create a standardized error handling middleware

### Database and Model Mistakes

1. **❌ N+1 Query Problems**
   - **Mistake**: Loading related data in loops
   - **Solution**: Use Django's `select_related` and `prefetch_related`:

```python
# Bad approach
barbershops = Barbearia.objects.all()
for shop in barbershops:
    print(shop.owner.name)  # Causes additional query for each shop

# Good approach
barbershops = Barbearia.objects.select_related('owner').all()
for shop in barbershops:
    print(shop.owner.name)  # No additional queries
```

2. **❌ Improper Form Validation**
   - **Mistake**: Client-side only validation or inconsistent validation
   - **Solution**: Use Django forms for consistent validation:

```python
# forms.py
class CheckInForm(forms.Form):
    telefone = forms.CharField(validators=[
        RegexValidator(r'^\d{10,15}$', 'Enter a valid phone number.')
    ])
```

### Home Page and Landing Page Issues

1. **❌ Disjointed Landing Page Sections**
   - **Mistake**: Creating separate templates for each home page section
   - **Solution**: Use template includes with consistent context:

```python
# home.html
{% extends 'base.html' %}

{% block content %}
    {% include 'sections/hero.html' %}
    {% include 'sections/features.html' %}
    {% include 'sections/pricing.html' %}
    {% include 'sections/tenant_search.html' %}
{% endblock %}
```

2. **❌ Non-Scalable Tenant Showcase**
   - **Mistake**: Hardcoding featured tenants
   - **Solution**: Create a proper featured tenant selection system:

```python
# models.py
class Barbearia(models.Model):
    # Other fields
    is_featured = models.BooleanField(default=False)
    featured_order = models.PositiveIntegerField(null=True, blank=True)

# views.py
def home(request):
    featured_tenants = Barbearia.objects.filter(
        is_featured=True
    ).order_by('featured_order')
    return render(request, 'home.html', {'featured_tenants': featured_tenants})
```

### Deployment Issues

1. **❌ Manual File Upload Process**
   - **Mistake**: Using FTP for deployments
   - **Solution**: Implement CI/CD or use deployment tools:

```python
# fabfile.py example
from fabric.api import run, cd, env

env.hosts = ['your-server.com']
env.user = 'deploy-user'

def deploy():
    with cd('/path/to/app'):
        run('git pull')
        run('pip install -r requirements.txt')
        run('python manage.py migrate')
        run('python manage.py collectstatic --noinput')
        run('systemctl restart gunicorn')
```

2. **❌ Missing Environment Configuration**
   - **Mistake**: Hardcoded settings or missing environment variables
   - **Solution**: Use django-environ or similar:

```python
# settings.py
import environ

env = environ.Env()
environ.Env.read_env()

DEBUG = env.bool('DEBUG', default=False)
SECRET_KEY = env('SECRET_KEY')
```

## 🧠 Key Takeaways for Django Migration

1. **Layout First, Content Second**
   - Establish base template hierarchy before diving into individual pages
   - Ensure consistent context variables across all templates

2. **Embrace Django's Built-in Features**
   - Use Django's form system rather than reinventing validation
   - Leverage Django's ORM features for query optimization
   - Take advantage of the template inheritance system

3. **Separate Concerns Properly**
   - Models for data structure and business logic
   - Forms for validation
   - Views for request handling
   - Templates for presentation

4. **Plan for Multi-Tenancy from the Start**
   - Design database models with tenant isolation in mind
   - Create middleware for tenant resolution
   - Build a flexible template system that supports tenant customization

5. **Implement Proper Testing**
   - Unit tests for models and business logic
   - Form tests for validation
   - View tests for HTTP responses
   - Template tests for rendering 

   Role	    Use Case	                                                Description
Client	Enter queue at barbershop	Client selects a barbershop and joins the queue through a mobile or kiosk interface.
Client	View estimated wait time	Client views the expected wait time calculated by the system algorithm based on real-time data.
Client	Cancel place in line	Client chooses to leave the queue, freeing up their spot for others.
Client	Change barbershop	Client switches to a different barbershop available on the platform.
Client	Be redirected to another barbershop	System offers to redirect the client to a nearby less busy barbershop.
Client	Login (optional)	Client logs into the system for personalized access (optional).
Client	Receive SMS notification	System sends SMS to notify the client when it’s nearly their turn.
Client	View real-time queue status	Client can view the live queue status for their selected barbershop.
Client	View service history (future)	Client accesses their history of past visits and services (future feature).
Client	Rate barber after service (future)	Client rates the barber or experience post-appointment (future feature).
Client	Confirm attendance via SMS	Client confirms presence when notified, helping to reduce no-shows.
Client	Schedule future appointment (future)	Client books a future time slot instead of entering the queue (future).
Barber	Login to barber panel	Barber logs in to their dashboard to manage their queue.
Barber	Call next client	Barber clicks to call the next person in line for service.
Barber	Finish current appointment	Barber marks current service as complete and logs completion time.
Barber	Start break	Barber starts a timed break, updating their availability status.
Barber	End break	Barber ends their break and returns to active status.
Barber	View current queue	Barber views the current queue of waiting clients.
Barber	View service history	Barber reviews their past clients and time spent (if tracked).
Barber	Change status (available/unavailable)	Barber updates their availability manually (e.g., busy, free).
Barber	Receive return reminder	System reminds barber when it's time to return from break.
Admin/Owner	Login to admin panel	Admin logs into the panel to manage settings and analytics.
Admin/Owner	Add barbers	Admin adds a new barber to the system.
Admin/Owner	Edit barber details	Admin edits existing barber profiles or roles.
Admin/Owner	Manage services (name/duration)	Admin sets up offered services and durations.
Admin/Owner	Set estimated time per service	Admin defines estimated service durations per type.
Admin/Owner	Customize branding	Admin updates branding (name, colors, etc.) per tenant.
Admin/Owner	Manage SMS configurations	Admin configures the content and timing of SMS notifications.
Admin/Owner	View metrics and reports	Admin accesses performance and usage reports.
Admin/Owner	Track live queue activity	Admin sees real-time queue and barber activity.
Admin/Owner	Temporarily disable queue	Admin pauses queue access temporarily (e.g., holiday).
Admin/Owner	Manage multiple locations	Admin oversees multiple barbershops under same account.
Admin/Owner	Create new barbershop	Platform-level admin creates a new barbershop tenant.
Admin Master	Edit barbershop settings	Admin edits tenant data like name, configs, and theme.
Admin Master	View cross-barbershop analytics	Admin sees aggregate data across all barbershops.
Admin Master	Apply system updates	Admin performs system-wide updates or upgrades.
Admin Master	Manage subscription plans (future)	Admin handles subscriptions or billing plans (future).
Admin Master	Set redirect rules for busy barbershops	System uses rules to route clients to niche barbershops.
Admin Master	Calculate estimated wait time	Algorithm calculates estimated wait times based on recent data.
System	Update average cache	System stores new wait time averages in in-memory cache.
System	Redirect to alternative barbershop	System suggests alternative barbershop to balance load.
System	Detect inactive barbers	System marks barbers as inactive if idle too long.
System	Reset average time every 3 months	System resets wait time averages periodically for accuracy.
System	Login via web portal	User logs in securely using web or kiosk login forms.
Auth	Protect sensitive routes	System restricts access to sensitive areas via middleware.
Auth	JWT token authentication	System issues JWT token after successful login.
Auth	QR code login (future)	Client or barber logs in via QR code for fast access (future).
Auth	Join queue anonymously	Client uses kiosk or public device to join the queue anonymously.
Kiosk	Input basic data (name, phone)	Client inputs only essential info (name, phone) to get a ticket.
Kiosk	Watch queue call locally	Client watches queue updates and callouts on-screen at location.
Kiosk	Cancel attendance via kiosk (future)	Client removes themselves from the list using kiosk (future).
Kiosk	Mark 'I'm arriving' via PWA	Client taps a button to say 'I'm arriving soon' via PWA.
