# GitHub Actions Setup Script for EuToNaFila QueueHub API
# This script helps set up the required Azure resources and credentials for GitHub Actions CI/CD

param(
    [Parameter(Mandatory=$true)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName = "rg-p-queuehub-core-001",
    
    [Parameter(Mandatory=$false)]
    [string]$AcrName = "acrqueuehubapi001",
    
    [Parameter(Mandatory=$false)]
    [string]$ServicePrincipalName = "github-actions-queuehub"
)

# Stop on first error
$ErrorActionPreference = "Stop"

Write-Host "Setting up GitHub Actions for EuToNaFila QueueHub API..." -ForegroundColor Green
Write-Host "Subscription ID: $SubscriptionId" -ForegroundColor Yellow
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor Yellow
Write-Host "ACR Name: $AcrName" -ForegroundColor Yellow

# Set the subscription
Write-Host "Setting Azure subscription..." -ForegroundColor Yellow
az account set --subscription $SubscriptionId

# Create Service Principal for GitHub Actions
Write-Host "Creating service principal for GitHub Actions..." -ForegroundColor Yellow
$spOutput = az ad sp create-for-rbac --name $ServicePrincipalName --role contributor --scopes "/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroupName" --sdk-auth

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to create service principal. Please check your Azure CLI permissions." -ForegroundColor Red
    exit 1
}

# Parse the service principal output
$spJson = $spOutput | ConvertFrom-Json

# Get ACR credentials
Write-Host "Getting ACR credentials..." -ForegroundColor Yellow
$acrUsername = az acr credential show --name $AcrName --query username --output tsv
$acrPassword = az acr credential show --name $AcrName --query passwords[0].value --output tsv

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to get ACR credentials. Please check if the ACR exists and you have proper permissions." -ForegroundColor Red
    exit 1
}

# Grant ACR Push permissions to the service principal
Write-Host "Granting ACR Push permissions to service principal..." -ForegroundColor Yellow
$spObjectId = az ad sp show --id $spJson.clientId --query objectId --output tsv
az role assignment create --assignee $spObjectId --scope "/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.ContainerRegistry/registries/$AcrName" --role "AcrPush"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Warning: Failed to grant ACR Push permissions. You may need to do this manually." -ForegroundColor Yellow
}

# Output the secrets that need to be added to GitHub
Write-Host "`n" -ForegroundColor Green
Write-Host "=== GITHUB REPOSITORY SECRETS ===" -ForegroundColor Cyan
Write-Host "Add the following secrets to your GitHub repository:" -ForegroundColor Cyan
Write-Host "Go to: Settings → Secrets and variables → Actions" -ForegroundColor Cyan
Write-Host "`n" -ForegroundColor White

Write-Host "1. AZURE_CREDENTIALS" -ForegroundColor Yellow
Write-Host "Value:" -ForegroundColor White
Write-Host $spOutput -ForegroundColor Gray
Write-Host "`n" -ForegroundColor White

Write-Host "2. ACR_USERNAME" -ForegroundColor Yellow
Write-Host "Value: $acrUsername" -ForegroundColor Gray
Write-Host "`n" -ForegroundColor White

Write-Host "3. ACR_PASSWORD" -ForegroundColor Yellow
Write-Host "Value: $acrPassword" -ForegroundColor Gray
Write-Host "`n" -ForegroundColor White

# Create a summary file
$summaryContent = @"
# GitHub Actions Setup Summary

## Service Principal Details
- Name: $ServicePrincipalName
- Client ID: $($spJson.clientId)
- Tenant ID: $($spJson.tenantId)
- Subscription ID: $($spJson.subscriptionId)

## ACR Details
- Name: $AcrName
- Username: $acrUsername
- Password: $acrPassword

## Required GitHub Secrets

### AZURE_CREDENTIALS
$spOutput

### ACR_USERNAME
$acrUsername

### ACR_PASSWORD
$acrPassword

## Next Steps
1. Add the above secrets to your GitHub repository
2. Create a 'production' environment in GitHub (optional)
3. Push code to main branch to trigger the workflow
4. Monitor the deployment in GitHub Actions

## Security Notes
- Keep these credentials secure
- Rotate the service principal password regularly
- Monitor for any unauthorized access
"@

$summaryContent | Out-File -FilePath "github-actions-setup-summary.txt" -Encoding UTF8

Write-Host "=== SETUP COMPLETE ===" -ForegroundColor Green
Write-Host "A summary has been saved to: github-actions-setup-summary.txt" -ForegroundColor Yellow
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Add the secrets above to your GitHub repository" -ForegroundColor White
Write-Host "2. Create a 'production' environment in GitHub (optional)" -ForegroundColor White
Write-Host "3. Push code to main branch to trigger the workflow" -ForegroundColor White
Write-Host "4. Monitor the deployment in GitHub Actions" -ForegroundColor White 