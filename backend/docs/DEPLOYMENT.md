# EuTôNaFila / QueueHub - Deployment Guide

## Overview

This document provides comprehensive deployment instructions for the EuTôNaFila queue management platform on BoaHost infrastructure.

## Prerequisites

### System Requirements
- **OS**: Linux (Ubuntu 20.04+) or Windows Server 2019+
- **RAM**: Minimum 8GB, Recommended 16GB+
- **CPU**: Minimum 4 cores, Recommended 8+ cores
- **Storage**: Minimum 100GB SSD, Recommended 500GB+ SSD
- **Network**: Stable internet connection with static IP

### Software Requirements
- **PowerShell**: 7.0+ (Windows) or PowerShell Core (Linux)
- **MySQL Client**: 8.0+
- **BoaHost Account**: With MySQL database access and Plesk control panel

## Deployment Methods

### Method 1: BoaHost Plesk Deployment (Recommended)

#### Step 1: Prepare Application
```bash
# Publish the application
dotnet publish GrandeTech.QueueHub/GrandeTech.QueueHub.API -c Release -o publish
```

#### Step 2: Configure BoaHost MySQL
1. Login to BoaHost Control Panel
2. Navigate to MySQL Databases
3. Create database: `QueueHubDb`
4. Create MySQL user with strong password
5. Note down connection details:
   - Host: `mysql.boahost.com`
   - Port: `3306`
   - Database: `QueueHubDb`
   - Username: Your MySQL username
   - Password: Your MySQL password

#### Step 3: Upload to BoaHost
1. Upload `publish/` folder to domain path (e.g., `httpdocs/api`)
2. Configure .NET application in Plesk
3. Set reverse proxy from domain to internal Kestrel port

#### Step 4: Configure Environment Variables
Set these in Plesk:
```bash
ASPNETCORE_ENVIRONMENT=Production
MYSQL_HOST=mysql.boahost.com
MYSQL_DATABASE=QueueHubDb
MYSQL_USER=your_username
MYSQL_PASSWORD=your_password
MYSQL_PORT=3306
JWT_KEY=your_jwt_secret_key
MONITORING_PROVIDER=BoaHost
BOAHOST_LOG_LEVEL=Information
BOAHOST_ENABLE_FILE_LOGGING=true
BOAHOST_LOG_PATH=/var/log/queuehub
```

#### Step 5: Database Migration
- Ensure `Database:AutoMigrate=true` in configuration
- Restart application to apply migrations automatically

### Method 2: Standalone Server Deployment

#### Step 1: Server Setup
```bash
# Create application directory
sudo mkdir -p /var/www/queuehub-api
sudo chown -R $USER:$USER /var/www/queuehub-api
cd /var/www/queuehub-api
```

#### Step 2: Install .NET 8
```bash
# For Ubuntu/Debian
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# For CentOS/RHEL
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
sudo yum install -y dotnet-sdk-8.0
```

#### Step 3: Copy Application Files
```bash
# Copy published files
cp -r /path/to/publish/* /var/www/queuehub-api/
chmod +x GrandeTech.QueueHub.API
```

#### Step 4: Create Systemd Service
```bash
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
User=$USER
Group=$USER
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:80

[Install]
WantedBy=multi-user.target
EOF
```

#### Step 5: Enable and Start Service
```bash
sudo systemctl daemon-reload
sudo systemctl enable queuehub-api
sudo systemctl start queuehub-api
```

## Configuration Files

### Production Settings (`appsettings.Production.json`)
```json
{
  "ConnectionStrings": {
    "MySqlConnection": "Server=mysql.boahost.com;Database=QueueHubDb;User=nafila;Password=sigmarizzlerz67;Port=3306;CharSet=utf8mb4;SslMode=Required;Pooling=true;MinimumPoolSize=10;MaximumPoolSize=100;"
  },
  "JWT": {
    "SecretKey": "ProductionJWTSecretKey123456789012345678901234567890123456789012345678901234567890",
    "Issuer": "GrandeTech.QueueHub.Production",
    "Audience": "GrandeTech.QueueHub.Production",
    "ExpirationMinutes": 30
  },
  "Database": {
    "Provider": "MySQL",
    "AutoMigrate": true
  }
}
```

## Service Management

### Useful Commands
```bash
# Start the service
sudo systemctl start queuehub-api

# Stop the service
sudo systemctl stop queuehub-api

# Restart the service
sudo systemctl restart queuehub-api

# Check service status
sudo systemctl status queuehub-api

# View logs
sudo journalctl -u queuehub-api -f

# View recent logs
sudo journalctl -u queuehub-api --no-pager -n 50
```

## Monitoring and Health Checks

### Health Endpoints
```bash
# Basic health check
curl http://localhost/health

# Detailed health check
curl http://localhost/api/Health/detailed

# Database health check
curl http://localhost/api/Health/database
```

### Log Analysis
```bash
# View all logs
sudo journalctl -u queuehub-api

# View logs from today
sudo journalctl -u queuehub-api --since today

# Filter for errors
sudo journalctl -u queuehub-api --no-pager | grep -i error
```

### Performance Monitoring
```bash
# Check if service is running
sudo systemctl is-active queuehub-api

# Check port usage
sudo netstat -tlnp | grep :80

# Check process details
ps aux | grep GrandeTech.QueueHub.API

# Monitor resource usage
top -p $(pgrep -f GrandeTech.QueueHub.API)
```

## Database Management

### Manual Database Operations
```bash
# Connect to MySQL
mysql -h mysql.boahost.com -u nafila -psigmarizzlerz67 -P 3306 QueueHubDb

# Run SQL scripts
mysql -h mysql.boahost.com -u nafila -psigmarizzlerz67 -P 3306 QueueHubDb < mysql-schema.sql

# Backup database
mysqldump -h mysql.boahost.com -u nafila -psigmarizzlerz67 -P 3306 QueueHubDb > backup.sql

# Restore database
mysql -h mysql.boahost.com -u nafila -psigmarizzlerz67 -P 3306 QueueHubDb < backup.sql
```

## Security Considerations

1. **Firewall Configuration**: Ensure only necessary ports are open
2. **SSL/TLS**: Set up SSL certificate for HTTPS
3. **Database Security**: Use strong passwords and limit database access
4. **Application Security**: Keep the application updated
5. **File Permissions**: Ensure proper file ownership and permissions

## Updates and Maintenance

### Updating the Application
```bash
# Stop the service
sudo systemctl stop queuehub-api

# Backup current version
sudo cp -r /var/www/queuehub-api /var/www/queuehub-api.backup

# Replace with new version
# (Upload and extract new deployment package)

# Start the service
sudo systemctl start queuehub-api

# Verify update
curl http://localhost/health
```

### Regular Maintenance
```bash
# Check disk space
df -h

# Check memory usage
free -h

# Check system logs
sudo journalctl --since "1 week ago"

# Update system packages
sudo apt update && sudo apt upgrade  # Ubuntu/Debian
sudo yum update                      # CentOS/RHEL
```

## Troubleshooting

### Common Issues

#### 1. Service Won't Start
```bash
# Check logs
sudo journalctl -u queuehub-api -n 50

# Verify file permissions
ls -la /var/www/queuehub-api/GrandeTech.QueueHub.API

# Check configuration
cat /var/www/queuehub-api/appsettings.Production.json
```

#### 2. Database Connection Issues
```bash
# Test MySQL connection
mysql -h mysql.boahost.com -u nafila -psigmarizzlerz67 -P 3306 -e "SELECT 1;"

# Check network connectivity
ping mysql.boahost.com

# Verify credentials in configuration
```

#### 3. Port Already in Use
```bash
# Check what's using port 80
sudo netstat -tlnp | grep :80

# Stop conflicting services or change port in configuration
```

#### 4. Permission Denied
```bash
# Fix file ownership
sudo chown -R $USER:$USER /var/www/queuehub-api

# Fix file permissions
chmod +x /var/www/queuehub-api/GrandeTech.QueueHub.API
```

## Post-Deployment Checklist

- [ ] Service is running: `sudo systemctl status queuehub-api`
- [ ] API responds: `curl http://localhost/health`
- [ ] Database is connected: `curl http://localhost/api/Health/database`
- [ ] Domain is configured to point to the server
- [ ] SSL certificate is installed (if using HTTPS)
- [ ] Monitoring is set up
- [ ] Backups are configured
- [ ] Firewall rules are properly configured

## Production URLs

Once deployed, your API will be available at:
- **Main API**: `https://api.eutonafila.com.br`
- **Swagger Docs**: `https://api.eutonafila.com.br/swagger/index.html`
- **Health Check**: `https://api.eutonafila.com.br/api/Health`

---

*Last updated: 2025-01-15*
