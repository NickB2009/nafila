# BoaHost Deployment Guide
## Phase 9: Production Deployment on BoaHost

### 🎯 **Overview**
This guide will help you deploy your QueueHub API to BoaHost with MySQL database integration.

### 📋 **Prerequisites**
- BoaHost hosting account with MySQL database access
- Docker installed on your local machine
- Your project files ready for deployment

### 🔧 **Step 1: Get BoaHost MySQL Credentials**

1. **Login to BoaHost Control Panel**
2. **Navigate to MySQL Databases**
3. **Create a new database** (if not already created):
   - Database Name: `QueueHubDb` (or your preferred name)
   - Username: Create a new MySQL user
   - Password: Generate a strong password
4. **Note down these details**:
   - Host/Server: Usually `mysql.boahost.com` or similar
   - Port: Usually `3306`
   - Database Name: `QueueHubDb`
   - Username: Your MySQL username
   - Password: Your MySQL password

### 🔧 **Step 2: Update Environment Configuration**

1. **Edit `boahost.env` file**:
   ```bash
   # Replace these with your actual BoaHost credentials
   MYSQL_HOST=mysql.boahost.com
   MYSQL_DATABASE=QueueHubDb
   MYSQL_USER=your_actual_username
   MYSQL_PASSWORD=your_actual_password
   MYSQL_PORT=3306
   ```

2. **Update other production settings** as needed:
   - JWT secret keys
   - CORS origins (your frontend domains)
   - SSL certificates (if using HTTPS)

### 🔧 **Step 3: Deploy to BoaHost**

#### Option A: Using Docker (Recommended)
```bash
# 1. Build and deploy
powershell -ExecutionPolicy Bypass -File scripts/deploy-to-boahost.ps1

# 2. Check status
docker-compose -f docker-compose.boahost.yml --env-file boahost.env ps
```

#### Option B: Manual Deployment
```bash
# 1. Build the Docker image
docker build -t grandetech-queuehub-api:boahost -f ./GrandeTech.QueueHub.API/Dockerfile ./GrandeTech.QueueHub.API

# 2. Start services
docker-compose -f docker-compose.boahost.yml --env-file boahost.env up -d
```

### 🔧 **Step 4: Database Migration**

```bash
# Run database migration
powershell -ExecutionPolicy Bypass -File scripts/migrate-boahost-database.ps1
```

### 🔧 **Step 5: Verify Deployment**

1. **Check API Status**:
   ```bash
   curl http://localhost
   # Should return: {"status":"online","environment":"Production",...}
   ```

2. **Check Swagger Documentation**:
   - Open browser: `http://your-domain.com/swagger`

3. **Check Health Endpoint**:
   ```bash
   curl http://localhost/health
   ```

### 🔧 **Step 6: Configure BoaHost Domain**

1. **Point your domain** to your BoaHost server
2. **Configure SSL** (if using HTTPS):
   - Upload SSL certificates to `./nginx/ssl/`
   - Update nginx configuration if needed

### 🔧 **Step 7: Production Monitoring**

1. **Check logs**:
   ```bash
   docker-compose -f docker-compose.boahost.yml --env-file boahost.env logs api
   ```

2. **Monitor performance**:
   - Check BoaHost control panel for resource usage
   - Monitor MySQL performance
   - Set up alerts for critical issues

### 🚨 **Troubleshooting**

#### Common Issues:

1. **Database Connection Failed**:
   - Verify MySQL credentials in `boahost.env`
   - Check if BoaHost allows external connections
   - Verify firewall settings

2. **API Not Responding**:
   - Check if Docker containers are running
   - Verify port mappings
   - Check nginx configuration

3. **Migration Errors**:
   - Ensure database exists on BoaHost
   - Check user permissions
   - Verify connection string format

### 📊 **Production Checklist**

- [ ] BoaHost MySQL credentials configured
- [ ] Environment variables updated
- [ ] Docker containers running
- [ ] Database migration completed
- [ ] API responding on correct port
- [ ] Swagger documentation accessible
- [ ] SSL certificates configured (if using HTTPS)
- [ ] Domain pointing to BoaHost server
- [ ] Monitoring and logging set up

### 🎉 **Success!**

Once all steps are completed, your QueueHub API will be running on BoaHost with MySQL database integration!

**Your API will be available at:**
- Main API: `http://your-domain.com`
- Swagger Docs: `http://your-domain.com/swagger`
- Health Check: `http://your-domain.com/health`

### 📞 **Support**

If you encounter any issues:
1. Check the logs: `docker-compose -f docker-compose.boahost.yml --env-file boahost.env logs`
2. Verify your BoaHost MySQL settings
3. Ensure all environment variables are correctly set
4. Check BoaHost documentation for specific hosting requirements
