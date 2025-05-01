from rest_framework import status
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework.permissions import AllowAny
from infrastructure.repositories import DjangoBarbeariaRepository, DjangoServicoRepository
from barbershop.models import Barbearia, Servico
from drf_yasg.utils import swagger_auto_schema
from drf_yasg import openapi

class BarbershopListView(APIView):
    """
    API view for listing all barbershops
    """
    permission_classes = [AllowAny]
    
    @swagger_auto_schema(
        operation_description="Get a list of all available barbershops",
        operation_summary="List all barbershops",
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
                            'slug': openapi.Schema(type=openapi.TYPE_STRING, description='Barbershop URL slug'),
                            'endereco': openapi.Schema(type=openapi.TYPE_STRING, description='Barbershop address'),
                            'telefone': openapi.Schema(type=openapi.TYPE_STRING, description='Barbershop phone number'),
                            'aberto': openapi.Schema(type=openapi.TYPE_BOOLEAN, description='Whether the barbershop is currently open'),
                        }
                    )
                )
            )
        }
    )
    def get(self, request, format=None):
        barbearia_repository = DjangoBarbeariaRepository()
        # Use the model directly since repository doesn't have list_all method
        barbearias = Barbearia.objects.all()
        
        result = []
        for barbearia in barbearias:
            # Safely check if barbershop is open
            try:
                is_open = barbearia.esta_aberto()
            except:
                is_open = False
                
            result.append({
                'id': str(barbearia.id),
                'nome': barbearia.nome,
                'slug': barbearia.slug,
                'endereco': barbearia.endereco,
                'telefone': barbearia.telefone,
                'aberto': is_open
            })
        
        return Response(result)


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
                        'slug': openapi.Schema(type=openapi.TYPE_STRING, description='Barbershop URL slug'),
                        'endereco': openapi.Schema(type=openapi.TYPE_STRING, description='Barbershop address'),
                        'telefone': openapi.Schema(type=openapi.TYPE_STRING, description='Barbershop phone number'),
                        'aberto': openapi.Schema(type=openapi.TYPE_BOOLEAN, description='Whether the barbershop is currently open'),
                        'horarios': openapi.Schema(
                            type=openapi.TYPE_ARRAY,
                            description='Barbershop operation hours',
                            items=openapi.Schema(
                                type=openapi.TYPE_OBJECT,
                                properties={
                                    'dia': openapi.Schema(type=openapi.TYPE_STRING, description='Day of the week'),
                                    'horario': openapi.Schema(type=openapi.TYPE_STRING, description='Operation hours'),
                                }
                            )
                        ),
                        'servicos': openapi.Schema(
                            type=openapi.TYPE_ARRAY,
                            description='Services offered by the barbershop',
                            items=openapi.Schema(
                                type=openapi.TYPE_OBJECT,
                                properties={
                                    'id': openapi.Schema(type=openapi.TYPE_STRING, description='Service UUID'),
                                    'nome': openapi.Schema(type=openapi.TYPE_STRING, description='Service name'),
                                    'descricao': openapi.Schema(type=openapi.TYPE_STRING, description='Service description'),
                                    'preco': openapi.Schema(type=openapi.TYPE_STRING, description='Service price'),
                                    'duracao': openapi.Schema(type=openapi.TYPE_INTEGER, description='Service duration in minutes'),
                                }
                            )
                        ),
                    }
                )
            ),
            404: openapi.Response(
                description="Barbershop not found",
                schema=openapi.Schema(
                    type=openapi.TYPE_OBJECT,
                    properties={
                        'error': openapi.Schema(type=openapi.TYPE_STRING, description='Error message'),
                    }
                )
            )
        }
    )
    def get(self, request, slug, format=None):
        try:
            # Clean the slug - remove any @eutonafila suffix if present
            clean_slug = slug.split('@')[0] if '@' in slug else slug
            
            # Get the barbershop using the repository
            barbearia_repository = DjangoBarbeariaRepository()
            barbearia = barbearia_repository.get_by_slug(clean_slug)
            
            if not barbearia:
                return Response(
                    {'error': 'Barbearia não encontrada'},
                    status=status.HTTP_404_NOT_FOUND
                )
            
            # Use the model directly
            servicos = Servico.objects.filter(barbearia=barbearia)
            servicos_data = []
            for servico in servicos:
                servicos_data.append({
                    'id': str(servico.id),
                    'nome': servico.nome,
                    'descricao': servico.descricao,
                    'preco': str(servico.preco),
                    'duracao': servico.duracao
                })
            
            # Get barbershop hours
            horarios = []
            if hasattr(barbearia, 'horario_segunda'):
                horarios = [
                    {'dia': 'Segunda', 'horario': barbearia.horario_segunda},
                    {'dia': 'Terça', 'horario': barbearia.horario_terca},
                    {'dia': 'Quarta', 'horario': barbearia.horario_quarta},
                    {'dia': 'Quinta', 'horario': barbearia.horario_quinta},
                    {'dia': 'Sexta', 'horario': barbearia.horario_sexta},
                    {'dia': 'Sábado', 'horario': barbearia.horario_sabado},
                    {'dia': 'Domingo', 'horario': barbearia.horario_domingo},
                ]
            else:
                # Simplified hours
                horarios = [
                    {'dia': 'Segunda-Domingo', 'horario': f"{barbearia.horario_abertura} - {barbearia.horario_fechamento}"}
                ]
            
            return Response({
                'id': str(barbearia.id),
                'nome': barbearia.nome,
                'slug': barbearia.slug,
                'endereco': barbearia.endereco,
                'telefone': barbearia.telefone,
                'aberto': barbearia.esta_aberto(),
                'horarios': horarios,
                'servicos': servicos_data
            })
        except Exception as e:
            return Response(
                {'error': f'Erro ao buscar barbearia: {str(e)}'},
                status=status.HTTP_500_INTERNAL_SERVER_ERROR
            ) 