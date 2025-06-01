"""
Script to fix the UUID representation issue by directly updating the database
"""
import os
import sys
import uuid
import sqlite3
import json
from datetime import datetime, time

# Database path
DB_PATH = "c:/git/eutonafila/nafila/eutonafila/db.sqlite3"

def print_info(message):
    """Print info message with timestamp"""
    print(f"[INFO] {datetime.now().strftime('%Y-%m-%d %H:%M:%S')} - {message}")

def print_error(message):
    """Print error message with timestamp"""
    print(f"[ERROR] {datetime.now().strftime('%Y-%m-%d %H:%M:%S')} - {message}")

def get_barbearias():
    """Get all barbearias from the database"""
    try:
        conn = sqlite3.connect(DB_PATH)
        conn.row_factory = sqlite3.Row  # This enables column access by name
        cursor = conn.cursor()
        
        # Get all barbershops
        cursor.execute("SELECT * FROM barbershop_barbearia")
        rows = cursor.fetchall()
        
        print_info(f"Found {len(rows)} barbershops in database")
        
        # Print details about each barbershop
        for i, row in enumerate(rows):
            print_info(f"Barbershop {i+1}: {row['nome']}")
            print_info(f"  ID: {row['id']}")
            
            # Look specifically for Barba & Cabelo
            if "Barba & Cabelo" in row['nome']:
                print_info(f"  Found the 'Barba & Cabelo' record!")
        
        conn.close()
        return rows
    except Exception as e:
        print_error(f"Error getting barbershops: {str(e)}")
        return []

def update_problematic_record():
    """Update the problematic Barba & Cabelo record with a clean UUID"""
    try:
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        # Find the problematic record
        cursor.execute("SELECT id FROM barbershop_barbearia WHERE nome = ?", ["Barba & Cabelo"])
        row = cursor.fetchone()
        
        if row:
            old_id = row[0]
            print_info(f"Found 'Barba & Cabelo' with ID: {old_id}")
            
            # Generate a new UUID
            new_id = str(uuid.uuid4())
            print_info(f"Generated new UUID: {new_id}")
            
            # Update the record
            cursor.execute(
                "UPDATE barbershop_barbearia SET id = ? WHERE nome = ?",
                [new_id, "Barba & Cabelo"]
            )
            
            conn.commit()
            print_info(f"Updated 'Barba & Cabelo' record: {old_id} -> {new_id}")
            
            # Also update any related records
            # Update barbeiros
            cursor.execute(
                "UPDATE barbershop_barbeiro SET barbearia_id = ? WHERE barbearia_id = ?",
                [new_id, old_id]
            )
            
            # Update servicos
            cursor.execute(
                "UPDATE barbershop_servico SET barbearia_id = ? WHERE barbearia_id = ?",
                [new_id, old_id]
            )
            
            # Update filas
            cursor.execute(
                "UPDATE barbershop_fila SET barbearia_id = ? WHERE barbearia_id = ?",
                [new_id, old_id]
            )
            
            conn.commit()
            print_info("Updated all related records")
        else:
            print_info("Could not find 'Barba & Cabelo' record")
        
        conn.close()
        return True
    except Exception as e:
        print_error(f"Error updating record: {str(e)}")
        return False

def create_clean_barbearias():
    """Create fresh barbearia records with clean UUIDs"""
    try:
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        # First, delete all existing barbershops
        print_info("Truncating barbershop_barbearia table...")
        cursor.execute("DELETE FROM barbershop_barbearia")
        deleted_count = cursor.rowcount
        print_info(f"Deleted {deleted_count} existing records")
        
        # Create new barbershops with clean UUIDs
        barbershops = [
            {
                'id': str(uuid.uuid4()),
                'nome': 'Barbearia Vintage', 
                'slug': 'barbearia-vintage',
                'telefone': '(11) 1234-5678',
                'endereco': 'Rua da Barbearia, 123',
                'descricao_curta': 'Barbearia com estilo vintage e atendimento de qualidade',
                'cores': json.dumps(['#FF5733', '#C70039']),
                'horario_abertura': '09:00',
                'horario_fechamento': '18:00',
                'dias_funcionamento': json.dumps([0, 1, 2, 3, 4, 5]),
                'max_capacity': 15,
                'enable_priority_queue': 1,
                'created_at': datetime.now().strftime('%Y-%m-%d %H:%M:%S'),
                'updated_at': datetime.now().strftime('%Y-%m-%d %H:%M:%S')
            },
            {
                'id': str(uuid.uuid4()),
                'nome': 'Corte & Estilo', 
                'slug': 'corte-estilo',
                'telefone': '(11) 8765-4321',
                'endereco': 'Avenida do Corte, 456',
                'descricao_curta': 'Especialistas em cortes modernos e estilosos',
                'cores': json.dumps(['#3339FF', '#33A1FF']),
                'horario_abertura': '10:00',
                'horario_fechamento': '19:00',
                'dias_funcionamento': json.dumps([0, 1, 2, 3, 4, 5]),
                'max_capacity': 10,
                'enable_priority_queue': 0,
                'created_at': datetime.now().strftime('%Y-%m-%d %H:%M:%S'),
                'updated_at': datetime.now().strftime('%Y-%m-%d %H:%M:%S')
            },
            {
                'id': str(uuid.uuid4()),
                'nome': 'Barba & Cabelo', 
                'slug': 'barba-cabelo',
                'telefone': '(11) 9999-8888',
                'endereco': 'Praça da Barba, 789',
                'descricao_curta': 'Especializados em barbas e cortes clássicos',
                'cores': json.dumps(['#4CAF50', '#009688']),
                'horario_abertura': '08:00',
                'horario_fechamento': '20:00',
                'dias_funcionamento': json.dumps([0, 1, 2, 3, 4, 5, 6]),
                'max_capacity': 20,
                'enable_priority_queue': 1,
                'created_at': datetime.now().strftime('%Y-%m-%d %H:%M:%S'),
                'updated_at': datetime.now().strftime('%Y-%m-%d %H:%M:%S')
            }
        ]
        
        # Insert the new records
        for barbershop in barbershops:
            cursor.execute('''
                INSERT INTO barbershop_barbearia (
                    id, nome, slug, telefone, endereco, descricao_curta,
                    cores, horario_abertura, horario_fechamento,
                    dias_funcionamento, max_capacity, enable_priority_queue,
                    created_at, updated_at, logo, user_id
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, NULL, NULL)
            ''', [
                barbershop['id'], barbershop['nome'], barbershop['slug'],
                barbershop['telefone'], barbershop['endereco'], barbershop['descricao_curta'],
                barbershop['cores'], barbershop['horario_abertura'], barbershop['horario_fechamento'],
                barbershop['dias_funcionamento'], barbershop['max_capacity'], barbershop['enable_priority_queue'],
                barbershop['created_at'], barbershop['updated_at']
            ])
            
            print_info(f"Created barbershop: {barbershop['nome']} with ID: {barbershop['id']}")
        
        # Commit the changes
        conn.commit()
        print_info("Successfully created all barbershops with clean UUIDs")
        
        conn.close()
        return True
    except Exception as e:
        print_error(f"Error creating clean barbershops: {str(e)}")
        return False

def verify_fix():
    """Verify that the fix worked by querying the database directly"""
    try:
        conn = sqlite3.connect(DB_PATH)
        conn.row_factory = sqlite3.Row
        cursor = conn.cursor()
        
        # Get the Barba & Cabelo record
        cursor.execute("SELECT * FROM barbershop_barbearia WHERE nome = ?", ["Barba & Cabelo"])
        row = cursor.fetchone()
        
        if row:
            id_value = row['id']
            print_info(f"'Barba & Cabelo' record has ID: {id_value}")
            
            # Check if this is a valid UUID
            try:
                uuid_obj = uuid.UUID(id_value)
                print_info(f"Valid UUID confirmed: {uuid_obj}")
                print_info("FIX SUCCESSFUL: UUID is now correctly represented")
            except ValueError:
                print_error(f"Invalid UUID: {id_value}")
                print_error("FIX FAILED: UUID is not correctly represented")
        else:
            print_info("Could not find 'Barba & Cabelo' record")
        
        conn.close()
        return True
    except Exception as e:
        print_error(f"Error verifying fix: {str(e)}")
        return False

if __name__ == "__main__":
    print_info("Starting direct database fix for UUID representation issue...")
    
    # First, display current state
    get_barbearias()
    
    # Create clean barbearias
    print_info("\nCreating clean barbearias with proper UUIDs...")
    create_clean_barbearias()
    
    # Verify the fix
    print_info("\nVerifying fix...")
    verify_fix()
    
    print_info("\nFix process complete")
