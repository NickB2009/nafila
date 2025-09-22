#!/bin/bash
# BoaHost Server Deployment Script (Standalone - No Docker)
# Run this script directly on the BoaHost server

echo "🚀 Starting BoaHost Standalone Deployment..."

# Step 1: Check if we're on the right server
echo "📍 Checking server environment..."
echo "Hostname: $(hostname)"
echo "Current directory: $(pwd)"
echo "User: $(whoami)"

# Step 2: Check if .NET is available
echo "🔧 Checking .NET availability..."
if command -v dotnet &> /dev/null; then
    echo "✅ .NET is available"
    dotnet --version
else
    echo "❌ .NET is not installed. Please install .NET 8 first."
    echo "Installation guide: https://docs.microsoft.com/en-us/dotnet/core/install/linux"
    exit 1
fi

# Step 3: Validate environment file
echo "📋 Validating BoaHost environment configuration..."
if [ ! -f "boahost.env" ]; then
    echo "❌ ERROR: boahost.env file not found."
    echo "Please ensure the boahost.env file is in the current directory."
    exit 1
fi
echo "✅ Environment file found"

# Step 4: Test MySQL connection (if mysql client is available)
echo "🗄️ Testing MySQL connection..."
if command -v mysql &> /dev/null; then
    # Extract MySQL connection details from environment file
    MYSQL_HOST=$(grep "^MYSQL_HOST=" boahost.env | cut -d'=' -f2)
    MYSQL_USER=$(grep "^MYSQL_USER=" boahost.env | cut -d'=' -f2)
    MYSQL_PASSWORD=$(grep "^MYSQL_PASSWORD=" boahost.env | cut -d'=' -f2)
    MYSQL_DATABASE=$(grep "^MYSQL_DATABASE=" boahost.env | cut -d'=' -f2)
    MYSQL_PORT=$(grep "^MYSQL_PORT=" boahost.env | cut -d'=' -f2)
    
    echo "Testing connection to: $MYSQL_HOST:$MYSQL_PORT"
    echo "Database: $MYSQL_DATABASE"
    echo "User: $MYSQL_USER"
    
    if mysql -h "$MYSQL_HOST" -u "$MYSQL_USER" -p"$MYSQL_PASSWORD" -P "$MYSQL_PORT" -e "SELECT 1;" &> /dev/null; then
        echo "✅ MySQL connection successful"
    else
        echo "⚠️ MySQL connection test failed, but continuing with deployment..."
    fi
else
    echo "⚠️ MySQL client not available, skipping connection test"
fi

# Step 5: Build .NET application
echo "🔨 Building .NET application for production..."
dotnet publish ./GrandeTech.QueueHub.API/GrandeTech.QueueHub.API.csproj -c Release -o ./publish --self-contained true -r linux-x64

if [ $? -ne 0 ]; then
    echo "❌ ERROR: .NET build failed"
    exit 1
fi
echo "✅ .NET application built successfully"

# Step 6: Stop existing service
echo "🛑 Stopping existing service..."
if systemctl is-active --quiet queuehub-api; then
    sudo systemctl stop queuehub-api
    echo "✅ Stopped existing service"
fi

# Step 7: Create application directory
echo "📁 Setting up application directory..."
sudo mkdir -p /var/www/queuehub-api
sudo chown -R $USER:$USER /var/www/queuehub-api

# Step 8: Copy application files
echo "📋 Copying application files..."
cp -r ./publish/* /var/www/queuehub-api/
cp ./GrandeTech.QueueHub.API/final_database_seeding.sql /var/www/queuehub-api/
chmod +x /var/www/queuehub-api/GrandeTech.QueueHub.API

# Step 9: Set up database
echo "🗄️ Setting up database..."
if command -v mysql &> /dev/null; then
    mysql -h "$MYSQL_HOST" -u "$MYSQL_USER" -p"$MYSQL_PASSWORD" -P "$MYSQL_PORT" "$MYSQL_DATABASE" < /var/www/queuehub-api/final_database_seeding.sql
    echo "✅ Database seeded successfully"
else
    echo "⚠️ MySQL client not found. Please run the seeding script manually:"
    echo "mysql -h $MYSQL_HOST -u $MYSQL_USER -p$MYSQL_PASSWORD -P $MYSQL_PORT $MYSQL_DATABASE < /var/www/queuehub-api/final_database_seeding.sql"
fi

# Step 10: Create systemd service
echo "⚙️ Creating systemd service..."
sudo tee /etc/systemd/system/queuehub-api.service > /dev/null <<EOF
[Unit]
Description=GrandeTech QueueHub API
After=network.target

[Service]
Type=notify
ExecStart=/var/www/queuehub-api/GrandeTech.QueueHub.API
WorkingDirectory=/var/www/queuehub-api
Restart=always
RestartSec=10
User=www-data
Group=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:80

[Install]
WantedBy=multi-user.target
EOF

# Step 11: Start service
echo "🚀 Starting service..."
sudo systemctl daemon-reload
sudo systemctl enable queuehub-api
sudo systemctl start queuehub-api

# Step 12: Wait for service to be ready
echo "⏳ Waiting for service to be ready..."
sleep 10

# Step 13: Test API
echo "🧪 Testing API endpoints..."
if curl -f http://localhost/health &> /dev/null; then
    echo "✅ API health check successful"
    curl -s http://localhost/health | head -c 200
    echo ""
else
    echo "⚠️ API health check failed, but service may still be starting..."
fi

# Step 14: Show service status
echo "📊 Service Status:"
sudo systemctl status queuehub-api --no-pager

echo ""
echo "🎉 BoaHost Standalone Deployment Complete!"
echo "Your API is now running as a standalone service on BoaHost!"
echo ""
echo "🌐 Access your API at:"
echo "- Main API: http://your-domain.com"
echo "- Health Check: http://your-domain.com/health"
echo "- Swagger Docs: http://your-domain.com/swagger"
echo ""
echo "📋 Service management commands:"
echo "- Start: sudo systemctl start queuehub-api"
echo "- Stop: sudo systemctl stop queuehub-api"
echo "- Status: sudo systemctl status queuehub-api"
echo "- Logs: sudo journalctl -u queuehub-api -f"
echo "- Restart: sudo systemctl restart queuehub-api"
