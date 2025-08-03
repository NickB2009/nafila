# QueueHub Docker Setup

This guide will help you set up the QueueHub application with SQL Server using Docker.

## Prerequisites

- Docker Desktop installed and running
- PowerShell (for Windows users)
- At least 4GB of available RAM for SQL Server

## Quick Start

1. **Navigate to the project directory:**
   ```powershell
   cd GrandeTech.QueueHub
   ```

2. **Start the services:**
   ```powershell
   .\docker-setup.ps1 up
   ```

3. **Access the application:**
   - API: http://localhost:8080
   - Swagger UI: http://localhost:8080/swagger

## Docker Services

The setup includes two main services:

### SQL Server
- **Image:** `mcr.microsoft.com/mssql/server:2022-latest`
- **Port:** 1433 (mapped to localhost:1433)
- **Database:** GrandeTechQueueHub
- **Credentials:**
  - Username: `sa`
  - Password: `DevPassword123!`
- **Data Persistence:** SQL Server data is stored in a Docker volume

### QueueHub API
- **Ports:** 8080 (HTTP), 8081 (HTTPS)
- **Environment:** Development
- **Database Connection:** Automatically connects to the SQL Server container

## Management Commands

### Using the PowerShell Script

```powershell
# Start services
.\docker-setup.ps1 up

# Stop services
.\docker-setup.ps1 down

# Rebuild containers
.\docker-setup.ps1 build

# View logs
.\docker-setup.ps1 logs

# Clean up (removes volumes)
.\docker-setup.ps1 clean

# Reset everything (clean + start)
.\docker-setup.ps1 reset
```

### Using Docker Compose Directly

```bash
# Start services
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f

# Rebuild and start
docker-compose up -d --build
```

## Database Management

### Connection Details
- **Server:** localhost,1433
- **Database:** GrandeTechQueueHub
- **Username:** sa
- **Password:** DevPassword123!
- **Trust Server Certificate:** True

### Using SQL Server Management Studio (SSMS)
1. Connect to `localhost,1433`
2. Use SQL Server Authentication
3. Username: `sa`
4. Password: `DevPassword123!`

### Using Azure Data Studio
1. Create a new connection
2. Server: `localhost,1433`
3. Authentication Type: SQL Login
4. Username: `sa`
5. Password: `DevPassword123!`

## Troubleshooting

### SQL Server Won't Start
1. Check if port 1433 is already in use:
   ```powershell
   netstat -an | findstr :1433
   ```
2. Stop any existing SQL Server instances
3. Restart Docker Desktop

### API Can't Connect to Database
1. Ensure SQL Server container is healthy:
   ```powershell
   docker-compose ps
   ```
2. Check SQL Server logs:
   ```powershell
   docker-compose logs sqlserver
   ```
3. Wait for the health check to pass (can take 30-60 seconds)

### Performance Issues
1. Increase Docker Desktop memory allocation (recommended: 8GB+)
2. Ensure you have enough disk space
3. Consider using SSD storage for better performance

### Reset Everything
If you encounter persistent issues:
```powershell
.\docker-setup.ps1 reset
```

## Development Workflow

1. **Start services:** `.\docker-setup.ps1 up`
2. **Make code changes** in your IDE
3. **Rebuild container:** `.\docker-setup.ps1 build`
4. **Restart services:** `.\docker-setup.ps1 down` then `.\docker-setup.ps1 up`
5. **View logs:** `.\docker-setup.ps1 logs`

## Data Persistence

- SQL Server data is stored in a Docker volume named `queuehub_sqlserver_data`
- This data persists between container restarts
- To completely reset the database, use `.\docker-setup.ps1 clean`

## Environment Variables

The following environment variables are configured:

### SQL Server
- `ACCEPT_EULA=Y` - Accepts the SQL Server EULA
- `SA_PASSWORD=DevPassword123!` - Sets the SA password
- `MSSQL_PID=Developer` - Uses the Developer edition

### QueueHub API
- `ASPNETCORE_ENVIRONMENT=Development`
- `ConnectionStrings__DefaultConnection` - Points to the SQL Server container

## Security Notes

⚠️ **Important:** This setup is for development only. The configuration includes:
- Weak passwords
- Trusted server certificates
- Developer edition of SQL Server
- Exposed ports

For production, ensure you:
- Use strong passwords
- Enable encryption
- Use proper SSL certificates
- Restrict network access
- Use the appropriate SQL Server edition 