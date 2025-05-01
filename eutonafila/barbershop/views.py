from django.shortcuts import render
from rest_framework import status
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework.permissions import AllowAny
from uuid import UUID
from drf_yasg.utils import swagger_auto_schema
from drf_yasg import openapi

from application.services import FilaService
from application.dtos import CheckInDTO, ClienteDTO
from infrastructure.repositories import (
    DjangoClienteRepository,
    DjangoFilaRepository,
    DjangoBarbeariaRepository,
    DjangoServicoRepository
)
from .services import notify_queue_status_change

# Create your views here.

class CheckInView(APIView):
    """
    API view for client check-in to barbershop queue
    """
    permission_classes = [AllowAny]
    
    @swagger_auto_schema(
        operation_description="Add a client to a barbershop queue",
        operation_summary="Client check-in to queue",
        request_body=openapi.Schema(
            type=openapi.TYPE_OBJECT,
            required=['nome', 'telefone', 'barbearia_slug', 'servico_id'],
            properties={
                'nome': openapi.Schema(type=openapi.TYPE_STRING, description='Client name'),
                'telefone': openapi.Schema(type=openapi.TYPE_STRING, description='Client phone number'),
                'email': openapi.Schema(type=openapi.TYPE_STRING, description='Client email address (optional)'),
                'barbearia_slug': openapi.Schema(type=openapi.TYPE_STRING, description='Barbershop URL slug'),
                'servico_id': openapi.Schema(type=openapi.TYPE_STRING, description='Service UUID'),
            }
        ),
        responses={
            201: openapi.Response(
                description="Check-in successful",
                schema=openapi.Schema(
                    type=openapi.TYPE_OBJECT,
                    properties={
                        'id': openapi.Schema(type=openapi.TYPE_STRING, description='Queue entry UUID'),
                        'cliente': openapi.Schema(type=openapi.TYPE_STRING, description='Client name'),
                        'barbearia': openapi.Schema(type=openapi.TYPE_STRING, description='Barbershop name'),
                        'servico': openapi.Schema(type=openapi.TYPE_STRING, description='Service name'),
                        'status': openapi.Schema(type=openapi.TYPE_STRING, description='Queue entry status'),
                        'data_entrada': openapi.Schema(type=openapi.TYPE_STRING, format=openapi.FORMAT_DATETIME, description='Check-in timestamp'),
                        'tempo_estimado': openapi.Schema(type=openapi.TYPE_STRING, description='Estimated wait time'),
                    }
                )
            ),
            400: openapi.Response(
                description="Bad request",
                schema=openapi.Schema(
                    type=openapi.TYPE_OBJECT,
                    properties={
                        'error': openapi.Schema(type=openapi.TYPE_STRING, description='Error message'),
                    }
                )
            )
        }
    )
    def post(self, request, format=None):
        # Validate required fields
        required_fields = ['nome', 'telefone', 'barbearia_slug', 'servico_id']
        for field in required_fields:
            if field not in request.data:
                return Response(
                    {'error': f'Campo obrigatório: {field}'},
                    status=status.HTTP_400_BAD_REQUEST
                )
        
        # Prepare DTO objects
        try:
            cliente_dto = ClienteDTO(
                nome=request.data['nome'],
                telefone=request.data['telefone'],
                email=request.data.get('email')
            )
            
            check_in_dto = CheckInDTO(
                cliente=cliente_dto,
                barbearia_slug=request.data['barbearia_slug'],
                servico_id=UUID(request.data['servico_id'])
            )
        except ValueError:
            return Response(
                {'error': 'ID de serviço inválido'},
                status=status.HTTP_400_BAD_REQUEST
            )
        
        # Initialize repositories
        cliente_repository = DjangoClienteRepository()
        fila_repository = DjangoFilaRepository()
        barbearia_repository = DjangoBarbeariaRepository()
        servico_repository = DjangoServicoRepository()
        
        # Create service with repositories
        fila_service = FilaService(
            cliente_repository=cliente_repository,
            fila_repository=fila_repository,
            barbearia_repository=barbearia_repository,
            servico_repository=servico_repository
        )
        
        # Execute check-in
        entrada_fila, result = fila_service.check_in(check_in_dto)
        
        if not entrada_fila:
            # Return error message
            return Response(
                {'error': result},
                status=status.HTTP_400_BAD_REQUEST
            )
        
        # Return success with queue entry data
        return Response({
            'id': str(entrada_fila.id),
            'cliente': entrada_fila.cliente.nome,
            'barbearia': entrada_fila.barbearia.nome,
            'servico': entrada_fila.servico.nome,
            'status': entrada_fila.get_status_display(),
            'data_entrada': entrada_fila.created_at,
            'tempo_estimado': result
        }, status=status.HTTP_201_CREATED)


class QueueStatusView(APIView):
    """
    API view for checking queue status for a client
    """
    permission_classes = [AllowAny]
    
    @swagger_auto_schema(
        operation_description="Get the current status of a client in the queue",
        operation_summary="Check queue status",
        manual_parameters=[
            openapi.Parameter(
                name='queue_id',
                in_=openapi.IN_PATH,
                type=openapi.TYPE_STRING,
                description='Queue entry UUID',
                required=True
            ),
        ],
        responses={
            200: openapi.Response(
                description="Queue status",
                schema=openapi.Schema(
                    type=openapi.TYPE_OBJECT,
                    properties={
                        'id': openapi.Schema(type=openapi.TYPE_STRING, description='Queue entry UUID'),
                        'cliente': openapi.Schema(type=openapi.TYPE_STRING, description='Client name'),
                        'barbearia': openapi.Schema(type=openapi.TYPE_STRING, description='Barbershop name'),
                        'servico': openapi.Schema(type=openapi.TYPE_STRING, description='Service name'),
                        'status': openapi.Schema(type=openapi.TYPE_STRING, description='Queue entry status'),
                        'posicao': openapi.Schema(type=openapi.TYPE_INTEGER, description='Position in queue'),
                        'tempo_estimado': openapi.Schema(type=openapi.TYPE_STRING, description='Estimated wait time'),
                        'data_entrada': openapi.Schema(type=openapi.TYPE_STRING, format=openapi.FORMAT_DATETIME, description='Check-in timestamp'),
                    }
                )
            ),
            404: openapi.Response(
                description="Queue entry not found",
                schema=openapi.Schema(
                    type=openapi.TYPE_OBJECT,
                    properties={
                        'error': openapi.Schema(type=openapi.TYPE_STRING, description='Error message'),
                    }
                )
            ),
            400: openapi.Response(
                description="Invalid UUID",
                schema=openapi.Schema(
                    type=openapi.TYPE_OBJECT,
                    properties={
                        'error': openapi.Schema(type=openapi.TYPE_STRING, description='Error message'),
                    }
                )
            )
        }
    )
    def get(self, request, queue_id, format=None):
        # Initialize repositories
        fila_repository = DjangoFilaRepository()
        
        try:
            entrada_id = UUID(str(queue_id))
            entrada = fila_repository.get_by_id(entrada_id)
            
            if not entrada:
                return Response(
                    {'error': 'Entrada na fila não encontrada'},
                    status=status.HTTP_404_NOT_FOUND
                )
            
            posicao = fila_repository.posicao_na_fila(entrada_id)
            
            # Format estimated time based on position and service duration
            if posicao <= 0:
                tempo_texto = "Você será atendido em seguida"
            else:
                minutos = posicao * entrada.servico.duracao
                if minutos < 60:
                    tempo_texto = f"{minutos} minutos"
                else:
                    horas = minutos // 60
                    min_restantes = minutos % 60
                    tempo_texto = f"{horas}h e {min_restantes}min"
            
            return Response({
                'id': str(entrada.id),
                'cliente': entrada.cliente.nome,
                'barbearia': entrada.barbearia.nome,
                'servico': entrada.servico.nome,
                'status': entrada.get_status_display(),
                'posicao': posicao,
                'tempo_estimado': tempo_texto,
                'data_entrada': entrada.created_at
            })
            
        except (ValueError, TypeError):
            return Response(
                {'error': 'ID inválido'},
                status=status.HTTP_400_BAD_REQUEST
            )


class CancelQueueEntryView(APIView):
    """
    API view for canceling a queue entry
    """
    permission_classes = [AllowAny]
    
    @swagger_auto_schema(
        operation_description="Cancel a client's queue entry",
        operation_summary="Cancel queue entry",
        manual_parameters=[
            openapi.Parameter(
                name='queue_id',
                in_=openapi.IN_PATH,
                type=openapi.TYPE_STRING,
                description='Queue entry UUID',
                required=True
            ),
        ],
        responses={
            200: openapi.Response(
                description="Queue entry canceled",
                schema=openapi.Schema(
                    type=openapi.TYPE_OBJECT,
                    properties={
                        'message': openapi.Schema(type=openapi.TYPE_STRING, description='Success message'),
                        'id': openapi.Schema(type=openapi.TYPE_STRING, description='Queue entry UUID'),
                        'status': openapi.Schema(type=openapi.TYPE_STRING, description='Queue entry status'),
                    }
                )
            ),
            404: openapi.Response(
                description="Queue entry not found",
                schema=openapi.Schema(
                    type=openapi.TYPE_OBJECT,
                    properties={
                        'error': openapi.Schema(type=openapi.TYPE_STRING, description='Error message'),
                    }
                )
            ),
            400: openapi.Response(
                description="Bad request",
                schema=openapi.Schema(
                    type=openapi.TYPE_OBJECT,
                    properties={
                        'error': openapi.Schema(type=openapi.TYPE_STRING, description='Error message'),
                    }
                )
            )
        }
    )
    def post(self, request, queue_id, format=None):
        try:
            # Get queue entry
            fila_repository = DjangoFilaRepository()
            entrada_id = UUID(str(queue_id))
            entrada = fila_repository.get_by_id(entrada_id)
            
            if not entrada:
                return Response(
                    {'error': 'Entrada na fila não encontrada'},
                    status=status.HTTP_404_NOT_FOUND
                )
            
            # Check if entry can be canceled
            if entrada.status != 'aguardando':
                return Response(
                    {'error': 'Não é possível cancelar um atendimento que não está em aguardo'},
                    status=status.HTTP_400_BAD_REQUEST
                )
            
            # Cancel entry
            success = entrada.cancelar_atendimento()
            
            if not success:
                return Response(
                    {'error': 'Não foi possível cancelar o atendimento'},
                    status=status.HTTP_400_BAD_REQUEST
                )
            
            # Manually trigger WebSocket notification
            notify_queue_status_change(queue_id, 'cancelado')
            
            return Response({
                'message': 'Atendimento cancelado com sucesso',
                'id': str(entrada.id),
                'status': entrada.status
            })
            
        except (ValueError, TypeError):
            return Response(
                {'error': 'ID inválido'},
                status=status.HTTP_400_BAD_REQUEST
            )
