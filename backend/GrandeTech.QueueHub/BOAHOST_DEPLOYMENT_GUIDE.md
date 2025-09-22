# BoaHost Deployment Guide

This guide explains how to deploy the QueueHub API directly to your BoaHost server where the MySQL database is accessible.

## Prerequisites

1. **BoaHost Server Access**: You need access to your BoaHost server (SSH, file manager, or control panel)
2. **Docker Support**: Your BoaHost server should support Docker and Docker Compose
3. **Domain Configuration**: Your domain should point to the BoaHost server

## Deployment Methods

### Method 1: SSH Deployment (Recommended)

If you have SSH access to your BoaHost server:

#### Step 1: Upload Files to Server
```bash
# Upload the deployment package
scp QueueHub-BoaHost-Deployment.zip username@your-boahost-server:/home/username/

# SSH into the server
ssh username@your-boahost-server

# Extract the files
cd /home/username
unzip QueueHub-BoaHost-Deployment.zip
cd QueueHub-BoaHost-Deployment
```

#### Step 2: Run Deployment Script
```bash
# Make the script executable
chmod +x scripts/boahost-server-deploy.sh

# Run the deployment
./scripts/boahost-server-deploy.sh
```

### Method 2: File Manager Upload

If BoaHost provides a web-based file manager:

1. **Upload Files**:
   - Upload `QueueHub-BoaHost-Deployment.zip` to your server
   - Extract it in your web directory (usually `/public_html` or `/www`)

2. **Access Server via SSH**:
   - Use BoaHost's terminal/SSH feature
   - Navigate to the extracted directory
   - Run the deployment script

### Method 3: BoaHost Control Panel

If BoaHost has a deployment interface:

1. **Upload via Control Panel**:
   - Use the file upload feature in your BoaHost control panel
   - Upload the `QueueHub-BoaHost-Deployment.zip` file

2. **Extract and Deploy**:
   - Extract the files in your hosting directory
   - Use the terminal/SSH feature to run deployment commands

## Required Files for Deployment

The deployment package includes:
- `GrandeTech.QueueHub.API/` - The API application
- `docker-compose.boahost.yml` - Docker Compose configuration
- `boahost.env` - Environment variables
- `scripts/boahost-server-deploy.sh` - Deployment script
- `mysql-schema.sql` - Database schema
- `scripts/` - Various utility scripts

## Environment Configuration

The `boahost.env` file contains your production settings:

```env
# Database Configuration
MYSQL_HOST=mysql.boahost.com
MYSQL_DATABASE=QueueHubDb
MYSQL_USER=nafila
MYSQL_PASSWORD=sigmarizzlerz67
MYSQL_PORT=3306

# JWT Configuration
JWT_SECRET_KEY=ProductionJWTSecretKey123456789012345678901234567890123456789012345678901234567890
JWT_ISSUER=GrandeTech.QueueHub.Production
JWT_AUDIENCE=GrandeTech.QueueHub.Production
JWT_EXPIRATION_MINUTES=30

# Other production settings...
```

## Manual Deployment Steps

If you prefer to run the deployment manually:

```bash
# 1. Stop any existing services
docker-compose -f docker-compose.boahost.yml --env-file boahost.env down

# 2. Build and start services
docker-compose -f docker-compose.boahost.yml --env-file boahost.env up -d --build

# 3. Check service status
docker-compose -f docker-compose.boahost.yml --env-file boahost.env ps

# 4. View logs
docker-compose -f docker-compose.boahost.yml --env-file boahost.env logs api

# 5. Test the API
curl http://localhost/health
```

## Database Migration

If the database tables don't exist, you may need to run the schema creation:

```bash
# Connect to MySQL and create tables
mysql -h mysql.boahost.com -u nafila -psigmarizzlerz67 -P 3306 < mysql-schema.sql

# Or use the API's test initialization endpoint
curl -X POST http://localhost/api/Test/initialize
```

## Verification

After deployment, verify everything is working:

1. **Health Check**:
   ```bash
   curl http://your-domain.com/health
   ```

2. **API Status**:
   ```bash
   curl http://your-domain.com/api/Health/database
   ```

3. **Swagger Documentation**:
   ```bash
   curl http://your-domain.com/swagger
   ```

## Troubleshooting

### Common Issues:

1. **Docker Not Available**:
   - Contact BoaHost support to enable Docker
   - Or use alternative deployment methods

2. **Permission Denied**:
   ```bash
   chmod +x scripts/boahost-server-deploy.sh
   sudo chown -R $USER:$USER .
   ```

3. **MySQL Connection Failed**:
   - Verify the database credentials in `boahost.env`
   - Check if the MySQL server is accessible from your BoaHost server
   - Ensure the database `QueueHubDb` exists

4. **Port Conflicts**:
   - Modify ports in `docker-compose.boahost.yml` if needed
   - Check what ports are available on your BoaHost server

### View Logs:
```bash
# API logs
docker-compose -f docker-compose.boahost.yml --env-file boahost.env logs api

# All services logs
docker-compose -f docker-compose.boahost.yml --env-file boahost.env logs

# Follow logs in real-time
docker-compose -f docker-compose.boahost.yml --env-file boahost.env logs -f api
```

## Post-Deployment

1. **Configure Domain**: Point your domain to the BoaHost server
2. **SSL Certificate**: Set up SSL/TLS certificate if needed
3. **Monitoring**: Set up monitoring and logging
4. **Backup**: Configure database backups
5. **Updates**: Plan for future updates and deployments

## Support

If you encounter issues:
1. Check the logs using the commands above
2. Verify all prerequisites are met
3. Contact BoaHost support for server-specific issues
4. Review the troubleshooting section above
