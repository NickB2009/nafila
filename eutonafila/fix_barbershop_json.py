#!/usr/bin/env python
"""
Script to fix the SQLite database schema issue with JSONField
"""
import sqlite3
import json
from datetime import datetime

# Create a barbershop with direct SQL
def create_barbershop(name, slug):
    # Current timestamp in ISO format
    now = datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f')
    
    # Connect to SQLite database
    conn = sqlite3.connect('db.sqlite3')
    cursor = conn.cursor()
    
    # First check if the barbershop already exists and delete it
    cursor.execute("DELETE FROM barbershop_barbearia WHERE slug = ?", (slug,))
    
    # Prepare data as TEXT for JSONField columns
    dias_funcionamento = json.dumps([0, 1, 2, 3, 4, 5])
    cores = json.dumps(['#FF5733', '#33FF57'])
    
    # Insert directly with SQL, let SQLite auto-generate the ID
    sql = """
    INSERT INTO barbershop_barbearia (
        nome, slug, telefone, endereco, descricao_curta, 
        cores, logo, horario_abertura, horario_fechamento, 
        dias_funcionamento, max_capacity, enable_priority_queue,
        created_at, updated_at, user_id
    ) VALUES (
        ?, ?, ?, ?, ?, 
        ?, ?, ?, ?, 
        ?, ?, ?,
        ?, ?, ?
    )
    """
    
    try:
        cursor.execute(sql, (
            name, slug, '(123) 456-7890', '123 Main St', 'A modern barbershop',
            cores, None, '09:00', '18:00',
            dias_funcionamento, 10, 1,
            now, now, None
        ))
        conn.commit()
        
        # Get the ID of the inserted row
        cursor.execute("SELECT last_insert_rowid()")
        barbershop_id = cursor.fetchone()[0]
        print(f"Successfully created barbershop '{name}' with ID: {barbershop_id}")
        
        # Verify it was created
        cursor.execute("SELECT id, nome, slug, dias_funcionamento FROM barbershop_barbearia WHERE slug = ?", (slug,))
        result = cursor.fetchone()
        if result:
            print(f"Verified: {result}")
        else:
            print("Failed to create barbershop")
    except Exception as e:
        print(f"Error creating barbershop: {str(e)}")
        import traceback
        traceback.print_exc()
    finally:
        conn.close()

def list_all_barbershops():
    # Connect to SQLite database
    conn = sqlite3.connect('db.sqlite3')
    cursor = conn.cursor()
    
    # List all barbershops
    cursor.execute("SELECT id, nome, slug FROM barbershop_barbearia")
    print("\nAll barbershops in database:")
    for row in cursor.fetchall():
        print(f"  ID: {row[0]}, Name: {row[1]}, Slug: {row[2]}")
    
    conn.close()

if __name__ == "__main__":
    # Display table schema
    conn = sqlite3.connect('db.sqlite3')
    cursor = conn.cursor()
    cursor.execute("PRAGMA table_info(barbershop_barbearia)")
    schema = cursor.fetchall()
    print("Table schema:")
    for col in schema:
        print(f"  {col}")
    conn.close()
    
    # Create two barbershops
    create_barbershop("Test Barbershop", "test-barbershop")
    create_barbershop("Classy Cuts Barber", "classy-cuts")
    
    # List all barbershops
    list_all_barbershops() 