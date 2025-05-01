# Nafila API Documentation

## Overview

The Nafila API provides endpoints for managing barbershop queues. It allows clients to check-in to a barbershop's queue, check their status in the queue, and cancel their queue entry.

## API Endpoints

All API endpoints are documented using Swagger/OpenAPI. You can access the interactive documentation at:

- **Swagger UI**: `/swagger/` - Interactive documentation
- **ReDoc**: `/redoc/` - Alternative documentation view
- **OpenAPI Schema**: `/swagger.json` - Raw OpenAPI schema

## Available Endpoints

### Barbershop Management

- `GET /api/barbershops/` - List all barbershops
- `GET /api/barbershops/{slug}/` - Get detailed information about a specific barbershop

### Queue Management

- `POST /api/checkin/` - Add a client to a barbershop queue
- `GET /api/queue-status/{queue_id}/` - Check a client's status in the queue
- `POST /api/cancel-queue/{queue_id}/` - Cancel a client's queue entry

## Authentication

Most API endpoints are publicly accessible and do not require authentication.

## Directory Structure

The project follows Clean Architecture principles with the following structure:

```
/nafila
  ├── domain/                 # Business logic and entities
  ├── application/            # Use cases and application services
  ├── infrastructure/         # External interfaces (repositories, ORM)
  ├── eutonafila/             # Django project configuration
  │   └── eutonafila/         # Django settings module
  └── barbershop/             # Barbershop app
```

### Directory Explanation

The structure has two `eutonafila` directories for the following reasons:

1. The outer `eutonafila/` directory is the Django project root
2. The inner `eutonafila/eutonafila/` directory contains Django settings and configuration

This is the standard structure created by Django's `startproject` command. The outer directory is a container for the whole project, while the inner directory contains the actual Django settings module.

## Development

To run the API locally:

1. Activate the virtual environment:
   ```
   .\venv\Scripts\activate
   ```

2. Run the development server:
   ```
   python manage.py runserver
   ```

3. Access the API documentation at http://localhost:8000/swagger/

## Testing the API

You can test the API using:

1. The Swagger UI at `/swagger/`
2. Postman or similar API tools
3. Command-line tools like curl or httpie 