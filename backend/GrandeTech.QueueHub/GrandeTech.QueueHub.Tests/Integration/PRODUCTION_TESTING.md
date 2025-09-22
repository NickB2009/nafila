# Production Integration Testing

This document explains how to run integration tests against the production API.

## Overview

The production integration tests are designed to test actual API endpoints against the live production environment. These tests are separate from the local integration tests and use different test categories.

## Test Categories

- `Production` - Tests that run against production API endpoints
- `Public` - Tests for public endpoints (no authentication required)
- `Auth` - Tests for authentication endpoints

## Running Production Tests

### Basic Production Test Run

```powershell
.\run-integration-tests.ps1 -Environment Prod
```

This will:
- Set `TEST_ENVIRONMENT=Prod`
- Set `PROD_API_URL=https://api.eutonafila.com.br`
- Run tests with `TestCategory=Production` filter
- Check production API health before running tests

### Custom Production URL

```powershell
.\run-integration-tests.ps1 -Environment Prod -ProdUrl "https://your-custom-api.com"
```

### Specific Test Categories

```powershell
# Run only public endpoint tests
.\run-integration-tests.ps1 -Environment Prod -TestFilter "TestCategory=Production&TestCategory=Public"

# Run only auth endpoint tests  
.\run-integration-tests.ps1 -Environment Prod -TestFilter "TestCategory=Production&TestCategory=Auth"
```

### BoaHost Environment

```powershell
.\run-integration-tests.ps1 -Environment BoaHost -ProdUrl "https://your-boahost-url.com"
```

## Test Behavior

### Expected Results

Production tests are designed to handle various scenarios:

1. **Successful Responses** - When endpoints work correctly
2. **Business Logic Errors** - When requests fail due to invalid data (404, 400, etc.)
3. **Server Errors** - When there are actual server issues (500, etc.)

### Test Philosophy

- Tests should **NOT fail** on business logic errors (like invalid data)
- Tests **SHOULD fail** on server errors (500, connection issues)
- Tests provide detailed logging for debugging

### Authentication

Currently, production tests run without authentication to test public endpoints. Future versions may include authenticated test scenarios.

## Test Classes

### ProductionIntegrationTestBase

Base class providing:
- Environment detection (`TEST_ENVIRONMENT`, `PROD_API_URL`)
- HTTP client configuration
- Common helper methods
- Response deserialization
- Health check verification

### ProductionPublicControllerTests

Tests for public endpoints:
- Health check
- Get locations
- Get services  
- Anonymous queue join
- Queue status
- Location info

### ProductionAuthControllerTests

Tests for authentication endpoints:
- Login with invalid credentials
- Registration with invalid data
- Token refresh with invalid token
- Token validation
- Logout without token
- Password reset requests

## Environment Variables

The tests use these environment variables:

- `TEST_ENVIRONMENT` - Set to "Prod", "BoaHost", or "Local"
- `PROD_API_URL` - The base URL for the production API

## Troubleshooting

### Common Issues

1. **Connection Timeout** - Check if production API is accessible
2. **SSL Certificate Errors** - Ensure production has valid SSL certificates
3. **Rate Limiting** - Production may have rate limits that could affect tests

### Debugging

All tests include detailed console output showing:
- Request/response status codes
- Response content
- Error messages

Check the test output for specific error details.

## Best Practices

1. **Run during low-traffic periods** to avoid impacting production
2. **Monitor production logs** during test runs
3. **Use appropriate test data** that won't interfere with real users
4. **Don't run tests against production during peak hours**

## Future Enhancements

- Authenticated test scenarios
- Performance testing capabilities
- Load testing integration
- Automated test scheduling
