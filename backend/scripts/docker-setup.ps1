param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("up", "down", "build", "logs", "clean", "reset")]
    [string]$Action = "up"
)

Write-Host "QueueHub Docker Setup Script" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Green

switch ($Action) {
    "up" {
        Write-Host "Starting QueueHub services..." -ForegroundColor Yellow
        docker-compose up -d
        Write-Host "Services started! Waiting for SQL Server to be ready..." -ForegroundColor Green
        
        # Wait for SQL Server to be healthy
        $maxAttempts = 30
        $attempt = 0
        do {
            Start-Sleep -Seconds 2
            $attempt++
            $status = docker-compose ps sqlserver --format "{{.Status}}"
            Write-Host "Attempt $attempt/$maxAttempts - SQL Server status: $status" -ForegroundColor Cyan
        } while ($status -notlike "*healthy*" -and $attempt -lt $maxAttempts)
        
        if ($attempt -lt $maxAttempts) {
            Write-Host "SQL Server is ready!" -ForegroundColor Green
            Write-Host "API should be available at: http://localhost:8080" -ForegroundColor Green
            Write-Host "Swagger UI at: http://localhost:8080/swagger" -ForegroundColor Green
        } else {
            Write-Host "SQL Server failed to start properly. Check logs with: docker-compose logs sqlserver" -ForegroundColor Red
        }
    }
    
    "down" {
        Write-Host "Stopping QueueHub services..." -ForegroundColor Yellow
        docker-compose down
        Write-Host "Services stopped!" -ForegroundColor Green
    }
    
    "build" {
        Write-Host "Building QueueHub services..." -ForegroundColor Yellow
        docker-compose build --no-cache
        Write-Host "Build completed!" -ForegroundColor Green
    }
    
    "logs" {
        Write-Host "Showing logs for QueueHub services..." -ForegroundColor Yellow
        docker-compose logs -f
    }
    
    "clean" {
        Write-Host "Cleaning up Docker resources..." -ForegroundColor Yellow
        docker-compose down -v
        docker system prune -f
        Write-Host "Cleanup completed!" -ForegroundColor Green
    }
    
    "reset" {
        Write-Host "Resetting QueueHub services (clean + up)..." -ForegroundColor Yellow
        docker-compose down -v
        docker system prune -f
        docker-compose up -d
        Write-Host "Reset completed! Services should be starting..." -ForegroundColor Green
    }
}

Write-Host "`nUseful commands:" -ForegroundColor Cyan
Write-Host "  .\docker-setup.ps1 up     - Start services" -ForegroundColor White
Write-Host "  .\docker-setup.ps1 down   - Stop services" -ForegroundColor White
Write-Host "  .\docker-setup.ps1 build  - Rebuild containers" -ForegroundColor White
Write-Host "  .\docker-setup.ps1 logs   - View logs" -ForegroundColor White
Write-Host "  .\docker-setup.ps1 clean  - Clean up resources" -ForegroundColor White
Write-Host "  .\docker-setup.ps1 reset  - Reset everything" -ForegroundColor White 