import os
import django

# Set up Django environment
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
django.setup()

# Import models after Django setup
from barbershop.models import Barbearia, Barbeiro, Servico, Cliente, Fila
from django.contrib.auth.models import User
from django.db import connection

def check_tables():
    """Check if tables exist and create test data if needed"""
    # Check tables
    with connection.cursor() as cursor:
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table';")
        tables = [table[0] for table in cursor.fetchall()]
        print(f"Database tables: {tables}")
    
    # Check existing data
    print("\nChecking existing data:")
    print(f"Users: {User.objects.count()}")
    print(f"Barbearias: {Barbearia.objects.count()}")
    print(f"Barbeiros: {Barbeiro.objects.count()}")
    print(f"Serviços: {Servico.objects.count()}")
    print(f"Clientes: {Cliente.objects.count()}")
    print(f"Filas: {Fila.objects.count()}")
    
    # Create test data if needed
    if Barbearia.objects.count() == 0:
        print("\nCreating test barbershop...")
        
        # Create test user if needed
        if User.objects.filter(username="admin").count() == 0:
            user = User.objects.create_user(
                username="admin",
                email="admin@example.com",
                password="admin123"
            )
            print("Created admin user")
        else:
            user = User.objects.get(username="admin")
        
        # Create barbearia
        barbearia = Barbearia(
            nome="Barbearia do Mineiro",
            slug="barbearia-do-mineiro",
            telefone="(11) 99999-9999",
            endereco="Rua das Barbas, 123",
            descricao_curta="Barbearia especializada em cuidados masculinos. Venha nos conhecer!",
            cores=["#FF3C00", "#151515"],
            horario_abertura="09:00",
            horario_fechamento="18:00",
            dias_funcionamento=[0, 1, 2, 3, 4, 5],
            user=user
        )
        barbearia.save()
        print(f"Created barbearia: {barbearia.nome}")
        
        # Create some services
        services = [
            {"nome": "Corte de Cabelo", "preco": 35.00, "duracao": 30},
            {"nome": "Barba", "preco": 25.00, "duracao": 20},
            {"nome": "Corte + Barba", "preco": 55.00, "duracao": 45}
        ]
        
        for service in services:
            Servico.objects.create(
                barbearia=barbearia,
                nome=service["nome"],
                descricao=f"Serviço de {service['nome'].lower()}",
                preco=service["preco"],
                duracao=service["duracao"]
            )
        print(f"Created {len(services)} services")
        
        # Create a barber
        barbeiro = Barbeiro.objects.create(
            usuario=user,
            barbearia=barbearia,
            nome="João Mineiro",
            telefone="(11) 88888-8888",
            status="available"
        )
        print(f"Created barber: {barbeiro.nome}")
    
    # Print barbearia details
    if Barbearia.objects.count() > 0:
        print("\nBarbearia details:")
        for b in Barbearia.objects.all():
            print(f"ID: {b.id}")
            print(f"Nome: {b.nome}")
            print(f"Slug: {b.slug}")
            print(f"Descrição curta: '{b.descricao_curta}'")
            print(f"Cores: {b.cores}")
            print("-" * 40)

if __name__ == "__main__":
    print("Checking database and models...")
    check_tables()
    print("Check complete.") 