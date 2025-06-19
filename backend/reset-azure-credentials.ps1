# Reset Azure Credentials Script for EuToNaFila QueueHub API
# This script deletes the old service principal and creates a new one with fresh credentials

param(
    [Parameter(Mandatory=$true)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName = "rg-p-queuehub-core-001",
    
    [Parameter(Mandatory=$false)]
    [string]$AcrName = "acrqueuehubapi001",
    
    [Parameter(Mandatory=$false)]
    [string]$ServicePrincipalName = "github-actions-queuehub",
    
    [Parameter(Mandatory=$false)]
    [switch]$Force
)

# Stop on first error
$ErrorActionPreference = "Stop"

Write-Host "Resetting Azure Credentials for EuToNaFila QueueHub API..." -ForegroundColor Green
Write-Host "Subscription ID: $SubscriptionId" -ForegroundColor Yellow
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor Yellow
Write-Host "ACR Name: $AcrName" -ForegroundColor Yellow
Write-Host "Service Principal Name: $ServicePrincipalName" -ForegroundColor Yellow

# Set the subscription
Write-Host "Setting Azure subscription..." -ForegroundColor Yellow
az account set --subscription $SubscriptionId

# Check if service principal exists
Write-Host "Checking if service principal exists..." -ForegroundColor Yellow
$existingSp = az ad sp list --display-name $ServicePrincipalName --query "[0]" --output json

if ($existingSp -ne "null") {
    $existingSpObj = $existingSp | ConvertFrom-Json
    Write-Host "Found existing service principal: $($existingSpObj.appId)" -ForegroundColor Yellow
    
    if (-not $Force) {
        $confirmation = Read-Host "Do you want to delete the existing service principal? (y/N)"
        if ($confirmation -ne "y" -and $confirmation -ne "Y") {
            Write-Host "Operation cancelled." -ForegroundColor Red
            exit 0
        }
    }
    
    # Delete existing service principal
    Write-Host "Deleting existing service principal..." -ForegroundColor Yellow
    az ad sp delete --id $existingSpObj.appId
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Successfully deleted existing service principal." -ForegroundColor Green
    } else {
        Write-Host "Warning: Failed to delete existing service principal. Continuing with creation..." -ForegroundColor Yellow
    }
} else {
    Write-Host "No existing service principal found. Creating new one..." -ForegroundColor Yellow
}

# Create new Service Principal for GitHub Actions
Write-Host "Creating new service principal for GitHub Actions..." -ForegroundColor Yellow
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

# Grant ACR Push permissions to the new service principal
Write-Host "Granting ACR Push permissions to new service principal..." -ForegroundColor Yellow
$spObjectId = az ad sp show --id $spJson.clientId --query objectId --output tsv
az role assignment create --assignee $spObjectId --scope "/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.ContainerRegistry/registries/$AcrName" --role "AcrPush"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Warning: Failed to grant ACR Push permissions. You may need to do this manually." -ForegroundColor Yellow
}

# Output the new secrets that need to be updated in GitHub
Write-Host "`n" -ForegroundColor Green
Write-Host "=== UPDATED GITHUB REPOSITORY SECRETS ===" -ForegroundColor Cyan
Write-Host "Update the following secrets in your GitHub repository:" -ForegroundColor Cyan
Write-Host "Go to: Settings → Secrets and variables → Actions" -ForegroundColor Cyan
Write-Host "`n" -ForegroundColor White

Write-Host "1. AZURE_CREDENTIALS (UPDATE THIS)" -ForegroundColor Yellow
Write-Host "Value:" -ForegroundColor White
Write-Host $spOutput -ForegroundColor Gray
Write-Host "`n" -ForegroundColor White

Write-Host "2. ACR_USERNAME (should be the same)" -ForegroundColor Yellow
Write-Host "Value: $acrUsername" -ForegroundColor Gray
Write-Host "`n" -ForegroundColor White

Write-Host "3. ACR_PASSWORD (should be the same)" -ForegroundColor Yellow
Write-Host "Value: $acrPassword" -ForegroundColor Gray
Write-Host "`n" -ForegroundColor White

# Create a summary file with timestamp
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$summaryContent = @"
# Azure Credentials Reset Summary - $timestamp

## Service Principal Details
- Name: $ServicePrincipalName
- Client ID: $($spJson.clientId)
- Tenant ID: $($spJson.tenantId)
- Subscription ID: $($spJson.subscriptionId)
- Created: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## ACR Details
- Name: $AcrName
- Username: $acrUsername
- Password: $acrPassword

## Updated GitHub Secrets

### AZURE_CREDENTIALS (UPDATED)
$spOutput

### ACR_USERNAME (unchanged)
$acrUsername

### ACR_PASSWORD (unchanged)
$acrPassword

## Next Steps
1. Update the AZURE_CREDENTIALS secret in your GitHub repository
2. Test the CI/CD pipeline by pushing a small change to main branch
3. Monitor the deployment in GitHub Actions

## Security Notes
- Old service principal has been deleted
- New credentials are ready for use
- Keep these credentials secure
- Consider setting up credential rotation schedule
- Monitor for any unauthorized access

## Troubleshooting
If the pipeline fails after updating credentials:
1. Check that the new service principal has proper permissions
2. Verify the ACR credentials are still valid
3. Check GitHub Actions logs for specific error messages
"@

$summaryFileName = "azure-credentials-reset-summary-$timestamp.txt"
$summaryContent | Out-File -FilePath $summaryFileName -Encoding UTF8

Write-Host "=== CREDENTIALS RESET COMPLETE ===" -ForegroundColor Green
Write-Host "A summary has been saved to: $summaryFileName" -ForegroundColor Yellow
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Update the AZURE_CREDENTIALS secret in your GitHub repository" -ForegroundColor White
Write-Host "2. Test the CI/CD pipeline by pushing a small change to main branch" -ForegroundColor White
Write-Host "3. Monitor the deployment in GitHub Actions" -ForegroundColor White
Write-Host "`n" -ForegroundColor White
Write-Host "⚠️  IMPORTANT: The old service principal has been deleted!" -ForegroundColor Red
Write-Host "   Make sure to update the GitHub secret before the next deployment." -ForegroundColor Red 