# ✅ Setup Verification Guide

This guide helps verify that your QueueHub setup is working perfectly with database, migrations, and seeding.

## 🔍 Step-by-Step Verification

### 1. Quick Health Check
```bash
# Test API is running
curl http://localhost:5098/api/health
# Expected: "Healthy"
```

### 2. Verify Database Connection
```bash
# Check organizations endpoint (should return 2 fake organizations)
curl http://localhost:5098/api/organizations
```

**Expected Response:**
```json
[
  {
    "id": "...",
    "name": "Downtown Barbers",
    "slug": "downtown-barbers",
    "contactInfo": {
      "phone": "(555) 123-4567",
      "email": "info@downtownbarbers.com",
      "address": "123 Main Street, Downtown, City 12345"
    },
    "branding": {
      "primaryColor": "#8B4513",
      "logoUrl": "https://downtownbarbers.com/logo.png"
    }
  },
  {
    "id": "...",
    "name": "Healthy Life Clinic",
    "slug": "healthy-life-clinic",
    "contactInfo": {
      "phone": "(555) 987-6543",
      "email": "contact@healthylifeclinic.com",
      "address": "456 Health Ave, Medical District, City 67890"
    },
    "branding": {
      "primaryColor": "#32CD32",
      "logoUrl": "https://healthylifeclinic.com/logo.png"
    }
  }
]
```

### 3. Verify User Authentication
```bash
# Test login with seeded customer
curl -X POST http://localhost:5098/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"customer_test","password":"CustomerPass123!"}'
```

**Expected:** JWT token response

### 4. Check Database Tables
If you have SQL Server Management Studio or Azure Data Studio:

**Connection Details:**
- Server: `localhost,1433`
- Database: `QueueHubDb`
- Username: `sa`
- Password: `DevPassword123!`

**Verify Tables Exist:**
- Users
- Organizations
- Locations
- Queues
- QueueEntries
- SubscriptionPlans
- StaffMembers
- ServicesOffered

**Sample Data Check:**
```sql
-- Should return 4 users
SELECT COUNT(*) FROM Users;

-- Should return 2 organizations
SELECT * FROM Organizations;

-- Should return 2 locations
SELECT * FROM Locations;

-- Should return 2 subscription plans
SELECT * FROM SubscriptionPlans;
```

### 5. Swagger Documentation
Open browser and go to: `https://localhost:7126/swagger`

**Verify:**
- ✅ Swagger UI loads
- ✅ All endpoints are documented
- ✅ Authorization section shows JWT Bearer
- ✅ You can test endpoints directly

## 🎉 Success Indicators

If you see all of the following, your setup is **PERFECT**:

✅ **API Health Check**: Returns "Healthy"  
✅ **Organizations Endpoint**: Returns 2 organizations with complete data  
✅ **Authentication**: Login works with test credentials  
✅ **Database**: All tables created with sample data  
✅ **Swagger**: Documentation loads and is interactive  

## 🚨 Common Issues & Solutions

### No Data Returned
**Problem**: API returns empty arrays `[]`
**Solution**: 
1. Check `appsettings.json` has `"SeedData": true`
2. Check application logs for seeding errors
3. Delete database and restart API (it will recreate)

### Authentication Fails
**Problem**: Login returns 401 or errors
**Solution**:
1. Verify JWT settings in `appsettings.json`
2. Check that users were seeded correctly
3. Try with different test credentials

### Database Connection Errors
**Problem**: Can't connect to database
**Solution**:
1. Verify SQL Server container is running: `docker ps`
2. Check connection string in `appsettings.json`
3. Wait longer for SQL Server to start (can take 30+ seconds)

### Swagger Not Loading
**Problem**: 404 or blank page at swagger URL
**Solution**:
1. Ensure you're using HTTPS: `https://localhost:7126/swagger`
2. Check if API is running on different port
3. Try HTTP version: `http://localhost:5098/swagger`

## 🎯 Test Scenarios

### Full Workflow Test
```bash
# 1. Login as customer
TOKEN=$(curl -s -X POST http://localhost:5098/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"customer_test","password":"CustomerPass123!"}' \
  | jq -r '.token')

# 2. Get locations (should show 2)
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5098/api/locations

# 3. Join a queue (replace {locationId} with actual ID)
curl -X POST -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  http://localhost:5098/api/queues/join \
  -d '{"locationId":"LOCATION-ID-HERE","serviceId":"SERVICE-ID-HERE"}'
```

## 📊 Performance Check

Your local setup should have:
- **API Response Time**: < 200ms for most endpoints
- **Database Query Time**: < 50ms for simple queries
- **Startup Time**: < 30 seconds from `dotnet run`

---

**If everything above works, congratulations! 🎉 Your QueueHub development environment is ready for amazing things!** 