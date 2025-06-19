# GitHub Actions CI/CD Setup

This document describes the CI/CD pipeline setup for the EuToNaFila QueueHub API using GitHub Actions.

## Overview

The CI/CD pipeline automatically builds, tests, and deploys the application when code is merged to the `main` branch. The pipeline consists of three main jobs:

1. **Build and Test** - Compiles the application and runs unit tests
2. **Build and Push Image** - Creates a Docker image and pushes it to Azure Container Registry
3. **Deploy to Azure** - Updates the Azure App Service with the new container image

## Prerequisites

### 1. Azure Resources
Ensure the following Azure resources are already created:
- Azure Container Registry (ACR): `acrqueuehubapi001`
- App Service: `app-p-queuehub-api-001`
- Resource Group: `rg-p-queuehub-core-001`

### 2. GitHub Repository Secrets

You need to configure the following secrets in your GitHub repository:

#### Required Secrets

1. **AZURE_CREDENTIALS**
   - Service Principal credentials for Azure authentication
   - Create using: `az ad sp create-for-rbac --name "github-actions-queuehub" --role contributor --scopes /subscriptions/{subscription-id}/resourceGroups/rg-p-queuehub-core-001 --sdk-auth`
   - Output should be a JSON object with `clientId`, `clientSecret`, `subscriptionId`, and `tenantId`

2. **ACR_USERNAME**
   - Azure Container Registry username
   - Get using: `az acr credential show --name acrqueuehubapi001 --query username --output tsv`

3. **ACR_PASSWORD**
   - Azure Container Registry password
   - Get using: `az acr credential show --name acrqueuehubapi001 --query passwords[0].value --output tsv`

#### Setting Up Secrets

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add each secret with the exact names listed above

## Workflow Configuration

### Environment Protection

The deployment job uses the `production` environment for additional protection:

1. Go to **Settings** → **Environments**
2. Create a new environment called `production`
3. Optionally add protection rules:
   - Required reviewers
   - Wait timer
   - Deployment branches

### Workflow Triggers

The workflow triggers on:
- **Push to main branch** - Full CI/CD pipeline
- **Pull requests to main** - Build and test only (no deployment)

### Path Filters

The workflow only runs when changes are made to:
- `GrandeTech.QueueHub.API/**` - Application code
- `.github/workflows/core-api-ci-cd.yml` - Workflow itself

## Pipeline Stages

### 1. Build and Test Job

**Purpose**: Validate code quality and functionality

**Steps**:
- Checkout code
- Setup .NET 8.0
- Restore dependencies
- Build application
- Run unit tests
- Upload test results as artifacts

**Artifacts**: Test results are preserved for 30 days

### 2. Build and Push Image Job

**Purpose**: Create and store container image

**Steps**:
- Setup Docker Buildx
- Login to Azure Container Registry
- Generate image tags (date + commit hash)
- Build Docker image with caching
- Push to ACR with multiple tags

**Image Tags**:
- `latest` - Always points to the most recent build
- `YYYYMMDD-commit` - Specific version tag

### 3. Deploy to Azure Job

**Purpose**: Deploy to production environment

**Steps**:
- Azure authentication
- Update App Service container image
- Restart App Service
- Health check verification
- Deployment summary

## Health Checks

The pipeline includes health checks to verify successful deployment:

- **Endpoint**: `https://{app-service-name}.azurewebsites.net/health`
- **Expected Response**: JSON with status "Healthy"
- **Retry Logic**: Up to 10 attempts with 30-second intervals

## Monitoring and Troubleshooting

### Pipeline Logs

1. Go to **Actions** tab in GitHub
2. Click on the workflow run
3. Expand job steps to view detailed logs

### Common Issues

#### Build Failures
- Check .NET version compatibility
- Verify all dependencies are restored
- Review test failures

#### Docker Build Issues
- Verify Dockerfile syntax
- Check build context
- Ensure all required files are present

#### Deployment Failures
- Verify Azure credentials
- Check App Service status
- Review container image tags
- Validate health check endpoint

#### Health Check Failures
- App Service may need more time to start
- Check application logs in Azure Portal
- Verify environment variables
- Review application configuration

### Azure Portal Monitoring

1. **App Service Logs**
   - Go to Azure Portal → App Service → Log stream
   - Monitor application startup and errors

2. **Container Registry**
   - Check ACR for successful image pushes
   - Verify image tags and sizes

3. **Application Insights** (if configured)
   - Monitor application performance
   - Track errors and exceptions

## Security Considerations

### Service Principal Permissions
The service principal should have minimal required permissions:
- Contributor access to the specific resource group
- ACR Push permissions for the container registry

### Secret Rotation
- Regularly rotate ACR credentials
- Update service principal credentials as needed
- Monitor secret expiration dates

### Network Security
- Consider using private endpoints for ACR
- Implement network security groups
- Use Azure Key Vault for sensitive configuration

## Cost Optimization

### Build Optimization
- Docker layer caching reduces build times
- Multi-stage builds minimize image size
- Conditional job execution prevents unnecessary builds

### Azure Resources
- Use appropriate App Service plan tier
- Monitor ACR storage costs
- Implement resource tagging for cost tracking

## Customization

### Environment Variables
Modify the `env` section in the workflow to change:
- Project names
- Resource names
- Image tags
- Build configurations

### Deployment Strategy
Consider implementing:
- Blue-green deployments
- Canary releases
- Rollback mechanisms
- Staging environments

### Notifications
Add notifications for:
- Deployment success/failure
- Build status
- Security alerts

## Support

For issues with the CI/CD pipeline:
1. Check GitHub Actions logs
2. Review Azure resource status
3. Verify secret configuration
4. Test locally with Docker
5. Contact the development team 