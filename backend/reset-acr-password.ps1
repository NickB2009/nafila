# Reset ACR Password Script for EuToNaFila QueueHub API
# This script regenerates the admin password for Azure Container Registry

param(
    [Parameter(Mandatory=$true)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory=$false)]
    [string]$AcrName = "acrqueuehubapi001",
    
    [Parameter(Mandatory=$false)]
    [switch]$Force
)

# Stop on first error
$ErrorActionPreference = "Stop"

Write-Host "Resetting ACR Password for EuToNaFila QueueHub API..." -ForegroundColor Green
Write-Host "Subscription ID: $SubscriptionId" -ForegroundColor Yellow
Write-Host "ACR Name: $AcrName" -ForegroundColor Yellow

# Set the subscription
Write-Host "Setting Azure subscription..." -ForegroundColor Yellow
az account set --subscription $SubscriptionId

# Check if ACR exists
Write-Host "Checking if ACR exists..." -ForegroundColor Yellow
$acrExists = az acr show --name $AcrName --query "name" --output tsv

if (-not $acrExists) {
    Write-Host "Error: ACR '$AcrName' not found. Please check the ACR name." -ForegroundColor Red
    exit 1
}

Write-Host "Found ACR: $acrExists" -ForegroundColor Green

# Check if admin user is enabled
Write-Host "Checking admin user status..." -ForegroundColor Yellow
$adminEnabled = az acr show --name $AcrName --query "adminUserEnabled" --output tsv

if ($adminEnabled -ne "True") {
    Write-Host "Admin user is not enabled. Enabling admin user..." -ForegroundColor Yellow
    az acr update --name $AcrName --admin-enabled true
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: Failed to enable admin user for ACR." -ForegroundColor Red
        exit 1
    }
    Write-Host "Admin user enabled successfully." -ForegroundColor Green
}

# Get current credentials
Write-Host "Getting current ACR credentials..." -ForegroundColor Yellow
$currentUsername = az acr credential show --name $AcrName --query username --output tsv
$currentPassword = az acr credential show --name $AcrName --query passwords[0].value --output tsv

Write-Host "Current ACR Username: $currentUsername" -ForegroundColor Yellow

if (-not $Force) {
    $confirmation = Read-Host "Do you want to regenerate the ACR password? This will invalidate the current password. (y/N)"
    if ($confirmation -ne "y" -and $confirmation -ne "Y") {
        Write-Host "Operation cancelled." -ForegroundColor Red
        exit 0
    }
}

# Regenerate ACR credentials
Write-Host "Regenerating ACR credentials..." -ForegroundColor Yellow
az acr credential renew --name $AcrName --password-name password

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to regenerate ACR password." -ForegroundColor Red
    exit 1
}

# Get new credentials
Write-Host "Getting new ACR credentials..." -ForegroundColor Yellow
$newUsername = az acr credential show --name $AcrName --query username --output tsv
$newPassword = az acr credential show --name $AcrName --query passwords[0].value --output tsv

# Verify credentials changed
if ($currentPassword -eq $newPassword) {
    Write-Host "Warning: Password appears to be the same. This might be due to ACR caching." -ForegroundColor Yellow
    Write-Host "Wait a few minutes and try again, or check the ACR in Azure Portal." -ForegroundColor Yellow
} else {
    Write-Host "Password successfully regenerated!" -ForegroundColor Green
}

# Output the new credentials
Write-Host "`n" -ForegroundColor Green
Write-Host "=== UPDATED ACR CREDENTIALS ===" -ForegroundColor Cyan
Write-Host "Update the following secrets in your GitHub repository:" -ForegroundColor Cyan
Write-Host "Go to: Settings → Secrets and variables → Actions" -ForegroundColor Cyan
Write-Host "`n" -ForegroundColor White

Write-Host "1. ACR_USERNAME (should be the same)" -ForegroundColor Yellow
Write-Host "Value: $newUsername" -ForegroundColor Gray
Write-Host "`n" -ForegroundColor White

Write-Host "2. ACR_PASSWORD (UPDATE THIS)" -ForegroundColor Yellow
Write-Host "Value: $newPassword" -ForegroundColor Gray
Write-Host "`n" -ForegroundColor White

# Test the new credentials
Write-Host "Testing new ACR credentials..." -ForegroundColor Yellow
try {
    # Create a temporary docker config with new credentials
    $dockerConfig = @{
        auths = @{
            "$AcrName.azurecr.io" = @{
                username = $newUsername
                password = $newPassword
            }
        }
    }
    
    $dockerConfigJson = $dockerConfig | ConvertTo-Json -Depth 3
    $dockerConfigJson | Out-File -FilePath "temp-docker-config.json" -Encoding UTF8
    
    # Test login (this will use the config file)
    $env:DOCKER_CONFIG = "."
    docker login $AcrName.azurecr.io --username $newUsername --password $newPassword
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ ACR login test successful!" -ForegroundColor Green
    } else {
        Write-Host "⚠️  ACR login test failed. Please verify the credentials manually." -ForegroundColor Yellow
    }
    
    # Clean up
    Remove-Item "temp-docker-config.json" -ErrorAction SilentlyContinue
    $env:DOCKER_CONFIG = $null
    
} catch {
    Write-Host "⚠️  Could not test ACR login (Docker might not be available). Please test manually." -ForegroundColor Yellow
}

# Create a summary file with timestamp
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$summaryContent = @"
# ACR Password Reset Summary - $timestamp

## ACR Details
- Name: $AcrName
- Subscription: $SubscriptionId
- Admin User Enabled: $adminEnabled

## Updated Credentials

### ACR_USERNAME (unchanged)
$newUsername

### ACR_PASSWORD (UPDATED)
$newPassword

## Previous Credentials (for reference)
- Username: $currentUsername
- Password: $currentPassword (old, no longer valid)

## Next Steps
1. Update the ACR_PASSWORD secret in your GitHub repository
2. Test the CI/CD pipeline by pushing a small change to main branch
3. Monitor the deployment in GitHub Actions

## Security Notes
- Old ACR password has been invalidated
- New password is ready for use
- Keep these credentials secure
- Consider setting up credential rotation schedule
- Monitor for any unauthorized access

## Testing Commands
```bash
# Test ACR login manually
az acr login --name $AcrName

# Or with Docker
docker login $AcrName.azurecr.io --username $newUsername --password $newPassword
```

## Troubleshooting
If the pipeline fails after updating ACR password:
1. Verify the new password is correct
2. Check that admin user is enabled on ACR
3. Ensure GitHub Actions has the updated secret
4. Check GitHub Actions logs for specific error messages
"@

$summaryFileName = "acr-password-reset-summary-$timestamp.txt"
$summaryContent | Out-File -FilePath $summaryFileName -Encoding UTF8

Write-Host "=== ACR PASSWORD RESET COMPLETE ===" -ForegroundColor Green
Write-Host "A summary has been saved to: $summaryFileName" -ForegroundColor Yellow
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Update the ACR_PASSWORD secret in your GitHub repository" -ForegroundColor White
Write-Host "2. Test the CI/CD pipeline by pushing a small change to main branch" -ForegroundColor White
Write-Host "3. Monitor the deployment in GitHub Actions" -ForegroundColor White
Write-Host "`n" -ForegroundColor White
Write-Host "⚠️  IMPORTANT: The old ACR password has been invalidated!" -ForegroundColor Red
Write-Host "   Make sure to update the GitHub secret before the next deployment." -ForegroundColor Red 