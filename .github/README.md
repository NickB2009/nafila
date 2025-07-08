# GitHub Actions CI/CD & Azure Deployment Guide

> **Note:** This document now consolidates all Azure deployment and CI/CD instructions. The previous `CI_CD_QUICK_START.md` has been merged here. For credential management, see [Azure Credentials Management Guide](../backend/markdown/AZURE_CREDENTIALS_MANAGEMENT.md).

## Quick Start (Summary)

### 1. Set Up GitHub Secrets
Run the setup script to get your Azure credentials:

```powershell
./setup-github-actions.ps1 -SubscriptionId "160af2e4-319f-4384-b38f-71e23d140a0f"
```

Add the following secrets to your GitHub repository (**Settings** → **Secrets and variables** → **Actions**):
- `AZURE_CREDENTIALS` - JSON from the setup script
- `ACR_USERNAME` - ACR username from the setup script
- `ACR_PASSWORD` - ACR password from the setup script

### 2. (Optional) Create Production Environment
Go to **Settings** → **Environments** → **New environment**:
- Name: `production`
- Add protection rules if needed

### 3. How It Works
- **Push to `main`**: Full build, test, and deploy
- **Pull Request to `main`**: Build and test only

#### Pipeline Stages
1. **Build & Test** - Compiles code and runs tests
2. **Build & Push Image** - Creates Docker image and pushes to ACR
3. **Deploy to Azure** - Updates App Service with new image

#### Health Checks
- Verifies deployment at `/health` endpoint (retries up to 10 times)

### 4. Test Your Setup
1. Commit and push to your repository
2. Merge to `main` branch
3. Check GitHub Actions tab for pipeline status
4. Monitor deployment in Azure Portal

### 5. Troubleshooting & Monitoring
- **GitHub Actions**: View pipeline status and logs
- **Azure Portal**: Monitor App Service and ACR
- **Health Endpoint**: `https://app-p-queuehub-api-001.azurewebsites.net/health`

#### Common Issues
- **Build fails**: Check .NET version and dependencies
- **Docker fails**: Verify Dockerfile and build context
- **Deploy fails**: Check Azure credentials and App Service status
- **Health check fails**: App may need more startup time

#### Quick Fixes
```bash
# Test locally
docker build -t test-image -f GrandeTech.QueueHub.API/GrandeTech.QueueHub.API/Dockerfile .

# Check Azure resources
az webapp show --name app-p-queuehub-api-001 --resource-group rg-p-queuehub-core-001

# View App Service logs
az webapp log tail --name app-p-queuehub-api-001 --resource-group rg-p-queuehub-core-001
``` 