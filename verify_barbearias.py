"""
Simple script to print Barbearias directly from database
"""
import os
import sqlite3

# Database path
DB_PATH = "c:/git/eutonafila/nafila/eutonafila/db.sqlite3"

def print_barbearias():
    """Print all barbearias in the database"""
    try:
        # Connect to the database
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        # Get all barbershops
        cursor.execute("SELECT id, nome, slug, telefone FROM barbershop_barbearia")
        rows = cursor.fetchall()
        
        print(f"\nFound {len(rows)} barbershops in database:")
        
        for row in rows:
            barbershop_id, nome, slug, telefone = row
            print(f"  - {nome} (ID: {barbershop_id}, Slug: {slug}, Tel: {telefone})")
            
            # Check if this is the problematic record
            if "Barba & Cabelo" in nome:
                print(f"    This is our target record - the ID is properly stored as: {barbershop_id}")
        
        conn.close()
        
    except Exception as e:
        print(f"Error: {str(e)}")

if __name__ == "__main__":
    print_barbearias()
