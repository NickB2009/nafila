# Azure Credentials Management Guide

## Overview

This guide covers how to manage Azure credentials for the EuToNaFila QueueHub API CI/CD pipeline, including resetting credentials when they expire or need to be rotated.

## Types of Credentials

### 1. AZURE_CREDENTIALS (Service Principal)
- Used for Azure resource management and deployment
- Contains client ID, client secret, tenant ID, and subscription ID
- Expires after 1 year by default

### 2. ACR_PASSWORD (Container Registry)
- Used for pushing Docker images to Azure Container Registry
- Admin password for the ACR instance
- Can be regenerated at any time

### 3. ACR_USERNAME (Container Registry)
- Username for Azure Container Registry
- Usually the same as the ACR name
- Rarely changes

## When to Reset Credentials

### AZURE_CREDENTIALS Reset Scenarios
- **Automatic Expiration**: Service principal passwords expire after 1 year
- **Security Rotation**: Recommended every 90-180 days
- **Compromise**: If credentials are suspected to be compromised
- **Permission Changes**: When service principal permissions need updating

### ACR_PASSWORD Reset Scenarios
- **Security Rotation**: Recommended every 90-180 days
- **Compromise**: If ACR credentials are suspected to be compromised
- **Access Issues**: When CI/CD pipeline fails with ACR authentication errors
- **Admin Changes**: When ACR admin user is disabled/enabled

## Quick Reset Processes

### Option 1: Reset AZURE_CREDENTIALS (Service Principal)

```powershell
# Run the reset script with your subscription ID
.\reset-azure-credentials.ps1 -SubscriptionId "160af2e4-319f-4384-b38f-71e23d140a0f"

# Or with force flag to skip confirmation
.\reset-azure-credentials.ps1 -SubscriptionId "160af2e4-319f-4384-b38f-71e23d140a0f" -Force
```

### Option 2: Reset ACR_PASSWORD (Container Registry)

```powershell
# Run the ACR password reset script
.\reset-acr-password.ps1 -SubscriptionId "160af2e4-319f-4384-b38f-71e23d140a0f"

# Or with force flag to skip confirmation
.\reset-acr-password.ps1 -SubscriptionId "160af2e4-319f-4384-b38f-71e23d140a0f" -Force
```

### Option 3: Manual ACR Password Reset

```powershell
# 1. Set subscription
az account set --subscription "160af2e4-319f-4384-b38f-71e23d140a0f"

# 2. Enable admin user (if not already enabled)
az acr update --name acrqueuehubapi001 --admin-enabled true

# 3. Regenerate password
az acr credential renew --name acrqueuehubapi001 --password-name password

# 4. Get new credentials
az acr credential show --name acrqueuehubapi001 --query username --output tsv
az acr credential show --name acrqueuehubapi001 --query passwords[0].value --output tsv
```

## Updating GitHub Secrets

### After AZURE_CREDENTIALS Reset
1. Go to GitHub repository → **Settings** → **Secrets and variables** → **Actions**
2. Find `AZURE_CREDENTIALS`
3. Click **Update** and paste the new JSON credentials
4. Click **Update secret**

### After ACR_PASSWORD Reset
1. Go to GitHub repository → **Settings** → **Secrets and variables** → **Actions**
2. Find `ACR_PASSWORD`
3. Click **Update** and paste the new password
4. Click **Update secret**
5. **Note**: `ACR_USERNAME` usually doesn't change

## Testing the New Credentials

### Test AZURE_CREDENTIALS
```bash
# Make a small change and push to main
echo "# Test azure credentials update" >> README.md
git add README.md
git commit -m "test: verify new azure credentials"
git push origin main
```

### Test ACR_PASSWORD
```bash
# Test ACR login locally
az acr login --name acrqueuehubapi001

# Or with Docker
docker login acrqueuehubapi001.azurecr.io --username <ACR_USERNAME> --password <NEW_ACR_PASSWORD>
```

## Troubleshooting Common Issues

### AZURE_CREDENTIALS Issues

**Symptoms:**
- Pipeline fails at "Azure Login" step
- Error: "AADSTS700016: Application with identifier was not found"

**Solution:**
1. Verify the service principal exists: `az ad sp list --display-name "github-actions-queuehub"`
2. Check if credentials were updated correctly in GitHub
3. Ensure the JSON format is correct (no extra spaces or characters)

### ACR_PASSWORD Issues

**Symptoms:**
- Pipeline fails at "Login to Azure Container Registry" step
- Error: "unauthorized: authentication required"
- Error: "unauthorized: incorrect username or password"

**Solution:**
1. Verify admin user is enabled: `az acr show --name acrqueuehubapi001 --query adminUserEnabled`
2. Check if password was updated correctly in GitHub
3. Test ACR login manually: `az acr login --name acrqueuehubapi001`
4. Regenerate password if needed: `az acr credential renew --name acrqueuehubapi001 --password-name password`

### Combined Issues

**Symptoms:**
- Pipeline fails at multiple authentication steps
- Both Azure and ACR authentication errors

**Solution:**
1. Reset both credentials in sequence
2. Update both GitHub secrets
3. Test with a small deployment
4. Check GitHub Actions logs for specific error messages

## Security Best Practices

### Credential Rotation Schedule
- **AZURE_CREDENTIALS**: Every 90 days (production), 180 days (development)
- **ACR_PASSWORD**: Every 90 days (production), 180 days (development)
- **Emergency**: Immediately if compromised

### Monitoring
- Set up Azure AD audit logs for service principal usage
- Monitor GitHub Actions for authentication failures
- Use Azure Security Center for credential monitoring
- Check ACR access logs for suspicious activity

### Access Control
- Use least privilege principle for service principals
- Regularly review service principal permissions
- Remove unused service principals
- Enable admin user only when needed for ACR

## Emergency Procedures

### If AZURE_CREDENTIALS are Compromised

1. **Immediate Actions:**
   ```powershell
   # Delete the compromised service principal immediately
   az ad sp delete --id <COMPROMISED_CLIENT_ID>
   ```

2. **Create Emergency Credentials:**
   ```powershell
   # Use the reset script with a different name
   .\reset-azure-credentials.ps1 -SubscriptionId "160af2e4-319f-4384-b38f-71e23d140a0f" -ServicePrincipalName "github-actions-queuehub-emergency"
   ```

### If ACR_PASSWORD is Compromised

1. **Immediate Actions:**
   ```powershell
   # Regenerate ACR password immediately
   az acr credential renew --name acrqueuehubapi001 --password-name password
   ```

2. **Update GitHub Secrets:**
   - Update ACR_PASSWORD immediately in GitHub repository
   - Test with a small deployment

### If Both Credentials are Compromised

1. **Reset Both Credentials:**
   ```powershell
   # Reset service principal
   .\reset-azure-credentials.ps1 -SubscriptionId "160af2e4-319f-4384-b38f-71e23d140a0f" -Force
   
   # Reset ACR password
   .\reset-acr-password.ps1 -SubscriptionId "160af2e4-319f-4384-b38f-71e23d140a0f" -Force
   ```

2. **Update All GitHub Secrets:**
   - Update AZURE_CREDENTIALS
   - Update ACR_PASSWORD
   - Test deployment

## Useful Commands

### Check Service Principal Status
```powershell
# List all service principals
az ad sp list --display-name "github-actions-queuehub*"

# Check specific service principal
az ad sp show --id <CLIENT_ID>

# Check permissions
az role assignment list --assignee <CLIENT_ID>
```

### Check ACR Status
```powershell
# Check ACR details
az acr show --name acrqueuehubapi001

# Check admin user status
az acr show --name acrqueuehubapi001 --query adminUserEnabled

# Get current ACR credentials
az acr credential show --name acrqueuehubapi001

# Test ACR login
az acr login --name acrqueuehubapi001

# List repositories
az acr repository list --name acrqueuehubapi001
```

### Validate Credentials
```powershell
# Test Azure login
az login --service-principal --username <CLIENT_ID> --password <CLIENT_SECRET> --tenant <TENANT_ID>

# Test resource access
az group list --query "[?name=='rg-p-queuehub-core-001']"

# Test ACR access
docker login acrqueuehubapi001.azurecr.io --username <ACR_USERNAME> --password <ACR_PASSWORD>
```

## Support and Resources

- **Azure CLI Documentation**: https://docs.microsoft.com/en-us/cli/azure/
- **GitHub Actions Documentation**: https://docs.github.com/en/actions
- **Azure Service Principal Guide**: https://docs.microsoft.com/en-us/azure/active-directory/develop/app-objects-and-service-principals
- **Azure Container Registry Documentation**: https://docs.microsoft.com/en-us/azure/container-registry/
- **Security Best Practices**: https://docs.microsoft.com/en-us/azure/security/

---

**Last Updated**: December 2024  
**Version**: 1.0.0 