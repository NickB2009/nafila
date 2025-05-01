---

# 🧾 README — EuTôNaFila (AI-Optimized for Context & Accuracy)

## 🧠 Prompt Context for AI Assistants

> This project is a **multi-tenant SaaS** built in **Django** for managing walk-in queues at barbershops.  
> It does **not** use Redis, CQRS, or multiple databases.  
> It uses a **single transactional MySQL database**, with **Django's cache framework**, and **Django's MTV architecture with Clean Architecture principles** for clarity and testability.  
> 
> ✨ Focus on simplicity, reusability, performance, and low operational cost.  
> 🧩 Follow the domain model structure for logic and avoid unnecessary complexity.  
> 
> 🔥 Use this prompt in **Cursor AI** or **GitHub Copilot** to generate new code inside this repo:
```plaintext
"You are working on EuTôNaFila, a multi-tenant barbershop SaaS built with Django, MySQL, and Django templates. No Redis, no CQRS. Business logic stays inside models or service classes. Caching is done with Django's cache framework. Use templates with inheritance for tenant customization. Follow Django's MTV pattern with Clean Architecture principles."
```

---

## 🎯 Project Overview

**EuTôNaFila** is a digital queue management platform for barbershops in Brazil. Clients check in digitally and receive an estimated wait time. Barbers pull the next client when ready, and admins track everything through dashboards.

---

## ⚙️ Stack

| Layer         | Tech                                              |
|---------------|---------------------------------------------------|
| Frontend      | HTML, CSS, JS (Vanilla), Django Templates         |
| Backend       | Django                                            |
| DB            | MySQL with Django ORM                             |
| Auth          | Django auth + DRF JWT for API                     |
| Cache         | Django's cache framework (LocMemCache)            |
| Hosting       | Low-cost VPS or shared Django hosting             |
| Infra         | No Redis, no microservices, no third-party cache  |

---

## 🧱 Architecture (Django MTV with Clean Architecture)

**Directory structure:**
```
nafila/
├── barbershop/      # Main app
│   ├── models.py    # Domain entities (Fila, Cliente, Barbeiro)
│   ├── services.py  # Use cases, business logic
│   ├── views.py     # Request handling
│   ├── urls.py      # URL patterns
│   ├── admin.py     # Admin interface
│   └── templates/   # Views/templates
├── core/            # Core functionality
│   ├── middleware/  # Including tenant middleware
│   └── utils/       # Utilities and helpers
└── templates/       # Base templates
```

### ❌ Not Used:
- No Redis
- No CQRS
- No event sourcing
- No dual-model read/write logic

---

## 🧃 Caching Strategy (Django Cache)

We use **Django's cache framework** with local memory cache to hold frequently used values:

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

> ⚠️ Caching is lightweight and will not persist across deployments. This is intentional for simplicity.

---

## 💇 Core Features (Modular by Role)

| Cliente               | Barbeiro                      | Admin                            |
|-----------------------|-------------------------------|----------------------------------|
| Check-in com QR code  | Atender próximo cliente       | Adicionar barbeiros              |
| Ver tempo estimado    | Pausa para almoço (30–60 min) | Gerenciar serviços e preços      |
| Branding personalizado| Fila ao vivo                  | Ver métricas e relatórios        |

---

## 🌐 Multi-Tenancy via Subpaths

Each barbearia is a **tenant** loaded via subpath using custom middleware:

```plaintext
eutonafila.com.br/barbeariaX
```

```python
# core/middleware/tenant.py
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

Tenant config includes:
```python
class Barbearia(models.Model):
    nome = models.CharField(max_length=100)
    slug = models.SlugField(unique=True)
    cores = models.JSONField(default=list)  # ["#FF0066", "#222222"]
    logo = models.ImageField(upload_to='logos/', null=True, blank=True)
    # Other fields

class Servico(models.Model):
    barbearia = models.ForeignKey(Barbearia, on_delete=models.CASCADE, related_name='servicos')
    nome = models.CharField(max_length=100)
    duracao = models.IntegerField()  # in minutes
    preco = models.DecimalField(max_digits=10, decimal_places=2)
```

> 🔧 Views should load tenant configs from request.tenant and use appropriate templates.

---

## 🎨 UI Design Notes for AI

- Use **Glassmorphism**, **dark theme**, and **vibrant gradients**
- Fonts: `Montserrat` for headings, `Poppins` for body text
- Icons: Font Awesome 6
- Use Django templates with inheritance for tenant customization

> 🧠 Template structure supports dynamic tenant-specific customization.

---

## 🔐 Auth

- Django auth with Django Rest Framework JWT for API auth
- Login views for standard Django auth
- Custom permissions for `admin` and `barbeiro` roles
- Protected views and API endpoints

---

## 📦 Setup

```bash
# Clone repository
git clone https://github.com/seu-usuario/nafila.git
cd nafila

# Set up Python environment
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt

# Configure database
cp .env.example .env
# Edit .env with your database credentials

# Run migrations
python manage.py migrate

# Create superuser
python manage.py createsuperuser

# Run development server
python manage.py runserver
```

App:  
`http://localhost:8000/barbeariaX`

---

## ✅ AI Prompt Tips

> You are adding a new feature to a Django project following MTV pattern with Clean Architecture principles.  
> Business logic should live in models or service classes.  
> Use Django's built-in features where possible.  
> Use Django templates with inheritance for tenant customization.  
> Multi-tenancy is implemented via URL paths and middleware.  
> Use Django's cache framework for caching.

---

## 📄 License

MIT – Free for use, contribution, and commercial projects.

---

HERE ARE SOME PROMPTS
**Say less — we're going full-on AI whisperer mode.**  
Below is the **README-enhanced version** with a dedicated section just for **AI-aware inline comments**, so no matter what coding assistant is reading the project, it knows **exactly** what to do and what *not* to do.

---

## 🤖 AI-Aware Inline Comments (Code Prompting Guide)

To ensure AI coding assistants (like Cursor, Copilot, Cody, etc.) understand the constraints and architecture of **EuTôNaFila**, we use **purposeful inline comments** that guide auto-generated code and prevent hallucinations.

These comments are strategically placed across layers:

---

### 🧱 **Domain Layer (`/src/domain/`)**
```js
// AI: This is a rich domain model (Entity).
// Encapsulate business logic here.
// Do NOT call DB, do NOT handle HTTP or routing here.
```

```js
// AI: Add behavior to this method (e.g. calcularTempoEspera).
// Keep it pure and isolated from infrastructure concerns.
```

---

### 🎯 **Application Layer (`/src/application/`)**
```js
// AI: This is a use case (command or query).
// Only orchestrate here — call domain methods and services.
// Do NOT implement raw SQL or business logic in this layer.
```

```js
// AI: Any external I/O should be abstracted via injected service.
```

---

### 🏗️ **Infrastructure Layer (`/src/infrastructure/`)**

#### 🔐 Auth
```js
// AI: JWT logic only. Do not implement roles or session control here.
```

#### 🧠 Caching
```js
// AI: This is a local in-memory cache using node-cache.
// We are NOT using Redis. Never suggest external caching or queuing tools here.
```

#### 🗄️ DB Access
```js
// AI: Repository layer for MySQL access.
// Use raw SQL or lightweight query builders.
// Do NOT use ORM or auto-migrations.
```

---

### 📡 **Interfaces Layer (`/src/interfaces/`)**

#### 🧩 Controllers
```js
// AI: Route controller. Only coordinates HTTP req → useCase → res.
// Do not embed business logic here. Call use cases from /application.
```

#### 🧭 Routes
```js
// AI: Define routes and assign to controller functions.
// Keep this layer free of logic or validation.
```

#### 🧑‍🎨 EJS Templates
```ejs
<!-- AI: This is an EJS template for a specific barbearia. -->
<!-- Use data injected by controller from tenant config. -->
<!-- Do NOT hardcode services, prices, or colors. Use <%= %> vars. -->
```

---

### 🌐 **Multi-Tenancy / Config**

```js
// AI: Tenant config loader.
// Loads per-barbearia settings like name, services, colors.
// Each request context includes tenant data.
```

```js
// AI: All customization (branding, services) is loaded from JSON config.
// Do not store this in DB yet.
```

---

### 🧪 **General Style & Structure**
```js
// AI: This is a Clean Architecture Node.js project (not NestJS).
// Avoid complex patterns unless absolutely needed.
// Use modules, not classes, where possible.
```

```js
// AI: This app runs on a single-node budget stack.
// Do not introduce microservices, Kafka, Redis, or GraphQL.
```

---
- Custom permissions for `admin` and `barbeiro`
```

# EuTôNaFila - Barbershop Queue Management System

A Django-based queue management system for barbershops, implementing Clean Architecture principles and featuring asynchronous processing with Celery and RabbitMQ.

## Features

- Real-time queue updates with WebSockets
- Asynchronous processing with Celery and RabbitMQ
- Modern, responsive UI
- Clean Architecture design
- Multi-tenant support for different barbershops

## Installation

1. Clone the repository
2. Create a virtual environment and activate it:
   ```bash
   python -m venv venv
   source venv/bin/activate  # On Windows: venv\Scripts\activate
   ```
3. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```
4. Apply migrations:
   ```bash
   python manage.py migrate
   ```

## Running the Application

### 1. Start RabbitMQ Server

#### Using Docker (recommended):
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management
```

#### Using RabbitMQ installation:
Start the RabbitMQ service according to your OS-specific instructions.

### 2. Start Celery Worker

```bash
# Run from the eutonafila directory
python run_celery.py
```

Or directly using the Celery command:
```bash
celery -A eutonafila worker --loglevel=info
```

### 3. Start Django Development Server

```bash
python manage.py runserver
```

## Architecture

The application follows Clean Architecture principles with the following layers:

1. **Domain Layer**: Contains entities and business rules
2. **Application Layer**: Contains use cases and services
3. **Infrastructure Layer**: Contains repositories, frameworks, and external services
4. **Interface Layer**: Contains controllers, views, and presenters

### Message Queue Implementation

The system implements an event-driven architecture using RabbitMQ and Celery for asynchronous processing, following these principles:

1. **Decoupled Components**: Each part of the system operates independently
2. **High Scalability**: Workers can be scaled horizontally
3. **Fault Tolerance**: Failed operations can be retried
4. **Eventual Consistency**: System achieves consistency over time

## Development

### Project Structure

```
eutonafila/
├── domain/
│   ├── models.py           # Domain entities
│   └── interfaces/         # Abstract interfaces 
├── application/
│   ├── services.py         # Application services
│   ├── dtos.py             # Data Transfer Objects
│   └── tasks/              # Async task definitions
├── infrastructure/
│   ├── repositories/       # Repository implementations
│   └── messaging/          # Message queue infrastructure
├── barbershop/             # Interface layer for barbershop
│   ├── web_views.py        # Web views
│   ├── api_views.py        # API views
│   └── templates/          # HTML templates
├── static/                 # Static files
└── templates/              # Base templates
```

## License

MIT
