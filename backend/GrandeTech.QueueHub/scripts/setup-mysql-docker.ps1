# Docker MySQL Setup Script for QueueHub Migration
# This script sets up MySQL using Docker for easy development

Write-Host "🐳 Setting up MySQL with Docker for QueueHub Migration..." -ForegroundColor Green

# Check if Docker is installed
Write-Host "`n📋 Checking Docker installation..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version 2>$null
    if ($dockerVersion) {
        Write-Host "✅ Docker is installed: $dockerVersion" -ForegroundColor Green
    } else {
        Write-Host "❌ Docker is not installed or not in PATH" -ForegroundColor Red
        Write-Host "Please install Docker Desktop from: https://www.docker.com/products/docker-desktop/" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "❌ Docker is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install Docker Desktop from: https://www.docker.com/products/docker-desktop/" -ForegroundColor Yellow
    exit 1
}

# Check if Docker is running
Write-Host "`n🔍 Checking Docker daemon..." -ForegroundColor Yellow
try {
    $dockerInfo = docker info 2>$null
    if ($dockerInfo) {
        Write-Host "✅ Docker daemon is running" -ForegroundColor Green
    } else {
        Write-Host "❌ Docker daemon is not running" -ForegroundColor Red
        Write-Host "Please start Docker Desktop" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "❌ Docker daemon is not running" -ForegroundColor Red
    Write-Host "Please start Docker Desktop" -ForegroundColor Yellow
    exit 1
}

# Stop any existing containers
Write-Host "`n🛑 Stopping existing MySQL containers..." -ForegroundColor Yellow
try {
    docker-compose -f docker-compose.mysql.yml down 2>$null
    Write-Host "✅ Existing containers stopped" -ForegroundColor Green
} catch {
    Write-Host "⚠️ No existing containers to stop" -ForegroundColor Yellow
}

# Start MySQL container
Write-Host "`n🚀 Starting MySQL container..." -ForegroundColor Yellow
try {
    docker-compose -f docker-compose.mysql.yml up -d
    Write-Host "✅ MySQL container started" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to start MySQL container: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Wait for MySQL to be ready
Write-Host "`n⏳ Waiting for MySQL to be ready..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
do {
    $attempt++
    Write-Host "Attempt $attempt/$maxAttempts..." -ForegroundColor Cyan
    try {
        $healthCheck = docker exec queuehub-mysql mysqladmin ping -h localhost -u root -pDevPassword123! --silent 2>$null
        if ($healthCheck -eq 0) {
            Write-Host "✅ MySQL is ready!" -ForegroundColor Green
            break
        }
    } catch {
        # Continue waiting
    }
    Start-Sleep -Seconds 2
} while ($attempt -lt $maxAttempts)

if ($attempt -eq $maxAttempts) {
    Write-Host "❌ MySQL failed to start within expected time" -ForegroundColor Red
    Write-Host "Check container logs: docker logs queuehub-mysql" -ForegroundColor Yellow
    exit 1
}

# Verify database creation
Write-Host "`n🔍 Verifying database creation..." -ForegroundColor Yellow
try {
    $dbCheck = docker exec queuehub-mysql mysql -u root -pDevPassword123! -e "SHOW DATABASES LIKE 'QueueHubDb';" 2>$null
    if ($dbCheck -match "QueueHubDb") {
        Write-Host "✅ QueueHubDb database verified" -ForegroundColor Green
    } else {
        Write-Host "❌ QueueHubDb database not found" -ForegroundColor Red
        Write-Host "Database may still be initializing. Check logs: docker logs queuehub-mysql" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Error verifying database: $($_.Exception.Message)" -ForegroundColor Red
}

# Test database connection
Write-Host "`n🔌 Testing database connection..." -ForegroundColor Yellow
try {
    $dbTest = docker exec queuehub-mysql mysql -u root -pDevPassword123! -D QueueHubDb -e "SELECT 'Connection successful' as status;" 2>$null
    if ($dbTest -match "Connection successful") {
        Write-Host "✅ Database connection successful" -ForegroundColor Green
    } else {
        Write-Host "❌ Database connection failed" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Database connection failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎉 MySQL Docker setup completed!" -ForegroundColor Green
Write-Host "`n📋 Container information:" -ForegroundColor Yellow
Write-Host "MySQL Container: queuehub-mysql" -ForegroundColor Cyan
Write-Host "phpMyAdmin Container: queuehub-phpmyadmin" -ForegroundColor Cyan
Write-Host "MySQL Port: 3306" -ForegroundColor Cyan
Write-Host "phpMyAdmin Port: 8080" -ForegroundColor Cyan

Write-Host "`n🔧 Connection details:" -ForegroundColor Yellow
Write-Host "Server: localhost" -ForegroundColor Cyan
Write-Host "Port: 3306" -ForegroundColor Cyan
Write-Host "Database: QueueHubDb" -ForegroundColor Cyan
Write-Host "Username: root" -ForegroundColor Cyan
Write-Host "Password: DevPassword123!" -ForegroundColor Cyan

Write-Host "`n🌐 Access phpMyAdmin at: http://localhost:8080" -ForegroundColor Yellow
Write-Host "Username: root" -ForegroundColor Cyan
Write-Host "Password: DevPassword123!" -ForegroundColor Cyan

Write-Host "`n📋 Next steps:" -ForegroundColor Yellow
Write-Host "1. Run the EF Core migration: dotnet ef database update" -ForegroundColor Cyan
Write-Host "2. Test the application startup" -ForegroundColor Cyan
Write-Host "3. Verify all tables are created correctly" -ForegroundColor Cyan

Write-Host "`n🛠️ Useful commands:" -ForegroundColor Yellow
Write-Host "View logs: docker logs queuehub-mysql" -ForegroundColor Cyan
Write-Host "Stop containers: docker-compose -f docker-compose.mysql.yml down" -ForegroundColor Cyan
Write-Host "Start containers: docker-compose -f docker-compose.mysql.yml up -d" -ForegroundColor Cyan
Write-Host "Access MySQL shell: docker exec -it queuehub-mysql mysql -u root -pDevPassword123!" -ForegroundColor Cyan
