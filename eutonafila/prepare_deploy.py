import os
import shutil
from pathlib import Path

def prepare_deployment():
    # Create deployment directory
    deploy_dir = 'deploy_package'
    if os.path.exists(deploy_dir):
        shutil.rmtree(deploy_dir)
    os.makedirs(deploy_dir)

    # Separate requirements by environment
    requirements = {
        'base': [
            'Django==5.0.2',
            'djangorestframework==3.14.0',
            'django-cors-headers==4.3.1',
            'djangorestframework-simplejwt==5.3.1',
            'channels==4.0.0',
            'daphne==4.0.0',
            'whitenoise==6.5.0',
            'python-dotenv==1.0.0',
            'gunicorn==21.2.0',
        ],
        'production': [
            'redis==5.0.1',
            'channels-redis==4.1.0',
        ]
    }

    # Create requirements files
    os.makedirs(os.path.join(deploy_dir, 'requirements'))
    
    with open(os.path.join(deploy_dir, 'requirements', 'base.txt'), 'w') as f:
        f.write('\n'.join(requirements['base']))
    
    with open(os.path.join(deploy_dir, 'requirements', 'production.txt'), 'w') as f:
        f.write('\n'.join(requirements['production']))

    # Files to copy
    project_files = [
        'manage.py',
        'passenger_wsgi.py',
        '.htaccess',
        'eutonafila/',
        'barbershop/',
        'domain/',
        'application/',
        'infrastructure/',
        'static/',
        'templates/',
        'media/'
    ]

    print("Copying project files...")
    for item in project_files:
        src = Path(item)
        dst = Path(deploy_dir) / src
        if src.exists():
            if src.is_file():
                dst.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(src, dst)
            elif src.is_dir():
                shutil.copytree(src, dst, dirs_exist_ok=True)
        else:
            print(f"Warning: {item} not found, skipping...")

    # Create environment configuration template
    env_template = """# Django Configuration
DJANGO_SETTINGS_MODULE=eutonafila.settings_production
DJANGO_SECRET_KEY=your-secret-key-here
DEBUG=False

# Database Configuration (MariaDB/MySQL)
DB_ENGINE=mysql
DB_NAME=your_database_name
DB_USER=your_database_user
DB_PASSWORD=your_database_password
DB_HOST=localhost
DB_PORT=3306

# Redis Configuration (Optional)
USE_REDIS=false
REDIS_HOST=localhost
REDIS_PORT=6379

# Email Configuration
EMAIL_HOST=smtp.kinghost.net
EMAIL_PORT=587
EMAIL_USE_TLS=True
EMAIL_HOST_USER=your_email@eutonafila.com.br
EMAIL_HOST_PASSWORD=your_email_password"""

    with open(os.path.join(deploy_dir, '.env.example'), 'w') as f:
        f.write(env_template)

    # Create shell script for easy deployment
    deploy_script = """#!/bin/bash
echo "Starting EuTôNaFila deployment..."

# Create virtual environment if it doesn't exist
if [ ! -d "venv" ]; then
    python3 -m venv venv
fi

# Activate virtual environment
source venv/bin/activate

# Install dependencies
pip install --upgrade pip
pip install -r requirements/base.txt
pip install -r requirements/production.txt

# Apply migrations
python manage.py migrate --noinput

# Collect static files
python manage.py collectstatic --noinput

echo "Deployment complete! Remember to:"
echo "1. Configure your .env file"
echo "2. Set up your database"
echo "3. Configure SSL in KingHost panel"
"""

    with open(os.path.join(deploy_dir, 'deploy.sh'), 'w') as f:
        f.write(deploy_script)

    # Create detailed deployment instructions
    instructions = """EuTôNaFila - KingHost Deployment Guide
================================

Pre-deployment Requirements:
-------------------------
1. KingHost account with Python support
2. Domain configured (eutonafila.com.br)
3. MySQL database credentials
4. SSL certificate (recommended)

Deployment Steps:
--------------
1. File Upload:
   - Upload all contents of this package to your KingHost directory
   - Set permissions:
     * chmod 755 for directories
     * chmod 644 for files
     * chmod 755 for deploy.sh

2. Environment Setup:
   - Copy .env.example to .env
   - Update .env with your credentials
   - Ensure Python 3.8+ is available

3. Database Configuration:
   - Create MySQL database via KingHost panel
   - Update database credentials in .env

4. Deployment:
   - Connect via SSH to your KingHost server
   - Navigate to project directory
   - Run: ./deploy.sh

5. Web Server Setup:
   - Configure Python WSGI in KingHost panel
   - Point to passenger_wsgi.py
   - Set up static files directory

6. SSL Configuration:
   - Enable SSL in KingHost panel
   - Verify HTTPS redirects

7. Verification:
   - Visit https://eutonafila.com.br
   - Check admin interface
   - Monitor logs for errors

Troubleshooting:
--------------
1. Static Files Issues:
   - Verify STATIC_ROOT in settings
   - Check directory permissions
   - Run collectstatic manually

2. Database Connection:
   - Verify credentials in .env
   - Test database connection
   - Check KingHost MySQL status

3. 500 Server Errors:
   - Check error logs
   - Verify all requirements installed
   - Check file permissions

Support:
-------
For hosting related issues:
- KingHost Support: https://king.host/suporte
- Django Deployment Docs: https://docs.djangoproject.com/en/5.0/howto/deployment/"""

    with open(os.path.join(deploy_dir, 'DEPLOY_INSTRUCTIONS.md'), 'w') as f:
        f.write(instructions)

    print("Creating deployment archive...")
    shutil.make_archive('eutonafila_deploy', 'zip', deploy_dir)
    
    print("\nDeployment package created successfully!")
    print("1. Upload eutonafila_deploy.zip to your KingHost server")
    print("2. Follow instructions in DEPLOY_INSTRUCTIONS.md")

if __name__ == '__main__':
    prepare_deployment()