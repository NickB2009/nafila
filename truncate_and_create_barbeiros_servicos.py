"""
Script to truncate and recreate barbeiros and servicos with clean test data
"""
import os
import sqlite3
import uuid
import json
from datetime import datetime, time

# Setup logging to file
LOG_FILE = "barbeiros_servicos_log.txt"

def log(message):
    """Log a message to both console and log file"""
    print(message)
    with open(LOG_FILE, 'a', encoding='utf-8') as f: # Added encoding='utf-8'
        f.write(f"{message}\\n")

def find_database():
    """Find the SQLite database file"""
    possible_paths = [
        r"C:\\git\\eutonafila\\nafila\\eutonafila\\db.sqlite3",
        r"C:\\git\\eutonafila\\nafila\\db.sqlite3"
    ]
    
    for path in possible_paths:
        if os.path.exists(path):
            log(f"Found valid database at: {path}") # Changed print to log
            return path
    
    log("Could not find database file") # Changed print to log
    return None

def truncate_barbeiros(conn, cursor):
    """Truncate the Barbeiros table by deleting all records"""
    try:
        # Get count before truncating
        cursor.execute("SELECT COUNT(*) FROM barbershop_barbeiro")
        count_before = cursor.fetchone()[0]
        log(f"Found {count_before} barber records before truncating") # Changed print to log
        
        # Delete all records
        cursor.execute("DELETE FROM barbershop_barbeiro")
        conn.commit()
        
        # Get count after truncating
        cursor.execute("SELECT COUNT(*) FROM barbershop_barbeiro")
        count_after = cursor.fetchone()[0]
        log(f"Deleted all barbers. Remaining count: {count_after}") # Changed print to log
        
        return True
    except Exception as e:
        log(f"Error truncating barbers: {str(e)}") # Changed print to log
        conn.rollback()
        return False

def truncate_servicos(conn, cursor):
    """Truncate the Servicos table by deleting all records"""
    try:
        # Get count before truncating
        cursor.execute("SELECT COUNT(*) FROM barbershop_servico")
        count_before = cursor.fetchone()[0]
        log(f"Found {count_before} service records before truncating") # Changed print to log
        
        # Delete all records
        cursor.execute("DELETE FROM barbershop_servico")
        conn.commit()
        
        # Get count after truncating
        cursor.execute("SELECT COUNT(*) FROM barbershop_servico")
        count_after = cursor.fetchone()[0]
        log(f"Deleted all services. Remaining count: {count_after}") # Changed print to log
        
        return True
    except Exception as e:
        log(f"Error truncating services: {str(e)}") # Changed print to log
        conn.rollback()
        return False

def truncate_especialidades(conn, cursor):
    """Truncate the barbershop_barbeiro_especialidades table"""
    try:
        cursor.execute("SELECT COUNT(*) FROM barbershop_barbeiro_especialidades")
        count_before = cursor.fetchone()[0]
        log(f"Found {count_before} barber specialty records before truncating")
        
        cursor.execute("DELETE FROM barbershop_barbeiro_especialidades")
        conn.commit()
        
        cursor.execute("SELECT COUNT(*) FROM barbershop_barbeiro_especialidades")
        count_after = cursor.fetchone()[0]
        log(f"Deleted all barber specialties. Remaining count: {count_after}")
        return True
    except Exception as e:
        log(f"Error truncating barber specialties: {str(e)}")
        conn.rollback()
        return False

def create_sample_barbeiros(conn, cursor):
    """Create sample barbers with clean data and good UUIDs"""
    try:
        log("\\nCreating sample barbers...") # Changed print to log
        
        # Get barbearia IDs from the database
        cursor.execute("SELECT id, nome FROM barbershop_barbearia")
        barbearias = cursor.fetchall()
        
        if not barbearias:
            log("No barbershops found in the database. Please create barbershops first.") # Changed print to log
            return False
            
        # Define barber templates for each barbearia
        barber_templates = []
        
        for barbearia_id, barbearia_nome in barbearias:
            if "Vintage" in barbearia_nome:
                barber_templates.extend([
                    {
                        'id': str(uuid.uuid4()),
                        'nome': 'João Silva',
                        'telefone': '(11) 99123-4567',
                        'status': 'available',
                        'barbearia_id': barbearia_id,
                    },
                    {
                        'id': str(uuid.uuid4()),
                        'nome': 'Carlos Oliveira',
                        'telefone': '(11) 98765-4321',
                        'status': 'busy',
                        'barbearia_id': barbearia_id,
                    }
                ])
            elif "Corte & Estilo" in barbearia_nome:
                barber_templates.extend([
                    {
                        'id': str(uuid.uuid4()),
                        'nome': 'Miguel Santos',
                        'telefone': '(11) 97777-8888',
                        'status': 'available',
                        'barbearia_id': barbearia_id,
                    }
                ])
            elif "Barba & Cabelo" in barbearia_nome:
                barber_templates.extend([
                    {
                        'id': str(uuid.uuid4()),
                        'nome': 'Pedro Almeida',
                        'telefone': '(11) 96666-5555',
                        'status': 'available',
                        'barbearia_id': barbearia_id,
                    },
                    {
                        'id': str(uuid.uuid4()),
                        'nome': 'Lucas Costa',
                        'telefone': '(11) 95555-4444',
                        'status': 'break',
                        'barbearia_id': barbearia_id,
                    },
                    {
                        'id': str(uuid.uuid4()),
                        'nome': 'Rafael Martins',
                        'telefone': '(11) 94444-3333',
                        'status': 'offline',
                        'barbearia_id': barbearia_id,
                    }
                ])
        
        # Insert the barbers, creating a unique user for each
        for barber_template in barber_templates:
            # Create a unique user for this barber
            unique_username = f"user_{barber_template['nome'].replace(' ', '_').lower().replace('ã', 'a').replace('ç', 'c')}_{uuid.uuid4().hex[:6]}"
            email = f"{unique_username}@example.com"
            first_name_parts = barber_template['nome'].split(' ')
            first_name = first_name_parts[0]
            last_name = ' '.join(first_name_parts[1:]) if len(first_name_parts) > 1 else 'User'

            try:
                cursor.execute("""
                    INSERT INTO auth_user (
                        username, password, is_superuser, first_name, last_name, email, is_staff, is_active, date_joined
                    ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
                """, [
                    unique_username, 
                    "pbkdf2_sha256$260000$testpassword$abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", # Example hash, replace if needed
                    0, 
                    first_name, 
                    last_name, 
                    email, 
                    0, 
                    1, 
                    datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                ])
                new_user_id = cursor.lastrowid
                log(f"Created user '{unique_username}' with ID {new_user_id} for barber '{barber_template['nome']}'")
            except sqlite3.IntegrityError as e:
                log(f"Error creating user {unique_username}: {e}. This barber will be skipped.")
                conn.rollback() # Rollback user creation
                continue # Skip this barber


            cursor.execute("""
                INSERT INTO barbershop_barbeiro (
                    id, nome, telefone, status, barbearia_id, usuario_id,
                    created_at, updated_at, foto
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
            """, [
                barber_template['id'],
                barber_template['nome'],
                barber_template['telefone'],
                barber_template['status'],
                barber_template['barbearia_id'],
                new_user_id, # Use the new unique user_id
                datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
                datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
                None
            ])
            log(f"Created barber: {barber_template['nome']}") # Changed print to log
            log(f"  ID: {barber_template['id']}") # Changed print to log
            log(f"  Status: {barber_template['status']}") # Changed print to log
        
        conn.commit()
        return True
    except Exception as e:
        log(f"Error creating barbers: {str(e)}") # Changed print to log
        conn.rollback()
        return False

def create_sample_servicos(conn, cursor):
    """Create sample services with clean data and good UUIDs"""
    try:
        log("\\nCreating sample services...") # Changed print to log
        
        # Get barbearia IDs from the database
        cursor.execute("SELECT id, nome FROM barbershop_barbearia")
        barbearias = cursor.fetchall()
        
        if not barbearias:
            log("No barbershops found in the database. Please create barbershops first.") # Changed print to log
            return False
        
        # Define services for each barbearia
        services_data = []
        
        for barbearia_id, barbearia_nome in barbearias:
            # Common services for all barbershops
            services_data.extend([
                {
                    'id': str(uuid.uuid4()),
                    'nome': 'Corte Simples',
                    'descricao': 'Corte básico de cabelo para todos os estilos',
                    'preco': 35.00,
                    'duracao': 30,
                    'complexity': 2,  # Medium complexity
                    'popularity': 5,
                    'barbearia_id': barbearia_id
                },
                {
                    'id': str(uuid.uuid4()),
                    'nome': 'Barba Completa',
                    'descricao': 'Aparo e modelagem completa da barba',
                    'preco': 25.00,
                    'duracao': 20,
                    'complexity': 2,  # Medium complexity
                    'popularity': 4,
                    'barbearia_id': barbearia_id
                },
                {
                    'id': str(uuid.uuid4()),
                    'nome': 'Combo Corte e Barba',
                    'descricao': 'Corte de cabelo e tratamento completo da barba',
                    'preco': 55.00,
                    'duracao': 50,
                    'complexity': 3,  # High complexity
                    'popularity': 5,
                    'barbearia_id': barbearia_id
                }
            ])
            
            # Specialized services for each barbershop
            if "Vintage" in barbearia_nome:
                services_data.extend([
                    {
                        'id': str(uuid.uuid4()),
                        'nome': 'Corte Vintage',
                        'descricao': 'Corte retrô inspirado nos anos 50',
                        'preco': 45.00,
                        'duracao': 40,
                        'complexity': 3,  # High complexity
                        'popularity': 3,
                        'barbearia_id': barbearia_id
                    },
                    {
                        'id': str(uuid.uuid4()),
                        'nome': 'Hot Towel Shave',
                        'descricao': 'Barbear tradicional com toalha quente',
                        'preco': 35.00,
                        'duracao': 30,
                        'complexity': 3,  # High complexity
                        'popularity': 4,
                        'barbearia_id': barbearia_id
                    }
                ])
            elif "Corte & Estilo" in barbearia_nome:
                services_data.extend([
                    {
                        'id': str(uuid.uuid4()),
                        'nome': 'Corte Moderno',
                        'descricao': 'Corte com técnicas modernas e tendências atuais',
                        'preco': 50.00,
                        'duracao': 35,
                        'complexity': 3,  # High complexity
                        'popularity': 5,
                        'barbearia_id': barbearia_id
                    },
                    {
                        'id': str(uuid.uuid4()),
                        'nome': 'Desenho na Cabeça',
                        'descricao': 'Desenhos e detalhes personalizados no cabelo',
                        'preco': 30.00,
                        'duracao': 25,
                        'complexity': 4,  # Very high complexity
                        'popularity': 4,
                        'barbearia_id': barbearia_id
                    }
                ])
            elif "Barba & Cabelo" in barbearia_nome:
                services_data.extend([
                    {
                        'id': str(uuid.uuid4()),
                        'nome': 'Tratamento de Barba Premium',
                        'descricao': 'Tratamento completo da barba com produtos premium',
                        'preco': 40.00,
                        'duracao': 30,
                        'complexity': 3,  # High complexity
                        'popularity': 5,
                        'barbearia_id': barbearia_id
                    },
                    {
                        'id': str(uuid.uuid4()),
                        'nome': 'Coloração de Barba',
                        'descricao': 'Coloração profissional para barba',
                        'preco': 45.00,
                        'duracao': 40,
                        'complexity': 3,  # High complexity
                        'popularity': 3,
                        'barbearia_id': barbearia_id
                    },
                    {
                        'id': str(uuid.uuid4()),
                        'nome': 'Hidratação Capilar',
                        'descricao': 'Tratamento de hidratação profunda para cabelos',
                        'preco': 35.00,
                        'duracao': 30,
                        'complexity': 2,  # Medium complexity
                        'popularity': 4,
                        'barbearia_id': barbearia_id
                    }
                ])
        
        # Insert the services
        for service in services_data:
            cursor.execute("""
                INSERT INTO barbershop_servico (
                    id, nome, descricao, preco, duracao, complexity, popularity,
                    barbearia_id, created_at, updated_at, imagem
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            """, [
                service['id'],
                service['nome'],
                service['descricao'],
                service['preco'],
                service['duracao'],
                service['complexity'],
                service['popularity'],
                service['barbearia_id'],
                datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
                datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
                None
            ])
            log(f"Created service: {service['nome']}") # Changed print to log
            log(f"  ID: {service['id']}") # Changed print to log
            log(f"  Price: R$ {service['preco']:.2f}") # Changed print to log
            log(f"  Duration: {service['duracao']} minutes") # Changed print to log
        
        conn.commit()
        return True
    except Exception as e:
        log(f"Error creating services: {str(e)}") # Changed print to log
        conn.rollback()
        return False

def verify_data(conn, cursor):
    """Verify the data was created correctly"""
    try:
        log("\\nVerifying data...") # Changed print to log
        
        # Check barbers
        cursor.execute("""
            SELECT b.nome AS barber_name, bb.nome AS barbershop_name
            FROM barbershop_barbeiro b
            JOIN barbershop_barbearia bb ON b.barbearia_id = bb.id
            ORDER BY bb.nome, b.nome
        """)
        barbers = cursor.fetchall()
        
        log(f"\\nBarbers in database ({len(barbers)}):") # Changed print to log
        current_barbershop = None
        for barber_name, barbershop_name in barbers:
            if barbershop_name != current_barbershop:
                log(f"\\n  Barbershop: {barbershop_name}") # Changed print to log
                current_barbershop = barbershop_name
            log(f"    - {barber_name}") # Changed print to log
        
        # Check services
        cursor.execute("""
            SELECT s.nome AS service_name, s.preco, s.duracao, bb.nome AS barbershop_name
            FROM barbershop_servico s
            JOIN barbershop_barbearia bb ON s.barbearia_id = bb.id
            ORDER BY bb.nome, s.nome
        """)
        services = cursor.fetchall()
        
        log(f"\\nServices in database ({len(services)}):") # Changed print to log
        current_barbershop = None
        for service_name, price, duration, barbershop_name in services:
            if barbershop_name != current_barbershop:
                log(f"\\n  Barbershop: {barbershop_name}") # Changed print to log
                current_barbershop = barbershop_name
            log(f"    - {service_name} (R$ {price:.2f}, {duration} min)") # Changed print to log
        
        return True
    except Exception as e:
        log(f"Error verifying data: {str(e)}") # Changed print to log
        return False

def create_especialidades_relationships(conn, cursor):
    """Create relationships between barbers and their specialties"""
    try:
        log("\\nCreating barber specialties relationships...") # Changed print to log
        
        # First, get all barbers
        cursor.execute("SELECT id, nome, barbearia_id FROM barbershop_barbeiro")
        barbers = cursor.fetchall()
        
        for barber_id, barber_name, barbearia_id in barbers:
            # Get all services for this barbershop
            cursor.execute("SELECT id, nome FROM barbershop_servico WHERE barbearia_id = ?", [barbearia_id])
            services = cursor.fetchall()
            
            # Assign 2-3 random specialties to each barber
            import random
            # Make sure we have at least 3 services
            num_specialties = min(len(services), random.randint(2, 3))
            specialties = random.sample(services, num_specialties)
            
            for service_id, service_name in specialties:
                cursor.execute("""
                    INSERT INTO barbershop_barbeiro_especialidades (
                        barbeiro_id, servico_id
                    ) VALUES (?, ?)
                """, [barber_id, service_id])
                log(f"Assigned specialty '{service_name}' to barber '{barber_name}'") # Changed print to log
        
        conn.commit()
        return True
    except Exception as e:
        log(f"Error creating specialties relationships: {str(e)}") # Changed print to log
        conn.rollback()
        return False

def main():
    # Clear the log file at the beginning of the run
    if os.path.exists(LOG_FILE):
        os.remove(LOG_FILE)
    log(f"Log file {LOG_FILE} cleared for new run.")

    # Find the database
    db_path = find_database()
    if not db_path:
        log("Error: Database not found") # Changed print to log
        return
    
    log(f"Using database: {db_path}") # Changed print to log
    
    # Connect to the database
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    
    try:
        # Truncate tables
        truncate_barbeiros(conn, cursor)
        truncate_servicos(conn, cursor)
        truncate_especialidades(conn, cursor) # Added call to truncate specialties
        
        # Create new data
        create_sample_servicos(conn, cursor)
        create_sample_barbeiros(conn, cursor)
        
        # Create relationships
        create_especialidades_relationships(conn, cursor)
        
        # Verify data
        verify_data(conn, cursor)
        
    except Exception as e:
        log(f"Error: {str(e)}") # Changed print to log
    finally:
        conn.close()

if __name__ == "__main__":
    main()
