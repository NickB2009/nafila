# BoaHost Standalone Deployment Guide (No Docker Required)

This guide explains how to deploy the QueueHub API directly to your BoaHost server as a standalone .NET application without Docker.

## Prerequisites

1. **BoaHost Server Access**: SSH access or file manager access to your BoaHost server
2. **Linux Server**: The BoaHost server should run Linux (Ubuntu, CentOS, or similar)
3. **Domain Configuration**: Your domain should point to the BoaHost server
4. **MySQL Access**: The server should be able to connect to `mysql.boahost.com`

## What This Deployment Includes

- ✅ **Standalone .NET Application**: Self-contained executable with all dependencies
- ✅ **Production Configuration**: Optimized settings for production environment
- ✅ **Systemd Service**: Automatic startup and management
- ✅ **Database Setup**: Automatic MySQL schema creation
- ✅ **Health Monitoring**: Built-in health checks and logging

## Deployment Methods

### Method 1: Complete Automated Deployment

1. **Upload the deployment package to your BoaHost server**
2. **Extract the files**
3. **Run the automated deployment script**

```bash
# Extract the deployment package
unzip QueueHub-BoaHost-Standalone.zip
cd QueueHub-BoaHost-Standalone

# Make scripts executable
chmod +x scripts/*.sh

# Run the complete deployment
./scripts/deploy-boahost-standalone.sh
```

### Method 2: Step-by-Step Manual Deployment

#### Step 1: Set Up Application Directory
```bash
# Create application directory
sudo mkdir -p /var/www/queuehub-api
sudo chown -R $USER:$USER /var/www/queuehub-api
cd /var/www/queuehub-api
```

#### Step 2: Install .NET 8 (if not already installed)
```bash
# For Ubuntu/Debian
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# For CentOS/RHEL
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
sudo yum install -y dotnet-sdk-8.0
```

#### Step 3: Copy Application Files
```bash
# Copy all files from the deployment package
cp -r /path/to/deployment/* /var/www/queuehub-api/
cd /var/www/queuehub-api

# Make the application executable
chmod +x GrandeTech.QueueHub.API
```

#### Step 4: Set Up Database
```bash
# Install MySQL client (if needed)
sudo apt-get install -y mysql-client  # Ubuntu/Debian
# or
sudo yum install -y mysql            # CentOS/RHEL

# Run database setup
./scripts/setup-database.sh
```

#### Step 5: Create Systemd Service
```bash
# Create the service file
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

# Enable and start the service
sudo systemctl daemon-reload
sudo systemctl enable queuehub-api
sudo systemctl start queuehub-api
```

#### Step 6: Verify Deployment
```bash
# Check service status
sudo systemctl status queuehub-api

# Test the API
curl http://localhost/health

# View logs
sudo journalctl -u queuehub-api -f
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
  "ASPNETCORE_ENVIRONMENT": "Production",
  "Urls": "http://0.0.0.0:80"
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

# Enable auto-start on boot
sudo systemctl enable queuehub-api

# Disable auto-start on boot
sudo systemctl disable queuehub-api
```

## Monitoring and Troubleshooting

### Health Checks

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

# View logs with timestamps
sudo journalctl -u queuehub-api --no-pager -o short-iso

# Filter for errors
sudo journalctl -u queuehub-api --no-pager | grep -i error
```

### Performance Monitoring

```bash
# Check if the service is running
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
# or
sudo yum update                      # CentOS/RHEL
```

## Troubleshooting

### Common Issues

1. **Service Won't Start**:
   - Check logs: `sudo journalctl -u queuehub-api -n 50`
   - Verify file permissions: `ls -la /var/www/queuehub-api/GrandeTech.QueueHub.API`
   - Check configuration: `cat /var/www/queuehub-api/appsettings.Production.json`

2. **Database Connection Issues**:
   - Test MySQL connection: `mysql -h mysql.boahost.com -u nafila -psigmarizzlerz67 -P 3306 -e "SELECT 1;"`
   - Check network connectivity: `ping mysql.boahost.com`
   - Verify credentials in configuration

3. **Port Already in Use**:
   - Check what's using port 80: `sudo netstat -tlnp | grep :80`
   - Stop conflicting services or change the port in configuration

4. **Permission Denied**:
   - Fix file ownership: `sudo chown -R $USER:$USER /var/www/queuehub-api`
   - Fix file permissions: `chmod +x /var/www/queuehub-api/GrandeTech.QueueHub.API`

### Getting Help

1. Check the application logs first
2. Verify all prerequisites are met
3. Test each component individually
4. Contact BoaHost support for server-specific issues

## Post-Deployment Checklist

- [ ] Service is running: `sudo systemctl status queuehub-api`
- [ ] API responds: `curl http://localhost/health`
- [ ] Database is connected: `curl http://localhost/api/Health/database`
- [ ] Domain is configured to point to the server
- [ ] SSL certificate is installed (if using HTTPS)
- [ ] Monitoring is set up
- [ ] Backups are configured
- [ ] Firewall rules are properly configured
