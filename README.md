# 🚀 EuToNaFila QueueHub - Quick Start

**Get up and running in 2 minutes with Docker + SQL Server + Awesome Fake Data!**

## ⚡ Super Quick Start

### Option 1: One-Command Setup (Recommended)
```powershell
# Run this single command - it does everything!
./quick-start.ps1
```

### Option 2: Docker Compose
```bash
# Start everything with Docker Compose
docker-compose up -d sqlserver
cd backend/GrandeTech.QueueHub/GrandeTech.QueueHub.API
dotnet run
```

### Option 3: Manual Setup
```bash
# 1. Start SQL Server
docker run -d --name nafila-sqlserver -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=DevPassword123!" -p 1433:1433 mcr.microsoft.com/mssql/server:2019-latest

# 2. Run the API (auto-migrates and seeds data)
cd backend/GrandeTech.QueueHub/GrandeTech.QueueHub.API
dotnet run
```

## 🎯 What You Get

After running any of the above:

✅ **SQL Server** running in Docker  
✅ **Database** created automatically  
✅ **Migrations** applied automatically  
✅ **Awesome fake data** seeded:
- 🏪 **2 Organizations**: Downtown Barbers, Healthy Life Clinic
- 📍 **2 Locations**: Main Street Barbers, Pediatrics Office  
- 🦸 **4 Customers**: Clark Kent, Lois Lane, Bruce Wayne, Diana Prince
- 💳 **2 Subscription Plans**: Barber Gold, Clinic Pro
- 🎨 Complete with branding, contact info, business hours

## 🌐 Access Points

- **API**: http://localhost:5098
- **Swagger UI**: https://localhost:7126/swagger
- **Health Check**: http://localhost:5098/api/health

## 🧪 Test It Out

```bash
# Test with awesome fake data
curl http://localhost:5098/api/organizations
curl http://localhost:5098/api/auth/login -d '{"username":"customer_test","password":"CustomerPass123!"}'
```

## 🛠️ Troubleshooting

### Port 1433 Already in Use
If you get "port is already allocated" error:

**Option A: Use Existing SQL Server**
```powershell
# If you have SQL Server installed locally, just run:
cd backend/GrandeTech.QueueHub/GrandeTech.QueueHub.API
dotnet run
# The app will connect to your local SQL Server and create the database
```

**Option B: Use Different Port**
```bash
# Run SQL Server on different port
docker run -d --name nafila-sqlserver -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=DevPassword123!" -p 1434:1433 mcr.microsoft.com/mssql/server:2019-latest

# Update connection string in appsettings.json to use port 1434
```

**Option C: Clean Up Existing Containers**
```bash
docker stop nafila-sqlserver
docker rm nafila-sqlserver
# Then retry the setup
```

### Database Connection Issues
If you see connection errors:
1. **Wait longer**: SQL Server takes 20-30 seconds to start
2. **Check Docker**: `docker ps` to verify container is running
3. **Check logs**: `docker logs nafila-sqlserver`
4. **Test connection**: `docker exec nafila-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "DevPassword123!" -Q "SELECT 1"`

### Migration Errors
If migrations fail:
1. **Delete database**: The app will recreate it automatically
2. **Clean Docker volumes**: `docker volume prune`
3. **Use Bogus data**: Change `"UseBogusRepositories": true` in appsettings.json for development

### Missing .NET SDK
```bash
# Install .NET 8 SDK
winget install Microsoft.DotNet.SDK.8
# Or download from: https://dotnet.microsoft.com/download/dotnet/8.0
```

### Docker Issues
```bash
# Restart Docker Desktop
# Or check if Docker is running:
docker version
```

## 📁 Project Structure

- `backend/` - .NET 8 API with Entity Framework
- `frontend/` - Flutter mobile app
- `docker-compose.yml` - Container orchestration
- `quick-start.ps1` - One-command setup script
- `quick-start.sh` - Bash version for Linux/Mac

## 🛠️ Requirements

- Docker Desktop
- .NET 8 SDK
- PowerShell (for quick-start script)

## 📚 Full Documentation

- [Backend README](backend/README.md) - Detailed API documentation
- [Frontend README](frontend/README.md) - Flutter app setup
- [Database Strategy](backend/markdown/SQL_DATABASE_IMPLEMENTATION_STRATEGY.md) - Database architecture

---

**That's it! You're ready to build amazing queue management experiences! 🎉** 