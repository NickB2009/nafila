# EuToNaFila Docker Build Script
# This script builds the Docker image locally for manual deployment

# Stop on first error
$ErrorActionPreference = "Stop"

# Ensure we're running from the repository root
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)
Set-Location $repoRoot
Write-Host "Working from repository root: $repoRoot" -ForegroundColor Yellow

# Configuration
$imageName = "queuehub-api"
$imageTag = "latest"
$projectPath = "backend/GrandeTech.QueueHub"
$dockerfilePath = "$projectPath/GrandeTech.QueueHub.API/Dockerfile"

Write-Host "Building Docker image for local deployment..." -ForegroundColor Green

# Build the Docker image
Write-Host "Building Docker image..." -ForegroundColor Yellow
Write-Host "Dockerfile path: $dockerfilePath" -ForegroundColor Yellow
Write-Host "Build context: $(Get-Location)" -ForegroundColor Yellow

try {
    # Build from the repository root with the correct context
    docker build -t ${imageName}:${imageTag} -f $dockerfilePath .
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build Docker image"
    }
} catch {
    Write-Host "Error: Failed to build Docker image. Please check if Docker is running and the Dockerfile exists." -ForegroundColor Red
    Write-Host "Current directory: $(Get-Location)" -ForegroundColor Yellow
    Write-Host "Dockerfile path: $dockerfilePath" -ForegroundColor Yellow
    exit 1
}

Write-Host "Docker image built successfully!" -ForegroundColor Green
Write-Host "Image: ${imageName}:${imageTag}" -ForegroundColor Cyan

# Test the image locally
Write-Host "Testing image locally..." -ForegroundColor Yellow
try {
    docker run --rm -d -p 8080:8080 --name queuehub-test ${imageName}:${imageTag}
    Start-Sleep -Seconds 5
    
    # Test health endpoint
    $response = Invoke-WebRequest -Uri "http://localhost:8080/health" -UseBasicParsing -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ Local test successful! Health check passed." -ForegroundColor Green
    } else {
        Write-Host "⚠️  Local test completed but health check returned status: $($response.StatusCode)" -ForegroundColor Yellow
    }
    
    # Stop the test container
    docker stop queuehub-test
} catch {
    Write-Host "⚠️  Local test failed or health check unavailable. This is normal if the app requires configuration." -ForegroundColor Yellow
}

Write-Host "`n📋 Manual Deployment Instructions:" -ForegroundColor Cyan
Write-Host "1. Install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows" -ForegroundColor White
Write-Host "2. Login to Azure: az login" -ForegroundColor White
Write-Host "3. Tag image for ACR: docker tag ${imageName}:${imageTag} acrqueuehubapi001.azurecr.io/${imageName}:${imageTag}" -ForegroundColor White
Write-Host "4. Login to ACR: az acr login --name acrqueuehubapi001" -ForegroundColor White
Write-Host "5. Push to ACR: docker push acrqueuehubapi001.azurecr.io/${imageName}:${imageTag}" -ForegroundColor White
Write-Host "6. Update App Service: az webapp config container set --name app-p-queuehub-api-001 --resource-group rg-p-queuehub-core-001 --container-image-name acrqueuehubapi001.azurecr.io/${imageName}:${imageTag}" -ForegroundColor White
Write-Host "7. Restart App Service: az webapp restart --name app-p-queuehub-api-001 --resource-group rg-p-queuehub-core-001" -ForegroundColor White

Write-Host "`n🎯 Or use the full deploy script after installing Azure CLI: .\deploy.ps1" -ForegroundColor Green 