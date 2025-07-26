#!/bin/bash

# Quick Start Script for EuToNaFila QueueHub
# This script gets you up and running with Docker, SQL Server, and fake data in minutes!

echo "🚀 EuToNaFila QueueHub Quick Start"
echo "================================="
echo ""

# Check if Docker is running
echo "🔍 Checking Docker..."
if ! docker version > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker first."
    exit 1
fi
echo "✅ Docker is running!"

# Start SQL Server in Docker
echo ""
echo "🐳 Starting SQL Server container..."
docker run -d --name nafila-sqlserver \
    -e "ACCEPT_EULA=Y" \
    -e "SA_PASSWORD=DevPassword123!" \
    -e "MSSQL_PID=Developer" \
    -p 1433:1433 \
    mcr.microsoft.com/mssql/server:2019-latest

if [ $? -ne 0 ]; then
    echo "Trying to restart existing container..."
    docker start nafila-sqlserver
fi

# Wait for SQL Server to be ready
echo "⏳ Waiting for SQL Server to be ready..."
timeout=30
elapsed=0
while [ $elapsed -lt $timeout ]; do
    sleep 2
    elapsed=$((elapsed + 2))
    if docker exec nafila-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "DevPassword123!" -Q "SELECT 1" > /dev/null 2>&1; then
        break
    fi
done

if [ $elapsed -ge $timeout ]; then
    echo "❌ SQL Server failed to start within $timeout seconds"
    exit 1
fi

echo "✅ SQL Server is ready!"

# Navigate to backend directory and run the API
echo ""
echo "🏗️ Building and starting the API..."
cd "backend/GrandeTech.QueueHub/GrandeTech.QueueHub.API"

echo "📦 Restoring packages..."
dotnet restore

echo "🔨 Building project..."
dotnet build

echo "🎯 Starting API with automatic database setup..."
echo ""
echo "🎉 The API will now:"
echo "   • Connect to SQL Server"
echo "   • Run database migrations"
echo "   • Seed with awesome fake data:"
echo "     - Downtown Barbers & Healthy Life Clinic"
echo "     - Clark Kent, Lois Lane, Bruce Wayne, Diana Prince"
echo "     - Realistic business data and branding"
echo ""
echo "🌐 API will be available at:"
echo "   • HTTP: http://localhost:5098"
echo "   • HTTPS: https://localhost:7126"
echo "   • Swagger: https://localhost:7126/swagger"
echo ""
echo "Press Ctrl+C to stop the API when you're done."
echo ""

# Run the API
dotnet run 