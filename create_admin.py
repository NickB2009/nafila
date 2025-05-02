import sqlite3
import hashlib
import random
import string
from datetime import datetime

def get_random_string(length=12):
    """Generate a random string of fixed length"""
    letters = string.ascii_letters + string.digits
    return ''.join(random.choice(letters) for i in range(length))

def make_password(password):
    """
    Create a simple password hash (not as secure as Django's but works for testing)
    Format: pbkdf2_sha256$<iterations>$<salt>$<hash>
    """
    algorithm = 'pbkdf2_sha256'
    iterations = 600000  # Match Django's default iterations
    salt = get_random_string(12)  # Generate a random salt
    
    # Hash the password with the salt
    hash_obj = hashlib.pbkdf2_hmac(
        'sha256', 
        password.encode(), 
        salt.encode(), 
        iterations,
        dklen=32  # Match Django's hash length
    )
    
    # Convert hash to hex
    hash_hex = hash_obj.hex()
    
    # Format according to Django's pattern
    return f"{algorithm}${iterations}${salt}${hash_hex}"

def create_admin_user():
    """Create or update the admin user in the database"""
    # Connect to the database
    conn = sqlite3.connect('db.sqlite3')
    cursor = conn.cursor()
    
    # Check if auth_user table exists
    cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='auth_user';")
    if not cursor.fetchone():
        print("The auth_user table does not exist. Django may not be set up correctly.")
        conn.close()
        return False
    
    # Get current timestamp
    now = datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f')
    
    # Check if admin user already exists
    cursor.execute("SELECT id FROM auth_user WHERE username = 'admin';")
    admin_exists = cursor.fetchone()
    
    password_hash = make_password('admin')
    
    if admin_exists:
        # Update existing admin
        admin_id = admin_exists[0]
        cursor.execute("""
            UPDATE auth_user SET 
                password = ?,
                last_login = NULL,
                is_superuser = 1,
                first_name = 'Admin',
                last_name = 'User',
                email = 'admin@example.com',
                is_staff = 1,
                is_active = 1,
                date_joined = ?
            WHERE id = ?;
        """, (password_hash, now, admin_id))
        print(f"Updated admin user (ID: {admin_id}) with password 'admin'")
    else:
        # Create new admin
        cursor.execute("""
            INSERT INTO auth_user (
                username, password, last_login, is_superuser, 
                first_name, last_name, email, is_staff, 
                is_active, date_joined
            ) VALUES (?, ?, NULL, 1, 'Admin', 'User', 'admin@example.com', 1, 1, ?);
        """, ('admin', password_hash, now))
        admin_id = cursor.lastrowid
        print(f"Created admin user (ID: {admin_id}) with password 'admin'")
    
    # Commit changes
    conn.commit()
    conn.close()
    
    return True

if __name__ == "__main__":
    print("Creating/updating admin user in the database...")
    success = create_admin_user()
    
    if success:
        print("\nAdmin credentials:")
        print("Username: admin")
        print("Password: admin")
        print("\nYou can now log in to the admin interface.")
    else:
        print("Failed to create admin user. Check the database setup.") 