import os
import sqlite3
import uuid
import json
from datetime import datetime

def truncate_barbearias(conn, cursor):
    """Truncate the Barbearias table by deleting all records"""
    try:
        # Get count before truncating
        cursor.execute("SELECT COUNT(*) FROM barbershop_barbearia")
        count_before = cursor.fetchone()[0]
        print(f"Found {count_before} barbershop records before truncating")
        
        # Delete all records
        cursor.execute("DELETE FROM barbershop_barbearia")
        conn.commit()
        
        # Get count after truncating
        cursor.execute("SELECT COUNT(*) FROM barbershop_barbearia")
        count_after = cursor.fetchone()[0]
        print(f"Deleted all barbershops. Remaining count: {count_after}")
        
        return True
    except Exception as e:
        print(f"Error truncating barbearias: {str(e)}")
        conn.rollback()
        return False

def create_sample_barbearias(conn, cursor):
    """Create a few barbershops with clean data and good UUIDs"""
    # Current timestamp in ISO format
    now = datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f')
    
    # Sample barbershops data with predefined UUIDs
    barbershops_data = [
        {
            'id': 'a1b2c3d4-e5f6-47a8-b9c0-d1e2f3a4b5c6',
            'nome': 'Barbearia Vintage',
            'slug': 'barbearia-vintage',
            'endereco': 'Rua Augusta, 1234, São Paulo',
            'telefone': '(11) 3456-7890',
            'descricao_curta': 'Barbearia tradicional com toque moderno',
            'cores': ['#2A363B', '#E84A5F'],
            'horario_abertura': '09:00:00',
            'horario_fechamento': '19:00:00',
            'dias_funcionamento': [0, 1, 2, 3, 4, 5],  # Monday to Saturday
            'max_capacity': 8,
            'enable_priority_queue': True,
        },
        {
            'id': 'b2c3d4e5-f6a7-48b9-c0d1-e2f3a4b5c6d7',
            'nome': 'Corte & Estilo',
            'slug': 'corte-estilo',
            'endereco': 'Av. Paulista, 1000, São Paulo',
            'telefone': '(11) 2345-6789',
            'descricao_curta': 'Seu estilo, nossa especialidade',
            'cores': ['#45B29D', '#DF5A49'],
            'horario_abertura': '10:00:00',
            'horario_fechamento': '20:00:00',
            'dias_funcionamento': [0, 1, 2, 3, 4, 5, 6],  # Monday to Sunday
            'max_capacity': 12,
            'enable_priority_queue': True,
        },
        {
            'id': 'c3d4e5f6-a7b8-49c0-d1e2-f3a4b5c6d7e8',
            'nome': 'Barba & Cabelo',
            'slug': 'barba-cabelo',
            'endereco': 'Rua Oscar Freire, 500, São Paulo',
            'telefone': '(11) 9876-5432',
            'descricao_curta': 'Especialistas em barba e cabelo masculino',
            'cores': ['#334D5C', '#EFC94C'],
            'horario_abertura': '08:30:00',
            'horario_fechamento': '19:30:00',
            'dias_funcionamento': [1, 2, 3, 4, 5],  # Tuesday to Saturday
            'max_capacity': 6,
            'enable_priority_queue': False,
        }
    ]

    print("\nCreating sample barbershops...")
    for data in barbershops_data:
        # Prepare JSON fields
        cores_json = json.dumps(data['cores'])
        dias_funcionamento_json = json.dumps(data['dias_funcionamento'])
        
        # Find if there's an existing barbershop with this slug to avoid duplicates
        cursor.execute("SELECT id FROM barbershop_barbearia WHERE slug = ?", (data['slug'],))
        existing = cursor.fetchone()
        
        if existing:
            print(f"A barbershop with slug '{data['slug']}' already exists. Skipping...")
            continue
        
        try:
            # Insert the barbershop
            sql = """
            INSERT INTO barbershop_barbearia (
                id, nome, slug, telefone, endereco, descricao_curta,
                cores, logo, horario_abertura, horario_fechamento,
                dias_funcionamento, max_capacity, enable_priority_queue,
                created_at, updated_at, user_id
            ) VALUES (
                ?, ?, ?, ?, ?, ?,
                ?, ?, ?, ?,
                ?, ?, ?,
                ?, ?, NULL
            )
            """
            
            cursor.execute(sql, (
                data['id'],
                data['nome'],
                data['slug'],
                data['telefone'],
                data['endereco'],
                data['descricao_curta'],
                cores_json,
                None,
                data['horario_abertura'],
                data['horario_fechamento'],
                dias_funcionamento_json,
                data['max_capacity'],
                1 if data['enable_priority_queue'] else 0,
                now,
                now
            ))
            conn.commit()
            
            print(f"Created barbershop: {data['nome']}")
            print(f"  ID: {data['id']}")
            print(f"  Slug: {data['slug']}")
            
        except Exception as e:
            print(f"Error creating barbershop {data['nome']}: {str(e)}")
            conn.rollback()

def main():
    """Main function to truncate and recreate barbershops"""
    # Try multiple possible database paths
    possible_db_paths = [
        os.path.join(os.path.dirname(os.path.abspath(__file__)), 'db.sqlite3'),
        os.path.join(os.path.dirname(os.path.abspath(__file__)), 'eutonafila', 'db.sqlite3')
    ]
    
    db_path = None
    for path in possible_db_paths:
        if os.path.exists(path) and os.path.getsize(path) > 0:
            db_path = path
            print(f"Found valid database at: {db_path}")
            break
    
    if not db_path:
        print(f"Valid database file not found in known locations")
        # Try to search for it
        for root, dirs, files in os.walk(os.path.dirname(os.path.abspath(__file__))):
            if 'db.sqlite3' in files:
                path = os.path.join(root, 'db.sqlite3')
                if os.path.getsize(path) > 0:
                    db_path = path
                    print(f"Found database at: {db_path}")
                    break
    
    if not os.path.exists(db_path):
        print("Could not find database file")
        return
    
    print(f"Using database: {db_path}")
    
    try:
        # Connect to SQLite database
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        
        # Check if the table exists
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='barbershop_barbearia'")
        if not cursor.fetchone():
            print("Table 'barbershop_barbearia' does not exist")
            return
        
        # Execute the truncation and creation operations
        if truncate_barbearias(conn, cursor):
            create_sample_barbearias(conn, cursor)
        
        # Get count after all operations
        cursor.execute("SELECT COUNT(*) FROM barbershop_barbearia")
        count_final = cursor.fetchone()[0]
        print(f"\nFinal barbershop count: {count_final}")
        
        # Show the created barbershops
        cursor.execute("SELECT id, nome, slug FROM barbershop_barbearia")
        print("\nBarbershops in database:")
        for row in cursor.fetchall():
            print(f"  - {row[1]} (ID: {row[0]}, Slug: {row[2]})")
        
    except Exception as e:
        print(f"Error: {str(e)}")
        import traceback
        traceback.print_exc()
    finally:
        if 'conn' in locals():
            conn.close()

if __name__ == '__main__':
    main()
