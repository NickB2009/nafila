# QueueHub Docker Setup

This guide explains how to run QueueHub locally with Docker using SQL Server 2022. It reflects the current compose setup used in this repo.

## Prerequisites

- Docker Desktop running
- PowerShell or a terminal
- At least 4 GB RAM available (SQL Server needs memory)

## Quick Start

1. From the backend root, change into the compose folder:
   ```powershell
   cd GrandeTech.QueueHub
   ```
2. Start services:
   ```powershell
   docker-compose up -d --build
   ```
3. Open the API:
   - API base: http://localhost:8080
   - Swagger: http://localhost:8080/swagger
   - Ping: http://localhost:8080/ping

## Services

### SQL Server
- Image: `mcr.microsoft.com/mssql/server:2022-latest`
- Ports: host `1434` → container `1433`
- DB Name: `GrandeTechQueueHub`
- Credentials:
  - User: `sa`
  - Password: `DevPassword123!`
- Health check: uses `/opt/mssql-tools18/bin/sqlcmd` with `-C` to trust the self‑signed cert
- Data volume: `grandetechqueuehub_sqlserver_data`

### QueueHub API
- Internal port: `80`
- Host mapping: `8080 -> 80`
- Environment: `Development`
- Connection string key: `ConnectionStrings:AzureSqlConnection`

## Useful Commands

```powershell
# Start
docker-compose up -d

# Stop
docker-compose down

# Rebuild API image
docker-compose build queuehub-api

# Follow logs
docker-compose logs -f

# Only SQL logs
docker-compose logs -f sqlserver
```

## Database Access From Host

- SSMS / Azure Data Studio
  - Server: `localhost,1434`
  - Auth: SQL Login
  - User: `sa`
  - Password: `DevPassword123!`
  - Encrypt: optional; if enabled, trust server certificate

- ADO/EF style connection string example (matches compose):
  ```text
  Server=sqlserver:1433;Database=GrandeTechQueueHub;User Id=sa;Password=DevPassword123!;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;
  ```

## Troubleshooting

- SQL container unhealthy
  - Wait 30–60s; initial database setup takes time
  - Check health logs:
    ```powershell
    docker inspect queuehub-sqlserver --format='{{json .State.Health}}'
    ```
  - Ensure healthcheck path uses `mssql-tools18` and `-C`

- API cannot connect to DB
  - Confirm SQL is Healthy: `docker ps`
  - Confirm network: both containers on `grandetechqueuehub_queuehub-network`
  - Ensure env var key is `ConnectionStrings__AzureSqlConnection`

## Notes

- This setup is for local development only. It uses a weak dev password, self‑signed TLS, and exposed ports.
- For production, use strong secrets, TLS with valid certificates, private networking, and Azure Key Vault for secrets. 

## See also

- `markdown/SQL_DATABASE_IMPLEMENTATION_STRATEGY.md` – EF Core, connection strings, and DB strategy
- `markdown/INSTRUCTIONS.md` – Core practices, environment, and implementation order 