# 🚀 CI/CD Quick Start Guide

## What's Been Set Up

I've created a complete GitHub Actions CI/CD pipeline for your EuToNaFila QueueHub API that will automatically deploy when you merge to `main`.

### Files Created:
- ✅ `.github/workflows/core-api-ci-cd.yml` - Main CI/CD workflow
- ✅ `GrandeTech.QueueHub.API/GrandeTech.QueueHub.API/Controllers/HealthController.cs` - Health check endpoint
- ✅ `.github/README.md` - Detailed setup documentation
- ✅ `setup-github-actions.ps1` - Automated setup script

## 🎯 Next Steps (Required)

### 1. Set Up GitHub Secrets
Run the setup script to get your Azure credentials:

```powershell
.\setup-github-actions.ps1 -SubscriptionId "160af2e4-319f-4384-b38f-71e23d140a0f"
```

### 2. Add Secrets to GitHub
Go to your GitHub repository → **Settings** → **Secrets and variables** → **Actions** and add:

- `AZURE_CREDENTIALS` - JSON from the setup script
- `ACR_USERNAME` - ACR username from the setup script  
- `ACR_PASSWORD` - ACR password from the setup script

### 3. Create Production Environment (Optional)
Go to **Settings** → **Environments** → **New environment**:
- Name: `production`
- Add protection rules if needed

## 🔄 How It Works

### Triggers:
- **Push to `main`** → Full build, test, and deploy
- **Pull Request to `main`** → Build and test only

### Pipeline Stages:
1. **Build & Test** - Compiles code and runs tests
2. **Build & Push Image** - Creates Docker image and pushes to ACR
3. **Deploy to Azure** - Updates App Service with new image

### Health Checks:
- Automatically verifies deployment at `/health` endpoint
- Retries up to 10 times if needed

## 🎉 Test Your Setup

1. **Commit and push** the new files to your repository
2. **Merge to `main`** branch
3. **Check GitHub Actions** tab to see the pipeline running
4. **Monitor deployment** in Azure Portal

## 📊 Monitoring

- **GitHub Actions**: View pipeline status and logs
- **Azure Portal**: Monitor App Service and ACR
- **Health Endpoint**: `https://app-p-queuehub-api-001.azurewebsites.net/health`

## 🛠️ Troubleshooting

### Common Issues:
- **Build fails**: Check .NET version and dependencies
- **Docker fails**: Verify Dockerfile and build context
- **Deploy fails**: Check Azure credentials and App Service status
- **Health check fails**: App may need more startup time

### Quick Fixes:
```bash
# Test locally
docker build -t test-image -f GrandeTech.QueueHub.API/GrandeTech.QueueHub.API/Dockerfile .

# Check Azure resources
az webapp show --name app-p-queuehub-api-001 --resource-group rg-p-queuehub-core-001

# View App Service logs
az webapp log tail --name app-p-queuehub-api-001 --resource-group rg-p-queuehub-core-001
```

## 🔐 Security Notes

- Service principal has minimal required permissions
- ACR credentials are used only for image pushing
- All secrets are encrypted in GitHub
- Consider rotating credentials regularly

## 📈 What Happens on Merge to Main

1. **Automatically triggers** the CI/CD pipeline
2. **Builds** your .NET application
3. **Runs** all unit tests
4. **Creates** a Docker image with versioned tags
5. **Pushes** to Azure Container Registry
6. **Deploys** to your Azure App Service
7. **Verifies** deployment with health checks
8. **Provides** deployment summary

## 🎯 Success Indicators

✅ Pipeline runs without errors  
✅ All tests pass  
✅ Docker image is created and pushed  
✅ App Service is updated  
✅ Health check returns "Healthy"  
✅ Application is accessible at your URL  

---

**Need Help?** Check the detailed documentation in `.github/README.md` or review the pipeline logs in GitHub Actions. 