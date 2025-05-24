from rest_framework import status
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework.permissions import AllowAny
from infrastructure.repositories import DjangoBarbeariaRepository, DjangoServicoRepository
from barbershop.models import Barbearia, Servico, Barbeiro
from drf_yasg.utils import swagger_auto_schema
from drf_yasg import openapi
import logging
from django.utils import timezone
import uuid
import django.core.exceptions

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
            logger = logging.getLogger(__name__)
            logger.debug("Listing all barbershops")
            
            # Use Django ORM instead of raw SQL
            barbershops = Barbearia.objects.all()
            data = [
                {
                    "id": str(barbershop.id),
                    "nome": barbershop.nome,
                    "slug": barbershop.slug,
                    "telefone": barbershop.telefone or "",
                    "endereco": barbershop.endereco or "",
                    "created_at": barbershop.created_at.isoformat() if barbershop.created_at else None,
                    "updated_at": barbershop.updated_at.isoformat() if barbershop.updated_at else None
                }
                for barbershop in barbershops
            ]
            
            return Response(data, status=status.HTTP_200_OK)
        except Exception as e:
            logger = logging.getLogger(__name__)
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
            200: openapi.Response(description="Success"),
            404: openapi.Response(description="Not found")
        }
    )
    
    def get(self, request, slug):
        """Get detailed information about a specific barbershop"""
        # Force a print statement to console at the very beginning
        print(f"--- API VIEW HIT: BarbershopDetailView.get for slug: {slug} ---", flush=True)
        logger = logging.getLogger(__name__)
        try:
            logger.info(f"[API_VIEW] Processing request for slug: {slug}")
            
            # Clean the slug - remove any @eutonafila suffix if present
            if '@' in slug:
                slug = slug.split('@')[0]
                logger.info(f"[API_VIEW] Cleaned @ from slug: {slug}")
            
            # Try to get by slug
            barbershop = None # Initialize
            try:
                logger.info(f"[API_VIEW] Attempting Barbearia.objects.get(slug='{slug}')")
                barbershop = Barbearia.objects.get(slug=slug)
                logger.info(f"[API_VIEW] Initial fetch: barbershop.slug='{barbershop.slug}', barbershop.pk='{str(getattr(barbershop, 'pk', 'PK_UNAVAILABLE'))}', type(barbershop.pk)='{type(getattr(barbershop, 'pk', None))}', barbershop.id='{str(barbershop.id)}', type(barbershop.id)='{type(barbershop.id)}'")

                # <<< WORKAROUND V2 START >>>
                # Check if the primary key itself is problematic, or if the ID field is problematic
                needs_id_fix = False
                current_pk_val = getattr(barbershop, 'pk', None)

                if not isinstance(current_pk_val, uuid.UUID):
                    logger.warning(f"[API_VIEW] barbershop.pk is NOT a UUID. Type: {type(current_pk_val)}. Value: {str(current_pk_val)}. This is highly unusual.")
                    # If PK itself is bad, we probably can't reliably reload. This indicates a deeper issue.
                    # For now, we will still try to see if .id is also bad.
                
                if not isinstance(barbershop.id, uuid.UUID):
                    logger.warning(f"[API_VIEW] barbershop.id is NOT a UUID instance (type: {type(barbershop.id)}, value: {str(barbershop.id)}). Will attempt to fix.")
                    needs_id_fix = True
                
                if needs_id_fix:
                    logger.info(f"[API_VIEW] Attempting to forcibly correct barbershop.id for pk: {str(current_pk_val)}")
                    try:
                        # We MUST use a known good UUID for lookup if pk is not a UUID.
                        # The only known good UUID for this problematic record is '33082411-2cc6-483d-ae1f-7e773df5781f' when slug is '1'
                        # This is very specific, but necessary if pk is also the DatabaseOperations object.
                        lookup_pk_value = None
                        if isinstance(current_pk_val, uuid.UUID):
                            lookup_pk_value = current_pk_val
                        elif barbershop.slug == '1': # Hardcoded fallback for the known problematic slug
                            logger.warning("[API_VIEW] Using hardcoded known good UUID for slug '1' because PK was not a UUID.")
                            lookup_pk_value = uuid.UUID('33082411-2cc6-483d-ae1f-7e773df5781f')
                        
                        if lookup_pk_value:
                            reloaded_barbershop_data = Barbearia.objects.filter(pk=lookup_pk_value).values('id', 'slug').first()
                            if reloaded_barbershop_data and isinstance(reloaded_barbershop_data['id'], uuid.UUID):
                                correct_id = reloaded_barbershop_data['id']
                                logger.info(f"[API_VIEW] Successfully re-fetched ID via .values(): {correct_id} (type: {type(correct_id)}). Assigning to instance.")
                                barbershop.id = correct_id # Directly assign the good UUID object
                                logger.info(f"[API_VIEW] After assignment: barbershop.id='{str(barbershop.id)}', type(barbershop.id)='{type(barbershop.id)}'")
                            else:
                                logger.error(f"[API_VIEW] Failed to re-fetch a valid UUID ID for pk '{str(lookup_pk_value)}'. Data: {reloaded_barbershop_data}")
                        else:
                            logger.error(f"[API_VIEW] Cannot attempt ID reload because a usable UUID PK for lookup could not be determined for slug '{barbershop.slug}'.")

                    except Exception as e_reload:
                        logger.error(f"[API_VIEW] Exception during ID reload attempt for pk '{str(current_pk_val)}': {str(e_reload)}", exc_info=True)
                # <<< WORKAROUND V2 END >>>

            except Barbearia.DoesNotExist:
                logger.error(f"[API_VIEW] No barbershop found with slug: {slug}")
                return Response({"detail": f"Barbearia com slug '{slug}' não encontrada"}, 
                              status=status.HTTP_404_NOT_FOUND)
            except Exception as e_get: # Catch any other error during get
                logger.error(f"[API_VIEW] Error during Barbearia.objects.get(slug=\'{slug}\'): {type(e_get).__name__} - {str(e_get)}", exc_info=True)
                # Directly return the specific error from get if it happens here
                return Response({"detail": f"Erro CRITICO ao buscar barbearia (get): {type(e_get).__name__} - {str(e_get)}"}, 
                                status=status.HTTP_500_INTERNAL_SERVER_ERROR)

            if not barbershop: # Safeguard
                logger.error(f"[API_VIEW] Barbershop is None after get for slug '{slug}', this should not happen.")
                return Response({"detail": f"Erro: Barbearia não encontrada para slug {slug} (inesperado)."}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)

            logger.info(f"[API_VIEW] Proceeding to prepare response_data. barbershop.id='{str(barbershop.id)}', type(barbershop.id)='{type(barbershop.id)}'")

            # Prepare response data - focus on ID first
            barbershop_id_for_response = "ERROR_PROCESSING_ID"
            try:
                # Test 1: What is the type of barbershop.id?
                logger.info(f"[API_VIEW] Type of barbershop.id before str(): {type(barbershop.id)}")
                # Test 2: Can it be stringified?
                barbershop_id_for_response = str(barbershop.id)
                logger.info(f"[API_VIEW] Successfully performed str(barbershop.id): {barbershop_id_for_response}")
                # Test 3: If it was stringified, is it a valid UUID?
                uuid.UUID(barbershop_id_for_response) # This will raise ValueError if not a valid UUID string
                logger.info(f"[API_VIEW] ID {barbershop_id_for_response} is a valid UUID string.")

            except ValueError as ve_uuid: # Error from uuid.UUID(str(barbershop.id))
                logger.error(f"[API_VIEW] str(barbershop.id) is NOT a valid UUID. Value: '{barbershop_id_for_response}'. Error: {str(ve_uuid)}", exc_info=True)
                # This is where the '%(value)s' error might be formed if a ValidationError is caught by the generic Exception
                # We want to see the actual value that failed validation
                # If barbershop_id_for_response contains the DatabaseOperations object string, it will be logged.
                # The response will show the problematic string directly.
                return Response({"detail": f"Erro de UUID: O ID da barbearia '{barbershop_id_for_response}' não é um UUID válido. ({str(ve_uuid)})"}, 
                                status=status.HTTP_500_INTERNAL_SERVER_ERROR)
            except Exception as e_id_processing: # Any other error processing ID
                logger.error(f"[API_VIEW] Unexpected error processing barbershop.id. Type: {type(e_id_processing).__name__}, Value: {str(barbershop.id if barbershop else 'NoBarbershopObj')}, Error: {str(e_id_processing)}", exc_info=True)
                return Response({"detail": f"Erro CRITICO ao processar ID da barbearia: {type(e_id_processing).__name__} - {str(e_id_processing)}"}, 
                                status=status.HTTP_500_INTERNAL_SERVER_ERROR)
            
            logger.info(f"[API_VIEW] barbershop_id_for_response is: {barbershop_id_for_response}")

            services = Servico.objects.filter(barbearia=barbershop)
            services_data = [
                {
                    "id": str(service.id),
                    "nome": service.nome,
                    "preco": float(service.preco),
                    "duracao": service.duracao,
                    "descricao": service.descricao or ""
                }
                for service in services
            ]
            
            # Get barbers
            barbers = Barbeiro.objects.filter(barbearia=barbershop)
            barbers_data = [
                {
                    "id": str(barber.id),
                    "nome": barber.nome,
                    "status": barber.status,
                    "foto": barber.foto.url if barber.foto else None
                }
                for barber in barbers
            ]
            
            # Prepare response data
            response_data = {
                "id": barbershop_id_for_response, # Use the carefully processed ID
                "nome": barbershop.nome,
                "slug": barbershop.slug,
                "telefone": barbershop.telefone or "",
                "endereco": barbershop.endereco or "",
                "cores": barbershop.cores if isinstance(barbershop.cores, list) else [],
                "logo": barbershop.logo.url if barbershop.logo else "",
                "horario_abertura": barbershop.horario_abertura.strftime("%H:%M") if barbershop.horario_abertura else "09:00",
                "horario_fechamento": barbershop.horario_fechamento.strftime("%H:%M") if barbershop.horario_fechamento else "18:00",
                "dias_funcionamento": barbershop.dias_funcionamento if isinstance(barbershop.dias_funcionamento, list) else [],
                "esta_aberto": barbershop.esta_aberto(),
                "created_at": barbershop.created_at.isoformat() if barbershop.created_at else None,
                "updated_at": barbershop.updated_at.isoformat() if barbershop.updated_at else None,
                "servicos": services_data,
                "barbeiros": barbers_data
            }
            
            return Response(response_data, status=status.HTTP_200_OK)
            
        except Exception as e:
            logger.error(f"[API_VIEW] Outer Exception: Error getting barbershop details for slug '{slug}': {type(e).__name__} - {str(e)}", exc_info=True)
            # Check if the exception args contain the original list with the DatabaseOperations object string
            error_detail_str = str(e)
            if e.args and isinstance(e.args[0], list) and len(e.args[0]) > 0:
                error_detail_str = str(e.args[0][0]) # Try to get the inner string
            elif isinstance(e, django.core.exceptions.ValidationError) and hasattr(e, 'message_dict'):
                error_detail_str = str(e.message_dict)
            elif isinstance(e, django.core.exceptions.ValidationError) and hasattr(e, 'messages'):
                error_detail_str = "; ".join(e.messages)
            
            return Response({"detail": f"Erro ao obter detalhes da barbearia (API Outer Catch): {error_detail_str}"}, 
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
    """List all barbers for a specific barbershop"""
    permission_classes = [AllowAny]
    
    @swagger_auto_schema(
        operation_description="List all barbers for a specific barbershop",
        responses={
            200: openapi.Response(description="Success"),
            404: openapi.Response(description="Not found")
        }
    )
    def get(self, request, slug):
        """Get all barbers for a barbershop"""
        try:
            # Clean the slug - remove any @eutonafila suffix if present
            if '@' in slug:
                slug = slug.split('@')[0]
            
            # Get the barbershop by slug
            try:
                barbershop = Barbearia.objects.get(slug=slug)
            except Barbearia.DoesNotExist:
                logger.error(f"No barbershop found with slug: {slug}")
                return Response(
                    {"error": f"Barbershop with slug '{slug}' not found"},
                    status=status.HTTP_404_NOT_FOUND
                )
            
            # Get all barbers for this barbershop
            barbers = Barbeiro.objects.filter(barbearia=barbershop)
            
            # Format the response data
            data = [
                {
                    "id": str(barber.id),
                    "name": barber.nome,
                    "status": barber.status,
                    "specialty": barber.especialidade if hasattr(barber, 'especialidade') else None,
                    "image_url": barber.foto.url if barber.foto else None
                }
                for barber in barbers
            ]
            
            return Response(data)
            
        except Exception as e:
            logger.error(f"Error listing barbers: {str(e)}", exc_info=True)
            return Response(
                {"error": f"Error listing barbers: {str(e)}"},
                status=status.HTTP_500_INTERNAL_SERVER_ERROR
            )

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