# BoaHost Deployment Guide
## Phase 9: Production Deployment on BoaHost

### 🎯 **Overview**
This guide will help you deploy your QueueHub API to BoaHost with MySQL database integration.

### 📋 **Prerequisites**
- BoaHost hosting account with MySQL database access
- Plesk access to configure the .NET application and environment variables
- Your project files published for deployment (framework-dependent or self-contained)

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

### 🔧 **Step 3: Deploy to BoaHost (Plesk)**

1. Publish the API:
   ```bash
   dotnet publish GrandeTech.QueueHub/GrandeTech.QueueHub.API -c Release -o publish
   ```
2. Upload the `publish/` folder to your domain (e.g., `httpdocs/api`).
3. In Plesk, configure the .NET application to run via Kestrel and set the reverse proxy.
4. Set environment variables in Plesk: `ASPNETCORE_ENVIRONMENT=Production`, `MYSQL_*`, `JWT_KEY`.

### 🔧 **Step 4: Database Migration**

Run migrations automatically on startup (`Database:AutoMigrate=true`) or execute a published `dotnet` binary with `--migrate` task if available. Ensure the `MySqlConnection` string resolves from environment variables.

### 🔧 **Step 5: Verify Deployment**

1. **Check API Status**: Browse to your domain root and verify the app is responding.

2. **Check Swagger Documentation**: `https://api.eutonafila.com.br/swagger/index.html` (or your domain `/swagger`).

3. **Check Health Endpoints**: `/api/Health`, `/api/Health/database`.

### 🔧 **Step 6: Configure BoaHost Domain**

1. **Point your domain** to your BoaHost server.
2. **Configure SSL** in Plesk (Let's Encrypt or upload certificate). Ensure reverse proxy forwards `X-Forwarded-*` headers.

### 🔧 **Step 7: Production Monitoring**

#### 7.1 Provider-Agnostic Monitoring Setup

The application supports multiple monitoring providers through environment configuration:

**Monitoring Providers:**
- **Azure Application Insights** (existing)
- **BoaHost Native Logging** (recommended for BoaHost)
- **None** (disabled monitoring)

#### 7.2 BoaHost Monitoring Configuration

1. **Set monitoring provider** in Plesk environment variables:
   ```bash
   MONITORING_PROVIDER=BoaHost
   BOAHOST_LOG_LEVEL=Information
   BOAHOST_ENABLE_FILE_LOGGING=true
   BOAHOST_LOG_PATH=/var/log/queuehub
   ```

2. **Configure BoaHost logging**:
   - **Log Level**: Information, Warning, Error
   - **File Logging**: Enabled for persistent logs
   - **Log Path**: `/var/log/queuehub/` (accessible via Plesk)
   - **Log Rotation**: Automatic via Plesk

3. **Monitor performance**:
   - Check BoaHost control panel for resource usage
   - Monitor MySQL performance via Plesk
   - Review application logs in Plesk file manager
   - Set up Plesk alerts for critical issues

#### 7.3 Monitoring Features

**BoaHost Native Monitoring:**
- ✅ **Application Logs**: Structured logging to files
- ✅ **Performance Metrics**: CPU, Memory, Disk usage
- ✅ **Database Health**: MySQL connection monitoring
- ✅ **Error Tracking**: Exception logging and alerting
- ✅ **Request Logging**: API request/response tracking

**Azure Application Insights** (if needed):
- Keep existing Azure monitoring by setting `MONITORING_PROVIDER=Azure`
- Requires `APPLICATION_INSIGHTS_CONNECTION_STRING` environment variable

#### 7.4 Environment Configuration Examples

**BoaHost Production (Recommended):**
```bash
# boahost.env
MONITORING_PROVIDER=BoaHost
BOAHOST_LOG_LEVEL=Information
BOAHOST_ENABLE_FILE_LOGGING=true
BOAHOST_LOG_PATH=/var/log/queuehub
BOAHOST_ENABLE_PERFORMANCE_METRICS=true
BOAHOST_ENABLE_ERROR_TRACKING=true
```

**Azure Production (Alternative):**
```bash
# production.env
MONITORING_PROVIDER=Azure
APPLICATION_INSIGHTS_CONNECTION_STRING=InstrumentationKey=your-key;IngestionEndpoint=https://your-region.in.applicationinsights.azure.com/
```

**Development/Testing:**
```bash
# development.env
MONITORING_PROVIDER=None
# No monitoring for local development
```

#### 7.5 Monitoring Implementation

The application uses a simple provider-agnostic interface:

```csharp
public interface IMonitoringProvider
{
    void TrackEvent(string eventName, Dictionary<string, string> properties = null);
    void TrackMetric(string metricName, double value);
    void TrackException(Exception exception);
    void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool success);
}
```

**Provider Selection in Program.cs:**
```csharp
var monitoringProvider = builder.Configuration["Monitoring:Provider"];

switch (monitoringProvider)
{
    case "Azure":
        builder.Services.AddApplicationInsightsTelemetry(/* existing config */);
        break;
    case "BoaHost":
        builder.Services.AddBoaHostLogging(/* file logging config */);
        break;
    case "None":
        // No monitoring
        break;
}
```

#### 7.6 Benefits of Provider-Agnostic Monitoring

**Why This Approach:**
- ✅ **No Vendor Lock-in**: Easy to switch between Azure and BoaHost
- ✅ **Simple Configuration**: One environment variable controls everything
- ✅ **Backward Compatible**: Existing Azure monitoring still works
- ✅ **Future-Proof**: Easy to add more monitoring providers
- ✅ **Cost Effective**: Use BoaHost native logging (free) instead of Azure costs
- ✅ **Better Performance**: File logging is faster than cloud telemetry

**Migration Path:**
1. **Phase 1**: Deploy with `MONITORING_PROVIDER=Azure` (existing setup)
2. **Phase 2**: Switch to `MONITORING_PROVIDER=BoaHost` (BoaHost native)
3. **Phase 3**: Add custom monitoring providers as needed

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

4. **Monitoring Issues**:
   - Verify `MONITORING_PROVIDER` environment variable is set
   - Check log file permissions in `/var/log/queuehub/`
   - Ensure BoaHost file logging is enabled in Plesk
   - Verify monitoring configuration in appsettings

### 📊 **Production Checklist**

- [ ] BoaHost MySQL credentials configured
- [ ] Environment variables updated
- [ ] Application pool configured and site running in Plesk
- [ ] Database migration completed
- [ ] API responding on correct port
- [ ] Swagger documentation accessible
- [ ] SSL certificates configured (if using HTTPS)
- [ ] Domain pointing to BoaHost server
- [ ] **Monitoring provider configured** (`MONITORING_PROVIDER=BoaHost`)
- [ ] **BoaHost logging enabled** and log path accessible
- [ ] **Performance metrics** collection verified
- [ ] **Error tracking** and alerting configured

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
