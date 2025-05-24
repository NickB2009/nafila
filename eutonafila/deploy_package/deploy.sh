#!/bin/bash
echo "Starting EuTÙNaFila deployment..."

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
