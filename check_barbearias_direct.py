"""
Enhanced script to check the contents of the Barbearias table and UUID handling
"""
import sqlite3
import os
import uuid

# Configure the database file path - try both locations
DB_PATHS = [
    r"c:\git\eutonafila\nafila\eutonafila\db.sqlite3",
    r"c:\git\eutonafila\nafila\db.sqlite3"
]

def check_barbearias():
    """Check all barbearias in the database"""
    db_path = None
    
    # Find the correct database path
    for path in DB_PATHS:
        if os.path.exists(path):
            db_path = path
            break
    
    if not db_path:
        print(f"Database file not found at any of these paths: {DB_PATHS}")
        return
    
    try:
        print(f"Using database at: {db_path}")
        
        # Connect to the database
        conn = sqlite3.connect(db_path)
        conn.row_factory = sqlite3.Row  # This enables column access by name
        cursor = conn.cursor()
        
        # Get schema
        cursor.execute("PRAGMA table_info(barbershop_barbearia)")
        columns = cursor.fetchall()
        print("\nTable Schema:")
        for col in columns:
            print(f"  {col[1]}: {col[2]}")
        
        # Get all barbershops
        cursor.execute("SELECT * FROM barbershop_barbearia")
        rows = cursor.fetchall()
        
        print(f"\nFound {len(rows)} barbershops in database:")
        
        # Print details about each barbershop
        for i, row in enumerate(rows):
            print(f"\n{i+1}. {row['nome']}")
            print(f"   ID: {row['id']}")
            print(f"   ID Type: {type(row['id']).__name__}")
            
            # Check if ID is a valid UUID string
            id_value = row['id']
            is_valid_uuid = False
            
            if isinstance(id_value, str):
                try:
                    # Try to convert to UUID
                    uuid_obj = uuid.UUID(id_value)
                    is_valid_uuid = True
                    print(f"   Valid UUID: YES - Can convert to {uuid_obj}")
                except ValueError:
                    is_valid_uuid = False
                    print(f"   Valid UUID: NO - Cannot convert to UUID")
                
                # Check for the DatabaseOperations problem
                if '🔑' in id_value or 'DatabaseOperations' in id_value:
                    print(f"   PROBLEM DETECTED: ID contains DatabaseOperations reference")
            
            print(f"   Slug: {row['slug']}")
            print(f"   Telefone: {row['telefone']}")
            
            # Look specifically for Barba & Cabelo
            if "Barba & Cabelo" in row['nome']:
                print(f"   This is the 'Barba & Cabelo' record we're concerned about")
                
        # Close the connection
        conn.close()
    except Exception as e:
        print(f"Error checking barbershops: {str(e)}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    print("Checking Barbearia records directly from the database...")
    check_barbearias()
