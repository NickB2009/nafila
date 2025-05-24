from rest_framework import status
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework.permissions import AllowAny
from infrastructure.repositories import DjangoBarbeariaRepository, DjangoServicoRepository
from barbershop.models import Barbearia, Servico
from drf_yasg.utils import swagger_auto_schema
from drf_yasg import openapi
import logging
from django.utils import timezone

class BarbershopListView(APIView):
    """
    API view for listing all barbershops
    """
    permission_classes = (AllowAny,)
    
    @swagger_auto_schema(
        operation_description="List all barbershops",
        operation_summary="List barbershops",
        responses={
            200: openapi.Response(
                description="Successful response",
                schema=openapi.Schema(
                    type=openapi.TYPE_ARRAY,
                    items=openapi.Schema(
                        type=openapi.TYPE_OBJECT,
                        properties={
                            'id': openapi.Schema(type=openapi.TYPE_STRING, description='Barbershop UUID'),
                            'nome': openapi.Schema(type=openapi.TYPE_STRING, description='Barbershop name'),
                            'slug': openapi.Schema(type=openapi.TYPE_STRING, description='URL slug'),
                            'telefone': openapi.Schema(type=openapi.TYPE_STRING, description='Phone number'),
                            'endereco': openapi.Schema(type=openapi.TYPE_STRING, description='Address'),
                        }
                    )
                )
            )
        }
    )
    def get(self, request, format=None):
        """List all barbershops"""
        try:
            logger.debug("Listing all barbershops")
            
            # Use direct SQL to avoid ORM issues
            from django.db import connection
            with connection.cursor() as cursor:
                cursor.execute(
                    "SELECT id, nome, slug, telefone, endereco, created_at, updated_at "
                    "FROM barbershop_barbearia"
                )
                
                barbershops = []
                for row in cursor.fetchall():
                    barbershop = {
                        "id": row[0],
                        "nome": row[1],
                        "slug": row[2],
                        "telefone": row[3] or "",
                        "endereco": row[4] or "",
                        "created_at": row[5],
                        "updated_at": row[6]
                    }
                    barbershops.append(barbershop)
                
            return Response(barbershops, status=status.HTTP_200_OK)
        except Exception as e:
            logger.error(f"ERROR: {str(e)}\n", exc_info=True)
            return Response({"error": f"Erro ao listar barbearias: {str(e)}"}, 
                          status=status.HTTP_500_INTERNAL_SERVER_ERROR)


class BarbershopDetailView(APIView):
    """
    API view for getting barbershop details
    """
    permission_classes = [AllowAny]
    
    @swagger_auto_schema(
        operation_description="Get detailed information about a specific barbershop",
        operation_summary="Get barbershop details",
        manual_parameters=[
            openapi.Parameter(
                name='slug',
                in_=openapi.IN_PATH,
                type=openapi.TYPE_STRING,
                description='Barbershop URL slug',
                required=True
            ),
        ],
        responses={
            200: openapi.Response(
                description="Successful response",
                schema=openapi.Schema(
                    type=openapi.TYPE_OBJECT,
                    properties={
                        'id': openapi.Schema(type=openapi.TYPE_STRING, description='Barbershop UUID'),
                        'nome': openapi.Schema(type=openapi.TYPE_STRING, description='Barbershop name'),
                        'slug': openapi.Schema(type=openapi.TYPE_STRING, description='URL slug'),
                        'telefone': openapi.Schema(type=openapi.TYPE_STRING, description='Phone number'),
                        'endereco': openapi.Schema(type=openapi.TYPE_STRING, description='Address'),
                        'cores': openapi.Schema(type=openapi.TYPE_ARRAY, items=openapi.Schema(type=openapi.TYPE_STRING), description='Brand colors'),
                        'logo': openapi.Schema(type=openapi.TYPE_STRING, description='Logo URL'),
                        'horario_abertura': openapi.Schema(type=openapi.TYPE_STRING, description='Opening time'),
                        'horario_fechamento': openapi.Schema(type=openapi.TYPE_STRING, description='Closing time'),
                        'dias_funcionamento': openapi.Schema(type=openapi.TYPE_ARRAY, items=openapi.Schema(type=openapi.TYPE_INTEGER), description='Operating days'),
                        'esta_aberto': openapi.Schema(type=openapi.TYPE_BOOLEAN, description='Whether the barbershop is open'),
                        'created_at': openapi.Schema(type=openapi.TYPE_STRING, format='date-time', description='Creation timestamp'),
                        'updated_at': openapi.Schema(type=openapi.TYPE_STRING, format='date-time', description='Last update timestamp'),
                        'servicos': openapi.Schema(type=openapi.TYPE_ARRAY, items=openapi.Schema(type=openapi.TYPE_OBJECT), description='List of services'),
                        'barbeiros': openapi.Schema(type=openapi.TYPE_ARRAY, items=openapi.Schema(type=openapi.TYPE_OBJECT), description='List of barbers'),
                    }
                )
            ),
            404: openapi.Response(
                description="Barbershop not found",
                schema=openapi.Schema(
                    type=openapi.TYPE_OBJECT,
                    properties={
                        'detail': openapi.Schema(type=openapi.TYPE_STRING, description='Error message')
                    }
                )
            )
        }
    )
    def get(self, request, slug=None, *args, **kwargs):
        """Get detailed information about a specific barbershop"""
        try:
            logger = logging.getLogger(__name__)
            
            logger.debug(f"1. Processing request for slug: {slug}, type: {type(slug)}")
            
            # Ensure slug is a string
            if slug is None:
                clean_slug = ""
            elif isinstance(slug, (int, float)):
                clean_slug = str(slug)
            elif isinstance(slug, str):
                clean_slug = slug
            else:
                # Handle any other type by converting to string
                clean_slug = str(slug)
                
            logger.debug(f"2. Converted slug to string: {clean_slug}, type: {type(clean_slug)}")
                
            # Clean the slug - remove any @eutonafila suffix if present
            if '@' in clean_slug:
                clean_slug = clean_slug.split('@')[0]
                logger.debug(f"3. Cleaned @ from slug: {clean_slug}")
            
            logger.debug(f"4. Attempting to get barbearia with slug: {clean_slug}")
            
            # Use direct SQL to avoid ORM issues
            from django.db import connection
            with connection.cursor() as cursor:
                cursor.execute(
                    "SELECT id, nome, slug, telefone, endereco, cores, logo, "
                    "horario_abertura, horario_fechamento, dias_funcionamento, "
                    "created_at, updated_at, descricao_curta, enable_priority_queue, "
                    "max_capacity FROM barbershop_barbearia WHERE slug = %s", 
                    [clean_slug]
                )
                row = cursor.fetchone()
                
                if not row:
                    return Response({"detail": f"Barbearia com slug '{clean_slug}' não encontrada"}, 
                                  status=status.HTTP_404_NOT_FOUND)
                
                # Convert row to dictionary
                barbearia_data = {
                    "id": row[0],
                    "nome": row[1],
                    "slug": row[2],
                    "telefone": row[3] or "",
                    "endereco": row[4] or "",
                    "cores": row[5] if isinstance(row[5], list) else [],
                    "logo": row[6] or "",
                    "horario_abertura": row[7],
                    "horario_fechamento": row[8],
                    "dias_funcionamento": row[9] if isinstance(row[9], list) else [],
                    "created_at": row[10],
                    "updated_at": row[11],
                    "descricao_curta": row[12] or "",
                    "enable_priority_queue": bool(row[13]),
                    "max_capacity": row[14] or 10,
                }
                
                # Handle JSON fields that might be stored as strings
                import json
                for field in ['cores', 'dias_funcionamento']:
                    if isinstance(barbearia_data[field], str):
                        try:
                            barbearia_data[field] = json.loads(barbearia_data[field])
                        except Exception:
                            barbearia_data[field] = []
                
                # Get services
                cursor.execute(
                    "SELECT id, nome, preco, duracao, descricao FROM barbershop_servico "
                    "WHERE barbearia_id = %s", 
                    [barbearia_data["id"]]
                )
                services = []
                for service_row in cursor.fetchall():
                    services.append({
                        "id": service_row[0],
                        "nome": service_row[1],
                        "preco": float(service_row[2]),
                        "duracao": service_row[3],
                        "descricao": service_row[4] or "",
                    })
                barbearia_data["servicos"] = services
                
                # Get barbers (if implemented)
                barbearia_data["barbeiros"] = []
                
                # Add esta_aberto based on current time
                barbearia_data["esta_aberto"] = True  # Simplified - always open for demo
                
            return Response(barbearia_data, status=status.HTTP_200_OK)
            
        except Exception as e:
            logger.error(f"ERROR: {str(e)}\n", exc_info=True)
            return Response({"error": f"Erro ao buscar barbearia: {str(e)}"}, 
                          status=status.HTTP_500_INTERNAL_SERVER_ERROR) 

class ServiceListView(APIView):
    """
    API view for listing services at a specific barbershop
    """
    permission_classes = (AllowAny,)

    @swagger_auto_schema(
        operation_description="List all services at a specific barbershop",
        operation_summary="List services",
        responses={
            200: openapi.Response(description="Success"),
            404: openapi.Response(description="Not found")
        }
    )
    def get(self, request, slug, format=None):
        try:
            repository = DjangoServicoRepository()
            services = repository.get_by_barbershop_slug(slug)
            data = [
                {
                    "id": str(service.id),
                    "name": service.nome,
                    "description": service.descricao,
                    "price": float(service.preco),
                    "duration": service.duracao_minutos,
                    "complexity": service.complexidade if hasattr(service, 'complexidade') else None,
                    "image_url": service.imagem.url if service.imagem else None
                }
                for service in services
            ]
            return Response(data)
        except Exception as e:
            return Response({"error": str(e)}, status=status.HTTP_404_NOT_FOUND)

class BarberListView(APIView):
    """
    API view for listing barbers at a specific barbershop
    """
    permission_classes = (AllowAny,)

    @swagger_auto_schema(
        operation_description="List all barbers at a specific barbershop",
        operation_summary="List barbers",
        responses={
            200: openapi.Response(description="Success"),
            404: openapi.Response(description="Not found")
        }
    )
    def get(self, request, slug, format=None):
        try:
            repository = DjangoBarbeariaRepository()
            barbershop = repository.get_by_slug(slug)
            barbers = barbershop.barbeiros.all()
            data = [
                {
                    "id": str(barber.id),
                    "name": barber.nome,
                    "status": barber.status,
                    "specialty": barber.especialidade,
                    "image_url": barber.foto.url if barber.foto else None
                }
                for barber in barbers
            ]
            return Response(data)
        except Exception as e:
            return Response({"error": str(e)}, status=status.HTTP_404_NOT_FOUND)

class QueueListView(APIView):
    """
    API view for listing queue entries at a specific barbershop
    """
    permission_classes = (AllowAny,)

    @swagger_auto_schema(
        operation_description="List all queue entries at a specific barbershop",
        operation_summary="List queue",
        responses={
            200: openapi.Response(description="Success"),
            404: openapi.Response(description="Not found")
        }
    )
    def get(self, request, slug, format=None):
        try:
            repository = DjangoBarbeariaRepository()
            barbershop = repository.get_by_slug(slug)
            
            # Get the first queue (most barbershops will have just one)
            queue = barbershop.filas.first()
            
            if not queue:
                return Response({"error": "No queue found for this barbershop"}, status=status.HTTP_404_NOT_FOUND)
            
            entries = queue.entradas.all().order_by('posicao')
            data = [
                {
                    "id": str(entry.id),
                    "client_name": entry.cliente_nome,
                    "service_name": entry.servico.nome if entry.servico else "Unknown",
                    "position": entry.posicao,
                    "status": entry.status,
                    "estimated_wait": entry.tempo_espera_estimado,
                    "created_at": entry.data_entrada.isoformat(),
                }
                for entry in entries
            ]
            return Response(data)
        except Exception as e:
            return Response({"error": str(e)}, status=status.HTTP_404_NOT_FOUND)

class CheckInView(APIView):
    """
    API view for client check-in to a barbershop queue
    """
    permission_classes = (AllowAny,)

    @swagger_auto_schema(
        operation_description="Check-in to a barbershop queue",
        operation_summary="Client check-in",
        request_body=openapi.Schema(
            type=openapi.TYPE_OBJECT,
            required=['barbershop_slug', 'client_name', 'service_id'],
            properties={
                'barbershop_slug': openapi.Schema(type=openapi.TYPE_STRING),
                'client_name': openapi.Schema(type=openapi.TYPE_STRING),
                'service_id': openapi.Schema(type=openapi.TYPE_STRING),
                'phone': openapi.Schema(type=openapi.TYPE_STRING),
                'email': openapi.Schema(type=openapi.TYPE_STRING)
            }
        ),
        responses={
            201: openapi.Response(description="Created"),
            400: openapi.Response(description="Bad request"),
            404: openapi.Response(description="Not found")
        }
    )
    def post(self, request, format=None):
        try:
            data = request.data
            slug = data.get('barbershop_slug')
            client_name = data.get('client_name')
            service_id = data.get('service_id')
            phone = data.get('phone', '')
            email = data.get('email', '')
            
            # Validate required fields
            if not all([slug, client_name, service_id]):
                return Response(
                    {"error": "Missing required fields: barbershop_slug, client_name, service_id"},
                    status=status.HTTP_400_BAD_REQUEST
                )
            
            # Get the barbershop
            barbershop_repo = DjangoBarbeariaRepository()
            barbershop = barbershop_repo.get_by_slug(slug)
            
            if not barbershop:
                return Response(
                    {"error": f"Barbershop with slug '{slug}' not found"},
                    status=status.HTTP_404_NOT_FOUND
                )
            
            # Get the service
            service_repo = DjangoServicoRepository()
            service = service_repo.get_by_id(service_id)
            
            if not service:
                return Response(
                    {"error": f"Service with id '{service_id}' not found"},
                    status=status.HTTP_404_NOT_FOUND
                )
            
            # Get the queue (most barbershops will have just one)
            queue = barbershop.filas.first()
            
            if not queue:
                return Response(
                    {"error": "No queue found for this barbershop"},
                    status=status.HTTP_404_NOT_FOUND
                )
            
            # Add client to queue
            entry = queue.adicionar_cliente(client_name, service, phone=phone, email=email)
            
            return Response({
                "id": str(entry.id),
                "message": f"Added {client_name} to the queue",
                "position": entry.posicao,
                "estimated_wait": entry.tempo_espera_estimado
            }, status=status.HTTP_201_CREATED)
            
        except Exception as e:
            return Response({"error": str(e)}, status=status.HTTP_400_BAD_REQUEST)

class QueueStatusView(APIView):
    """
    API view for checking queue entry status
    """
    permission_classes = (AllowAny,)

    @swagger_auto_schema(
        operation_description="Check status of a queue entry",
        operation_summary="Check queue status",
        responses={
            200: openapi.Response(description="Success"),
            404: openapi.Response(description="Not found")
        }
    )
    def get(self, request, queue_id, format=None):
        try:
            from barbershop.models import EntradaFila
            
            try:
                entry = EntradaFila.objects.get(id=queue_id)
            except EntradaFila.DoesNotExist:
                return Response(
                    {"error": f"Queue entry with id '{queue_id}' not found"},
                    status=status.HTTP_404_NOT_FOUND
                )
            
            # Get the current position and estimated wait time
            position = entry.posicao
            wait_time = entry.tempo_espera_estimado
            status_value = entry.status
            
            # Get all entries ahead in the queue
            queue = entry.fila
            entries_ahead = queue.entradas.filter(posicao__lt=position).count()
            
            data = {
                "id": str(entry.id),
                "client_name": entry.cliente_nome,
                "position": position,
                "entries_ahead": entries_ahead,
                "status": status_value,
                "estimated_wait": wait_time,
                "service": {
                    "id": str(entry.servico.id),
                    "name": entry.servico.nome,
                    "duration": entry.servico.duracao_minutos
                } if entry.servico else None,
                "barbershop": {
                    "id": str(queue.barbearia.id),
                    "name": queue.barbearia.nome,
                    "slug": queue.barbearia.slug
                },
                "created_at": entry.data_entrada.isoformat(),
                "updated_at": entry.data_atualizacao.isoformat() if hasattr(entry, 'data_atualizacao') else None,
            }
            
            # Add barber info if assigned
            if entry.barbeiro:
                data["barber"] = {
                    "id": str(entry.barbeiro.id),
                    "name": entry.barbeiro.nome
                }
            
            return Response(data)
            
        except Exception as e:
            return Response({"error": str(e)}, status=status.HTTP_400_BAD_REQUEST)

class CancelQueueEntryView(APIView):
    """
    API view for canceling a queue entry
    """
    permission_classes = (AllowAny,)

    @swagger_auto_schema(
        operation_description="Cancel a queue entry",
        operation_summary="Cancel queue entry",
        responses={
            200: openapi.Response(description="Success"),
            404: openapi.Response(description="Not found")
        }
    )
    def post(self, request, queue_id, format=None):
        try:
            from barbershop.models import EntradaFila, Fila
            from domain.domain_models import EntradaFila as DomainEntradaFila
            
            try:
                entry = EntradaFila.objects.get(id=queue_id)
            except EntradaFila.DoesNotExist:
                return Response(
                    {"error": f"Queue entry with id '{queue_id}' not found"},
                    status=status.HTTP_404_NOT_FOUND
                )
            
            # Get the queue
            queue = entry.fila
            
            # Cancel the entry
            entry.status = DomainEntradaFila.Status.CANCELADO
            entry.save()
            
            # Reorder queue positions
            queue.reordenar_posicoes()
            
            return Response({
                "message": f"Queue entry {queue_id} has been canceled",
                "status": "canceled"
            })
            
        except Exception as e:
            return Response({"error": str(e)}, status=status.HTTP_400_BAD_REQUEST)

class StartServiceView(APIView):
    """
    API view for starting service for a queue entry
    """
    permission_classes = (AllowAny,)

    @swagger_auto_schema(
        operation_description="Start service for a queue entry",
        operation_summary="Start service",
        request_body=openapi.Schema(
            type=openapi.TYPE_OBJECT,
            required=['barber_id'],
            properties={
                'barber_id': openapi.Schema(type=openapi.TYPE_STRING)
            }
        ),
        responses={
            200: openapi.Response(description="Success"),
            400: openapi.Response(description="Bad request"),
            404: openapi.Response(description="Not found")
        }
    )
    def post(self, request, queue_id, format=None):
        try:
            from barbershop.models import EntradaFila, Barbeiro
            from domain.domain_models import EntradaFila as DomainEntradaFila
            
            data = request.data
            barber_id = data.get('barber_id')
            
            if not barber_id:
                return Response(
                    {"error": "Missing required field: barber_id"},
                    status=status.HTTP_400_BAD_REQUEST
                )
            
            try:
                entry = EntradaFila.objects.get(id=queue_id)
            except EntradaFila.DoesNotExist:
                return Response(
                    {"error": f"Queue entry with id '{queue_id}' not found"},
                    status=status.HTTP_404_NOT_FOUND
                )
            
            try:
                barber = Barbeiro.objects.get(id=barber_id)
            except Barbeiro.DoesNotExist:
                return Response(
                    {"error": f"Barber with id '{barber_id}' not found"},
                    status=status.HTTP_404_NOT_FOUND
                )
            
            # Start the service
            entry.status = DomainEntradaFila.Status.EM_ATENDIMENTO
            entry.barbeiro = barber
            entry.data_inicio_atendimento = timezone.now()
            entry.save()
            
            return Response({
                "message": f"Service for queue entry {queue_id} has started with barber {barber.nome}",
                "status": "in_service"
            })
            
        except Exception as e:
            return Response({"error": str(e)}, status=status.HTTP_400_BAD_REQUEST)

class FinishServiceView(APIView):
    """
    API view for finishing service for a queue entry
    """
    permission_classes = (AllowAny,)

    @swagger_auto_schema(
        operation_description="Finish service for a queue entry",
        operation_summary="Finish service",
        responses={
            200: openapi.Response(description="Success"),
            404: openapi.Response(description="Not found")
        }
    )
    def post(self, request, queue_id, format=None):
        try:
            from barbershop.models import EntradaFila
            from domain.domain_models import EntradaFila as DomainEntradaFila
            from django.utils import timezone
            
            try:
                entry = EntradaFila.objects.get(id=queue_id)
            except EntradaFila.DoesNotExist:
                return Response(
                    {"error": f"Queue entry with id '{queue_id}' not found"},
                    status=status.HTTP_404_NOT_FOUND
                )
            
            # Finish the service
            entry.status = DomainEntradaFila.Status.CONCLUIDO
            entry.data_fim_atendimento = timezone.now()
            entry.save()
            
            # Get the queue and reorder positions
            queue = entry.fila
            queue.reordenar_posicoes()
            
            return Response({
                "message": f"Service for queue entry {queue_id} has been completed",
                "status": "completed"
            })
            
        except Exception as e:
            return Response({"error": str(e)}, status=status.HTTP_400_BAD_REQUEST) 