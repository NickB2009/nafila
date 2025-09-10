# 🚨 Backend Registration API Bug Report

**Date:** September 8, 2025  
**Priority:** HIGH - Blocking user registration  
**Component:** Authentication API - Registration Endpoint  

## 🔍 **Issue Summary**

The registration API endpoint is returning **500 Internal Server Error** when receiving valid phone number-based registration requests from the Flutter frontend.

## 📊 **Error Details**

### **Endpoint:** `POST https://api.eutonafila.com.br/api/Auth/register`

### **Request Format (Frontend is sending):**
```json
{
  "FullName": "João Silva",
  "Email": "joao@example.com",
  "PhoneNumber": "+5511999999999",
  "Password": "StrongPass123!"
}
```

### **Error Response:**
```json
{
  "error": "Internal Server Error",
  "message": "An unexpected error occurred",
  "details": ["Please try again later or contact support"],
  "timestamp": "2025-09-07T22:14:00.5668626Z",
  "traceId": "3ea0c30c-b94e-44da-86e6-f0239c9b3489"
}
```

### **HTTP Status:** 500 Internal Server Error

## 🧪 **Frontend Testing Results**

✅ **Frontend is working correctly:**
- All 18 unit tests pass for authentication models
- Request format matches backend specification exactly
- Phone number validation works properly
- Password strength validation implemented
- Error handling is comprehensive

❌ **Backend API is failing:**
- Returns 500 error for all registration attempts
- Same error occurs with different phone number formats
- Integration tests confirm frontend sends correct data

## 🔧 **Likely Root Causes**

Based on the migration from username to phone number authentication:

### 1. **Database Schema Issues**
- `User` table may be missing `FullName` column
- `User` table may be missing `PhoneNumber` column
- `Username` column constraints may still be required
- Phone number field may not allow international format

### 2. **Backend Model Binding**
```csharp
// Check if RegisterRequest model matches:
public class RegisterRequest 
{
    public string FullName { get; set; }      // ✅ Required
    public string Email { get; set; }         // ✅ Required  
    public string PhoneNumber { get; set; }   // ✅ Required
    public string Password { get; set; }      // ✅ Required
    
    // ❌ These should be REMOVED:
    // public string Username { get; set; }
    // public string ConfirmPassword { get; set; }
}
```

### 3. **Database Entity Configuration**
```csharp
// Check User entity:
public class User 
{
    public string FullName { get; set; }      // ✅ Required
    public string PhoneNumber { get; set; }   // ✅ Required (Unique)
    public string Email { get; set; }         // ✅ Required (Unique)
    
    // ❌ This should be REMOVED or made optional:
    // public string Username { get; set; }
}
```

### 4. **Phone Number Validation**
- Backend may not handle international format `+5511999999999`
- Consider supporting both `+5511999999999` and `11999999999` formats

## 📋 **Action Items for Backend Developer**

### **Immediate (High Priority):**

1. **Check Application Logs**
   - Look for detailed exception information using traceId: `3ea0c30c-b94e-44da-86e6-f0239c9b3489`
   - Check for database constraint violations
   - Look for model binding errors

2. **Verify Database Schema**
   ```sql
   -- Ensure these columns exist:
   ALTER TABLE Users ADD COLUMN FullName NVARCHAR(255) NOT NULL;
   ALTER TABLE Users ADD COLUMN PhoneNumber NVARCHAR(20) UNIQUE NOT NULL;
   
   -- Remove username requirement if present:
   ALTER TABLE Users ALTER COLUMN Username NVARCHAR(255) NULL;
   ```

3. **Update RegisterRequest Model**
   - Remove `Username` and `ConfirmPassword` fields
   - Add `FullName` and `PhoneNumber` fields
   - Update validation attributes

4. **Test Phone Number Formats**
   - Support international format: `+5511999999999`
   - Support national format: `11999999999`
   - Add proper phone number validation regex

### **Medium Priority:**

5. **Update User Entity**
   - Add `FullName` property
   - Add `PhoneNumber` property with unique constraint
   - Make `Username` optional or remove entirely

6. **Update Authentication Logic**
   - Login should use `PhoneNumber` instead of `Username`
   - Update JWT token claims to include phone number
   - Update profile endpoint to return new fields

### **Testing:**

7. **Manual API Testing**
   ```bash
   # Test with curl or Postman:
   curl -X POST https://api.eutonafila.com.br/api/Auth/register \
     -H "Content-Type: application/json" \
     -d '{
       "FullName": "Test User",
       "Email": "test@example.com", 
       "PhoneNumber": "+5511999999999",
       "Password": "StrongPass123!"
     }'
   ```

## 📞 **Phone Number Format Support**

The frontend will send phone numbers in international format by default:
- **Brazil:** `+5511999999999`
- **US:** `+15551234567`
- **Other countries:** `+[country_code][number]`

Backend should:
- Accept international format with `+` prefix
- Store in consistent format
- Validate country codes if needed
- Support at least Brazilian numbers initially

## 🎯 **Expected Resolution**

Once fixed, the registration flow should:
1. ✅ Accept phone number registration requests
2. ✅ Store user with FullName and PhoneNumber
3. ✅ Return success response
4. ✅ Allow login with phone number + password
5. ✅ Return user profile with new fields

## 📧 **Frontend Contact**

The frontend team has implemented:
- ✅ Complete phone number authentication UI
- ✅ International phone number support
- ✅ Enhanced error handling and logging
- ✅ Comprehensive test coverage

Frontend is ready and waiting for backend fixes.

---

**Report generated by:** Frontend Development Team  
**Next Update:** After backend deployment  
**Tracking:** Backend Registration API - Phone Number Authentication
